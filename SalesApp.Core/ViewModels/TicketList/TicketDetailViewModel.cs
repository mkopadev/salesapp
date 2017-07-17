using SalesApp.Core.BL.Models.TicketList;

namespace SalesApp.Core.ViewModels.TicketList
{
    /// <summary>
    /// This is the portable view model that contains the data and actions to support the ticket detail view
    /// </summary>
    public class TicketDetailViewModel : BaseViewModel
    {
        /// <summary>
        /// Determines whether or not the product is shown
        /// </summary>
        private bool productVisible;

        /// <summary>
        /// The label of the second text view. It is Name for customer ticket and Issue for DSR ticket
        /// </summary>
        private string secondLabel;

        /// <summary>
        /// The given ticket
        /// </summary>
        private AbstractTicketBase ticket;

        /// <summary>
        /// Gets or sets the given ticket
        /// </summary>
        public AbstractTicketBase Ticket
        {
            get
            {
                return this.ticket;
            }

            set
            {
                this.SetProperty(ref this.ticket, value, () => this.Ticket);
            }
        }

        /// <summary>
        /// Gets or sets the label of the second text view
        /// </summary>
        public string SecondLabel
        {
            get
            {
                return this.secondLabel;
            }

            set
            {
                this.SetProperty(ref this.secondLabel, value, () => this.SecondLabel);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the product is displayed
        /// </summary>
        public bool ProductVisible
        {
            get
            {
                return this.productVisible;
            }

            set
            {
                this.SetProperty(ref this.productVisible, value, () => this.ProductVisible);
            }
        }
    }
}
