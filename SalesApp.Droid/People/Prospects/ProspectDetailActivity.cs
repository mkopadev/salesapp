using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Extensions;
using SalesApp.Core.Extensions.Data;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;
using SalesApp.Core.Services.Notifications;
using SalesApp.Droid.BusinessLogic.Controllers.Security;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Components.UIComponents.CustomInfo;
using SalesApp.Droid.Enums;
using SalesApp.Droid.People.UnifiedUi;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.UI.Wizardry;

namespace SalesApp.Droid.People.Prospects
{
    [Activity(Label = "Prospect Details", Theme = "@style/AppTheme.SmallToolbar", NoHistory = false, ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(ProspectListView))]
    public class ProspectDetailActivity : ActivityBase2,
        CustomDatePickerFragment.CustomDatePickerListener,
        FragmentProspectMain.FragmentProspectMainListener
    {
        CustomDatePickerFragment _customDatePickerFragment;
        public const string CustomDatePickerFragmentTag = "CustomDatePickerFragmentTag";
        private const string MainFragmentTag = "MainFragmentTag";
        private const string ReminderCreatedSuccessfullyTag = "ReminderCreatedSuccessfullyTag";
        public const string ExistingProspectId = "ExistingProspectId";
        public const string ProspectDetailsOrigin = "ProspectDetailsOrigin";
        private ProspectDetailsOrigin _origin;
        private FragmentProspectMain _prospectDetailsMainFragment;
        private ProspectItem _originalProspect, _modifiedProspect;
        private const string DateTimeKey = "DateTimeKey";
        private UiPermissionsController _uiPermissionsController;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.layout_prospect_details);
            this.AddToolbar(Resource.String.prospect_details, true);

            if (bundle != null)
            {
                CreatingReminder = bundle.GetBoolean(ReminderCreatedSuccessfullyTag);
                this.DateTimeSelected = JsonConvert.DeserializeObject<DateTime>(bundle.GetString(DateTimeKey));
            }

