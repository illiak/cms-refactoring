using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Moq;
using MvcApplication1.Models;
using MvcApplication1.Models.Domain;
using MvcApplication1.Models.Infrastructure;
using NUnit.Framework;

namespace MvcApplication1.Tests
{
    public class GivenCreatedPage : GivenCreatedPageContext
    {
        [Test]
        public void ItsDataInitializedProperly()
        {
            Assert.That(_page.DataDraft.Name, Is.Not.Empty);
            Assert.That(_page.DataDraft.Markup, Is.Not.Empty);
            Assert.That(_page.DataDraft.RoutePattern, Is.Not.Empty);
        }

        [Test]
        public void CanGetLastVersion()
        {
            Assert.That(_page.LastVersion, Is.Not.Null);
        }

        [Test]
        public void ItsDraftVersionCanBePreviewed()
        {
            _mvcRequestContextMock.HasAdminCookie = true;
            _mvcRequestContextMock.HasDraftCookie = true;
            _cmsFrontendService.UpdateContentFiles();

            var response = _cmsFrontendService.ProcessRequest(_page.DataDraft.RoutePattern);

            Assert.That(response.Type, Is.EqualTo(ResponseType.OK));
            Assert.That(response.Body, Is.Not.Empty);
        }

        [Test]
        public void ItCanBePublishedAndViewed()
        {
            _page.Publish();
            var response = _cmsFrontendService.ProcessRequest(_page.DataPublished.RoutePattern);

            Assert.That(response.Type, Is.EqualTo(ResponseType.OK));
            Assert.That(response.Body, Is.Not.Empty);
        }

        [Test]
        public void ItsInDraftStatus()
        {
            Assert.That(_page.Status, Is.EqualTo(ContentItemStatus.Draft));
        }

        [Test, Ignore]
        public void ItCanBeDeletedAndNoLongerAccessible()
        {
            _page.Delete();

            _mvcRequestContextMock.HasAdminCookie = true;
            _mvcRequestContextMock.HasDraftCookie = true;
            var response = _cmsFrontendService.ProcessRequest(_page.DataDraft.RoutePattern);

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

            Assert.That(_page.Status, Is.EqualTo(ContentItemStatus.Draft));
            Assert.That(updateData.Route, Is.EqualTo(_page.DataDraft.RoutePattern));
        }
    }

    public abstract class GivenCreatedPageContext : GivenCmsEngineContext
    {
        protected Page _page;

        protected override void Given()
        {
            base.Given();

            _page = CreateTestPage();
        }
    }
}