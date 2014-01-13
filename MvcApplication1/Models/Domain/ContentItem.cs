using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace FCG.RegoCms
{ 
	public class ContentItem<TContent> where TContent : class
	{
		public ContentItem(Func<TContent, Guid> keySelector, TContent draft, TContent published = null)
		{
            Contract.Requires(keySelector != null);
            Contract.Requires(draft != null || published != null);

            Id = draft != null ? keySelector(draft) : keySelector(published);
            if (draft != null)
                Draft = new ContentItemVersion<TContent>(Guid.NewGuid(), keySelector, draft);
            if (published != null)
                Published = new ContentItemVersion<TContent>(Guid.NewGuid(), keySelector, published);
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

        public ContentItem<TContent> Update(Action<TContent> updateFunc)
        {
            Contract.Requires(updateFunc != null);

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

    public enum ContentVersionType { Draft, Published }
	public enum ContentStatus { Active, Deleted }
}

