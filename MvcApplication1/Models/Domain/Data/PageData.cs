using System;

namespace MvcApplication1.Models
{
    public class PageData
    {
        public Guid   Id { get; internal set; }
        public string Name { get; internal set; }
        public string RoutePattern { get; internal set; }
        public string Markup { get; internal set; }
        public string ViewPath { get; internal set; }

        public PageData Clone()
        {
            return (PageData)MemberwiseClone();
        }
    }
}