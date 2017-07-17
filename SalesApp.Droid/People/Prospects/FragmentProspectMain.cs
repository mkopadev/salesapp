using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Components.UIComponents.CustomInfo;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.Services.Phone;

namespace SalesApp.Droid.People.Prospects
{
    // TODO clea up this class

    /// <summary>
    /// The main prospect fragment
    /// </summary>
    public class FragmentProspectMain : FragmentBase3
    {
        /// <summary>
        /// The key to use to retrieve the prospect from the bundle
        /// </summary>
        public static readonly string ProspectBundleKey = "ProspectBundleKey";

        private ImageButton btnCallProspect, btnConvertToCustomer;
        private Button btnCreateReminder;
        private TextView tvProspectTitle, tvProspectPhone, tvProspectDetailsReminderTime, tvProspectDetailsReminderDate;
        private ImageView imgEditReminder, imgDeleteReminder, imgmoney, imgneed, imgauth;
        private RelativeLayout detailsReminderActionsLayout;
        private bool _callInitiated = false;
        private RelativeLayout ConvertToCustomerBtn;
        private bool _creatingRemider = false;
        public ProspectItem prospect { get; set; }

        public override void SetViewPermissions()
        {
            throw new NotImplementedException();
        }

        private View view { get; set; }

        private FragmentProspectMainListener _prospectListner;

        private ProspectDetailActivity parentActivity;
        public interface FragmentProspectMainListener
        {
            void onActionSelected(int selection);
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            _prospectListner = (FragmentProspectMainListener) activity;
            parentActivity = (ProspectDetailActivity) activity;
        }

        /// <summary>
        /// Create the view that this fragment displays
        /// </summary>
        /// <param name="inflater">The inflator</param>
        /// <param name="container">The container view</param>
        /// <param name="savedInstanceState">The saved state if any</param>
        /// <returns>Returns the inflated view</returns>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // grb the arguments
            if (savedInstanceState !=null && savedInstanceState.GetString(ProspectBundleKey) != null)
            {
                this.prospect = JsonConvert.DeserializeObject<ProspectItem>(savedInstanceState.GetString(ProspectBundleKey));
            }
            else
            {
                this.prospect = this.GetArgument<ProspectItem>(ProspectBundleKey);
            }


            // App trackking
            GoogleAnalyticService.Instance.TrackScreen(Activity.GetString(Resource.String.prospect_details));


            this.view = inflater.Inflate(Resource.Layout.layout_prospect_details_main, container, false);
            this.InitializeUI();
            this.SetEventHandlers();

            return this.view;
        }

        public override void OnResume()
        {
            base.OnResume();
            //this.InitializeUI();
            this.InitializeData();
        }

        public override void UpdateUI(bool calledFromUiThread = false)
        {
        }

        protected override void SetEventHandlers()
        {
            btnCreateReminder.Click += CreateReminder;
            btnCallProspect.Click += CallProspect;
            ConvertToCustomerBtn.Click += ConvertCustomer;

            if (imgEditReminder.Visibility == ViewStates.Visible)
            {
                imgEditReminder.Click += EditReminder;
            }

            if (imgDeleteReminder.Visibility == ViewStates.Visible)
            {
                imgDeleteReminder.Click += DeleteReminder;
            }
        }

        public override bool Validate()
        {
            return true;
        }

        protected override void InitializeUI(bool isOnUiThread = false)
        {
             btnCreateReminder = view.FindViewById<Button>(Resource.Id.bcreate_reminder);
             btnCreateReminder.Enabled = true; 
             btnCallProspect  = view.FindViewById<ImageButton>(Resource.Id.callProspectBtn);
             btnConvertToCustomer = view.FindViewById<ImageButton>(Resource.Id.convertBtn);
             imgEditReminder = view.FindViewById<ImageView>(Resource.Id.prospect_details_edit);
             imgDeleteReminder = view.FindViewById<ImageView>(Resource.Id.prospect_details_delete);
             tvProspectDetailsReminderTime = view.FindViewById<TextView>(Resource.Id.prospect_details_reminder_time);
             tvProspectDetailsReminderDate = view.FindViewById<TextView>(Resource.Id.prospect_details_reminder_date);
             detailsReminderActionsLayout = view.FindViewById<RelativeLayout>(Resource.Id.pdetails_reminder_set_actions);
             ConvertToCustomerBtn = view.FindViewById<RelativeLayout>(Resource.Id.relConverter);

             if (!_callInitiated)
             {
                 if (prospect.SearchResult.ReminderTime == default(DateTime))
                 {
                     detailsReminderActionsLayout.Visibility = ViewStates.Gone;
                 }
                 else
                 {
                     detailsReminderActionsLayout.Visibility = ViewStates.Visible;
                 }

                 tvProspectTitle = view.FindViewById<TextView>(Resource.Id.prospect_title);

                 imgmoney = view.FindViewById<ImageView>(Resource.Id.img_money);
                 imgneed = view.FindViewById<ImageView>(Resource.Id.img_need);
                 imgauth = view.FindViewById<ImageView>(Resource.Id.img_auth);

                 tvProspectPhone = view.FindViewById<TextView>(Resource.Id.prospect_phoneno);

                 InitializeData();
             }
             else
             {
                 _callInitiated = false;
             }

            //LoadDatePicker();
        }

