using Android.OS;
using Android.Views;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.Api.ManageStock;
using SalesApp.Core.ViewModels.ManageStock;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.Views.ManageStock
{
    public class FragmentManageStockComplete : ManageStockFragmentBase
    {
        public const string AllocatedProductResponseKey = "AllocatedProductResponseKey";

        protected override bool MonitorInternetConnection
        {
            get
            {
                return false;
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            this.FragmentView = this.BindingInflate(Resource.Layout.fragment_units_allocation_complete, container, false);

            ManageStockPostApiResponse response = this.GetArgument<ManageStockPostApiResponse>(AllocatedProductResponseKey);
            this.ManageStockViewModel.ProcessApiResponse(response);

            BindableProgressDialog progressDialog = new BindableProgressDialog(this.Activity);

            var set = this.CreateBindingSet<FragmentManageStockComplete, ManageStockViewModel>();
            set.Bind(progressDialog).For(target => target.Visible).To(source => source.IsBusy);
            set.Bind(progressDialog).For(target => target.Message).To(source => source.ProgressDialogMessage);
            set.Apply();

            this.ManageStockViewModel.NextButtonVisible = false;
            this.ManageStockViewModel.PreviousButtonVisible = false;
            this.ManageStockViewModel.NegativeButtonText = this.GetString(Resource.String.home);

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Manage Stock Complete");

            return this.FragmentView;
        }
    }
}