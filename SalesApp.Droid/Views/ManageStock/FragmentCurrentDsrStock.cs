using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.Enums.ManageStock;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.Views.ManageStock
{
    public class FragmentCurrentDsrStock : ManageStockFragmentBase, IPreviousNavigator, INextNavigator
    {
        private Button _btnNext;
        private Button _btnPrevious;

        protected override bool MonitorInternetConnection
        {
            get
            {
                return true;
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            this.FragmentView = this.BindingInflate(Resource.Layout.fragment_current_dsr_stock, container, false);

            this.ManageStockViewModel.NextButtonVisible = true;

            this.ManageStockViewModel.PreviousButtonVisible = true;
            this.ManageStockViewModel.NextButtonText = Activity.GetString(Resource.String.next);

            this.ManageStockViewModel.CanProcessSelectedItems = this.ManageStockViewModel.StockAction ==
                                                                    ManageStockAction.Receive;

            if (this.ManageStockViewModel.StockAction == ManageStockAction.Receive)
            {
                this.ManageStockViewModel.NextButtonEnabled = this.ManageStockViewModel.SelectedUnits.Count > 0;
            }
            else if (this.ManageStockViewModel.StockAction == ManageStockAction.Issue)
            {
                this.ManageStockViewModel.NextButtonEnabled = this.ManageStockViewModel.DsrStock.UnitsAllocated < this.ManageStockViewModel.DsrStock.MaxAllowedUnits;
            }


            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Current DSR Stock");

            return this.FragmentView;
        }

        public bool Next()
        {
            if (this.ManageStockViewModel.StockAction == ManageStockAction.Issue)
            {
                FragmentTransaction ft = FragmentManager.BeginTransaction();
                var fragment = new FragmentProductsInStock();
                ft.Replace(Resource.Id.main_content, fragment, ManageStockView.MainContentFragmentTag);
                ft.Commit();
            }
            else
            {
                FragmentTransaction ft = FragmentManager.BeginTransaction();
                var fragment = new FragmentReasonsForReturn();
                ft.Replace(Resource.Id.main_content, fragment, ManageStockView.MainContentFragmentTag);
                ft.Commit();
            }

            return true;
        }

        public bool Previous()
        {
            FragmentTransaction ft = FragmentManager.BeginTransaction();
            var fragment = new FragmentDsrPhoneNumber();
            ft.Replace(Resource.Id.main_content, fragment, ManageStockView.MainContentFragmentTag);
            ft.Commit();

            return true;
        }
    }
}