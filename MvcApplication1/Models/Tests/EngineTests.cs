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
    [TestFixture, Category("Unit")]
    public class EngineTests
    {
        private readonly UnityContainer _container;
        private CmsEngine               _cmsEngine;

        public EngineTests()
        {
            _container = new UnityContainer();
        }

        [Test]
        public void CanCreateViewAndPreviewItsDraft()
        {
            var mockRequestContext = new MvcRequestContextMock();
            _container.RegisterInstance<MvcRequestContext>(mockRequestContext);
            _container.RegisterInstance<MvcApplicationContext>(new MvcApplicationContextMock());

            _cmsEngine = _container.Resolve<CmsEngine>();

            var markup = string.Format("<html><body>{0}</body></html>", RandomHelper.GetRandomString());
            var routePattern = string.Format("http://test.com/en-gb/{0}", RandomHelper.GetRandomString());
            var name = "testPage-" + RandomHelper.GetRandomString();

            var page = _cmsEngine.CreatePage(name, routePattern, markup);
            page.Publish();

            var response = _cmsEngine.ProcessRequest(
                routePattern, 
                showDrafts: true,
                mvcRequestContext: mockRequestContext
            );

            Assert.That(response.Type, Is.EqualTo(ResponseType.OK));
        }

        #region Mocks

        class MvcRequestContextMock : MvcRequestContext
        {
            public override StringBuilder RenderRazorViewToString(string viewFilePath, object model)
            {
                var path = GetFileSystemTempPath(viewFilePath);
                Assert.That(File.Exists(path));
                
                return new StringBuilder("some markup");
            }
        }

        class MvcApplicationContextMock : MvcApplicationContext
        {
            public override string GetFileSystemPath(string virtualPath)
            {
                return GetFileSystemTempPath(virtualPath);
            }
        }

        static string GetFileSystemTempPath(string virtualPath)
        {
            var result = new StringBuilder(virtualPath);
            result.Replace("~/", Path.GetTempPath());
            result.Replace("/", "\\");

            return result.ToString();
        }

        #endregion
    }


}