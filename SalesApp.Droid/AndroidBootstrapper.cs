using Android.App;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Controllers.Security;
using SalesApp.Core.BL.Controllers.Synchronization;
using SalesApp.Core.Framework;
using SalesApp.Core.Logging;
using SalesApp.Core.Services;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Device;
using SalesApp.Core.Services.Interfaces;
using SalesApp.Core.Services.Locations;
using SalesApp.Core.Services.Notifications;
using SalesApp.Core.Services.Person.Customer.Photo;
using SalesApp.Core.Services.Platform;
using SalesApp.Core.Services.SharedPrefs;
using SalesApp.Core.ViewModels;
using SalesApp.Core.ViewModels.Dialog;
using SalesApp.Droid.Adapters.Auth;
using SalesApp.Droid.BusinessLogic.Controllers.Cryptography;
using SalesApp.Droid.Framework;
using SalesApp.Droid.Logging;
using SalesApp.Droid.Services;
using SalesApp.Droid.Services.Connectivity;
using SalesApp.Droid.Services.Customer.Photo;
using SalesApp.Droid.Services.Device;
using SalesApp.Droid.Services.Locations;
using SalesApp.Droid.Services.Notification;
using SalesApp.Droid.Services.Platform;
using SalesApp.Droid.Services.SharedPrefs;
using SalesApp.Droid.UI;
using SalesApp.Droid.Views;

namespace SalesApp.Droid
{
    public class AndroidBootstrapper
    {
        /// <summary>
        /// Load the application
        /// </summary>
        public void Bootstrap()
        {
            Resolver resolver = Resolver.Instance;

            resolver.Register<ILog, Log>();
            RegisterServices(resolver);
            resolver.Register<ISalesAppSession, SalesAppSession>();
            resolver.Register<IHashing, Hashing>();
            resolver.Register<IXamarinPlatform, XamarinPlatform>();
            resolver.Register<IStorageService, StorageService>();
            resolver.Register<ILogSettings, LogSettings>();
            resolver.Register<IConfigService, ConfigService>();
            resolver.RegisterClass<ProspectsController>();
            resolver.RegisterClass<SyncingController>();
            resolver.Register<ICulture, Culture>();
            resolver.Register<ILocationServiceListener, LocationService>();
            resolver.Register<INotificationService, LocalNotificationService>();
            resolver.RegisterSingleton<IDeviceResource>(new AndroidDeviceResource(Application.Context));
            resolver.Register<ISharedPrefService, SharedPrefService>();
            resolver.Register<IDialogService, DialogService>();
            resolver.Register<IPhotoService, PhotoService>();
        }

        private void RegisterServices(Resolver resolver)
        {
            resolver.Register<IConnectivityService, ConnectivityService>();
            resolver.Register<ISmsService, AndroidSmsService>();
            resolver.Register<IInformation, AndroidInformation>();
            resolver.Register<ITimeService, TimeService>();
        }
    }
}