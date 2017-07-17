using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using SalesApp.Core.Extensions;
using SalesApp.Droid.UI.Wizardry;

namespace SalesApp.Droid.Components.UIComponents
{
    public class ProgressFragment : WizardOverlayFragment
    {
        public event EventHandler ContentChanged;

        public TextView ProgressMessage { get; set; }
        public TextView ProgressMessageDetails { get; set; }

        public static readonly string TitleKey = "TitleKey";
        public static readonly string MessageKey = "MessageKey";
        public static readonly string DetailsKey = "DetailsKey";


        private string title;
        private string message;
        private string details;

        public ProgressFragment()
        {
            
        }


/*
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ProgressFragment(int titleRes, int progressInformationRes)
        {
            this.TitleRes = titleRes;
            this.ProgressInformationRes = progressInformationRes;
            this.ProgressInformationDetailRes = -1;
        }



        public ProgressFragment(string title, string message)
        {
            _title = title;
            _message = message;
            this.ProgressInformationDetailRes = -1;
        }

        /// <summary>
        /// Constructor including detailed message.
        /// </summary>
        public ProgressFragment(int titleRes, int progressInformationRes, int progressInformationDetailRes)
        {
            this.TitleRes = titleRes;
            this.ProgressInformationRes = progressInformationRes;
            this.ProgressInformationDetailRes = progressInformationDetailRes;
        }*/
/*
        public void UpdateContent(string message, string detail)
        {

            ProgressMessage = (TextView)view.FindViewById(Resource.Id.progress_message);
            ProgressMessage.Text = message;
            ProgressMessageDetails = (TextView)view.FindViewById(Resource.Id.progress_message_detail);
            if (string.IsNullOrEmpty(detail))
                ProgressMessageDetails.Visibility = ViewStates.Gone;
            else
                ProgressMessageDetails.Text = detail;
            
        }*/
/*
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }*/
/*

        void Update(string message, string detail)
        {
            
        }
*/


        public override bool Validate()
        {
            return true;
        }

        protected override void InitializeUI(bool isOnUiThread = false)
        {
            UpdateUI();
        }

        public override void UpdateUI(bool calledFromUiThread = false)
        {
            // Set the title
            if (Activity.ActionBar != null)
            {
                Activity.ActionBar.Title = title;
            }   

            // set the progress message
            ProgressMessage = (TextView)view.FindViewById(Resource.Id.progress_message);
            if (ProgressMessage != null)
            {
                ProgressMessage.Text = title;
            }
            
            ProgressMessageDetails = (TextView)view.FindViewById(Resource.Id.progress_message_detail);
            if (message.IsBlank())
            { 
                ProgressMessageDetails.Visibility = ViewStates.Gone;
            }
            else
            {
                ProgressMessageDetails.Text = message;
            }
        }

        protected override void SetEventHandlers()
        {
            
        }

        public override void SetViewPermissions()
        {
            
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            view = inflater.Inflate(Resource.Layout.layout_progress, container, false);
            
            InitializeUI();
            SetEventHandlers();
            
            return view;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (this.Arguments != null)
            {

                title = this.Arguments.GetString(TitleKey);
                message = this.Arguments.GetString(MessageKey);
                details = this.Arguments.GetString(DetailsKey);
            }

            /*if (savedInstanceState != null)
            {
                _actionBarTitle = savedInstanceState.GetString(actionBarTitleKey);
                _title = savedInstanceState.GetString(titleKey);
                content = savedInstanceState.GetString(contentKey);
                btnPositiveTitle = savedInstanceState.GetString(btnPositiveKey);
                btnNegativeTitle = savedInstanceState.GetString(btnNegativeKey);
                _imageResource = savedInstanceState.GetInt(imageResourceKey);
            }*/
        }

    }
}