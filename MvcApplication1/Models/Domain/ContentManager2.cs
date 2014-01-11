using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1
{
	public class ContentRepository
	{
		public ContentItem2<TContent>               GetContentItem<TContent>(Guid id) where TContent : class { throw new NotImplementedException(); }
		public IEnumerable<ContentItem2<TContent>>  GetContentItems<TContent>() where TContent : class { throw new NotImplementedException(); }
		public ContentItem2<TContent>    			Create<TContent>(Guid id, TContent content)
		{
			var newItem = ContentItem2<TContent>.Create(id, content);

		}
	}

	public class ContentItem2<TContent> where TContent : class
	{
		private readonly ContentItemVersion2<TContent> _draft;
		private readonly ContentItemVersion2<TContent> _published;

		public ContentItem2(TContent draft, TContent published = null)
		{
			_draft = draft;
			_published = published;
		}

		public ContentItemVersion2<TContent> Draft;
		public ContentItemVersion2<TContent> Published;
		public ContentItemVersion2<TContent> Last; 

		public DateTime CreatedOn;
		public DateTime ModifiedOn;
		public DateTime PublishedOn;

		internal static ContentItem2<TContent> Create(Guid id, TContent content)
		{
			return new ContentItem2<TContent> (draft: content);
		}

		public void Update(TContent content)
		{
			if (Draft == null) Draft = new ContentItemVersion2<TContent>(content);
		}

		public void Publish()
		{
			Draft.Publish();
			Published = Draft;
			Draft = null;
		}
		public void Delete() { }
	}

	public class ContentItemVersion2<TContent> where TContent : class
	{
		public ContentItemVersion2(TContent content)
		{
			Content = content;
		}

		public Guid                     Id;
		public TContent                 Content;
		public ContentItemVersionMode   Mode;
		public ContentItemStatus2       Status;

		public DateTimeOffset           Created;
		public DateTimeOffset           Modified;
		public DateTimeOffset           Deleted;

		public void Publish()
		{
			if (Mode != ContentItemVersionMode.Draft) throw new ApplicationException("Only versions that are in Draft mode can be published");
			if (Status != ContentItemStatus2.Active) throw new ApplicationException("Only versions in Active status could be published");

			Mode = ContentItemVersionMode.Published;
			Modified = DateTimeOffset.UtcNow;
		}

		public void Update(TContent content) 
		{
			if(content == null) throw new ArgumentNullException("content", "Content should not be null");

			Content = content;
			Mode = ContentItemVersionMode.Draft;
			Modified = DateTimeOffset.UtcNow;
		}

		public void Delete()
		{
			if (Status == ContentItemStatus2.Deleted) throw new ApplicationException("This content version is already deleted");
			if (Status != ContentItemStatus2.Active) throw new ApplicationException("Only versions in Active status could be deleted");

			Status = ContentItemStatus2.Deleted;
		}
	}

	public enum ContentItemVersionMode { Draft, Published }
	public enum ContentItemStatus2 { Active, Deleted }
}

