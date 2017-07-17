using MvvmCross.Core.ViewModels;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Security;
using SalesApp.Core.Services.SharedPrefs;
using SalesApp.Core.ViewModels.Security;

namespace SalesApp.Core
{
    /// <summary>
    /// Custom app start mechanism
    /// </summary>
    public class SalesAppStart : MvxNavigatingObject, IMvxAppStart
    {
        /// <summary>
        /// Show different screen based on login status
        /// </summary>
        /// <param name="hint">The hint</param>
        public void Start(object hint = null)
        {
            var sharedPrefService = Resolver.Instance.Get<ISharedPrefService>();
            bool isFirstLogin = sharedPrefService.GetBool(LoginService.IsFirstLogin, true);

            if (isFirstLogin)
            {
                this.ShowViewModel<DeviceRegistrationStep1ViewModel>();
            }
            else
            {
                this.ShowViewModel<LoginActivityViewModel>();
            }
        }
    }
}