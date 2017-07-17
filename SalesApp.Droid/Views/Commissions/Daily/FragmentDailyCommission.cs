using System;
using Android.OS;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.BL.Models.Commissions;
using SalesApp.Core.ViewModels.Commissions.Daily;
using SalesApp.Droid.Views.Commissions.Summary;

namespace SalesApp.Droid.Views.Commissions.Daily
{
    public class FragmentDailyCommission : CommissionDetailsFragmentBase
    {
        private DateTime _currentMonth;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            View view = this.BindingInflate(Resource.Layout.fragment_daily_commission, null);

            CommissionType commissionType = CommissionType.DailyCommission;
            if (this.Arguments != null)
            {
                commissionType = (CommissionType)this.Arguments.GetInt(FragmentCommissionSummary.CommissionTypeBundleKey);
                string month = this.Arguments.GetString(FragmentCommissionSummary.CurrentMonthBundleKey);
                _currentMonth = DateTime.Parse(month);
            }

            DailyCommissionViewModel vm = new DailyCommissionViewModel(this._currentMonth);

            if (commissionType == CommissionType.DailyCommission)
            {
                vm.FirstColLabel = GetString(Resource.String.commissions_date);
                vm.SecondColLabel = GetString(Resource.String.commissions_sale);
                vm.SummaryLabel = GetString(Resource.String.total_commission_daily); ;
                vm.NoDataMessage = GetString(Resource.String.commissions_no_sales_made);
                vm.CommissionType = CommissionType.DailyCommission;
                vm.SecondColVisible = true;
            }
            else if (commissionType == CommissionType.QualityCommission)
            {
                vm.FirstColLabel = GetString(Resource.String.commissions_month);
                // vm.SecondColLabel = GetString(Resource.String.commissions_quality_sale);
                vm.SummaryLabel = GetString(Resource.String.total_commission_quality);
                vm.NoDataMessage = GetString(Resource.String.commissions_no_quality_commissions);
                vm.CommissionType = CommissionType.QualityCommission;
                vm.SecondColVisible = false;
            }
                
            this.ViewModel = vm;

            return view;
        }
    }
}