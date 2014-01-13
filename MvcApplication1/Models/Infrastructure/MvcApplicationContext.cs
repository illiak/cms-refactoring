using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Ajax.Utilities;

namespace MvcApplication1.Models
{
    public class MvcApplicationContext
    {
        public virtual string GetFileSystemPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }

        public virtual bool IsFormsCookieValueValid(string cookieValue)
        {
            FormsAuthenticationTicket ticket;
            try
            {
                ticket = FormsAuthentication.Decrypt(cookieValue);
            }
            catch (ArgumentException)
            {
                ticket = null;
            }
            return ticket != null;
        }

        public void SaveCurrentRequestContextData(ControllerContext controllerContext, ViewDataDictionary viewData, TempDataDictionary tempData)
        {
            HttpContext.Current.Items["mvcRequestContext"] = new MvcRequestContext(controllerContext, viewData, tempData);
        }

        public virtual MvcRequestContext GetCurrentMvcRequestContext()
        {
            return (MvcRequestContext)HttpContext.Current.Items["mvcRequestContext"];
        }
    }
}