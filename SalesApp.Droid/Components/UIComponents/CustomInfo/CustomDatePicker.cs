using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Exceptions.Validation.People;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Database;
using SalesApp.Droid.Components.UIComponents.DateAndTime;

namespace SalesApp.Droid.Components.UIComponents.CustomInfo
{
    public class CustomDatePickerFragment : FragmentBase
    {
        DatePicker datePicker;
        TimePicker timePicker;
        private Button _btnDate;
        private Button _btnTime;
        private DateTime _reminderTime;
        private Button btnCreateReminder, btnCancelReminder;
        private View view;
        private ProspectFollowup _followup;

        private ProspectFollowUpsController _followUpsController;
        public static string PROSPECT_FOLLOWUP_KEY = "PROSPECT_FOLLOWUP";

        public bool FinishedCreatingReminder { get; set; }

        private CustomDatePickerListener DatePickerListner;
        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            DatePickerListner = (CustomDatePickerListener) activity;
        }

        public CustomDatePickerFragment()
        {
        }

        public interface CustomDatePickerListener
        {
            void onCustomDatePickerSelected(DateTime selection, bool success = true);

            void onCustomDatePickerDestroyed();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            view = inflater.Inflate(Resource.Layout.layout_custom_datepicker, container, false);
            Guid prospctId = JsonConvert.DeserializeObject<Guid>(this.Arguments.GetString(PROSPECT_FOLLOWUP_KEY));
            if (savedInstanceState != null)
            {
                FinishedCreatingReminder = savedInstanceState.GetBoolean("Finished");
            }

            SetFollowup(prospctId);
            InitializeUI();
            SetEventHandlers();
            return view;
            
        }

        protected override void SetEventHandlers()
        {
            btnCreateReminder.Click += CreateReminder;
            btnCancelReminder.Click += CancelReminder;
            BtnDate.Click += BtnDate_Click;
            BtnTime.Click += BtnTime_Click;
        }

        private void CreateReminder(object sender, EventArgs e)
        {
            Task.Run
                (
                    async () =>
                    {
                        await MakeReminder();
                    }
                );
            DatePickerListner.onCustomDatePickerSelected(getDateFromDatePicker());
        }

        public override void SetViewPermissions()
        {
            
        }

        protected override void InitializeUI(bool isOnUiThread = false)
        {
            btnCreateReminder = view.FindViewById<Button>(Resource.Id.createReminderButton);
            btnCancelReminder = view.FindViewById<Button>(Resource.Id.cancelReminderButton);
        }

        void UpdateCreateButtonEnabledState()
        {
            DateTime date = DateTime.Now;
            view.FindViewById<Button>(Resource.Id.createReminderButton)
                .Enabled = _reminderTime > DateTime.Now
                           && DateTime.TryParse(BtnTime.Text, out date);
        }

        void CancelReminder(object sender, EventArgs e)
        {
           DatePickerListner.onCustomDatePickerDestroyed();
        }

