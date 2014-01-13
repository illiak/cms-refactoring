using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using MvcApplication1.Models;
using MvcApplication1.Models.Domain;

namespace FCG.RegoCms
{
    public class CmsService
    {
        private readonly Func<ContentRepository>    _repositoryFactory;

        public CmsService(Func<ContentRepository> repositoryFactory)
        {
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
            Contract.Requires(createPageData != null);

            var page = new Page(
                contentChangedEvent: () => ContentChanged(), 
                repositoryFactory: _repositoryFactory
            );
            page.Create(createPageData);
            ContentChanged();
            return page;
        }

        public Page GetPage(Guid id)
        {
            using (var repository = _repositoryFactory())
            {
                var contentItemData = repository.ContentItems.Single(x => x.Id == id);

                ViewVersionData draftVersionData = null, publishedVersionData = null;
                if (contentItemData.DraftVersionId != null)
                    draftVersionData = repository.ViewVersions.Single(x => x.Id == contentItemData.DraftVersionId);
                if (contentItemData.PublishedVersionId != null)
                    publishedVersionData = repository.ViewVersions.Single(x => x.Id == contentItemData.PublishedVersionId);

                var page = new Page(
                        contentChangedEvent: () => ContentChanged(),
                        repositoryFactory: _repositoryFactory
                    );
                page.Load(contentItemData, draftVersionData, publishedVersionData);

                return page;
            }
        }

        public Page[] GetPages(bool excludeDeleted = true)
        {
            using (var repository = _repositoryFactory())
            {
                var query = repository.ContentItems.AsQueryable();
                if (excludeDeleted)
                    query = query.Where(x => x.Status != ContentStatus.Deleted);

                var contentItems = query.ToArray();

                var versionIds = contentItems.SelectMany(x => new[] { x.DraftVersionId, x.PublishedVersionId })
                                             .Where(x => x != null)
                                             .ToArray();
                var versions = repository.ViewVersions.Where(x => versionIds.Contains(x.Id)).ToArray();

                var pages = contentItems.Select(contentItemData =>
                {
                    ViewVersionData draftVersionData = null, publishedVersionData = null;
                    if (contentItemData.DraftVersionId != null)
                        draftVersionData = versions.Single(x => x.Id == contentItemData.DraftVersionId);
                    if (contentItemData.PublishedVersionId != null)
                        publishedVersionData = versions.Single(x => x.Id == contentItemData.PublishedVersionId);

                    var page = new Page(
                        contentChangedEvent: () => ContentChanged(),
                        repositoryFactory: _repositoryFactory
                    );
                    page.Load(contentItemData, draftVersionData, publishedVersionData);
                    return page;
                });

                return pages.ToArray();
            }
        }
    }
}