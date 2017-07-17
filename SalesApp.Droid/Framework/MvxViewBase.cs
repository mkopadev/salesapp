using MvvmCross.Core.ViewModels;
using SalesApp.Droid.Components.UIComponents;

namespace SalesApp.Droid.Framework
{
    public abstract class MvxViewBase<T> : ActivityBase where T : IMvxViewModel
    {
        /// <summary>
        /// Set the permissions needed to make this activity visible
        /// </summary>
        public override void SetViewPermissions()
        {
        }

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
    }
}