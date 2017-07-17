using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.Enums.Stats;
using SalesApp.Core.Events.Stats;
using SalesApp.Core.ViewModels.Stats.Rankings;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.UI.Stats.Rankings
{
    /// <summary>
    /// This fragment displays the stats list
    /// </summary>
    public class RankingStatsListFragment : MvxFragmentBase, AdapterView.IOnItemSelectedListener, ISwipeRefreshFragment
    {
        private LinearLayout _mainContainer;
        private Button _monthButton;
        private Button _quarterButton;
        private Button _yearButton;
        private Snackbar _snackBar;
        private Spinner _regionSpinner;
        private ISwipeRefreshable _swipeRefreshable;
        private const int HeaderViewCount = 4;

        private ColumnInfo[] Columns
        {
            get
            {
                return new[]
                {
                    new ColumnInfo(1, GravityFlags.Left),
                    new ColumnInfo(3, GravityFlags.Left),
                    new ColumnInfo(1)
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

            RankingStatsListFragmentViewModel viewModel = new RankingStatsListFragmentViewModel();
            viewModel.DataFetched += DataFetched;
            this.ViewModel = viewModel;

            this.AddHeaderToList(Resource.Layout.stats_blocks_ranking);
            View spinnerContainer = this.AddHeaderToList(Resource.Layout.stats_spinner);
            this._regionSpinner = spinnerContainer.FindViewById<Spinner>(Resource.Id.table_spinner);
            View buttonsContainer = this.AddHeaderToList(Resource.Layout.stats_period_buttons);

            this._monthButton = buttonsContainer.FindViewById<Button>(Resource.Id.month_button);
            this._monthButton.Click += this.PeriodButtonOnClick;

            this._quarterButton = buttonsContainer.FindViewById<Button>(Resource.Id.quarter_button);
            this._quarterButton.Click += this.PeriodButtonOnClick;

            this._yearButton = buttonsContainer.FindViewById<Button>(Resource.Id.year_button);
            this._yearButton.Click += PeriodButtonOnClick;

            this.AddColumnHeaders();

            string[] items = this.Activity.Resources.GetStringArray(Resource.Array.stats_region_array);
            var spinnerAdapter = new DefaultSpinnerAdapter().GetAdapter(items, this.Activity);
            this._regionSpinner.Adapter = spinnerAdapter;

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Ranking Stats List");

            return this.FragmentView;
        }

        private void PeriodButtonOnClick(object sender, EventArgs eventArgs)
        {
            Button clickedButton = (Button) sender;
            var vm = this.GetTypeSafeViewModel<RankingStatsListFragmentViewModel>();

            Period selectedPeriod = Period.Month;

            switch (clickedButton.Id)
            {
                case Resource.Id.month_button:
                    selectedPeriod = Period.Month;
                    break;
                case Resource.Id.quarter_button:
                    selectedPeriod = Period.Quarter;
                    break;
                case Resource.Id.year_button:
                    selectedPeriod = Period.Year;
                    break;
            }

            if (vm.CurrentPeriod == selectedPeriod)
            {
                return;
            }

            this._swipeRefreshable.SetIsBusy(true);

            ResetPeriodButtons();
            clickedButton.SetBackgroundResource(Resource.Drawable.period_button_selected);
            clickedButton.SetTextAppearance(this.Activity, Resource.Style.period_button_text_selected);

            vm.CurrentPeriod = selectedPeriod;
            vm.LoadRankingsData(false);
        }

        private void ResetPeriodButtons()
        {
            this._monthButton.SetBackgroundResource(Resource.Drawable.period_button);
            this._quarterButton.SetBackgroundResource(Resource.Drawable.period_button);
            this._yearButton.SetBackgroundResource(Resource.Drawable.period_button);

            this._monthButton.SetTextAppearance(this.Activity, Resource.Style.period_button_text);
            this._quarterButton.SetTextAppearance(this.Activity, Resource.Style.period_button_text);
            this._yearButton.SetTextAppearance(this.Activity, Resource.Style.period_button_text);
        }

        private void DataFetched(object sender, StatsListFetchedEvent e)
        {
            var rows = e.Rows;
            var currentRows = this._mainContainer.ChildCount - HeaderViewCount;

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
                LinearLayout view = (LinearLayout)inflater.Inflate(Resource.Layout.layout_sales_stats_row, null);

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
            var vm = this.GetTypeSafeViewModel<RankingStatsListFragmentViewModel>();

            this._swipeRefreshable.SetIsBusy(true);
            vm.UpdateCurrentRegion(position);
            vm.LoadRankingsData(false);
        }

        public void OnNothingSelected(AdapterView parent)
        {
        }

        public async Task SwipeRefresh(bool forceRemote = true)
        {
            this._swipeRefreshable.SetIsBusy(true);
            var vm = this.GetTypeSafeViewModel<RankingStatsListFragmentViewModel>();
            vm.LoadRankingsData(forceRemote);

            if (!forceRemote)
            {
                // only set the spinner listener when we move to this page (in pager adapter) to prevent auto loading ;)
                // as auto loading leads to UI of previous fragment freezing
                this._regionSpinner.OnItemSelectedListener = this;
            }
        }
    }
}