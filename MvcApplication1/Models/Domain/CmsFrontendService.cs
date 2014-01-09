using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Instrumentation;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using MvcApplication1.Models.Domain;
using WebGrease.Css.Extensions;

namespace MvcApplication1.Models
{
    public class CmsFrontendService
    {
        private readonly MvcApplicationContext  _mvcApplicationContext;
        private readonly UrlRouter<PageData>    _releaseUrlRouter;
        private readonly UrlRouter<PageData>    _draftUrlRouter;
        private readonly InMemoryContentManager _contentManager;
        
        public const string AdminFormsCookieName = "CmsAdminFormsAuth";
        public const string ShowDraftsCookieName = "CmsShowDrafts";

        public CmsFrontendService(MvcApplicationContext mvcApplicationContext, 
                                  InMemoryContentManager contentManager)
        {
            _mvcApplicationContext = mvcApplicationContext;
            _contentManager = contentManager;
            _releaseUrlRouter = new UrlRouter<PageData>();
            _draftUrlRouter = new UrlRouter<PageData>();
        }

        public void UpdateContentFiles()
        {
            var viewsDirectory = new DirectoryInfo(_mvcApplicationContext.GetFileSystemPath("~/Views"));
            var draftsDirectory = viewsDirectory.CreateSubdirectory("Draft");
            var publishedDirectory = viewsDirectory.CreateSubdirectory("Published");


            ClearDirectory(draftsDirectory);
            var draftContentItems = _contentManager.GetVersions<PageData>(ContentVersionType.Draft);
            _draftUrlRouter.UnregisterAll();
            foreach (var draftPageDataItem in draftContentItems)
            {
                var viewPath = draftsDirectory.FullName + "\\" + draftPageDataItem.Content.ViewPath;
                CreateViewFile(draftPageDataItem.Content.Markup, viewPath);
                _draftUrlRouter.RegisterRoute(draftPageDataItem.Content.RoutePattern, draftPageDataItem.Content);
            }


            ClearDirectory(publishedDirectory);
            var publishedPageDataItems = _contentManager.GetVersions<PageData>(ContentVersionType.Published);
            _releaseUrlRouter.UnregisterAll();
            foreach (var publishedPageDataItem in publishedPageDataItems)
            {
                var viewPath = publishedDirectory.FullName + "\\" + publishedPageDataItem.Content.ViewPath;
                CreateViewFile(publishedPageDataItem.Content.Markup, viewPath);
                _releaseUrlRouter.RegisterRoute(publishedPageDataItem.Content.RoutePattern, publishedPageDataItem.Content);
            }
        }

        static void ClearDirectory(DirectoryInfo directory)
        {
            directory.EnumerateFileSystemInfos().ForEach(DeleteFileSystemInfo);
        }

        static void DeleteFileSystemInfo(FileSystemInfo fsi)
        {
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

            var pageData = match.Routable;

            var contentVersionType = showDrafts ? ContentVersionType.Draft : ContentVersionType.Published;
            var pageContentItemVersion = _contentManager.GetVersionOrNull<PageData>(pageData.Id, contentVersionType);

            var pageHtmlBuilder = mvcRequestContext.RenderPageContentItemVersion(pageContentItemVersion: pageContentItemVersion, model: new object());

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
    }

    public enum ResponseType { OK, Redirect, PageNotFound, Error, Unauthorized }
    
    public struct Response
    {
        public ResponseType Type { get; set; }
        public string       Description { get; set; }
        public string       Body { get; set; }
    }

}