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
            var mockRequestContext = new Mock<MvcRequestContext>();
            mockRequestContext.Setup(x => x.RenderRazorViewToString(It.Is<string>(p => File.Exists(p)), It.IsAny<object>()))
                .Returns(new StringBuilder("some markup"))
                .Verifiable();
            _container.RegisterInstance(mockRequestContext.Object);

            var mockApplicationContext = new Mock<MvcApplicationContext>();
            mockApplicationContext.Setup(x => x.GetFileSystemPath(It.IsAny<string>()))
                .Returns(Path.GetTempPath());
            _container.RegisterInstance(mockApplicationContext.Object);

            _cmsEngine = _container.Resolve<CmsEngine>();

            var markup = string.Format("<html><body>{0}</body></html>", RandomHelper.GetRandomString());
            var url = string.Format("http://test.com/en-gb/{0}", RandomHelper.GetRandomString());

            _cmsEngine.CreateView(markup, url, ViewStatus.Draft);

            var response = _cmsEngine.ProcessRequest(
                url: url, 
                showDrafts: true, 
                mvcRequestContext: mockRequestContext.Object
            );

            Assert.That(response.Type, Is.EqualTo(ResponseType.OK));
            Assert.DoesNotThrow(mockRequestContext.Verify);
        }
    }
}