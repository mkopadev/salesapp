using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using Newtonsoft.Json;
using SalesApp.Core.BL;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.Syncing;
using SalesApp.Core.Events.CustomerRegistration;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Person;
using SalesApp.Core.Services.Person.Customer;
using SalesApp.Core.Services.Settings;
using SalesApp.Core.ViewModels.Person.Customer;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Components.UIComponents.Image;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.Services.Phone;
using SalesApp.Droid.Tickets;
using Exception = System.Exception;

namespace SalesApp.Droid.People.Customers
{
    public class CustomerDetailFragment : MvxFragmentBase, RegistrationFinishedFragmentAlert.IRegistrationActions
    {
        public interface IRegistrationStatusListener
        {
            void OnRegistrationFinished(Bundle arguments);

            void OnRegistrationFinished(string arguments, string messageDetail);

            void OnRegistrationCanceled();
        }

        private readonly RegistrationStatusService _registrationStatusService = new RegistrationStatusService();
        private TextView _txtSmsSendInfo;
        private CircularImageView _imgCustomerPhoto;
        private TextView _txtCustomerName;
        private TextView _txtCustomerPhone;
        private TextView _txtCustomerProduct;
        private Button _btnRaiseIssue;
        private Button _btnCallCustomer;
        private TextView _txtRegistrationProcessSteps;
        private TextView _txtStepDoneNo;
        private TextView _txtStepDone;
        private TextView _txtStepCurrentNo;
        private TextView _txtStepCurrent;
        private TextView _txtStepFutureNo;
        private TextView _txtStepFuture;
        private TextView _txtExtraInformation;
        private View _currentView;

        private CustomerSearchResult _customerSearchResult;

        private LinearLayout _linStepDoneNo;
        private View _lineStepDone;
        private LinearLayout _linStepCurrentNo;
        private View _lineStepCurrent;
        private LinearLayout _linStepFutureNo;
        private RelativeLayout _lSmsInfoStatus;
        private ProgressBar _loadingAnimation;
        private TextView _btnResendRegistration;

        internal const string SearchResult = "searchResult";

        internal static readonly string CustomerKey = "CustomerKey";

        internal const string AlertKey = "AlertKey";

        /// <summary>
        /// Registering customers whose registration failed
        /// </summary>
        private CustomerService _customerService;

        private RegistrationFinishedFragmentAlert _registrationFailedFragment;
        private Customer _customer;
        private ProgressDialog _dialog;

        private ProgressDialog AlertDialog
        {
            get
            {
                if (_dialog == null)
                {
                    if (Activity == null)
                    {
                        throw new Exception("Context is null");
                    }

                    _dialog = new ProgressDialog(Activity) { Indeterminate = true };
                    _dialog.SetMessage(Activity.GetString(Resource.String.resending));
                }

                return _dialog;
            }
        }

        public CustomerDetailFragment()
        {
            _customer = new Customer();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            if (this._customerSearchResult != null)
            {
                outState.PutString(SearchResult, JsonConvert.SerializeObject(this._customerSearchResult));
            }
        }

        private CustomerService CustomerService
        {
            get
            {
                if (this._customerService == null)
                {
                    this._customerService = new CustomerService();

                    this._customerService.RegistrationAttempted += this.RegistrationAttempted;
                    this._customerService.RegistrationCompleted += this.RegistrationCompleted;
                }

                return this._customerService;
        }
        }


        /// <summary>
        /// Method is called when customer registration was attempted but failed.
        /// The <paramref name="e" /> has an integer indicating the attempt number .
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event</param>
        private async void RegistrationAttempted(object sender, CustomerRegistrationAttemptedEventArgs e)
        {
            if (Activity == null)
            {
                return;
            }

            if (e.Channel == DataChannel.Fallback)
            {
                RegistrationByFallbackFailed();
            }
            else
            {
                RegistrationByDataFailed();
            }
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
                return;
            }

            if (e.Channel == DataChannel.Fallback)
            {
                this.RegistrationByFallbackCompleted(e);
            }
            else
            {
                this.RegistrationByDataCompleted(e);
            }
        }

