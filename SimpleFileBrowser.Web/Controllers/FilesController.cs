using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using SFBWeb.Models;
using SFBWeb.Service;

namespace SFBWeb.Controllers
{
    [RoutePrefix("api/files")]
    public class FilesController : ApiController
    {
        private IFileService fileService { get; set; }

        public FilesController(IFileService fileService)
        {
            this.fileService = fileService;
        }

        [HttpGet]
        [Route("{*dirs?}")]
        public IEnumerable<ISimpleFile> Get(string dirs = "", string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return this.fileService.GetFiles(dirs, searchPattern, searchOption);
        }

        [HttpDelete]
        [Route("{path}")]
        public IHttpActionResult Delete(string path, bool isDirectory)
        {
            var success = this.fileService.Delete(path, isDirectory);

            if (success)
            {
                return Ok();
            }

            if (isDirectory)
            {
                return Content(HttpStatusCode.Forbidden, "Directory Could not be Deleted, make sure the directory is empty!");
            }

            return Content(HttpStatusCode.Forbidden, "File Could not be Deleted!");

        }

        [HttpPost]
        [Route("copy")]
        public IEnumerable<ISimpleFile> Copy([FromUri] string path, [FromUri] bool isDirectory)
        {
            return this.fileService.Copy(path, isDirectory);
        }

        [HttpPost]
        [Route("move")]
        public IEnumerable<ISimpleFile> Move([FromUri] string fromPath, [FromUri] string toPath, [FromUri] bool isDirectory)
        {
            return this.fileService.Move(fromPath, toPath, isDirectory);
        }

    }
}
