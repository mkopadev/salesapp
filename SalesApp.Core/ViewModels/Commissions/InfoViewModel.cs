namespace SalesApp.Core.ViewModels.Commissions
{
    public class InfoViewModel : BaseViewModel
    {
        private string title;
        private string message;
        private bool hasIcon;

        /// <summary>
        /// Gets or sets a title
        /// </summary>
        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.SetProperty(ref this.title, value, () => this.Title);
            }
        }

        /// <summary>
        /// Gets or sets a message
        /// </summary>
        public string Message
        {
            get
            {
                return this.message;
            }

            set
            {
                this.SetProperty(ref this.message, value, () => this.Message);
            }
        }

        public bool HasIcon
        {
            get
            {
                return this.hasIcon;
            }

            set
            {
                this.SetProperty(ref this.hasIcon, value, () => this.HasIcon);
            }
        }
    }
}