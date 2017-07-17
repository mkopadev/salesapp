using System;
using Android.OS;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.ViewModels.Commissions.Payments;
using SalesApp.Droid.Views.Commissions.Summary;

namespace SalesApp.Droid.Views.Commissions.Payments
{
    public class CommissionPaymentFragment : CommissionDetailsFragmentBase
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedState)
        {
            base.OnCreateView(inflater, container, savedState);
            View view = this.BindingInflate(Resource.Layout.fragment_commission_payments, null);

            string month = this.Arguments.GetString(FragmentCommissionSummary.CurrentMonthBundleKey);

            PaymentsViewModel vm = new PaymentsViewModel(DateTime.Parse(month));
            vm.NoDataMessage = GetString(Resource.String.commissions_no_payments);
            this.ViewModel = vm;

            return view;
        }
    }
}