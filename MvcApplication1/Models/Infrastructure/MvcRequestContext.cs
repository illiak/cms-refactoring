using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using FCG.RegoCms;
using Microsoft.Ajax.Utilities;
using MvcApplication1.Models.Domain;

namespace MvcApplication1.Models
{
    public class MvcRequestContext
    {
        private readonly ViewDataDictionary _viewData;
        private readonly TempDataDictionary _tempData;
        private readonly ControllerContext _controllerContext;

        //required for creating mocks
        protected MvcRequestContext() { }

        public MvcRequestContext(ControllerContext controllerContext, ViewDataDictionary viewData, TempDataDictionary tempData)
        {
            _controllerContext = controllerContext;
            _viewData = viewData;
            _tempData = tempData;
        }

        public virtual StringBuilder RenderPageContentItemVersion(string markupVirtualPath, object model = null)
        {
            _viewData.Model = model;
            using (var writer = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(_controllerContext, markupVirtualPath);

                if (viewResult.View == null)
                    return null;

                var viewContext = new ViewContext(_controllerContext, viewResult.View, _viewData, _tempData, writer);
                viewResult.View.Render(viewContext, writer);
                viewResult.ViewEngine.ReleaseView(_controllerContext, viewResult.View);
                var result = writer.GetStringBuilder();
                if (result == null)
                    throw new ApplicationException(string.Format("Page file was not found by the path specified: '{0}'", markupVirtualPath));
                return result;
            }
        }

        public virtual bool HasCookie(string cookieName)
        {
            return _controllerContext.HttpContext.Request.Cookies[cookieName] != null;
        }

        public virtual string GetCookieValue(string cookieName)
        {
            var cookie = _controllerContext.HttpContext.Request.Cookies[cookieName];
            if (cookie == null) throw new ApplicationException(string.Format("Cookie named '{0}' was not found", cookieName));
            return cookie.Value;
        }
    }

    
}