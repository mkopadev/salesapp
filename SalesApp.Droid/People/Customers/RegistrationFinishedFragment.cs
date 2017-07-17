using System;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Settings;
using SalesApp.Droid.People.Prospects;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.UI.Wizardry;
using Uri = Android.Net.Uri;

namespace SalesApp.Droid.People.Customers
{
    public class RegistrationFinishedFragment : WizardOverlayFragment
    {
        private Button _positive;
        private Button _negative;
        private Button btnCallHq;

        private string _btnPositive;

        private string _btnNegative;

        private static readonly ILog Logger = LogManager.Get(typeof(RegistrationFinishedFragment));
        private bool _wasSmsRegistration = false;
        private bool _success;
        private int _titleRes;
        private string _message;
        private int _retries;
        private IntentStartPointTracker.IntentStartPoint _intentStartPoint;

        public const string WasRegistrationKey = "WasRegistrationKey";
        public const string SuccessKey = "SuccessKey";
        public const string TitleResKey = "TitleResKey";
        public const string MessageKey = "MessageKey";
        public const string IntentStartPointKey = "IntentStartPointKey";
        public const string BtnPositiveKey = "BtnPositiveKey";
        public const string BtnNegativeKey = "BtnNegativeKey";
        public const string Retries = "Retries";

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RetainInstance = true;
            if (this.Arguments != null)
            {
                _wasSmsRegistration = this.Arguments.GetBoolean(WasRegistrationKey);
                _success = this.Arguments.GetBoolean(SuccessKey);
                _titleRes = this.Arguments.GetInt(TitleResKey);
                this._message = this.Arguments.GetString(MessageKey);
                _intentStartPoint = this.Arguments.GetString(IntentStartPointKey).ToEnumValue<IntentStartPointTracker.IntentStartPoint>();
                _btnPositive = this.Arguments.GetString(BtnPositiveKey);
                _btnNegative = this.Arguments.GetString(BtnNegativeKey);
                _retries = this.Arguments.GetInt(Retries);
            }
        }

        public override bool Validate()
        {
            return true;
        }

        protected override void InitializeUI(bool isOnUiThread = false)
        {

        }

        public override void UpdateUI(bool calledFromUiThread = false)
        {

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
            var view = inflater.Inflate(Resource.Layout.layout_registration_done, container, false);

            TextView title = view.FindViewById<TextView>(Resource.Id.txtTitle);
            TextView message = view.FindViewById<TextView>(Resource.Id.txtMessage);
            btnCallHq = view.FindViewById<Button>(Resource.Id.btnCallHq);

            _positive = (Button)view.FindViewById(Resource.Id.btnAddCustomer);
            if (!string.IsNullOrEmpty(_btnPositive))
            {
                _positive.Text = _btnPositive;
            }

            _negative = (Button)view.FindViewById(Resource.Id.btnGoHome);
            if (!string.IsNullOrEmpty(_btnNegative))
            {
                _negative.Text = _btnNegative;
            }

            _positive.Click += delegate
            {
                ClickPositive();
            };
            _negative.Click += delegate
            {
                ClickNegative();
            };

            Action<int> setImage = img =>
            {
                Activity.RunOnUiThread (
                        () =>
                        {
                            ImageView imgView = view.FindViewById<ImageView>(Resource.Id.imgRegistered);
                            if (imgView == null)
                            {
                                return;
                            }
                            imgView.Background = null;
                            imgView.SetBackgroundResource(img);
                        });
            };

            // if SMS registration
            if (this._wasSmsRegistration)
            {
                title.SetText(Resource.String.sms_registration_done_title);
                // message.SetText(Resource.String.sms_registration_done);
                message.Text = this.Activity.GetString(Resource.String.sms_registration_done);
                btnCallHq.Visibility = ViewStates.Gone;

                setImage(Resource.Drawable.sync_channel_full_success);
                if (!this._success)
                {
                    title.SetText(Resource.String.sms_registration_failed_title);
                    setImage(Resource.Drawable.errornew);
                    if (_retries < 3)
                    {
                        message.Text = this._message;
                    }
                    else
                    {
                        message.Text = this._message;
                        ShowCallHelpButton();
                    }
                }
            }
            else if (_success)
            {
                title.SetText(Resource.String.successful_registration);
                if (_titleRes != Resource.String.successful_registration && _titleRes != default(int))
                {
                    title.SetText(_titleRes);
                }

                message.SetText(Resource.String.successful_registration_message);
                if (!string.IsNullOrEmpty(_message))
                {
                    message.Text = _message;
                }

                btnCallHq.Visibility = ViewStates.Gone;
                setImage(Resource.Drawable.sync_channel_full_success);
                
                // App trackking
                GoogleAnalyticService.Instance.TrackScreen(Activity.GetString(Resource.String.customer_registration_complete_data));

            }
            else
            {
                title.SetText(Resource.String.customer_registration_failed_title);
                message.Text = this._message;
                btnCallHq.Visibility = ViewStates.Gone;
                setImage(Resource.Drawable.sync_channel_fallback_fail);
                
                // App trackking
                GoogleAnalyticService.Instance.TrackScreen(Activity.GetString(Resource.String.customer_registration_complete_fallback));

            }

            return view;
        }

        private void ShowCallHelpButton()
        {
            btnCallHq.Visibility = ViewStates.Visible;
            btnCallHq.Text = Settings.Instance.DealerSupportLine;
            btnCallHq.Click += (sender, args) =>
            {
                var uri = Uri.Parse("tel:" + btnCallHq.Text);
                var intent = new Intent(Intent.ActionDial, uri);
                this.StartActivity(intent);
            };
        }

        IOverlayParent OverlayParent
        {
            get
            {
                IOverlayParent myParent = (Activity as WizardActivity).CurrentFragment as IOverlayParent;
                if (myParent == null)
                {
                    throw  new Exception("Unable to determine overlay's parent.");
                }
                return myParent;
            }
        }

        private void ClickPositive()
        {
            this.OverlayParent.PositiveAction();   
        }

        private void ClickNegative()
        {
            this.OverlayParent.NegativeAction();
        }
    }
}