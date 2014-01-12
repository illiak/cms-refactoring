using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FCG.RegoCms;
using Microsoft.Practices.Unity;
using MvcApplication1.Models;
using MvcApplication1.Models.Domain;
using MvcApplication1.Models.Infrastructure;
using NUnit.Framework;

namespace MvcApplication1.Tests
{
    public class GivenCmsServices : GivenCmsServicesContext
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

            Assert.Throws<ApplicationException>(() => _cmsBackendService.CreatePage(string.Empty, route, markup));
            Assert.Throws<ApplicationException>(() => _cmsBackendService.CreatePage("/test", route, markup));
            Assert.Throws<ApplicationException>(() => _cmsBackendService.CreatePage("test/", route, markup));
            Assert.Throws<ApplicationException>(() => _cmsBackendService.CreatePage("test1//test2", route, markup));
            Assert.Throws<ApplicationException>(() => _cmsBackendService.CreatePage("test1///test2", route, markup));
            Assert.Throws<ApplicationException>(() => _cmsBackendService.CreatePage("test1 test2", route, markup));
            Assert.Throws<ApplicationException>(() => _cmsBackendService.CreatePage("test1%test2", route, markup)); //some weird invalid character
        }

        [Test]
        public void QueryingNotExistingPageResultsInNotFoundResponse()
        {
            var notExistingPageUrl = string.Format("http://test.com/en-gb/{0}", RandomHelper.GetRandomString());

            var response = _cmsFrontendService.ProcessRequest(notExistingPageUrl);

            Assert.That(response.Type, Is.EqualTo(ResponseType.PageNotFound));
        }
    }

    public abstract class GivenCmsServicesContext : BddUnitTestBase
    {
        protected CmsFrontendService    _cmsFrontendService;
        protected CmsBackendService     _cmsBackendService;
        protected MvcRequestContextMock _mvcRequestContextMock;

        protected override void Given()
        {
            _mvcRequestContextMock = new MvcRequestContextMock();
            _container.RegisterInstance<MvcRequestContext>(_mvcRequestContextMock);
            _container.RegisterInstance<MvcApplicationContext>(new MvcApplicationContextMock 
            {
                FormsCookieIsAlwaysValid = true, //faking forms authentication
                MvcRequestContext = _mvcRequestContextMock
            });
            _container.RegisterType<ContentService>(new SingletonLifetimeManager());

            _cmsFrontendService = _container.Resolve<CmsFrontendService>();
            _cmsBackendService = _container.Resolve<CmsBackendService>();
            _cmsBackendService.ContentChanged += () => _cmsFrontendService.UpdateContentFiles(); //simulating content updater from admin side
        }

        protected Page CreateTestPage()
        {
            var markup = string.Format("<html><body>{0}</body></html>", RandomHelper.GetRandomString());
            var routePattern = string.Format("http://test.com/en-gb/{0}", RandomHelper.GetRandomString());
            var name = "testPage-" + RandomHelper.GetRandomString();

            return _cmsBackendService.CreatePage(name, routePattern, markup);
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

        protected class MvcRequestContextMock : MvcRequestContext
        {
            public bool HasDraftCookie;
            public bool HasAdminCookie;

            public override StringBuilder RenderPageContentItemVersion(ContentItemVersion<PageData> pageContentItemVersion, object model = null)
            {
                return new StringBuilder(string.Format("<rendered>{0}</rendered>", pageContentItemVersion.Content.Markup));
            }

            public override string GetCookieValue(string cookieName)
            {
                if (cookieName == CmsFrontendService.ShowDraftsCookieName)
                    return HasDraftCookie.ToString();

                if (cookieName == CmsFrontendService.AdminFormsCookieName)
                    return "valid cookie for admin";

                throw new ApplicationException("this mock was intended for querying draft and admin cookies only");
            }

            public override bool HasCookie(string cookieName)
            {
                if (cookieName == CmsFrontendService.ShowDraftsCookieName)
                    return HasDraftCookie;

                if (cookieName == CmsFrontendService.AdminFormsCookieName)
                    return HasAdminCookie;

                return false;
            }

            public string RenderPageToString(PageData page, ContentVersionType versionType)
            {
                var pageMarkup = versionType == ContentVersionType.Draft ? page.DataDraft.Markup : page.DataPublished.Markup;
                return new StringBuilder(string.Format("<rendered>{0}</rendered>", pageMarkup)).ToString();
            }
        }

        protected class MvcApplicationContextMock : MvcApplicationContext
        {
            public bool FormsCookieIsAlwaysValid;
            public MvcRequestContext MvcRequestContext;

            public override string GetFileSystemPath(string virtualPath)
            {
                return GetFileSystemTempPath(virtualPath);
            }

            public override bool IsFormsCookieValueValid(string cookieValue)
            {
                if (FormsCookieIsAlwaysValid) return true;

                return base.IsFormsCookieValueValid(cookieValue);
            }

            public override MvcRequestContext GetCurrentMvcRequestContext()
            {
                return MvcRequestContext;
            }
        }

        protected static string GetFileSystemTempPath(string virtualPath)
        {
            var result = new StringBuilder(virtualPath);
            result.Replace("~/", Path.GetTempPath());
            result.Replace("/", "\\");

            return result.ToString();
        }

        #endregion
    }
}