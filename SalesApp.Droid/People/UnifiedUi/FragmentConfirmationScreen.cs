using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using SalesApp.Core.BL.Models.Chama;
using SalesApp.Core.BL.Models.People;
using SalesApp.Droid.People.Customers;
using SalesApp.Droid.People.Prospects;
using SalesApp.Droid.UI.Wizardry;

namespace SalesApp.Droid.People.UnifiedUi
{
    /// <summary>
    /// This is the confirmation screen
    /// </summary>
    public abstract class FragmentConfirmationScreen : WizardStepFragment
    {
        protected ListView GroupList;
        protected ConfirmationScreenAdapter Adapter;
        /// <summary>
        /// Text to show on the contextual next button
        /// </summary>
        public override int NextButtonText
        {
            get
            {
                return Resource.String.unified_register;
            }
        }

        /// <summary>
        /// Gets the lead that we are selecting the product for
        /// </summary>
        protected abstract Lead Lead { get; }

        protected abstract List<GroupKeyValue> ConfirmationDetials { get; }

        /// <summary>
        /// Creates the view for this fragment
        /// </summary>
        /// <param name="inflater">The inflator</param>
        /// <param name="container">The container</param>
        /// <param name="savedInstanceState">The saved state</param>
        /// <returns>The view</returns>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.FragmentView = inflater.Inflate(Resource.Layout.fragment_confirmation_screen, container, false);

            GroupList = this.FragmentView.FindViewById<ListView>(Resource.Id.group_info);

            this.WizardActivity.ButtonNext.Visibility = ViewStates.Visible;
            this.WizardActivity.ButtonNext.Enabled = true;

            Adapter = new ConfirmationScreenAdapter(Activity, ConfirmationDetials);
            GroupList.Adapter = Adapter;

            return this.FragmentView;
        }

        /// <summary>
        /// Called when registration is cancelled
        /// </summary>
        public void OnCancel()
        {
            if (WizardActivity.StartPoint == IntentStartPointTracker.IntentStartPoint.Modules)
            {
                // Go back home
                Intent intent = new Intent(this.Activity, typeof(HomeView));
                this.StartActivity(intent);
            }

            this.Activity.Finish();
        }

        /// <summary>
        /// Validate fragment data
        /// </summary>
        /// <returns>True if validation passes, false otherwise</returns>
        public override bool Validate()
        {
            if (this.Lead.Product == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// What happens before we go next
        /// </summary>
        /// <returns>The validation before going to the next fragment</returns>
        public override bool BeforeGoNext()
        {
            return true;
        }

        /// <summary>
        /// Get the next fragment
        /// </summary>
        /// <returns>The next fragment to load</returns>
        public override Type GetNextFragment()
        {
            return default(Type);
        }
    }
}