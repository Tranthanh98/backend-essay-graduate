using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;

namespace RollCallSystem
{
    public static class WebApiConfig
    {
        internal class JsonContentNegotiator : IContentNegotiator
        {
            private readonly JsonMediaTypeFormatter _jsonFormatter;

            public JsonContentNegotiator(JsonMediaTypeFormatter formatter)
            {
                _jsonFormatter = formatter;
                _jsonFormatter.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            }

            public ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            {
                return new ContentNegotiationResult(_jsonFormatter, new MediaTypeHeaderValue("application/json"));
            }
        }
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { }
            );
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            var jsonFormatter = new JsonMediaTypeFormatter();
            config.Services.Replace(typeof(IContentNegotiator), new JsonContentNegotiator(jsonFormatter));
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling
                  = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        }
    }
}
