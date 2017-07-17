using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Services.Database;

namespace SalesApp.Core.BL.Controllers.People
{
    /// <summary>
    /// Class to help save/retrieve prospect products into the local database
    /// </summary>
    public class ProspectProductController : SQLiteDataService<ProspectProduct>
    {
    }
}