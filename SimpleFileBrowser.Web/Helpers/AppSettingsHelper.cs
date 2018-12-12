using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SFBWeb.Helpers
{
    public static class AppSettingsHelper
    {
        public static string RelativeUploadDirectory
        {
            get
            {
                return VirtualPathUtility.AppendTrailingSlash(ConfigurationManager.AppSettings["UploadDirectory"]);
            }
        }
    }
}