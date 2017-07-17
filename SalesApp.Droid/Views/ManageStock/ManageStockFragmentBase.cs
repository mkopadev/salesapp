using System;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Views;
using SalesApp.Core.ViewModels.ManageStock;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.Connectivity;

namespace SalesApp.Droid.Views.ManageStock
{
    public abstract class ManageStockFragmentBase : MvxFragmentBase
    {
        private ConnectivityChangeReceiver _connectivityChangeReceiver;

        protected ManageStockViewModel ManageStockViewModel
        {
            get
            {
                return this.GetTypeSafeViewModel<ManageStockViewModel>();
            }
        }

        protected abstract bool MonitorInternetConnection { get; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (!this.MonitorInternetConnection)
            {
                return base.OnCreateView(inflater, container, savedInstanceState);
            }

            this._connectivityChangeReceiver = new ConnectivityChangeReceiver();
            this._connectivityChangeReceiver.ConnectionChanged += ConnectionChanged;
            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            ManageStockView stockAllocationView = activity as ManageStockView;

            if (stockAllocationView == null)
            {
                return;
            }

            this.ViewModel = stockAllocationView.ViewModel;
        }

        public override void OnResume()
        {
            base.OnResume();

            if (!this.MonitorInternetConnection)
            {
                return;
            }

            this.Activity.RegisterReceiver(this._connectivityChangeReceiver, new IntentFilter(ConnectivityManager.ConnectivityAction));
            this.ManageStockViewModel.ShowNoInternetAlert = !this.ActivityBase.ConnectedToNetwork;
        }

        public override void OnPause()
        {
            base.OnPause();

            if (!this.MonitorInternetConnection)
            {
                return;
            }

            this.Activity.UnregisterReceiver(this._connectivityChangeReceiver);
        }

        private void ConnectionChanged(object sender, EventArgs eventArgs)
        {
            this.ManageStockViewModel.ShowNoInternetAlert = !this.ActivityBase.ConnectedToNetwork;
        }
    }
}