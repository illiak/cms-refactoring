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
        private const string _adminFormsCookieName = "CmsAdminFormsAuth";
        private const string _showDraftsCookieName = "CmsShowDrafts";

        public CmsController(CmsEngine cmsEngine)
        {
            _cmsEngine = cmsEngine;
        }

        [HttpGet]
        public ActionResult ProcessRequest()
        {
            var showDrafts = CheckIfHasToShowDrafts();
            if (showDrafts)
            {
                var isAdmin = CheckIfUserIsAdmin();
                if (!isAdmin) return new HttpUnauthorizedResult("Only admins can view drafts");
            }
            
            var response = _cmsEngine.ProcessRequest(Request.Url, showDrafts, new MvcRequestContext(ControllerContext, ViewData, TempData));
            switch (response.Type)
            {
                case ResponseType.OK:
                    return Content(response.Body);
                case ResponseType.Redirect:
                    throw new NotImplementedException();
                case ResponseType.PageNotFound:
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                case ResponseType.Error:
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [HttpPost]
        public ActionResult PreviewDraft(string pageUrl)
        {
            var isAdmin = CheckIfUserIsAdmin();
            if(!isAdmin) throw new ApplicationException("Unable to preview draft: user is not an admin");

            var cookie = new HttpCookie(_showDraftsCookieName, true.ToString()) { Secure = false };
            Response.Cookies.Add(cookie);

            return Redirect(pageUrl);
        }

        private bool CheckIfUserIsAdmin()
        {
            var cookie = Request.Cookies[_adminFormsCookieName];
            if(cookie == null) return false;

            FormsAuthenticationTicket ticket;
            try
            {
                ticket = FormsAuthentication.Decrypt(cookie.Value);
            }
            catch (ArgumentException)
            {
                ticket = null;
            }
            return ticket != null;
        }

        private bool CheckIfHasToShowDrafts()
        {
            var showDraftsCookie = HttpContext.Request.Cookies[_showDraftsCookieName];
            if (showDraftsCookie == null) return false;
            return bool.Parse(showDraftsCookie.Value);
        }

#if DEBUG
        public string SimulateAdminLogin()
        {
            var ticket = new FormsAuthenticationTicket(1 /*version*/, "admin", DateTime.UtcNow /*issue date*/,
                                                           DateTime.UtcNow.AddMinutes(FormsAuthentication.Timeout.TotalMinutes), true, string.Empty);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(_adminFormsCookieName, encryptedTicket) { Secure = false };
            Response.Cookies.Add(cookie);
            Response.Cookies.Add(new HttpCookie(_showDraftsCookieName, true.ToString()));

            return "Admin login simulated";
        }
#endif
    }

}
