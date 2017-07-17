using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace SalesApp.Droid.UI.Utils
{
    public class AlertDialogFragment : DialogFragment
    {
        public Dictionary<string, Action> Buttons = new Dictionary<string, Action>();

        private int _buttons { get; set; }

        private string _message { get; set; }

        private string _title { get; set; }

        private string _buttonA { get; set; }

        private string _buttonB { get; set; }

        private static Action actionA { get; set; }

        private static Action actionB { get; set; }

        private Button btnContinue;
        private Button btnCancel;
        private TextView txtMessage;

        private static string msg = "MESSAGE";
        private static string tit = "TITLE";
        private static string but = "BUTTON";
        private static string butA = "BUTTON_A";
        private static string butB = "BUTTON_B";

        private static string testMessage;

        public static AlertDialogFragment NewInstance(string message, string title, Dictionary<string, Action> buttons)
        {
            var fragment = new AlertDialogFragment { Arguments = new Bundle() };
            fragment.Arguments.PutString(msg,  message);
            fragment.Arguments.PutString(tit, title);
            fragment.Arguments.PutInt(but, buttons.Count);
            testMessage = message;
            if (buttons.Count == 1)
            {
                fragment.Arguments.PutString(butA, buttons.Keys.ElementAt(0));
                actionA = buttons[buttons.Keys.ElementAt(0)];
            }
            else
            {
                fragment.Arguments.PutString(butA, buttons.Keys.ElementAt(0));
                actionA = buttons[buttons.Keys.ElementAt(0)];
                fragment.Arguments.PutString(butB, buttons.Keys.ElementAt(1));
                actionB = buttons[buttons.Keys.ElementAt(1)];
            }

            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (this.Arguments != null)
            {
                _message = this.Arguments.GetString(msg);
                _title = this.Arguments.GetString(tit);
                _buttons = this.Arguments.GetInt(but);
                if (_buttons == 1)
                {
                    _buttonA = this.Arguments.GetString(butA);
                }
                else
                {
                    _buttonA = this.Arguments.GetString(butA);
                    _buttonB = this.Arguments.GetString(butB);
                }

                if (string.IsNullOrEmpty(_title))
                {
                    this.SetStyle(DialogFragmentStyle.NoTitle, 0);
                }

                Cancelable = false;
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View rootView = inflater.Inflate(Resource.Layout.fragment_alert_dialog_fragement_builder, container, false);

            if (this._buttons == 2)
            {
                btnContinue = (Button)rootView.FindViewById(Resource.Id.btnContinue);
                if (!string.IsNullOrEmpty(_buttonA))
                {
                    btnContinue.Text = _buttonA.ToUpper();
                }

                btnCancel = (Button)rootView.FindViewById(Resource.Id.btnCancel);
                if (!string.IsNullOrEmpty(_buttonB))
                {
                    btnCancel.Text = _buttonB.ToUpper();
                }

                btnContinue.Click += (sender, args) => actionA();
                btnCancel.Click += (sender, args) => actionB();
            }
            else
            {
                rootView = inflater.Inflate(Resource.Layout.fragment_alert_dialog_fragement_builder_one, container, false);
                btnContinue = (Button)rootView.FindViewById(Resource.Id.btnContinue);

                if (!string.IsNullOrEmpty(_buttonA))
                {
                    btnContinue.Text = _buttonA.ToUpper();
                }

                btnContinue.Click += (sender, args) => actionA();
            }

            txtMessage = (TextView)rootView.FindViewById(Resource.Id.txtAlertMessage);
            txtMessage.Text = _message;
            return rootView;
        }
    }
}