using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.RegoCms.Tests.Mocks
{
    public class FakeContentRepository : ContentRepository
    {
        private readonly FakeDbSet<ContentItemData> _contentItems;
        private readonly FakeDbSet<ViewVersionData> _viewVersions;

        public FakeContentRepository()
        {
            _contentItems = new FakeDbSet<ContentItemData>();
            _viewVersions = new FakeDbSet<ViewVersionData>();
        }

        public override IDbSet<ContentItemData> ContentItems { get { return _contentItems; } }
        public override IDbSet<ViewVersionData> ViewVersions { get { return _viewVersions; } }
    }
}
