using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Exceptions.Validation.People;
using SalesApp.Core.Extensions;
using SalesApp.Droid.Components.UIComponents.DateAndTime;
using SalesApp.Droid.Enums;
using SalesApp.Droid.People.Customers;
using SalesApp.Droid.People.Prospects;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.UI.Wizardry;

namespace SalesApp.Droid.People.UnifiedUi.Prospect
{
    public class FragmentFollowupReminder : WizardStepFragment, IOverlayParent
    {
        private SalesApp.Core.BL.Models.People.Prospect _personRegistrationInformation;
        private Button _btnDate;
        private Button _btnTime;
        private DateTime _reminderTime = DateTime.Now;
        private WizardOverlayFragment _registrationFinishedFragment;

        private Guid ProspectId { get; set; }

        public override int PreviousButtonText
        {
            get
            {
                return Resource.String.prospect_followup_skip;
            }
        }

        public override int NextButtonText
        {
            get
            {
                return Resource.String.prospect_followup_set;
            }
        }

        public override int StepTitle
        {
            get { return Resource.String.prospect_registered_title; }
        }

        public override GravityFlags TitleGravity
        {
            get
            {
                return GravityFlags.Center;
            }
        }

        public override Action OnNextClicked
        {
            get
            {
                return this.NextClick;
            }
        }

        public override Action OnPreviousClicked
        {
            get
            {
                return this.PrevClick;
            }
        }

        public override bool BeforeGoNext()
        {
            return true;
        }

        private Button BtnDate
        {
            get
            {
                if (_btnDate == null)
                {
                    _btnDate = this.FragmentView.FindViewById<Button>(Resource.Id.btnDate);
                }

                return _btnDate;
            }
        }

        private Button BtnTime
        {
            get
            {
                if (_btnTime == null)
                {
                    _btnTime = FragmentView.FindViewById<Button>(Resource.Id.btnTime);
                }

                return _btnTime;
            }
        }

        private async void NextClick()
        {
            bool succeeded = await CreateReminder();
            ShowOverlay(succeeded);
        }

        private void ShowOverlay(bool reminderSet)
        {
            string posButtonTxt = WizardActivity.GetString(Resource.String.add_new_prospect);
            string message = WizardActivity.GetString(Resource.String.successful_registration_message_prospect);

            if (reminderSet)
            {
                message = WizardActivity.GetString(Resource.String.prospect_followup_reminder_set);
            }

            _registrationFinishedFragment = new RegistrationFinishedFragment();
            Bundle arguments = new Bundle();
            arguments.PutBoolean(RegistrationFinishedFragment.WasRegistrationKey, false);
            arguments.PutBoolean(RegistrationFinishedFragment.SuccessKey, true);
            arguments.PutString(RegistrationFinishedFragment.MessageKey, message);
            arguments.PutString(RegistrationFinishedFragment.BtnPositiveKey, posButtonTxt);
            arguments.PutString(RegistrationFinishedFragment.BtnNegativeKey, this.OverlayNegativeButtonText);
            arguments.PutString(RegistrationFinishedFragment.IntentStartPointKey, WizardActivity.StartPoint.ToString());
            arguments.PutInt(RegistrationFinishedFragment.TitleResKey, Resource.String.prospect_registered_title);

            _registrationFinishedFragment.Arguments = arguments;

            WizardActivity.ShowOverlay(_registrationFinishedFragment, true);

        }

        private void PrevClick()
        {
            ShowOverlay(false);
        }

        public void BtnTime_Click(object sender, EventArgs e)
        {
            FragmentTimePicker dialogTime = new FragmentTimePicker(_reminderTime);
            dialogTime.TimeSelected += DialogTime_TimeSelected;
            dialogTime.Show(FragmentManager, FragmentTimePicker.TagFragmentTimePicker);
        }

        void BtnDate_Click(object sender, EventArgs e)
        {
            FragmentDatePicker dialogDate = new FragmentDatePicker(_reminderTime);
            dialogDate.DateSelected += DialogDate_DateSelected;
            dialogDate.Show(FragmentManager, FragmentDatePicker.TagDateFragment);
        }

        void DialogDate_DateSelected(object sender, EventArgs e)
        {
            SetDate(DateTime.Parse(sender.ToString()));
        }

        public void DialogTime_TimeSelected(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            DateTime time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, e.HourOfDay, e.Minute, 0);
            SetTime(time);
        }

        private void SetTime(DateTime time)
        {
            BtnTime.Text = time.GetTimeStandardFormat();
            _reminderTime = new DateTime(_reminderTime.Year, _reminderTime.Month, _reminderTime.Day, time.Hour, time.Minute, time.Second);

            SetNextButtonState();
        }

        private void SetDate(DateTime date)
        {
            BtnDate.Text = date.GetDateStandardFormat();
            _reminderTime = new DateTime(date.Year, date.Month, date.Day, _reminderTime.Hour, _reminderTime.Minute, _reminderTime.Second);

            SetNextButtonState();
        }

        void SetNextButtonState()
        {
            string setTimePrompt = Resources.GetString(Resource.String.prospect_followup_set_time_prompt);
            string setDatePrompt = Resources.GetString(Resource.String.prospect_followup_set_date_prompt);

            WizardActivity.ButtonNextEnabled = _reminderTime > DateTime.Now && BtnTime.Text != setTimePrompt && BtnDate.Text != setDatePrompt;
        }

        public override void SetData(string serializedString)
        {
            _personRegistrationInformation =
                JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Prospect>(
                    serializedString);

            this.ProspectId = _personRegistrationInformation.Id;
        }

        public override string GetData()
        {
            return JsonConvert.SerializeObject(_personRegistrationInformation);
        }

        public override Type GetNextFragment()
        {
            return default(Type);
        }

        public override bool Validate()
        {
            return true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            this.FragmentView = inflater.Inflate(Resource.Layout.fragment_prospect_followup, container, false);

            BtnDate.Click += BtnDate_Click;
            BtnTime.Click += BtnTime_Click;

            // App tracking
            GoogleAnalyticService.Instance.TrackScreen("Prospect reminder");

            return this.FragmentView;
        }

        private async Task<bool> CreateReminder()
        {
            try
            {
                await new ProspectFollowUpsController()
                    .SaveFollowupAsync
                    (
                        new ProspectFollowup
                        {
                            ProspectId = this.ProspectId
                            ,
                            ReminderTime = _reminderTime
                        }
                    );
                return true;
            }
            catch (ProspectFollowUpInvalidException exception)
            {
                // OwnerActivity.PositiveAction = OwnerActivity.HideMessage;
                int message = exception.ValidationResult == ProspectFollowUpValidationResultsEnum.PastTime
                    ? Resource.String.prospect_followup_past_time_story
                    : Resource.String.prospect_followup_past_date_story;
                int title = message == Resource.String.prospect_followup_past_date_story
                    ? Resource.String.prospect_followup_past_date_title
                    : Resource.String.prospect_followup_past_time_title;

                // OwnerActivity.ShowMessage(Resource.Id.modalContent, title, message, Resource.String.ok);
            }
            catch (Exception)
            {
                throw;
            }

            return false;
        }

        public void PositiveAction()
        {
            WizardLauncher.Launch(Activity, WizardTypes.ProspectRegistration, WizardActivity.StartPoint);
        }

        public void NegativeAction()
        {
            if (WizardActivity.StartPoint == IntentStartPointTracker.IntentStartPoint.Modules)
            {
                // Go back home
                Intent intent = new Intent(this.Activity, typeof(HomeView));
                this.StartActivity(intent);
            }

            Activity.Finish();
        }
    }
}