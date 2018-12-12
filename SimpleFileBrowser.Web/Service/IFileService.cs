using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using SFBWeb.Models;

namespace SFBWeb.Service
{
    public interface IFileService
    {
        IEnumerable<ISimpleFile> GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);
        Task<HttpResponseMessage> UploadFile(HttpRequestMessage msg, string path);
        bool Delete(string path, bool isDirectory);
        IEnumerable<ISimpleFile> Move(string fromPath, string toPath, bool isDirectory);
        IEnumerable<ISimpleFile> Copy(string path, bool isDirectory);
    }
}
