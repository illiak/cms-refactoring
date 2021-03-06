using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FCG.RegoCms;
using FCG.RegoCms.Tests.Mocks;
using Microsoft.Practices.Unity;
using MvcApplication1.Models;
using MvcApplication1.Models.Domain;
using MvcApplication1.Models.Infrastructure;
using NUnit.Framework;

namespace FCG.RegoCms.Tests.Unit.CmsServicesTests
{
    class GivenCmsServices : GivenCmsServicesContext
    {
        [Test]
        public void NewPageCanBeCreated()
        {
            var page = CreateTestPage();

            Assert.That(page, Is.Not.Null);
        }

        [Test]
        public void CanCreateMultiplePages()
        {
            var pages = CreateMultipleTestPages();

            foreach (var page in pages) 
                Assert.That(page, Is.Not.Null);
        }

        [Test]
        public void UnableToCreatePageWithInvalidName()
        {
            var markup = GenerateValidMarkup();
            var route = GenerateValidRoute();

            Assert.Throws<ApplicationException>(() => _cmsService.CreatePage(string.Empty, route, markup));
            Assert.Throws<ApplicationException>(() => _cmsService.CreatePage("/test", route, markup));
            Assert.Throws<ApplicationException>(() => _cmsService.CreatePage("test/", route, markup));
            Assert.Throws<ApplicationException>(() => _cmsService.CreatePage("test1//test2", route, markup));
            Assert.Throws<ApplicationException>(() => _cmsService.CreatePage("test1///test2", route, markup));
            Assert.Throws<ApplicationException>(() => _cmsService.CreatePage("test1 test2", route, markup));
            Assert.Throws<ApplicationException>(() => _cmsService.CreatePage("test1%test2", route, markup)); //some weird invalid character
        }

        [Test]
        public void QueryingNotExistingPageResultsInNotFoundResponse()
        {
            var notExistingPageUrl = string.Format("http://test.com/en-gb/{0}", RandomHelper.GetRandomString());

            var response = _cmsFrontendService.ProcessRequest(notExistingPageUrl);

            Assert.That(response.Type, Is.EqualTo(ResponseType.PageNotFound));
        }

        [Test]
        public void CanGetLanguages()
        {
            var languages = _cmsService.GetLanguages();
            Assert.That(languages, Is.Not.Empty);
        }
    }

    abstract class GivenCmsServicesContext : BddUnitTestBase
    {
        protected CmsFrontendService    _cmsFrontendService;
        protected CmsService            _cmsService;
        protected FakeMvcRequestContext _mvcRequestContextMock;
        protected FakeContentRepository _fakeContentRepository;

        protected override void Given()
        {
            _mvcRequestContextMock = new FakeMvcRequestContext();
            _container.RegisterInstance<MvcRequestContext>(_mvcRequestContextMock);
            _container.RegisterInstance<MvcApplicationContext>(new FakeMvcApplicationContext 
            {
                FormsCookieIsAlwaysValid = true, //faking forms authentication
                MvcRequestContext = _mvcRequestContextMock
            });
            
            _fakeContentRepository = new FakeContentRepository();
            _container.RegisterInstance<ContentRepository>(_fakeContentRepository);
            
            _cmsFrontendService = _container.Resolve<CmsFrontendService>();
            _cmsService = _container.Resolve<CmsService>();
            
            _cmsService.ContentChanged += () => _cmsFrontendService.UpdateContentFiles(); //simulating content updater from admin side
        }

        protected Page CreateTestPage()
        {
            var markup = string.Format("<html><body>{0}</body></html>", RandomHelper.GetRandomString());
            var routePattern = string.Format("http://test.com/en-gb/{0}", RandomHelper.GetRandomString());
            var name = "testPage-" + RandomHelper.GetRandomString();

            return _cmsService.CreatePage(name, routePattern, markup);
        }

        protected IEnumerable<Page> CreateMultipleTestPages()
        {
            var result = new List<Page>();
            for (var i = 0; i < 10; i++)
                result.Add(CreateTestPage());
            return result;
        }

        protected string GenerateValidMarkup()
        {
            return string.Format("<html><body>{0}</body></html>", RandomHelper.GetRandomString());
        }

        protected string GenerateValidRoute()
        {
            return string.Format("http://test.com/en-gb/{0}", RandomHelper.GetRandomString());
        }

        #region Mocks

        

        #endregion
    }
}