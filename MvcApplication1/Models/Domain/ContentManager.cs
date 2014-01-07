using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Models.Domain
{
    public class InMemoryContentManager
    {
        private readonly Dictionary<Guid, ContentItem> _contentItems = new Dictionary<Guid, ContentItem>();

        public IEnumerable<ContentItem<TContent>>           GetContentItems<TContent>() { throw new NotImplementedException(); }

        public ContentItem<TContent>                        GetContentItem<TContent>(Guid id)
        {
            return (ContentItem<TContent>)_contentItems[id];
        }

        public IEnumerable<ContentItemVersion<TContent>>    GetVersions<TContent>(ContentVersionType versionType)
        {
            return _contentItems.Select(x => GetVersionOrNull<TContent>(x.Key, versionType))
                                .Where(x => x != null)
                                .ToArray();
        }
        public ContentItemVersion<TContent>                 GetVersionOrNull<TContent>(Guid id, ContentVersionType versionType)
        {
            ContentItem contentItem;
            switch (versionType)
            {
                case ContentVersionType.Draft:
                    contentItem = _contentItems.Where(x => x.Value.Id == id )
                                               .Where(x => x.Value.Status == ContentItemStatus.Draft ||
                                                           x.Value.Status == ContentItemStatus.PublishedAndHasDraft)
                                               .Select(x => x.Value)
                                               .SingleOrDefault();
                    break;
                case ContentVersionType.Published:
                    contentItem = _contentItems.Where(x => x.Value.Id == id)
                                               .Where(x => x.Value.Status == ContentItemStatus.Published ||
                                                           x.Value.Status == ContentItemStatus.PublishedAndHasDraft ||
                                                           x.Value.Status == ContentItemStatus.PublishedAndDeletedInDraft)
                                               .Select(x => x.Value)
                                               .SingleOrDefault();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("versionType");
            }

            if (contentItem == null) return null;

            return new ContentItemVersion<TContent>((ContentItem<TContent>)contentItem, versionType);
        }

        public ContentItem<TContent> Create<TContent>(Guid id, TContent content)
        {
            var contentItem = new ContentItem<TContent> { Id = id, ContentDraft = content, Status = ContentItemStatus.Draft };
            _contentItems.Add(id, contentItem);
            return contentItem;
        }

        public void Update<TContent>(Guid id, TContent content)
        {
            var contentItem = GetContentItem<TContent>(id);
            
            switch (contentItem.Status)
            {
                case ContentItemStatus.Draft:
                    contentItem.ContentDraft = content;
                    break;
                case ContentItemStatus.Published:
                    throw new NotImplementedException();
                case ContentItemStatus.PublishedAndHasDraft:
                    throw new NotImplementedException();
                case ContentItemStatus.PublishedAndDeletedInDraft:
                    throw new NotImplementedException();
                case ContentItemStatus.DeletedInDraftAndNeverPublished:
                    throw new NotImplementedException();
                case ContentItemStatus.DeletedVersionPublished:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void Delete(Guid id) { }

        public void Publish(Guid contentItemId)
        {
            if (!_contentItems.ContainsKey(contentItemId))
                throw new ApplicationException("Unable to find content item with the id specified");

            var contentItem = _contentItems[contentItemId];

            switch (contentItem.Status)
            {
                case ContentItemStatus.Draft:
                    contentItem.ContentPublished = contentItem.ContentDraft;
                    contentItem.ContentDraft = null;
                    contentItem.Status = ContentItemStatus.Published;
                    break;
                case ContentItemStatus.Published:
                    throw new ApplicationException("Content item was already published and does not have a draft version");
                case ContentItemStatus.PublishedAndHasDraft:
                    throw new NotImplementedException();
                case ContentItemStatus.PublishedAndDeletedInDraft:
                    throw new NotImplementedException();
                case ContentItemStatus.DeletedInDraftAndNeverPublished:
                    throw new ApplicationException("Content item was deleted in draft and was never published before, so there is nothing to publish");
                case ContentItemStatus.DeletedVersionPublished:
                    throw new ApplicationException("Deleted version of the content was published already");;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class ContentItemVersion<TContent>
    {
        private readonly ContentItem<TContent> _contentItem;
        private readonly ContentVersionType _versionType;

        public ContentItemVersion(ContentItem<TContent> contentItem, ContentVersionType versionType)
        {
            _contentItem = contentItem;
            _versionType = versionType;
        }

        public Guid                 Id { get { return _contentItem.Id; } }
        public ContentVersionType   Type { get { return _versionType; } }
        public TContent             Content { 
            get
            {
                return _versionType == ContentVersionType.Draft
                    ? _contentItem.ContentDraft
                    : _contentItem.ContentPublished;
            }
        }
    }

    public enum ContentVersionType { Draft, Published }

    public class ContentItem
    {
        public Guid                Id { get; set; }
        public ContentItemStatus   Status { get; set; }
        public object              ContentDraft { get; set; }
        public object              ContentPublished { get; set; }
    }

    public class ContentItem<TContent> : ContentItem
    {
        public new TContent             ContentDraft { get { return (TContent)base.ContentDraft; } set { base.ContentDraft = value; } }
        public new TContent             ContentPublished { get { return (TContent)base.ContentPublished; } set { base.ContentPublished = value; } }
    }

    public enum ContentItemStatus { Draft, Published, PublishedAndHasDraft, PublishedAndDeletedInDraft, DeletedInDraftAndNeverPublished, DeletedVersionPublished }
} 