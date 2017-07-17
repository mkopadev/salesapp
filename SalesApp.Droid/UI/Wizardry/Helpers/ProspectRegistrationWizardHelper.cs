using System;
using SalesApp.Droid.People.UnifiedUi.Prospect;

namespace SalesApp.Droid.UI.Wizardry.Helpers
{
    public class ProspectRegistrationWizardHelper : WizardHelperBase
    {
        public override int ScreenTitle
        {
            get { return Resource.String.unified_sms_register_prospect; }
        }

        public override int StepCount
        {
            get { return 5; }
        }

        public override Type GetFirstFragment()
        {
            return typeof(ProspectFragmentBasicInfo);
        }
    }
}