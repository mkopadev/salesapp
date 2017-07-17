using System;
using SalesApp.Core.Services.Settings;
using SalesApp.Droid.People.UnifiedUi.Customer;

namespace SalesApp.Droid.UI.Wizardry.Helpers
{
    public class CustomerRegistrationWizardHelper : WizardHelperBase
    {
        public override Type GetFirstFragment()
        {
            return typeof(CustomerFragmentBasicInfo);
        }

        public override int ScreenTitle
        {
            get { return Resource.String.unified_register_customer_wizard_title; }
        }

        public override int StepCount
        {
            get
            {
                int photoFeatureEnabled = Settings.Instance.PhotoFeatureEnabled;
                return (photoFeatureEnabled == 1) ? 4 : 3;
            }
        }

    }
}