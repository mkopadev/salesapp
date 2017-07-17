using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.Api.Tickets;
using SalesApp.Core.BL.Models.Tickets;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.Tickets
{
    public class FragmentTicketSummary : FragmentBase2
    {
        private TextView _tvAccountNumber;
        private TextView _tvPhoneNumber;
        private TextView _tvSerialNumber;
        private TextView _tvDescription;
        private TextView _tvAccountNumberValue;
        private TextView _tvPhoneNumberValue;
        private TextView _tvSerialNumberValue;
        private TextView _tvDescriptionValue;
        private View _vLineSeparator;
        private TextView _tvError;
        private Button _buttonPrevious;
        private Button _buttonSubmit;
        private string AccountNumber { set; get; }
        private string Entity { set; get; }
        private string PhoneNumber { set; get; }
        private string SerialNumber { set; get; }
        private string StepAnswerList { set; get; }
        private string OutComeId { set; get; }
        private Ticket Ticket { get; set; }

        public FragmentTicketSummary()
        {
            
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.fragment_ticket_summary, container, false);
            // build the screen
            Logger.Debug("Initializing UI");
            InitializeUI();
            Logger.Debug("Initializing Ticket");
            InitializeTicket();
            UpdateUI();
            SetEventHandlers();
            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Ticket Summary");
            return view;
        }

        protected override void InitializeUI(bool isOnUiThread = false)
        {
            _tvAccountNumber = view.FindViewById<TextView>(Resource.Id.tvAccountNumber);
            _tvPhoneNumber = view.FindViewById<TextView>(Resource.Id.tvPhoneNumber);
            _tvDescription = view.FindViewById<TextView>(Resource.Id.tvDescription);
            _tvSerialNumber = view.FindViewById<TextView>(Resource.Id.tvSerialNumber);
            _tvError = view.FindViewById<TextView>(Resource.Id.tvError);

            _tvAccountNumberValue = view.FindViewById<TextView>(Resource.Id.tvAccountNumberValue);
            _tvPhoneNumberValue = view.FindViewById<TextView>(Resource.Id.tvPhoneNumberValue);
            _tvDescriptionValue = view.FindViewById<TextView>(Resource.Id.tvDescriptionValue);
            _tvSerialNumberValue = view.FindViewById<TextView>(Resource.Id.tvSerialNumberValue);

            _buttonSubmit = view.FindViewById<Button>(Resource.Id.buttonSubmit);
            _buttonPrevious = view.FindViewById<Button>(Resource.Id.buttonPrevious);

            _vLineSeparator = view.FindViewById<View>(Resource.Id.lineSeparator);
        }

        private void InitializeTicket()
        {
            Entity = Arguments.GetString(ProcessFlowActivity.Entity);
            AccountNumber = Arguments.GetString(ProcessFlowActivity.AccountNumber);
            PhoneNumber = Arguments.GetString(ProcessFlowActivity.PhoneNumber);
            SerialNumber = Arguments.GetString(ProcessFlowActivity.SerialNumber);
            StepAnswerList = Arguments.GetString(ProcessFlowActivity.ListStepAnswers);
            OutComeId = Arguments.GetString(ProcessFlowActivity.OutComeId);

            var entityIdentifier = new EntityIdentifier
            {
                accountNumber = AccountNumber,
                phoneNumber = PhoneNumber,
                serialNumber = SerialNumber
            };

            var wizard = new Wizard
            {
                Id = "1",
                OutcomeId = OutComeId,
                StepAnswersList = JsonConvert.DeserializeObject<List<StepAnswer>>(StepAnswerList)
            };

            Ticket = new Ticket
            {
                entity = Entity,
                date = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                entityIdentifier = entityIdentifier,
                wizard = wizard
            };
        }

        public override void UpdateUI(bool calledFromUiThread = false)
        {
            //only make visible the UI elements that have values in them
            if (!string.IsNullOrEmpty(Ticket.entityIdentifier.accountNumber))
            {
                _tvAccountNumberValue.Text = Ticket.entityIdentifier.accountNumber;
                _tvAccountNumber.Visibility = ViewStates.Visible;
                _tvAccountNumberValue.Visibility = ViewStates.Visible;
                _vLineSeparator.Visibility = ViewStates.Visible;
            }

            if (!string.IsNullOrEmpty(Ticket.entityIdentifier.phoneNumber))
            {
                _tvPhoneNumberValue.Text = Ticket.entityIdentifier.phoneNumber;
                _tvPhoneNumber.Visibility = ViewStates.Visible;
                _tvPhoneNumberValue.Visibility = ViewStates.Visible;
                _vLineSeparator.Visibility = ViewStates.Visible;
            }

            if (!string.IsNullOrEmpty(Ticket.entityIdentifier.serialNumber))
            {
                _tvSerialNumberValue.Text = Ticket.entityIdentifier.serialNumber;
                _tvSerialNumber.Visibility = ViewStates.Visible;
                _tvSerialNumberValue.Visibility = ViewStates.Visible;
                _vLineSeparator.Visibility = ViewStates.Visible;
            }

            if (Ticket.wizard.StepAnswersList.Any())
            {
                StringBuilder sb = new StringBuilder();
                Ticket.wizard.StepAnswersList.ForEach(delegate(StepAnswer stepAnswer)
                {
                    sb.Append(stepAnswer.ToString());
                    sb.AppendLine();
                });
                _tvDescriptionValue.Text = sb.ToString();
                _tvDescription.Visibility = ViewStates.Visible;
                _tvDescriptionValue.Visibility = ViewStates.Visible;
            }                      
        }

        protected override void SetEventHandlers()
        {
            _buttonPrevious.Click += delegate(object sender, EventArgs args)
            {
                var ticketSubmissionActivity = (TicketSubmissionActivity)this.Activity;
				ticketSubmissionActivity.OnBackPressed();
            };

            _buttonSubmit.Click += async delegate(object sender, EventArgs args)
            {
                try
                {
                    var ticketSubmissionActivity = (TicketSubmissionActivity)this.Activity;

                    if (ticketSubmissionActivity.ConnectedToNetwork)
                    {
                        _buttonSubmit.Enabled = false;
                        _buttonPrevious.Enabled = false;

                        if (_tvError.Visibility == ViewStates.Visible)
                        {
                            _tvError.Text = "";
                            _tvError.Visibility = ViewStates.Invisible;
                        }

                        //while submitting the ticket, disable some UI elements
                        ticketSubmissionActivity.IsSubmitting = true;
                        if (ticketSubmissionActivity.SupportActionBar != null)
                        {
                            ticketSubmissionActivity.SupportActionBar.SetDisplayHomeAsUpEnabled(false); ;
                            ticketSubmissionActivity.SupportActionBar.SetHomeButtonEnabled(false);
                        }

                        var ticketApi = new TicketApi("ticketV2");
                        var response = await ticketApi.SubmitTicket(Ticket);

                        Logger.Verbose(response.Text);

                        if (response.Success)
                        {
                            //load ticket submit result fragment
                            ticketSubmissionActivity.LoadSubmitResultFragment(response.Text);
                        }
                        else
                        {                        
                            _tvError.Visibility = ViewStates.Visible;
                            _tvError.Text = GetText(Resource.String.issue_fail_submit);                        
                        }

                        ticketSubmissionActivity.IsSubmitting = false;
                        if (ticketSubmissionActivity.SupportActionBar != null)
                        {
                            ticketSubmissionActivity.SupportActionBar.SetDisplayHomeAsUpEnabled(true); ;
                            ticketSubmissionActivity.SupportActionBar.SetHomeButtonEnabled(true);
                        }

                        _buttonSubmit.Enabled = true;
                        _buttonPrevious.Enabled = true;
                    }
                    else
                    {
                        _tvError.Visibility = ViewStates.Visible;
                        _tvError.Text = GetText(Resource.String.issue_fail_internet);
                    }
                }
                catch (Exception e)
                {
                    Logger.Verbose(e.Message);
                }

            };
        }

        public override void SetViewPermissions()
        {
            throw new NotImplementedException();
        }

        public override bool Validate()
        {
            throw new NotImplementedException();
        }
    }
}