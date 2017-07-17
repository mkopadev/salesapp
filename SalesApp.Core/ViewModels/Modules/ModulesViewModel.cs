namespace SalesApp.Core.ViewModels.Modules
{
    public class ModulesViewModel : BaseViewModel
    {
        private bool _canRegister;

        public bool CanRegister
        {
            get
            {
                return this._canRegister;
            }

            set
            {
                this.SetProperty(ref this._canRegister, value, () => this.CanRegister);
            }
        }
    }
}