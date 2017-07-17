using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Newtonsoft.Json;
using SalesApp.Core.BL;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.Chama;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Events.CustomerRegistration;
using SalesApp.Core.Exceptions.Database;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Person;
using SalesApp.Core.Services.Person.Customer;
using SalesApp.Core.Services.Settings;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Enums;
using SalesApp.Droid.People.Customers;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.UI.Wizardry;

namespace SalesApp.Droid.People.UnifiedUi.Customer
{
    /// <summary>
    /// This is the customer info confirmation screen
    /// </summary>
    public class FragmentCustomerConfirmationScreen : FragmentConfirmationScreen , IOverlayParent
    {
        private Action cachedAction;

        /// <summary>
        /// Maximum number of SMS send tries
        /// </summary>
        private int maxSmsTries;

        /// <summary>
        /// The registration information
        /// </summary>
        private SalesApp.Core.BL.Models.People.Customer personRegistrationInfo;

        /// <summary>
        /// The customer service
        /// </summary>
        private CustomerService customerService;

        /// <summary>
        /// SMS failed overlay
        /// </summary>
        private WizardOverlayFragment _registrationFinishedOverlay;

        private readonly string SMSRetries = "SMS_RETRIES";
        private readonly string SUCCESS_KEY = "SUCCESS_KEY";

        private CustomerService CustomerService
        {
            get
            {
                if (this.customerService == null)
                {
                    this.customerService = new CustomerService();

                    this.customerService.RegistrationAttempted += this.RegistrationAttempted;
                    this.customerService.RegistrationCompleted += this.RegistrationCompleted;
                }

                return this.customerService;
            }
        }

        /// <summary>
        /// Gets the step title
        /// </summary>
        public override int StepTitle
        {
            get { return Resource.String.unified_customer_information_title; }
        }

        /// <summary>
        /// ENUM represents different saving progress statuses
        /// </summary>
        /*protected enum SavingProgress
        {
            /// <summary>
            /// Not started
            /// </summary>
            NotStarted,

            /// <summary>
            /// Saving in progress
            /// </summary>
            Saving,

            /// <summary>
            /// Saving succeeded
            /// </summary>
            Succeeded,

            /// <summary>
            /// Saving failed
            /// </summary>
            Failed,

            /// <summary>
            /// Saving completed
            /// </summary>
            Done
        }*/

        /// <summary>
        /// Gets the customer that we are selecting the product for
        /// </summary>
        protected override Lead Lead
        {
            get
            {
                return this.personRegistrationInfo;
            }
        }

        protected override List<GroupKeyValue> ConfirmationDetials
        {
            get
            {
                string nameLabel = this.GetString(Resource.String.unified_name);
                string phoneLabel = this.GetString(Resource.String.unified_phone);
                string productLabel = this.GetString(Resource.String.unified_product);
                string idLabel = this.GetString(Resource.String.unified_id);
                
                List<GroupKeyValue> confirmationDetails = new List<GroupKeyValue>
                {
                    new GroupKeyValue { Key = nameLabel, Name = this.Lead.FullName },
                    new GroupKeyValue { Key = phoneLabel, Name = this.Lead.Phone },
                    new GroupKeyValue { Key = productLabel, Name = this.Lead.Product.DisplayName },
                    new GroupKeyValue { Key = idLabel, Name = this.personRegistrationInfo.NationalId }
                };

                if (this.Lead.GroupInfo == null)
                {
                    return confirmationDetails;
                }

                foreach (var groupInfo in this.Lead.GroupInfo)
                {
                    confirmationDetails.Add(groupInfo);
                }

                return confirmationDetails;
            }
        }

        /// <summary>
        /// Gets or sets the saving progress status
        /// </summary>
        // protected SavingProgress SavingProgressStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not registration is in progress
        /// </summary>
        protected bool RegistrationInProgress { get; set; }

        /// <summary>
        /// Creates the view for this fragment
        /// </summary>
        /// <param name="inflater">The inflator</param>
        /// <param name="container">The container</param>
        /// <param name="savedInstanceState">The saved state</param>
        /// <returns>The view</returns>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            //this.FragmentView.FindViewById<TextView>(Resource.Id.tvId).Text = this.personRegistrationInfo.NationalId;
            // this.RowScore.Visibility = ViewStates.Gone;

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Customer confirmation");

            return this.FragmentView;
        }

