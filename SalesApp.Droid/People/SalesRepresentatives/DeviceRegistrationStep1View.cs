using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Util;
using SalesApp.Core.Enums.MultiCountry;
using SalesApp.Core.Services.Settings;
using SalesApp.Core.ViewModels.Security;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Framework;

namespace SalesApp.Droid.People.SalesRepresentatives
{
    [Activity()]
    public class DeviceRegistrationStep1View : MvxViewBase<DeviceRegistrationStep1ViewModel>
    {
        private string _countryName;
        private CountryCodes _countryCode;
        private TextView _textViewError;
        private Spinner _spinner;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetScreenTitle(GetString(Resource.String.device_registration));
            // Create your application here
            SetContentView(Resource.Layout.activity_deviceregistration_step1);

            _spinner = FindViewById<Spinner>(Resource.Id.spinnerCountries);

            _spinner.ItemSelected += SpinnerItemSelected;

            var items = this.Resources.GetStringArray(Resource.Array.countries_array);
            var adapter = new DefaultSpinnerAdapter().GetAdapter(items, this);

            _spinner.Adapter = adapter;

            _textViewError = FindViewById<TextView>(Resource.Id.textViewError);
        }

        public override void SetViewPermissions()
        {
        }

        [Java.Interop.Export("OnNextButtonClicked")]
        public void OnNextButtonClicked(View v)
        {
            if (_spinner.SelectedItemId > 0)
            {
                _textViewError.Visibility = ViewStates.Invisible;

                Intent intent = new Intent(this, typeof(DeviceRegistration_Step_2));
                intent.PutExtra("Country", _countryName);
                intent.PutExtra("CountryCode", _countryCode.ToString());
                intent.PutExtra("CountrySelectedIndex", _spinner.SelectedItemId);
                StartActivity(intent);
            }
            else
            {
                _textViewError.Visibility = ViewStates.Visible;
                _textViewError.Text = GetText(Resource.String.select_country_error);
            }
        }

        private void SpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            _countryName = string.Format("{0}", spinner.GetItemAtPosition(e.Position));
            // TODO find a better way to link the codes to the names, now codes will not correspond to names when order changes
            switch (e.Position)
            {
                case 0: //default option to select a country
                    this.ViewModel.CountrySelected = false;
                    break;
                case 1: //Kenya
                    _countryCode = CountryCodes.KE;
                    this.ViewModel.CountrySelected = true;
                    break;
                case 2: //Uganda
                    _countryCode = CountryCodes.UG;
                    this.ViewModel.CountrySelected = true;
                    break;
                case 3: //Ghana
                    _countryCode = CountryCodes.GH;
                    this.ViewModel.CountrySelected = true;
                    break;
                case 4: //Ghana
                    _countryCode = CountryCodes.TZ;
                    this.ViewModel.CountrySelected = true;
                    break;
            }

            if (e.Position > 0)
            {
                Settings.Instance.DsrCountryCode = _countryCode;
                SetAppLanguage();
            }
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
    }
}