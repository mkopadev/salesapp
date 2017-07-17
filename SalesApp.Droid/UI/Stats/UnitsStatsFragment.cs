using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.Events.Stats;
using SalesApp.Core.Events.Stats.Units;
using SalesApp.Core.Framework;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Product;
using SalesApp.Core.Services.Stats.Units;
using SalesApp.Core.ViewModels.Stats.Units;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.UI.Stats
{
    public class UnitsStatsFragment : MvxFragmentBase, ISwipeRefreshFragment
    {
        private LinearLayout _unitsTable;
        private LinearLayout _historyTable;
        private Snackbar _snackBar;
        private ISwipeRefreshable _swipeRefreshable;
        private int _displayCount = 7;
        private readonly ICulture _culture = Resolver.Instance.Get<ICulture>();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedState)
        {
            base.OnCreateView(inflater, container, savedState);
            this.FragmentView = this.BindingInflate(Resource.Layout.layout_units_stats_fragment, container, false);
            _unitsTable = this.FragmentView.FindViewById<LinearLayout>(Resource.Id.units_table);
            this._historyTable = this.FragmentView.FindViewById<LinearLayout>(Resource.Id.units_history_table);

            UnitsStatsFragmentViewModel viewModel = new UnitsStatsFragmentViewModel(new LocalProductService(), new RemoteProductService("products"), new LocalUnitsService(), new RemoteUnitsService());
            this.ViewModel = viewModel;
            viewModel.ProductsFetched += this.ProductsFetched;
            viewModel.UnitsFetched += this.UnitsFetched;

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Units Stats");

            return this.FragmentView;
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            this._swipeRefreshable = activity as ISwipeRefreshable;
        }

        private void ProductsFetched(object sender, ProductsFetchedEvent e)
        {
            var products = e.Products;

            if (products == null || products.Count <= 0)
            {
                return;
            }

            this._unitsTable.RemoveAllViews();

            foreach (var unit in products)
            {
                View row = this.BindingInflate(Resource.Layout.layout_units_info_row, null);

                DateTime dateTime = new DateTime();

                var parsed = false;

                if (unit.DateAcquired != null)
                {
                    parsed = DateTime.TryParse(unit.DateAcquired, out dateTime);
                }

                if (!parsed)
                {
                    DateTime.TryParse(DateTime.Today.ToString(CultureInfo.InvariantCulture), out dateTime);
                }

                row.FindViewById<TextView>(Resource.Id.unit_row_datereceived).Text =
                    dateTime.ToString(this._culture.GetShortDateFormat());
                row.FindViewById<TextView>(Resource.Id.unit_row_serialnumber).Text = unit.SerialNumber;
                this._unitsTable.AddView(row);
            }
        }

        private void UnitsFetched(object sender, StatsListFetchedEvent e)
        {
            var rows = e.Rows;
            ICulture culture = Resolver.Instance.Get<ICulture>();

            if (rows == null || rows.Count <= 0)
            {
                this._swipeRefreshable.SetIsBusy(false);
                if (!ActivityBase.ConnectedToNetwork && this._historyTable.ChildCount == 0)
                {
                    this._snackBar = this.ActivityBase.ShowSnackbar(Resource.Id.pager, Resource.String.stats_refresh_prompt);
                }
                else if (!ActivityBase.ConnectedToNetwork && this._historyTable.ChildCount > 0)
                {
                    Toast.MakeText(this.Activity, Resource.String.unable_to_sync_internet, ToastLength.Long).Show();
                }

                return;
            }

            _historyTable.RemoveAllViews();

            foreach (var item in rows.Take(_displayCount))
            {
                DateTime rowDate;
                DateTime.TryParseExact(item.Items[1], culture.GetShortDateFormat(), CultureInfo.InvariantCulture, DateTimeStyles.None, out rowDate);
                View rowView = this.BindingInflate(Resource.Layout.layout_units_history_row, null);

                rowView.FindViewById<TextView>(Resource.Id.valueDay).Text = item.Items[0];

                rowView.FindViewById<TextView>(Resource.Id.valueDate).Text = item.Items[1];

                rowView.FindViewById<TextView>(Resource.Id.valueAcquired).Text = item.Items[2];
                rowView.FindViewById<TextView>(Resource.Id.valueStarted).Text = item.Items[3];
                rowView.FindViewById<TextView>(Resource.Id.valueRemoved).Text = item.Items[4];

                if (rowDate == DateTime.Today)
                    rowView.FindViewById<TextView>(Resource.Id.today_text).Visibility =
                        ViewStates.Visible;

                _historyTable.AddView(rowView);
            }

            this._swipeRefreshable.SetIsBusy(false);
            if (this._snackBar != null)
            {
                this._snackBar.Dismiss();
            }
        }

        public async Task SwipeRefresh(bool forceRemote = true)
        {
            this._swipeRefreshable.SetIsBusy(true);
            var vm = this.GetTypeSafeViewModel<UnitsStatsFragmentViewModel>();
            vm.RefreshData(forceRemote);
        }
    }
}