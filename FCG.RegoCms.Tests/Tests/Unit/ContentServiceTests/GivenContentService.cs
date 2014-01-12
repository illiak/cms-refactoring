using System;
using System.Collections;
using System.Collections.Generic;
using MvcApplication1.Models.Infrastructure;
using MvcApplication1.Tests;
using NUnit.Framework;

namespace FCG.RegoCms.Tests.ContentServiceTests
{
    public class GivenContentService : GivenContentServiceContext
	{
        [Test]
        public void CanRegisterNewContentType()
        {
            RegisterSampleContentType();
        }

        [Test]
		public void CanCreateNewContentItem()
        {
            RegisterSampleContentType();
            var contentItem = CreateTestContentItem();
            Assert.That(contentItem, Is.Not.Null);
		}

        [Test]
        public void CanCreateMultipleContentItems()
        {
            RegisterSampleContentType();
            
            Assert.DoesNotThrow(() =>
            {
                var contentItems = CreateMultipleContentItems();
                Assert.That(contentItems, Is.Not.Empty);
            });
        }
	}

    public abstract class GivenContentServiceContext : BddUnitTestBase
    {
        protected ContentService _contentService;

        protected override void Given()
        {
            _contentService = new ContentService();
        }

        protected ContentItem<SampleItem> CreateTestContentItem(SampleItem sampleItem = null)
        {
            var data = sampleItem ?? CreateSampleItem();
            return _contentService.Create(data);
        }

        protected IEnumerable<ContentItem<SampleItem>> CreateMultipleContentItems(int count = 10)
        {
            var result = new List<ContentItem<SampleItem>>();
            for (int i = 0; i < count; i++)
            {
                result.Add(CreateTestContentItem());
            }
            return result;
        }

        protected SampleItem CreateSampleItem()
        {
            return new SampleItem
            {
                Id = Guid.NewGuid(), 
                Name = RandomHelper.GetRandomString(), 
                Markup = RandomHelper.GetRandomString()
            };
        }

        protected void RegisterSampleContentType()
        {
            _contentService.RegisterContentType<SampleItem>(name: "Sample Type", keySelector: x => x.Id);
        }

        protected class SampleItem 
		{
			public Guid 	Id;
			public string 	Name;
			public string   Markup;
			public string 	ViewPath;
		}
    }
}