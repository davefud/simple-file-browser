using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SFBWeb.Models
{
    public class SimpleFile : ISimpleFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsDirectory { get; set; }
        public string LastModifiedTime { get; set; }
        public string Size { get; set; }
        public string Extension { get; set; }
        public string DownloadUrl { get; set; }
    }
}