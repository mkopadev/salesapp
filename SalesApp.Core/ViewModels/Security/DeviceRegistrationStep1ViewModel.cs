namespace SalesApp.Core.ViewModels.Security
{
    /// <summary>
    /// Model for the step one of device registration - country selection screen
    /// </summary>
    public class DeviceRegistrationStep1ViewModel : BaseViewModel
    {
        private bool _countrySelected;

        public bool CountrySelected
        {
            get
            {
                return this._countrySelected;
            }

            set
            {
                this.SetProperty(ref this._countrySelected, value, () => this.CountrySelected);
            }
        }
    }
}
