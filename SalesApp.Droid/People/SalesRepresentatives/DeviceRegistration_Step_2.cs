using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Interop;
using SalesApp.Core.Enums.MultiCountry;
using SalesApp.Core.Enums.Validation;
using SalesApp.Core.Extensions;
using SalesApp.Core.Validation;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid.People.SalesRepresentatives
{
    [Activity()]
    public class DeviceRegistration_Step_2 : ActivityBase
    {
        private string _countryName;
        private string _countryCode;
        private int _countrySelectedIndex;
        private CountryCodes _CountryCodeEnum;
        private string _phoneNumber;
        private EditText _editTextPhoneNumber;
        private TextView _textViewError;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetScreenTitle(GetString(Resource.String.device_registration));
            // Create your application here
            SetContentView(Resource.Layout.activity_deviceregistration_step2);

            //get values specified in the first screen of device registration
            _countryName = Intent.GetStringExtra("Country");
            _countryCode = Intent.GetStringExtra("CountryCode");
            _phoneNumber = Intent.GetStringExtra("PhoneNumber");
            //_countrySelectedIndex = Intent.GetIntExtra("CountrySelectedIndex",1);

            _CountryCodeEnum = _countryCode.ToEnumValue<CountryCodes>();

            TextView textViewStepSelectedValue = FindViewById<TextView>(Resource.Id.textViewStepSelectedValue);
            textViewStepSelectedValue.Text = _countryName;

            _textViewError = FindViewById<TextView>(Resource.Id.textViewError);

            _editTextPhoneNumber = FindViewById<EditText>(Resource.Id.editTextPhoneNumber);

            if (_phoneNumber != null)
            {
                _editTextPhoneNumber.Text = _phoneNumber;
            }

        }

        public override void SetViewPermissions()
        {
            
        }

        [Export("onNextButtonClicked")]
        public void onNextButtonClicked(View v)
        {
            // validate phone number
            var validator = new PeopleDetailsValidater();
            PhoneValidationResultEnum phoneValidationResult = validator.ValidatePhoneNumber(this._editTextPhoneNumber.Text);

            if (phoneValidationResult != PhoneValidationResultEnum.NumberOk)
            {
                // _textViewError.SetTextColor(Color.Red);
                int errorRes = 0;
                switch (phoneValidationResult)
                {
                    case PhoneValidationResultEnum.InvalidCharacters:
                        errorRes = Resource.String.pin_validation_bad_chars;
                        break;
                    case PhoneValidationResultEnum.InvalidFormat:
                        errorRes = Resource.String.phone_validation_invalid_format;
                        break;
                    case PhoneValidationResultEnum.NullEntry:
                        errorRes = Resource.String.phone_validation_null;
                        break;
                    case PhoneValidationResultEnum.NumberTooLong:
                        errorRes = Resource.String.pin_validation_long;
                        break;
                    case PhoneValidationResultEnum.NumberTooShort:
                        errorRes = Resource.String.pin_validation_short;
                        break;

                }
                _textViewError.Visibility = ViewStates.Visible;
                _textViewError.Text = GetText(errorRes);
                //parent.ShowKeyboard(edtPhone);
                return;
            }

            _textViewError.Text = "";
            _textViewError.Visibility = ViewStates.Invisible;

            //pass countryName, CountryCode and Phone Number to next screen
            Intent intent = new Intent(this, typeof(DeviceRegistrationStep3));
            intent.PutExtra("Country", _countryName);
            intent.PutExtra("CountryCode", _countryCode);
            intent.PutExtra("PhoneNumber", _editTextPhoneNumber.Text);

            StartActivity(intent);
        }

        [Export("onPreviousButtonClicked")]
        public void onPreviousButtonClicked(View v)
        {
            Intent intent = new Intent(this, typeof(DeviceRegistrationStep1View));
            //intent.PutExtra("CountrySelectedIndex", _countrySelectedIndex);
            StartActivity(intent);
        }
    }
}