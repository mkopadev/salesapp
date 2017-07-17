using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using SalesApp.Core.Api.DownSync;
using SalesApp.Core.BL.Controllers.Synchronization;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.BL.Models.TicketList;
using SalesApp.Core.Enums.TicketList;
using SalesApp.Core.Framework;
using SalesApp.Core.Services.Database;

namespace SalesApp.Core.ViewModels.TicketList
{
    /// <summary>
    /// View model for the ticket list
    /// </summary>
    public class TicketListViewModel : BaseViewModel
    {
        private MvxCommand<AbstractTicketBase> itemClickCommand;

        public Action<AbstractTicketBase> ItemClickAction { get; set; }

        public TicketType TicketType { get; set; }

        private CustomerSearchResult customer;

        /// <summary>
        /// The criteria to use when fetching the tickets
        /// </summary>
        public CustomerSearchResult Customer
        {
            get
            {
                return this.customer;
            }

            set
            {
                this.customer = value;

                if (value != null)
                {
                    this.ToolBarVisible = false;
                }
            }
        }

        private bool toolbarVisible;

        /// <summary>
        /// The tickets to be displayed in the list
        /// </summary>
        private SortedObservableCollection<AbstractTicketBase> tickets;

        /// <summary>
        /// The sync controller
        /// </summary>
        private SyncingController syncingController;

        /// <summary>
        /// Gets the sync controller
        /// </summary>
        public SyncingController SyncController
        {
            get
            {
                this.syncingController = this.syncingController ?? new SyncingController();
                return this.syncingController;
            }
        }

        public TicketListViewModel()
        {
            this.ToolBarVisible = true;
        }

        /// <summary>
        /// Requests for new tickets from the server
        /// </summary>
        /// <param name="fetchRemote">Whether to fetch remote first</param>
        /// <returns>An empty task</returns>
        private async Task FetchDsrTickets(bool fetchRemote = false)
        {
            if (fetchRemote)
            {
                await this.SyncController.SyncDownAsync<DownSyncServerResponse<DsrTicket>, DsrTicket>();
            }

            string sql = "SELECT * FROM DsrTicket ORDER BY Status, DateRaised DESC";

            List<DsrTicket> dsrTickets = await new QueryRunner().RunQuery<DsrTicket>(sql);

            if (dsrTickets == null || dsrTickets.Count == 0)
            {
                return;
            }

            this.ShowSnackBar = false;
            this.UpdateTickets(dsrTickets);
        }

        /// <summary>
        /// Requests for new tickets from the server
        /// </summary>
        /// <param name="fetchRemote">Whether to fetch remote first</param>
        /// <returns>An empty task</returns>
        private async Task FetchCustomerTickets(bool fetchRemote = false)
        {
            if (fetchRemote)
            {
                await this.SyncController.SyncDownAsync<DownSyncServerResponse<CustomerTicket>, CustomerTicket>();
            }

            List<CustomerTicket> customerTickets = new List<CustomerTicket>();

            if (this.Customer == null)
            {
                string sql = "SELECT * FROM CustomerTicket ORDER BY Status, DateRaised DESC";

                customerTickets = await new QueryRunner().RunQuery<CustomerTicket>(sql);
            }
            else if (!string.IsNullOrEmpty(this.Customer.Phone))
            {
                string sql = string.Format("SELECT * FROM CustomerTicket WHERE CustomerPhone = '{0}' ORDER BY Status, DateRaised DESC", this.Customer.Phone);

                customerTickets = await new QueryRunner().RunQuery<CustomerTicket>(sql);
            }

            if (customerTickets == null || customerTickets.Count == 0)
            {
                if (!this.ShowSnackBar)
                {
                    this.ShowSnackBar = true;
                }

                return;
            }

            this.ShowSnackBar = false;
            this.UpdateTickets(customerTickets);
        }

        private void UpdateTickets(List<DsrTicket> dsrTickets)
        {
            List<string> headers = new List<string>();

            foreach (var ticket in dsrTickets)
            {
                string header = ticket.SectionHeader;

                if (headers.Contains(header))
                {
                    continue;
                }

                ticket.IsSectionHeader = true;
                headers.Add(header);
            }

            this.Tickets = new SortedObservableCollection<AbstractTicketBase>(dsrTickets);
        }

        private void UpdateTickets(List<CustomerTicket> customerTickets)
        {
            List<string> headers = new List<string>();

            foreach (var ticket in customerTickets)
            {
                string header = ticket.SectionHeader;

                if (headers.Contains(header))
                {
                    continue;
                }

                ticket.IsSectionHeader = true;
                headers.Add(header);
            }

            this.Tickets = new SortedObservableCollection<AbstractTicketBase>(customerTickets);
        }

        public async Task FetchTickets(bool fetchRemote = false)
        {
            if (this.TicketType == TicketType.Customer)
            {
                await this.FetchCustomerTickets(fetchRemote);
            }
            else
            {
                await this.FetchDsrTickets(fetchRemote);
            }
        }

        private bool snackBarVisible;

        public bool ShowSnackBar
        {
            get
            {
                return this.snackBarVisible;
            }

            set
            {
                this.SetProperty(ref this.snackBarVisible, value, () => this.ShowSnackBar);
            }
        }

        /// <summary>
        /// Gets or sets the list of tickets
        /// </summary>
        public SortedObservableCollection<AbstractTicketBase> Tickets
        {
            get
            {
                return this.tickets;
            }

            set
            {
                this.SetProperty(ref this.tickets, value, () => this.Tickets);
            }
        }

        /// <summary>
        /// The tool bar should not be visible when we are at the customer details activity
        /// </summary>
        public bool ToolBarVisible
        {
            get
            {
                return this.toolbarVisible;
            }

            set
            {
                this.SetProperty(ref this.toolbarVisible, value, () => this.ToolBarVisible);
            }
        }

        public ICommand ItemClickCommand
        {
            get
            {
                this.itemClickCommand = this.itemClickCommand ?? new MvxCommand<AbstractTicketBase>(this.ItemClick);
                return this.itemClickCommand;
            }
        }

        private void ItemClick(AbstractTicketBase item)
        {
            if (this.ItemClickAction == null)
            {
                throw new NullReferenceException(string.Format("I {0}, dont know what to do with the clicked item", this.GetType().FullName));
            }

            this.ItemClickAction(item);
        }
    }
}