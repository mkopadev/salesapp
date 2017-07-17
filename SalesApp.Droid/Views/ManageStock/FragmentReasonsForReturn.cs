using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Droid.Services.GAnalytics;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace SalesApp.Droid.Views.ManageStock
{
    public class FragmentReasonsForReturn : ManageStockFragmentBase, IPreviousNavigator
    {
        private ListView _lvReasons;

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
            this.FragmentView = this.BindingInflate(Resource.Layout.fragment_reasons_for_return, container, false);

            _lvReasons = this.FragmentView.FindViewById<ListView>(Resource.Id.reason_list);
            _lvReasons.ItemClick += this.LvReasonsOnItemClick;

            this.ManageStockViewModel.GetReasons();

            this.ManageStockViewModel.NextButtonVisible = false;
            this.ManageStockViewModel.PreviousButtonVisible = true;


            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Reasons For Return");

            return this.FragmentView;
        }

        private void LvReasonsOnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            this.ManageStockViewModel.SelectedReason = this.ManageStockViewModel.Reasons[itemClickEventArgs.Position];
            FragmentTransaction ft = FragmentManager.BeginTransaction();
            var fragment = new FragmentSelectedUnits();
            ft.Replace(Resource.Id.main_content, fragment, ManageStockView.MainContentFragmentTag);
            ft.Commit();
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