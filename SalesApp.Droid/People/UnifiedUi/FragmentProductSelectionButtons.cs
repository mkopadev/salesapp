using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using MK.Solar.Components.UIComponents;
using MK.Solar.Enums;
using MK.Solar.People.Customers;
using MK.Solar.People.Prospects;
using MK.Solar.UI.Wizardry;
using Mkopa.Core.BL;
using Mkopa.Core.BL.Controllers.People;
using Mkopa.Core.BL.Controllers.Synchronization;
using Mkopa.Core.BL.Models.People;
using Mkopa.Core.BL.Models.Syncing;
using Mkopa.Core.Enums.Api;
using Mkopa.Core.Enums.Syncing;
using Mkopa.Core.Extensions;
using Mkopa.Core.Services.Database;
using Mkopa.Core.Services.Database.Querying;
using Mkopa.Core.Services.DependancyInjection;
using Mkopa.Core.Services.Person;
using Mkopa.Core.Services.Settings;
using MK.Solar.People.UnifiedUi.Customer;
using Newtonsoft.Json;
using SQLite.Net;
using Exception = System.Exception;

namespace MK.Solar.People.UnifiedUi
{
    /// <summary>
    /// This is the product selection screen that uses buttons instead of spinners
    /// </summary>
    public class FragmentProductSelectionButtons : WizardStepFragment
    {
        /// <summary>
        /// Bundle key for the registration information payload
        /// </summary>
        private const string BundledRegistrationInfo = "BundledRegistrationInfo";

        /// <summary>
        /// Registration Successful Fragment
        /// </summary>
        private const string RegistrationSuccessfulFragment = "RegistrationSuccessfulFragment";

        /// <summary>
        /// The registration information
        /// </summary>
        private Mkopa.Core.BL.Models.People.Customer personRegistrationInfo;

        /// <summary>
        /// The customer service
        /// </summary>
        private ICustomerService customerService;

        /// <summary>
        /// The time taken processing
        /// </summary>
        private DateTime processingShown;

        /// <summary>
        /// Whether or not registration is in progress
        /// </summary>
        private bool registrationInProgress;

        /// <summary>
        /// whether or not SMS sending failed
        /// </summary>
        private int smsFailed;

        /// <summary>
        /// Maximum number of SMS send tries
        /// </summary>
        private int maxSmsTries;

        /// <summary>
        /// SMS successful overlay
        /// </summary>
        private WizardOverlayFragment registrationSuccessfulFragment;

        /// <summary>
        /// SMS failed overlay
        /// </summary>
        private WizardOverlayFragment registrationFailedFragment;

        /// <summary>
        /// A list of the available products
        /// </summary>
        private List<Product> products;

        /// <summary>
        /// Saving progress status
        /// </summary>
        private SavingProgress savingProgress;

        private string ADD_PROD_TAG = "ADDITONAL_PRODUCT";

        /// <summary>
        /// ENUM represents different saving progress statuses
        /// </summary>
        private enum SavingProgress
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
        }

        /// <summary>
        /// Gets the step title
        /// </summary>
        public override int StepTitle
        {
            get { return Resource.String.unified_select_product_title; }
        }

        /// <summary>
        /// Gets this fragment's activity instance
        /// </summary>
        private ActivityBase ActivityBase
        {
            get
            {
                return this.Activity as ActivityBase;
            }
        }

        /// <summary>
        /// Load the registration data from a JSON string
        /// </summary>
        /// <param name="serializedString">The JSON string</param>
        public override void SetData(string serializedString)
        {
            if (!serializedString.IsBlank())
            {
                this.personRegistrationInfo =
                    JsonConvert.DeserializeObject<Mkopa.Core.BL.Models.People.Customer>(serializedString);
                if (this.personRegistrationInfo.Product == null)
                {
                    this.personRegistrationInfo.Product = new Product();
                }
            }
        }

