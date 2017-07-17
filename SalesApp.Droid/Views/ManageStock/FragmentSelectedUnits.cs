using System.Threading.Tasks;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.Api.ManageStock;
using SalesApp.Core.BL.Models.ManageStock;
using SalesApp.Core.Enums.ManageStock;
using SalesApp.Core.ViewModels.ManageStock;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.Views.ManageStock
{
    public class FragmentSelectedUnits : ManageStockFragmentBase, IPreviousNavigator, INextNavigator
    {
        private KeyValue _selectedUnits = new KeyValue();

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
            this.FragmentView = this.BindingInflate(Resource.Layout.fragment_selected_units, container, false);
       
            this.ManageStockViewModel.NextButtonVisible = true;
            this.ManageStockViewModel.PreviousButtonVisible = true;
            this.ManageStockViewModel.NextButtonText = Activity.GetString(Resource.String.allocate);

            this.ManageStockViewModel.SectionizeSelectedUnitsList();

            if (this.ManageStockViewModel.StockAction == ManageStockAction.Issue)
            {
                _selectedUnits.Key = this.GetString(Resource.String.stats_units_allocated_now) + ":";
                _selectedUnits.Value = this.ManageStockViewModel.SelectedUnits.Count.ToString();

                this.ManageStockViewModel.DsrDetails.Remove(this._selectedUnits);
                this.ManageStockViewModel.DsrDetails.Add(this._selectedUnits);
            }
            else
            {
                this.ManageStockViewModel.RemoveKeyFromDsrDetails(Activity.GetString(Resource.String.units_allocated_key));
            }

            this.ManageStockViewModel.ShowGreyDivider = true;

            BindableProgressDialog progressDialog = new BindableProgressDialog(this.Activity);

            var set = this.CreateBindingSet<FragmentSelectedUnits, ManageStockViewModel>();
            set.Bind(progressDialog).For(target => target.Visible).To(source => source.IsBusy);
            set.Bind(progressDialog).For(target => target.Message).To(source => source.ProgressDialogMessage);
            set.Apply();

            if (this.ManageStockViewModel.StockAction == ManageStockAction.Issue)
            {
                this.ManageStockViewModel.SelectMoreOrReasonTitle =
                    Activity.GetString(Resource.String.select_more_units);
            }
            else
            {
                this.ManageStockViewModel.SelectMoreOrReasonTitle =
                    Activity.GetString(Resource.String.reason_for_return_title);
                this.ManageStockViewModel.NextButtonText = Activity.GetString(Resource.String.confirm_return);
                this.ManageStockViewModel.ShowCheckBoxOnConfirmationScreen(false);
            }

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Selected Units");
            return this.FragmentView;
        }

        private async Task AlocateSelectedUnits()
        {
            ManageStockPostApiResponse response = await this.ManageStockViewModel.AllocateSelectedUnits();
            this.ShowlastFragment(response);
        }

        private void ShowlastFragment(ManageStockPostApiResponse res)
        {
            FragmentTransaction ft = FragmentManager.BeginTransaction();
            MvxFragmentBase fragment = new FragmentManageStockComplete();
            fragment.SetArgument(FragmentManageStockComplete.AllocatedProductResponseKey, res);
            ft.Replace(Resource.Id.main_content, fragment, ManageStockView.MainContentFragmentTag);
            ft.Commit();
        }

        public bool Next()
        {
            switch (ManageStockViewModel.StockAction)
            {
                    case ManageStockAction.Issue:
                        this.AlocateSelectedUnits();
                        return true;
                    case ManageStockAction.Receive:
                        this.ReceiveStock();
                        return true;
            }

            return false;
        }

        private async Task ReceiveStock()
        {
            ManageStockPostApiResponse response = await this.ManageStockViewModel.ReceiveStock();
            this.ShowlastFragment(response);
        }

        public bool Previous()
        {
            if (this.ManageStockViewModel.StockAction == ManageStockAction.Issue)
            {
                FragmentTransaction ft = FragmentManager.BeginTransaction();
                Fragment frag = new FragmentSelectUnits();
                ft.Replace(Resource.Id.main_content, frag, ManageStockView.MainContentFragmentTag);
                ft.Commit();

                this.ManageStockViewModel.DsrDetails.Remove(this._selectedUnits);
            }
            else
            {
                this.ManageStockViewModel.ShowCheckBoxOnConfirmationScreen(true);
                FragmentTransaction ft = FragmentManager.BeginTransaction();
                Fragment frag = new FragmentReasonsForReturn();
                ft.Replace(Resource.Id.main_content, frag, ManageStockView.MainContentFragmentTag);
                ft.Commit();
            }

            return true;
        }
    }
}