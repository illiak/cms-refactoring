using System;

namespace MvcApplication1.Models
{
    public class PageData
    {
        public Guid             Id;
        public string           Name;
        public string           RoutePattern;
        public string           Markup;
        public string           ViewPath;

        public PageData Clone()
        {
            return (PageData)MemberwiseClone();
        }
    }
}