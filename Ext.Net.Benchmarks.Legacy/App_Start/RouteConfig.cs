using System.Web.Mvc;
using System.Web.Routing;

namespace Ext.Net.Benchmarks.Legacy
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{extnet-root}/{extnet-file}/ext.axd");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Benchmark", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
