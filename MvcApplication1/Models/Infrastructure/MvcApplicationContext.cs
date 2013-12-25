using System.Web.Hosting;

namespace MvcApplication1.Models
{
    public class MvcApplicationContext
    {
        public virtual string GetFileSystemPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(virtualPath);
        }
    }
}