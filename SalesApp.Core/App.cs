using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;

namespace SalesApp.Core
{
    public class App : MvxApplication, IMvxApplication
    {
        public App()
        {
            Mvx.RegisterSingleton<IMvxAppStart>(new SalesAppStart());
        }
    }
}
