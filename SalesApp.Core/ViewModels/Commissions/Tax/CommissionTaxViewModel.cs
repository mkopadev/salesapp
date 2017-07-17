using System;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.Commissions.Tax;

namespace SalesApp.Core.ViewModels.Commissions.Tax
{
    public class CommissionTaxViewModel : BaseDetailsViewModel
    {
        private double totalEarnings;
        private double tax;

        public CommissionTaxViewModel(DateTime currentMonth) : base(currentMonth)
        {
            this.LoadTaxDetails();
        }

        /// <summary>
        /// Gets or sets a summary info
        /// </summary>
        public double TotalEarnings
        {
            get { return this.totalEarnings; }

            set { this.SetProperty(ref this.totalEarnings, value, () => this.TotalEarnings); }
        }

        /// <summary>
        /// Gets or sets a summary info
        /// </summary>
        public double Tax
        {
            get { return this.tax; }

            set { this.SetProperty(ref this.tax, value, () => this.Tax); }
        }

        private async Task LoadTaxDetails()
        {
            string urlParams = this.BaseUrlParams + string.Format("&statsType={0}", "Tax");
            string cacheKey = this.CurrentMonth.ToString("MMMM") + "_Tax";

            CommissionTaxServerResponse data = await this.service.GetCommissionDetails<CommissionTaxServerResponse>(urlParams, cacheKey);

            if (data == null)
            {
                return;
            }

            this.Info = data.Info;
            this.Tax = data.Taxation.Tax;
            this.TotalEarnings = data.Taxation.Earnings;
        }
    }
}