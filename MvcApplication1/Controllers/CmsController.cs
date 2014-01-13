using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using FCG.RegoCms;
using MvcApplication1.Models;

namespace MvcApplication1.Controllers
{
    public class CmsController : Controller
    {
        private readonly CmsFrontendService     _cmsFrontendService;
        private readonly MvcApplicationContext  _mvcApplicationContext;
        private readonly MvcRequestContextFactory _mvcRequestContextFactory;

        public CmsController(CmsFrontendService cmsFrontendService, MvcApplicationContext mvcApplicationContext, MvcRequestContextFactory mvcRequestContextFactory)
        {
            _cmsFrontendService = cmsFrontendService;
            _mvcApplicationContext = mvcApplicationContext;
            _mvcRequestContextFactory = mvcRequestContextFactory;
        }

        [HttpGet]
        public ActionResult ProcessRequest()
        {
            _mvcApplicationContext.SaveCurrentRequestContextData(ControllerContext, ViewData, TempData);

            var response = _cmsFrontendService.ProcessRequest(Request.Url);

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

        [HttpPost]
        public ActionResult UpdateContentFiles()
        {
            _cmsFrontendService.UpdateContentFiles();
            return new HttpStatusCodeResult(HttpStatusCode.OK); 
        }
    }
}
