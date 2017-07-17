using SQLite.Net;
using SQLite.Net.Async;
using SQLite.Net.Interop;

namespace SalesApp.Core.Services.Database
{
    class DbConnection
    {
        private SQLiteAsyncConnection _connection;

        private string _fullPath;

        private ISQLitePlatform _platform;

        public bool Configured { get; private set; } = false;

        public SQLiteAsyncConnection Connection
        {
            get
            {
                if (_connection == default(SQLiteAsyncConnection))
                {
                    InitializeConnection();
                }
                return _connection;
            }
        }

        private DbConnection()
        {
            
        }

        private static DbConnection _instance;

        public static DbConnection Instance => _instance ?? (_instance = new DbConnection());

        public void Configure(string fullPath, ISQLitePlatform platform)
        {
            this._fullPath = fullPath;
            this._platform = platform;
            Configured = true;
        }

        private void InitializeConnection()
        {
            
            if (_connection == null)
            {
                _connection = new SQLiteAsyncConnection
                    (
                        () => new SQLiteConnectionWithLock
                            (
                                _platform
                                , new SQLiteConnectionString(_fullPath, false)
                            )
                    );
                
            }
        }
    }
}