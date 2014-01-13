using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using MvcApplication1.Models;
using MvcApplication1.Models.Domain;
using WebGrease.Css.Extensions;

namespace FCG.RegoCms
{
    public class CmsFrontendService
    {
        private readonly MvcApplicationContext  _mvcApplicationContext;
        private readonly CmsService             _cmsService;
        private readonly UrlRouter<Page>        _releaseUrlRouter;
        private readonly UrlRouter<Page>        _draftUrlRouter;
        
        public const string AdminFormsCookieName = "CmsAdminFormsAuth";
        public const string ShowDraftsCookieName = "CmsShowDrafts";

        public CmsFrontendService(MvcApplicationContext mvcApplicationContext, CmsService cmsService)
        {
            _mvcApplicationContext = mvcApplicationContext;
            _cmsService = cmsService;
            _releaseUrlRouter = new UrlRouter<Page>();
            _draftUrlRouter = new UrlRouter<Page>();
        }

        public void UpdateContentFiles()
        {
            var viewsDirectory = new DirectoryInfo(_mvcApplicationContext.GetFileSystemPath("~/Views"));

            //update page content files
            var draftsDirectory = viewsDirectory.CreateSubdirectory("Draft");
            ClearDirectory(draftsDirectory);
            var pages = _cmsService.GetPages(excludeDeleted: true);
            _draftUrlRouter.UnregisterAll();
            foreach (var page in pages)
            {
                var lastVersionData = page.LastData;
                var viewPath = draftsDirectory.FullName + "\\" + lastVersionData.ViewPath;
                CreateViewFile(lastVersionData.Markup, viewPath);
                _draftUrlRouter.RegisterRoute(lastVersionData.Route, page);
            }

            var publishedDirectory = viewsDirectory.CreateSubdirectory("Published");
            ClearDirectory(publishedDirectory);
            _releaseUrlRouter.UnregisterAll();
            foreach (var page in pages.Where(x => x.PublishedData != null))
            {
                var publishedVersionData = page.PublishedData;
                var viewPath = publishedDirectory.FullName + "\\" + publishedVersionData.ViewPath;
                CreateViewFile(publishedVersionData.Markup, viewPath);
                _releaseUrlRouter.RegisterRoute(publishedVersionData.Route, page);
            }
        }

        static void ClearDirectory(DirectoryInfo directory)
        {
            Contract.Requires(directory != null);

            directory.EnumerateFileSystemInfos().ForEach(DeleteFileSystemInfo);
        }

        static void DeleteFileSystemInfo(FileSystemInfo fsi)
        {
            Contract.Requires(fsi != null);

            fsi.Attributes = FileAttributes.Normal;
            var di = fsi as DirectoryInfo;

            if (di != null) // it's a directory  
                foreach (var childFsi in di.GetFileSystemInfos())
                    DeleteFileSystemInfo(childFsi);

            fsi.Delete();
        }

        static void CreateViewFile(string markup, string viewFilePath)
        {
            var fileInfo = new FileInfo(viewFilePath);

            Directory.CreateDirectory(fileInfo.Directory.FullName);

            using (var writer = new StreamWriter(fileInfo.FullName))
            {
                // It seems all view files created in Visual Studio will have BOM. So, we need it here too.
                writer.Write('\xfeff');
                writer.Write(markup);
            }
        }

        public Response ProcessRequest(string url)
        {
            Contract.Requires(url != null);

            return ProcessRequest(new Uri(url));
        }
        public Response ProcessRequest(Uri url)
        {
            Contract.Requires(url != null);

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

            var pageContentItemVersion = showDrafts ? page.LastVersion : page.PublishedVersion;
            var markupVirtualPath = GetViewVirtualPath(pageContentItemVersion);
            var pageHtmlBuilder = mvcRequestContext.RenderPageContentItemVersion(markupVirtualPath, model: new object());

            return new Response { Body = pageHtmlBuilder.ToString(), Type = ResponseType.OK };
        }

        private string GetViewVirtualPath(ContentItemVersion<PageData> pageContentItemVersion)
        {
            Contract.Requires(pageContentItemVersion != null);

            var versionFolderName = pageContentItemVersion.Type == ContentVersionType.Draft ? "Draft/" : "Published/";
            return "~/Views/" + versionFolderName + pageContentItemVersion.Content.ViewPath;
        }

        bool    CheckIfRequestUserIsAdmin(MvcRequestContext mvcRequestContext)
        {
            Contract.Requires(mvcRequestContext != null);

            if (!mvcRequestContext.HasCookie(AdminFormsCookieName)) return false;

            var cookieValue = mvcRequestContext.GetCookieValue(AdminFormsCookieName);

            return _mvcApplicationContext.IsFormsCookieValueValid(cookieValue);
        }
        bool    CheckIfHasToShowDrafts(MvcRequestContext mvcRequestContext)
        {
            Contract.Requires(mvcRequestContext != null);

            if (!mvcRequestContext.HasCookie(ShowDraftsCookieName)) return false;
            var showDraftsCookieValue = mvcRequestContext.GetCookieValue(ShowDraftsCookieName);
            return bool.Parse(showDraftsCookieValue);
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