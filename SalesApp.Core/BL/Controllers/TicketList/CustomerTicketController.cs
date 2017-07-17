using SalesApp.Core.BL.Models.TicketList;
using SalesApp.Core.Services.Database;

namespace SalesApp.Core.BL.Controllers.TicketList
{
    /// <summary>
    /// Helps with saving/getting tickets to the data base
    /// </summary>
    public class CustomerTicketController : SQLiteDataService<CustomerTicket>
    {
    }
}