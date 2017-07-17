using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.Commissions.Adjustments;

namespace SalesApp.Core.ViewModels.Commissions.Adjustments
{
    public class AdjustmentsViewModel : BaseDetailsViewModel
    {
        private ObservableCollection<Adjustment> adjustments;
        private double total;

        public AdjustmentsViewModel(DateTime currentMonth) : base(currentMonth)
        {
            this.LoadAdjustments();
        }

        /// <summary>
        /// Gets or sets a list of files in the SalesApp logs directory
        /// </summary>
        public ObservableCollection<Adjustment> Adjustments
        {
            get
            {
                return this.adjustments;
            }

            set
            {
                this.SetProperty(ref this.adjustments, value, () => this.Adjustments);
            }
        }

        /// <summary>
        /// Gets or sets a summary info
        /// </summary>
        public double Total
        {
            get
            {
                return this.total;
            }

            set
            {
                this.SetProperty(ref this.total, value, () => this.Total);
            }
        }

        private async Task LoadAdjustments()
        {
            this.Loading = true;

            string urlParams = this.BaseUrlParams + string.Format("&statsType={0}", "Adjustments");
            string cacheKey = this.CurrentMonth.ToString("MMMM") + "_Adjustments";

            AdjustmentServerResponse data = await this.service.GetCommissionDetails<AdjustmentServerResponse>(urlParams, cacheKey);
            if (data == null || data.Total == 0 || data.Adjustments == null || data.Adjustments.Count == 0)
            {
                if (data != null)
                {
                    this.Info = data.Info;
                }

                this.HasSales = false;
                this.SalesNotLoaded = true;
            }
            else
            {
                this.HasSales = true;
                this.SalesNotLoaded = false;

                this.Total = data.Total;
                this.Info = data.Info;
                this.Adjustments = new ObservableCollection<Adjustment>(data.Adjustments);
            }

            this.Loading = false;
        }
    }
}