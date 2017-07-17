using System;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.People.Prospects;

namespace SalesApp.Droid.UI.Wizardry
{
    public abstract class WizardStepFragment : MvxFragmentBase
    {
        /// <summary>
        /// Bundle key for the registration information payload
        /// </summary>
        protected const string BundledRegistrationInfo = "BundledRegistrationInfo";

        private Activity _activity;

        /// <summary>
        /// Gets this fragment's activity instance
        /// </summary>
        protected ActivityBase ActivityBase
        {
            get
            {
                return this.Activity as ActivityBase;
            }
        }

        /// <summary>
        /// Gets the text for overlay negative button
        /// </summary>
        protected string OverlayNegativeButtonText
        {
            get
            {
                switch (this.ActivityBase.StartPointIntent)
                {
                    case IntentStartPointTracker.IntentStartPoint.CustomerList:
                        return this.WizardActivity.GetString(Resource.String.prospect_followup_return_to_customer_list);
                    case IntentStartPointTracker.IntentStartPoint.ProspectsList:
                        return this.WizardActivity.GetString(Resource.String.prospect_followup_return_to_prospect_list);
                    case IntentStartPointTracker.IntentStartPoint.WelcomeScreen:
                        return WizardActivity.GetString(Resource.String.return_to_home);
                    case IntentStartPointTracker.IntentStartPoint.TicketList:
                        return this.WizardActivity.GetString(Resource.String.ticket_list_return_to_ticket_list);
                    case IntentStartPointTracker.IntentStartPoint.ProspectConversion:
                        return this.WizardActivity.GetString(Resource.String.prospect_followup_return_to_prospect_list);
                    default:
                        return this.WizardActivity.GetString(Resource.String.return_to_home);
                }
            }
        }

        public IWizardActivity WizardActivity
        {
            get
            {
                return _activity as IWizardActivity;
            }
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            _activity = activity;
        }

        public override void OnResume()
        {
            base.OnResume();

            WizardActivity wizardActivity = this._activity as WizardActivity;

            if (wizardActivity == null)
            {
                return;
            }

            wizardActivity.SetWizardTitle();
        }

        public abstract void SetData(string serializedString);

        public abstract string GetData();

        public abstract Type GetNextFragment();

        public abstract int StepTitle { get; }

        public virtual void FinishWizard()
        {  
        }

        public virtual GravityFlags TitleGravity
        {
            get { return GravityFlags.Left; }
        }

        public virtual int NextButtonText
        {
            get
            {
                return Resource.String.wizard_button_next;
            }
        }

        public virtual int PreviousButtonText
        {
            get
            {
                return Resource.String.wizard_button_prev;
            }
        }

        public virtual Action OnNextClicked { get; set; }

        public virtual Action OnPreviousClicked { get; set; }

        public abstract bool BeforeGoNext();

        /// <summary>
        /// Async method wrapper for BeforeGoNext();
        /// </summary>
        /// <returns>True, when allowed to go next, otherwise false</returns>
        public async virtual Task<bool> BeforeGoNextAsync()
        {
            return await Task.Run(() => BeforeGoNext());
        }

        public abstract bool Validate();

        /// <summary>
        /// Returns true if this fragment is the last one in the wizard
        /// </summary>
        public bool IsLastStep
        {
            get
            {
                if (this.GetNextFragment() == default(Type))
                {
                    return true;
                }

                return false;
            }
        }

        public void SetViewVisibility(View view, ViewStates state)
        {
            if (Activity != null)
            {
                Activity.RunOnUiThread(() => view.Visibility = state);
            }
        }
    }
}