using SalesApp.Core.Services.Platform;
using SQLite.Net.Interop;

namespace SalesApp.Droid.Services.Platform
{
    public class XamarinPlatform : IXamarinPlatform
    {
        public ISQLitePlatform GetSqlitePlatform()
        {
            ISQLitePlatform androidSQlitePlatform = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();
            return androidSQlitePlatform;
        }
    }
}