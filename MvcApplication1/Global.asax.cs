using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using FCG.RegoCms;
using Microsoft.Practices.Unity;
using MvcApplication1.Models;
using MvcApplication1.Models.Domain;
using Unity.Mvc4;

namespace MvcApplication1
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static UnityContainer Container = new UnityContainer();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DependencyResolver.SetResolver(new UnityDependencyResolver(Container));
            Container.RegisterType<CmsFrontendService>(new SingletonLifetimeManager());

            var cmsBackendService = Container.Resolve<CmsService>();
            var cmsFrontendService = Container.Resolve<CmsFrontendService>();
            var page = cmsBackendService.CreatePage(
                name: "test page markup", 
                route: "http://localhost:33586/en-gb/testPage", 
                markup: "test page markup goes here"
            );
            page.Publish();
            cmsFrontendService.UpdateContentFiles();
        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            var authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null || authCookie.Value == "")
                return;

            FormsAuthenticationTicket authTicket;
            try
            {
                authTicket = FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch
            {
                return;
            }

            // retrieve roles from UserData
            var roles = authTicket.UserData.Split(';');

            if (Context.User != null)
                Context.User = new GenericPrincipal(Context.User.Identity, roles);
        }
    }
}