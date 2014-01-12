using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MvcApplication1.Models;
using MvcApplication1.Models.Domain;

namespace FCG.RegoCms
{
    public class CmsBackendService
    {
        private readonly ContentService _contentService;

        public CmsBackendService(ContentService contentService)
        {
            _contentService = contentService;

            ContentChanged += () => {}; //so we don't check for null each time we firing it
        }

        public event Action ContentChanged;

        public IEnumerable<Language> GetLanguages()
        {
            return new[] { new Language { Code = "en-gb", Name = "English" } };
        }

        public ContentItem<PageData> CreatePage(string name, string route, string markup)
        {
            return CreatePage(new CreatePageData { Name = name, Markup = markup, Route = route });
        }

        public ContentItem<PageData> CreatePage(CreatePageData createPageData)
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
            if (!isValidRoutePattern) throw new ApplicationException("Route pattern is not valid, make sure its absolute and not relative"); //relative patterns are not supported for now

            var id = Guid.NewGuid();
            var data = new PageData
            {
                Id = id,
                Markup = createPageData.Markup,
                Name = createPageData.Name,
                RoutePattern = createPageData.Route,
                ViewPath = string.Format("{0}.cshtml", id)
            };
            var result = _contentService.Create(data);

            ContentChanged();
            return result;
        }

        public ContentItem<PageData> GetPage(Guid id)
        {
            return _pageFactory.Create(id);
        }

        public ContentItem<PageData>[] GetPages()
        {
            throw new NotImplementedException();
        }
    }
}