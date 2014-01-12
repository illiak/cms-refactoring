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
                query = query.Where(x => x.Status != ContentItemStatus2.Deleted);

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
            Draft = new ContentItemVersion<TContent>(keySelector, draft);
            if (published != null)
                Published = new ContentItemVersion<TContent>(keySelector, published);
		    CreatedOn = DateTimeOffset.Now;
		}

	    public ContentItemVersion<TContent> Draft { get; private set; }
        public ContentItemVersion<TContent> Published { get; private set; }
        public ContentItemVersion<TContent> Last { get { return Draft ?? Published; } }

	    public ContentItemStatus2 Status {
	        get
	        {
                if (   (Draft != null && Draft.Status == ContentItemStatus2.Active)
                    || (Published != null && Published.Status == ContentItemStatus2.Active))
                { 
                    return ContentItemStatus2.Active;
                }
	            return ContentItemStatus2.Deleted;
	        }
	    }
	    public DateTimeOffset   CreatedOn { get; private set; }
        public DateTimeOffset?  ModifiedOn { get; private set; }
        public DateTimeOffset?  PublishedOn { get; private set; }

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
            updated.ModifiedOn = DateTimeOffset.Now;

            return updated;
        }

        public ContentItem<TContent> Publish()
		{
		    var published = Clone();

            published.Published = Draft.Publish();
            published.Draft = null;
            var now = DateTimeOffset.Now;
            published.ModifiedOn = now;
            published.PublishedOn = now;

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
            _keySelector = keySelector;
            Content = content;
        }

	    public Guid                     Id { get { return _keySelector(Content); } }
	    public TContent                 Content { get; internal set; }
		public ContentItemVersionType   Type { get; internal set; }
		public ContentItemStatus2       Status { get; internal set; }

		public DateTimeOffset           Created { get; internal set; }
		public DateTimeOffset           Modified { get; internal set; }
		public DateTimeOffset           Deleted { get; internal set; }

        internal ContentItemVersion<TContent> Publish()
		{
            Contract.Requires(Type == ContentItemVersionType.Draft, "Only Draft versions can be published");
            Contract.Requires(Status == ContentItemStatus2.Active, "Only versions with Active status can be published");

		    var published = Clone();
            published.Type = ContentItemVersionType.Published;
            published.Modified = DateTimeOffset.UtcNow;

            return published;
		}

        internal ContentItemVersion<TContent> Update(Action<TContent> updateFunc) 
		{
            Contract.Requires<ArgumentNullException>(updateFunc != null);

            var updated = Clone();
            updateFunc(updated.Content);
            updated.Type = ContentItemVersionType.Draft;
            updated.Modified = DateTimeOffset.UtcNow;

            return updated;
		}

        internal ContentItemVersion<TContent> Delete()
		{
            Contract.Requires(Status == ContentItemStatus2.Deleted, "This content version was already deleted");
            Contract.Requires(Status == ContentItemStatus2.Active, "Only Active version can be deleted");

            var deleted = Clone();
            deleted.Status = ContentItemStatus2.Deleted;

            return deleted;
		}

        internal ContentItemVersion<TContent> Clone()
        {
            return (ContentItemVersion<TContent>)this.MemberwiseClone();
        }
	}

	public enum ContentItemVersionType { Draft, Published }
	public enum ContentItemStatus2 { Active, Deleted }
}

