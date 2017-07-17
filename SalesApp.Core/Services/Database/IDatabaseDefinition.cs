using System;
using System.Threading.Tasks;
using SQLite.Net.Interop;

namespace SalesApp.Core.Services.Database
{
    public interface IDatabaseDefinition
    {
        /// <summary>
        /// Returns the current database's newVersion number
        /// </summary>
        Task<int> GetVersion();

        /// <summary>
        /// Using scripts provided by the platform the app is running on, this creates or updates a database.
        /// </summary>
        /// <returns>The database newVersion</returns>
        Task<int> InitializeDatabase(Func<int, string> scriptGetter, string fullPath, ISQLitePlatform platform);
    }
}