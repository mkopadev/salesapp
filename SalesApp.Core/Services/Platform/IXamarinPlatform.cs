using SQLite.Net.Interop;

namespace SalesApp.Core.Services.Platform
{
    public interface IXamarinPlatform
    {
        ISQLitePlatform GetSqlitePlatform();
    }
}