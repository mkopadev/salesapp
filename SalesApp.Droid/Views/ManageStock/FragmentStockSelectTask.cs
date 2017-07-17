using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.Enums.ManageStock;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.Views.ManageStock
{
    public class FragmentStockSelectTask : ManageStockFragmentBase
    {
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

            this.FragmentView = this.BindingInflate(Resource.Layout.stock_select_task, container, false);
            this.ActivityBase.SetScreenTitle(Resource.String.manage_stock);

            Button issueStockButton = this.FragmentView.FindViewById<Button>(Resource.Id.button_issue_stock);
            Button receiveStockButton = this.FragmentView.FindViewById<Button>(Resource.Id.button_receive_stock);

            issueStockButton.Click += IssueStockButtonOnClick;
            receiveStockButton.Click += ReciveStockButtonOnClick;

            this.ManageStockViewModel.NextButtonVisible = false;
            this.ManageStockViewModel.PreviousButtonVisible = false;

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Stock Select Task");

            return this.FragmentView;
        }

        private void ReciveStockButtonOnClick(object sender, EventArgs eventArgs)
        {
            this.ActivityBase.SetScreenTitle(Resource.String.receive_stock);
            if (this.ManageStockViewModel.StockAction == ManageStockAction.Issue)
            {
                // Do some clean up
                this.ManageStockViewModel.SelectedUnits.Clear();
                this.ManageStockViewModel.DsrPhoneNumber = string.Empty;
            }

            this.ManageStockViewModel.StockAction = ManageStockAction.Receive;
            ShowDsrPhoneNumber();
        }

        private void IssueStockButtonOnClick(object sender, EventArgs eventArgs)
        {
            this.ActivityBase.SetScreenTitle(Resource.String.issue_stock);
            if (this.ManageStockViewModel.StockAction == ManageStockAction.Receive)
            {
                // Do some clean up
                this.ManageStockViewModel.SelectedUnits.Clear();
                this.ManageStockViewModel.DsrPhoneNumber = string.Empty;
            }

            this.ManageStockViewModel.StockAction = ManageStockAction.Issue;
            ShowDsrPhoneNumber();
        }

        private void ShowDsrPhoneNumber()
        {
            FragmentTransaction ft = FragmentManager.BeginTransaction();
            Fragment fragment = new FragmentDsrPhoneNumber();
            ft.Replace(Resource.Id.main_content, fragment, ManageStockView.MainContentFragmentTag);
            ft.Commit();
        }
    }
}