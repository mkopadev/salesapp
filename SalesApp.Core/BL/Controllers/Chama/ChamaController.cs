using SalesApp.Core.BL.Models.Chama;
using SalesApp.Core.Services.Database;

namespace SalesApp.Core.BL.Controllers.Chama
{
    public class ChamaController : SQLiteDataService<Chamas>
    {
    }
}