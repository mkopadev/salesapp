using System.Collections.Generic;
using Android.Content;
using Newtonsoft.Json;
using SalesApp.Droid.Enums;
using SalesApp.Droid.People.Prospects;

namespace SalesApp.Droid.UI.Wizardry
{
    public static class WizardLauncher
    {
        public static void Launch(Context context, WizardTypes wizardType, IntentStartPointTracker.IntentStartPoint startPoint, Dictionary<string, object> bundledItems = null)
        {
            Intent intent = new IntentStartPointTracker().GetIntentWithTracking(context, typeof (WizardActivity), startPoint);
            
            intent.PutExtra(WizardActivity.BundledWizardType, (int)wizardType);
            if (bundledItems != null)
            {
                intent.PutExtra(WizardActivity.KeyBundledItems, JsonConvert.SerializeObject(bundledItems));
            }
            
            context.StartActivity(intent);
        }
    }
}