        /// <summary>
        /// Called when registration is cancelled
        /// </summary>
        public void OnCancel()
        {
            this.Activity.Finish();
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
        /// Get the next fragment
        /// </summary>
        /// <returns>The next fragment to load</returns>
        public override Type GetNextFragment()
        {
            return default(Type);
        }

        /// <summary>
        /// Load the products
        /// </summary>
        public void LoadProducts()
        {
            var productsJson = Settings.Instance.Products;
            this.products = JsonConvert.DeserializeObject<List<Product>>(productsJson);
        }

        /// <summary>
        /// Creates the view for this fragment
        /// </summary>
        /// <param name="inflater">The inflator</param>
        /// <param name="container">The container</param>
        /// <param name="savedInstanceState">The saved state</param>
        /// <returns>The view</returns>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            this.view = base.OnCreateView(inflater, container, savedInstanceState);
            this.savingProgress = SavingProgress.NotStarted;

            this.view = inflater.Inflate(Resource.Layout.fragment_layout_select_product_buttons, container, false);

            if (savedInstanceState != null)
            {
                string regInfo = savedInstanceState.GetString(BundledRegistrationInfo);
                if (!regInfo.IsBlank())
                {
                    this.personRegistrationInfo = JsonConvert.DeserializeObject<Mkopa.Core.BL.Models.People.Customer>(regInfo);
                }
            }

            this.InitializeUI();
            return this.view;
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
                this.registrationSuccessfulFragment = this.Activity.SupportFragmentManager.GetFragment(savedInstanceState, RegistrationSuccessfulFragment) as WizardOverlayFragment;
                this.registrationFailedFragment = this.Activity.SupportFragmentManager.GetFragment(savedInstanceState, RegistrationSuccessfulFragment) as WizardOverlayFragment;
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

            // store the success fragment for redisplay
            if (this.registrationSuccessfulFragment != null)
            {
                // outState.PutString(RegistrationSuccessfulFragment, JsonConvert.SerializeObject(registrationSuccessfulFragment));
                this.Activity.SupportFragmentManager.PutFragment(outState, this.registrationSuccessfulFragment.FragmentTag, this.registrationSuccessfulFragment);
            }

            // store the failed fragment for redisplay
            if (this.registrationFailedFragment != null)
            {
                // outState.PutString(RegistrationFailedFragment, JsonConvert.SerializeObject(registrationFailedFragment));
                this.Activity.SupportFragmentManager.PutFragment(outState, this.registrationFailedFragment.FragmentTag, this.registrationFailedFragment);
            }
        }

        /// <summary>
        /// Update the UI
        /// </summary>
        /// <param name="calledFromUiThread">Whether or not we are on the UI thread</param>
        public override void UpdateUI(bool calledFromUiThread = false)
        {
        }

        /// <summary>
        /// Set the permissions
        /// </summary>
        public override void SetViewPermissions()
        {
        }

