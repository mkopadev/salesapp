using System.Threading.Tasks;
using SalesApp.Core.Services.Database.Logging;
using SQLite.Net.Interop;

namespace SalesApp.Core.Services.Database
{
    public interface IDbConnection
    {
        Task InitializeDb(ISQLitePlatform sqLitePlatform, string path);

        string GetScript(int version);

        void LogDbLib(object sender, LogEvent e);
    }
}