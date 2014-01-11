using System;
using System.Collections.Generic;
using System.Linq;
using MvcApplication1.Models.Domain;

namespace MvcApplication1.Models
{
    public class CmsBackendService
    {
        private readonly PageFactory            _pageFactory;
        private readonly InMemoryContentManager _contentManager;

        public CmsBackendService(PageFactory pageFactory, 
                                 InMemoryContentManager contentManager)
        {
            _pageFactory = pageFactory;
            _contentManager = contentManager;

            ContentUpdated += () => {}; //so we don't check for null each time we firing it
        }

        public event Action ContentUpdated;

        public IEnumerable<Language> GetLanguages()
        {
            return new[] { new Language { Code = "en-gb", Name = "English" } };
        }

        public IEnumerable<ContentItemVersion2<PageData>> PagesGetLastVersions()
        {
            throw new NotImplementedException();
        }
        public 



        public Page[]   CreatePages(CreatePageData[] data)
        {
            return data.Select(CreatePage).ToArray();
        }
        public Page     CreatePage(string name, string route, string markup)
        {
            var page = CreatePage(new CreatePageData { Name = name, Markup = markup, Route = route });
            page.PageChanged += () => ContentUpdated();
            ContentUpdated();
            return page;
        }

        private Page    CreatePage(CreatePageData createPageData)
        {
            return _pageFactory.Create(createPageData);
        }

        public Page     GetPage(Guid id)
        {
            return _pageFactory.Create(id);
        }

        public IEnumerable<Page> GetPages()
        {
            throw new NotImplementedException();
        }
    }
}