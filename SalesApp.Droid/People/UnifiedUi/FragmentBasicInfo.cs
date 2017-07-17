using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.Api.Person;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Enums.Syncing;
using SalesApp.Core.Enums.Validation;
using SalesApp.Core.Extensions;
using SalesApp.Core.Validation;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Enums;
using SalesApp.Droid.UI.Utils;
using SalesApp.Droid.UI.Utils.ViewsHelper;
using SalesApp.Droid.UI.Wizardry;
using SalesApp.Droid.UI.Wizardry.Fragments;

namespace SalesApp.Droid.People.UnifiedUi
{
    public abstract class FragmentBasicInfo : WizardStepFragment
    {
        /// <summary>
        /// The key to use for saving the overlay fragment into the bundle
        /// </summary>
        private const string OverLayFragmentBundleKey = "OverLayFragmentBundleKey";

        /// <summary>
        /// The overlay fragment
        /// </summary>
        private FragmentInfo _fragmentInfo;

        protected View View { get; set; }

        protected bool IsAdditionalProduct { get; set; }

        protected Validator Validator { get; set; }

        readonly PeopleDetailsValidater _peopleDetailsValidater = new PeopleDetailsValidater();

        public const string KeyProspectIdBundled = "KeyProspectIdBundled";

        public const string KeyCustomerIdBundled = "KeyCustomerIdBundled";

        public abstract void FillFromExistingRecord(Person person);

        public abstract void BindViews();

        private ProspectSearchResult _prospectBeingConverted;

        private const string ProspectBeingConvertedBundleKey = "ProspectBeingConvertedBundleKey";

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
        }

        protected void WriteProspectIfConversion()
        {
            if (WizardActivity.BundledItems == null || !WizardActivity.BundledItems.ContainsKey(KeyProspectIdBundled))
            {
                return;
            }

            string prospectJson = WizardActivity.BundledItems[KeyProspectIdBundled].ToString();
            this._prospectBeingConverted = JsonConvert.DeserializeObject<ProspectSearchResult>(prospectJson);
            if (this._prospectBeingConverted == null)
            {
                return;
            }

            SalesApp.Core.BL.Models.People.Customer c = new SalesApp.Core.BL.Models.People.Customer
            {
                FirstName = this._prospectBeingConverted.FirstName,
                LastName = this._prospectBeingConverted.LastName,
                Phone = this._prospectBeingConverted.Phone,
                DsrPhone = this._prospectBeingConverted.DsrPhone
            };

            FillFromExistingRecord(c);

            WizardActivity.IsProspectConversion = true;
            View.FindViewById<EditText>(Resource.Id.edtPhone).Enabled = false;

            /*
                Fixes issue 18879 - Missing Customer ID number
            */
            WizardActivity.BundledItems.Remove(KeyProspectIdBundled);
        }

        public virtual void RegisterAdditionalProductFromProspectWizard()
        {
            if (WizardActivity.BundledItems == null || !WizardActivity.BundledItems.ContainsKey(KeyCustomerIdBundled))
            {
                return;
            }

            string customerJson = WizardActivity.BundledItems[KeyCustomerIdBundled].ToString();
            Person customerSearch = JsonConvert.DeserializeObject<Person>(customerJson);

            if (customerSearch == null)
            {
                return;
            }

            FillFromExistingRecord(customerSearch);

            View.FindViewById<EditText>(Resource.Id.edtPhone).Enabled = false;
        }
        private void SetIdFieldVisibility()
        {
            Activity.RunOnUiThread(() =>
            {
                View.FindViewById<LinearLayout>(Resource.Id.linId).Visibility =
                WizardActivity.WizardType == WizardTypes.ProspectRegistration ?
                ViewStates.Gone : ViewStates.Visible;
            });
        }

