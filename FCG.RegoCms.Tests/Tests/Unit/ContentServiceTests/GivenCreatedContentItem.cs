using System;
using NUnit.Framework;

namespace FCG.RegoCms.Tests.ContentServiceTests
{
    public class GivenCreatedContentItem : GivenCreatedContentItemContext
    {
        [Test]
        public void AllPropertiesAreValid()
        {
            Assert.That(_createdContentItem.Draft, Is.Not.Null);
            Assert.That(_createdContentItem.Draft.Id, Is.Not.EqualTo(default(Guid)));
            Assert.That(_createdContentItem.Draft.Type, Is.EqualTo(ContentItemVersionType.Draft));
            Assert.That(_createdContentItem.Draft.Status, Is.EqualTo(ContentItemStatus2.Active));
            Assert.That(_createdContentItem.Draft.Content, Is.Not.Null);

            Assert.That(_createdContentItem.Published, Is.Null);
            Assert.That(_createdContentItem.Last, Is.EqualTo(_createdContentItem.Draft));
            Assert.That(_createdContentItem.CreatedOn, Is.Not.EqualTo(default(DateTimeOffset)));
            Assert.That(_createdContentItem.ModifiedOn, Is.Null);
            Assert.That(_createdContentItem.PublishedOn, Is.Null);
        }

        [Test]
        public void ItCanBeUpdated()
        {
            Assert.DoesNotThrow(() =>
            {
                var updated = _createdContentItem.Update(x =>
                {
                    x.Name = "updated name";
                    x.Markup = "updated markup";
                });

                Assert.That(updated.Last.Type, Is.EqualTo(ContentItemVersionType.Draft));
            });
        }

        [Test]
        public void ItCanBePublished()
        {
            Assert.DoesNotThrow(() =>
            {
                var published = _createdContentItem.Publish();
                Assert.That(published, Is.Not.Null);
            });
        }

        [Test]
        public void CanGetSingleContentItem()
        {
            var contentItem = _contentService.GetContentItem<SampleItem>(_newSampleItem.Id);
            Assert.That(contentItem, Is.Not.Null);
        }
    }

    public abstract class GivenCreatedContentItemContext : GivenContentServiceContext
    {
        protected ContentItem<SampleItem>  _createdContentItem;
        protected SampleItem                _newSampleItem;

        protected override void Given()
        {
            base.Given();

            RegisterSampleContentType();
            _newSampleItem = CreateSampleItem();
            _createdContentItem = CreateTestContentItem(_newSampleItem);
        }
    }

    
}