using System;
using System.Collections;
using System.Collections.Generic;
using MvcApplication1.Models.Infrastructure;
using MvcApplication1.Tests;
using NUnit.Framework;

namespace FCG.RegoCms.Tests.ContentItemTests
{
    public class ContentItemTests : ContentItemTestsContext
	{
        [Test]
		public void CanCreateNewContentItem()
        {
            var contentItem = CreateTestContentItem();
            Assert.That(contentItem, Is.Not.Null);
		}

        [Test]
        public void CanCreateMultipleContentItems()
        {
            Assert.DoesNotThrow(() =>
            {
                var contentItems = CreateMultipleContentItems();
                Assert.That(contentItems, Is.Not.Empty);
            });
        }
	}

    public class ContentItemTestsContext : BddUnitTestBase
    {
        protected ContentItem<SampleItem> CreateTestContentItem(SampleItem sampleItem = null)
        {
            var data = sampleItem ?? CreateSampleItem();

            return new ContentItem<SampleItem>(x => x.Id, draft: data);
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

        protected class SampleItem
        {
            public Guid Id;
            public string Name;
            public string Markup;
            public string ViewPath;
        }
    }
}