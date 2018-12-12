using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SFBWeb.App_Start;

namespace SFBWeb
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            IocConfig.RegisterIoc(GlobalConfiguration.Configuration);
            
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            GlobalConfiguration.Configure(WebApiConfig.Register);

            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
