using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using SFBWeb.Models;
using SFBWeb.Helpers;
using System.Web.Http;

namespace SFBWeb.Service
{
    public class FileService : IFileService
    {
        public IEnumerable<ISimpleFile> GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var fullPath = GetPath(path);

            return WalkDirectoryTree(fullPath, HttpContext.Current.Request, searchPattern, searchOption);
        }

        public Task<HttpResponseMessage> UploadFile(HttpRequestMessage request, string path)
        {
            string rootPath = GetPath(path);

            var provider = new MultipartFileStreamProvider(rootPath);

            var newFullFilePath = "";

            var task = request.Content.ReadAsMultipartAsync(provider).ContinueWith<HttpResponseMessage>(t =>
            {
                List<string> savedFilePath = new List<string>();

                if (t.IsCanceled || t.IsFaulted)
                {
                    request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                }

                foreach (MultipartFileData item in provider.FileData)
                {
                    try
                    {
                        string name = item.Headers.ContentDisposition.FileName.Replace("\"", "");

                        // TODO: Don't allow executable extensions to be uploaded or use an ext whitelist?
                        string newFileName = Guid.NewGuid() + Path.GetExtension(name);

                        newFullFilePath = Path.Combine(rootPath, newFileName);

                        File.Move(item.LocalFileName, newFullFilePath);

                    }
                    catch (Exception ex)
                    {
                        string message = ex.Message;
                    }
                }

                FileInfo fi = new FileInfo(newFullFilePath);

                string relativePath = GetRelativeUploadPath(newFullFilePath);

                var downloadUrl = GetDownloadUrl(request.RequestUri, relativePath);

                var newFile = new[] {
                        new SimpleFile
                        {
                            Name = fi.Name,
                            Path = relativePath,
                            IsDirectory = false,
                            Extension = fi.Extension,
                            LastModifiedTime = fi.LastWriteTimeUtc.ToString("MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture),
                            Size = FileHelper.FormatByteSize(fi.Length, 0),
                            DownloadUrl = downloadUrl
                        }
                    };

                return request.CreateResponse(HttpStatusCode.Created, newFile);
            });

            return task;
        }

         public bool Delete(string path, bool isDirectory)
        {
            var fullPath = GetPath(path);

            try
            {
                if (isDirectory)
                {
                    Directory.Delete(fullPath);
                }
                else
                {
                    File.Delete(fullPath);
                }

                return true;

            }
            catch (Exception ex) when (ex is IOException 
                || ex is DirectoryNotFoundException 
                || ex is ArgumentException 
                || ex is ArgumentNullException
                || ex is PathTooLongException
                || ex is UnauthorizedAccessException)
            {
                return false;
            }
        }

        public IEnumerable<ISimpleFile> Move(string fromPath, string toPath, bool isDirectory)
        {
            var simpleFiles = new List<ISimpleFile>();

            try
            {
                string sourceFullPath = GetPath(fromPath);
                FileInfo sourceFile = new FileInfo(sourceFullPath);

                string destFullPath = "";

                if (toPath.StartsWith("/"))
                {
                    toPath = toPath.Remove(0, 1);
                    destFullPath = GetPath(toPath, sourceFile.Name);

                } else
                {
                    destFullPath = GetPath(Path.Combine(sourceFile.DirectoryName, toPath, sourceFile.Name));
                }
                
                FileInfo destFile = new FileInfo(destFullPath);

                string destRelativePath = GetRelativeUploadPath(destFullPath);

                var downloadUrl = GetDownloadUrl(HttpContext.Current.Request.Url, destRelativePath);

                File.Move(sourceFullPath, destFullPath);

                simpleFiles.Add(new SimpleFile
                {
                    Name = destFile.Name,
                    Path = destRelativePath,
                    IsDirectory = false,
                    Extension = destFile.Extension,
                    LastModifiedTime = destFile.LastWriteTimeUtc.ToString("MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture),
                    Size = FileHelper.FormatByteSize(destFile.Length, 0),
                    DownloadUrl = downloadUrl
                });

                return simpleFiles;
            }
            catch (FileNotFoundException) 
            {
                return simpleFiles;
            }
        }

        public IEnumerable<ISimpleFile> Copy(string path, bool isDirectory)
        {

            var simpleFiles = new List<ISimpleFile>();

            try
            {
                string originalFullPath = GetPath(path);

                FileInfo originalFile = new FileInfo(originalFullPath);

                string copiedFileName = Guid.NewGuid() + originalFile.Extension;

                string copiedFullPath = Path.Combine(originalFile.DirectoryName, copiedFileName);

                FileInfo CopiedFile = new FileInfo(copiedFullPath);

                string copiedRelativePath = GetRelativeUploadPath(copiedFullPath);

                var downloadUrl = GetDownloadUrl(HttpContext.Current.Request.Url, copiedRelativePath);

                File.Copy(originalFullPath, copiedFullPath);

                simpleFiles.Add(new SimpleFile
                {
                    Name = CopiedFile.Name,
                    Path = copiedRelativePath,
                    IsDirectory = false,
                    Extension = CopiedFile.Extension,
                    LastModifiedTime = CopiedFile.LastWriteTimeUtc.ToString("MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture),
                    Size = FileHelper.FormatByteSize(CopiedFile.Length, 0),
                    DownloadUrl = downloadUrl
                });

                return simpleFiles;
            }
            catch (FileNotFoundException)
            {
                return simpleFiles;
            }
        }

        private List<ISimpleFile> WalkDirectoryTree(string path, HttpRequest currentRequest, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var simpleFiles = new List<ISimpleFile>();
            Stack<string> dirs = new Stack<string>(25);

            if (!Directory.Exists(path))
            {
                return simpleFiles;
            }

            dirs.Push(path);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                string[] subDirs;

                try
                {
                    subDirs = Directory.GetDirectories(currentDir);
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException 
                    || ex is DirectoryNotFoundException 
                    || ex is ArgumentException 
                    || ex is ArgumentOutOfRangeException)
                {
                    continue;
                }
                
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(currentDir);
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException 
                    || ex is DirectoryNotFoundException 
                    || ex is ArgumentException 
                    || ex is ArgumentOutOfRangeException)
                {
                    continue;
                }

                foreach (var subDir in subDirs)
                {
                    try
                    {
                        var di = new DirectoryInfo(subDir);

                        string relativePath = GetRelativeUploadPath(subDir);

                        simpleFiles.Add(new SimpleFile
                        {
                            Name = di.Name,
                            Path = relativePath,
                            Extension = di.Extension,
                            IsDirectory = true,
                            LastModifiedTime = di.LastWriteTimeUtc.ToString("MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture),
                            Size = "",
                            DownloadUrl = ""
                        });
                    }
                    catch (FileNotFoundException)
                    {
                        continue;
                    }
                }

                foreach (string file in files)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(file);

                        string relativePath = GetRelativeUploadPath(file);

                        var downloadUrl = GetDownloadUrl(currentRequest.Url, relativePath);

                        simpleFiles.Add(new SimpleFile
                        {
                            Name = fi.Name,
                            Path = relativePath,
                            IsDirectory = false,
                            Extension = fi.Extension,
                            LastModifiedTime = fi.LastWriteTimeUtc.ToString("MM/dd/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture),
                            Size = FileHelper.FormatByteSize(fi.Length, 0),
                            DownloadUrl = downloadUrl
                        });
                    }
                    catch (FileNotFoundException)
                    {
                        continue;
                    }
                }

                if (searchOption == SearchOption.AllDirectories)
                {
                    foreach (string str in subDirs)
                    {
                        dirs.Push(str);
                    }
                }
                    
            }

            return simpleFiles;
        }

        private string GetRelativeUploadPath(string path)
        {
            string filePath = path.Replace("\\", "/");

            string uploadDirectory = AppSettingsHelper.RelativeUploadDirectory.Replace("~", "");

            int idx = filePath.IndexOf(uploadDirectory);

            if (idx == -1)
            {
                return "";
            }

            int startIdx = idx + uploadDirectory.Length;

            if (startIdx >= filePath.Length)
            {
                return "";
            }

            return filePath.Substring(startIdx);
        }

        private string GetPath(string path, string filename = "")
        {
            var reequestPath = path != null ? path : "";

            var basePath = HttpContext.Current.Server.MapPath(AppSettingsHelper.RelativeUploadDirectory);

            var fullPath = Path.GetFullPath(Path.Combine(basePath, reequestPath, filename));

            if (fullPath.StartsWith(basePath))
            {
                return fullPath;
            }

            return null;
        }


        private string GetDownloadUrl(Uri requestUri, string path)
        {
            var requestPath = path != null ? path : "";

            Uri baseUri = new Uri(requestUri, VirtualPathUtility.ToAbsolute(AppSettingsHelper.RelativeUploadDirectory));

            string downloadUrl = new Uri(baseUri, requestPath).AbsoluteUri;

            if (downloadUrl.StartsWith(baseUri.AbsoluteUri))
            {
                return downloadUrl;
            }

            return null;
        }
    }
}