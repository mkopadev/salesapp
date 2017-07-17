using MvvmCross.Core.ViewModels;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.UI.SwipableViews;

namespace SalesApp.Droid.Framework
{
    public abstract class MvxNavigationViewBase<T> : ActivityWithNavigation where T : IMvxViewModel
    {
        /// <summary>
        /// Gets or sets the type-safe view model for this view
        /// </summary>
        public new T ViewModel
        {
            get
            {
                return (T)base.ViewModel;
            }

            set
            {
                base.ViewModel = value;
            }
        }

        public override void SetViewPermissions()
        {
        }

        public override void InitializeScreen()
        {
        }

        public override void RetrieveScreenInput()
        {
        }

        public override void UpdateScreen()
        {
        }

        public override void SetListeners()
        {
        }

        public override bool Validate()
        {
            return true;
        }

        public void ScrollYChanged(ScrollInformation scrollInformation)
        {
        }
    }
}