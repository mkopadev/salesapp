using System;
using Android.OS;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.ViewModels.Commissions.Retainer;
using SalesApp.Droid.Views.Commissions.Summary;

namespace SalesApp.Droid.Views.Commissions.Retainer
{
    public class FragmentRetainerCommission : CommissionDetailsFragmentBase
    {
        private DateTime currentMonth;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            View view = this.BindingInflate(Resource.Layout.fragment_retainer_commission, null);

            if (this.Arguments != null)
            {
                string month = this.Arguments.GetString(FragmentCommissionSummary.CurrentMonthBundleKey);
                this.currentMonth = DateTime.Parse(month);
            }

            RetainerCommissionViewModel vm = new RetainerCommissionViewModel(this.currentMonth);
            vm.NoDataMessage = GetString(Resource.String.commissions_no_retainer_commissions);
            this.ViewModel = vm;

            return view;
        }
    }
}