using System;
using Android.Content;
using Android.Gms.Analytics;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.GAnalytics;
using SalesApp.Core.Services.Settings;

namespace SalesApp.Droid.Services.GAnalytics
{
    public class GoogleAnalyticService : IGoogleAnalyticService
    {
        private static GoogleAnalytics _googleAnalytics;
        private static Tracker _tracker;
        private static GoogleAnalyticService _instance;
        private ILog _logger = LogManager.Get(typeof (GoogleAnalyticService));
        private GoogleAnalyticService()
        {
        }

        public static GoogleAnalyticService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GoogleAnalyticService();
                }

                return _instance;
            }
        }

        public void Initialize(Context context)
        {
            var trackingId = Settings.Instance.GoogleAnalyticsTrackingId;
            if (trackingId == null)
            {
                _logger.Debug("Google analytics tracking id not found");
                return;
            }
           
            _googleAnalytics = GoogleAnalytics.GetInstance(context.ApplicationContext);
            _googleAnalytics.SetLocalDispatchPeriod(10);
            _tracker = _googleAnalytics.NewTracker(trackingId);
            _tracker.EnableExceptionReporting(true);
            _tracker.EnableAdvertisingIdCollection(true);
            _tracker.EnableAutoActivityTracking(false);
        
        }


        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void TrackScreen(string pageName)
        {
            if (_tracker != null)
            {
            _tracker.SetScreenName(pageName);
            _tracker.Send(new HitBuilders.ScreenViewBuilder().Build());
        }

        }

        public void TrackEvent(string category,string title, string eventName)
        {
            if (_tracker != null)
            {
            HitBuilders.EventBuilder builder = new HitBuilders.EventBuilder();
            builder.SetCategory(category);
            builder.SetLabel(title);
            builder.SetAction(eventName);
            _tracker.Send(builder.Build());
        }
    }
}
}