        /// <summary>
        /// Validate fragment data
        /// </summary>
        /// <returns>True if validation passes, false otherwise</returns>
        public override bool Validate()
        {
            if (this.personRegistrationInfo.Product == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called to finish the wizard and return
        /// </summary>
        public override void FinishWizard()
        {
            this.Save();
        }

        /// <summary>
        /// What happens before we go next
        /// </summary>
        /// <returns>The validation before going to the next fragment</returns>
        public override bool BeforeGoNext()
        {
            return true;
        }

        /// <summary>
        /// Initialize our UI
        /// </summary>
        /// <param name="isOnUiThread">Whether or not we are on the UI thread</param>
        protected override void InitializeUI(bool isOnUiThread = false)
        {
            if (!isOnUiThread)
            {
                this.Activity.RunOnUiThread(
                        () =>
                        {
                            this.InitializeUI(true);
                        });

                return;
            }

            if (this.personRegistrationInfo == null)
            {
                throw new Exception("No data was passed to this fragment, customer registration information should never be null here");
            }

            this.WizardActivity.ButtonNextEnabled = false;
            this.WizardActivity.ButtonNext.Visibility = ViewStates.Gone;
            this.view.FindViewById<TextView>(Resource.Id.tvName).Text = this.personRegistrationInfo.FullName;
            this.view.FindViewById<TextView>(Resource.Id.tvPhone).Text = this.personRegistrationInfo.Phone;
            this.view.FindViewById<TextView>(Resource.Id.tvId).Text = this.personRegistrationInfo.NationalId;
            this.LoadProducts();

            // load the buttons
            this.LoadProductButtons();
        }

        /// <summary>
        /// Set the event handlers
        /// </summary>
        protected override void SetEventHandlers()
        {
        }

        /// <summary>
        /// Called when the product button is clicked
        /// </summary>
        /// <param name="sender">The button that was clicked</param>
        /// <param name="e">The event args</param>
        private void ProductButtonClick(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            string displayName = clickedButton.Text;
            this.ProductSelected(displayName);

            this.FinishWizard();
        }

        /// <summary>
        /// This inflates the products buttons using the OTA settings into the given container
        /// </summary>
        private void LoadProductButtons()
        {
            LayoutInflater inflater = LayoutInflater.From(this.Activity);
            LinearLayout productsContainer = this.view.FindViewById<LinearLayout>(Resource.Id.productsContainer);
            if (productsContainer == null)
            {
                throw new NullPointerException("The container for the product buttons cannot be null!");
            }

            int index = 0;

            foreach (var product in this.products)
            {
                LinearLayout layout = (LinearLayout)inflater.Inflate(Resource.Layout.product_button, productsContainer);

                Button productButton = (Button)layout.GetChildAt(index);

                if (productButton == null)
                {
                    // A child of the buttons container is not a button. Stop!
                    throw new IllegalArgumentException("All the children of the product buttons Linear Layout must be buttons");
                }

                productButton.Text = product.DisplayName;
                productButton.Click += this.ProductButtonClick;

                index++;
            }
        }

        // TODO move this to generic spot, more places might use this

        /// <summary>
        /// Show a progress
        /// </summary>
        /// <param name="shown">The date</param>
        /// <param name="now">The current date</param>
        /// <returns>An empty task</returns>
        private async Task FragmentShowAwaiter(DateTime shown, DateTime now)
        {
            // remove the listener
            this.customerService.RegistrationStatusChanged -= this.RegistrationStatusChanged;

            int minAwaitTime = 3000;
            int shownMilliseconds = (int)now.Subtract(shown).TotalMilliseconds;
            this.Logger.Verbose("Processing shown for: " + shownMilliseconds);

            if (shownMilliseconds < minAwaitTime)
            {
                int extraWait = minAwaitTime - shownMilliseconds;
                this.Logger.Verbose("Wait extra: " + extraWait);
                await Task.Delay(extraWait);
            }
        }

        /// <summary>
        /// Called when a serial number is selected
        /// </summary>
        /// <param name="displayName">The text</param>
        private void ProductSelected(string displayName)
        {
            this.Activity.RunOnUiThread(
                    () =>
                    {
                        var product = this.products.SingleOrDefault(prod => prod.DisplayName.AreEqual(displayName));

                        if (product == null)
                        {
                            return;
                        }

                        this.personRegistrationInfo.Product = new Product
                        {
                            ProductTypeId = product.Id,
                            DisplayName = product.DisplayName
                        };
                    });
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

            // set variable indicating we are saving the customer
            this.savingProgress = SavingProgress.Saving;

            this.personRegistrationInfo.DsrPhone = Settings.Instance.DsrPhone;

            this.WizardActivity.ShowWaitInfo(Resource.String.customer_registration, Resource.String.sending_information);

            this.customerService = new CustomerService();
            this.customerService.RegistrationStatusChanged += this.RegistrationStatusChanged;

            personRegistrationInfo.IsAdditionalProduct = true;
            if (personRegistrationInfo.IsAdditionalProduct)
            {
                Log.Info(ADD_PROD_TAG, "Adding additional product");
                this.customerService.RegisterAdditionalProduct(this.personRegistrationInfo);
            }
            else
            {
                Log.Info(ADD_PROD_TAG, "This is a new customer");
                // register customer async
                this.processingShown = DateTime.Now;

                this.registrationInProgress = true;

                Task.Run(
                        async () =>
                        {
                            await this.customerService.RegisterCustomer(this.personRegistrationInfo);
                        });
           }

        }

        /// <summary>
        /// Saves the customers selected product to the device
        /// </summary>
        /// <param name="customerId">The customer's ID</param>
        /// <param name="tran">The transaction in which to run</param>
        /// <returns>A void task</returns>
        private async Task SaveCustomerProductToDevice(Guid customerId, SQLiteConnection tran)
        {
            CustomerProduct cutomerProduct = JsonConvert.DeserializeObject<CustomerProduct>(JsonConvert.SerializeObject(this.personRegistrationInfo.Product));
            cutomerProduct.CustomerId = customerId;
            await new CustomerProductController().SaveAsync(tran, cutomerProduct);
        }

        /// <summary>
        /// Called when registration information has changed
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event arguments</param>
        private async void RegistrationStatusChanged(object sender, CustomerService.RegistrationStatusChangedEventArgs e)
        {
            this.registrationFailedFragment = null;
            this.registrationSuccessfulFragment = null;

            this.Logger.Verbose(e.Response != null ? e.Response.ResponseText : "No response...");

            this.maxSmsTries = Settings.Instance.MaxFallbackRetries;

            // still trying to post via API
            if (!e.Response.Successful && e.Channel != DataChannel.Fallback)
            {
                // still trying to register customer, show progress
                string message = this.WizardActivity.GetString(Resource.String.not_able_to_send);
                string messageDetail = string.Format(this.WizardActivity.GetString(Resource.String.retrying), e.NumberOfTries);
                this.WizardActivity.ShowWaitInfo(message, messageDetail);
            }
            else if (e.Response.RegistrationSuccessful)
            {
                this.savingProgress = SavingProgress.Succeeded;

                // make sure we show the processing fragment for some time
                // await this.FragmentShowAwaiter(this.processingShown, DateTime.Now);

                // pass boolean whether the registration was SMS to control the message to the user
                this.registrationSuccessfulFragment = new RegistrationFinishedFragment();
                Bundle arguments = new Bundle();
                arguments.PutBoolean(RegistrationFinishedFragment.WasRegistrationKey, e.Channel == DataChannel.Fallback);
                arguments.PutBoolean(RegistrationFinishedFragment.SuccessKey, true);
                arguments.PutString(RegistrationFinishedFragment.IntentStartPointKey, this.ActivityBase.StartPointIntent.ToString());
                this.registrationSuccessfulFragment.Arguments = arguments;
                this.WizardActivity.ShowOverlay(this.registrationSuccessfulFragment, true);
            }
            else if (e.Response.Successful && !e.Response.RegistrationSuccessful)
            {
                // if registration is not done, show error
                this.savingProgress = SavingProgress.Failed;

                // make sure we show the processing fragment for some time
                await this.FragmentShowAwaiter(this.processingShown, DateTime.Now);

                // pass boolean whether the registration was SMS to control the message to the user
                this.registrationFailedFragment = new RegistrationFinishedFragment();/*
                    e.Channel == DataChannel.Fallback,
                    true,
                    this.ActivityBase.StartPointIntent);*/

                Bundle arguments = new Bundle();
                arguments.PutBoolean(RegistrationFinishedFragment.WasRegistrationKey, e.Channel == DataChannel.Fallback);
                arguments.PutBoolean(RegistrationFinishedFragment.SuccessKey, true);
                arguments.PutString(RegistrationFinishedFragment.IntentStartPointKey, this.ActivityBase.StartPointIntent.ToString());
                this.registrationFailedFragment.Arguments = arguments;

                this.WizardActivity.ShowOverlay(this.registrationFailedFragment, true);
            }
            else if (e.Channel == DataChannel.Fallback)
            {
                // process the case of SMS
                this.savingProgress = SavingProgress.Failed;

                // make sure we show the processing fragment for some time
                await this.FragmentShowAwaiter(this.processingShown, DateTime.Now);
                this.smsFailed++;

                // depending on how many times SMS failed, decide what to show
                string posButtonTxt;
                string negButtonTxt;
                if (this.smsFailed >= this.maxSmsTries)
                {
                    posButtonTxt = this.WizardActivity.GetString(Resource.String.add_new_customer);
                    switch (this.ActivityBase.StartPointIntent)
                    {
                        case IntentStartPointTracker.IntentStartPoint.CustomerList:
                            negButtonTxt = this.WizardActivity.GetString(Resource.String.prospect_followup_return_to_customer_list);
                            break;
                        case IntentStartPointTracker.IntentStartPoint.ProspectsList:
                            negButtonTxt = this.WizardActivity.GetString(Resource.String.prospect_followup_return_to_prospect_list);
                            break;
                        case IntentStartPointTracker.IntentStartPoint.WelcomeScreen:
                            negButtonTxt = this.WizardActivity.GetString(Resource.String.return_to_home);
                            break;
                        default:
                            negButtonTxt = this.WizardActivity.GetString(Resource.String.return_to_home);
                            break;
                    }
                }
                else
                {
                    posButtonTxt = this.WizardActivity.GetString(Resource.String.try_again);
                    negButtonTxt = this.WizardActivity.GetString(Resource.String.cancel_registration);
                }

                bool stillTryingToSendSms = this.smsFailed < this.maxSmsTries;

                // SMS must have failed otherwise if statement before would have caught it
                string message = stillTryingToSendSms
                    ? this.WizardActivity.GetString(Resource.String.sms_registration_failed)
                    : string.Format(this.WizardActivity.GetString(Resource.String.unified_sms_registration_failed_n_tries), this.smsFailed);

                this.registrationFailedFragment = new RegistrationFinishedFragment();
                /*
                    e.Channel == DataChannel.Fallback,
                    false,
                    message,
                    posButtonTxt,
                    negButtonTxt,
                    this.ActivityBase.StartPointIntent);
                */

                Bundle arguments = new Bundle();
                arguments.PutBoolean(RegistrationFinishedFragment.WasRegistrationKey, e.Channel == DataChannel.Fallback);
                arguments.PutBoolean(RegistrationFinishedFragment.SuccessKey, false);
                arguments.PutString(RegistrationFinishedFragment.MessageKey, message);
                arguments.PutString(RegistrationFinishedFragment.BtnPositiveKey, posButtonTxt);
                arguments.PutString(RegistrationFinishedFragment.BtnNegativeKey, negButtonTxt);
                arguments.PutString(RegistrationFinishedFragment.IntentStartPointKey, this.ActivityBase.StartPointIntent.ToString());
                this.registrationFailedFragment.Arguments = arguments;

                // depending on sms retries, decide the event hadler
                if (this.smsFailed >= this.maxSmsTries)
                {
                    (this.registrationFailedFragment as RegistrationFinishedFragment).PositiveAction += delegate
                    {
                        WizardLauncher.Launch(this.Activity, WizardTypes.CustomerRegistration, ActivityBase.StartPointIntent);
                        (this.WizardActivity as Activity).Finish();
                    };

                    (this.registrationFailedFragment as RegistrationFinishedFragment).NegativeAction += this.OnCancel;
                }
                else
                {
                    (this.registrationFailedFragment as RegistrationFinishedFragment).PositiveAction += this.Save;
                    (this.registrationFailedFragment as RegistrationFinishedFragment).NegativeAction += this.OnCancel;
                }

                this.WizardActivity.ShowOverlay(this.registrationFailedFragment, true);
            }

            await this.SaveCustomerToDevice(e, this.smsFailed, this.maxSmsTries);
            this.registrationInProgress = false;
            this.savingProgress = SavingProgress.Done;
        }

        /// <summary>
        /// Called to save a customer to the device
        /// </summary>
        /// <param name="e">The event arguments</param>
        /// <param name="smsSendAttempts">Number of attempts</param>
        /// <param name="smsAttemptLimit">The Limit</param>
        /// <returns>An empty task</returns>
        private async Task SaveCustomerToDevice(CustomerService.RegistrationStatusChangedEventArgs e, int smsSendAttempts, int smsAttemptLimit)
        {
            bool succeeded = e.SmsRegistrationSucceeded || e.Response.RegistrationSuccessful;
            if (!succeeded)
            {
                this.Logger.Debug(string.Format("Sms send attempts = {0} and sms send limts = {1}", smsSendAttempts, smsAttemptLimit));

                succeeded = e.Channel == DataChannel.Fallback && smsAttemptLimit == smsSendAttempts;
                if (!succeeded)
                {
                    return;
                }
            }

            this.Logger.Debug("Attempting to save customer to device");
            if (e.Response == null || e.Response.Customer == null)
            {
                return;
            }

            ProspectsController prospectsController = new ProspectsController();
            Mkopa.Core.BL.Models.People.Prospect prospect = await prospectsController
                .GetByPhoneNumberAsync(e.Response.Customer.Phone);

            await Resolver.Instance.Get<ISQLiteDB>().Connection.RunInTransactionAsync(
                    async (SQLiteConnection connTran) =>
                    {
                        try
                        {
                            this.Logger.Debug("Trying to save customer");
                            CustomersController customersController = new CustomersController();
                            e.Response.Customer.Channel = e.Channel;
                            string customerPhone = e.Response.Customer.Phone;
                            Guid custGuid;

                            Mkopa.Core.BL.Models.People.Customer customer =
                                   await customersController.GetSingleByCriteria(CriteriaBuilder.New()
                                       .Add("Phone", customerPhone));

                            // customer to have more than one product
                            if (customer == null || customer.Id == default(Guid))
                            {
                                SaveResponse<Mkopa.Core.BL.Models.People.Customer> saveResponse =
                                    await customersController.SaveAsync(connTran, e.Response.Customer, true);

                                if (saveResponse.SavedModel == null || saveResponse.SavedModel.Id == default(Guid))
                                {
                                    new ReusableScreens(this.Activity)
                                        .ShowInfo(
                                            Resource.String.customer_save_err_actionbar,
                                            Resource.String.customer_save_err_title,
                                            Resource.String.customer_save_err_message,
                                            Resource.String.customer_save_err_button);
                                    return;
                                }
                                custGuid = saveResponse.SavedModel.Id;
                            }
                            else
                            {
                                custGuid = customer.Id;
                            }

                            await this.SaveCustomerProductToDevice(custGuid, connTran);

                            if (prospect != null)
                            {
                                this.Logger.Debug("There was a prospect with the same number so we convert to customer");
                                prospect.Converted = true;
                                await prospectsController.SaveAsync(connTran, prospect);
                            }

                            RecordStatus syncStatus = e.Channel == DataChannel.Fallback
                                ? (e.SmsRegistrationSucceeded
                                    ? RecordStatus.FallbackSent
                                    : RecordStatus.Pending)
                                : RecordStatus.Synced;

                            SyncingController syncController = new SyncingController();

                            SyncRecord syncRec = await syncController.GetSyncRecordAsync(e.Response.Customer.RequestId);
                            this.Logger.Debug("Fetching sync record for customer");

                            if (syncRec == null)
                            {
                                this.Logger.Debug("The sync record is null so we generate one");
                                syncRec = new SyncRecord
                                {
                                    ModelId = e.Response.Customer.Id,
                                    ModelType = e.Response.Customer.TableName,
                                    Status = syncStatus,
                                    RequestId = e.Response.Customer.RequestId
                                };
                                this.Logger.Debug("Saving generated sync record");
                                SaveResponse<SyncRecord> syncSaveResponse = await syncController.SaveAsync(
                                        connTran,
                                        syncRec);

                                this.Logger.Debug("Sync record was saved correctly");
                            }
                            else
                            {
                                syncRec.Status = syncStatus;
                            }

                            await syncController.SaveAsync(connTran, syncRec);
                        }
                        catch (Exception exception)
                        {
                            this.Logger.Error(exception);
                        }
                    });
        }
    }
}