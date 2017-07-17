namespace SalesApp.Core.ViewModels.Stats.Reporting
{
    public class ReportingLevelStatsViewModel : BaseViewModel
    {
        private bool _busy;

        public bool Busy
        {
            get
            {
                return this._busy;
            }

            set
            {
                this.SetProperty(ref this._busy, value, () => this.Busy);
            }
        }
    }
}