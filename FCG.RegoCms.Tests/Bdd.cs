using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace MvcApplication1.Tests
{
    [TestFixture]
    public abstract class BddTestBase
    {
        protected readonly UnityContainer _container;

        public BddTestBase()
        {
            _container = new UnityContainer();
        }

        [SetUp]
        protected abstract void Given();
    }

    [Category("Unit")]
    public abstract class BddUnitTestBase : BddTestBase { }
}