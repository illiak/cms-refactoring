using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using MvcApplication1.Models;

namespace FCG.RegoCms.Tests.Mocks
{
    class FakeMvcRequestContext : MvcRequestContext
    {
        public bool HasDraftCookie;
        public bool HasAdminCookie;

        public override StringBuilder RenderPageContentItemVersion(string markupVirtualPath, object model = null)
        {
            var markupFileSystemPath = FakeMvcApplicationContext.GetFileSystemTempPath(markupVirtualPath);
            using (var reader = new FileInfo(markupFileSystemPath).OpenText())
            {
                return new StringBuilder(string.Format("<rendered>{0}</rendered>", reader.ReadToEnd()));
            }
        }

        public override string GetCookieValue(string cookieName)
        {
            if (cookieName == CmsFrontendService.ShowDraftsCookieName)
                return HasDraftCookie.ToString();

            if (cookieName == CmsFrontendService.AdminFormsCookieName)
                return "valid cookie for admin";

            throw new ApplicationException("this mock was intended for querying draft and admin cookies only");
        }

        public override bool HasCookie(string cookieName)
        {
            if (cookieName == CmsFrontendService.ShowDraftsCookieName)
                return HasDraftCookie;

            if (cookieName == CmsFrontendService.AdminFormsCookieName)
                return HasAdminCookie;

            return false;
        }
    }
}
