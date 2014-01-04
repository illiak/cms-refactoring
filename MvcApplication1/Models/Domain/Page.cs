using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace MvcApplication1.Models
{
    public class Page
    {
        private PageData                        _data;
        private readonly MvcApplicationContext  _mvcApplicationContext;
        private readonly ContentRepository      _viewDataRepository;

        public Page(CreatePageData createPageData, MvcApplicationContext mvcApplicationContext, ContentRepository viewDataRepository)
            : this(createPageData, ContentStatus.Draft, mvcApplicationContext, viewDataRepository) { }

        protected Page(CreatePageData createPageData, ContentStatus contentStatus, MvcApplicationContext mvcApplicationContext, ContentRepository viewDataRepository) 
        {
            _mvcApplicationContext = mvcApplicationContext;
            _viewDataRepository = viewDataRepository;
            Create(createPageData, contentStatus);
        }

        public PageData Data { get { return _data; } }

        void Create(CreatePageData createPageData, ContentStatus contentStatus)
        {
            const string viewsPath = "~/Views";

            var draftsPath = Path.Combine(viewsPath, "/Draft");
            var releasePath = Path.Combine(viewsPath, "/Release");

            var routePath = GetPathFromRoutePattern(createPageData.RoutePattern);

            var basePath = contentStatus == ContentStatus.Draft ? draftsPath : releasePath;

            var viewVirtualPath = viewsPath + basePath + routePath + ".cshtml";

            var viewFilePath = _mvcApplicationContext.GetFileSystemPath(viewVirtualPath);
            var fileInfo = new FileInfo(viewFilePath);

            Directory.CreateDirectory(fileInfo.Directory.FullName);

            using (var writer = new StreamWriter(fileInfo.FullName))
            {
                // It seems all view files created in Visual Studio will have BOM. So, we need it here too.
                writer.Write('\xfeff');
                writer.Write(createPageData.Markup);
            }

            _data = new PageData
            {
                Id = Guid.NewGuid(),
                Name = createPageData.Name,
                VirtualPath = viewVirtualPath,
                Status = contentStatus,
                RoutePattern = createPageData.RoutePattern,
                Markup = createPageData.Markup
            };
            _viewDataRepository.Pages.Add(_data);
            _viewDataRepository.SaveChanges();
        }

        private string GetPathFromRoutePattern(string routePattern)
        {
            Uri result;
            var routePatternIsValidUri = Uri.TryCreate(routePattern, UriKind.Absolute, out result);
            if (!routePatternIsValidUri)
                throw new NotImplementedException("Route pattern is not valid Uri: scenario not implemented yet");

            return result.AbsolutePath;
        }

        public Page     Publish()
        {
            var publishedPageData = new CreatePageData { Markup = _data.Markup, Name = _data.Name, RoutePattern = _data.RoutePattern };
            return new Page(publishedPageData, ContentStatus.Release, _mvcApplicationContext, _viewDataRepository);
        }
        
        public void     Delete() { }
    }

    public class PageFactory
    {
        private readonly MvcApplicationContext  _mvcApplicationContext;
        private readonly ContentRepository      _viewDataRepository;

        public PageFactory(MvcApplicationContext mvcApplicationContext, ContentRepository viewDataRepository)
        {
            _mvcApplicationContext = mvcApplicationContext;
            _viewDataRepository = viewDataRepository;
        }

        public Page Create(CreatePageData createPageData)
        {
            return new Page(createPageData, _mvcApplicationContext, _viewDataRepository);
        }
    }

    public enum ContentStatus { Draft, Release }
}