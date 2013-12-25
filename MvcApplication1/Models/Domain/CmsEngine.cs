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
        private readonly ViewDataRepository _viewDataRepository;
        private readonly UrlRouter<View> _releaseUrlRouter;
        private readonly UrlRouter<View> _draftUrlRouter;

        public const string PreviewDraftCookieName = "Preview";

        public CmsEngine(MvcApplicationContext mvcApplicationContext, 
                         ViewDataRepository viewDataRepository)
        {
            _mvcApplicationContext = mvcApplicationContext;
            _viewDataRepository = viewDataRepository;
            _releaseUrlRouter = new UrlRouter<View>();
            _draftUrlRouter = new UrlRouter<View>();
        }

        public IEnumerable<Language>    GetLanguages()
        {
            return new[] { new Language { Code = "en-gb", Name = "English" } };
        }

        public View     CreateView(string markup, string routePattern, ViewStatus status)
        {
            const string viewsPath = "~/Views";

            var draftsPath = Path.Combine(viewsPath, "/Draft");
            var releasePath = Path.Combine(viewsPath, "/Release");

            var routePath = GetPathFromRoutePattern(routePattern);
            
            var basePath = status == ViewStatus.Draft ? draftsPath : releasePath;

            var viewVirtualPath = viewsPath + basePath + routePath + ".cshtml";
            
            var viewFilePath = _mvcApplicationContext.GetFileSystemPath(viewVirtualPath);
            var fileInfo = new FileInfo(viewFilePath);

            Directory.CreateDirectory(fileInfo.Directory.FullName);

            using (var writer = new StreamWriter(fileInfo.FullName))
            {
                // It seems all view files created in Visual Studio will have BOM. So, we need it here too.
                writer.Write('\xfeff');
                writer.Write(markup);
            }
            
            var view = new View(id: Guid.NewGuid(), virtualPath: viewVirtualPath, status: status, routePattern: routePattern);
            _viewDataRepository.Views.Add(view.Data);

            var urlRouter = status == ViewStatus.Draft ? _draftUrlRouter : _releaseUrlRouter;
            urlRouter.RegisterRoute(routePattern, view);
            return view;
        }

        private string GetPathFromRoutePattern(string routePattern)
        {
            Uri result;
            var routePatternIsValidUri = Uri.TryCreate(routePattern, UriKind.Absolute, out result);
            if (!routePatternIsValidUri) 
                throw new NotImplementedException("Route pattern is not valid Uri: scenario not implemented yet");

            return result.AbsolutePath;
        }

        public View     CreateView(CreateViewData data)
        {
            return CreateView(data.Markup, data.Url, data.Status);
        }
        public View[] CreateViews(CreateViewData[] data)
        {
            return data.Select(CreateView).ToArray();
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
                return new Response { Type = ResponseType.PageNotFound };

            return new Response { Body = pageHtmlBuilder.ToString(), Type = ResponseType.OK };
        }
    }

    public enum ResponseType { OK, Redirect, PageNotFound, Error }
    

    public struct Response
    {
        public ResponseType Type { get; set; }
        public string       Body;
    }


    public struct CreateViewData
    {
        public string       Url;
        public string       Markup;
        public ViewStatus   Status;
    }
}