        public void ValidatePhoneNumber(View vw)
        {
            string phone = ((EditText)vw).Text;
            int res = PhoneNumberIsValid(phone);

            if (res == 0)
            {
                return;
            }

            TextView tv = Activity.FindViewById<TextView>(Resource.Id.tvPhoneError);
            tv.SetText(res);
        }

        public async Task<bool> ShowOverlayIfPhoneNumberDuplicate(string phone, string nationalId = null)
        {
            ErrorFilterFlags flag = ErrorFilterFlags.DisableErrorHandling;
            Person person;
            SalesApp.Core.BL.Models.People.Customer customer = await new CustomersController().GetPersonIfExists(phone, nationalId, filterFlags: flag, checkDuplicate: false);
            SalesApp.Core.BL.Models.People.Prospect prospect = null;

            if (customer == null)
            {
                prospect = await new ProspectsController().GetPersonIfExists(phone, filterFlags: flag, checkDuplicate: false);
                person = JsonConvert.DeserializeObject<Person>(JsonConvert.SerializeObject(prospect));
            }
            else
            {
                if (customer.PersonType == PersonTypeEnum.Prospect)
                {
                    prospect =
                        JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Prospect>(
                            JsonConvert.SerializeObject(customer));
                }
                else
                {
                    string urlParam = customer.Phone + "&foradditionalproduct=true";
                    CustomerStatus customerStatus = await new CustomerStatusApi().GetAsync(urlParam);

                    if (customerStatus != null)
                    {
                        customer.AccountStatus = customerStatus.AccountStatus;
                    }
                }

                person = JsonConvert.DeserializeObject<Person>(JsonConvert.SerializeObject(customer));
            }

            if (person == null)
            {
                return false;
            }

            this._fragmentInfo = new FragmentInfo();
            this._fragmentInfo.SetArgument(FragmentInfo.ResourceIdBundleKey, Resource.Layout.fragment_unified_existing_person);

            this._fragmentInfo.ViewCreated += (sender, args) =>
            {
                ViewsHelper<Person> viewsHelper = new ViewsHelper<Person>(Activity as ActivityBase,
                    _fragmentInfo.InflatedView);

                viewsHelper.BindEvent
                    (
                        BindableEvents.OnClick
                        , Resource.Id.linEditCustomer
                        , linEditCustomer =>
                        {
                            WizardActivity.HideOverlay(true);
                        });

                viewsHelper.BindEvent(
                BindableEvents.OnClick,
                Resource.Id.linAddProduct,
                linAddProduct =>
                {
                    // if doing prospect registration, do conversion to customer
                    if (WizardActivity.WizardType == WizardTypes.ProspectRegistration)
                    {
                        // do conversion to customer
                        if (person.PersonType == PersonTypeEnum.Prospect)
                        {
                            ProspectSearchResult psr = JsonConvert.DeserializeObject<ProspectSearchResult>(JsonConvert.SerializeObject(prospect));
                            Dictionary<string, object> bundledItems = new Dictionary<string, object>();
                            bundledItems.Add(KeyProspectIdBundled, psr);

                            WizardLauncher.Launch(Activity, WizardTypes.CustomerRegistration, WizardActivity.StartPoint, bundledItems);
                        }
                        else
                        {
                            // posible additional product
                            if (customer != null)
                            {
                                WizardActivity.HideOverlay(true);
                                IsAdditionalProduct = true;
                                WizardActivity.IsProspectConversion = true;
                                CustomerSearchResult csr = JsonConvert.DeserializeObject<CustomerSearchResult>(JsonConvert.SerializeObject(customer));
                                Dictionary<string, object> bundledItems = new Dictionary<string, object>();
                                bundledItems.Add(KeyCustomerIdBundled, csr);

                                WizardLauncher.Launch(Activity, WizardTypes.CustomerRegistration, WizardActivity.StartPoint, bundledItems);
                            }
                        }
                    }
                    else
                    {
                        // customer registration
                        if (person.PersonType == PersonTypeEnum.Prospect)
                        {
                            // auto fill the details
                            FillFromExistingRecord(prospect);
                            WizardActivity.IsProspectConversion = true;
                        }
                        else
                        {
                            // auto fill the details
                            FillFromExistingRecord(customer);
                        }

                        WizardActivity.HideOverlay(true);
                        IsAdditionalProduct = true; // though this may be cheating ;)
                        WizardActivity.GoNext();
                    }
                });

                viewsHelper.Write(Resource.Id.tvPersonName, person.FullName);
                viewsHelper.Write(Resource.Id.tvPhone, person.Phone);
                viewsHelper.WriteBoundViews(customer);

                if (customer != null && customer.AccountStatus != null && !customer.AccountStatus.Equals("Active"))
                {
                    ShowConversionButton(_fragmentInfo.InflatedView, ViewStates.Gone);
                    ShowPersonStatus(_fragmentInfo.InflatedView);
                }

                if (person.PersonType == PersonTypeEnum.Prospect)
                {
                    // show conversion button
                    ShowConversionButton(_fragmentInfo.InflatedView, ViewStates.Visible);
                }
            };

            WizardActivity.ShowOverlay(_fragmentInfo, false);
            return true;
        }

