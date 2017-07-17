using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AppseeAnalytics.Android;
using Com.Crashlytics.Android;
using MvvmCross.Droid.Views;
using SalesApp.Core.Auth;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Droid
{
    /// <summary>
    /// This is the splash screen activity
    /// </summary>
    [Activity(Theme = "@style/Theme.Splash", MainLauncher = true, NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : MvxSplashScreenActivity, IAppseeListener
    {
        private static ILog _logger = LogManager.Get(typeof(ILog));

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Logger.Initialize(this.GetType().FullName);

            try
            {
                IO.Fabric.Sdk.Android.Fabric.With(this, new Com.Crashlytics.Android.Crashlytics());
                ISalesAppSession session = Resolver.Instance.Get<ISalesAppSession>();
                if (session != null)
                {
                    Crashlytics.SetUserIdentifier(session.UserId.ToString());
                    Crashlytics.SetUserName(session.FirstName + " " + session.LastName);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            try
            {
                Appsee.AddAppseeListener(this);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            try
            {
                string key = "8d85ddd07dbb41909e60e7a54c6d0a7e";
#if RELEASE
                key = "16f9c8d0c2244b5dab0eef726d7a6245";
#endif
                Appsee.Start(key);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }
        protected static ILog Logger
        {
            get
            {
                return _logger;
            }
        }

        public void OnAppseeScreenDetected(AppseeScreenDetectedInfo p0)
        {
        }

        public void OnAppseeSessionEnded(AppseeSessionEndedInfo p0)
        {
        }

        public void OnAppseeSessionEnding(AppseeSessionEndingInfo p0)
        {
        }

        public void OnAppseeSessionStarted(AppseeSessionStartedInfo p0)
        {
            try
            {
                ISalesAppSession session = Resolver.Instance.Get<ISalesAppSession>();
                if (session != null)
                {
                    Appsee.SetUserId(session.UserId.ToString());
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void OnAppseeSessionStarting(AppseeSessionStartingInfo p0)
        {
        }
    }
}