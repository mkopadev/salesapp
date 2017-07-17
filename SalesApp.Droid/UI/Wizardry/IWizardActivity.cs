using System.Collections.Generic;
using Android.Widget;
using SalesApp.Droid.Enums;
using SalesApp.Droid.People.Prospects;

namespace SalesApp.Droid.UI.Wizardry
{
    public interface IWizardActivity
    {
        void ShowOverlay(WizardOverlayFragment fragment, bool big);

        void HideOverlay(bool showKeyboard);

        bool ButtonNextEnabled { get; set; }

        void ShowWaitInfo(int titleId, int storyId);

        void ShowWaitInfo(string title, string story);

        void HideWait();

        string GetString(int resourceId);

        Dictionary<string, object> BundledItems { get; set; }

        void GoNext();

        void Go(bool goNext);

        Button ButtonNext { get; }

        WizardTypes WizardType { get; }

        IntentStartPointTracker.IntentStartPoint StartPoint { get; }

        WizardStepFragment CurrentFragment { get; set; }

        bool IsProspectConversion { get; set; }
    }
}