        private void LoadDatePicker()
        {
            var frag = this.FragmentManager.FindFragmentByTag(ProspectDetailActivity.CustomDatePickerFragmentTag);
            if (frag != null)
            {
                CustomDatePickerFragment datePickerFragment = (CustomDatePickerFragment) frag;
                if (datePickerFragment.FinishedCreatingReminder)
                {
                    
                }
            }
        }

        void InitializeData()
        {
            tvProspectPhone.Text = prospect.SearchResult.Phone;
            tvProspectTitle.Text = prospect.SearchResult.FullName;

            if (!prospect.SearchResult.Money)
                imgmoney.SetImageResource(Resource.Drawable.error);
            if (!prospect.SearchResult.Need)
                imgneed.SetImageResource(Resource.Drawable.error);
            if (!prospect.SearchResult.Authority)
                imgauth.SetImageResource(Resource.Drawable.error);

            if (prospect.SearchResult.ReminderTime != DateTime.MinValue || prospect.SearchResult.ReminderTime != default(DateTime))
            {
                tvProspectDetailsReminderTime.Text = prospect.SearchResult.ReminderTime.ToString(" hh:mm tt");
                tvProspectDetailsReminderDate.Text = prospect.SearchResult.ReminderTime.ToString("dd/MM/yy");

                UpdateView(prospect.SearchResult.ReminderTime);
            }
        }

        void CreateReminder(object sender, EventArgs e)
        {
            _creatingRemider = true;
            var tran = this.FragmentManager.BeginTransaction();
            CustomDatePickerFragment customDatePickerFragment = null;

            // App trackking
            GoogleAnalyticService.Instance.TrackEvent(Activity.GetString(Resource.String.prospect_details), Activity.GetString(Resource.String.prospect_details), "Create Reminder");

            var frag = this.FragmentManager.FindFragmentByTag(ProspectDetailActivity.CustomDatePickerFragmentTag);
            if (frag == null)
            {
                customDatePickerFragment = new CustomDatePickerFragment();
                Bundle b = new Bundle();
                b.PutString(CustomDatePickerFragment.PROSPECT_FOLLOWUP_KEY, JsonConvert.SerializeObject(this.prospect.SearchResult.Id));
                customDatePickerFragment.Arguments = b;
                // register fragment
            }
            else
            {
                customDatePickerFragment = (CustomDatePickerFragment)frag;
            }
            Activity.SetTitle(Resource.String.create_reminder);
            tran.Hide(this);
            tran.Add(Resource.Id.prospect_details_placeholder, customDatePickerFragment, ProspectDetailActivity.CustomDatePickerFragmentTag);
            tran.Commit();
        }

        void EditReminder(object sender, EventArgs e)
        {
            var tran = this.FragmentManager.BeginTransaction();
            CustomDatePickerFragment customDatePickerFragment = null;

            // App trackking
            GoogleAnalyticService.Instance.TrackEvent(Activity.GetString(Resource.String.prospect_details), Activity.GetString(Resource.String.prospect_details), "Edit Reminder");


            var frag = this.FragmentManager.FindFragmentByTag(ProspectDetailActivity.CustomDatePickerFragmentTag);
            if (frag == null)
            {
                customDatePickerFragment = new CustomDatePickerFragment();
                Bundle b = new Bundle();
                b.PutString(CustomDatePickerFragment.PROSPECT_FOLLOWUP_KEY, JsonConvert.SerializeObject(this.prospect.SearchResult.Id));
                customDatePickerFragment.Arguments = b;
                // register fragment
            }
            else
            {
                customDatePickerFragment = (CustomDatePickerFragment)frag;
            }

            Activity.SetTitle(Resource.String.edit_reminder);
            tran.Hide(this);
            tran.Add(Resource.Id.prospect_details_placeholder, customDatePickerFragment, ProspectDetailActivity.CustomDatePickerFragmentTag);
            tran.Commit();
        }

