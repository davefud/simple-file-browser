using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Net.Http.Extensions.Compression.Core.Compressors;
using Microsoft.AspNet.WebApi.Extensions.Compression.Server;

namespace SFBWeb
{
    public class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // TODO: Add any additional configuration code.

            var formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;

            formatter.SerializerSettings.ContractResolver = 
                new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // Web API routes
            config.MapHttpAttributeRoutes();

            // Add Compression Handlers
            config.MessageHandlers.Insert(0, new ServerCompressionHandler(
                new GZipCompressor(),
                new DeflateCompressor())
            );

        }
    }
}