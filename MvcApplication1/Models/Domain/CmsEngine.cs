using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
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

        public const string PreviewDraftCookieName = "Preview";

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
            return CreatePage(new CreatePageData { Markup = markup, RoutePattern = routePattern });
        }

        private Page    CreatePage(CreatePageData createPageData)
        {
            var page = _pageFactory.Create(createPageData);

            var urlRouter = page.Data.Status == ContentStatus.Draft ? _draftUrlRouter : _releaseUrlRouter;
            urlRouter.RegisterRoute(createPageData.RoutePattern, page);

            return page;
        }

        public Response ProcessRequest(string url, bool showDrafts, MvcRequestContext mvcRequestContext)
        {
            return ProcessRequest(new Uri(url), showDrafts, mvcRequestContext);
        }
        public Response ProcessRequest(Uri url, bool showDrafts, MvcRequestContext mvcRequestContext)
        {
            var urlRouter = showDrafts ? _draftUrlRouter : _releaseUrlRouter;

            var match = urlRouter.MatchOrNull(url);
            if (match == null) return new Response { Type = ResponseType.PageNotFound };

            var page = match.Routable;

            var pageHtmlBuilder = mvcRequestContext.RenderRazorViewToString(viewFilePath: page.Data.VirtualPath, model: new object());
            if (pageHtmlBuilder == null)
                throw new ApplicationException(string.Format("Page was not found by the path specified: '{0}'", page.Data.VirtualPath));

            return new Response { Body = pageHtmlBuilder.ToString(), Type = ResponseType.OK };
        }
    }

    public enum ResponseType { OK, Redirect, PageNotFound, Error }
    

    public struct Response
    {
        public ResponseType Type { get; set; }
        public string       Body;
    }

}