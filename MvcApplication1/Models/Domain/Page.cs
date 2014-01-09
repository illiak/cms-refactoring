using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MvcApplication1.Models.Domain;
using NUnit.Framework;

namespace MvcApplication1.Models
{
    public class Page
    {
        private readonly Guid                   _id;
        private readonly InMemoryContentManager _contentManager;

        public Action PageChanged;

        public Page(CreatePageData createPageData,  InMemoryContentManager contentManager)
            : this(contentManager)
        {
            _id = Guid.NewGuid();
            Create(createPageData);
        }
        
        internal Page(Guid id, InMemoryContentManager contentManager)
            : this(contentManager)
        {
            _id = id;
        }

        private Page(InMemoryContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public Guid                 Id { get { return _id; } }
        public PageData             DataDraft { get { return _contentManager.GetContentItem<PageData>(_id).ContentDraft; } }
        public PageData             DataPublished { get { return _contentManager.GetContentItem<PageData>(_id).ContentPublished; } }
        public ContentItemStatus    Status { get { return _contentManager.GetContentItem<PageData>(_id).Status; } }

        public ContentItem<PageData>        ContentItem 
        {
            get { return _contentManager.GetContentItem<PageData>(_id); }
        }
        public ContentItemVersion<PageData> LastVersion 
        {
            get { return _contentManager.GetLastVersion<PageData>(_id); }
        }

        void Create(CreatePageData createPageData)
        {
            if (string.IsNullOrEmpty(createPageData.Name) || createPageData.Name.Length > 40)
                throw new ApplicationException("Please enter a page name. Must be between 1 and 40 characters long.");

            if (!Regex.IsMatch(createPageData.Name, "^[a-zA-Z0-9_-]+(/[a-zA-Z0-9_-]+)*$"))
                throw new ApplicationException("Please enter a valid name. Allowed symbols:<br/>-  alphanumeric, en dash(-),  slash (/),  and underscore (_)<br/>Not allowed:<br/>- blanks <br/>- single slash(/) symbol at the beginning or the end of the name<br/>- multiple consecutive slash (/) symbols");

            //this version supports absolute urls only
            //if (!Regex.IsMatch(createPageData.Route, "^/?[a-zA-Z0-9_-]+(/[a-zA-Z0-9_-]+)*/?$"))
            //    throw new ApplicationException("Please enter a valid url. <br/>Allowed symbols:<br/>-  alphanumeric, en dash(-),  slash (/),  and underscore (_)<br/>No blanks are allowed.");

            Uri uri;
            var isValidRoutePattern = Uri.TryCreate(createPageData.Route, UriKind.Absolute, out uri);
            if(!isValidRoutePattern) throw new ApplicationException("Route pattern is not valid, make sure its absolute and not relative"); //relative patterns are not supported for now

            var data = new PageData
            {
                Id = _id,
                Markup = createPageData.Markup,
                Name = createPageData.Name,
                RoutePattern = createPageData.Route,
                ViewPath = string.Format("{0}.cshtml", _id)
            };

            _contentManager.Create<PageData>(_id, data);
        }

        public void Publish()
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
                RoutePattern = updatePageData.Route, 
                ViewPath = clonedContentToUpdate.ViewPath
            });
            PageChanged();
        }
    }

    public class PageFactory
    {
        private readonly InMemoryContentManager _contentManager;

        public PageFactory(InMemoryContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public Page Create(CreatePageData createPageData)
        {
            return new Page(createPageData,  _contentManager);
        }

        public Page Create(Guid id)
        {
            return new Page(id, _contentManager);
        }
    }

    public enum ContentStatus { Draft, Published, Deleted }
}