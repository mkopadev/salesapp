namespace SalesApp.Core.ViewModels.Person.Customer
{
    /// <summary>
    /// This class is the view model for CustomerDetailFragment />
    /// For now we only bind the photo
    /// </summary>
    public class CustomerDetailFragmentViewModel : BaseViewModel
    {
        private string _mostRecentPhoto;

        public string MostRecentPhoto
        {
            get
            {
                return this._mostRecentPhoto;
            }

            set
            {
                this.SetProperty(ref this._mostRecentPhoto, value, () => this.MostRecentPhoto);
            }
        }
    }
}