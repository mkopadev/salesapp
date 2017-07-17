using System;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.Tickets
{
    public class FragmentTicketSubmitResult : FragmentBase3
    {
        /// <summary>
        /// The key to use to retrieve the message from the bundle
        /// </summary>
        public static readonly string MessageKey = "TicketReferenceNumber";

        private TextView _tvTicketSubmitResult;
        private Button _btnAddTicket;
        private Button _btnGoHome;

        private string _submitResult;

        public FragmentTicketSubmitResult()
        {
            
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.fragment_ticket_success, container, false);

            if (Arguments != null)
            {
                _submitResult = Arguments.GetString(MessageKey);
            }

            Logger.Debug("Initializing UI");
            InitializeUI();
            UpdateUI();
            SetEventHandlers();
            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Ticket Submit Result");
            return view;
        }

        protected override void InitializeUI(bool isOnUiThread = false)
        {
            _tvTicketSubmitResult = view.FindViewById<TextView>(Resource.Id.tvTicketSubmitResult);
            _btnAddTicket = view.FindViewById<Button>(Resource.Id.btnAddTicket);
            _btnGoHome = view.FindViewById<Button>(Resource.Id.btnGoHome);
        }

        public override void UpdateUI(bool calledFromUiThread = false)
        {
            _tvTicketSubmitResult.Text = _submitResult;
        }

        protected override void SetEventHandlers()
        {
            _btnAddTicket.Click += delegate(object sender, EventArgs args)
            {
                // raise another ticket
                Intent intent = new Intent(this.Activity, typeof(TicketStartActivity));
                intent.AddFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
                Activity.Finish();

            };

            _btnGoHome.Click += delegate(object sender, EventArgs args)
            {
                //go to the home page
                Intent intent = new Intent(this.Activity, typeof(HomeView));
                intent.AddFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
                Activity.Finish();
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