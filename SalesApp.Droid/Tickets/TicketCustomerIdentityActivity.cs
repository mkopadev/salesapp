using System;
using System.IO;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using SalesApp.Core.Services.Settings;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.UI.Utils;

namespace SalesApp.Droid.Tickets
{
    [Activity(Label = "Raise Issue", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = false, ParentActivity = typeof(HomeView))]
    public class TicketCustomerIdentityActivity : ActivityBase2
    {
        public const string PhoneNumber = "PhoneNumber";
        private EditText _editTexAccountNumber;
        private EditText _editTextPhoneNumber;
        private EditText _editTextSerialNumber;
        private Button _buttonPrevious;
        private Button _buttonNext;
        private TextView _tvError;
        private string _entity;
        private string _accountNumber;
        private string _phoneNumber;
        private string _serialNumber;
        private int _startScreenId;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            InitializeScreen();
            UpdateScreen();
            SetListeners();
        }

        public override void SetViewPermissions()
        {
            throw new NotImplementedException();
        }

        public override void InitializeScreen()
        {
            SetContentView(Resource.Layout.fragment_ticket_customeridentity);
            _editTexAccountNumber = FindViewById<EditText>(Resource.Id.editTextAccountNumber);
            _editTextPhoneNumber = FindViewById<EditText>(Resource.Id.editTextPhone);
            _editTextSerialNumber = FindViewById<EditText>(Resource.Id.editTextSerialNumber);
            _buttonNext = FindViewById<Button>(Resource.Id.buttonNext);
            _buttonPrevious = FindViewById<Button>(Resource.Id.buttonPrevious);
            _tvError = FindViewById<TextView>(Resource.Id.tvError);
        }

        public override void RetrieveScreenInput()
        {
            _accountNumber = _editTexAccountNumber.Text.Trim();
            _phoneNumber = _editTextPhoneNumber.Text.Trim();
            _serialNumber = _editTextSerialNumber.Text.Trim();
        }

        public override void UpdateScreen()
        {
            if (!string.IsNullOrEmpty(Intent.GetStringExtra(PhoneNumber)))
            {
                _editTextPhoneNumber.Text = Intent.GetStringExtra(PhoneNumber);
            }
        }

        public override void SetListeners()
        {
            _buttonPrevious.Click += delegate(object sender, EventArgs args)
            {
                base.OnBackPressed();
            };

            _buttonNext.Click += delegate(object sender, EventArgs args)
            {
                RetrieveScreenInput();

                if (!Validate())
                {
                    _tvError.Visibility = ViewStates.Visible;
                    _tvError.Text = GetText(Resource.String.pin_validation_bad_chars);

                    return;
                }

                if (_tvError.Visibility == ViewStates.Visible)
                {
                    _tvError.Text = "";
                    _tvError.Visibility = ViewStates.Invisible;
                }

                //set the entity type and specify the first screen to show for the customer wizard
                _entity = "Customer";
                Intent intent = new Intent(this, typeof(ProcessFlowActivity));

                //Get the Process flow definition from OTA settings
                var processFlowStringDefintion = Settings.Instance.CustomerWizard;

                if (!string.IsNullOrEmpty(processFlowStringDefintion))
                {
                    //start the ticket wizard and pass the entered details
                    intent.PutExtra(ProcessFlowActivity.Entity, _entity);
                    intent.PutExtra(ProcessFlowActivity.AccountNumber, _accountNumber);
                    intent.PutExtra(ProcessFlowActivity.PhoneNumber, _phoneNumber);
                    intent.PutExtra(ProcessFlowActivity.SerialNumber, _serialNumber);
                    intent.PutExtra(ProcessFlowActivity.ProcessFlowStep, processFlowStringDefintion);

                    StartActivity(intent);
                }
                else
                {
                    try
                    {
                        //else load the customer process flow from file if loading from OTA fails
                        using (var sr = new StreamReader(Assets.Open("customerprocessflow.json")))
                        {
                            //load the screen definitions from the json file located in the Assets folder
                            processFlowStringDefintion = sr.ReadToEnd();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }

                    //if we still don't have a process flow definition stored locally
                    if (string.IsNullOrEmpty(processFlowStringDefintion))
                    {
                        //inform the user that there is no process flow defintion
                        AlertDialogBuilder.Instance
                            .AddButton(Resource.String.ok, okButtonHandler)
                            .SetText(0, Resource.String.missing_processflow_definition)
                            .Show(this);
                    }
                    else
                    {
                        Logger.Verbose("Starting Customer Process flow using local definition");
                        //start activity with process defition from file stored in the Assets folder
                        intent.PutExtra(ProcessFlowActivity.Entity, _entity);
                        intent.PutExtra(ProcessFlowActivity.AccountNumber, _accountNumber);
                        intent.PutExtra(ProcessFlowActivity.PhoneNumber, _phoneNumber);
                        intent.PutExtra(ProcessFlowActivity.SerialNumber, _serialNumber);
                        intent.PutExtra(ProcessFlowActivity.ProcessFlowStep, processFlowStringDefintion);

                        StartActivity(intent);
                    }
                }
            };
        }

        private void okButtonHandler()
        {
            return;
        }

        public override bool Validate()
        {
            if (!string.IsNullOrEmpty(_phoneNumber) && !new Regex(@"^[0-9]+$").IsMatch(_phoneNumber))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}