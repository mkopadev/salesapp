using System.Threading.Tasks;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Controllers.Security;
using SalesApp.Core.BL.Controllers.Synchronization;
using SalesApp.Core.Framework;
using SalesApp.Core.Logging;
using SalesApp.Core.Services;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Locations;
using SalesApp.Core.Tests.BL.Controllers.Cryptography;
using SalesApp.Core.Tests.Services.Connectivity;
using SalesApp.Core.Tests.Services.Locations;
using SalesApp.Core.ViewModels;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Generic;

namespace SalesApp.Core.Tests
{
    public class TestBootstrapper
    {
        public async Task Bootstrap()
        {
            Resolver resolver = Resolver.Instance;
            resolver.Register<ILog, LogManager.DefaultLogger>();
            resolver.Register<IHashing, Hashing>();
            resolver.Register<IConfigService, ConfigService>();
            resolver.RegisterClass<ProspectsController>();
            resolver.RegisterClass<SyncingController>();
            resolver.Register<ICulture, TestCulture>();
            resolver.Register<ILocationServiceListener, LocationServiceListener>();
            resolver.Register<ISalesAppSession, Session>();
            resolver.Register<IConnectivityService, ConnectivityService>();
            resolver.Register<ITimeService, TimeService>();
            resolver.Register<IDeviceResource, TestDeviceResource>();

            await this.InitializeDb();
        }

        private async Task InitializeDb()
        {
            StorageService storageService = new StorageService();
            string path = storageService.GetPathForFileAsync("SalesApp.db3");

            ISQLitePlatform sqLitePlatform = new SQLitePlatformGeneric();
            await DatabaseInstance.Instance.InitializeDb(sqLitePlatform, path);
        }
    }
}
