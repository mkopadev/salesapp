using System;
using System.Resources;
using System.Threading.Tasks;
using Akavache.Sqlite3.Internal;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database.Logging;
using SalesApp.Core.Services.DependancyInjection;
using SQLite.Net.Interop;

namespace SalesApp.Core.Services.Database
{
    public class DatabaseInstance : IDbConnection
    {
        private ILog _logger = Resolver.Instance.Get<ILog>();
        public const string DbName = "SalesAppDB.db3";

        public static DatabaseInstance Instance = new DatabaseInstance();

        private DatabaseInstance()
        {
        }

        public SQLiteConnection Connection
        {
            get { return null; }
        }

        public async Task InitializeDb(ISQLitePlatform sqLitePlatform, string path)
        {
            try
            {
                DatabaseDefinition dbDefinition = new DatabaseDefinition();
                DataAccess.Instance.EventOccured += this.LogDbLib;
                await dbDefinition.InitializeDatabase(this.GetScript, path, sqLitePlatform);
            }
            catch (Exception ex)
            {
                this._logger.Debug(ex);
            }
        }

        private void Instance_EventOccured(object sender, LogEvent e)
        {
            throw new NotImplementedException();
        }

        public string GetScript(int version)
        {
            try
            {
                string script = DbScript.ResourceManager.GetString("V" + version);
                return script;
            }
            catch (MissingManifestResourceException)
            {
#pragma warning disable 1587
                /// PCL does not support enumeration of resources    
                /// This exception is thrown when we request for a missing resource so
                /// when that happens we intepret that to mean all available scripts have 
                /// been processed and return null to signify this.
#pragma warning restore 1587
                return null;
            }
        }

        public void LogDbLib(object sender, LogEvent e)
        {
            this._logger.Verbose(e.Information);
        }
    }
}