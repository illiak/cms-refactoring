using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FCG.RegoCms;
using MvcApplication1.Models;
using NUnit.Framework;

namespace FCG.RegoCms.Tests.Unit.CmsServicesTests
{
    class GivenMultipleCreatedPages : GivenCreatedPageContext
    {
        protected IEnumerable<Page> _pages;

        protected override void Given()
        {
            base.Given();

            _pages = CreateMultipleTestPages();
        }

        [Test]
        public void CanGetListOfPages()
        {
            var pages = _cmsBackendService.GetPages();
            Assert.That(pages.Count(), Is.EqualTo(_pages.Count() + 1)); //adding one because one page was created before calling CreateMultipleTestPages() method
        }
    }
}