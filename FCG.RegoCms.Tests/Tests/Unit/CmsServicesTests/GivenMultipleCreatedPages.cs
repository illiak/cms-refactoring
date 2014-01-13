using System;
using System.Collections;
using System.Collections.Generic;
using FCG.RegoCms;
using MvcApplication1.Models;

namespace FCG.RegoCms.Tests.CmsServicesTests
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
            throw new NotImplementedException();
        }
    }
}