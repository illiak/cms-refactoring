using System.Collections;
using System.Collections.Generic;
using MvcApplication1.Models;

namespace MvcApplication1.Tests
{
    public class GivenMultipleCreatedPages : GivenCreatedPageContext
    {
        protected IEnumerable<Page> _pages;

        protected override void Given()
        {
            base.Given();

            _pages = CreateMultipleTestPages();
        }

        public void CanGetListOfPages()
        {
            var pages = _cmsBackendService.GetPages();
        }
    }
}