using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Android.Views;
using SalesApp.Core.Api;
using SalesApp.Core.Api.Security;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Settings;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Components.UIComponents.CustomInfo;
using Fragment = Android.Support.V4.App.Fragment;

namespace SalesApp.Droid.Adapters.Auth.ResetPin
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true, ParentActivity = typeof(LoginActivityView))]
    public class ResetPinActivity : ActivityBase2
        , CustomDialogFragment.CustomDialogListener
        , ResetPinSuccessfulFragment.ResetPinSuccesfulFragmentListener
    {
        private IConnectivityService _connectivityService = Resolver.Instance.Get<IConnectivityService>();

        private const string RESETPINFAILED_FRAGMENT_TAG = "RESETPINFAILED_FRAGMENT_TAG";
        private const string RESETPINPROGRESS_FRAGMENT_TAG = "RESETPINPROGRESS_FRAGMENT_TAG";
        private const string RESETPINRESULT_FRAGMENT_TAG = "RESETPINRESULT_FRAGMENT_TAG";
        public const string RESETPIN_FRAGMENT_TAG = "RESETPINRESULT_FRAGMENT_TAG";

        private CustomInfoFragment resetPinFailedFragment;
        private CustomInfoFragment resetPinResultInfoFragment;

        private ProgressFragment progressFragment;
        private ResetPinFragment fragmentResetPin;

        private CustomDialogFragment alertinfo;
        private bool _apiCallRunning = false;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.layout_resetpin);

            var trans = GetFragmentManager().BeginTransaction();

            // TODO check why this exists
            alertinfo = new CustomDialogFragment(
             string.Empty,
                this.GetString(Resource.String.pin_reset_no_internet),
                 this.GetString(Resource.String.go_back), null, null);

            string dealerSupportLine = Settings.Instance.DealerSupportLine;
            CustomInfoFragment.Info info = new CustomInfoFragment.Info
            {
                ActionBarTitle = Resources.GetString(Resource.String.pin_reset_fail_title),
                Title = Resources.GetString(Resource.String.pin_reset_fail_title),
                Content = string.Format(Resources.GetString(Resource.String.pin_reset_fail_msg), dealerSupportLine),
                PositiveButtonCaption = Resources.GetString(Resource.String.try_again),
                NegativeButtonCaption = Resources.GetString(Resource.String.cancel_reset)
            };

            resetPinFailedFragment = new CustomInfoFragment();
            resetPinFailedFragment.SetArgument(CustomInfoFragment.InfoKey, info);

            // set event handlers for failed pin
            resetPinFailedFragment.PositiveAction += new CustomInfoFragment.BtnClick(go);
            resetPinFailedFragment.NegativeAction += new CustomInfoFragment.BtnClick(GoBackToLogin);

            // register fragment
            trans.Add(Resource.Id.reset_pin_placeholder, resetPinFailedFragment, RESETPINFAILED_FRAGMENT_TAG);
            trans.AddToBackStack(null);
            trans.Hide(resetPinFailedFragment);

            CustomInfoFragment.Info infoSuccess = new CustomInfoFragment.Info()
            {
                ActionBarTitle = Resources.GetString(Resource.String.pin_reset_done),
                Title = Resources.GetString(Resource.String.pin_reset_done),
                Content = Resources.GetString(Resource.String.pin_reset_success_msg),
                PositiveButtonCaption = Resources.GetString(Resource.String.back_to_login)
            };
            
            resetPinResultInfoFragment = new CustomInfoFragment();
            resetPinResultInfoFragment.SetArgument(CustomInfoFragment.InfoKey, infoSuccess);

            resetPinResultInfoFragment.PositiveAction += new CustomInfoFragment.BtnClick(GoBackToLogin);
            
            trans.Add(Resource.Id.reset_pin_placeholder, resetPinResultInfoFragment, RESETPINRESULT_FRAGMENT_TAG);
            trans.AddToBackStack(null);
            trans.Hide(resetPinResultInfoFragment);

            fragmentResetPin = new ResetPinFragment();
            fragmentResetPin.ButtonNextClicked += fragmentResetPin_ButtonNextClicked;
            trans.Add(Resource.Id.reset_pin_placeholder, fragmentResetPin, RESETPIN_FRAGMENT_TAG);
            trans.AddToBackStack(null);
            trans.Hide(fragmentResetPin);

            progressFragment = new ProgressFragment();
            Bundle arguments = new Bundle();
            arguments.PutString(ProgressFragment.TitleKey, GetString(Resource.String.reset_pin_title));
            arguments.PutString(ProgressFragment.MessageKey, GetString(Resource.String.resetting_pin));
            progressFragment.Arguments = arguments;
            trans.Add(Resource.Id.reset_pin_placeholder, progressFragment, RESETPINPROGRESS_FRAGMENT_TAG);
            trans.AddToBackStack(null);

            trans.Commit();
            go();
            // TODO check why we are adding all the fragments to start with and not only when needed
        }

        public override void SetViewPermissions()
        {
            
        }

        public override void OnBackPressed()
        {
            GoBackToLogin();
        }
        
        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                GoBackToLogin();
                return false;
            }

            return base.OnKeyUp(keyCode, e);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                GoBackToLogin();
                return false;
            }
            else
                return base.OnOptionsItemSelected(item);
        }


        void fragmentResetPin_ButtonCancelClicked(object sender, EventArgs e)
        {
            GoBackToLogin();
        }


        private void ShowFragment(Fragment fragment)
        {
            var trans = GetFragmentManager().BeginTransaction();

            if (fragment != null)
            {
                trans.Hide(progressFragment);
                trans.Hide(resetPinResultInfoFragment);
                trans.Hide(resetPinFailedFragment);
                trans.Hide(fragmentResetPin);
                trans.Show(fragment);
            }
            else
            {
                trans.Remove(progressFragment);
                trans.Remove(resetPinResultInfoFragment);
                trans.Remove(resetPinFailedFragment);
                trans.Remove(fragmentResetPin);
                trans.Remove(alertinfo);
            }

            trans.Commit();
        }

        private void go()
        {
            if (!_connectivityService.HasConnection())
            {
                ShowFragment(resetPinFailedFragment);
            }
            else
            {
                ShowFragment(progressFragment);
                DoResetPin();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
        }


        private string DsrPhone
        {
            get
            {
                return Settings.Instance.DsrPhone;
            }
        }

        private async void DoResetPin()
        {
            string dsrPhone = DsrPhone;
            if (string.IsNullOrEmpty(dsrPhone))
            {

                ShowFragment(fragmentResetPin);
                return;
            }
            else
            {
                await ResetPinApiCall(dsrPhone);
            }

        }

        async Task ResetPinApiCall(string dsrPhone)
        {
            _apiCallRunning = false;
            try
            {
                if (string.IsNullOrEmpty(dsrPhone))
                {
                    dsrPhone = DsrPhone;
                }

                ShowFragment(progressFragment);
              
                ActionBar.SetHomeButtonEnabled(false);

                _apiCallRunning = true;
                ServerResponseDto serverResponse =
                    await new PinResetOperations
                        (
                            
                        ).ResetPinAsync(dsrPhone);

                _apiCallRunning = false;
                ActionBar.SetHomeButtonEnabled(true);


                resetPinResultInfoFragment.SetContent(serverResponse.Title, serverResponse.Message);
                
                // if the response is empty, we consider it not correct
                if (String.IsNullOrEmpty(serverResponse.Message))
                {
                    serverResponse.RequestException = new Exception("No valid response from server.");    
                }
                
                if (serverResponse.RequestException == null)
                {
                    resetPinResultInfoFragment.SetContent(serverResponse.Title, serverResponse.Message, GetString(Resource.String.back_to_login), null);
                    ShowFragment(resetPinResultInfoFragment);
                }
                else
                {
                    ShowFragment(resetPinFailedFragment);
                }

            }
            catch (Exception ex)
            {
                Log.Error("ResetPinActivity", ex.Message + "\n\n" + ex.StackTrace);
                ShowFragment(resetPinFailedFragment);
               
            }
            finally
            {
                _apiCallRunning = false;
            }
            
        }

        async void fragmentResetPin_ButtonNextClicked(object sender, EventArgs e)
        {
            string dsrPhone = DsrPhone;
            if (string.IsNullOrEmpty(dsrPhone))
            {
                GoBackToLogin();
            }
            else
            {
                await ResetPinApiCall(dsrPhone);
            }
        }

        public void onSuccessSelected(int p)
        {
            GoBackToLogin();
        }

        void GoBackToLogin()
        {
            if (!_apiCallRunning)
                this.Finish();
        }

        protected void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode == 2)
            {

            }
        }



        public void onCustomDialogSelected(int selection)
        {

            switch (selection)
            {
                case 1:
                    GoBackToLogin();
                    break;
            }
        }

        //public void onWaitSelected()
        //{
        //    throw new NotImplementedException();
        //}


        public override void InitializeScreen()
        {
            
        }

        public override void RetrieveScreenInput()
        {
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
    }
}
