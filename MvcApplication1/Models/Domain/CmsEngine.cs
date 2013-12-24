using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcApplication1.Models.Domain;

namespace MvcApplication1.Models
{
    public class CmsEngine
    {
        private readonly UrlRouter<Page> _releaseUrlRouter;
        private readonly UrlRouter<Page> _draftUrlRouter;

        public const string PreviewDraftCookieName = "Preview";

        public CmsEngine()
        {
            _releaseUrlRouter = new UrlRouter<Page>();
            _draftUrlRouter = new UrlRouter<Page>();
        }

        public IEnumerable<Language>    GetLanguages()
        {
            throw new NotImplementedException();
        }

        public Page                     CreatePage(string name, Language language, string url, string markup)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<Page>        GetPages()
        {
            throw new NotImplementedException();
        }
        public IEnumerable<PageData>    GetPagesData()
        {
            throw new NotImplementedException();
        }
        public void                     DeleteAllPages() {}

        public Response ProcessRequest(string url, MvcRequestContext mvcRequestContext)
        {
            return ProcessRequest(new Uri(url), mvcRequestContext);
        }
        public Response ProcessRequest(Uri url, MvcRequestContext mvcRequestContext)
        {
            var isViewingDraft = mvcRequestContext.HasCookie(PreviewDraftCookieName);
            var urlRouter = isViewingDraft ? _draftUrlRouter : _releaseUrlRouter;

            var match = urlRouter.MatchOrNull(url);
            if (match == null) return new Response { Type = ResponseType.PageNotFound };

            var page = match.Routable;
            var pageData = page.GetData();

            var pageHtmlBuilder = mvcRequestContext.RenderRazorViewToString(viewFilePath: pageData.Filepath, model: new object());
            if (pageHtmlBuilder == null)
                return new Response { Type = ResponseType.PageNotFound };

            return new Response { Body = pageHtmlBuilder.ToString(), Type = ResponseType.OK };
        }
    }

    public enum ResponseType { OK, Redirect, PageNotFound, Error }

    public struct Response
    {
        public ResponseType Type { get; set; }
        public string Body;
    }
}