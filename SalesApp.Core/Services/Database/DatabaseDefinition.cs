using System;
using System.Threading.Tasks;
using SalesApp.Core.Enums.Database;
using SalesApp.Core.Services.Database.Logging;
using SalesApp.Core.Services.Database.Models;
using SalesApp.Core.Services.Database.Querying;
using SQLite.Net.Interop;

namespace SalesApp.Core.Services.Database
{
    /// <summary>
    /// Contains methods to perform database creation and alteration
    /// </summary>
    public class DatabaseDefinition 
    {
        
        /// <summary>
        /// Returns the current database's newVersion number
        /// </summary>
        public async Task<int> GetVersion()
        {
            try
            {
                if (DbConnection.Instance.Configured == false)
                {
                    return 0;
                }
                CriteriaBuilder cb = new CriteriaBuilder();
                cb.Add("Version", 0, ConjunctionsEnum.And, Operators.GreaterThan);
                DatabaseVersion dbVersion = await DataAccess.Instance.GetSingle<DatabaseVersion>
                    (
                       cb 
                    );

                if (dbVersion == null)
                {
                    return 0;
                }
                else
                {
                    return dbVersion.Version;
                }
            }
            catch (Exception erException)
            {
                if (erException.Message.ToLower().Contains("no such table"))
                {
                    return 0;
                }
                throw;
            }
        }
        
        /// <summary>
        /// Using scripts provided by the platform the app is running on, this creates or updates a database.
        /// </summary>
        /// <returns>The database newVersion</returns>
        public async Task<int> InitializeDatabase(Func<int,string> scriptGetter ,string fullPath, ISQLitePlatform platform, bool useLibDates = false)
        {
            DbConnection.Instance.Configure(fullPath, platform);
            await DataAccess.Instance.RunQueryAsync
                    (

                        @"CREATE TABLE IF NOT EXISTS DatabaseVersion(Id Varchar(36) PRIMARY KEY, Version Integer"
                        + ", Created DateTime not null, Modified DateTime not null);"

                    );

            int currentVersion = await GetVersion();
            int nextVersion = currentVersion + 1;
            string query = scriptGetter(nextVersion);

            while (!string.IsNullOrEmpty(query))
            {
                string[] queries = query.Split(new[] {';'});
                foreach (var sql in queries)
                {
                    if (!string.IsNullOrEmpty(sql))
                    {
                        await DataAccess.Instance.RunQueryAsync(sql + ";");
                    }
                }
                nextVersion++;
                query = scriptGetter(nextVersion);
            }
            nextVersion--;
            if (nextVersion > currentVersion)
            {
                await SetDatabaseVersionAsync(nextVersion);
            }

            return nextVersion;
        }

        private void _logger_EventOccured(object sender, LogEvent e)
        {
            
        }

        private async Task<bool> SetDatabaseVersionAsync(int newVersion)
        {
            if (newVersion < 1)
            {
                return true;
            }
            
            var dbList = await DataAccess.Instance.GetAllAsync<DatabaseVersion>();
            DatabaseVersion dbVersion = null;
            if(dbList != null)
            {
                switch (dbList.Count)
                {
                    case 1:
                        dbVersion = dbList?[0];
                        break;
                    default:
                        foreach (var databaseVersion in dbList)
                        {
                            await DataAccess.Instance.DeleteAsync<DatabaseVersion>(databaseVersion.Id);
                        }
                        break;

                }
            }

            dbVersion = dbVersion ?? new DatabaseVersion();
            dbVersion.Version = newVersion;
            await DataAccess.Instance.SaveAsync(dbVersion);
            return true;
        }

        


    }
}
