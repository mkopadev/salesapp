using SalesApp.Core.BL.Models.Modules.Facts;

namespace SalesApp.Core.ViewModels.Modules.Facts
{
    public class FactDetailsViewModel : BaseViewModel
    {
        public Fact _fact;

        public Fact Fact
        {
            get
            {
                return _fact;
            }

            set
            {
                this.SetProperty(ref this._fact, value, () => this.Fact);
            }
        }
    }
}