using System;
using System.Threading;
using NUnit.Framework;

namespace FCG.RegoCms.Tests.ContentServiceTests
{
    public class GivenPublishedContentItem : GivenPublishedContentItemContext
    {
        [Test]
        public void ItsPropertiesAreValid()
        {
            Assert.That(_publishedContentItem.Last, Is.EqualTo(_publishedContentItem.Published));
            Assert.That(_publishedContentItem.Last.Type, Is.EqualTo(ContentItemVersionType.Published));
            Assert.That(_publishedContentItem.Draft, Is.Null);

            Assert.That(_publishedContentItem.ModifiedOn, Is.Not.EqualTo(_createdContentItem.ModifiedOn));
            Assert.That(_publishedContentItem.CreatedOn, Is.EqualTo(_createdContentItem.CreatedOn));
            Assert.That(_publishedContentItem.PublishedOn, Is.Not.EqualTo(_createdContentItem.CreatedOn));
            Assert.That(_publishedContentItem.PublishedOn, Is.EqualTo(_publishedContentItem.ModifiedOn));
        }

        [Test]
        public void ItCanBeUpdated()
        {
            Assert.DoesNotThrow(() =>
            {
                var updated = _publishedContentItem.Update(x =>
                {
                    x.Name = "updated name";
                    x.Markup = "updated markup";
                });

                Assert.That(updated, Is.Not.Null);
                Assert.That(updated.Draft, Is.Not.Null);
                Assert.That(updated.Published, Is.Not.Null);
                Assert.That(updated.Last, Is.EqualTo(updated.Draft));
            });
        }
    }

    public abstract class GivenPublishedContentItemContext : GivenCreatedContentItemContext
    {
        protected ContentItem<SampleItem> _publishedContentItem;

        protected override void Given()
        {
            base.Given();

            //making sure that publish time will be different
            Thread.Sleep(TimeSpan.FromMilliseconds(5)); 
            
            _publishedContentItem = _createdContentItem.Publish();
        }
    }
}