        private void RegistrationByFallbackFailed()
        {
            string posButtonTxt = Activity.GetString(Resource.String.try_again_resending);
            string negButtonTxt = Activity.GetString(Resource.String.cancel_resending);

            string message = Activity.GetString(Resource.String.customer_registration_failed_retry);
            Bundle arguments = new Bundle();
            arguments.PutBoolean(RegistrationFinishedFragment.WasRegistrationKey, true);
            arguments.PutBoolean(RegistrationFinishedFragment.SuccessKey, false);
            arguments.PutString(RegistrationFinishedFragment.MessageKey, message);
            arguments.PutString(RegistrationFinishedFragment.BtnPositiveKey, posButtonTxt);
            arguments.PutString(RegistrationFinishedFragment.BtnNegativeKey, negButtonTxt);

            ShowRegistrationStatus(arguments);

        }

        private void ShowRegistrationStatus(Bundle arguments)
        {
            var trans = this.FragmentManager.BeginTransaction();
            var frag = this.FragmentManager.FindFragmentByTag(AlertKey);
            if (frag != null)
            {
                this._registrationFailedFragment = (RegistrationFinishedFragmentAlert)frag;
                trans.Remove(this._registrationFailedFragment).Commit();
                trans.AddToBackStack(null);
            }
            else
            {
                this._registrationFailedFragment = new RegistrationFinishedFragmentAlert();
                this._registrationFailedFragment.Arguments = arguments;
            }

            this._registrationFailedFragment.Show(this.FragmentManager, AlertKey);
        }

        private void RegistrationByDataFailed()
        {
        }

        private void RegistrationByDataCompleted(CustomerRegistrationCompletedEventArgs e)
        {
            Bundle arguments = new Bundle();
            arguments.PutBoolean(RegistrationFinishedFragment.WasRegistrationKey, false);
            arguments.PutBoolean(RegistrationFinishedFragment.SuccessKey, e.Succeeded);
            arguments.PutString(RegistrationFinishedFragment.MessageKey, Activity.GetString(Resource.String.successful_registration_message));

            ShowRegistrationStatus(arguments);

            if (e.Succeeded)
            {
                AlertDialog.Dismiss();
                _customerSearchResult.SyncStatus = RecordStatus.Synced;
                _customerSearchResult.Channel = DataChannel.Full;
                new CustomersController().SaveCustomerToDevice(_customer, e, false);
                UpdateUi(true);
            }
        }

        private void RegistrationByFallbackCompleted(CustomerRegistrationCompletedEventArgs e)
        {
            string posButtonTxt = Activity.GetString(Resource.String.add_new_customer);
            string negButtonTxt = Activity.GetString(Resource.String.return_to_customer_details);
            string message = Activity.GetString(Resource.String.sms_registration_done);

            if (!e.Succeeded)
            {
                message = Activity.GetString(Resource.String.sms_registration_failed_three_tries);
            }
            Bundle arguments = new Bundle();
            arguments.PutBoolean(RegistrationFinishedFragment.WasRegistrationKey, true);
            arguments.PutBoolean(RegistrationFinishedFragment.SuccessKey, e.Succeeded);
            arguments.PutString(RegistrationFinishedFragment.MessageKey, message);
            arguments.PutString(RegistrationFinishedFragment.BtnPositiveKey, posButtonTxt);
            arguments.PutString(RegistrationFinishedFragment.BtnNegativeKey, negButtonTxt);
            int maxFallbackTries = Settings.Instance.MaxFallbackRetries;
            arguments.PutInt(RegistrationFinishedFragment.Retries, maxFallbackTries);

            ShowRegistrationStatus(arguments);

            if (e.Succeeded)
            {
                AlertDialog.Dismiss();
                _customerSearchResult.SyncStatus = RecordStatus.FallbackSent;
                new CustomersController().SaveCustomerToDevice(_customer, e, false);
                UpdateUi(true);
            }
        }

        /// <summary>
        /// Initializes a customer object from a customer search result
        /// </summary>
        private async Task InitCustomer()
        {
            CustomerProduct customerProduct = null;

            if (this._customerSearchResult != null && this._customerSearchResult.Product == null)
            {
                string sql = string.Format("SELECT * FROM CustomerProduct cp WHERE cp.CustomerId='{0}' ORDER BY cp.DateCreated DESC LIMIT 1", _customerSearchResult.Id);

                List<CustomerProduct> products = await new QueryRunner().RunQuery<CustomerProduct>(sql);

                if (products != null && products.Count > 0)
                {
                    customerProduct = products.ElementAt(0);
                }

                if (customerProduct == null || customerProduct.CustomerId == default(Guid))
                {
                    this._customerSearchResult.Product = new Product
                    {
                        DisplayName = "Product III"
                    };
                }
                else
                {
                    this._customerSearchResult.Product = new Product
                    {
                        DisplayName = customerProduct.DisplayName,
                        SerialNumber = customerProduct.SerialNumber,
                        ProductTypeId = customerProduct.ProductTypeId,
                        Id = customerProduct.Id
                    };
                }
            }

            await this.LoadCustomerPhoto(this._customerSearchResult.NationalId);
            this.UpdateUi();
        }

