using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Java.Interop;
using SalesApp.Core.BL.Controllers.Security;
using SalesApp.Core.BL.Models;
using SalesApp.Core.Enums.MultiCountry;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Droid.People.SalesRepresentatives
{
    [Activity()]
    public class DeviceRegistrationStep3 : LoginActivityBase
    {
        private string _countryName;
        private string _countryCode;
        private string _phoneNumber;
        private CountryCodes _countryCodeEnum;
        private TextView _textViewError;
        
        private IConnectivityService _connectivityService = Resolver.Instance.Get<IConnectivityService>();
        private LoginController _loginController;
        private DsrProfile _dsr;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetScreenTitle(GetString(Resource.String.device_registration));

            SetContentView(Resource.Layout.activity_deviceregistration_step3);
            EditPin = FindViewById<EditText>(Resource.Id.editPin);
            TxtPin1 = FindViewById<TextView>(Resource.Id.txtPin1);
            TxtPin2 = FindViewById<TextView>(Resource.Id.txtPin2);
            TxtPin3 = FindViewById<TextView>(Resource.Id.txtPin3);
            TxtPin4 = FindViewById<TextView>(Resource.Id.txtPin4);

            TxtPinArray = new TextView[4];
            TxtPinArray[0] = TxtPin1;
            TxtPinArray[1] = TxtPin2;
            TxtPinArray[2] = TxtPin3;
            TxtPinArray[3] = TxtPin4;

            InitializeScreen();
            SetListeners();
        }

        public override void SetViewPermissions()
        {  
        }

        public bool ConnectionExists
        {
            get { return _connectivityService.HasConnection(); }
        }

        
        [Export("onPreviousButtonClicked")]
        public void onPreviousButtonClicked(View v)
        {

            Intent intent = new Intent(this, typeof(DeviceRegistration_Step_2));
            intent.PutExtra("Country", _countryName);
            intent.PutExtra("CountryCode", _countryCode);
            intent.PutExtra("PhoneNumber", _phoneNumber);
            StartActivity(intent);

        }

        [Export("onLoginButtonClicked")]
        public void onLoginButtonClicked(View v)
        {
            Login(EnteredPin, _phoneNumber, _countryCodeEnum, true);
        }

        public override void ShowLoginResultMessage(string text, bool isInformational)
        {
            // set the type of box to show
            _textViewError.SetBackgroundResource(isInformational
                ? Resource.Drawable.list_box_information
                : Resource.Drawable.list_box_error);

            _textViewError.Text = text;
            Animation fadeInAnimation = AnimationUtils.LoadAnimation(this, Resource.Animation.fade_in);

            _textViewError.Visibility = ViewStates.Visible;
            _textViewError.StartAnimation(fadeInAnimation);

        }

        public override void ContinueToWelcome()
        {
            Intent intent = new Intent(this, typeof(HomeView));
            this.StartActivity(intent);
        }

        private async void Login(string pin, string mobileNumber, CountryCodes country, bool isFirstTime)
        {
            Settings.DsrPhone = mobileNumber;
            Settings.DsrLanguage = LanguagesEnum.EN;
            EnteredPin = pin;

            if (_connectivityService.HasConnection())
            {

                await LoginOnline();
            }
            else
            {
                _textViewError.Visibility = ViewStates.Visible;
                _textViewError.Text = GetText(Resource.String.network_connection_required);
            }
        }

        public override void HideLoginResultMessage()
        {
            _textViewError.Visibility = ViewStates.Invisible;
        }

        public override void InitializeScreen()
        {
            //get values specified in the first and second screen of device registration
            _countryName = Intent.GetStringExtra("Country");
            _countryCode = Intent.GetStringExtra("CountryCode");
            _phoneNumber = Intent.GetStringExtra("PhoneNumber");

            _countryCodeEnum = _countryCode.ToEnumValue<CountryCodes>();

            TextView textViewStepSelectedValue = FindViewById<TextView>(Resource.Id.textViewStepSelectedValue);
            textViewStepSelectedValue.Text = _countryName;
            TextView textViewPhoneRegisteredValue = FindViewById<TextView>(Resource.Id.textViewPhoneRegisteredValue);
            textViewPhoneRegisteredValue.Text = _phoneNumber;

            _textViewError = FindViewById<TextView>(Resource.Id.textViewError);
            LoginButton = FindViewById<Button>(Resource.Id.buttonLogin);
        }

        public override void UpdateScreen()
        {
            //throw new NotImplementedException();
        }

        public override bool Validate()
        {
            throw new NotImplementedException();
        }
    }
}