using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using MvcApplication1.Models;
using NUnit.Framework;

namespace FCG.RegoCms
{
    public class Page
    {
        private readonly Guid                       _id;
        private readonly Action                     _contentChangedEvent;
        private readonly ContentService             _contentService;
        private readonly Func<ContentRepository>    _repositoryFactory;
        private ContentItem<PageData>               _contentItem;
        private bool                                _initialized;

        public Page(
            Guid id, 
            Action contentChangedEvent, 
            ContentService contentService,
            Func<ContentRepository> repositoryFactory)
        {
            _id = id;
            _contentChangedEvent = contentChangedEvent;
            _contentService = contentService;
            _repositoryFactory = repositoryFactory;
            _initialized = false;
        }

        public PageData DraftData { get { return _contentItem.Draft.Content; } }
        public PageData PublishedData { get { return _contentItem.Published.Content; } }
        public PageData LastData { get { return _contentItem.Last.Content; } }
        public ContentItemVersion<PageData> LastVersion { get { return _contentItem.Last; } }

        public ValidationResult Validate(CreatePageData createPageData)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrEmpty(createPageData.Name) || createPageData.Name.Length > 40)
                errors.Add(new ValidationError 
                { 
                    Field =  "Name", 
                    Message = "Please enter a page name. Must be between 1 and 40 characters long."
                });

            if (!Regex.IsMatch(createPageData.Name, "^[a-zA-Z0-9_-]+(/[a-zA-Z0-9_-]+)*$"))
                errors.Add(new ValidationError
                {
                    Field = "Name",
                    Message = "Please enter a valid name. Allowed symbols:<br/>-  alphanumeric, en dash(-),  slash (/),  and underscore (_)<br/>Not allowed:<br/>- blanks <br/>- single slash(/) symbol at the beginning or the end of the name<br/>- multiple consecutive slash (/) symbols"
                });

            //todo: impelement relative urls support
            //if (!Regex.IsMatch(createPageData.Route, "^/?[a-zA-Z0-9_-]+(/[a-zA-Z0-9_-]+)*/?$"))
            //    throw new ApplicationException("Please enter a valid url. <br/>Allowed symbols:<br/>-  alphanumeric, en dash(-),  slash (/),  and underscore (_)<br/>No blanks are allowed.");

            Uri uri;
            var isValidRoutePattern = Uri.TryCreate(createPageData.Route, UriKind.Absolute, out uri);
            if (!isValidRoutePattern)
                errors.Add(new ValidationError
                {
                    Field = "Route",
                    Message = "Route pattern is not valid, make sure its absolute and not relative"
                });
            return new ValidationResult { IsValid = !errors.Any(), ValidationErrors = errors.ToArray() };
        }

        internal void Create(CreatePageData createPageData)
        {
            Contract.Assume(!_initialized, "This page instance is initialized already");

            

            var id = Guid.NewGuid();
            var data = new PageData
            {
                Id = id,
                Markup = createPageData.Markup,
                Name = createPageData.Name,
                RoutePattern = createPageData.Route,
                ViewPath = string.Format("{0}.cshtml", id)
            };
            _contentItem = _contentService.Create(data);

            using (var repository = _repositoryFactory())
            {
                repository.ContentItems.Add(new ContentItemData
                {
                    Id = _contentItem.Id,
                    DraftVersionId = _contentItem.Draft.Id,
                    PublishedVersionId = _contentItem.Published.Id,
                    Status = _contentItem.Status
                });
                if (_contentItem.Draft != null)
                    repository.ViewVersions.Add(new ViewVersionData(_contentItem.Draft));
                if (_contentItem.Published != null)
                    repository.ViewVersions.Add(new ViewVersionData(_contentItem.Published));

                repository.SaveChanges();
            }
            _initialized = true;
            _contentChangedEvent();
        }

        public void Publish()
        {
            Contract.Assume(_initialized, "This page instance is not initialized yet");

            var oldContentItem = _contentItem;
            _contentItem = _contentItem.Publish();
            using (var repository = _repositoryFactory())
            {
                var contentItemData = repository.ContentItems.Single(x => x.Id == _id);

                //delete draft from repository
                contentItemData.DraftVersionId = null;
                var draftVersionData = repository.ViewVersions.Single(x => x.Id == oldContentItem.Draft.Id);
                repository.ViewVersions.Remove(draftVersionData);
                
                //update published version data
                var publishedVersionData = repository.ViewVersions.Single(x => x.Id == _contentItem.Published.Id);
                publishedVersionData.VersionType = _contentItem.Published.Type;
                contentItemData.PublishedVersionId = publishedVersionData.Id;

                repository.SaveChanges();
            }
            _contentChangedEvent();
        }


        public void Delete()
        {
            Contract.Assume(_initialized, "This page instance is not initialized yet");
            _contentItem = _contentItem.Delete();
        }

        public void Update(UpdatePageData updateData)
        {
            throw new NotImplementedException();
        }
    }
}