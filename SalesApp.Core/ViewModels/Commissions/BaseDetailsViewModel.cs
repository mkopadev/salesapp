using System;
using SalesApp.Core.Api.Commissions;
using SalesApp.Core.Auth;
using SalesApp.Core.Services.Commissions;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.ViewModels.Commissions
{
    public abstract class BaseDetailsViewModel : BaseViewModel
    {
        protected CommissionService service;
        private DateTime currentMonth;
        private string noDataMessage;
        private string monthInfo;
        private string info;
        private bool hasSales;
        private bool loading;
        private bool salesNotLoaded;

        protected BaseDetailsViewModel(DateTime currentMonth)
        {
            this.CurrentMonth = currentMonth;
            this.service = new CommissionService(new CommissionApi("commissions"));
        }

        protected string BaseUrlParams
        {
            get
            {
                string userId = Resolver.Instance.Get<ISalesAppSession>().UserId.ToString();

                var monthStart = new DateTime(this.CurrentMonth.Year, this.CurrentMonth.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                string from = monthStart.ToString("yyyy-MM-dd");
                string to = monthEnd.ToString("yyyy-MM-dd");

                string urlParams = string.Format("/stats?userId={0}&from={1}&to={2}", userId, from, to);

                return urlParams;
            }
        }

        public string NoDataMessage
        {
            get
            {
                return this.noDataMessage;
            }

            set
            {
                this.SetProperty(ref this.noDataMessage, value, () => this.NoDataMessage);
            }
        }

        public string MonthInfo
        {
            get
            {
                this.monthInfo = this.currentMonth.ToString("MMMM, yyyy");
                return this.monthInfo;
            }

            set
            {
                this.SetProperty(ref this.monthInfo, value, () => this.MonthInfo);
            }
        }

        public DateTime CurrentMonth
        {
            get
            {
                return this.currentMonth;
            }

            set
            {
                this.SetProperty(ref this.currentMonth, value, () => this.CurrentMonth);
                string monthYear = this.currentMonth.ToString("MMMM, yyyy");
                this.MonthInfo = monthYear;
            }
        }

        public string Info
        {
            get
            {
                return this.info;
            }

            set
            {
                this.SetProperty(ref this.info, value, () => this.Info);
            }
        }

        public bool Loading
        {
            get
            {
                return this.loading;
            }

            set
            {
                this.SetProperty(ref this.loading, value, () => this.Loading);
            }
        }

        public bool SalesNotLoaded
        {
            get
            {
                return this.salesNotLoaded;
            }

            set
            {
                this.SetProperty(ref this.salesNotLoaded, value, () => this.SalesNotLoaded);
            }
        }

        public bool HasSales
        {
            get
            {
                return this.hasSales;
            }

            set
            {
                this.SetProperty(ref this.hasSales, value, () => this.HasSales);
            }
        }
    }
}