using System;

namespace MvcApplication1.Models
{
    public class PageData
    {
        public Guid   Id { get; internal set; }
        public string Name { get; internal set; }
        public string LanguageCode { get; internal set; }
        public Guid?  LayoutId { get; internal set; }
        public string RoutePattern { get; internal set; }
        public string Title { get; internal set; }
        public string Markup { get; internal set; }
        public string ViewPath { get; internal set; }

        public PageData Clone()
        {
            return (PageData)MemberwiseClone();
        }
    }

    public class LayoutData
    {
        public Guid     Id { get; internal set; }
        public string   Name { get; internal set; }
        public string   LanguageCode { get; internal set; }
        public Guid?    LayoutId { get; internal set; }
        public string   Markup { get; internal set; }
        public string   ViewPath { get; internal set; }

        public LayoutData Clone()
        {
            return (LayoutData)MemberwiseClone();
        }
    }

    public class FragmentData
    {
        public Guid     Id { get; internal set; }
        public string   Name { get; internal set; }
        public string   LanguageCode { get; internal set; }
        public Guid?    LayoutId { get; internal set; }
        public string   Markup { get; internal set; }
        public string   ViewPath { get; internal set; }

        public LayoutData Clone()
        {
            return (LayoutData)MemberwiseClone();
        }
    }
}