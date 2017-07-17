using SalesApp.Core.BL.Models.Syncing;
using SalesApp.Core.Services.Database;

namespace SalesApp.Core.BL.Controllers.DownSync
{
    /// <summary>
    /// Controller for down sync functionality
    /// </summary>
    public class DownSyncController : SQLiteDataService<DownSyncTracker>
    {
    }
}
