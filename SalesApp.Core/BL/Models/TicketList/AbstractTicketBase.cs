using System;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Enums.TicketList;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.ViewModels;
using SQLite.Net.Attributes;

namespace SalesApp.Core.BL.Models.TicketList
{
    /// <summary>
    /// A base class for tickets models used in the ticket list activity
    /// </summary>
    public abstract class AbstractTicketBase : BusinessEntityBase, IListSectionItem
    {
        private IDeviceResource _deviceResource = Resolver.Instance.Get<IDeviceResource>();

        /// <summary>
        /// Gets or sets the date the ticket was raised
        /// </summary>
        public DateTime DateRaised { get; set; }

        /// <summary>
        /// Gets or sets the ticket status
        /// </summary>
        public TicketStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the ticket description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the issue
        /// </summary>
        public string Issue { get; set; }

        /// <summary>
        /// Gets or sets the ticket reference number
        /// </summary>
        public string RefNo { get; set; }

        [Ignore]
        public bool IsSectionHeader { get; set; }

        [Ignore]
        public string SectionHeader
        {
            get
            {
               return this.Status + " Tickets";
            }
        }

        [Ignore]
        public int StatusIcon
        {
            get
            {
                if (this.Status == TicketStatus.Closed)
                {
                    return _deviceResource.TicketClosedResourceId;
                }

                return _deviceResource.TicketOpenResourceId;
            }
        }

        [Ignore]
        public bool IssueVisible
        {
            get
            {
                return this.GetType() == typeof(DsrTicket);
            }
        }
    }
}