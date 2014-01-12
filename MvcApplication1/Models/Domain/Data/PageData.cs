using System;

namespace MvcApplication1.Models
{
    public class Page
    {
        public Guid   Id { get; internal set; }
        public string Name { get; internal set; }
        public string LanguageCode { get; internal set; }
        public Guid?  LayoutId { get; internal set; }
        public string RoutePattern { get; internal set; }
        public string Title { get; internal set; }
        public string Markup { get; internal set; }
        public string ViewPath { get; internal set; }

        public Page Clone()
        {
            return (Page)MemberwiseClone();
        }
    }

    public class Layout
    {
        public Guid     Id { get; internal set; }
        public string   Name { get; internal set; }
        public string   LanguageCode { get; internal set; }
        public Guid?    LayoutId { get; internal set; }
        public string   Markup { get; internal set; }
        public string   ViewPath { get; internal set; }

        public Layout Clone()
        {
            return (Layout)MemberwiseClone();
        }
    }

    public class Fragment 
    {
        public Guid     Id { get; internal set; }
        public string   Name { get; internal set; }
        public string   LanguageCode { get; internal set; }
        public Guid?    LayoutId { get; internal set; }
        public string   Markup { get; internal set; }
        public string   ViewPath { get; internal set; }

        public Layout Clone()
        {
            return (Layout)MemberwiseClone();
        }
    }
}