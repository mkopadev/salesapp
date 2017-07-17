using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.Enums.ManageStock;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.Views.ManageStock
{
    public class FragmentSelectUnits : ManageStockFragmentBase, IPreviousNavigator, INextNavigator
    {
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
            Activity.SetTitle(Resource.String.manage_stock);

            View view = this.BindingInflate(Resource.Layout.fragment_select_units, container, false);

            this.ManageStockViewModel.NextButtonVisible = true;
            this.ManageStockViewModel.PreviousButtonVisible = true;
            this.ManageStockViewModel.NextButtonText = Activity.GetString(Resource.String.next);
            this.ManageStockViewModel.ShowGreyDivider = false;

            this.ManageStockViewModel.CanProcessSelectedItems = this.ManageStockViewModel.StockAction ==
                                                                    ManageStockAction.Issue;

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Select Units");
            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
            this.ManageStockViewModel.UpdateSelectedUnitsCountStatus();
        }

        public bool Next()
        {
            FragmentTransaction ft = FragmentManager.BeginTransaction();
            var fragment = new FragmentSelectedUnits();
            ft.Replace(Resource.Id.main_content, fragment, ManageStockView.MainContentFragmentTag);
            ft.Commit();

            return true;
        }

        public bool Previous()
        {
            FragmentTransaction ft = FragmentManager.BeginTransaction();
            Fragment frag = new FragmentProductsInStock();
            ft.Replace(Resource.Id.main_content, frag, ManageStockView.MainContentFragmentTag);
            ft.Commit();

            return true;
        }

       
    }
}