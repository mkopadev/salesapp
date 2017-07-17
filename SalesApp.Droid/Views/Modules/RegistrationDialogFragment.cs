using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using SalesApp.Droid.Enums;
using SalesApp.Droid.People.Prospects;
using SalesApp.Droid.UI.Wizardry;

namespace SalesApp.Droid.Views.Modules
{
    public class RegistrationDialogFragment : DialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View dialogView = inflater.Inflate(Resource.Layout.layout_modules_registration_dialog, container, false);

            Button addProspect = dialogView.FindViewById<Button>(Resource.Id.add_prospect);

            addProspect.Click += (sender, args) =>
            {
                this.Dismiss();

                WizardLauncher.Launch(this.Activity, WizardTypes.ProspectRegistration,
                    IntentStartPointTracker.IntentStartPoint.Modules);
            };

            Button addCustomer = dialogView.FindViewById<Button>(Resource.Id.add_customer);

            addCustomer.Click += (sender, args) =>
            {
                this.Dismiss();

                WizardLauncher.Launch(this.Activity, WizardTypes.CustomerRegistration,
                    IntentStartPointTracker.IntentStartPoint.Modules);
            };

            Button cancel = dialogView.FindViewById<Button>(Resource.Id.cancel_button);

            cancel.Click += (sender, args) =>
            {
                this.Dismiss();
            };

            return dialogView;
        }
    }
}