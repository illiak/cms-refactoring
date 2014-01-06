using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models.Domain
{
    public class ContentManager
    {
        public IEnumerable<ContentItem<TContent>>           GetContentItems<TContent>() { throw new NotImplementedException(); }
        public ContentItem<TContent>                        GetContentItem<TContent>(Guid id) { throw new NotImplementedException(); }

        public IEnumerable<ContentItemVersion<TContent>>    GetVersions<TContent>(ContentVersionType versionType) { throw new NotImplementedException(); }
        public ContentItemVersion<TContent>                 GetVersion<TContent>(ContentVersionType versionType) { throw new NotImplementedException(); }

        public void Create<TContent>(Guid id, TContent content) { }
        public void Update<TContent>(Guid id, TContent content) { }
        public void Delete(Guid id) { }

        public void Publish(Guid contentItemId) { }
    }

    public class ContentItemVersion<TContent>
    {
        public Guid Id;
        public ContentVersionType Type;
        public TContent Content;
    }

    public enum ContentVersionType { Draft, Published }

    public class ContentItem<TContent>
    {
        public Guid Id;
        public ContentItemStatus Status;
        public TContent ContentDraft;
        public TContent ContentPublished;
    }

    public enum ContentItemStatus { Draft, Published, PublishedAndHasDraft, DeletedInDraft, Deleted }

}