        public async Task<bool> ShowOverlayIfOffline(SalesApp.Core.BL.Models.People.Customer customer)
        {
            this._fragmentInfo = new FragmentInfo();
            this._fragmentInfo.SetArgument(FragmentInfo.ResourceIdBundleKey, Resource.Layout.fragment_choose_registration_type);

            this._fragmentInfo.ViewCreated += (sender, args) =>
            {
                ViewsHelper<Person> viewsHelper = new ViewsHelper<Person>(Activity as ActivityBase,
                    _fragmentInfo.InflatedView);

                viewsHelper.BindEvent
                    (
                        BindableEvents.OnClick
                        , Resource.Id.linFirstTimeProduct
                        , linFirstTimeProduct =>
                        {
                            WizardActivity.HideOverlay(false);
                            IsAdditionalProduct = false;
                            customer.IsAdditionalProduct = IsAdditionalProduct;
                            WizardActivity.ButtonNextEnabled = true;

                            string serializedData = JsonConvert.SerializeObject(customer);
                            WizardActivity.CurrentFragment.SetData(serializedData);
                            WizardActivity.Go(true);
                        });

                viewsHelper.BindEvent(
                BindableEvents.OnClick,
                Resource.Id.linAddProduct,
                linAddProduct =>
                {
                    WizardActivity.HideOverlay(false);
                    IsAdditionalProduct = true;
                    customer.IsAdditionalProduct = IsAdditionalProduct;
                    WizardActivity.ButtonNextEnabled = true;

                    string serializedData = JsonConvert.SerializeObject(customer);
                    WizardActivity.CurrentFragment.SetData(serializedData);
                    WizardActivity.Go(true);
                });

                viewsHelper.Write(Resource.Id.tvPersonName, customer.FullName);
                viewsHelper.Write(Resource.Id.tvPhone, customer.Phone);
                viewsHelper.WriteBoundViews(customer);
            };

            WizardActivity.ShowOverlay(_fragmentInfo, false);
            return false;
        }

        private void ShowPersonStatus(View parent)
        {
            LinearLayout personStatus = parent.FindViewById<LinearLayout>(Resource.Id.personStatus);
            if (personStatus == null)
            {
                return;
            }

            personStatus.Visibility = ViewStates.Visible;
            TextView txtAccountStatus = personStatus.FindViewById<TextView>(Resource.Id.txt_person_account_status);
            txtAccountStatus.SetText(Resource.String.account_status);
            txtAccountStatus.SetAllCaps(true);
            TextView txtOverlayTitle = parent.FindViewById<TextView>(Resource.Id.unified_is_this_the_person);
            txtOverlayTitle.Text = GetString(Resource.String.account_status_rejected);
        }

