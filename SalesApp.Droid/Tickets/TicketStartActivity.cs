using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Java.Interop;
using SalesApp.Core.Services.OtaSettings;
using SalesApp.Core.Services.Settings;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.People.Prospects;
using SalesApp.Droid.UI.Utils;

namespace SalesApp.Droid.Tickets
{
    [Activity(Label = "Raise Issue", ParentActivity = typeof(HomeView))]
    public class TicketStartActivity : ActivityBase
    {
        private string _entity;
        private int _startScreenId;
        private IntentStartPointTracker.IntentStartPoint _intentStartPoint;
        private LocalOtaService _localOtaService;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.activity_ticketstart);
            //AddToolbar(Resource.String.report_problem);
            _intentStartPoint = new IntentStartPointTracker().GetStartPoint(Intent);
        }

        public override void SetViewPermissions()
        {
            //throw new NotImplementedException();
        }

        [Export("onCustomerButtonClicked")]
        public void onCustomerButtonClicked(View v)
        {
            //start the customer identity activity
            Intent intent = new Intent(this, typeof(TicketCustomerIdentityActivity));
            StartActivity(intent);
        }

        [Export("onDSRButtonClicked")]
        public void onDSRButtonClicked(View v)
        {
            _entity = "Dealership Operator";

            //create an intent and pass the arguments for the dsr process flow
            Intent intent = new Intent(this, typeof(ProcessFlowActivity));
            intent.PutExtra("Entity", _entity);

            //get the Process flow definition from OTA settings
            var processFlowStringDefintion = Settings.Instance.DsrWizard;

            //start activity with process defition from OTA settings (if it exists)
            if (!string.IsNullOrEmpty(processFlowStringDefintion))
            {
                intent.PutExtra(ProcessFlowActivity.ProcessFlowStep, processFlowStringDefintion);
                StartActivity(intent);
            }
            else
            {
                try
                {
                    //else load the dsr process flow from file if loading from OTA fails
                    using (var sr = new StreamReader(Assets.Open("dsrprocessflow.json")))
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
                    Logger.Verbose("Starting Dsr Process flow using local definition");
                    //start activity with process defition from file stored in the Assets folder
                    intent.PutExtra(ProcessFlowActivity.ProcessFlowStep, processFlowStringDefintion);
                    StartActivity(intent);
                }
            }
        }

        private void okButtonHandler()
        {
            return;
        }
    }
}