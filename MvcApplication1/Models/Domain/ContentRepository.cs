using System;
using System.Data.Entity;
using MvcApplication1.Models;

namespace FCG.RegoCms
{
    public class ContentRepository : DbContext
    {
        public DbSet<ContentItemData> ContentItems { get; private set; }
        public DbSet<ViewVersionData> ViewVersions { get; private set; }
    }

    public class ContentItemData
    {
        public Guid             Id { get; set; }
        public ContentStatus    Status { get; internal set; }
        public Guid?            DraftVersionId { get; set; }
        public Guid?            PublishedVersionId { get; set; }
    }

    public class ViewVersionData
    {
        public ViewVersionData(ContentItemVersion<PageData> pageVersion)
        {
            Id = pageVersion.Id;
            VersionType = pageVersion.Type;
            ViewType = RazorViewType.Page;
            Status = pageVersion.Status;
            CreatedOn = pageVersion.CreatedOn;
            ModifiedOn = pageVersion.ModifiedOn;
            DeletedOn = pageVersion.DeletedOn;
            
            ViewId = pageVersion.ContentId;
            Name = pageVersion.Content.Name;
            RoutePattern = pageVersion.Content.RoutePattern;
            LanguageCode = pageVersion.Content.LanguageCode;
            LayoutId = pageVersion.Content.LayoutId;
            Title = pageVersion.Content.Title;
            Markup = pageVersion.Content.Markup;
            ViewPath = pageVersion.Content.ViewPath;
        }

        public Guid                 Id { get; set; }
        public ContentVersionType   VersionType { get; internal set; }
        public RazorViewType        ViewType { get; internal set; }
        public ContentStatus        Status { get; internal set; }
        public DateTimeOffset       CreatedOn { get; set; }
        public DateTimeOffset?      ModifiedOn { get; set; }
        public DateTimeOffset?      DeletedOn { get; set; }

        public Guid     ViewId { get; internal set; }
        public string   Name { get; internal set; }
        public string   RoutePattern { get; internal set; }
        public string   LanguageCode { get; set; }
        public Guid?    LayoutId { get; internal set; }
        public string   Title { get; internal set; }
        public string   Markup { get; internal set; }
        public string   ViewPath { get; internal set; }
    }

    public enum RazorViewType { Page, Layout, Fragment }
}