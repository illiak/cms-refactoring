using FCG.RegoCms;
using MvcApplication1.Models;
using MvcApplication1.Models.Domain;
using NUnit.Framework;

namespace FCG.RegoCms.Tests.Unit.CmsServicesTests
{
    class GivenCreatedAndUpdatedPage : GivenCreatedPageContext
    {
        UpdatePageData  _updateData;

        protected override void Given()
        {
            base.Given();

            _updateData = new UpdatePageData
            {
                Name = "updated name",
                Route = "/en-gb/updatedRoutePattern",
                Markup = "<p>updated markup</p>"
            };

            _page.Update(_updateData);
        }

        [Test]
        public void ItsStatusBecomesDraft()
        {
            Assert.That(_page.LastVersion.Type, Is.EqualTo(ContentVersionType.Draft));
        }

        [Test]
        public void ItsDraftIsAccessibleAfterUpdate()
        {
            var updateData = new UpdatePageData
            {
                Name = "updated name",
                Route = "http://test.com/en-gb/updatedRoutePattern",
                Markup = "<p>updated markup</p>"
            };

            _page.Update(updateData);

            _mvcRequestContextMock.HasAdminCookie = true;
            _mvcRequestContextMock.HasDraftCookie = true;
            var response = _cmsFrontendService.ProcessRequest(_page.DraftData.Route);

            Assert.That(response.Type, Is.EqualTo(ResponseType.OK));
        }
    }
}