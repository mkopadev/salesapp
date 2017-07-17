using System;
using Android.OS;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.Auth;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.ViewModels.Commissions.Adjustments;
using SalesApp.Droid.Views.Commissions.Summary;

namespace SalesApp.Droid.Views.Commissions.Adjustments
{
    public class AdjustmentsFragment : CommissionDetailsFragmentBase
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedState)
        {
            base.OnCreateView(inflater, container, savedState);
            View view = this.BindingInflate(Resource.Layout.fragment_commission_adjustments, null);

            string userId = Resolver.Instance.Get<ISalesAppSession>().UserId.ToString();

            DateTime currentMonth = DateTime.Now;

            if (this.Arguments != null)
            {
                string month = this.Arguments.GetString(FragmentCommissionSummary.CurrentMonthBundleKey);
                currentMonth = DateTime.Parse(month);
            }

            AdjustmentsViewModel vm = new AdjustmentsViewModel(currentMonth);
            vm.NoDataMessage = GetString(Resource.String.commissions_no_adjustments);
            this.ViewModel = vm;

            return view;
        }
    }
}