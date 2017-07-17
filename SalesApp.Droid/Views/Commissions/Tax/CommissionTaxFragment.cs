using System;
using Android.OS;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.ViewModels.Commissions.Tax;
using SalesApp.Droid.Views.Commissions.Summary;

namespace SalesApp.Droid.Views.Commissions.Tax
{
    public class CommissionTaxFragment : CommissionDetailsFragmentBase
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedState)
        {
            base.OnCreateView(inflater, container, savedState);
            View view = this.BindingInflate(Resource.Layout.fragment_commission_tax, null);

            string month = this.Arguments.GetString(FragmentCommissionSummary.CurrentMonthBundleKey);
            DateTime currentMonth = DateTime.Parse(month);
            CommissionTaxViewModel vm = new CommissionTaxViewModel(currentMonth);
            this.ViewModel = vm;

            return view;
        }
    }
}