        private void ShowConversionButton(View parent, ViewStates visible)
        {
            LinearLayout addProductButton = parent.FindViewById<LinearLayout>(Resource.Id.linAddProduct);
            if (addProductButton == null)
            {
                return;
            }

            addProductButton.Visibility = visible;
            TextView titleTextView = addProductButton.FindViewById<TextView>(Resource.Id.txt_additional_product);
            titleTextView.SetText(Resource.String.existing_prospect_register_customer);
            titleTextView.SetAllCaps(true);

            TextView detailsTextView = addProductButton.FindViewById<TextView>(Resource.Id.txt_additional_product_details);
            detailsTextView.SetText(Resource.String.existing_prospect_go_cust_reg);
        }

        private void RegisterViewsForValidation()
        {
            Validator = new Validator(Activity)
                .Add(
                   Resource.Id.edtPhone,
                   Resource.Id.tvPhoneError,
                   new Func<string, int>[] { PhoneNumberIsValid })
               .Add(
                    Resource.Id.edtLastName,
                    Resource.Id.tvLastNameError,
                    new Func<string, int>[]
                    {
                        name =>
                        {
                            TextView tv = Activity.FindViewById<TextView>(Resource.Id.tvLastNameError);
                            SetViewVisibility(tv, ViewStates.Visible);
                            var res = _peopleDetailsValidater.ValidateName(name);
                            switch (res)
                            {
                                case PersonNameValidationResultsEnum.NameBlank:
                                    return Resource.String.unified_last_name_empty;
                                case PersonNameValidationResultsEnum.NameTooLong:
                                    return Resource.String.unified_last_name_too_long;
                            }

                            SetViewVisibility(tv, ViewStates.Gone);
                            return 0;
                        }
                    }
               )
               .Add
               (
                   Resource.Id.edtFirstName
                   , Resource.Id.tvFirstNameError
                   , new Func<string, int>[]
                    {
                        name =>
                        {
                            TextView tv = Activity.FindViewById<TextView>(Resource.Id.tvIdError);
                            SetViewVisibility(tv, ViewStates.Visible);
                            var res = _peopleDetailsValidater.ValidateName(name);
                            switch (res)
                            {
                                case PersonNameValidationResultsEnum.NameBlank:
                                    return Resource.String.unified_first_name_empty;
                                case PersonNameValidationResultsEnum.NameTooLong:
                                    return Resource.String.unified_first_name_too_long;
                            }

                            SetViewVisibility(tv, ViewStates.Gone);
                            return 0;
                        }
                    }
               )
               .Add
               (
                   Resource.Id.edtId
                   , Resource.Id.tvIdError
                   , new Func<string, int>[]
                    {
                        idValue =>
                            {
                            TextView tv = Activity.FindViewById<TextView>(Resource.Id.tvIdError);
                            SetViewVisibility(tv, ViewStates.Visible);
                            if (idValue.IsBlank())
                            {
                                this.ShowIdError();
                                return Resource.String.unified_id_null;
                            }
                            if (idValue.Length < PeopleDetailsValidater.NationalIdLength)
                            {
                                this.ShowIdError();
                                return Resource.String.unified_id_too_short;
                            }

                            SetViewVisibility(tv, ViewStates.Gone);
                            return 0;
                        }
                    }
               );
        }

        public bool ValidateHasContent(string content)
        {
            return !string.IsNullOrEmpty(content);
        }

        public bool PhoneNumberLooksOk(string phoneNumber)
        {
            return PhoneNumberIsValid(phoneNumber) == 0;
        }

