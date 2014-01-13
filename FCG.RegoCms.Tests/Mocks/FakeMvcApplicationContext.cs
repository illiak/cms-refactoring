using System.IO;
using System.Text;
using MvcApplication1.Models;

namespace FCG.RegoCms.Tests.Mocks
{
    internal class FakeMvcApplicationContext : MvcApplicationContext
    {
        public bool FormsCookieIsAlwaysValid;
        public MvcRequestContext MvcRequestContext;

        public override string GetFileSystemPath(string virtualPath)
        {
            return GetFileSystemTempPath(virtualPath);
        }

        public override bool IsFormsCookieValueValid(string cookieValue)
        {
            if (FormsCookieIsAlwaysValid) return true;

            return base.IsFormsCookieValueValid(cookieValue);
        }

        public override MvcRequestContext GetCurrentMvcRequestContext()
        {
            return MvcRequestContext;
        }

        internal static string GetFileSystemTempPath(string virtualPath)
        {
            var result = new StringBuilder(virtualPath);
            result.Replace("~/", Path.GetTempPath());
            result.Replace("/", "\\");

            return result.ToString();
        }
    }
}