using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using MvcApplication1.Models.Domain;
using NUnit.Framework;

namespace MvcApplication1.Models
{
    public class Page
    {
        private readonly Guid _id;
        private readonly MvcApplicationContext  _mvcApplicationContext;
        private readonly ContentRepository      _viewDataRepository;
        private readonly InMemoryContentManager         _contentManager;

        public Action PageChanged;

        public Page(CreatePageData createPageData,  MvcApplicationContext mvcApplicationContext, ContentRepository viewDataRepository, InMemoryContentManager contentManager)
            : this(mvcApplicationContext, viewDataRepository, contentManager)
        {
            _id = Guid.NewGuid();
            Create(createPageData);
        }
        
        internal Page(PageData page, MvcApplicationContext mvcApplicationContext, ContentRepository viewDataRepository, InMemoryContentManager contentManager)
            : this(mvcApplicationContext, viewDataRepository, contentManager)
        {
            throw new NotImplementedException();
        }

        private Page(MvcApplicationContext mvcApplicationContext, ContentRepository viewDataRepository, InMemoryContentManager contentManager)
        {
            _mvcApplicationContext = mvcApplicationContext;
            _viewDataRepository = viewDataRepository;
            _contentManager = contentManager;
        }

        public Guid                 Id { get { return _id; } }
        public PageData             DataDraft { get { return _contentManager.GetContentItem<PageData>(_id).ContentDraft; } }
        public PageData             DataPublished { get { return _contentManager.GetContentItem<PageData>(_id).ContentPublished; } }
        public ContentItemStatus    Status { get { return _contentManager.GetContentItem<PageData>(_id).Status; } }

        void Create(CreatePageData createPageData)
        {
            Uri uri;
            var isValidRoutePattern = Uri.TryCreate(createPageData.RoutePattern, UriKind.Absolute, out uri);
            if(!isValidRoutePattern) throw new ApplicationException("Route pattern is not valid, make sure its absolute and not relative"); //relative patterns are not supported for now

            var data = new PageData
            {
                Id = _id,
                Markup = createPageData.Markup,
                Name = createPageData.Name,
                RoutePattern = createPageData.RoutePattern,
                ViewPath = string.Format("{0}.cshtml", _id)
            };

            _contentManager.Create<PageData>(_id, data);
        }

        public void     Publish()
        {
            _contentManager.Publish(_id);
            PageChanged();
        }

        public void Delete()
        {
            _contentManager.Delete(_id);
            PageChanged();
        } 

        public void Update(UpdatePageData updatePageData)
        {
            var contentItem = _contentManager.GetContentItem<PageData>(_id);
            var contentToUpdate = contentItem.ContentDraft ?? contentItem.ContentPublished;
            var clonedContentToUpdate = contentToUpdate.Clone(); //make sure that published version will not be changed by a chance

            _contentManager.Update<PageData>(_id, new PageData
            {
                Id = _id,
                Markup = updatePageData.Markup, 
                Name = updatePageData.Name, 
                RoutePattern = updatePageData.RoutePattern, 
                ViewPath = clonedContentToUpdate.ViewPath
            });
            PageChanged();
        }
    }

    public class PageFactory
    {
        private readonly MvcApplicationContext  _mvcApplicationContext;
        private readonly ContentRepository      _viewDataRepository;
        private readonly InMemoryContentManager         _contentManager;

        public PageFactory(MvcApplicationContext mvcApplicationContext, ContentRepository viewDataRepository, InMemoryContentManager contentManager)
        {
            _mvcApplicationContext = mvcApplicationContext;
            _viewDataRepository = viewDataRepository;
            _contentManager = contentManager;
        }

        public Page Create(CreatePageData createPageData)
        {
            return new Page(createPageData, _mvcApplicationContext, _viewDataRepository, _contentManager);
        }
    }

    public enum ContentStatus { Draft, Published, Deleted }
}