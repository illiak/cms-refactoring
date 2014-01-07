using MvcApplication1.Models;
using MvcApplication1.Models.Domain;
using NUnit.Framework;

namespace MvcApplication1.Tests
{
    public class GivenCreatedAndUpdatedPage : GivenCreatedPageContext
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
            Assert.That(_page.Status, Is.EqualTo(ContentItemStatus.Draft));
        }

        [Test]
        public void ItsDraftIsAccessibleAfterUpdate()
        {
            var updateData = new UpdatePageData
            {
                Name = "updated name",
                RoutePattern = "http://test.com/en-gb/updatedRoutePattern",
                Markup = "<p>updated markup</p>"
            };

            _page.Update(updateData);

            _mvcRequestContextMock.HasAdminCookie = true;
            _mvcRequestContextMock.HasDraftCookie = true;
            var response = _cmsEngine.ProcessRequest(_page.DataDraft.RoutePattern);

            Assert.That(response.Type, Is.EqualTo(ResponseType.OK));
        }
    }
}