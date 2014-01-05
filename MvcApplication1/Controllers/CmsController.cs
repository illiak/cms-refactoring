using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers
{
    public class CmsController : Controller
    {
        private readonly CmsEngine _cmsEngine;
        private readonly MvcApplicationContext _mvcApplicationContext;

        public CmsController(CmsEngine cmsEngine, MvcApplicationContext mvcApplicationContext)
        {
            _cmsEngine = cmsEngine;
            _mvcApplicationContext = mvcApplicationContext;
        }

        [HttpGet]
        public ActionResult ProcessRequest()
        {
            _mvcApplicationContext.SaveCurrentRequestContext(new MvcRequestContext(ControllerContext, ViewData, TempData));

            var response = _cmsEngine.ProcessRequest(Request.Url);
            switch (response.Type)
            {
                case ResponseType.OK:
                    return Content(response.Body);
                case ResponseType.Redirect:
                    throw new NotImplementedException("Redirects are not implemented yet");
                case ResponseType.PageNotFound:
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                case ResponseType.Error:
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                case ResponseType.Unauthorized:
                    return new HttpUnauthorizedResult(response.Description);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

#if DEBUG
        [HttpPost]
        public string SimulateAdminLogin()
        {
            var ticket = new FormsAuthenticationTicket(1 /*version*/, "admin", DateTime.UtcNow /*issue date*/,
                                                           DateTime.UtcNow.AddMinutes(FormsAuthentication.Timeout.TotalMinutes), true, string.Empty);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(CmsEngine.AdminFormsCookieName, encryptedTicket) { Secure = false };
            Response.Cookies.Add(cookie);

            return "Admin login simulated";
        }

        [HttpPost]
        public string SimulateShowDraftsMode()
        {
            Response.Cookies.Add(new HttpCookie(CmsEngine.ShowDraftsCookieName, true.ToString()));

            return "Show drafts mode simulated";
        }
#endif
    }
}
