using System;
using System.Collections.Generic;
using System.Linq;
using MvcApplication1.Models;
using MvcApplication1.Models.Domain;

namespace FCG.RegoCms
{
    public class CmsBackendService
    {
        private readonly ContentService             _contentService;
        private readonly Func<ContentRepository>    _repositoryFactory;

        public CmsBackendService(ContentService contentService, Func<ContentRepository> repositoryFactory)
        {
            _contentService = contentService;
            _repositoryFactory = repositoryFactory;

            ContentChanged += () => {}; //so we don't check for null each time we firing it
        }

        public event Action ContentChanged;

        public IEnumerable<Language> GetLanguages()
        {
            return new[] { new Language { Code = "en-gb", Name = "English" } };
        }

        public Page CreatePage(string name, string route, string markup)
        {
            return CreatePage(new CreatePageData { Name = name, Markup = markup, Route = route });
        }

        public Page CreatePage(CreatePageData createPageData)
        {
            var page = new Page(
                id: Guid.NewGuid(), 
                contentChangedEvent: () => ContentChanged(), 
                contentService:_contentService,
                repositoryFactory: _repositoryFactory
            );
            ContentChanged();
            return page;
        }

        public Page GetPage(Guid id)
        {
            throw new NotImplementedException();
        }

        public Page[] GetPages()
        {
            throw new NotImplementedException();
        }
    }
}