        async Task MakeReminder()
        {
            try
            {
                bool succeeded = await CreateReminder();
                if (succeeded)
                {
                    FireCompleted(false);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        public DateTime getDateFromDatePicker()
        {
            return _reminderTime;
        }

        void BtnDate_Click(object sender, EventArgs e)
        {
            FragmentDatePicker dialogDate = new FragmentDatePicker(_reminderTime);
            dialogDate.DateSelected += dialogDate_DateSelected;
            dialogDate.Show(FragmentManager, FragmentDatePicker.TagDateFragment);
        }

        void BtnTime_Click(object sender, EventArgs e)
        {
            FragmentTimePicker dialogTime = new FragmentTimePicker(_reminderTime);
            dialogTime.TimeSelected += dialogTime_TimeSelected;
            dialogTime.Show(FragmentManager, FragmentTimePicker.TagFragmentTimePicker);
        }

        void dialogTime_TimeSelected(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            DateTime time = new DateTime
                (
                DateTime.Now.Year
                , DateTime.Now.Month
                , DateTime.Now.Day
                , e.HourOfDay
                , e.Minute
                , 0
                );
            SetTime(time);

        }

        void dialogDate_DateSelected(object sender, EventArgs e)
        {
            SetDate(DateTime.Parse(sender.ToString()));
        }

        private void SetDate(DateTime date)
        {
            BtnDate.Text = date.GetDateStandardFormat();
            _reminderTime = new DateTime
                (
                    date.Year
                    , date.Month
                    , date.Day
                    , _reminderTime.Hour
                    , _reminderTime.Minute
                    , _reminderTime.Second
                );
            UpdateCreateButtonEnabledState();
        }

        private void SetTime(DateTime time)
        {
            BtnTime.Text = time.GetTimeStandardFormat();
            _reminderTime = new DateTime
                (
                    _reminderTime.Year
                    , _reminderTime.Month
                    , _reminderTime.Day
                    , time.Hour
                    , time.Minute
                    , time.Second
                );
            if (_reminderTime >= DateTime.Now)
            {
                UpdateCreateButtonEnabledState();
            }
        }


        private Button BtnDate
        {
            get
            {
                if (_btnDate == null)
                {
                    _btnDate = view.FindViewById<Button>(Resource.Id.btnDate);
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
                    _btnTime = view.FindViewById<Button>(Resource.Id.btnTime);
                }
                return _btnTime;
            }
        }

        private ProspectFollowUpsController FollowUpsController
        {
            get
            {
                if (_followUpsController == null)
                {
                    _followUpsController = new ProspectFollowUpsController();
                }
                return _followUpsController;
            }
        }

        private async Task<bool> CreateReminder()
        {
            try
            {
                Logger.Debug("Creating reminder");

                SaveResponse<ProspectFollowup> saveResponse = await FollowUpsController
                    .SaveFollowupAsync
                    (
                        new ProspectFollowup
                        {
                            ProspectId = _followup.ProspectId
                            ,
                            ReminderTime = _reminderTime
                            ,
                            Id = _followup.Id

                        }
                    );
                Logger.Debug("Looks like we finished creating the reminder");
                FinishedCreatingReminder = true;
                return saveResponse.SavedModel.Id != default(Guid);
            }
            catch (ProspectFollowUpInvalidException exception)
            {
                int message = exception.ValidationResult == ProspectFollowUpValidationResultsEnum.PastTime
                    ? Resource.String.prospect_followup_past_time_story
                    : Resource.String.prospect_followup_past_date_story;
                int title = message == Resource.String.prospect_followup_past_date_story
                    ? Resource.String.prospect_followup_past_date_title
                    : Resource.String.prospect_followup_past_time_title;
                new ReusableScreens(Activity)
                    .ShowQuestion(title, title, message, Resource.String.ok, 0);
            }
            catch (Exception e)
            {
                Logger.Debug(e);
            }
            return false;
        }

        public async Task SetFollowup(Guid prospectId)
        {
            string sql = "SELECT * FROM ProspectFollowup pf WHERE pf.ProspectId='" + prospectId + "'";
            List<ProspectFollowup> followUps = await new QueryRunner().RunQuery<ProspectFollowup>(sql);
            this._followup = (followUps != null && followUps.Count > 0) ? followUps.ElementAt(0) : null;
            if (_followup == null)
            {
                _followup = new ProspectFollowup
                {
                    ReminderTime = DateTime.Now,
                    ProspectId = prospectId
                };
            }
            SetDate(_followup.ReminderTime);
            SetTime(_followup.ReminderTime);
        }

        public override void UpdateUI(bool calledFromUiThread = false)
        {
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutBoolean("Finished", FinishedCreatingReminder);
            outState.PutString(PROSPECT_FOLLOWUP_KEY, JsonConvert.SerializeObject(_followup));
        }
    }
}