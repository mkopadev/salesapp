using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Util;
using Java.Util;
using SalesApp.Core.BL.Cache;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Interfaces;
using SalesApp.Core.Services.Locations;
using SalesApp.Core.Services.Settings;
using SQLite.Net.Interop;
using Xamarin;

namespace SalesApp.Droid
{
    [Application]
    public class SalesApplication : Application
    {
        static bool _falseflag = false;

        public static bool IsInBackground;

        private ILog _log;

        public static SalesApplication Instance { get; private set; }


        private Intent _locationServiceIntent = null;

        private AlarmManager _alarmManager;

        private PendingIntent _pendingIntent;

        public SalesApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
            Instance = this;

            // Set up Xamarin Insights
            // first configure attempt to catch start-up exceptions
            Insights.HasPendingCrashReport += (sender, isStartupCrash) =>
            {
                if (isStartupCrash)
                {
                    Insights.PurgePendingCrashReports().Wait();
                }
            };

            // now configure different keys for
            #if DEBUG
                 Insights.Initialize(Insights.DebugModeKey, this);
            #elif UAT
                 Insights.Initialize(Settings.Instance.InsightsDebugApiKey, this);
            #elif STAGING
                 Insights.Initialize(Settings.Instance.InsightsDebugApiKey, this);
            #else
                 Insights.Initialize(Settings.Instance.InsightsProductionApiKey, this);
            #endif

            //set up global exception handler to log app crashes
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Bootstrap the application, load all needed components
            new AndroidBootstrapper().Bootstrap();

            // set up global exception handler to log app crashes
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // retrieve the logger
            _log = LogManager.Get(typeof(SalesApplication));
            _log.Verbose("Starting application, initialized logger.");

            AsyncHelper.RunSync(async () => await this.ManageDbVersion());

            SetAppLanguage();
        }

        private void SetAppLanguage()
        {
            // Change locale settings in the app.
            DisplayMetrics displayMetrics = Resources.DisplayMetrics;
            Configuration configuration = Resources.Configuration;
            string lang = Settings.Instance.DsrLanguage.ToString().ToLower();
            Logger.Verbose("Language to set " + lang);
            configuration.Locale = new Locale(lang);
            Resources.UpdateConfiguration(configuration, displayMetrics);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var unhandledException = e.ExceptionObject as Exception;
            if (unhandledException != null)
            {
                System.Diagnostics.Debug.WriteLine(unhandledException.Message);
                System.Diagnostics.Debug.WriteLine(unhandledException.StackTrace);
                Resolver.Instance.Get<ILog>().Error(unhandledException);
                Insights.Report(unhandledException, Insights.Severity.Error);
            }
        }

        public async Task ManageDbVersion()
        {
            try
            {
                ISQLitePlatform androidSQlitePlatform = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();
                IStorageService storageService = Resolver.Instance.Get<IStorageService>();
                await DatabaseInstance.Instance.InitializeDb(androidSQlitePlatform, storageService.GetPathForFileAsync(DatabaseInstance.DbName));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        private ILog _logger;

        private ILog Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = Resolver.Instance.Get<ILog>();
                    _logger.Initialize(this.GetType().FullName);
                }
                return _logger;
            }
        }
        
        public override void OnCreate()
        {
            // If OnCreate is overridden, the overridden c'tor will also be called.
            base.OnCreate();
            _alarmManager = GetSystemService(Context.AlarmService) as AlarmManager;
        }

        public async Task CheckLocation()
        {

            MemoryCache memoryCache = MemoryCache.Instance;
            bool enabled = await memoryCache.Get<bool>("locationEnabled");
            bool locationIsOn = await memoryCache.Get<bool>("LocationIsOn");
            int locaInterval = await memoryCache.Get<int>("LocaInterval");
            // If EnableLocationInfo is true then the app will listen to location updates
            if (enabled)
            {
                if (locationIsOn)
                {
                    int interval = locaInterval == 0 ? 5 : locaInterval;
                    ScheduleLocationUpdate(interval);
                }
            }
        }

        // Schedule location updates using alarm
        private async Task ScheduleLocationUpdate(int interval)
        {
            Intent intent = new Intent(this, typeof(LocationReceiver));
            DateTime dateTime = DateTime.Now;
            long startTime = dateTime.Millisecond;
            Log.Debug("Location", "Requested after " + interval + " minutes");
            _pendingIntent = PendingIntent.GetBroadcast(this, 0, intent, 0);
            _alarmManager.SetRepeating(AlarmType.Rtc, startTime, interval * 60 * interval, _pendingIntent);
        }

        public void StopLocationUpdates()
        {
            _alarmManager.Cancel(_pendingIntent);
        }

        public void InitializeLocationUpdates()
        {
            MemoryCache memoryCache = MemoryCache.Instance;
            var locationServices = Resolver.Instance.Get<ILocationServiceListener>();
            var settings = Settings.Instance;
            memoryCache.Store<string>("providerType", settings.ProviderType);
            memoryCache.Store<bool>("locationEnabled", settings.EnableLocationInfo);
            memoryCache.Store<int>("LocaInterval", settings.LocationInfoRetrievalInterval);
            memoryCache.Store<bool>("LocationIsOn", locationServices.IsLocationOn());
            CheckLocation();
        }
    }
}