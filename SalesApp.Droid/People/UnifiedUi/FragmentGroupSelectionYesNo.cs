using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Services.Settings;
using SalesApp.Droid.Enums;
using SalesApp.Droid.People.UnifiedUi.Customer;
using SalesApp.Droid.People.UnifiedUi.Prospect;
using SalesApp.Droid.UI.Wizardry;

namespace SalesApp.Droid.People.UnifiedUi
{
    public class FragmentGroupSelectionYesNo : WizardStepFragment
    {
        private bool _yesClicked;
        private Lead _lead;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            this.FragmentView = this.BindingInflate(Resource.Layout.fragment_group_selection_yes_no, container, false);

            this.WizardActivity.ButtonNext.Visibility = ViewStates.Gone;

            Button yesButton = this.FragmentView.FindViewById<Button>(Resource.Id.yesButton);
            yesButton.Click += YesButtonOnClick;
            Button noButton = this.FragmentView.FindViewById<Button>(Resource.Id.noButton);
            noButton.Click += NoButtonOnClick;

            return this.FragmentView;
        }

        private void NoButtonOnClick(object sender, EventArgs eventArgs)
        {
            this._yesClicked = false;
            this._lead.GroupInfo = null;
            this.WizardActivity.GoNext();
        }

        private void YesButtonOnClick(object sender, EventArgs eventArgs)
        {
            this._yesClicked = true;
            this.WizardActivity.GoNext();
        }

        public override void SetData(string serializedString)
        {
            this._lead = JsonConvert.DeserializeObject<SalesApp.Core.BL.Models.People.Customer>(serializedString);
        }

        public override string GetData()
        {
            return JsonConvert.SerializeObject(this._lead);
        }

        public override Type GetNextFragment()
        {
            if (this._yesClicked)
            {
                switch (this.WizardActivity.WizardType)
                {
                    case WizardTypes.CustomerRegistration:
                        return typeof(FragmentCustomerGroupSelection);
                    case WizardTypes.ProspectRegistration:
                        return typeof(FragmentProspectGroupSelection);
                }
            }

            switch (this.WizardActivity.WizardType)
            {
                case WizardTypes.CustomerRegistration:
                    var photoFeatureEnabled = Settings.Instance.PhotoFeatureEnabled;
                    if (photoFeatureEnabled == 1)
                    {
                        return typeof(CustomerPhotoFragment);
                    }
                    return typeof(FragmentCustomerConfirmationScreen);
                case WizardTypes.ProspectRegistration:
                    return typeof(FragmentScoreProspect);
            }

            return null;
        }

        public override int StepTitle
        {
            get
            {
                switch (this.WizardActivity.WizardType)
                {
                    case WizardTypes.CustomerRegistration:
                        return Resource.String.is_customer_in_a_group;
                    case WizardTypes.ProspectRegistration:
                        return Resource.String.is_prospect_in_a_group;
                }

                return 0;
            }
        }

        public override bool BeforeGoNext()
        {
            return true;
        }

        public override bool Validate()
        {
            return true;
        }
    }
}