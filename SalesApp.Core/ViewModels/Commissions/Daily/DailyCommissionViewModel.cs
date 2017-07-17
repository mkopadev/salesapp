using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Models.Commissions;
using SalesApp.Core.BL.Models.Commissions.Daily;
using SalesApp.Core.BL.Models.Commissions.Quality;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.ViewModels.Commissions.Daily
{
    public class DailyCommissionViewModel : BaseDetailsViewModel
    {
        private ObservableCollection<DailyCommissionItem> _commissions;
        private CommissionType _commissionType;
        private string _firstColLabel;
        private string _secondColLabel;
        private string _thirdColLabel;
        private string _summaryLabel;
        private double _total;
        private bool _secondColVisisble;

        public DailyCommissionViewModel(DateTime currentMonth) : base(currentMonth)
        {
        }

        public CommissionType CommissionType
        {
            get
            {
                return this._commissionType;
            }

            set
            {
                this._commissionType = value;
                this.LoadData();
            }
        }

        public string SummaryLabel
        {
            get
            {
                return this._summaryLabel;
            }

            set
            {
                this.SetProperty(ref this._summaryLabel, value, () => this.SummaryLabel);
            }
        }

        public string FirstColLabel
        {
            get
            {
                return this._firstColLabel;
            }

            set
            {
                this.SetProperty(ref this._firstColLabel, value, () => this.FirstColLabel);
            }
        }

        public bool SecondColVisible
        {
            get
            {
                return this._secondColVisisble;
            }

            set
            {
                this.SetProperty(ref this._secondColVisisble, value, () => this.SecondColVisible);
            }
        }

        public string SecondColLabel
        {
            get
            {
                return this._secondColLabel;
            }

            set
            {
                this.SetProperty(ref this._secondColLabel, value, () => this.SecondColLabel);
            }
        }

        public string ThirdColLabel
        {
            get
            {
                return this._thirdColLabel;
            }

            set
            {
                this.SetProperty(ref this._thirdColLabel, value, () => this.ThirdColLabel);
            }
        }

        public ObservableCollection<DailyCommissionItem> Commmissions
        {
            get
            {
                return this._commissions;
            }

            set
            {
                this.SetProperty(ref this._commissions, value, () => this.Commmissions);
            }
        }

        public double Total
        {
            get
            {
                return this._total;
            }

            set
            {
                this.SetProperty(ref this._total, value, () => this.Total);
            }
        }

        private async void LoadData()
        {
            this.Loading = true;
            if (this.CommissionType == CommissionType.DailyCommission)
            {
                await this.LoadDailyCommission(this.CurrentMonth);
            }
            else
            {
                await this.LoadQualityCommmission(this.CurrentMonth);
            }

            this.Loading = false;
        }

        private async Task LoadQualityCommmission(DateTime month)
        {
            string userId = Resolver.Instance.Get<ISalesAppSession>().UserId.ToString();

            var monthStart = new DateTime(month.Year, month.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            string from = monthStart.ToString("yyyy-MM-dd");
            string to = monthEnd.ToString("yyyy-MM-dd");
            string type = "Quality Commissions";

            string urlParams = string.Format("/stats?userId={0}&from={1}&to={2}&statsType={3}", userId, from, to, type);

            string qualityKey = monthStart.ToString("MMMM") + "_Quality";
            QualityCommissionResponse data = await this.service.GetCommissionDetails<QualityCommissionResponse>(urlParams, qualityKey);

            if (data == null || data.QualityCommission == null || data.QualityCommission.Count == 0)
            {
                this.HasSales = false;
                this.SalesNotLoaded = true;

                if (data != null)
                {
                    this.Info = data.Info;
                    this.Total = data.Total;
                }

                return;
            }

            this.HasSales = true;
            this.SalesNotLoaded = false;

            List<DailyCommissionItem> items = new List<DailyCommissionItem>();
            foreach (var item in data.QualityCommission)
            {
                DailyCommissionItem i = new DailyCommissionItem
                {
                    Commissions = item.Commissions,
                    Date = item.Date
                };

                items.Add(i);
            }

            this.Commmissions = new ObservableCollection<DailyCommissionItem>(items);
            this.Info = data.Info;
            this.Total = data.Total;
        }

        private async Task LoadDailyCommission(DateTime month)
        {
            string userId = Resolver.Instance.Get<ISalesAppSession>().UserId.ToString();

            var monthStart = new DateTime(month.Year, month.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            string from = monthStart.ToString("yyyy-MM-dd");
            string to = monthEnd.ToString("yyyy-MM-dd");
            string type = "Daily Commissions";

            string urlParams = string.Format("/stats?userId={0}&from={1}&to={2}&statsType={3}", userId, from, to, type);

            string dailyKey = monthStart.ToString("MMMM") + "_daily";

            DailyCommissionResponse data = await this.service.GetCommissionDetails<DailyCommissionResponse>(urlParams, dailyKey);

            if (data == null || data.DailyCommission == null || data.DailyCommission.Count == 0)
            {
                this.HasSales = false;
                this.SalesNotLoaded = true;

                if (data != null)
                {
                    this.Info = data.Info;
                    this.Total = data.Total;
                }

                return;
            }

            this.HasSales = true;
            this.SalesNotLoaded = false;

            this.Commmissions = new ObservableCollection<DailyCommissionItem>(data.DailyCommission);
            this.Info = data.Info;
            this.Total = data.Total;
        }
    }
}