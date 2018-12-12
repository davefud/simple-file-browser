using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFBWeb.Models
{
    public interface ISimpleFile
    {
        string Name { get; set; }
        string Path { get; set; }
        bool IsDirectory { get; set; }
        string LastModifiedTime { get; set; }
        string Size { get; set; }
        string Extension { get; set; }
        string DownloadUrl { get; set; }
    }
}
