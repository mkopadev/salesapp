namespace SalesApp.Core.Enums.TicketList
{
    /// <summary>
    /// Represents the different statuses that a ticket can exist in
    /// </summary>
    public enum TicketStatus
    {
        /// <summary>
        /// Means that the ticket is still open
        /// </summary>
        Open = 1,

        /// <summary>
        /// Means that the ticket has been resolved and closed
        /// </summary>
        Closed = 2
    }
}