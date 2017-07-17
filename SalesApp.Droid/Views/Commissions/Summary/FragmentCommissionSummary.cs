using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.BL.Models.Commissions;
using SalesApp.Core.BL.Models.Commissions.Summary;
using SalesApp.Core.ViewModels.Commissions.Summary;
using SalesApp.Droid.Components.UIComponents.Swipe;
using SalesApp.Droid.Views.Commissions.Adjustments;
using SalesApp.Droid.Views.Commissions.Daily;
using SalesApp.Droid.Views.Commissions.Payments;
using SalesApp.Droid.Views.Commissions.Retainer;
using SalesApp.Droid.Views.Commissions.Tax;
using Fragment = Android.Support.V4.App.Fragment;

namespace SalesApp.Droid.Views.Commissions.Summary
{
    public class FragmentCommissionSummary : CommissionsFragmentBase
    {
        public const string CurrentMonthBundleKey = "CurrentMonthBundleKey";
        public const string MonthDeltaBundleKey = "MonthDeltaBundleKey";
        public const string CommissionTypeBundleKey = "CommissionTypeBundleKey";
        private SummaryViewModel vm;

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString(CurrentMonthBundleKey, this.vm.ThisMonth.ToString());
            outState.PutInt(MonthDeltaBundleKey, this.vm.MonthDelta);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedState)
        {
            base.OnCreateView(inflater, container, savedState);
            View view = this.BindingInflate(Resource.Layout.fragment_commission_summary, null);

            SwipeLinearLayout swipeLayout = view as SwipeLinearLayout;

            if (swipeLayout != null)
            {
                swipeLayout.Swipe += Swipe;
            }

            DateTime monthToLoad = DateTime.Now;
            int monthdelta = 0;

            bool nextVisible = false;
            bool previousVisible = true;

            if (savedState != null)
            {
                string dateString = savedState.GetString(CurrentMonthBundleKey);
                monthToLoad = DateTime.Parse(dateString);
                monthdelta = savedState.GetInt(MonthDeltaBundleKey);
            }
            else
            {
                // Get intial data from the arguments
                if (this.Arguments != null)
                {
                    string dateString = this.Arguments.GetString(CurrentMonthBundleKey);
                    monthToLoad = DateTime.Parse(dateString);
                    monthdelta = this.Arguments.GetInt(MonthDeltaBundleKey);
                }
            }

            if (monthdelta > 0)
            {
                nextVisible = true;
            }
            if (monthdelta >= SummaryViewModel.MaxBackScrollMonths)
            {
                previousVisible = false;
            }

            BindableOverlayFragment suspendedOverlay = new BindableOverlayFragment(SuspendedOverlay, this.parentActivity, Resource.Id.main_content);
            BindableOverlayFragment noDataOverlay = new BindableOverlayFragment(NoDataOverlay, this.parentActivity, Resource.Id.main_content);

            vm = new SummaryViewModel(monthToLoad)
            {
                GoToDetailsScreen = this.GoToDetailsScreen,
                HasDetailsFunction = this.HasDetails,
                MonthLoaded = this.MonthLoaded,
                MonthDelta = monthdelta,
                NextVisible = nextVisible,
                PreviousVisible = previousVisible
            };

            this.ViewModel = vm;

            var set = this.CreateBindingSet<FragmentCommissionSummary, SummaryViewModel>();
            set.Bind(this.parentActivity.SwipeRefreshLayout).For(target => target.Refreshing).To(model => model.Busy);
            set.Bind(suspendedOverlay).For(target => target.Visible).To(model => model.Suspended);
            set.Bind(noDataOverlay).For(target => target.Visible).To(model => model.NoData);
            set.Apply();

            vm.LoadMonthData(monthToLoad);

            return view;
        }

        public async Task LoadData()
        {
            await vm.LoadMonthData(vm.ThisMonth);
        }

        private void Swipe(object source, SwipeEventArgs e)
        {
            if (Math.Abs(e.DeltaX) <= Math.Abs(e.DeltaY))
            {
                // vertical scroll; not interested
                return;
            }

            // horizontal scroll
            if (e.DeltaX > 0)
            {
                this.vm.LoadPreviousMonth();
            }
            else
            {
                this.vm.LoadNextMonth();
            }
        }

