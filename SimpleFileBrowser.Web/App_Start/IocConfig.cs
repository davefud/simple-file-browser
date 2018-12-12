using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using SFBWeb.Models;
using SFBWeb.Service;

namespace SFBWeb.App_Start
{
    public class IocConfig
    {
        public static void RegisterIoc(HttpConfiguration config)
        {
            var kernel = new StandardKernel();

            kernel.Bind<ISimpleFile>().To<SimpleFile>();
            kernel.Bind<IFileService>().To<FileService>();

            // Tell WebApi how to use Ninject IoC
            config.DependencyResolver = new NinjectDependencyResolver(kernel);
        }
    }
}