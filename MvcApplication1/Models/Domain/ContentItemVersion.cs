using System;
using System.Diagnostics.Contracts;

namespace FCG.RegoCms
{
    public class ContentItemVersion<TContent> where TContent : class
    {
        private readonly Func<TContent, Guid> _keySelector;

        public ContentItemVersion(Guid id, Func<TContent, Guid> keySelector, TContent content)
        {
            Contract.Requires(keySelector != null);
            Contract.Requires(content != null);
            Id = id;
            _keySelector = keySelector;
            Content = content;
            CreatedOn = DateTimeOffset.Now;
        }

        public Guid                 Id { get; private set; }
        public Guid                 ContentId { get { return _keySelector(Content); } }
        public TContent             Content { get; internal set; }
        public ContentVersionType   Type { get; internal set; }
        public ContentStatus        Status { get; internal set; }

        public DateTimeOffset       CreatedOn { get; internal set; }
        public DateTimeOffset?      ModifiedOn { get; internal set; }
        public DateTimeOffset?      DeletedOn { get; internal set; }

        internal ContentItemVersion<TContent> Publish()
        {
            Contract.Requires(Type == ContentVersionType.Draft, "Only Draft versions can be published");
            Contract.Requires(Status == ContentStatus.Active, "Only versions with Active status can be published");

            var published = Clone();
            published.Id = Guid.NewGuid();
            published.Type = ContentVersionType.Published;
            published.ModifiedOn = DateTimeOffset.UtcNow;

            return published;
        }

        internal ContentItemVersion<TContent> Update(Action<TContent> updateFunc) 
        {
            Contract.Requires(updateFunc != null);

            var updated = Clone();
            updated.Id = Guid.NewGuid();
            updateFunc(updated.Content);
            updated.Type = ContentVersionType.Draft;
            updated.ModifiedOn = DateTimeOffset.UtcNow;

            return updated;
        }

        internal ContentItemVersion<TContent> Delete()
        {
            Contract.Requires(Status == ContentStatus.Deleted, "This content version was already deleted");
            Contract.Requires(Status == ContentStatus.Active, "Only Active version can be deleted");

            var deleted = Clone();
            deleted.Id = Guid.NewGuid();
            deleted.Status = ContentStatus.Deleted;

            return deleted;
        }

        internal ContentItemVersion<TContent> Clone()
        {
            return (ContentItemVersion<TContent>)this.MemberwiseClone();
        }
    }
}