        private async Task LoadCustomerPhoto(string nationalId)
        {
            CustomerPhotoService service = new CustomerPhotoService();
            CustomerPhoto photo = await service.GetMostRecentCustomerPhoto(nationalId);

            CustomerDetailFragmentViewModel vm = this.ViewModel as CustomerDetailFragmentViewModel;

            if (photo == null || photo.FilePath == null || vm == null)
            {
                return;
            }

            vm.MostRecentPhoto = photo.FilePath;
            try
            {
                Bitmap bitmap = BitmapFactory.DecodeFile(photo.FilePath);

                if (bitmap == null)
                {
                    return;
                }

                this.Activity.RunOnUiThread(() =>
                {
                    this._imgCustomerPhoto.SetImageBitmap(bitmap);
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        } 

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedState)
        {
            base.OnCreateView(inflater, container, savedState);
            _currentView = this.BindingInflate(Resource.Layout.customer_details, container, false);

            this.ViewModel = new CustomerDetailFragmentViewModel();

            _txtSmsSendInfo = _currentView.FindViewById<TextView>(Resource.Id.txtSmsSendInfo);
            _imgCustomerPhoto = _currentView.FindViewById<CircularImageView>(Resource.Id.imgCustomerPhoto);
            _txtCustomerName = _currentView.FindViewById<TextView>(Resource.Id.txtCustomerName);
            _txtCustomerPhone = _currentView.FindViewById<TextView>(Resource.Id.txtCustomerPhone);
            _currentView.FindViewById<TextView>(Resource.Id.txtLblProduct);
            _txtCustomerProduct = _currentView.FindViewById<TextView>(Resource.Id.txtCustomerProduct);
            _btnRaiseIssue = _currentView.FindViewById<Button>(Resource.Id.btnRaiseIssue);
            _btnRaiseIssue.TransformationMethod = null;
            _btnCallCustomer = _currentView.FindViewById<Button>(Resource.Id.btnCallCustomer);
            _btnCallCustomer.TransformationMethod = null;
            _txtRegistrationProcessSteps = _currentView.FindViewById<TextView>(Resource.Id.txtRegistrationProcessSteps);
            _txtStepDoneNo = _currentView.FindViewById<TextView>(Resource.Id.txtStepDoneNo);
            _txtStepDone = _currentView.FindViewById<TextView>(Resource.Id.txtStepDone);
            _txtStepCurrentNo = _currentView.FindViewById<TextView>(Resource.Id.txtStepCurrentNo);
            _txtStepCurrent = _currentView.FindViewById<TextView>(Resource.Id.txtStepCurrent);
            _txtStepFutureNo = _currentView.FindViewById<TextView>(Resource.Id.txtStepFutureNo);
            _txtStepFuture = _currentView.FindViewById<TextView>(Resource.Id.txtStepFuture);
            _txtExtraInformation = _currentView.FindViewById<TextView>(Resource.Id.txtExtraInformation);
            _txtExtraInformation.Visibility = ViewStates.Gone;

            _linStepDoneNo = _currentView.FindViewById<LinearLayout>(Resource.Id.linStepDoneNo);
            _lineStepDone = _currentView.FindViewById<View>(Resource.Id.lineStepDoneNo);
            _linStepCurrentNo = _currentView.FindViewById<LinearLayout>(Resource.Id.linStepCurrentNo);
            _lineStepCurrent = _currentView.FindViewById<View>(Resource.Id.lineStepCurrent);
            _linStepFutureNo = _currentView.FindViewById<LinearLayout>(Resource.Id.linStepFutureNo);
            _lSmsInfoStatus = _currentView.FindViewById<RelativeLayout>(Resource.Id.lSmsInfoStatus);
            _btnResendRegistration = _currentView.FindViewById<TextView>(Resource.Id.btnResendRegistration);
            _loadingAnimation = _currentView.FindViewById<ProgressBar>(Resource.Id.loadingAnimation);

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen(Activity.GetString(Resource.String.customer_details));

            if (savedState != null)
            {
                string json = savedState.GetString(SearchResult);
                if (!string.IsNullOrEmpty(json))
                {
                    _customerSearchResult = JsonConvert.DeserializeObject<CustomerSearchResult>(json);
                }
            }

            if (_customerSearchResult == null)
            {
                _customerSearchResult = JsonConvert.DeserializeObject<CustomerSearchResult>(Arguments.GetString(SearchResult));
            }

            AsyncHelper.RunSync(async () => await this.InitCustomer());

            SetEventHandlers();
            return _currentView;
        }

        private void ShowSteps(bool show)
        {
            _linStepCurrentNo.Visibility =
                _lineStepCurrent.Visibility =
                    _linStepDoneNo.Visibility =
                        _lineStepDone.Visibility =
                            _linStepFutureNo.Visibility =
                                show ? ViewStates.Visible : ViewStates.Gone;
            if (!show)
            {
                _txtRegistrationProcessSteps.Text = GetString(Resource.String.no_registration_information);
            }
        }

        private ViewStates SmsInfoViewState
        {
            get
            {
                if (_customerSearchResult.Channel != DataChannel.Fallback ||
                    _customerSearchResult.SyncStatus == RecordStatus.Synced)
                {
                    return ViewStates.Gone;
                }
                return ViewStates.Visible;
            }
        }

        private void ShowSmsSendingStatus()
        {   
            _lSmsInfoStatus.Visibility = SmsInfoViewState;
            if (_lSmsInfoStatus.Visibility == ViewStates.Gone)
            {
                return;
            }

            if (_customerSearchResult.SyncStatus == RecordStatus.FallbackSent)
            {
                _btnResendRegistration.Visibility = ViewStates.Gone;
                _txtSmsSendInfo.Text = GetString(Resource.String.sms_sent);
                _lSmsInfoStatus.SetBackgroundColor(Color.ParseColor("#3c97b5"));
            }
            else
            {
                _txtSmsSendInfo.Text = GetString(Resource.String.sms_not_sent);
            }
        }

        public bool FragmentWasUnloaded
        {
            get { return Activity == null; }
        }

        public async void UpdateUi(bool calledFromUiThread = false)
        {
            if (!calledFromUiThread)
            {
                Activity.RunOnUiThread(() => UpdateUi(true));
                return;
            }

            this.ShowSmsSendingStatus();
            _txtCustomerName.Text = _customerSearchResult.FullName;

            _txtCustomerPhone.Text = _customerSearchResult.Phone;
            _txtCustomerProduct.Text = _customerSearchResult.Product.DisplayName;
            bool connectedToInternet = Resolver.Instance.Get<IConnectivityService>().HasConnection();
            RegistrationStatusOverview overview = null;
            try
            {
                ShowSteps(false);
                _txtRegistrationProcessSteps.Text = GetString(Resource.String.loading_status);
                _loadingAnimation.Visibility = ViewStates.Visible;

                if (connectedToInternet)
                {
                    var status = await _customerSearchResult.GetStatusAsync();
                    Customer c = new Customer
                    {
                        Id = _customerSearchResult.Id,
                        Phone = _customerSearchResult.Phone,
                        FirstName = _customerSearchResult.FullName,
                        LastName = _customerSearchResult.LastName
                    };

                    if (status != null && status.Steps != null && status.AdditionalInfo != null)
                    {
                        overview = await _registrationStatusService.GetVisibleSteps(c, status.Steps, status.AdditionalInfo);
                        Intent callerIntent = new Intent(this.Activity, typeof(CustomerListView));
                        callerIntent.PutExtra(CustomerListView.CustomerStatusBundled, JsonConvert.SerializeObject(status));
                        this.Activity.SetResult(Result.Ok, callerIntent);
                    }
                }
                else
                {
                    await _registrationStatusService.GetVisibleSteps(_customerSearchResult.Id);
                }

            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
            finally
            {
                // always hide the loader
                _loadingAnimation.Visibility = ViewStates.Gone;
            }

            if (FragmentWasUnloaded)
            {
                return;
            }

            if (!connectedToInternet)
            {
                CriteriaBuilder criteriaBuilder = new CriteriaBuilder();
                var regStatus = await new CustomerRegistrationStepsStatusController()
                    .GetManyByCriteria(
                        criteriaBuilder
                            .Add("CustomerId", _customerSearchResult.Id));

                if (regStatus != null)
                {
                    var status = regStatus.OrderByDescending(reg => reg.Modified).FirstOrDefault();
                    if (status != null && !status.AdditionalInfo.IsBlank())
                    {
                        _txtExtraInformation.Text = status.AdditionalInfo;
                        _txtExtraInformation.Visibility = ViewStates.Visible;
                    }
                }
            }
            else
            {
                if (overview != null && overview.HasCurrentStep && !overview.CurrentStep.AdditionalInfo.IsBlank())
                {
                    _txtExtraInformation.Text = overview.CurrentStep.AdditionalInfo;
                    _txtExtraInformation.Visibility = ViewStates.Visible;
                }
            }

            if (overview == null)
            {
                ShowSteps(false);
                _txtSmsSendInfo.Visibility = SmsInfoViewState;
            }
            else
            {
                ShowSteps(true);
                _txtRegistrationProcessSteps.Text = string.Format(
                    GetString(Resource.String.registration_process_steps),
                    overview.CurrentStepNo != 0 ? overview.CurrentStepNo : overview.TotalSteps,
                    overview.TotalSteps);

                if (overview.HasPreviousStep)
                {
                    _linStepDoneNo.Visibility = ViewStates.Visible;
                    _lineStepDone.Visibility = ViewStates.Visible;
                    _txtStepDoneNo.Text = overview.PreviousStep.StepNumber.ToString();
                    _txtStepDone.Text = overview.PreviousStep.StepName;
                }
                else
                {
                    _linStepDoneNo.Visibility = ViewStates.Gone;
                    _lineStepDone.Visibility = ViewStates.Gone;
                }

                if (overview.HasCurrentStep)
                {
                    _linStepCurrentNo.Visibility = ViewStates.Visible;
                    _lineStepCurrent.Visibility = ViewStates.Visible;
                    _txtStepCurrentNo.Text = overview.CurrentStep.StepNumber.ToString();
                    _txtStepCurrent.Text = overview.CurrentStep.StepName;
                }
                else
                {
                    _linStepCurrentNo.Visibility = ViewStates.Gone;
                    _lineStepCurrent.Visibility = ViewStates.Gone;
                }

                if (overview.HasFutureStep)
                {
                    _linStepFutureNo.Visibility = ViewStates.Visible;
                    _txtStepFutureNo.Text = overview.FutureStep.StepNumber.ToString();
                    _txtStepFuture.Text = overview.FutureStep.StepName;
                }
                else
                {
                    _lineStepCurrent.Visibility = ViewStates.Gone;
                    _linStepFutureNo.Visibility = ViewStates.Gone;
                }
            }

            ActivityBase activity = Activity as ActivityBase;
            if (activity != null)
            {
                activity.SetScreenTitle(_customerSearchResult.FullName);
            }
        }

        protected void SetEventHandlers()
        {
            _btnCallCustomer.Click += delegate
            {
                new CallService().Dial(_customerSearchResult.Phone, Activity);                
            };

            _btnRaiseIssue.Click += delegate
            {
                Intent intent = new Intent(Activity, typeof (TicketCustomerIdentityActivity));
                intent.PutExtra(TicketCustomerIdentityActivity.PhoneNumber, _customerSearchResult.Phone);
                StartActivity(intent);
            };

            _btnResendRegistration.Click += delegate
            {
                AlertDialog.Show();
                RegisterCustomer();
            };
        }

        private void RegisterCustomer()
        {
            _customer.Id = _customerSearchResult.Id;
            _customer.FirstName = _customerSearchResult.FirstName;
            _customer.LastName = _customerSearchResult.LastName;
            _customer.Phone = _customerSearchResult.Phone;
            _customer.NationalId = _customerSearchResult.NationalId;
            _customer.DsrPhone = Settings.Instance.DsrPhone;
            _customer.Product = _customerSearchResult.Product;
            _customer.PersonType = _customerSearchResult.PersonType;
            _customer.Modified = _customerSearchResult.Modified;
            _customer.Created = _customerSearchResult.Created;
            CustomerService.RegisterCustomer(_customer);
        }

        public void PositiveAction()
        {
            AlertDialog.Dismiss();
            if (!CustomerService.RegistrationSuccessful)
            {
                if (_customer != null)
                {
                    RegisterCustomer();
                }
            }
            else
            {
                UpdateUi(true);
                _customerService = null;
            }
        }

        public void NegativeAction()
        {
            AlertDialog.Dismiss();
            _customerService = null;
        }

        public override void OnDestroy()
        {
            _customerService = null;
            base.OnDestroy();
        }

        public override void OnResume()
        {
            base.OnResume();
            UpdateUi(true);
        }
    }
}