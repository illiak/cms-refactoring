using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MvcApplication1
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

//            routes.MapRoute(
//                name: "Default",
//                url: "{controller}/{action}/{id}",
//                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
//            );

            routes.MapRoute(name: "update content files", url: "updateContentFiles", defaults: new { controller = "Cms", action = "UpdateContentFiles" });

            routes.MapRoute(name: "simulate admin login", url: "simulateAdminLogin", defaults: new { controller = "Cms", action = "SimulateAdminLogin"});
            routes.MapRoute(name: "simulate show drafts mode", url: "simulateShowDraftsMode", defaults: new { controller = "Cms", action = "SimulateShowDraftsMode" });

            routes.MapRoute(name: "ProcessRequest",
                             url: "{*url}",
                        defaults: new { controller = "Cms", action = "ProcessRequest" });
        }
    }
}