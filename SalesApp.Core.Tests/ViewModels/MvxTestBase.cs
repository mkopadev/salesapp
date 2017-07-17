using MvvmCross.Core.Views;
using MvvmCross.Platform.Core;
using MvvmCross.Test.Core;

namespace SalesApp.Core.Tests.ViewModels
{
    public class MvxTestBase : MvxIoCSupportingTest
    {
        protected MockDispatcher MockDispatcher { get; private set; }

        protected override void AdditionalSetup()
        {
            this.MockDispatcher = new MockDispatcher();
            this.Ioc.RegisterSingleton<IMvxViewDispatcher>(this.MockDispatcher);
            this.Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(this.MockDispatcher);
        }
    }
}