using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Telephony;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using MvvmCross.Droid.Platform;
using MvvmCross.Droid.Support.V7.AppCompat;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Device;
using SalesApp.Core.Services.RemoteServices.ErrorHandling;
using SalesApp.Droid.BusinessLogic.Controllers.Security;
using SalesApp.Droid.Components.UIComponents.ActivityBaseHelpers;
using SalesApp.Droid.Components.UIComponents.CustomInfo;
using SalesApp.Droid.People.Prospects;
using SalesApp.Droid.Views.LogFiles;
using ActionBar = Android.Support.V7.App.ActionBar;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace SalesApp.Droid.Components.UIComponents
{
    public interface IActivity
    {
        void SetScreenTitle(string title);

        void SetScreenTitle(int resId);

        void SetViewPermissions();

        /// <summary>
        /// Returns true if connection to the network exists or false if it doesn't
        /// </summary>
        bool ConnectedToNetwork { get; }

        /// <summary>
        /// Attempts to force the keyboard to be displayed.
        /// </summary>
        /// <param name="focusedView">The UI element in focus that the keyboard should send keypresses to</param>
        void ShowKeyboard(View focusedView);

        void HideKeyboard();
    }

    [Activity]
    public abstract class ActivityBase : MvxAppCompatActivity, IActivity
    {
        private ErrorHandlingHelper _errorHandlingHelper;

        public int ErrorFragmentContainerId { get; set; }

        public int ErrorSnackbarContainerId { get; set; }

        public bool ErrorFragmentContainerExists
        {
            get { return FindViewById(ErrorFragmentContainerId) != null; }
        }

        public bool ErrorSnackbarContainerExists
        {
            get { return FindViewById(ErrorSnackbarContainerId) != null; }
        }

        private UiPermissionsController _uiPermissionsController;

        protected UiPermissionsController UiPermissionsController
        {
            get
            {
                if (_uiPermissionsController == null)
                {
                    _uiPermissionsController = new UiPermissionsController(this);
                }

                return _uiPermissionsController;
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            ApiErrorHandler.SetErrorOccuredCallback(this.ErrorOccuredCallback);
            SalesApplication.IsInBackground = false;
        }

        private void ErrorOccuredCallback(ErrorDescriber errorDescriber)
        {
            this._errorHandlingHelper.ErrorOccured(errorDescriber);
        }

        private readonly IConnectivityService _connectivityService = Resolver.Instance.Get<IConnectivityService>();

        private CustomInfoFragment _customInfoFragment;
        protected Toolbar Toolbar;
        private Snackbar _snackbar;

        private static ILog _logger = LogManager.Get(typeof(ILog));

        protected static ILog Logger
        {
            get
            {
                return _logger;
            }
        }

        public IntentStartPointTracker.IntentStartPoint StartPointIntent { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Logger.Initialize(this.GetType().FullName);
            this._errorHandlingHelper = new ErrorHandlingHelper(this);

            // ensure the initialization is done
            var setup = MvxAndroidSetupSingleton.EnsureSingletonAvailable(this.ApplicationContext);
            setup.EnsureInitialized();

            base.OnCreate(savedInstanceState);
            
            try
            {
                this.SetViewPermissions();
            }
            catch (NotImplementedException)
            {
                Logger.Debug("View permissions not set for this activity. We'll pretend this never happened.");
            }

            ResolveStartPoint(savedInstanceState);
        }
        
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
              MenuInflater.Inflate(Resource.Menu.login_activity_items, menu);

              return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle item selection
            switch (item.ItemId)
            {
                case Resource.Id.action_about:
                    this.ShowAbout();
                    return true;
                case Resource.Id.action_logs:
                    this.GoToLogs();
                    return true;
                case Resource.Id.action_privacy:
                    this.LaunchPrivacyLink();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
        }
        }

        public void ShowAbout()
        {
            #if DEBUG
                        const string buildType = "Debug";
            #elif UAT
                        const string buildType = "UAT";
            #elif STAGING
                        const string buildType = "Staging";            
            #else
                        const string buildType = "Prod";
            #endif

            Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
            Android.App.AlertDialog alertDialog = builder.Create();
            IInformation information = Resolver.Instance.Get<IInformation>();
            alertDialog.SetTitle(Resource.String.app_name);
            alertDialog.SetMessage(string.Format(
                Resources.GetString(Resource.String.build_version),
                information.DeviceAppVersion,
                information.CoreVersion,
                information.BuildTime,
                buildType));

            alertDialog.SetButton(GetString(Resource.String.ok), (s, eventArgs) =>
            {
                alertDialog.Dismiss();
            });

            alertDialog.Show();
        }

        /// <summary>
        /// Goes to the log files screen
        /// </summary>
        public void GoToLogs()
        {
            Type thisType = this.GetType();
            Type targetType = typeof(LogSettingsView);
            if (thisType == targetType)
            {
                return;
            }

            Intent intent = new Intent(this, targetType);
            this.StartActivity(intent);
        }

        public void LaunchPrivacyLink()
        {
            var privacyPolicyUrl = GetString(Resource.String.privacy_policy_url);
            Intent intent = new Intent(Intent.ActionView);
            intent.SetData(Uri.Parse(privacyPolicyUrl));
            StartActivity(intent);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutString(IntentStartPointTracker.ActivityStartPoint, StartPointIntent.ToString());
            base.OnSaveInstanceState(outState);
        }

        void ResolveStartPoint(Bundle savedInstanceBundle)
        {
            if (savedInstanceBundle != null)
            {
                string startPoint = savedInstanceBundle.GetString(IntentStartPointTracker.ActivityStartPoint);
                if (startPoint.IsBlank())
                {
                    return;
                }
                StartPointIntent = startPoint.ToEnumValue<IntentStartPointTracker.IntentStartPoint>();
            }
            else
            {
                if (Intent.Extras != null)
                {
                    ResolveStartPoint(Intent.Extras);
                }
            }
        }
        
        public void SetScreenTitle(string title)
        {
            if (ActionBar == null)
            {
                return;
            }
                
                ActionBar.Title = title;
        }

        public FragmentManager GetFragmentManager()
        {
            return SupportFragmentManager;
        }

        public void AddToolbar(int title, bool isOnUiThread = false)
        {
            if (!isOnUiThread)
            {
                this.RunOnUiThread(() => AddToolbar(title, true));
                return;
            }

            Toolbar = this.FindViewById<Toolbar>(Resource.Id.main_toolbar);
            if (Toolbar == null)
            {
                throw new Exception(string.Format("Your activity requires to include android_toolbar.axml to implement the toolbar. Please include it in your activity's layout."));
            }

            this.SetSupportActionBar(Toolbar);
            if (ActionBar != null)
            {
                ActionBar.Elevation = 0;
            }

            this.SetScreenTitle(title);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            var upArrow = Resources.GetDrawable(Resource.Drawable.abc_ic_ab_back_mtrl_am_alpha);
            upArrow.SetColorFilter(Color.White, PorterDuff.Mode.SrcAtop);
            SupportActionBar.SetHomeAsUpIndicator(upArrow);
        }
        
        protected new ActionBar ActionBar
        {
            get { return SupportActionBar; }
        }

        public void SetScreenTitle(int resId)
        {
            SetScreenTitle(GetString(resId));
        }

        public void EnableHomeButton()
        {
            if (ActionBar != null)
            {
                ActionBar.SetDisplayHomeAsUpEnabled(true);
                ActionBar.SetHomeButtonEnabled(true);
            }
        }

        public abstract void SetViewPermissions();

        public void DisplayNetworkRequiredAlert(int messageRes, int negativeBtnRes)
        {
            var connectivityDialog = new AlertDialog.Builder(this);
            connectivityDialog.SetMessage(GetText(messageRes));
            connectivityDialog.SetNegativeButton(GetText(negativeBtnRes), delegate { });

            // Show the alert dialog to the user and wait for response.
            connectivityDialog.Show();
        }

        /// <summary>
        /// Returns true if connection to the network (internet) exists or false if it doesn't
        /// </summary>
        public bool ConnectedToNetwork
        {
            get
            {
                return _connectivityService.HasConnection();
            }
        }

        /// <summary>
        /// Attempts to force the keyboard to be displayed.
        /// </summary>
        /// <param name="focusedView">The UI element in focus that the keyboard should send keypresses to</param>
        public void ShowKeyboard(View focusedView)
        {
            InputMethodManager imm = (InputMethodManager)this.GetSystemService(InputMethodService);
            imm.ShowSoftInput(focusedView, ShowFlags.Forced);
            imm.ToggleSoftInput(ShowFlags.Implicit, HideSoftInputFlags.ImplicitOnly);
        }

        public void HideKeyboard()
        {
            HideKeyboard(false);
        }

        /// <summary>
        /// THis method hides the soft input window (Keyboard).
        /// </summary>
        /// <param name="force">If true, always hide the keyboard,
        /// if false, only hide if the keyboard is not explicitly shown by the user (user explicitly popped up keyboard).</param>
        public void HideKeyboard(bool force)
        {
            var inputMethodManager = (InputMethodManager)this.GetSystemService(InputMethodService);
            if (Window.CurrentFocus == null)
            {
                return;
            }

            var token = Window.CurrentFocus.WindowToken;
            if (token != null)
            {
                inputMethodManager.HideSoftInputFromWindow(token, force ? HideSoftInputFlags.None : HideSoftInputFlags.ImplicitOnly);
            }
        }

        protected void LoadFragment(Fragment fragment, int holder, string tag)
        {
            var fragmentTx = this.GetFragmentManager().BeginTransaction();
            fragmentTx.Add(holder, fragment, tag);
            fragmentTx.Commit();
        }

        protected void ReplaceFragment(Fragment fragment, int holder, string tag ="")
        {
            var fragmentTx = this.GetFragmentManager().BeginTransaction();
            fragmentTx.Replace(holder, fragment, tag);
            fragmentTx.Commit();
        }
        
        /// <summary>
        /// This method is called when the user needs to be alerted synchronization failed.
        /// </summary>
        public void ShowAlertSynchronizeFailed()
        {
            RunOnUiThread(
                    () =>
                    {
                        Toast.MakeText(this, GetString(Resource.String.failed_to_sync), ToastLength.Long).Show();
                    });
                    }

        /// <summary>
        /// This method is called when the user needs to be alerted synchronization failed.
        /// </summary>
        public void ShowAlertNoInternet()
        {
            RunOnUiThread(() =>
            {
                Toast.MakeText(this, Resource.String.unable_to_sync_internet, ToastLength.Long)
                    .Show();
            });
        }

        public void ShowSomethingWentWrong()
        {
            RunOnUiThread(() =>
            {
                Toast
                    .MakeText(this, Resource.String.something_wrong_try_again, ToastLength.Long)
                    .Show();
            });
        }

        public void ShowToast(int resId)
        {
            RunOnUiThread(() =>
            {
                Toast
                    .MakeText(this, resId, ToastLength.Long)
                    .Show();
            });
        }

        public void HideSnackbar()
        {
            if (_snackbar != null)
            {
                _snackbar.Dismiss();
            }

            _snackbar = null;
        }

        public Snackbar ShowSnackbar(int parentId, int resId)
        {
            View at = FindViewById(parentId);
            
            if (at != null)
            {
                return ShowSnackbar(at, GetString(resId));
            }

            return null;
        }

        public Snackbar ShowSnackbar(View attachToView, string message,Action<View> action = null, int actionTextId = 0 )
        {
            // don't show the message if there already
            if (_snackbar != null)
            {
                return _snackbar;
            }

            _snackbar = Snackbar.Make(
                    attachToView,
                    message,
                    Snackbar.LengthIndefinite);

            if (action != null && actionTextId != 0)
            {
                _snackbar.SetAction(actionTextId, action);
            }

            _snackbar.Show();
            return this._snackbar;
        }

        public void ShowDialog(string title, string message, string positive, string negative, EventHandler<DialogClickEventArgs> positiveHandler, EventHandler<DialogClickEventArgs> negativeHandler)
        {
            var builder = new AlertDialog.Builder(this);
            if (title != null)
            {
                builder.SetTitle(title);
            }

            builder.SetMessage(message).SetPositiveButton(positive, positiveHandler);

            if (negative != null)
            {
                builder.SetNegativeButton(negative, negativeHandler);
            }

            var dialog = builder.Create();
            dialog.Show();
        }

        public void ShowDialog(string message, string positive, string negative,
        EventHandler<DialogClickEventArgs> positiveHandler, EventHandler<DialogClickEventArgs> negativeHandler)
        {
            this.ShowDialog(null, message, positive, negative, positiveHandler, negativeHandler);
        }

        public void ShowDialog(string message, string positive, EventHandler<DialogClickEventArgs> positiveHandler)
        {
            this.ShowDialog(null, message, positive, null, positiveHandler, null);
        }

        protected override void OnStart()
        {
            base.OnStart();
            var setup = MvxAndroidSetupSingleton.EnsureSingletonAvailable(this.ApplicationContext);
            setup.EnsureInitialized();

            SalesApplication.IsInBackground = false;
        }

        protected override void OnPause()
        {
            base.OnPause();
            SalesApplication.IsInBackground = true;
            ApiErrorHandler.UnsetCallbacks();
        }

        protected override void OnStop()
        {
            base.OnStop();
            if (SalesApplication.IsInBackground)
            {
                SalesApplication.Instance.StopLocationUpdates();
            }
        }

        protected string OperatorName
        {
            get
            {
                TelephonyManager telephonyManager = (TelephonyManager)GetSystemService(TelephonyService);
                return telephonyManager.SimOperatorName;
            }
        }
    }
}