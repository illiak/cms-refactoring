using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace FCG.RegoCms.Tests
{
    [TestFixture]
    public abstract class BddTestBase
    {
        protected readonly UnityContainer _container;

        protected BddTestBase()
        {
            _container = new UnityContainer();
        }

        [SetUp]
        protected virtual void Given() { }
    }

    [Category("Unit")]
    public abstract class BddUnitTestBase : BddTestBase { }
}