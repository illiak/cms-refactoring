using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace FCG.RegoCms
{ 
	public class ContentService
	{
        readonly Dictionary<Guid, object>                   _contentItems = new Dictionary<Guid, object>();
        readonly Dictionary<Type, ContentTypeDescriptor>    _contentTypeDescriptors = new Dictionary<Type, ContentTypeDescriptor>();

        public void RegisterContentType<TContent>(string name, Func<TContent, Guid> keySelector)
        {
            _contentTypeDescriptors.Add(typeof(TContent), new ContentTypeDescriptor
            {
                ContentType = typeof(TContent),
                Name = name,
                KeySelector = x => keySelector((TContent)x)
            });
        }

	    public string                               GetContentTypeName<TContent>()
	    {
	        return _contentTypeDescriptors[typeof (TContent)].Name;
	    }
	    public ContentItem<TContent>               GetContentItem<TContent>(Guid id) where TContent : class
	    {
            return (ContentItem<TContent>)_contentItems[id];
	    }
	    public ContentItem<TContent>[]             GetContentItems<TContent>(bool excludeDeleted = true) where TContent : class
	    {
	        var query = _contentItems.Values.OfType<ContentItem<TContent>>();
            if (excludeDeleted)
                query = query.Where(x => x.Status != ContentStatus.Deleted);

            return query.ToArray();
	    }
        public ContentItem<TContent>               Create<TContent>(TContent content) where TContent : class
		{
            var newItem = ContentItem<TContent>.Create(GetKey, content);
            _contentItems.Add(GetKey(content), newItem);
            return newItem;
		}

        Guid GetKey<TContent>(TContent content)
        {
            Contract.Assume(_contentTypeDescriptors.ContainsKey(content.GetType()));

            var keySelector = _contentTypeDescriptors[content.GetType()].KeySelector;

            return keySelector(content);
        }

	    class ContentTypeDescriptor
	    {
	        public Type                ContentType;
            public string              Name;
	        public Func<object, Guid>  KeySelector;
	    }
	}

	public class ContentItem<TContent> where TContent : class
	{
		internal ContentItem(Func<TContent, Guid> keySelector, TContent draft, TContent published = null)
		{
            Id = keySelector(draft);
            Draft = new ContentItemVersion<TContent>(keySelector, draft);
            if (published != null)
                Published = new ContentItemVersion<TContent>(keySelector, published);
		}

	    public Guid                         Id { get; private set; }
	    public ContentItemVersion<TContent> Draft { get; private set; }
        public ContentItemVersion<TContent> Published { get; private set; }
        public ContentItemVersion<TContent> Last { get { return Draft ?? Published; } }

	    public ContentStatus        Status {
	        get
	        {
                if (   (Draft != null && Draft.Status == ContentStatus.Active)
                    || (Published != null && Published.Status == ContentStatus.Active))
                { 
                    return ContentStatus.Active;
                }
	            return ContentStatus.Deleted;
	        }
	    }
	    public DateTimeOffset       CreatedOn 
        {
	        get
	        {
	            return Draft != null ? Draft.CreatedOn : Published.CreatedOn;
	        }
	    }
        public DateTimeOffset?      ModifiedOn {
            get { return Last.ModifiedOn; }
        }
        public DateTimeOffset?      PublishedOn {
            get { return Published != null ? Published.ModifiedOn : null; }
        }

		internal static ContentItem<TContent> Create(Func<TContent, Guid> keySelector, TContent content)
		{
            return new ContentItem<TContent>(keySelector, draft: content);
		}

        public ContentItem<TContent> Update(Action<TContent> updateFunc)
        {
            var updated = Clone();
            if (Last == Draft)
            {
                updated.Draft = Draft.Update(updateFunc);
            }
            else if (Last == Published)
            {
                updated.Draft = Published.Update(updateFunc);
            }

            return updated;
        }

        public ContentItem<TContent> Publish()
		{
		    var published = Clone();

            published.Published = Draft.Publish();
            published.Draft = null;

		    return published;
		}
        public ContentItem<TContent> Delete() { throw new NotImplementedException(); }

	    public ContentItem<TContent> Clone()
	    {
	        return (ContentItem<TContent>) MemberwiseClone();
	    }
	}

	public class ContentItemVersion<TContent> where TContent : class
	{
        private readonly Func<TContent, Guid> _keySelector;

        public ContentItemVersion(Func<TContent, Guid> keySelector, TContent content)
        {
            Id = Guid.NewGuid();
            _keySelector = keySelector;
            Content = content;
        }

	    public Guid                 Id { get; private set; }
	    public Guid                 ContentId { get { return _keySelector(Content); } }
	    public TContent             Content { get; internal set; }
		public ContentVersionType   Type { get; internal set; }
		public ContentStatus        Status { get; internal set; }

		public DateTimeOffset       CreatedOn { get; internal set; }
		public DateTimeOffset?      ModifiedOn { get; internal set; }
		public DateTimeOffset?      DeletedOn { get; internal set; }

        internal ContentItemVersion<TContent> Publish()
		{
            Contract.Requires(Type == ContentVersionType.Draft, "Only Draft versions can be published");
            Contract.Requires(Status == ContentStatus.Active, "Only versions with Active status can be published");

		    var published = Clone();
            published.Id = Guid.NewGuid();
            published.Type = ContentVersionType.Published;
            published.ModifiedOn = DateTimeOffset.UtcNow;

            return published;
		}

        internal ContentItemVersion<TContent> Update(Action<TContent> updateFunc) 
		{
            Contract.Requires<ArgumentNullException>(updateFunc != null);

            var updated = Clone();
            updated.Id = Guid.NewGuid();
            updateFunc(updated.Content);
            updated.Type = ContentVersionType.Draft;
            updated.ModifiedOn = DateTimeOffset.UtcNow;

            return updated;
		}

        internal ContentItemVersion<TContent> Delete()
		{
            Contract.Requires(Status == ContentStatus.Deleted, "This content version was already deleted");
            Contract.Requires(Status == ContentStatus.Active, "Only Active version can be deleted");

            var deleted = Clone();
            deleted.Id = Guid.NewGuid();
            deleted.Status = ContentStatus.Deleted;

            return deleted;
		}

        internal ContentItemVersion<TContent> Clone()
        {
            return (ContentItemVersion<TContent>)this.MemberwiseClone();
        }
	}

	public enum ContentVersionType { Draft, Published }
	public enum ContentStatus { Active, Deleted }
}

