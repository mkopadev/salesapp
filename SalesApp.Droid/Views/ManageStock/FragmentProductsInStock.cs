using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.BL.Models.ManageStock;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.Views.ManageStock
{
    public class FragmentProductsInStock : ManageStockFragmentBase, IPreviousNavigator
    {
        private ListView _stockListView;

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
            View view = this.BindingInflate(Resource.Layout.fragment_products_in_stock, container, false);

            _stockListView = view.FindViewById<ListView>(Resource.Id.scm_stock_list);
            _stockListView.ItemClick += StockListViewOnItemClick;

            this.ManageStockViewModel.NextButtonVisible = false;
            this.ManageStockViewModel.PreviousButtonVisible = true;

            this.ManageStockViewModel.FetchCurrentScmStock();

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Products In Stock");

            return view;
        }

        private void StockListViewOnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            ScmStock selectedProduct = this.ManageStockViewModel.ScmUnitsInStock[(int) itemClickEventArgs.Id];
            ScmStock previousSelection = this.ManageStockViewModel.SelectedProduct;

            // If we select a different product, clear any previous selection as we don't support multiple products yet!
            if (previousSelection != null && previousSelection.Name != selectedProduct.Name)
            {
                this.ManageStockViewModel.SelectedUnits.Clear();
            }

            this.ManageStockViewModel.SelectedProduct = selectedProduct;
            FragmentTransaction ft = FragmentManager.BeginTransaction();
            var fragment = new FragmentSelectUnits();
            ft.Replace(Resource.Id.main_content, fragment, ManageStockView.MainContentFragmentTag);
            ft.Commit();

            this.ManageStockViewModel.ShowNoInternetAlert = false;
        }

        public bool Previous()
        {
            FragmentTransaction ft = FragmentManager.BeginTransaction();
            var fragment = new FragmentCurrentDsrStock();
            ft.Replace(Resource.Id.main_content, fragment, ManageStockView.MainContentFragmentTag);
            ft.Commit();

            return true;
        }
    }
}