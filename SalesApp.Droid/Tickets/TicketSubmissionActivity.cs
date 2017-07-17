using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid.Tickets
{
    [Activity(Label = "Raise Issue", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = false, ParentActivity = typeof(HomeView))]
    public class TicketSubmissionActivity : ActivityBase2
    {
        private const string TicketSubmissionTag = "TicketSubmissionTag";
        private const string SummaryFragmentTag = "SummaryFragmentTag";
        private const string SubmitResultFragmentTag = "SubmitResultFragmentTag";

        public string AccountNumber { set; get; }
        public string Description { set; get; }
        public string Entity { set; get; }
        public string PhoneNumber { set; get; }
        public string SerialNumber { set; get; }
        public string OutComeId { set; get; }
        public bool IsSubmitting { set; get; }
        public string StepAnswerList { set; get; }

        private FragmentTicketSummary TicketSummaryFragmentInstance
        {
            get
            {
                var fragmentTicketSummary = new FragmentTicketSummary();
                
                Bundle fragmentBundle = new Bundle();
                fragmentBundle.PutString(ProcessFlowActivity.Entity, Entity);
                fragmentBundle.PutString(ProcessFlowActivity.AccountNumber, AccountNumber);
                fragmentBundle.PutString(ProcessFlowActivity.PhoneNumber, PhoneNumber);
                fragmentBundle.PutString(ProcessFlowActivity.SerialNumber, SerialNumber);
                fragmentBundle.PutString(ProcessFlowActivity.ListStepAnswers, StepAnswerList);
                fragmentBundle.PutString(ProcessFlowActivity.OutComeId, OutComeId);

                fragmentTicketSummary.Arguments = fragmentBundle;

                return fragmentTicketSummary;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            InitializeScreen();
        }

        public override void OnBackPressed()
        {
            if (IsSubmitting)
            {
                return;
            }

            var submitResultFragment = this.GetFragmentManager().FindFragmentByTag(SubmitResultFragmentTag);

            if (submitResultFragment == null)
            {
                base.OnBackPressed();
            }
            else
            {
                if (submitResultFragment.IsVisible)
                {
                    return;
                }
                else
                {
                    base.OnBackPressed();
                }
            }
        }

        public override void SetViewPermissions()
        {
            throw new NotImplementedException();
        }

        public override void InitializeScreen()
        {
            SetContentView(Resource.Layout.activity_ticketsubmission);
            InitializeTicket();
            LoadFragment(TicketSummaryFragmentInstance, Resource.Id.layout_ticketsubmission, SummaryFragmentTag);
        }

        public override void RetrieveScreenInput()
        {
            throw new NotImplementedException();
        }

        public override void UpdateScreen()
        {
            throw new NotImplementedException();
        }

        public override void SetListeners()
        {
            throw new NotImplementedException();
        }

        public override bool Validate()
        {
            throw new NotImplementedException();
        }

        private void InitializeTicket()
        {
            Entity = Intent.GetStringExtra(ProcessFlowActivity.Entity);
            AccountNumber = Intent.GetStringExtra(ProcessFlowActivity.AccountNumber);
            PhoneNumber = Intent.GetStringExtra(ProcessFlowActivity.PhoneNumber);
            SerialNumber = Intent.GetStringExtra(ProcessFlowActivity.SerialNumber);
            StepAnswerList = Intent.GetStringExtra(ProcessFlowActivity.ListStepAnswers);
            OutComeId = Intent.GetStringExtra(ProcessFlowActivity.OutComeId);
        }

        public void LoadSummaryFragment(string description)
        {
            //Description = description;
            //Ticket.wizard.description = description;
            //ReplaceFragment(TicketSummaryFragmentInstance, Resource.Id.layout_ticketsubmission, SummaryFragmentTag);
			var fragmentTx = this.GetFragmentManager().BeginTransaction();
			fragmentTx.Add(Resource.Id.layout_ticketsubmission, TicketSummaryFragmentInstance, SummaryFragmentTag);
			fragmentTx.CommitAllowingStateLoss ();
        }

        /// <summary>
        /// Load the result of the ticket submission
        /// </summary>
        /// <param name="message">The message to display</param>
        public void LoadSubmitResultFragment(string message)
        {
            var fragmentTicketSubmitResult = new FragmentTicketSubmitResult();
            var bundle = new Bundle();
            bundle.PutString(FragmentTicketSubmitResult.MessageKey, message);

            fragmentTicketSubmitResult.Arguments = bundle;

            ReplaceFragment(fragmentTicketSubmitResult, Resource.Id.layout_ticketsubmission, SubmitResultFragmentTag);
        }
    }
}