using Android.App;
using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.Api.SwapComponent;
using SalesApp.Core.Enums.Validation;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Validation;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid.People.SwapComponents
{
    public class CustomerIdentificationFragment : FragmentBase3
    {
        public static readonly string TabBundleKey = "TabBundleKey";
        public static readonly string IdentifierBundleKey = "IdentifierBundleKey";
        private static readonly ILog Log = LogManager.Get(typeof(CustomerIdentificationFragment));
        private View rootView;
        private Activity context;
        private int tab, _identifierType;
        private TextView instructionsTextView;
        private EditText identifierEditText;
        private Button findCustomerButton;
        private string _identifier, identifierTypeMessage;
        private SwapComponentApi api;
        private TextView errorTextView;
        private bool _cameBack;

        /// <summary>
        /// Create the UI that this fragment displays
        /// </summary>
        /// <param name="inflater">The inflator to use</param>
        /// <param name="container">The container view</param>
        /// <param name="savedInstanceState">The saved stated if any</param>
        /// <returns>The inflated view</returns>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // get the arguments
            if (this.Arguments != null)
            {
                this.tab = this.Arguments.GetInt(TabBundleKey);
                if (Arguments.ContainsKey("identifier"))
                {
                    _identifier = Arguments.GetString("identifier");
                    _cameBack = true;
                }
            }

            base.OnCreateView(inflater, container, savedInstanceState);
            this.context = this.Activity;
            this.rootView = inflater.Inflate(Resource.Layout.layout_swap_customer_identification, container, false);
            this.InitializeUI();
            return this.rootView;
        }

        protected override void InitializeUI(bool isOnUiThread = false)
        {
            instructionsTextView = rootView.FindViewById<TextView>(Resource.Id.textView_title);
            identifierEditText = rootView.FindViewById<EditText>(Resource.Id.editText_input);
            findCustomerButton = rootView.FindViewById<Button>(Resource.Id.button_find_customer);
            errorTextView = rootView.FindViewById<TextView>(Resource.Id.textViewError);

            int instructions = Resource.String.phone_number_message;
            //int icon = Resource.Drawable.ic_action_call;
            switch (tab)
            {
                case SwapComponentsActivity.phoneTab:
                    _identifierType = 2;
                    instructions = Resource.String.phone_number_message;
                    identifierEditText.InputType = InputTypes.ClassPhone;
                    //icon = Resource.Drawable.ic_action_call;
                    identifierTypeMessage = GetString(Resource.String.telephone_number_);
                    break;
                case SwapComponentsActivity.idTab:
                    _identifierType = 1;
                    instructions = Resource.String.id_message;
                    identifierEditText.InputType = InputTypes.ClassText;
                    //icon = Resource.Drawable.ic_action_person;
                    identifierTypeMessage = GetString(Resource.String.Id_number_);
                    break;
                case SwapComponentsActivity.serialTab:
                    _identifierType = 3;
                    instructions = Resource.String.serial_message;
                    identifierEditText.InputType = InputTypes.ClassText;
                    //icon = Resource.Drawable.ic_action_phone;
                    identifierTypeMessage = GetString(Resource.String.serial_number);
                    break;
            }
            if (!string.IsNullOrEmpty(_identifier) && _cameBack)
            {
                identifierEditText.Text = _identifier;
                findCustomerButton.Enabled = true;
            }
            instructionsTextView.Text = GetString(instructions);

            SetEventHandlers();
            findCustomerButton.Enabled = false;
            identifierEditText.AfterTextChanged += (sender, args) =>
            {
                _identifier = identifierEditText.Text.ToString();
                if (!string.IsNullOrEmpty(_identifier))
                {
                    findCustomerButton.Enabled = true;
                }
                else
                {
                    findCustomerButton.Enabled = false;
                }
            };
        }

        public override bool Validate()
        {
            bool valid = false;
            _identifier = identifierEditText.Text.ToString();
            if (_identifierType == 2)
            {
                valid = ValidatePhoneNumber(_identifier);
            }
            else
            {
                if (!string.IsNullOrEmpty(_identifier))
                {
                    valid = true;
                }
                else
                {
                    errorTextView.Text = GetString(Resource.String.please_enter) + identifierTypeMessage;
                    errorTextView.Visibility = ViewStates.Visible;
                }
            }
            return valid;
        }

        private bool ValidatePhoneNumber(string phoneNumber)
        {
            // validate phone number
            PhoneValidationResultEnum phoneValidationResult =
             new PeopleDetailsValidater().ValidatePhoneNumber(phoneNumber);

            if (phoneValidationResult != PhoneValidationResultEnum.NumberOk)
            {
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
                errorTextView.Visibility = ViewStates.Visible;
                errorTextView.Text = GetText(errorRes);
                return false;
            }
            errorTextView.Text = "";
            errorTextView.Visibility = ViewStates.Invisible;
            return true;
        }
        protected override void SetEventHandlers()
        {
            findCustomerButton.Click += delegate
            {
                if (Validate())
                {
                    GetCustomerDetails();
                }
            };
        }

        private async void GetCustomerDetails()
        {
            Log.Verbose("Get details of a customer.");
            var progressDialog = ProgressDialog.Show(context, GetString(Resource.String.please_wait), GetString(Resource.String.loading_customer_details), true);
            api = new SwapComponentApi();
            string parameters = "/customer?id=" + _identifier + "&RequestType=" + _identifierType;
            bool isOnline = Resolver.Instance.Get<IConnectivityService>().HasConnection();
            if (!isOnline)
            {
                Toast.MakeText(context, GetString(Resource.String.unable_to_continue_net),
                    ToastLength.Long).Show();
                errorTextView.Visibility = ViewStates.Invisible;
                progressDialog.Hide();
            }
            else
            {
                CustomerDetailsResponse customerDetailsResponse = await api.GetCustomerDetails(parameters);
                progressDialog.Hide();
                if (customerDetailsResponse.Successful)
                {
                    Log.Verbose("API Call successful");
                    if (!customerDetailsResponse.CustomerFound)
                    {
                        findCustomerButton.Enabled = false;
                        errorTextView.Text = GetString(Resource.String.customer_not_exist_reenter) + " " + identifierTypeMessage;
                        errorTextView.Visibility = ViewStates.Visible;
                    }
                    else if (customerDetailsResponse.AccountStatus == AccountStatus.Blocked)
                    {
                        findCustomerButton.Enabled = false;
                        errorTextView.Text = GetString(Resource.String.blocked_customer_account);
                        errorTextView.Visibility = ViewStates.Visible;
                    }
                    else if (customerDetailsResponse.AccountStatus == AccountStatus.Cancelled)
                    {
                        findCustomerButton.Enabled = false;
                        errorTextView.Text = GetString(Resource.String.cancelled_customer_account);
                        errorTextView.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        customerDetailsResponse.IdentifierType = _identifierType;
                        customerDetailsResponse.Identifier = _identifier;
                        Bundle extras = new Bundle();
                        extras.PutString("customerDetailsResponse", JsonConvert.SerializeObject(customerDetailsResponse));
                        Intent intent = new Intent(context, typeof(CustomerDetailsActivity));
                        intent.PutExtras(extras);
                        StartActivity(intent);
                    }
                }
                else
                {
                    Log.Verbose("something went wrong");
                    errorTextView.Text = GetString(Resource.String.connection_error);
                    errorTextView.Visibility = ViewStates.Visible;
                }
            }
        }

        public override void UpdateUI(bool calledFromUiThread = false)
        {
        }

        public override void SetViewPermissions()
        {
        }
    }
}