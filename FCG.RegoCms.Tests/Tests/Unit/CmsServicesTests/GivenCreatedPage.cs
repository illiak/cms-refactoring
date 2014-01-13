using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using FCG.RegoCms;
using Microsoft.Practices.Unity;
using MvcApplication1.Models;
using MvcApplication1.Models.Domain;
using MvcApplication1.Models.Infrastructure;
using NUnit.Framework;

namespace FCG.RegoCms.Tests.CmsServicesTests
{
    public class GivenCreatedPage : GivenCreatedPageContext
    {
        [Test]
        public void ItsDataInitializedProperly()
        {
            Assert.That(_page.DraftData.Name, Is.Not.Empty);
            Assert.That(_page.DraftData.Markup, Is.Not.Empty);
            Assert.That(_page.DraftData.Route, Is.Not.Empty);
        }

        [Test]
        public void CanGetLastVersion()
        {
            Assert.That(_page.LastData, Is.Not.Null);
        }

        [Test]
        public void ItsDraftVersionCanBePreviewed()
        {
            _mvcRequestContextMock.HasAdminCookie = true;
            _mvcRequestContextMock.HasDraftCookie = true;
            _cmsFrontendService.UpdateContentFiles();

            var response = _cmsFrontendService.ProcessRequest(_page.DraftData.Route);

            Assert.That(response.Type, Is.EqualTo(ResponseType.OK));
            Assert.That(response.Body, Is.Not.Empty);
        }

        [Test]
        public void ItCanBePublishedAndViewed()
        {
            _page.Publish();
            var response = _cmsFrontendService.ProcessRequest(_page.PublishedData.Route);

            Assert.That(response.Type, Is.EqualTo(ResponseType.OK));
            Assert.That(response.Body, Is.Not.Empty);
        }

        [Test]
        public void ItsLastVersionIsDraft()
        {
            Assert.That(_page.LastVersion.Type, Is.EqualTo(ContentVersionType.Draft));
        }

        [Test, Ignore]
        public void ItCanBeDeletedAndNoLongerAccessible()
        {
            _page.Delete();

            _mvcRequestContextMock.HasAdminCookie = true;
            _mvcRequestContextMock.HasDraftCookie = true;
            var response = _cmsFrontendService.ProcessRequest(_page.DraftData.Route);

            Assert.That(response.Type, Is.EqualTo(ResponseType.PageNotFound));
        }

        [Test]
        public void ItCanBeUpdated()
        {
            var updateData = new UpdatePageData
            {
                Name = "updated name",
                Route = "http://test.com/en-gb/updatedRoutePattern",
                Markup = "<p>updated markup</p>"
            };

            _page.Update(updateData);

            Assert.That(_page.LastVersion.Type, Is.EqualTo(ContentVersionType.Draft));
            Assert.That(updateData.Route, Is.EqualTo(_page.DraftData.Route));
        }
    }

    public abstract class GivenCreatedPageContext : GivenCmsServicesContext
    {
        protected Page _page;

        protected override void Given()
        {
            base.Given();

            _page = CreateTestPage();
        }
    }
}