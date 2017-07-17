using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database.Models;

namespace SalesApp.Core.Services.Database
{
    public class QueryRunner
    {
        private ILog Logger = LogManager.Get(typeof(QueryRunner));

        public QueryRunner()
        {
        }

        public async Task<List<T>> RunQuery<T>(string query) where T : ModelBase, new()
        {
            try
            {
                List<T> list = null;
                await DataAccess.Instance.Connection.RunInTransactionAsync(
                  async tran =>
                  {
                      DataAccess.Instance.StartTransaction(tran);
                      list = await DataAccess.Instance.SelectQueryAsync<T>(query);
                  });

                DataAccess.Instance.CommitTransaction();

                if (list.Count < 1)
                {
                    return new List<T>();
                }
                else
                {
                    return list;
                }
            }
            catch (Exception e)
            {
                Logger.Debug(e);
                return new List<T>();
            }
        }
    }
}