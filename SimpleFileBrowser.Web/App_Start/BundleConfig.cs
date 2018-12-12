using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace SFBWeb.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/main.js",
                        "~/Scripts/jquery-{version}.js"));


            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/styles.css"));
        }
    }
}