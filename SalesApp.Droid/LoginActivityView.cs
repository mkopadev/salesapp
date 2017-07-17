using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using SalesApp.Core.Services.Security;
using SalesApp.Droid.Adapters.Auth.ResetPin;
using SalesApp.Droid.UI.Utils;

namespace SalesApp.Droid
{
    /// <summary>
    /// This class controls the Login screen functionality.
    /// </summary>
    [Activity(Theme = "@style/AppTheme.SmallToolbar", ScreenOrientation = ScreenOrientation.Portrait, NoHistory = false)]
    public class LoginActivityView : LoginActivityBase
    {
        private TextView _txtForgotPin;
        private TextView _txtLoginMessage;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.layout_login);
            this.AddToolbar(Resource.String.welcome, true);
            this.Toolbar.NavigationIcon = null;

            this.InitializeScreen();
            this.RetrieveScreenInput();
            this.UpdateScreen();
            this.SetListeners();

            this.LoginService = new LoginService();
        }

        public override void SetViewPermissions()
        {
        }

        public override void ContinueToWelcome()
        {
            EditPin.Text = string.Empty;
            Intent intent = new Intent(this, typeof(HomeView));
            StartActivity(intent);
        }

        public override void HideLoginResultMessage()
        {
            _txtLoginMessage.Visibility = ViewStates.Gone;
        }

        public override void InitializeScreen()
        {
            _txtForgotPin = FindViewById<TextView>(Resource.Id.txtForgotPin);
            EditPin = FindViewById<EditText>(Resource.Id.editPin);
            TxtPin1 = FindViewById<TextView>(Resource.Id.txtPin1);
            TxtPin2 = FindViewById<TextView>(Resource.Id.txtPin2);
            TxtPin3 = FindViewById<TextView>(Resource.Id.txtPin3);
            TxtPin4 = FindViewById<TextView>(Resource.Id.txtPin4);
            LoginButton = FindViewById<Button>(Resource.Id.btnLogIn);
            _txtLoginMessage = FindViewById<TextView>(Resource.Id.txtLoginMessage);
            TxtPinArray = new TextView[4];
            TxtPinArray[0] = TxtPin1;
            TxtPinArray[1] = TxtPin2;
            TxtPinArray[2] = TxtPin3;
            TxtPinArray[3] = TxtPin4;

            // set focus of screen to Pin Edit
            EditPin.RequestFocus();
            ChangeFocusColor();
        }

        public override void UpdateScreen()
        {
            // disable login if not 4 pin numbers
            LoginButton.Enabled = PinLength >= 4;
            ShowLoginResultMessage(
                ConnectedToNetwork ? Resource.String.logging_in_online : Resource.String.logging_in_offline, true);
        }

        public override void SetListeners()
        {
            base.SetListeners();

            // reset PIN click
            _txtForgotPin.Click += (sender, args) =>
            {
                ResetPinClick();
            };

            // Login event handler
            LoginButton.Click += (sender, args) =>
            {
                LoginClick();
            };
        }

        public override bool Validate()
        {
            if (EnteredPin.Length != 4)
            {
                return false;
            }

            return true;
        }

        public async void LoginClick()
        {
            // get input
            RetrieveScreenInput();

            // validate before continue
            if (!Validate())
            {
                ShowLoginResultMessage(GetString(Resource.String.enter_pin_4_digits));
                return;
            }

            // form is valid, please continue
            if (ConnectedToNetwork)
            {
                await LoginOnline(true);
            }
            else
            {
                await LoginOffline();
            }
        }

        public void ResetPinClick()
        {
            HideLoginResultMessage();
            if (!ConnectedToNetwork)
            {
                ShowLoginResultMessage(GetString(Resource.String.reset_pin_need_internet));
                return;
            }

            AlertDialogBuilder.Instance
                           .AddButton(Resource.String.yes, ResetPassword)
                           .AddButton(Resource.String.no, DontResetPassword)
                           .SetText(null, GetString(Resource.String.resetting_pin))
                           .Show(this);
        }

        public override void ShowLoginResultMessage(string text, bool isInformational)
        {
            // set the type of box to show
            _txtLoginMessage.SetBackgroundResource(isInformational
                ? Resource.Drawable.list_box_information2
                : Resource.Drawable.list_box_error);

            _txtLoginMessage.Text = text;
            Animation fadeInAnimation = AnimationUtils.LoadAnimation(this, Resource.Animation.fade_in);

            //wrongPin.setVisibility(View.VISIBLE);
            _txtLoginMessage.Visibility = ViewStates.Visible;
            _txtLoginMessage.StartAnimation(fadeInAnimation);

        }

        public void ResetPassword()
        {
            Intent intent = new Intent(this, typeof(ResetPinActivity));
            StartActivityForResult(intent, 2);
        }

        public void DontResetPassword()
        {
            // Do nothing
        }
    }
}