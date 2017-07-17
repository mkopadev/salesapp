using System;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using SalesApp.Core.Enums.Validation;
using SalesApp.Core.Services.Settings;
using SalesApp.Core.Validation;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid.Adapters.Auth.ResetPin
{
    public class ResetPinFragment : FragmentBase3 
    {
        public event EventHandler<EventArgs> ButtonNextClicked;

        private Button BtnResetPin { get; set; }
        private EditText EditPhone { get; set; }
        private TextView TvStory { get; set; }
        //private TextView txtPhoneError { get; set; }


       

        private View view { get; set; }


        public override void SetViewPermissions()
        {
            throw new NotImplementedException();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.layout_reset_pin_enter_phone, container,false);
            InitializeUI();
            SetEventHandlers();
            ActivityBase activityBase = (Activity as ActivityBase);
            if (activityBase != null)
            {
                activityBase.ShowKeyboard(EditPhone);
                activityBase.SetScreenTitle(Resource.String.reset_pin);
            }

            
            
            return view;
        }


        public override bool Validate()
        {
            return true;
        }

        protected override void InitializeUI(bool isOnUiThread = false)
        {
            BtnResetPin = view.FindViewById<Button>(Resource.Id.btnResetPin);
            EditPhone = view.FindViewById<EditText>(Resource.Id.editTextPhone);
            TvStory = view.FindViewById<TextView>(Resource.Id.tvStory);
            //TextView header = view.FindViewById<TextView>(Resource.Id.txtHeading);


            //header.ResponseText = GetText(Resource.String.reset_pin);
            EditPhone.RequestFocus();
            //editPhone.ResponseText = settings.DsrPhone;
            EditPhone.ImeOptions = ImeAction.Done;
            EditPhone.EditorAction += edtPhone_EditorAction;


            /*txtPhoneError.Visibility = ViewStates.Invisible;
            btnCancel.Enabled = true;*/
        }

        void edtPhone_EditorAction(object sender, TextView.EditorActionEventArgs e)
        {
            if (e.ActionId == ImeAction.Done)
            {
                if (BtnResetPin.Enabled)
                {
                    btnNext_Click(sender,e);
                }
            }
        }

        public override void UpdateUI(bool calledFromUiThread = false)
        {
            throw new NotImplementedException();
        }

        protected override void SetEventHandlers()
        {

            BtnResetPin.Click += btnNext_Click;
            EditPhone.TextChanged += edtPhone_TextChanged;
            
        }

        

        void edtPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            BtnResetPin.Enabled = true;
        }
        
        void btnNext_Click(object sender, EventArgs e)
        {
            ActivityBase parent = Activity as ActivityBase;
            parent.HideKeyboard();
            //txtPhoneError.Visibility = ViewStates.Invisible;
            if (!parent.ConnectedToNetwork)
            {
                parent.DisplayNetworkRequiredAlert(Resource.String.network_connection_required, Resource.String.ok);
                return;
            }
            PhoneValidationResultEnum phoneValidationResult =
                new PeopleDetailsValidater().ValidatePhoneNumber(EditPhone.Text);
            if(phoneValidationResult != PhoneValidationResultEnum.NumberOk)
            {
                TvStory.SetTextColor(Color.Red);
                int errorRes = 0;
                switch (phoneValidationResult)
                {
                    case PhoneValidationResultEnum.InvalidCharacters:
                        errorRes = Resource.String.pin_validation_bad_chars;
                        break;
                    case PhoneValidationResultEnum.InvalidFormat:
                        errorRes = Resource.String.pin_validation_invalid_format;
                        break;
                    case PhoneValidationResultEnum.NullEntry:
                        errorRes = Resource.String.pin_validation_null;
                        break;
                    case PhoneValidationResultEnum.NumberTooLong:
                        errorRes = Resource.String.pin_validation_long;
                        break;
                    case PhoneValidationResultEnum.NumberTooShort:
                        errorRes = Resource.String.pin_validation_short;
                        break;

                }
                TvStory.Text = GetText(errorRes);
                parent.ShowKeyboard(EditPhone);
                return;
            }

            Settings.Instance.DsrPhone = EditPhone.Text;
            if (ButtonNextClicked != null)
            {
                this.ButtonNextClicked.Invoke(this, e);
            }
        }

        


        
    }
}