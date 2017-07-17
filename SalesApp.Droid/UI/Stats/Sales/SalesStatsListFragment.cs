using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.Events.Stats;
using SalesApp.Core.ViewModels.Stats.Sales;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.UI.Stats.Sales
{
    /// <summary>
    /// This fragment displays the stats list
    /// </summary>
    public class SalesStatsListFragment : MvxFragmentBase, AdapterView.IOnItemSelectedListener, ISwipeRefreshFragment
    {
        private LinearLayout _mainContainer;
        private ISwipeRefreshable _swipeRefreshable;
        private Snackbar _snackBar;
        private const int HeaderViewCount = 3;

        private ColumnInfo[] Columns
        {
            get
            {
                return new[]
                {
                    new ColumnInfo(3, GravityFlags.Left),
                    new ColumnInfo(3),
                    new ColumnInfo(3)
                };
            }
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            this._swipeRefreshable = activity as ISwipeRefreshable;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedState)
        {
            base.OnCreateView(inflater, container, savedState);
            this.FragmentView = this.BindingInflate(Resource.Layout.stats_list_view, container, false);
            this._mainContainer = this.FragmentView.FindViewById<LinearLayout>(Resource.Id.main_content);

            SalesStatsFragmentViewModel viewModel = new SalesStatsFragmentViewModel();
            viewModel.DataFetched += DataFetched;
            this.ViewModel = viewModel;

            this.AddHeaderToList(Resource.Layout.stats_blocks);
            View spinnerContainer = this.AddHeaderToList(Resource.Layout.stats_spinner);
            Spinner spinner = spinnerContainer.FindViewById<Spinner>(Resource.Id.table_spinner);
            this.AddColumnHeaders();

            string[] items = this.Activity.Resources.GetStringArray(Resource.Array.stats_period_array);
            var spinnerAdapter = new DefaultSpinnerAdapter().GetAdapter(items, this.Activity);
            spinner.Adapter = spinnerAdapter;
            spinner.OnItemSelectedListener = this;
            
            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Sales Stats");

            return this.FragmentView;
        }

        private void DataFetched(object sender, StatsListFetchedEvent e)
        {
            var rows = e.Rows;
            int currentRows =  this._mainContainer.ChildCount - HeaderViewCount;

            if (rows == null || rows.Count <= 0)
            {
                this._swipeRefreshable.SetIsBusy(false);
                if (!ActivityBase.ConnectedToNetwork && currentRows == 0)
                {
                    this._snackBar = this.ActivityBase.ShowSnackbar(Resource.Id.pager, Resource.String.stats_refresh_prompt);
                }
                else if (!ActivityBase.ConnectedToNetwork && currentRows > 0)
                {
                    Toast.MakeText(this.Activity, Resource.String.unable_to_sync_internet, ToastLength.Long).Show();
                }

                return;
            }

            this.RemoveNonHeaderRows();

            foreach (var row in e.Rows)
            {
                LayoutInflater inflater = LayoutInflater.FromContext(this.Activity);
                LinearLayout view = (LinearLayout) inflater.Inflate(Resource.Layout.layout_sales_stats_row, null);

                // view.Tag = row.Tag;
                view.SetBackgroundColor(row.IsSelected ? Resources.GetColor(Resource.Color.grey_light) : Resources.GetColor(Resource.Color.white));

                List<TextView> rowViews = new List<TextView>
                {
                    view.FindViewById<TextView>(Resource.Id.stats_row_column_1),
                    view.FindViewById<TextView>(Resource.Id.stats_row_column_2),
                    view.FindViewById<TextView>(Resource.Id.stats_row_column_3),
                    view.FindViewById<TextView>(Resource.Id.stats_row_column_4),
                    view.FindViewById<TextView>(Resource.Id.stats_row_column_5),
                    view.FindViewById<TextView>(Resource.Id.stats_row_column_6),
                    view.FindViewById<TextView>(Resource.Id.stats_row_column_7)
                };

                for (int i = 0; i < this.Columns.Length; i++)
                {
                    ColumnInfo column = this.Columns[i];
                    rowViews[i].Gravity = column.Gravity;
                    rowViews[i].LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, column.Weight);
                    rowViews[i].Visibility = ViewStates.Visible;
                    rowViews[i].Text = row.Items[i] ?? string.Empty;
                }

                this._mainContainer.AddView(view);
            }

            this._swipeRefreshable.SetIsBusy(false);

            if (this._snackBar != null)
            {
                this._snackBar.Dismiss();
            }
        }

        private void RemoveNonHeaderRows()
        {
            if (this._mainContainer.ChildCount <= HeaderViewCount)
            {
                return;
            }

            int numberOfNonHeaderViews = this._mainContainer.ChildCount - HeaderViewCount;
            this._mainContainer.RemoveViews(HeaderViewCount, numberOfNonHeaderViews);
        }

        private View AddHeaderToList(int layoutId)
        {
            View headerView = this.BindingInflate(layoutId, null);
            this._mainContainer.AddView(headerView);

            return headerView;
        }

        private void AddColumnHeaders()
        {
            View statsListHeader = AddHeaderToList(Resource.Layout.layout_sales_stats_header);
            TableRow tableHeader = statsListHeader.FindViewById<TableRow>(Resource.Id.table_header);

            for (int i = 0; i < this.Columns.Length; i++)
            {
                TextView textView = (TextView)tableHeader.GetChildAt(i);
                textView.LayoutParameters = new TableRow.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, this.Columns[i].Weight);
                textView.Gravity = this.Columns[i].Gravity;
            }
        }

        public void OnItemSelected(AdapterView parent, View view, int position, long id)
        {
            this._swipeRefreshable.SetIsBusy(true);
            var vm = this.GetTypeSafeViewModel<SalesStatsFragmentViewModel>();

            vm.SetCurrentPeriod(position);
            vm.LoadSalesData();
        }

        public void OnNothingSelected(AdapterView parent)
        {
        }

        public async Task SwipeRefresh(bool forceRemote = true)
        {
            this._swipeRefreshable.SetIsBusy(true);
            var vm = this.GetTypeSafeViewModel<SalesStatsFragmentViewModel>();
            vm.LoadSalesData(forceRemote);
        }
    }
}