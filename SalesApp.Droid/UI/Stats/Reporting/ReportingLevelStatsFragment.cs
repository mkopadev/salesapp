using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using SalesApp.Core.BL.Models.Stats.Reporting;
using SalesApp.Core.Enums;
using SalesApp.Core.Events.Stats.Reporting;
using SalesApp.Core.Services.Stats;
using SalesApp.Core.Services.Stats.Aggregated;
using SalesApp.Core.ViewModels.Stats.Reporting;
using SalesApp.Droid.Components.UIComponents;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.UI.Stats.Reporting
{
    public class ReportingLevelStatsFragment : MvxFragmentBase, AdapterView.IOnItemSelectedListener, ISwipeRefreshFragment, View.IOnClickListener
    {
        private View _topContainer;
        private View _periodContainer;
        private LinearLayout _mainContainer;
        private Button _upButton;
        private ISwipeRefreshable _swipeRefreshable;
        private Snackbar _snackbar;
        private const int HeaderViewCount = 5;

        private List<Row> _curreRows = new List<Row>(); 

        private ColumnInfo[] Columns
        {
            get
            {
                return new[]
                {
                    new ColumnInfo(2),
                    new ColumnInfo(4, GravityFlags.Left),
                    new ColumnInfo(2),
                    new ColumnInfo(3)
                };
            }
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            this._swipeRefreshable = activity as ISwipeRefreshable;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            this.FragmentView = this.BindingInflate(Resource.Layout.stats_list_view, container, false);
            this._mainContainer = this.FragmentView.FindViewById<LinearLayout>(Resource.Id.main_content);

            ReportingLevelStatsFragmentViewModel viewModel = new ReportingLevelStatsFragmentViewModel(new ReportingLevelStatsService());
            viewModel.DataFetched += DataFetched;

            this.ViewModel = viewModel;

            // add the headers in their required order
            this._topContainer = this.AddHeaderToList(Resource.Layout.stats_reporting_level_top);
            this._upButton = this._topContainer.FindViewById<Button>(Resource.Id.btnLevelUp);
            this._upButton.Click += BtnLevelUpClick;

            this.AddHeaderToList(Resource.Layout.stats_blocks);
            View spinnerContainer = this.AddHeaderToList(Resource.Layout.stats_spinner);
            Spinner spinner = spinnerContainer.FindViewById<Spinner>(Resource.Id.table_spinner);
            this._periodContainer = this.AddHeaderToList(Resource.Layout.stats_reportinglevel_period_selector);
            this.AddColumnHeaders();

            LinearLayout nextButton = this._periodContainer.FindViewById<LinearLayout>(Resource.Id.btnListSelectorNext);
            nextButton.Click += this.BtnSelectorNext;

            LinearLayout previousButton = this._periodContainer.FindViewById<LinearLayout>(Resource.Id.btnListSelectorPrevious);
            previousButton.Click += this.BtnSelectorPrevious;

            string[] items = this.Activity.Resources.GetStringArray(Resource.Array.stats_period_array);
            var spinnerAdapter = new DefaultSpinnerAdapter().GetAdapter(items, this.Activity);
            spinner.Adapter = spinnerAdapter;
            spinner.OnItemSelectedListener = this;

            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Reporting Level Stats");

            return this.FragmentView;
        }

        private void BtnLevelUpClick(object sender, EventArgs e)
        {
            var vm = this.GetTypeSafeViewModel<ReportingLevelStatsFragmentViewModel>();

            ReportingLevelEntity.ParentItem item = vm.GetParentItem();

            // if no item or item does not have a level, do not continue
            if (item == null || !item.Level.HasValue)
            {
                return;
            }
            
            vm.UpdateSelectedItemId(item.ParentId);
            vm.UpdateSelectedLevel(item.Level ?? 0);

            this._swipeRefreshable.SetIsBusy(true);
            vm.GetReportingStats();
        }

        private void BtnSelectorPrevious(object sender, EventArgs e)
        {
            var vm = this.GetTypeSafeViewModel<ReportingLevelStatsFragmentViewModel>();

            if (vm.PreviousPeriod == null)
            {
                return;
            }

            this._swipeRefreshable.SetIsBusy(true);
            vm.GoPrevious();
        }

        private void BtnSelectorNext(object sender, EventArgs e)
        {
            var vm = this.GetTypeSafeViewModel<ReportingLevelStatsFragmentViewModel>();

            if (vm.NextPeriod == null)
            {
                return;
            }

            this._swipeRefreshable.SetIsBusy(true);
            vm.GoNext();
        }

        private void DataFetched(object sender, ReportingStatsDataFetchedEvent e)
        {
            ReportingLevelEntity reportingLevelEntity = e.Entity;

            if (reportingLevelEntity == null || !reportingLevelEntity.HasValidData)
            {
                if (reportingLevelEntity != null)
                {
                    this.ShowStatus(reportingLevelEntity.Status);
                }
            }
            else
            {
                var vm = this.GetTypeSafeViewModel<ReportingLevelStatsFragmentViewModel>();

                // if no selection period, take the first one
                var selectionPeriod = vm.GetSelectedPeriod(reportingLevelEntity.ReportStats);

                this.ActivityBase.SetScreenTitle(reportingLevelEntity.StatsType);

                // before we continue, let's filter period stuff
                ReportStat[] stats = reportingLevelEntity.ReportStats.Where(r => r.Date == selectionPeriod).ToArray();

                List<Row> rows = new List<Row>();
                rows.AddRange(stats.Select(r => new Row
                {
                    IsSelected = false,
                    Tag = new ReportingLevelEntity.Item
                    {
                        Level = r.Level,
                        ItemId = r.ItemId
                    },
                    Items = new List<string>
                    {
                        r.Rank.ToString(),
                        r.Name,
                        r.Sales.ToString(),
                        r.Prospects.ToString()
                    }
                }));

                this._curreRows = rows;
                this.RemoveNonHeaderRows();

                foreach (var row in rows)
                {
                    LayoutInflater inflater = LayoutInflater.FromContext(this.Activity);
                    LinearLayout view = (LinearLayout)inflater.Inflate(Resource.Layout.layout_sales_stats_row, null);

                    view.SetOnClickListener(this);
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

                this.ShowStatus(reportingLevelEntity.Status);
            }

            if (Build.VERSION.SdkInt == BuildVersionCodes.Kitkat)
            {
                // Hack to make Title/Period bar update on android 4.4.4
                this._topContainer.RequestLayout();
                this._periodContainer.RequestLayout();
            }

            this._swipeRefreshable.SetIsBusy(false);
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

        private void ShowStatus(ServiceReturnStatus status)
        {
            switch (status)
            {
                case ServiceReturnStatus.InitialData:
                    this.ActivityBase.ShowToast(Resource.String.something_wrong_try_again);
                    break;
                case ServiceReturnStatus.NoInternet:
                    this._snackbar = this.ActivityBase.ShowSnackbar(Resource.Id.reporting_placeholder, Resource.String.stats_refresh_prompt);
                    break;
                case ServiceReturnStatus.ParseError:
                case ServiceReturnStatus.ServerError:
                    this.ActivityBase.ShowToast(Resource.String.something_wrong_server_try_again);
                    break;
                case ServiceReturnStatus.Success:
                    this.HideSnackBar();
                    break;
            }
        }

        public void OnItemSelected(AdapterView parent, View view, int position, long id)
        {
            this._swipeRefreshable.SetIsBusy(true);
            var vm = this.GetTypeSafeViewModel<ReportingLevelStatsFragmentViewModel>();

            vm.UpdatePeriodType(position);

            // reset the selection cache period, always display the 1st when changing types
            vm.UpdatedSelectedPeriod(null);

            vm.GetReportingStats();
        }

        public void OnNothingSelected(AdapterView parent)
        {
        }

        public async Task SwipeRefresh(bool forceRemote = true)
        {
            var vm = this.GetTypeSafeViewModel<ReportingLevelStatsFragmentViewModel>();
            vm.GetReportingStats(forceRemote);
        }

        private void HideSnackBar()
        {
            if (this._snackbar == null)
            {
                return;
            }

            this._snackbar.Dismiss();
        }

        public void OnClick(View v)
        {
            int position = this._mainContainer.IndexOfChild(v);
            int indexOfChild = position - HeaderViewCount;

            if (indexOfChild < 0 || this._curreRows == null || this._curreRows.Count == 0)
            {
                return;
            }

            Row row = this._curreRows[indexOfChild];
            var vm = this.GetTypeSafeViewModel<ReportingLevelStatsFragmentViewModel>();
            var childItem = row.Tag as ReportingLevelEntity.Item;

            // if no childitem or item does not have a level, do not continue
            if (childItem == null || !childItem.Level.HasValue)
            {
                return;
            }

            vm.UpdateSelectedItemId(childItem.ItemId);
            vm.UpdateSelectedLevel(childItem.Level ?? 0);

            this._swipeRefreshable.SetIsBusy(true);
            vm.GetReportingStats();
        }
    }
}