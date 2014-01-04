using System;

namespace MvcApplication1.Models
{
    public class PageData
    {
        public Guid             Id;
        public string           Name;
        public ContentStatus    Status;
        public string           RoutePattern;
        public string           Markup;
        public string           VirtualPath;
    }
}