        public override void OnResume()
        {
            base.OnResume();
            this.parentActivity.MenuDrawerToggle.DrawerIndicatorEnabled = true;
            this.parentActivity.SwipeRefreshLayout.Enabled = true;

            Snackbar snackbar = Snackbar.Make(View, GetString(Resource.String.turn_on_net_to_view_commissions), Snackbar.LengthIndefinite);
            BindableSnackBar bindableSnackBar = new BindableSnackBar(snackbar);

            var set = this.CreateBindingSet<FragmentCommissionSummary, SummaryViewModel>();
            set.Bind(bindableSnackBar).For(obj => obj.Visible).To(x => x.ShowSnackBar);
            set.Apply();
        }

        /// <summary>
        /// Function is called from the viewm model when a new month has been loaded
        /// </summary>
        /// <param name="loadedMonth">THe month that has been loaded</param>
        private void MonthLoaded(DateTime loadedMonth, int monthDelta)
        {
            this.parentActivity.CurrentMonth = loadedMonth;
            this.parentActivity.MonthDelta = monthDelta;
        }

        private CommissionsInfoFragment SuspendedOverlay
        {
            get
            {
                CommissionsInfoFragment fragment = new CommissionsInfoFragment();
                Bundle bundle = new Bundle();
                bundle.PutString(CommissionsInfoFragment.TitleBundleKey, this.GetString(Resource.String.commissions_suspended_title));
                bundle.PutString(CommissionsInfoFragment.MessageBundleKey, this.GetString(Resource.String.commissions_suspended_message));
                bundle.PutBoolean(CommissionsInfoFragment.IconBundleKey, true);
                fragment.Arguments = bundle;

                return fragment;
            }
        }

        private CommissionsInfoFragment NoDataOverlay
        {
            get
            {
                CommissionsInfoFragment fragment = new CommissionsInfoFragment();
                Bundle bundle = new Bundle();
                bundle.PutString(CommissionsInfoFragment.MessageBundleKey, this.GetString(Resource.String.commissions_no_data));
                fragment.Arguments = bundle;
                return fragment;
            }
        }

        private void GoToDetailsScreen(CommissionItem item)
        {
            string name = item.Name;

            Fragment fragment = this.DecideDetailsFragment(name);

            if (fragment == null)
            {
                return;
            }

            this.parentActivity.LoadFragment(fragment, CommissionsView.CommissionDetailsTag);
            this.parentActivity.SetScreenTitle(name);
        }

        private bool HasDetails(CommissionItem item)
        {
            Fragment fragment = DecideDetailsFragment(item.Name);
            if (fragment == null)
            {
                return false;
            }

            return true;
        }

        private Fragment DecideDetailsFragment(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return null;
            }
            name = name.ToLower();
            Bundle bundle = new Bundle();
            bundle.PutString(CurrentMonthBundleKey, vm.ThisMonth.ToString());

            if (name.Contains("daily") || name.Contains("quality"))
            {
                Fragment fragment = new FragmentDailyCommission();

                if (name.Contains("daily"))
                {
                    bundle.PutInt(CommissionTypeBundleKey, (int)CommissionType.DailyCommission);
                }
                else
                {
                    bundle.PutInt(CommissionTypeBundleKey, (int)CommissionType.QualityCommission);
                }

                fragment.Arguments = bundle;
                return fragment;
            }

            if (name.Contains("retainer"))
            {
                Fragment fragment = new FragmentRetainerCommission();
                fragment.Arguments = bundle;
                return fragment;
            }

            if (name.Contains("adjustment"))
            {
                Fragment fragment = new AdjustmentsFragment();
                fragment.Arguments = bundle;
                return fragment;
            }

            if (name.Contains("tax"))
            {
                Fragment fragment = new CommissionTaxFragment();
                fragment.Arguments = bundle;
                return fragment;
            }

            if (name.Contains("payment"))
            {
                Fragment fragment = new CommissionPaymentFragment();
                fragment.Arguments = bundle;
                return fragment;
            }
            return null;
        }
    }
}