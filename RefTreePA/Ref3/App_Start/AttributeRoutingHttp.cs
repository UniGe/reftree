using System.Web.Http;
using AttributeRouting.Web.Http.WebHost;
using System.Web.Http.WebHost;
using System.Web.SessionState;
using System.Web.Routing;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Ref3.AttributeRoutingHttp), "Start")]

namespace Ref3 
{
    public static class AttributeRoutingHttp
    {
        public class MyHttpControllerHandler : HttpControllerHandler, IRequiresSessionState
        {
            public MyHttpControllerHandler(RouteData routeData)
                : base(routeData)
            {
            }
        }
        public class MyHttpControllerRouteHandler : HttpControllerRouteHandler
        {
            protected override System.Web.IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                return new MyHttpControllerHandler(requestContext.RouteData);
            }
        }
		public static void RegisterRoutes(HttpRouteCollection routes) 
		{    
			// See http://github.com/mccalltd/AttributeRouting/wiki for more options.
			// To debug routes locally using the built in ASP.NET development server, go to /routes.axd
            
            routes.MapHttpAttributeRoutes(config => { config.RouteHandlerFactory = () => new MyHttpControllerRouteHandler(); });
		}

        public static void Start() 
		{
            RegisterRoutes(GlobalConfiguration.Configuration.Routes);
        }
    }
}
