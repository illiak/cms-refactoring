using System;

namespace MvcApplication1
{
	public class ContentRepositoryTests
	{
		ContentRepository _contentRepository;

		public void Init()
		{
			_contentRepository = new ContentRepository();
		}

		public void CanCreateContentItem()
		{
			var contentItem = new SampleItem { Id = Guid.NewGuid(), Name = "test01" };
			_contentRepository.Create(contentItem.Id, contentItem);
		}

		class SampleItem 
		{
			public Guid 	Id;
			public string 	Name;
			public string   Markup;
			public string 	ViewPath;
		}
	}
}

