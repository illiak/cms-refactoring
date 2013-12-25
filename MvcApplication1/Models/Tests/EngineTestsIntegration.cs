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
using MvcIntegrationTestFramework.Hosting;
using NUnit.Framework;

namespace MvcApplication1.Tests
{
    [TestFixture, Category("Integration")]
    public class EngineTestsIntegration
    {
        public EngineTestsIntegration()
        {
        }

        [Test]
        public void CanCreateViewAndPreviewItsDraft()
        {
            var host = AppHost.Simulate(@"\MvcApplication1");
            host.Start(browsingSession =>
            {
                var cmsEngine = WebApiApplication.Container.Resolve<CmsEngine>();

                var markup = string.Format("<html><body>{0}</body></html>", RandomHelper.GetRandomString());
                //MvcIntegrationTestFramework works only with 127.0.0.1 address
                var url = new Uri(string.Format("http://127.0.0.1/en-gb/{0}", RandomHelper.GetRandomString())); 

                cmsEngine.CreateView(markup, url.ToString(), ViewStatus.Release);

                var response = browsingSession.Get(url);
                Assert.AreEqual(200, response.ResponseStatusCode);
            });
        }
    }
}