using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
            StatusChanged += () => { };
        }

        public PageData     Data { get { return _data.Clone(); } }

        public FileInfo     MarkupFile
        {
            get
            {
                if (Data.Status == ContentStatus.Deleted) return null;
                return new FileInfo(_mvcApplicationContext.GetFileSystemPath((_data.VirtualPath)));
            }
        }

        public event Action StatusChanged;

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

        public void     Publish()
        {
            if (_data.Status == ContentStatus.Published) throw new ApplicationException("Unable to publish already published Page");
            if (_data.Status == ContentStatus.Deleted) throw new ApplicationException("Unable to publish deleted page");

            Delete();
            Create(new CreatePageData { Markup = _data.Markup, Name = _data.Name, RoutePattern = _data.RoutePattern }, ContentStatus.Published);
            StatusChanged();
        }

        public void Delete()
        {
            var viewFilePath = _mvcApplicationContext.GetFileSystemPath(_data.VirtualPath);
            var fileInfo = new FileInfo(viewFilePath);
            fileInfo.Delete();

            _viewDataRepository.Pages.RemoveAll(x => x.Id == _data.Id);
            _viewDataRepository.SaveChanges();

            _data.Status = ContentStatus.Deleted;
            StatusChanged();
        } 

        public void Update(UpdatePageData updatePageData)
        {
            Update(from: updatePageData, to: _data);

            var pageData = _viewDataRepository.Pages.Single(x => x.Id == _data.Id);
            Update(from: _data, to: pageData);
            _viewDataRepository.SaveChanges();
        }

        static void Update(PageData from, PageData to)
        {
            from.Markup = to.Markup;
            from.Name = to.Name;
            from.RoutePattern = to.RoutePattern;
        }
        static void Update(UpdatePageData from, PageData to)
        {
            from.Markup = to.Markup;
            from.Name = to.Name;
            from.RoutePattern = to.RoutePattern;
        }
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

    public enum ContentStatus { Draft, Published, Deleted }
}