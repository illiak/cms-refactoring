using MvcApplication1.Models;
using NUnit.Framework;

namespace MvcApplication1.Tests
{
    public class GivenUpdatedPage : GivenNewlyCreatedPageContext
    {
        UpdatePageData  _updateData;

        protected override void Given()
        {
            base.Given();

            _updateData = new UpdatePageData
            {
                Name = "updated name",
                RoutePattern = "/en-gb/updatedRoutePattern",
                Markup = "<p>updated markup</p>"
            };

            _page.Update(_updateData);
        }

        [Test]
        public void ItsStatusBecomesDraft()
        {
            Assert.That(_page.Data.Status, Is.EqualTo(ContentStatus.Draft));
        }

        [Test, Ignore]
        public void PublishedVersionWithSameIdIsStillAvailable()
        {
//            var response = _cmsEngine.ProcessRequest(_page.Data.RoutePattern);
//            Assert.That(response.Type, Is.EqualTo(ResponseType.OK));
//
//            var page = _cmsEngine.GetPage(_page.Data.Name, ContentStatus.Published);
//            Assert.That(page, Is.Not.Null);
        }

        [Test, Ignore]
        public void UpdatedContentHasNoEffectOnPublishedVersion()
        {
//            var response = _cmsEngine.ProcessRequest(_updatedPage.Data.RoutePattern);
//            var renderedPage = _mvcRequestContextMock.RenderPage(_updatedPage);
//            Assert.That(_mvcRequestContextMock.IsRendered(sourceMarkup: _page.Data.Markup, renderedMarkup: response.Body), Is.False);
//
//            var publishedPage = _cmsEngine.GetPage(_page.Data.Id, ContentStatus.Published);
//            Assert.That(_page.Data.Markup, Is.Not.EqualTo(page.Data.Markup));
        }
    }
}