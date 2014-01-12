using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using FCG.RegoCms;
using Microsoft.Practices.Unity;
using MvcApplication1.Models;
using MvcApplication1.Models.Infrastructure;
using MvcIntegrationTestFramework.Hosting;
using NUnit.Framework;

namespace MvcApplication1.Tests.Integration
{
    [TestFixture, Category("Integration")]
    public class EngineTests
    {
        const string hostAddress = "http://127.0.0.1/"; //MvcIntegrationTestFramework works only with 127.0.0.1 address

        [Test]
        public void CanViewCreatedDraftPage()
        {
            var host = AppHost.Simulate(@"\MvcApplication1");
            host.Start(browsingSession =>
            {
                var cmsBackendService = WebApiApplication.Container.Resolve<CmsBackendService>();
                cmsBackendService.ContentChanged += () => browsingSession.Post(hostAddress + "updateContentFiles");

                var markup = string.Format("<html><body>{0}</body></html>", RandomHelper.GetRandomString());

                var url = hostAddress + "en-gb/" + RandomHelper.GetRandomString();
                var name = "testPage" + RandomHelper.GetRandomString();

                cmsBackendService.CreatePage(name: name, route: url.ToString(), markup: markup);
                
                browsingSession.Post(hostAddress + "simulateAdminLogin");
                browsingSession.Post(hostAddress + "simulateShowDraftsMode");

                var response = browsingSession.Get(url);
                Assert.AreEqual(200, response.ResponseStatusCode);
            });
        }

        [Test]
        public void CanViewCreatedAndPublishedPage()
        {
            var host = AppHost.Simulate(@"\MvcApplication1");
            host.Start(browsingSession =>
            {
                var cmsBackendService = WebApiApplication.Container.Resolve<CmsBackendService>();
                cmsBackendService.ContentChanged += () => browsingSession.Post(hostAddress + "updateContentFiles");

                var markup = string.Format("<html><body>{0}</body></html>", RandomHelper.GetRandomString());

                var url = hostAddress + "en-gb/" + RandomHelper.GetRandomString();
                var name = "testPage" + RandomHelper.GetRandomString();

                var page = cmsBackendService.CreatePage(name: name, route: url.ToString(), markup: markup);
                page.Publish();

                var response = browsingSession.Get(url);
                Assert.AreEqual(200, response.ResponseStatusCode);
            });
        }
    }
}