        int PhoneNumberIsValid(string phoneNumber)
        {
            var validationResult = _peopleDetailsValidater.ValidatePhoneNumber(phoneNumber);
            TextView tv = Activity.FindViewById<TextView>(Resource.Id.tvPhoneError);
            Activity.RunOnUiThread(() => tv.Visibility = ViewStates.Visible);
            switch (validationResult)
            {
                case PhoneValidationResultEnum.InvalidFormat:
                    return Resource.String.unified_phone_validation_invalid_format;
                case PhoneValidationResultEnum.InvalidCharacters:
                    return Resource.String.unified_phone_validation_bad_chars;
                case PhoneValidationResultEnum.NullEntry:
                    return Resource.String.unified_phone_validation_null;
                case PhoneValidationResultEnum.NumberOk:
                    tv.Visibility = ViewStates.Gone;
                    return 0;
                case PhoneValidationResultEnum.NumberTooShort:
                    return Resource.String.unified_phone_validation_short;
                case PhoneValidationResultEnum.NumberTooLong:
                    return Resource.String.unified_phone_validation_long;
            }

            Activity.RunOnUiThread(() => tv.Visibility = ViewStates.Gone);
            return 0;
        }

        private void ShowIdError()
        {
            ScrollView scrollView = Activity.FindViewById<ScrollView>(Resource.Id.scrlContent);
            if (scrollView == null)
            {
                return;
            }

            // get id error text view
            TextView errorTv = scrollView.FindViewById<TextView>(Resource.Id.tvIdError);
            if (errorTv == null)
            {
                return;
            }

            errorTv.RequestFocus();
        }

        public abstract void SetData();

        public override int StepTitle
        {
            get
            {
                Logger.Debug("Wizard = " + WizardActivity);
                return Resource.String.unified_basic_information;
            }
        }

        protected bool EnableDuplicateChecking
        {
            get
            {
                if (IsAdditionalProduct)
                {
                    return false;
                }

                if (WizardActivity.IsProspectConversion)
                {
                    if (this._prospectBeingConverted == null)
                    {
                        return false;
                    }

                    if (this._prospectBeingConverted.SyncRecord == null)
                    {
                        return false;
                    }

                    if (this._prospectBeingConverted.SyncRecord.Status == RecordStatus.InError)
                    {
                        return true;
                    }

                    return false;
                }

                return true;
            }
        }

        public override bool BeforeGoNext()
        {
            return true;
        }

        public abstract void WriteViews();

        public override void OnResume()
        {
            base.OnResume();
            if (View == null)
            {
                return;
            }

            RegisterViewsForValidation();
            BindViews();
            WriteViews();
            WriteProspectIfConversion();
            RegisterAdditionalProductFromProspectWizard();
            SetIdFieldVisibility();
            this.WizardActivity.ButtonNext.Visibility = ViewStates.Visible;
        }

        /// <summary>
        /// Saves the fragment state into a bundle
        /// </summary>
        /// <param name="outState">The bundle</param>
        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            outState.PutString(ProspectBeingConvertedBundleKey, JsonConvert.SerializeObject(this._prospectBeingConverted));
            if (this._fragmentInfo != null)
            {
                this.Activity.SupportFragmentManager.PutFragment(outState, OverLayFragmentBundleKey, this._fragmentInfo);
            }
        }

        public abstract void CreateView();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            this.View = inflater.Inflate(Resource.Layout.fragment_unified_basic_info, container, false);

            this.CreateView();

            if (savedInstanceState != null)
            {
                this._fragmentInfo = this.Activity.SupportFragmentManager.GetFragment(savedInstanceState, OverLayFragmentBundleKey) as FragmentInfo;

                if (this._fragmentInfo != null)
                {
                    this.WizardActivity.ShowOverlay(this._fragmentInfo, false);
                }

                if (savedInstanceState.ContainsKey(ProspectBeingConvertedBundleKey))
                {
                    this._prospectBeingConverted =
                        JsonConvert.DeserializeObject<ProspectSearchResult>(
                            savedInstanceState.GetString(ProspectBeingConvertedBundleKey));
                }
            }

            return this.View;
        }


    }
}