        /// <summary>
        /// Creates this fragment
        /// </summary>
        /// <param name="savedInstanceState">The saved state</param>
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
                CustomerService.SmsCurrentTry = savedInstanceState.GetInt(SMSRetries);
                CustomerService.RegistrationSuccessful = savedInstanceState.GetBoolean(SUCCESS_KEY);
            }
        }

        /// <summary>
        /// Save our current state into a bundle
        /// </summary>
        /// <param name="outState">The bundle</param>
        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString(BundledRegistrationInfo, JsonConvert.SerializeObject(this.personRegistrationInfo));
            //outState.PutInt(SMSRetries,CustomerService.SmsCurrentTry);
            outState.PutBoolean(SUCCESS_KEY,CustomerService.RegistrationSuccessful);
        }

        /// <summary>
        /// Load the registration data from a JSON string
        /// </summary>
        /// <param name="serializedString">The JSON string</param>
        public override void SetData(string serializedString)
        {
            if (!serializedString.IsBlank())
            {
                this.personRegistrationInfo = JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Customer>(serializedString);
                this.personRegistrationInfo.Groups = JsonConvert.SerializeObject(this.personRegistrationInfo.GroupInfo);

                if (this.personRegistrationInfo.Product == null)
                {
                    this.personRegistrationInfo.Product = new Product();
                }
            }
        }

        /// <summary>
        /// Get the registration data
        /// </summary>
        /// <returns>The string representation of the data</returns>
        public override string GetData()
        {
            return JsonConvert.SerializeObject(this.personRegistrationInfo);
        }

        /// <summary>
        /// Called to finish the wizard and return
        /// </summary>
        public override void FinishWizard()
        {
            this.Save();
        }

        /// <summary>
        /// Saves the registration info
        /// </summary>
        private void Save()
        {
            if (this.personRegistrationInfo.Product == null)
            {
                return;
            }

            this.personRegistrationInfo.DsrPhone = Settings.Instance.DsrPhone;

            this.WizardActivity.ShowWaitInfo(Resource.String.customer_registration, Resource.String.sending_information);

            this.RegistrationInProgress = true;

            Task.Run(
                    async () =>
                    {
                        await this.CustomerService.RegisterCustomer(this.personRegistrationInfo);
                    });
        }

        private void RegistrationByFallbackCompleted(CustomerRegistrationCompletedEventArgs e)
        {
            string posButtonTxt = this.WizardActivity.GetString(Resource.String.add_new_customer);

            this._registrationFinishedOverlay = new RegistrationFinishedFragment();
            string message =
                string.Format(this.WizardActivity.GetString(Resource.String.unified_sms_registration_failed_n_tries), 3);
            Bundle arguments = new Bundle();
            arguments.PutBoolean(RegistrationFinishedFragment.WasRegistrationKey, true);
            arguments.PutBoolean(RegistrationFinishedFragment.SuccessKey, e.Succeeded);
            arguments.PutString(RegistrationFinishedFragment.MessageKey, message);
            arguments.PutString(RegistrationFinishedFragment.BtnPositiveKey, posButtonTxt);
            arguments.PutString(RegistrationFinishedFragment.BtnNegativeKey, this.OverlayNegativeButtonText);
            arguments.PutString(RegistrationFinishedFragment.IntentStartPointKey,
                this.ActivityBase.StartPointIntent.ToString());
            int maxFallbackTries = Settings.Instance.MaxFallbackRetries;
            arguments.PutInt(RegistrationFinishedFragment.Retries, maxFallbackTries);
            this._registrationFinishedOverlay.Arguments = arguments;

            this.WizardActivity.ShowOverlay(this._registrationFinishedOverlay, true);
        }

        private void RegistrationByFallbackFailed(int currentAttempt)
        {
            string posButtonTxt = this.WizardActivity.GetString(Resource.String.try_again);
            string negButtonTxt = this.WizardActivity.GetString(Resource.String.cancel_registration);

            string message = string.Format(this.WizardActivity.GetString(Resource.String.unified_sms_registration_failed_n_tries), currentAttempt);

            this._registrationFinishedOverlay = new RegistrationFinishedFragment();

            Bundle arguments = new Bundle();
            arguments.PutBoolean(RegistrationFinishedFragment.WasRegistrationKey, true);
            arguments.PutBoolean(RegistrationFinishedFragment.SuccessKey, false);
            arguments.PutString(RegistrationFinishedFragment.MessageKey, message);
            arguments.PutString(RegistrationFinishedFragment.BtnPositiveKey, posButtonTxt);
            arguments.PutString(RegistrationFinishedFragment.BtnNegativeKey, negButtonTxt);
            arguments.PutString(RegistrationFinishedFragment.IntentStartPointKey, this.ActivityBase.StartPointIntent.ToString());
            arguments.PutInt(RegistrationFinishedFragment.Retries, currentAttempt);
            this._registrationFinishedOverlay.Arguments = arguments;
            this.WizardActivity.ShowOverlay(this._registrationFinishedOverlay, true);
        }

        private void RegistrationByDataFailed(int currentAttempt)
        {
            string message = this.WizardActivity.GetString(Resource.String.not_able_to_send);
            string messageDetail = string.Format(this.WizardActivity.GetString(Resource.String.retrying), currentAttempt);
            this.WizardActivity.ShowWaitInfo(message, messageDetail);
        }

        private void RegistrationByDataCompleted(CustomerRegistrationCompletedEventArgs e)
        {
            this._registrationFinishedOverlay = new RegistrationFinishedFragment();
            
            Bundle arguments = new Bundle();
            arguments.PutBoolean(RegistrationFinishedFragment.WasRegistrationKey, false);
            arguments.PutBoolean(RegistrationFinishedFragment.SuccessKey, e.Succeeded);
            arguments.PutString(RegistrationFinishedFragment.IntentStartPointKey, this.ActivityBase.StartPointIntent.ToString());
            arguments.PutString(RegistrationFinishedFragment.BtnNegativeKey, this.OverlayNegativeButtonText);
            this._registrationFinishedOverlay.Arguments = arguments;

            this.WizardActivity.ShowOverlay(this._registrationFinishedOverlay, true);
        }

        /// <summary>
        /// Method is called when customer registration was attempted but failed.
        /// The <paramref name="e" /> has an integer indicating the attempt number .
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event</param>
        private void RegistrationAttempted(object sender, CustomerRegistrationAttemptedEventArgs e)
        {
            if(Activity == null)
            {
                cachedAction = () => { RegistrationAttempted(sender, e); };
                return;
            }

            if (e.Channel == DataChannel.Fallback)
            {
                RegistrationByFallbackFailed(e.CurrentAttempt);
            }
            else
            {
                RegistrationByDataFailed(e.CurrentAttempt);
            }

            this.WizardActivity.HideWait();
        }

        /// <summary>
        /// Method is called when customer registration is attempted
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event</param>
        private async void RegistrationCompleted(object sender, CustomerRegistrationCompletedEventArgs e)
        {
            if (Activity == null)
            {
                cachedAction  = () =>  { RegistrationCompleted(sender, e); };
                return;
            }

            await this.SaveCustomerToDevice(e);
            await this.MarkReadyForUpload(e);
            this.WizardActivity.HideWait();

            if (e.Channel == DataChannel.Fallback)
            {
                this.RegistrationByFallbackCompleted(e);
            }
            else
            {
                this.RegistrationByDataCompleted(e);
            }

            this.CustomerService.RegistrationAttempted -= this.RegistrationAttempted;
            this.CustomerService.RegistrationCompleted -= this.RegistrationCompleted;
        }

        public override void OnResume()
        {
            base.OnResume();

            Task.Run(
                async () =>
                {
                    if(cachedAction != null)
                    {
                        cachedAction();
                        cachedAction = null;
                    }
                });
        }

        /// <summary>
        /// Called to save a customer to the device
        /// </summary>
        /// <returns>An empty task</returns>
        private async Task SaveCustomerToDevice(CustomerRegistrationCompletedEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            this.personRegistrationInfo.Channel = e.Channel;

            //if we have a successfully generated customer registration Id from the API, set the corresponding property in the Customer Model
            if (e.RegistrationId != Guid.Empty)
            {
                this.personRegistrationInfo.RegistrationId = e.RegistrationId;
            }

            try
            {
                SalesApp.Core.BL.Models.People.Customer customer =
                    await new CustomersController().SaveCustomerToDevice(this.personRegistrationInfo, e, false);

                if (customer == null || customer.Id == default(Guid))
                {
                    new ReusableScreens(this.Activity)
                        .ShowInfo(
                            Resource.String.customer_save_err_actionbar,
                            Resource.String.customer_save_err_title,
                            Resource.String.customer_save_err_message,
                            Resource.String.customer_save_err_button);
                }

            }
            catch (DuplicateValuesException dx)
            {
                new ReusableScreens(this.Activity)
                        .ShowInfo(
                            Resource.String.customer_save_err_actionbar,
                            Resource.String.customer_save_err_title,
                            Resource.String.customer_duplicate_error_message,
                            Resource.String.customer_save_err_button);
            }
        }

        private async Task MarkReadyForUpload(CustomerRegistrationCompletedEventArgs e)
        {
            if (e == null || !e.Succeeded)
            {
                return;
            }

            CustomerPhotoService service = new CustomerPhotoService();
            await service.MarkForUpload(this.personRegistrationInfo.NationalId);
        }

        public void PositiveAction()
        {
            if (CustomerService.RegistrationSuccessful)
            {
                if (WizardActivity.BundledItems != null)
                {
                    // Clear the bundle just in case it contains prospect conversion info, we dont want it displayed when we relaunch
                    WizardActivity.BundledItems.Clear();
                }
                
                WizardLauncher.Launch(this.Activity, WizardTypes.CustomerRegistration, ActivityBase.StartPointIntent);
            }
            else
            {
                Save();
            }
           
        }

        public void NegativeAction()
        {
            OnCancel();
        }
    }
}