using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Newtonsoft.Json;
using SalesApp.Core.Api.DownSync;
using SalesApp.Core.Api.Tickets;
using SalesApp.Core.BL.Controllers.Synchronization;
using SalesApp.Core.BL.Models.TicketList;
using SalesApp.Core.BL.Models.Tickets;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.GAnalytics;
using Exception = System.Exception;
using StringBuilder = System.Text.StringBuilder;

namespace SalesApp.Droid.Tickets
{
    public class ProcessFlowSummaryFragment : FragmentBase3
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

        private ITicketSubmissionListener _ticketSubmissionListerListener;

        public interface ITicketSubmissionListener
        {
            // Invoke this method in the host activity
            void ShowPostSubmissionFragment(string ticketReferenceNumber);
        }

        public ProcessFlowSummaryFragment()
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
            GoogleAnalyticService.Instance.TrackScreen("Process Flow Summary");
            return view;
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);

            if (activity is ITicketSubmissionListener)
            {
                _ticketSubmissionListerListener = (ITicketSubmissionListener)activity;
            }
            else
            {
                throw new ClassCastException(activity.ToString() + " must implement ProcessFlowSummaryFragment.ITicketSubmissionListener");
            }
        }

        public override bool Validate()
        {
            throw new NotImplementedException();
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

        protected override void SetEventHandlers()
        {
            _buttonPrevious.Click += delegate(object sender, EventArgs args)
            {
                Activity.OnBackPressed();
            };

            _buttonSubmit.Click += async delegate(object sender, EventArgs args)
            {
                if (Activity is ProcessFlowActivity)
                {
                    var hostActivity = (ProcessFlowActivity)Activity;

                    try
                    {
                        if (hostActivity.ConnectedToNetwork)
                        {
                            _buttonSubmit.Enabled = false;
                            _buttonPrevious.Enabled = false;

                            if (_tvError.Visibility == ViewStates.Visible)
                            {
                                _tvError.Text = "";
                                _tvError.Visibility = ViewStates.Invisible;
                            }

                            //while submitting the ticket, disable some UI elements
                            hostActivity.IsSubmitting = true;
                            if (hostActivity.SupportActionBar != null)
                            {
                                hostActivity.SupportActionBar.SetDisplayHomeAsUpEnabled(false); ;
                                hostActivity.SupportActionBar.SetHomeButtonEnabled(false);
                            }

                            var ticketApi = new TicketApi("ticketV2");
                            var response = await ticketApi.SubmitTicket(Ticket);

                            Logger.Verbose(response.Text);

                            if (response.Success)
                            {
                                //load ticket submit result fragment
                                _ticketSubmissionListerListener.ShowPostSubmissionFragment(response.Text);

                                // Fetch tickets automatically in the background
                                if (Ticket.entity == "Dealership Operator")
                                {
                                    new SyncingController().SyncDownAsync<DownSyncServerResponse<DsrTicket>, DsrTicket>();
                                }
                                else if (Ticket.entity == "Customer")
                                {
                                    new SyncingController().SyncDownAsync<DownSyncServerResponse<CustomerTicket>, CustomerTicket>();
                                }
                            }
                            else
                            {
                                _tvError.Visibility = ViewStates.Visible;
                                _tvError.Text = GetText(Resource.String.issue_fail_submit);
                            }

                            hostActivity.IsSubmitting = false;
                            if (hostActivity.SupportActionBar != null)
                            {
                                hostActivity.SupportActionBar.SetDisplayHomeAsUpEnabled(true); ;
                                hostActivity.SupportActionBar.SetHomeButtonEnabled(true);
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
                }
                else
                {
                    throw new ClassCastException(Activity.ToString() + " must implement ProcessFlowSummaryFragment.ITicketSubmissionListener");
                }
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

        public override void SetViewPermissions()
        {
            throw new NotImplementedException();
        }
    }
}