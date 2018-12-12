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
using System.Web.Http;
using SFBWeb.Models;
using SFBWeb.Service;
using SFBWeb.Helpers;

namespace SFBWeb.Controllers
{
    [RoutePrefix("api/Upload")]
    public class UploadController : ApiController
    {
        private IFileService fileService { get; set; }

        public UploadController(IFileService fileService)
        {
            this.fileService = fileService;
        }

        [HttpPost]
        [Route("{*path?}")]
        public async Task<HttpResponseMessage> Post([FromUri] string path)
        {
            
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            return await fileService.UploadFile(Request, path);

        }
    }
}
