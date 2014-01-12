using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;
using NUnit.Framework;

namespace FCG.RegoCms.Tests.ContentServiceTests
{
    public class GivenMultipleContentItems : GivenContentServiceContext
    {
        protected IEnumerable<ContentItem<SampleItem>> _contentItems;

        protected override void Given()
        {
            base.Given();
            RegisterSampleContentType();
            _contentItems = CreateMultipleContentItems(count:10);
        }

        [Test]
        public void CanGetLatestContentVersions()
        {
            var contentItems = _contentService.GetContentItems<SampleItem>();
            
            Assert.That(contentItems.Count(), Is.EqualTo(10));
            contentItems.ForEach(x => Assert.That(x.Last, Is.Not.Null));
        }

        
    }
}