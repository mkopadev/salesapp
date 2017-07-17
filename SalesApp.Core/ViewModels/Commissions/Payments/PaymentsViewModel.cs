using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.Commissions.Payments;

namespace SalesApp.Core.ViewModels.Commissions.Payments
{
    public class PaymentsViewModel : BaseDetailsViewModel
    {
        private ObservableCollection<Payment> payments;
        private double total;

        public PaymentsViewModel(DateTime currentMonth) : base(currentMonth)
        {
            this.LoadPayments(this.CurrentMonth);
        }

        public ObservableCollection<Payment> Payments
        {
            get { return this.payments; }

            set { this.SetProperty(ref this.payments, value, () => this.Payments); }
        }

        /// <summary>
        /// Gets or sets a summary info
        /// </summary>
        public double Total
        {
            get { return this.total; }

            set { this.SetProperty(ref this.total, value, () => this.Total); }
        }

        private async Task LoadPayments(DateTime month)
        {
            this.Loading = true;

            string urlParams = this.BaseUrlParams + string.Format("&statsType={0}", "Payments Made");
            string cacheKey = this.CurrentMonth.ToString("MMMM") + "_Payments";

            PaymentsServerResponse data = await this.service.GetCommissionDetails<PaymentsServerResponse>(urlParams, cacheKey);

            if (data == null || data.Payments == null || data.Payments.Count == 0)
            {
                this.HasSales = false;
                this.SalesNotLoaded = true;

                if (data != null)
                {
                    this.Info = data.Info;
                    this.Total = data.Total;
                }

                this.Loading = false;

                return;
            }

            this.HasSales = true;
            this.SalesNotLoaded = false;

            this.Payments = new ObservableCollection<Payment>(data.Payments);
            this.Info = data.Info;
            this.Total = data.Total;

            this.Loading = false;
        }
    }
}