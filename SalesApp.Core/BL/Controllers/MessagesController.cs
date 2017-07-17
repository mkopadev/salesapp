using SalesApp.Core.Services.Database;

namespace SalesApp.Core.BL.Controllers
{
    public class MessagesController : SQLiteDataService<Message>
    {
        public MessagesController(): base()
        {
        }
    }
}
