using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using FCG.RegoCms;
using MvcApplication1.Models.Domain;
using NUnit.Framework;

namespace MvcApplication1.Models
{
    public class Page
    {
        private readonly Func<PageData, ContentItem<PageData>> _contentItemFactory;
        private ContentItem<PageData>   _contentItem; 

        public Page(Func<PageData, ContentItem<PageData>> contentItemFactory)
        {
            _contentItemFactory = contentItemFactory;
        }

        public void Create(CreatePageData createPageData)
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

            _contentItem = _contentItemFactory(data);
        }

        public void Publish()
        {
            _contentService.Publish(_id);
            PageChanged();
        }

        public void Delete()
        {
            _contentService.Delete(_id);
            PageChanged();
        } 

        public void Update(UpdatePageData updatePageData)
        {
            var contentItem = _contentService.GetContentItem<PageData>(_id);
            var contentToUpdate = contentItem.ContentDraft ?? contentItem.ContentPublished;
            var clonedContentToUpdate = contentToUpdate.Clone(); //make sure that published version will not be changed by a chance

            _contentService.Update<PageData>(_id, new PageData
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
        private readonly ContentService _contentService;

        public PageFactory(ContentService contentService)
        {
            _contentService = contentService;
        }

        public Page Create(CreatePageData createPageData)
        {
            return new Page(createPageData,  _contentService);
        }

        public Page Create(Guid id)
        {
            return new Page(id, _contentService);
        }
    }

    public enum ContentStatus { Draft, Published, Deleted }
}