            InitializeData();
            InitializeUi();
        }

        public override void SetViewPermissions()
        {
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.OnBackPressed();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }


        protected override void OnResume()
        {
            base.OnResume();
            RefreshUi();
            UIPermissionsController.SetViewsVisibilty();
        }
        
        private async Task<bool> ExitIfConverted()
        {
            Prospect prosp = await ProspectsController.GetByIdAsync(_originalProspect.SearchResult.Id);
            
            if (prosp == null || prosp.Converted)
            {
                RunOnUiThread
                    (
                        () =>
                        {
                            Toast.MakeText(this, Resource.String.already_converted, ToastLength.Long).Show();
                            Finish();
                        }
                    );
                return true;
            }

            return false;
        }

        void InitializeData()
        {
            if (null != Intent)
            {
                Bundle extras = Intent.Extras;
                _originalProspect = extras != null ? JsonConvert.DeserializeObject<ProspectItem>(extras.GetString(ExistingProspectId)) : null;
                _modifiedProspect = _originalProspect;
                bool exited = AsyncHelper.RunSync(async () => await ExitIfConverted());
                if (exited)
                {
                    return;
                }

                string origin = extras == null ? string.Empty : extras.GetString(ProspectDetailsOrigin);

                if (!string.IsNullOrEmpty(origin))
                {
                    Enum.TryParse(origin, out this._origin);
                }

                Task.Run
                    (
                        async () =>
                        {
                            {
                                if (_originalProspect.SearchResult.ReminderTime != default(DateTime))
                                {
                                    CriteriaBuilder criteriaBuilder = new CriteriaBuilder();

                                    ProspectFollowup followup = await new ProspectFollowUpsController()

                                        .GetSingleByCriteria
                                        (
                                            criteriaBuilder
                                                .Add("ProspectId", _originalProspect.SearchResult.Id)
                                                .AddDateCriterion("ReminderTime", _originalProspect.SearchResult.ReminderTime)
                                        );


                                    if (followup != null && followup.Id != default(Guid))
                                    {
                                        await
                                            new NotificationsCoreService().SetSingleNotificationViewed(
                                                followup.TableName,
                                                followup.Id.ToString());
                                    }
                                }
                            }
                        }
                    );

            }
        }

        /// <summary>
        /// Initialize the UI
        /// </summary>
        private void InitializeUi()
        {
            SetTitle(Resource.String.prospect_details);
            var trans = this.GetFragmentManager().BeginTransaction();
            var prospectFrag = this.GetFragmentManager().FindFragmentByTag(MainFragmentTag);
            if (prospectFrag == null)
            {
                this._prospectDetailsMainFragment = new FragmentProspectMain();
                this._prospectDetailsMainFragment.SetArgument(FragmentProspectMain.ProspectBundleKey, this._originalProspect);
            }
            else
            {
                this._prospectDetailsMainFragment = (FragmentProspectMain)prospectFrag;
            }

            // register fragment
            trans.Replace(Resource.Id.prospect_details_placeholder, this._prospectDetailsMainFragment, MainFragmentTag);
            trans.Show(this._prospectDetailsMainFragment);
            trans.Commit();
        }

        public void onCustomDatePickerDestroyed()
        {
            InitializeUi();
        }

        private void RefreshUi(bool isOnUiThread = false)
        {
            if (!isOnUiThread)
            {
                RunOnUiThread
                    (
                        () =>
                        {
                            RefreshUi(true);
                        }
                    );
                return;
            }

            _prospectDetailsMainFragment.UpdateView(DateTimeSelected);
            SetTitle(Resource.String.prospect_details);
        }

        public void onCustomDatePickerSelected(DateTime selection, bool success = true)
        {
            this.DateTimeSelected = selection;
            InitializeUi();
            RefreshUi();
        }

        public bool CreatingReminder { get; set; }

        public DateTime DateTimeSelected { get; set; }


        protected override void OnDestroy()
        {
            Intent returnIntent = new Intent();
            bool modified = _originalProspect == _modifiedProspect;
            returnIntent.PutExtra("modified", modified);
            SetResult(Result.Ok, returnIntent);

            base.OnDestroy();
        }

        public async void onActionSelected(int selection)
        {
            var trans = GetFragmentManager().BeginTransaction();
            switch (selection)
            {

                case 0:
                    this.Finish();
                    break;
                case 1:
                    RefreshUi();
                    InitializeUi();
                    break;

                case 2:
                    Prospect props = _originalProspect.SearchResult.CastTo<Prospect>();
                    SaveResponse<Prospect> saveResult = await ProspectsController.SaveAsync(props);
                    if (saveResult.SavedModel == null || saveResult.SavedModel.Id == default(Guid))
                    {
                        throw new Exception("Could not save prospect");
                    }
                    break;

                case 3:
                    // App trackking
                    GoogleAnalyticService.Instance.TrackEvent(GetString(Resource.String.prospect_details), GetString(Resource.String.prospect_details), "Convert Prospect");

                    Dictionary<string, object> bundledItems = new Dictionary<string, object>
                    {
                        { FragmentBasicInfo.KeyProspectIdBundled, _originalProspect.SearchResult }
                    };

                    // if we accessed prospect details from a reminder, then return to home after the conversion

                    IntentStartPointTracker.IntentStartPoint startPoint = IntentStartPointTracker.IntentStartPoint.ProspectConversion;

                    if (this._origin == Enums.ProspectDetailsOrigin.ProspectReminderClick)
                    {
                        startPoint = IntentStartPointTracker.IntentStartPoint.WelcomeScreen;
                    }

                    WizardLauncher.Launch
                        (
                            this,
                            WizardTypes.CustomerRegistration,
                            startPoint,
                            bundledItems
                        );
                    Finish();
                    break;
                case 10:
                    CreatingReminder = true;
                    trans.Hide(_prospectDetailsMainFragment);
                    trans.Show(_customDatePickerFragment);
                    trans.Commit();
                    SetTitle(Resource.String.create_reminder);

                    break;
                case 11:
                    CreatingReminder = true;
                    SetTitle(Resource.String.edit_reminder);
                    trans.Hide(_prospectDetailsMainFragment);
                    trans.Show(_customDatePickerFragment);
                    trans.Commit();
                    break;
            }

        }
        private ProspectsController ProspectsController
        {
            get
            {
                if (_prospectsController == null)
                {
                    _prospectsController = new ProspectsController();
                }
                return _prospectsController;
            }
        }
        public ProspectsController _prospectsController { get; set; }

        private UiPermissionsController UIPermissionsController
        {
            get
            {
                if (_uiPermissionsController == null)
                {
                    _uiPermissionsController = new UiPermissionsController(this);
                }
                return _uiPermissionsController;
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString(DateTimeKey, JsonConvert.SerializeObject(this.DateTimeSelected));
        }

        public override void InitializeScreen()
        {
            throw new NotImplementedException();
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
    }
}