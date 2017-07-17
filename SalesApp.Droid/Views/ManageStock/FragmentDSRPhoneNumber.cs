using Android.OS;
using Android.Views;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.Api.ManageStock;
using SalesApp.Core.ViewModels.ManageStock;
using SalesApp.Droid.Services.GAnalytics;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace SalesApp.Droid.Views.ManageStock
{
    public class FragmentDsrPhoneNumber : ManageStockFragmentBase, IPreviousNavigator, INextNavigator
    {
        private DsrStockServerResponseObject _dsrStock;
        private bool _nextClicked;

        protected override bool MonitorInternetConnection
        {
            get
            {
                return true;
            }
        }

        public DsrStockServerResponseObject DsrStock
        {
            get
            {
                return this._dsrStock;
            }

            set
            {
                if (value == null || !this._nextClicked)
                {
                    return;
                }

                this._dsrStock = value;

                FragmentTransaction ft = FragmentManager.BeginTransaction();
                var fragment = new FragmentCurrentDsrStock();
                ft.Replace(Resource.Id.main_content, fragment, ManageStockView.MainContentFragmentTag);
                ft.Commit();
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            this.FragmentView = this.BindingInflate(Resource.Layout.fragment_dsr_phone_number, container, false);

            var set = this.CreateBindingSet<FragmentDsrPhoneNumber, ManageStockViewModel>();
            set.Bind(this).For(v => v.DsrStock).To(vm => vm.DsrStock);
            set.Apply();

            this.ManageStockViewModel.NextButtonVisible = true;
            this.ManageStockViewModel.NextButtonEnabled = true;
            this.ManageStockViewModel.NextButtonText = Activity.GetString(Resource.String.find_dsr);

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen(" DSR Phone Number");
            return this.FragmentView;
        }

        public bool Next()
        {
            this._nextClicked = true;
            this.ManageStockViewModel.FindDsrAsync();

            return true;
        }

        public bool Previous()
        {
            FragmentTransaction ft = FragmentManager.BeginTransaction();
            var fragment = new FragmentStockSelectTask();
            ft.Replace(Resource.Id.main_content, fragment, ManageStockView.MainContentFragmentTag);
            ft.Commit();

            return true;
        }
    }
}