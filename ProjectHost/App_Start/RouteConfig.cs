using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ProjectHost
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Google API Sign-in",
                url: "signin-google",
                defaults: new { controller = "Account", action = "ExternalLoginCallbackRedirect" }
            );

            routes.MapRoute(
                name: "ProjectById",
                url: "p/{id}",
                defaults: new { controller = "Projects", action = "Index" }
            );

            routes.MapRoute(
                name: "LatestRelease",
                url: "latest/{id}",
                defaults: new { controller = "Projects", action = "LatestRelease" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Projects", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
