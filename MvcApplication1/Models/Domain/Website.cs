using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models.Domain
{
//    public class CmsEngine2
//    {
//        List<ContentItem>       _draftContentItems;
//        List<ContentChange>     _draftPendingChanges;
//
//        List<ContentItem>       _publishedContentItems;
//
//        public IEnumerable<ContentItem> GetDraftVersion() { }
//        public IEnumerable<ContentItem> GetPublishedVersion() { }
//            
//        public ContentItem  CreatePage(CreatePageData createPageData)
//        {
//            var page = new ContentPage(createPageData);
//        }
//
//        public void Publish(IEnumerable<ContentChange> changes) { }
//
//        public void Publish(ContentChange contentChange) { }
//    }

    public class ContentItem<TContent>
    {
        public Guid                 Id;
        public ContentItemStatus    Status;

        public TContent Published;
        public TContent Draft;
    }

    public enum ContentItemStatus
    {
        Draft, Published, PublishedAndHasDraft, DraftDeleted, Deleted
    }

    public class ContentManager
    {
        public IEnumerable<TContent> GetDraftVersion<TContent>()
        {
            
        }

        public IEnumerable<TContent> GetPublishedVersion<TContent>()
        {

        }
    }
}