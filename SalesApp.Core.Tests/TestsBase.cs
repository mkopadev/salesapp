using System;
using NUnit.Framework;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Tests
{
    [TestFixture]
    public abstract class TestsBase
    {
        protected static readonly Guid ApiGuid = Guid.Parse("91311660-50FD-E411-BEB2-0009DC09884A");
        protected static readonly Guid LocalGuid = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public ILog Logger { get; private set; }

        [SetUp]
        public virtual void Setup()
        {
            try
            {
                AsyncHelper.RunSync(async () => await new TestBootstrapper().Bootstrap());
                this.Logger = Resolver.Instance.Get<ILog>();
                this.Logger.Initialize(this.GetType().FullName);
                this.Logger.Debug("Bootstrapping completed");
            }
            catch (Exception exception)
            {
                this.Logger.Error(exception);
                throw;
            }
        }

        [TearDown]
        public virtual void TearDown()
        {
        }
    }
}