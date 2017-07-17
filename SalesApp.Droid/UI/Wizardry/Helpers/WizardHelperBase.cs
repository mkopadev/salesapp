using System;

namespace SalesApp.Droid.UI.Wizardry.Helpers
{
    public abstract class WizardHelperBase
    {
        /// <summary>
        /// Resource ID for the screen title.
        /// </summary>
        public abstract int ScreenTitle { get; }

        /// <summary>
        /// This property indicates how many steps this wizard has.
        /// </summary>
        public abstract int StepCount { get; }

        /// <summary>
        /// This method returns the type if the first Fragment in the wizard.
        /// </summary>
        /// <returns>Typpe of the first fragment</returns>
        public abstract Type GetFirstFragment();

    }
}