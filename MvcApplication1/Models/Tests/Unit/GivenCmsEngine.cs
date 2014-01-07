using System;
using System.IO;
using System.Text;
using Microsoft.Practices.Unity;
using MvcApplication1.Models;
using MvcApplication1.Models.Domain;
using MvcApplication1.Models.Infrastructure;
using NUnit.Framework;

namespace MvcApplication1.Tests
{
    public class GivenCmsEngine : GivenCmsEngineContext
    {
        [Test]
        public void NewPageCanBeCreated()
        {
            var page = CreateTestPage();

            Assert.That(page, Is.Not.Null);
        }

        [Test]
        public void QueryingNotExistingPageResultsInNotFoundResponse()
        {
            var notExistingPageUrl = string.Format("http://test.com/en-gb/{0}", RandomHelper.GetRandomString());

            var response = _cmsEngine.ProcessRequest(notExistingPageUrl);

            Assert.That(response.Type, Is.EqualTo(ResponseType.PageNotFound));
        }
    }

    public abstract class GivenCmsEngineContext : BddUnitTestBase
    {
        protected CmsEngine             _cmsEngine;
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
            _container.RegisterType<InMemoryContentManager>(new SingletonLifetimeManager());

            _cmsEngine = _container.Resolve<CmsEngine>();

            _cmsEngine.ContentUpdated += () => _cmsEngine.UpdateContentFiles(); //simulating content updater from admin side
        }

        protected Page CreateTestPage()
        {
            var markup = string.Format("<html><body>{0}</body></html>", RandomHelper.GetRandomString());
            var routePattern = string.Format("http://test.com/en-gb/{0}", RandomHelper.GetRandomString());
            var name = "testPage-" + RandomHelper.GetRandomString();

            return _cmsEngine.CreatePage(name, routePattern, markup);
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
                if (cookieName == CmsEngine.ShowDraftsCookieName)
                    return HasDraftCookie.ToString();

                if (cookieName == CmsEngine.AdminFormsCookieName)
                    return "valid cookie for admin";

                throw new ApplicationException("this mock was intended for querying draft and admin cookies only");
            }

            public override bool HasCookie(string cookieName)
            {
                if (cookieName == CmsEngine.ShowDraftsCookieName)
                    return HasDraftCookie;

                if (cookieName == CmsEngine.AdminFormsCookieName)
                    return HasAdminCookie;

                return false;
            }

            public string RenderPageToString(Page page, ContentVersionType versionType)
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