        private void DeleteReminder(object sender, EventArgs e)
        {
            // App trackking
            GoogleAnalyticService.Instance.TrackEvent(Activity.GetString(Resource.String.prospect_details), Activity.GetString(Resource.String.prospect_details), "Delete Reminder");

            prospect.SearchResult.ReminderTime = DateTime.MinValue;
            parentActivity.DateTimeSelected = DateTime.MinValue;
            Task.Run(async () => { await DeleteReminder();});
            //_prospectListner.onActionSelected(1);
            UpdateView();
        }

        private async Task DeleteReminder()
        {
            ProspectFollowUpsController followUpsController = new ProspectFollowUpsController();

            List<ProspectFollowup> followups = await followUpsController.GetAllAsync();

            followups = followups.Where(a => a.ProspectId == prospect.SearchResult.Id).ToList();

            foreach (var prospectFollowup in followups)
            {
                await followUpsController.DeleteAsync(prospectFollowup);
            }
        }

        void CallProspect(object sender, EventArgs e)
        {
            // App trackking
            GoogleAnalyticService.Instance.TrackEvent(Activity.GetString(Resource.String.prospect_details), Activity.GetString(Resource.String.prospect_details), "Call Prospect");

            if (prospect.SearchResult.Phone != null)
            {
                _callInitiated = true;
                new CallService().Dial(prospect.SearchResult.Phone, Activity);
            }
        }

        void ConvertCustomer(object sender, EventArgs e)
        {
            _prospectListner.onActionSelected(3);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString(ProspectBundleKey, JsonConvert.SerializeObject(this.prospect));
        }

        private void UpdateView()
        {
            UpdateView(DateTime.MinValue);
        }

        internal void UpdateView(DateTime dateTimeSelected, bool isOnUiThread = false)
        {
            if (!isOnUiThread)
            {
                this.Activity.RunOnUiThread(
                        () =>
                        {
                            this.UpdateView(dateTimeSelected,true);
                        });
                return;
            }

            var viewTime = view.FindViewById<TextView>(Resource.Id.prospect_details_reminder_time);
            var viewDate = view.FindViewById<TextView>(Resource.Id.prospect_details_reminder_date);
            var pdetails_reminder_set_actions = view.FindViewById<RelativeLayout>(Resource.Id.pdetails_reminder_set_actions);
            var reminderImage = view.FindViewById<ImageView>(Resource.Id.imgreminder);
            var reminderLabel = view.FindViewById<TextView>(Resource.Id.prospect_details_reminder_label);
            var button = view.FindViewById<Button>(Resource.Id.bcreate_reminder);

            if (dateTimeSelected != DateTime.MinValue)
            {
                pdetails_reminder_set_actions.Visibility = ViewStates.Visible;
                viewTime.Text = dateTimeSelected.ToString(" hh:mm tt");
                viewDate.Text = dateTimeSelected.ToString("dd/MM/yy");
                reminderLabel.SetTextColor(Color.White);
                reminderImage.Visibility = ViewStates.Visible;
                button.Visibility = ViewStates.Gone;
                LinearLayout reminderBack = view.FindViewById<LinearLayout>(Resource.Id.pdetails_reminder_label);
                Color colour = new Color(99, 99, 99);
                reminderBack.SetBackgroundColor(colour);
            }
            else
            {
                pdetails_reminder_set_actions.Visibility = ViewStates.Gone;
                viewTime.Text = dateTimeSelected.ToString(" hh:mm tt");
                viewDate.Text = dateTimeSelected.ToString("dd/MM/yy");
                reminderLabel.SetTextColor(Color.White);
                reminderImage.Visibility = ViewStates.Gone;
                button.Visibility = ViewStates.Visible;
            }
        }
    }
}