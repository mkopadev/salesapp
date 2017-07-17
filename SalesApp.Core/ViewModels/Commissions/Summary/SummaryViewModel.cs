using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using SalesApp.Core.Api.Commissions;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Models.Commissions.Summary;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Commissions;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.ViewModels.Commissions.Summary
{
    public class SummaryViewModel : BaseViewModel
    {
        public const int MaxBackScrollMonths = 3;
        private ObservableCollection<CommissionItem> _earnings;
        private ObservableCollection<CommissionItem> _deductions;
        private ObservableCollection<CommissionItem> _summary;
        private MvxCommand<CommissionItem> _itemClickedCommand;
        private CommissionService _service;
        private double _totalEarnings;
        private double _totalDeductions;
        private double _balance;
        private bool _canLoad = true;
        private bool _busy;
        private bool _nodata;
        private bool _nextVisible;
        private bool _previousVisible;
        private bool _showSnackkBar;
        private bool _suspended;

        private string _previousMonth;
        private string _nextMonth;

        public SummaryViewModel(DateTime month)
        {
            this.ThisMonth = month;
            this.CurrentMonth = this.ThisMonth.ToString();
            this._service = new CommissionService(new CommissionApi("commissions"));
        }

        public int MonthDelta { get; set; }

        public Func<CommissionItem, bool> HasDetailsFunction { get; set; }

        public Action<DateTime, int> MonthLoaded { get; set; }

        /// <summary>
        /// Gets or sets the opening the details screen when an item is clicked. To be implemented and instantiated in each platform specific-project.
        /// </summary>
        public Action<CommissionItem> GoToDetailsScreen { get; set; }

        public DateTime ThisMonth { get; private set; }

        public bool NoData
        {
            get
            {
                return this._nodata;
            }

            set
            {
                if (value == this._nodata)
                {
                    return;
                }

                this.SetProperty(ref this._nodata, value, () => this.NoData);
            }
        }

        public bool Busy
        {
            get
            {
                return this._busy;
            }

            set
            {
                if (value == this._busy)
                {
                    return;
                }

                this.SetProperty(ref this._busy, value, () => this.Busy);
            }
        }

        public bool Suspended
        {
            get
            {
                return this._suspended;
            }

            set
            {
                if (value == this._suspended)
                {
                    return;
                }

                this.SetProperty(ref this._suspended, value, () => this.Suspended);
            }
        }

        public bool CanLoad
        {
            get
            {
                return this._canLoad;
            }

            set
            {
                this.SetProperty(ref this._canLoad, value, () => this.CanLoad);
                this.Busy = !value;
            }
        }

        public bool NextVisible
        {
            get
            {
                return this._nextVisible;
            }

            set
            {
                this.SetProperty(ref this._nextVisible, value, () => this.NextVisible);
            }
        }

        public bool PreviousVisible
        {
            get
            {
                return this._previousVisible;
            }

            set
            {
                this.SetProperty(ref this._previousVisible, value, () => this.PreviousVisible);
            }
        }

        public bool ShowSnackBar
        {
            get
            {
                return this._showSnackkBar;
            }

            set
            {
                this.SetProperty(ref this._showSnackkBar, value, () => this.ShowSnackBar);
            }
        }

        /// <summary>
        /// Total deductions
        /// </summary>
        public double Balance
        {
            get
            {
                return this._balance;
            }

            set
            {
                this.SetProperty(ref this._balance, value, () => this.Balance);
            }
        }

        /// <summary>
        /// Total deductions
        /// </summary>
        public double TotalDeductions
        {
            get
            {
                return this._totalDeductions;
            }

            set
            {
                this.SetProperty(ref this._totalDeductions, value, () => this.TotalDeductions);
            }
        }

        /// <summary>
        /// Total earnings
        /// </summary>
        public double TotalEarnings
        {
            get
            {
                return this._totalEarnings;
            }

            set
            {
                this.SetProperty(ref this._totalEarnings, value, () => this.TotalEarnings);
            }
        }

        /// <summary>
        /// Gets or sets a summary info
        /// </summary>
        public string CurrentMonth
        {
            get
            {
                return this.ThisMonth.ToMonthName();
            }

            set
            {
                this.ThisMonth = DateTime.Parse(value);
                this.RaisePropertyChanged(() => this.CurrentMonth);
                this.NextMonth = this.ThisMonth.AddMonths(1).ToShortMonthName();
                this.PreviousMonth = this.ThisMonth.AddMonths(-1).ToShortMonthName();

                if (this.MonthLoaded != null)
                {
                    this.MonthLoaded(this.ThisMonth, this.MonthDelta);
                }
            }
        }

        /// <summary>
        /// Gets or sets a summary info
        /// </summary>
        public string PreviousMonth
        {
            get
            {
                return this._previousMonth;
            }

            set
            {
                this.SetProperty(ref this._previousMonth, value, () => this.PreviousMonth);
            }
        }

        /// <summary>
        /// Gets or sets a summary info
        /// </summary>
        public string NextMonth
        {
            get
            {
                return this._nextMonth;
            }

            set
            {
                this.SetProperty(ref this._nextMonth, value, () => this.NextMonth);
            }
        }

        public ObservableCollection<CommissionItem> Earnings
        {
            get
            {
                return this._earnings;
            }

            set
            {
                for (int i = 0; i < value.Count; i++)
                {
                    CommissionItem item = value[i];
                    bool hasDetails = this.HasDetailsFunction(item);
                    item.HasDetails = hasDetails;
                }

                this.SetProperty(ref this._earnings, value, () => this.Earnings);
            }
        }

        public ObservableCollection<CommissionItem> Deductions
        {
            get
            {
                return this._deductions;
            }

            set
            {
                for (int i = 0; i < value.Count; i++)
                {
                    CommissionItem item = value[i];
                    bool hasDetails = this.HasDetailsFunction(item);
                    item.HasDetails = hasDetails;
                }

                this.SetProperty(ref this._deductions, value, () => this.Deductions);
            }
        }

        public ObservableCollection<CommissionItem> Summary
        {
            get
            {
                return this._summary;
            }

            set
            {
                for (int i = 0; i < value.Count; i++)
                {
                    CommissionItem item = value[i];
                    bool hasDetails = this.HasDetailsFunction(item);
                    item.HasDetails = hasDetails;
                }

                this.SetProperty(ref this._summary, value, () => this.Summary);
            }
        }

        public ICommand PreviousMonthCommand
        {
            get
            {
                return new MvxCommand(this.LoadPreviousMonth);
            }
        }

        public ICommand NextMonthCommand
        {
            get
            {
                return new MvxCommand(this.LoadNextMonth);
            }
        }

        public ICommand ItemClickCommand
        {
            get
            {
                this._itemClickedCommand = this._itemClickedCommand ?? new MvxCommand<CommissionItem>(this.ClickEarningsItem);
                return this._itemClickedCommand;
            }
        }

        private void ClickEarningsItem(CommissionItem item)
        {
            if (this.GoToDetailsScreen == null)
            {
                throw new NullReferenceException(string.Format("I, {0} dont dont know what to do with the selected item", this.GetType().FullName));
            }

            this.GoToDetailsScreen(item);
        }

        private void ClearCurrentData()
        {
            this.Earnings = new ObservableCollection<CommissionItem>();
            this.TotalEarnings = 0.0;
            this.Deductions = new ObservableCollection<CommissionItem>();
            this.TotalDeductions = 0.0;
            this.Summary = new ObservableCollection<CommissionItem>();
            this.Balance = 0.0;
        }

        public async void LoadPreviousMonth()
        {
            if (this.MonthDelta >= MaxBackScrollMonths)
            {
                return;
            }

            this.ClearCurrentData();
            DateTime lastMonth = this.ThisMonth.AddMonths(-1);
            await this.LoadMonthData(lastMonth);
            this.MonthDelta++;

            if (this.MonthDelta >= MaxBackScrollMonths)
            {
                this.PreviousVisible = false;
            }

            this.CurrentMonth = lastMonth.ToString();
            this.NextVisible = true;
        }

        public async void LoadNextMonth()
        {
            if (this.MonthDelta == 0)
        {
                return;
            }

            this.ClearCurrentData();
            DateTime nxtMonth = this.ThisMonth.AddMonths(1);
            await this.LoadMonthData(nxtMonth);
            this.MonthDelta--;

            if (this.MonthDelta == 0)
            {
                this.NextVisible = false;
            }

            this.CurrentMonth = nxtMonth.ToString();
            this.PreviousVisible = true;
        }

        public async Task LoadMonthData(DateTime month = default(DateTime))
        {
            try
            {
            this.CanLoad = false;
            if (month == default(DateTime))
            {
                month = DateTime.Now;
            }

            string userId = Resolver.Instance.Get<ISalesAppSession>().UserId.ToString();

            var monthStart = new DateTime(month.Year, month.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            string from = monthStart.ToString("yyyy-MM-dd");
            string to = monthEnd.ToString("yyyy-MM-dd");

            string urlParams = string.Format("?userId={0}&from={1}&to={2}", userId, from, to);

            string cacheKey = monthStart.ToString("MMMM") + "_CommissionsSummary";

                CommissionSummaryResponse response = await this._service.GetCommissionSummary(urlParams, cacheKey);
                this.ShowSnackBar = false;

            if (response == null)
            {
                    this.NoData = true;
            }
            else
            {
                this.Earnings = new ObservableCollection<CommissionItem>(response.Earnings.Earnings);
                this.TotalEarnings = response.Earnings.Total;
                this.Deductions = new ObservableCollection<CommissionItem>(response.Deductions.Deductions);
                this.TotalDeductions = response.Deductions.Total;
                this.Summary = new ObservableCollection<CommissionItem>(response.Summary.Summary);
                this.Balance = response.Summary.Balance;
            }
            }
            catch (NotConnectedToInternetException)
            {
                this.ShowSnackBar = true;
            }
            catch (UnauthorizedHttpException)
            {
                this.Suspended = true;
            }
            finally
            {
            this.CanLoad = true;
        }
    }
}
}