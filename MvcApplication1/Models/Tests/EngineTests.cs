using System;
using System.Collections.Generic;
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
    [TestFixture]
    public class EngineTests
    {
        private readonly UnityContainer _container;
        private CmsEngine               _cmsEngine;

        public EngineTests()
        {
            _container = new UnityContainer();
        }

        [Test]
        public void CanCreateAndPreviewPage()
        {
            var page = CreateTestPage();
            var pageData = page.GetData();

            var mock = new Mock<MvcRequestContext>();
            mock.Setup(x => x.HasCookie(It.Is<string>(p => p == CmsEngine.PreviewDraftCookieName)))
                .Returns(true)
                .Verifiable();
            mock.Setup(x => x.RenderRazorViewToString(It.Is<string>(p => p == pageData.Filepath), It.IsAny<object>()))
                .Returns(new StringBuilder("some markup"))
                .Verifiable();
            _container.RegisterInstance(mock.Object);
            _cmsEngine = _container.Resolve<CmsEngine>();

            var response = _cmsEngine.ProcessRequest(pageData.Url, mock.Object);

            Assert.That(response.Type, Is.EqualTo(ResponseType.OK));
            Assert.DoesNotThrow(mock.Verify);
        }

        Page CreateTestPage()
        {
            var name = RandomHelper.GetRandomString();
            var markup = string.Format("<html><body>{0}</body></html>", RandomHelper.GetRandomString());
            var language = _cmsEngine.GetLanguages().First();
            var url = string.Format("http://test.com/{0}", RandomHelper.GetRandomString());

            return _cmsEngine.CreatePage(name, language, url, markup);
        }
    }

    public class MockMvcRequestContext : MvcRequestContext
    {
        public MockMvcRequestContext() : base(null, null, null) { }

        public string   RenderResult { get; set; }
        public bool     HasCookieResult { get; set; }

        public override bool HasCookie(string previewCookieName)
        {
            return HasCookieResult;
        }
        public override System.Text.StringBuilder RenderRazorViewToString(string viewFilePath, object model)
        {
            return new StringBuilder(RenderResult);
        }
    }
}