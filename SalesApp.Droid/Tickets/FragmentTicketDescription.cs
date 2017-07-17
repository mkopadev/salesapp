using System;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.Tickets
{
    public class FragmentTicketDescription : FragmentBase2
    {
        //private TextView _tvTitle;
        //private TextView _tvTitleValue;
        private EditText _editTextDescription;
        private Button _buttonPrevious;
        private Button _buttonNext;

        public string Description { set; get; }

        public FragmentTicketDescription()
        {
            
        }

        public FragmentTicketDescription(string ticketDescription)
        {
            Description = ticketDescription;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.fragment_ticket_description, container, false);
            // build the screen
            Logger.Debug("Initializing UI");
            InitializeUI();
            UpdateUI();
            SetEventHandlers();

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Ticket Description");

            return view;
        }

        public override bool Validate()
        {
            if (string.IsNullOrEmpty(_editTextDescription.Text.Trim()))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected override void InitializeUI(bool isOnUiThread = false)
        {
            if (!isOnUiThread)
            {
                Activity.RunOnUiThread
                    (
                        () => InitializeUI(true)
                    );
                return;
            }

            _editTextDescription = view.FindViewById<EditText>(Resource.Id.editTextDescription);
            _buttonNext = view.FindViewById<Button>(Resource.Id.buttonNext);
            _buttonPrevious = view.FindViewById<Button>(Resource.Id.buttonPrevious);
            //_tvTitle = view.FindViewById<TextView>(Resource.Id.tVAddCustomer);
            //_tvTitleValue = view.FindViewById<TextView>(Resource.Id.tVAddCustomer);
        }

        public override void UpdateUI(bool calledFromUiThread = false)
        {
            if (Description != null)
            {
                _editTextDescription.Text = Description;
                if (!string.IsNullOrEmpty(Description))
                {
                    _buttonNext.Enabled = true;
                }
            }
        }

        protected override void SetEventHandlers()
        {
            var ticketSubmissionActivity = (TicketSubmissionActivity)this.Activity;

            _editTextDescription.TextChanged += editTextDescription_TextChanged;

            _buttonPrevious.Click += delegate(object sender, EventArgs args)
            {
                // TODO add handler to return back to previous activity
                ticketSubmissionActivity.OnBackPressed();
            };

            _buttonNext.Click += delegate(object sender, EventArgs args)
            {
                // TODO add handler to proceed to load ticket summary fragment
                Description = _editTextDescription.Text.Trim();
                ticketSubmissionActivity.LoadSummaryFragment(_editTextDescription.Text.Trim());
            };
        }

        public override void SetViewPermissions()
        {
            throw new NotImplementedException();
        }

        void editTextDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            _buttonNext.Enabled = (!string.IsNullOrEmpty((_editTextDescription.Text)));
        }
    }
}