using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V4;
using SalesApp.Core.ViewModels.Chama;

namespace SalesApp.Droid.Views.Chama
{
    public class SpinnerDialogFragment : MvxDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            return this.BindingInflate(Resource.Layout.spinner_dialog_fragment, container, false);
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            Dialog dialog = base.OnCreateDialog(savedInstanceState);
            dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            return dialog;
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            base.OnDismiss(dialog);

            GroupSelectionViewModel vm = this.ViewModel as GroupSelectionViewModel;

            if (vm == null || string.IsNullOrEmpty(vm.FilterText))
            {
                return;
            }

            vm.FilterText = string.Empty;
        }
    }
}