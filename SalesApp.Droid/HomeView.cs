using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using SalesApp.Core.Auth;
using SalesApp.Core.Enums.Security;
using SalesApp.Core.Events.CustomerPhoto;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Locations;
using SalesApp.Core.Services.Settings;
using SalesApp.Core.Services.SharedPrefs;
using SalesApp.Core.ViewModels.Home;
using SalesApp.Droid.BusinessLogic.Models.Security;
using SalesApp.Droid.Enums;
using SalesApp.Droid.Framework;
using SalesApp.Droid.People.Customers;
using SalesApp.Droid.People.Prospects;
using SalesApp.Droid.People.SwapComponents;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.Tickets;
using SalesApp.Droid.UI.Utils;
using SalesApp.Droid.UI.Wizardry;

namespace SalesApp.Droid
{
    [Activity(Theme = "@style/AppTheme.SmallToolbar", ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = false)]
    public class HomeView : MvxNavigationViewBase<HomeViewModel>
    {
        private ISalesAppSession _session;
        private WelcomeView _welcomeView;
        private readonly ILog _logger = Resolver.Instance.Get<ILog>();
        private ILocationServiceListener _locationListener = Resolver.Instance.Get<ILocationServiceListener>();
        private Intent _photoUploadServiceIntent;
        private CustomerPhotoUploaderReceiver _customerPhotoUploaderReceiver;
        private ISharedPrefService _sharedPrefService;
        
        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            _session = Resolver.Instance.Get<ISalesAppSession>();
            _logger.Initialize(this.GetType().FullName);
            SetContentView(Resource.Layout.layout_welcome);

            this.AddToolbar(Resource.String.app_name, true);

            UiPermissionsController.SetViewsVisibilty();

            _sharedPrefService = Resolver.Instance.Get<ISharedPrefService>();

            _welcomeView = new WelcomeView(FindViewById<ViewGroup>(Resource.Id.welcomeRoot), this);
            _welcomeView.SetUser(_session.FirstName, _session.LastName);

            _welcomeView.RegisterProspectTouched += welcomeView_registerProspectTouched;
            
            _welcomeView.SwapComponentsTouched += (sender, e) => StartActivity(new Intent(this, typeof (SwapComponentsActivity)));
            
            FindViewById<RelativeLayout>(Resource.Id.btnRaiseIssue)
                .Click += (sender, e) =>
                {
                    new IntentStartPointTracker()
                        .StartIntentWithTracker
                        (
                            this
                            , IntentStartPointTracker.IntentStartPoint.WelcomeScreen
                            , typeof (TicketStartActivity)
                        );
                };
            _welcomeView.RegisterCustomerTouched += welcomeView_registerCustomerTouched;

            // check Location setting status from OTA
            // If its true then we check whether location settings on the device are on
            if (Settings.Instance.EnableLocationInfo)
            {            
                if (!_locationListener.IsLocationOn())
                {
                    string acceptLocationTracking = _sharedPrefService.Get("accept_location_tracking");
                    if (acceptLocationTracking == null || !acceptLocationTracking.Equals("NO"))
                    {
                        CheckLocationStatus();
                    }
                }
            }

            // set app Google analytics
            GoogleAnalyticService.Instance.Initialize(this);

            Logger.Verbose("Creating new instance of CustomerPhotoUploaderReceiver");
            _customerPhotoUploaderReceiver = new CustomerPhotoUploaderReceiver();

            _photoUploadServiceIntent = new Intent(this, typeof (CustomerPhotoUploadService));

            // start the customer photo upload service
            StartService(_photoUploadServiceIntent);
        }

        private void CheckLocationStatus()
        {
            AlertDialogBuilder.Instance
                .AddButton(Resource.String.yes, UserAcceptedLocation)
                .AddButton(Resource.String.no, UserRejectedLocation)
                .SetText(null, GetString(Resource.String.switch_location_on))
                .Show(this);
        }

        void welcomeView_registerProspectTouched(object sender, EventArgs e)
        {
            WizardLauncher.Launch(this, WizardTypes.ProspectRegistration, IntentStartPointTracker.IntentStartPoint.WelcomeScreen);
        }

        void welcomeView_registerCustomerTouched(object sender, EventArgs e)
        {
            WizardLauncher.Launch(this, WizardTypes.CustomerRegistration, IntentStartPointTracker.IntentStartPoint.WelcomeScreen);
        }

        public override void SetViewPermissions()
        {
            UiPermissionsController.RegisterViews(
                new ViewPermission(Resource.Id.btnNewProspect, Permissions.RegisterProspect), 
                new ViewPermission(Resource.Id.btnNewCustomer, Permissions.RegisterCustomer), 
                new ViewPermission(Resource.Id.button_manage_stock, Permissions.StockManagement), 
                new ViewPermission(Resource.Id.btnSwapComponents,Permissions.SwapComponents), 
                new  ViewPermission(Resource.Id.btnRaiseIssue, Permissions.RaiseIssue));
        }

		protected override void OnResume ()
		{
            _welcomeView.SetUser(_session.FirstName, _session.LastName);
		    base.OnResume ();
            

            var intentFilter = new IntentFilter(CustomerPhotoUploadService.PhotosUploadedAction);

            RegisterReceiver(_customerPhotoUploaderReceiver, intentFilter);
            _customerPhotoUploaderReceiver.UploadStatusEvent += UploadStatusEvent;
		}

        protected override void OnPause()
        {
            base.OnPause();
            UnregisterReceiver(_customerPhotoUploaderReceiver);
        }


        public override void InitializeScreen()
        {
        }

        public override void RetrieveScreenInput()
        {
            throw new NotImplementedException();
        }

        public override void UpdateScreen()
        {
        }

        public override void SetListeners()
        {
        }

        public override bool Validate()
        {
            return true;
        }

        public void UserRejectedLocation()
        {
            _sharedPrefService.Save("accept_location_tracking", "NO");
        }

        public void UserAcceptedLocation()
        {
            StartActivity(new Intent(Android.Provider.Settings.ActionLocationSourceSettings));
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Home)
            {
                // do something
            }

            return base.OnKeyDown(keyCode, e);
        }

        protected override async void OnStart()
        {
            base.OnStart();
            SalesApplication.Instance.InitializeLocationUpdates();
            SalesApplication.Instance.CheckLocation();
        }

        public override void OnBackPressed()
        {
            // Dont go anywhere where back button is pressed
        }

        private void UploadStatusEvent(object sender, UploadStatusEventArgs e)
        {
            if (e.PhotoUploadStatusDictionary != null)
            {
                Logger.Verbose("Photo Upload Broadcast received : " + e.PhotoUploadStatusDictionary.Count);
            }
        }

    }
}