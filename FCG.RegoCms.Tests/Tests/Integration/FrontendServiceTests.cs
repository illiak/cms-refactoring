using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using FCG.RegoCms;
using FCG.RegoCms.Tests.Mocks;
using Microsoft.Practices.Unity;
using MvcApplication1;
using MvcApplication1.Models;
using MvcApplication1.Models.Domain;
using MvcApplication1.Models.Infrastructure;
using MvcIntegrationTestFramework.Browsing;
using MvcIntegrationTestFramework.Hosting;
using NUnit.Framework;

namespace FCG.RegoCms.Tests.Integration
{
    [TestFixture, Category("Integration")]
    public class FrontendServiceTests
    {
        const string hostAddress = "http://127.0.0.1/"; //MvcIntegrationTestFramework works only with 127.0.0.1 address

        [Test]
        public void CanViewCreatedDraftPage()
        {
            var host = AppHost.Simulate(@"\MvcApplication1");
            host.Start(browsingSession =>
            {
                SetupMocks(browsingSession, draftMode: true);

                var page = CreateTestPage();
                
                var response = browsingSession.Get(page.DraftData.Route);

                Assert.AreEqual(200, response.ResponseStatusCode);
            });
        }

        [Test]
        public void CanViewCreatedAndPublishedPage()
        {
            var host = AppHost.Simulate(@"\MvcApplication1");
            host.Start(browsingSession =>
            {
                SetupMocks(browsingSession);

                var page = CreateTestPage();
                page.Publish();

                var response = browsingSession.Get(page.PublishedData.Route);

                Assert.AreEqual(200, response.ResponseStatusCode);
            });
        }

        static void SetupMocks(BrowsingSession browsingSession, bool draftMode = false)
        {
            WebApiApplication.Container.RegisterType<ContentRepository, FakeContentRepository>(new SingletonLifetimeManager());
            
            WebApiApplication.Container.RegisterType<CmsService>(new SingletonLifetimeManager());
            var cmsBackendService = WebApiApplication.Container.Resolve<CmsService>();
            cmsBackendService.ContentChanged += () => browsingSession.Post(hostAddress + "updateContentFiles");

            var fakeMvcRequestContext = new FakeMvcRequestContext { HasAdminCookie = draftMode, HasDraftCookie = draftMode };
            var fakeMvcApplicationContext = new FakeMvcApplicationContext { FormsCookieIsAlwaysValid = true, MvcRequestContext = fakeMvcRequestContext };
            WebApiApplication.Container.RegisterInstance<MvcApplicationContext>(fakeMvcApplicationContext);
        }

        static Page CreateTestPage()
        {
            var markup = string.Format("<html><body>{0}</body></html>", RandomHelper.GetRandomString());

            var url = hostAddress + "en-gb/" + RandomHelper.GetRandomString();
            var name = "testPage" + RandomHelper.GetRandomString();

            var cmsBackendService = WebApiApplication.Container.Resolve<CmsService>();
            return cmsBackendService.CreatePage(name: name, route: url.ToString(), markup: markup);
        }
    }
}