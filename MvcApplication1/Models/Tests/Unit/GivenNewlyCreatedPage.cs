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
using MvcApplication1.Models.Infrastructure;
using NUnit.Framework;

namespace MvcApplication1.Tests
{

    public class GivenNewlyCreatedPage : GivenNewlyCreatedPageContext
    {
        [Test]
        public void ItsDraftVersionCanBePreviewed()
        {
            _mvcRequestContextMock.HasAdminCookie = true;
            _mvcRequestContextMock.HasDraftCookie = true;
            var response = _cmsEngine.ProcessRequest(_page.Data.RoutePattern);

            Assert.That(response.Type, Is.EqualTo(ResponseType.OK));
            Assert.That(response.Body, Is.Not.Empty);
        }

        [Test]
        public void ItCanBePublishedAndViewed()
        {
            _page.Publish();
            var response = _cmsEngine.ProcessRequest(_page.Data.RoutePattern);

            Assert.That(response.Type, Is.EqualTo(ResponseType.OK));
            Assert.That(response.Body, Is.Not.Empty);
        }

        [Test]
        public void ItsInDraftStatus()
        {
            Assert.That(_page.Data.Status, Is.EqualTo(ContentStatus.Draft));
        }

        [Test]
        public void ItsDataInitializedProperly()
        {
            Assert.That(_page.Data.Name, Is.Not.Empty);
            Assert.That(_page.Data.Markup, Is.Not.Empty);
            Assert.That(_page.Data.RoutePattern, Is.Not.Empty);
            Assert.That(_page.Data.VirtualPath, Is.Not.Empty);
            Assert.That(_page.MarkupFile.Exists);
        }

        [Test]
        public void ItCanBeDeletedAndNoLongerAccessible()
        {
            _page.Delete();
            var response = _cmsEngine.ProcessRequest(_page.Data.RoutePattern);

            Assert.That(response.Type, Is.EqualTo(ResponseType.PageNotFound));
        }

        [Test]
        public void ItCanBeUpdated()
        {
            var updateData = new UpdatePageData
            {
                Name = "updated name",
                RoutePattern = "/en-gb/updatedRoutePattern",
                Markup = "<p>updated markup</p>"
            };

            _page.Update(updateData);

            Assert.That(updateData.RoutePattern, Is.EqualTo(_page.Data.RoutePattern));

            _cmsEngine.ProcessRequest(_page.Data.RoutePattern);
        }
    }

    public abstract class GivenNewlyCreatedPageContext : GivenCmsEngineContext
    {
        protected Page _page;

        protected override void Given()
        {
            base.Given();

            _page = CreateTestPage();
        }
    }
}