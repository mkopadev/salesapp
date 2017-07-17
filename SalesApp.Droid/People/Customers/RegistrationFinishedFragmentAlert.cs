using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Settings;
using SalesApp.Droid.People.Prospects;
using SalesApp.Droid.Services.GAnalytics;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using Uri = Android.Net.Uri;

namespace SalesApp.Droid.People.Customers
{
    public class RegistrationFinishedFragmentAlert : DialogFragment
    {
        public interface IRegistrationActions
        {
            void PositiveAction();
            void NegativeAction();
        }

        private IRegistrationActions RegistrationAction;

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
        private Type goHomeType;

        public static readonly string WasRegistrationKey = "WasRegistrationKey";
        public static readonly string SuccessKey = "SuccessKey";
        public static readonly string TitleResKey = "TitleResKey";
        public static readonly string MessageKey = "MessageKey";
        public static readonly string IntentStartPointKey = "IntentStartPointKey";
        public static readonly string BtnPositiveKey = "BtnPositiveKey";
        public static readonly string BtnNegativeKey = "BtnNegativeKey";
        public static readonly string Retries = "Retries";
        public const string CustomerDetailFragmentTag = "CustomerDetailFragmentTag";

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            CustomerDetailView act = activity as CustomerDetailView;

            if (act == null)
            {
                return;
            }
            var frag = this.FragmentManager.FindFragmentByTag(getFragmentTag(Resource.Id.pager, 0));
            RegistrationAction = (CustomerDetailFragment)frag;
        }

        private String getFragmentTag(int viewPagerId, int fragmentPosition)
        {
            return "android:switcher:" + viewPagerId + ":" + fragmentPosition;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.SetStyle(StyleNoTitle, 0);
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

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.layout_resend_registration_done, container, false);

            TextView title = view.FindViewById<TextView>(Resource.Id.txtTitle);
            TextView message = view.FindViewById<TextView>(Resource.Id.txtMessage);
            btnCallHq = view.FindViewById<Button>(Resource.Id.btnCallHq);

            _positive = (Button)view.FindViewById(Resource.Id.btnAddCustomer);
            if (!String.IsNullOrEmpty(_btnPositive))
            {
                _positive.Text = _btnPositive;
            }

            _negative = (Button)view.FindViewById(Resource.Id.btnGoHome);
            if (!String.IsNullOrEmpty(_btnNegative))
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
                // App trackking
                GoogleAnalyticService.Instance.TrackScreen(Activity.GetString(Resource.String.customer_resend_registration_complete_fallback));
                if (this._success)
                {
                    setImage(Resource.Drawable.sync_channel_full_success);
                    title.SetText(Resource.String.sms_registration_done_title);
                    message.Text = this.Activity.GetString(Resource.String.sms_registration_done);
                    _negative.Visibility = ViewStates.Gone;
                    _positive.Text = this.Activity.GetString(Resource.String.return_to_customer_details);
                }
                else
                {
                    title.SetText(Resource.String.sms_registration_failed_title);
                    setImage(Resource.Drawable.ic_failed);
                    if (_retries < 3)
                    {
                        message.Text = this._message;
                        _negative.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        message.Text = this._message;
                        _negative.Visibility = ViewStates.Gone;
                        _positive.Text = this.Activity.GetString(Resource.String.return_to_customer_details);
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
                if (!String.IsNullOrEmpty(_message))
                {
                    message.Text = _message;
                }

                btnCallHq.Visibility = ViewStates.Gone;
                setImage(Resource.Drawable.sync_channel_full_success);
                message.Text = this._message;
                _negative.Visibility = ViewStates.Gone;
                _positive.Text = this.Activity.GetString(Resource.String.return_to_customer_details);
            }
            else
            {
                title.SetText(Resource.String.customer_registration_failed_title);
                message.Text = this._message;
                btnCallHq.Visibility = ViewStates.Gone;
                setImage(Resource.Drawable.sync_channel_fallback_fail);
            }

            return view;
        }

        private void ShowCallHelpButton()
        {
            btnCallHq.Visibility = ViewStates.Visible;
            btnCallHq.Text  = Settings.Instance.DealerSupportLine;
            btnCallHq.Click += (sender, args) =>
            {
                var uri = Uri.Parse("tel:" + btnCallHq.Text);
                var intent = new Intent(Intent.ActionDial, uri);
                this.StartActivity(intent);
            };
        }

        private void ClickPositive()
        {
            this.Dismiss();
            RegistrationAction.PositiveAction();
        }

        private void ClickNegative()
        {
            this.Dismiss();
            RegistrationAction.NegativeAction();
        }

        public override void OnPause()
        {
            base.OnPause();
            // this.Dismiss();
        }
    }
}   