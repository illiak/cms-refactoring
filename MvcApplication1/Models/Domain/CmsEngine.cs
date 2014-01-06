using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using MvcApplication1.Models.Domain;
using WebGrease.Css.Extensions;

namespace MvcApplication1.Models
{
    public class CmsEngine
    {
        private readonly MvcApplicationContext _mvcApplicationContext;
        private readonly ContentRepository _viewDataRepository;
        private readonly PageFactory _pageFactory;
        private readonly UrlRouter<Page> _releaseUrlRouter;
        private readonly UrlRouter<Page> _draftUrlRouter;

        public const string AdminFormsCookieName = "CmsAdminFormsAuth";
        public const string ShowDraftsCookieName = "CmsShowDrafts";

        public CmsEngine(MvcApplicationContext mvcApplicationContext, 
                         ContentRepository viewDataRepository,
                         PageFactory pageFactory)
        {
            _mvcApplicationContext = mvcApplicationContext;
            _viewDataRepository = viewDataRepository;
            _pageFactory = pageFactory;
            _releaseUrlRouter = new UrlRouter<Page>();
            _draftUrlRouter = new UrlRouter<Page>();
        }

        public IEnumerable<Language>    GetLanguages()
        {
            return new[] { new Language { Code = "en-gb", Name = "English" } };
        }

        public Page[]   CreatePages(CreatePageData[] data)
        {
            return data.Select(CreatePage).ToArray();
        }
        public Page     CreatePage(string name, string routePattern, string markup)
        {
            return CreatePage(new CreatePageData {Name = name, Markup = markup, RoutePattern = routePattern });
        }

        private Page    CreatePage(CreatePageData createPageData)
        {
            var page = _pageFactory.Create(createPageData);
            
            var urlRouter = page.Data.Status == ContentStatus.Draft ? _draftUrlRouter : _releaseUrlRouter;
            urlRouter.RegisterRoute(createPageData.RoutePattern, page);

            page.StatusChanged += () =>
            {
                switch (page.Data.Status)
                {
                    case ContentStatus.Draft:
                        _releaseUrlRouter.RemoveRoutableIfRegistered(page);
                        _draftUrlRouter.RegisterRouteIfNotRegistered(page.Data.RoutePattern, page);
                        break;
                    case ContentStatus.Published:
                        _draftUrlRouter.RemoveRoutableIfRegistered(page);
                        _releaseUrlRouter.RegisterRouteIfNotRegistered(page.Data.RoutePattern, page);
                        break;
                    case ContentStatus.Deleted:
                        _releaseUrlRouter.RemoveRoutableIfRegistered(page);
                        _draftUrlRouter.RemoveRoutableIfRegistered(page);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            return page;
        }

        public Response ProcessRequest(string url)
        {
            return ProcessRequest(new Uri(url));
        }
        public Response ProcessRequest(Uri url)
        {
            var mvcRequestContext = _mvcApplicationContext.GetCurrentMvcRequestContext();

            var showDrafts = CheckIfHasToShowDrafts(mvcRequestContext);
            if (showDrafts)
            {
                var isAdmin = CheckIfRequestUserIsAdmin(mvcRequestContext);
                if (!isAdmin) return new Response { Type = ResponseType.Unauthorized, Description = "Only admins can view drafts"};
            }

            var urlRouter = showDrafts ? _draftUrlRouter : _releaseUrlRouter;

            var match = urlRouter.MatchOrNull(url);
            if (match == null) return new Response { Type = ResponseType.PageNotFound };

            var page = match.Routable;

            var pageHtmlBuilder = mvcRequestContext.RenderPage(page: page, model: new object());

            return new Response { Body = pageHtmlBuilder.ToString(), Type = ResponseType.OK };
        }

        bool    CheckIfRequestUserIsAdmin(MvcRequestContext mvcRequestContext)
        {
            if (!mvcRequestContext.HasCookie(AdminFormsCookieName)) return false;

            var cookieValue = mvcRequestContext.GetCookieValue(AdminFormsCookieName);

            return _mvcApplicationContext.IsFormsCookieValueValid(cookieValue);
        }
        bool    CheckIfHasToShowDrafts(MvcRequestContext mvcRequestContext)
        {
            if (!mvcRequestContext.HasCookie(ShowDraftsCookieName)) return false;
            var showDraftsCookieValue = mvcRequestContext.GetCookieValue(ShowDraftsCookieName);
            return bool.Parse(showDraftsCookieValue);
        }

        public Page GetPage(Guid id, ContentStatus contentStatus)
        {
            throw new NotImplementedException();
        }
    }

    public enum ResponseType { OK, Redirect, PageNotFound, Error, Unauthorized }
    
    public struct Response
    {
        public ResponseType Type { get; set; }
        public string       Description { get; set; }
        public string       Body { get; set; }
    }

}