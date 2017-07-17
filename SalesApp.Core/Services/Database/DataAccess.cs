using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Core.Services.Database.Locking;
using SalesApp.Core.Services.Database.Logging;
using SalesApp.Core.Services.Database.Models;
using SalesApp.Core.Services.Database.Querying;
using SQLite.Net;
using SQLite.Net.Async;

namespace SalesApp.Core.Services.Database
{
     /// <summary>
    /// This class contains methods to allow database CRUD operations
    /// </summary>
    public class DataAccess
     {

         public event EventHandler<LogEvent> EventOccured;
         private Logger _logger;

        private SQLiteConnection _transactionConnection;

        private DataAccess()
        {
            Connection = DbConnection.Instance.Connection;
        }

         private static DataAccess _instance;

         public static DataAccess Instance
         {
             get
             {
                 _instance = _instance ?? new DataAccess();
                return _instance;
             }
         }

         internal Logger Logger
         {
             get
             {
                 if (_logger == null)
                 {
                     _logger = new Logger();
                     _logger.EventOccured += this.FireEventOccured;
                 }
                return _logger;
             }
         }

        private void FireEventOccured(object sender, LogEvent logEvent)
        {
            if (this.EventOccured == null)
            {
                return;
            }

            this.EventOccured.Invoke(sender, logEvent);
        }

        public async Task<int> RunQueryAsync(string query, [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0, [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            return await Run
                (
                    () =>
                    {
                        if (_transactionConnection != null)
                        {
                            return _transactionConnection.Execute(query);
                        }
                        return -1;
                    }
                    , async () => await DbConnection.Instance.Connection.ExecuteAsync(query)
                    , sourceLineNumber
                    , sourceFilePath
                );
        }

         public async Task<TModel> SaveAsync<TModel>(TModel model,
             [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0,
             [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") where TModel : ModelBase
         {
                 try
                 {
                    model.Modified = DateTime.Now;
                    if (model.Id == default(Guid))
                     {
                         model.Id = Guid.NewGuid();
                         model.Created = DateTime.Now;
                    return await Run(() =>
                                 {
                                     try
                                     {
                                         _transactionConnection.Insert(model);
                                     }
                                     catch (Exception)
                                     {
                                         _transactionConnection.Rollback();
                                         throw;
                                     }
                                     return model;
                                 },
                                 async () =>
                                 {
                                     try
                                     {
                                         await DbConnection.Instance.Connection.InsertAsync(model);
                                     }
                                     catch (Exception e)
                                     {
                                         Logger.Log("Exception "+e.Message, true);
                                         throw e;
                                     }
                                     return model;
                                 },
                                 sourceLineNumber,
                                 sourceFilePath
                             );
                     }

                     return await Run(() =>
                             {
                                 var affectedRows = _transactionConnection.Update(model);

                                 // Takes care of records that have prmary id but the are not saved on the device
                                 // Especially when doing downsync
                                 if (affectedRows == 0)
                                 {
                                     _transactionConnection.Insert(model);
                                 }

                                 return model;
                             }, 
                             async () =>
                             {
                                 var affectedRows =  await DbConnection.Instance.Connection.UpdateAsync(model);
                                 // Takes care of records that have prmary id but the are not saved on the device
                                 // Especially when doing downsync
                                 if (affectedRows == 0)
                                 {
                                     await DbConnection.Instance.Connection.InsertAsync(model);
                                 }

                                 return model;
                             }, 
                             sourceLineNumber,
                             sourceFilePath);

             }catch(Exception)
            {
                _transactionConnection.Rollback();
                return default(TModel);
            }
        }

         public async Task<List<TModel>> GetAllAsync<TModel>([System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0, [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") where TModel : ModelBase , new()
         {
            string query = $"Select * From {new TModel().TableName}";
             return await Run(() => _transactionConnection.Query<TModel>(query),
                 async () => await DbConnection.Instance.Connection.QueryAsync<TModel>(query),
                 sourceLineNumber,
                 sourceFilePath);
         }

         public async Task<int> DeleteAsync<TModel>(Guid id,
             [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0,
             [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") where TModel : ModelBase
         {

             return await Run(() =>
                     {
                         int rows = 0;
                         var deleted =  _transactionConnection?.Delete<TModel>(id);
                         if (deleted != null)
                         {
                             return (int) deleted;
                         }
                         return rows;
                     },
                     async () => await DbConnection.Instance.Connection.DeleteAsync<TModel>(id), 
                     sourceLineNumber, 
                     sourceFilePath);
         }

        public async Task<int> DeleteAllAsync<TModel>([System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") where TModel : ModelBase
        {

            return await Run(
                    () =>
                    {
                        int rows = 0;
                        var deleteAll = _transactionConnection?.DeleteAll<TModel>();
                        if (deleteAll != null)
                            rows = (int) deleteAll;
                        return rows;
                    }, 
                    async () => await DbConnection.Instance.Connection.DeleteAllAsync<TModel>(), 
                    sourceLineNumber, 
                    sourceFilePath
                );
        }
        /// <summary>
        /// Queries the database based on specified criteria and returns the single record matching said criteria.
        /// If multiple records match the criteria then an exception is thrown
        /// </summary>
        /// <typeparam name="TModel">The type of the record to be returned</typeparam>
        /// <param name="criteriaBuilder">A criteria builder</param>
        /// <returns>The Record that may match the criteria or null if no match found</returns>
        public async Task<TModel> GetSingle<TModel>(CriteriaBuilder criteriaBuilder, [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0, [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") where TModel : ModelBase, new()
         {

             List<TModel> result = await GetMany<TModel>(criteriaBuilder,sourceLineNumber,sourceFilePath);
             if (result == null || result.Count == 0)
             {
                 return null;
             }
             switch (result.Count)
             {
                 case 1:
                     return result[0];
                 default:
                     throw new Exception("Multiple records returned while only one was expected");

             }
         }

        public async Task<TModel> GetSingle<TModel>(Guid id, [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0, [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") where TModel : ModelBase, new()
        {
            return await Run(
                   () =>
                   {
                       var obj = _transactionConnection?.Find<TModel>(id);
                       
                       return obj;
                   },
                   async () => await DbConnection.Instance.Connection.FindAsync<TModel>(id),
                   sourceLineNumber,
                   sourceFilePath
               );
        }


        public void StartTransaction(SQLiteConnection transactionConnection)
         {
             _transactionConnection = transactionConnection;
         }

         public void CommitTransaction()
         {
             if (_transactionConnection != null)
             {
                _transactionConnection.Commit();
                _transactionConnection = null;
            }
         }
         

         private async Task<TResult> Run<TResult>(Func<TResult> transactedMethod,
             Func<Task<TResult>> fallbackMethodAsync, int sourceLineNumber, string sourceFilePath)
         {
             try
             {
                 if (_transactionConnection != null && transactedMethod != null)
                 {
                     SemaphoreProvider.DbSemaphore.Wait(sourceLineNumber, sourceFilePath);
                     return transactedMethod();
                    
                 }
                 else
                 {
                     try
                     {

                        await SemaphoreProvider.DbSemaphore.WaitAsync(sourceLineNumber, sourceFilePath);
                        return await fallbackMethodAsync();
                    }
                     catch (Exception e)
                     {
                         throw e;
                     }
                 }
             }
             catch (Exception ex)
             {
               Logger.Log($"Database Error: line {sourceLineNumber}, file {sourceFilePath}",true);
               Logger.Log($"{ex.Message}",true);
                 throw;
             }
             finally
             {
                SemaphoreProvider.DbSemaphore.Release(sourceLineNumber, sourceFilePath);
             }
             
         }

         /// <summary>
         /// Queries the database based on specified criteria and returns records matching said criteria
         /// </summary>
         /// <typeparam name="TModel">The type of records to be returned</typeparam>
         /// <param name="criteriaBuilder">A criteria builder</param>
         /// <returns>Records that may match the criteria or null if no matches found</returns>
         public async Task<List<TModel>> GetMany<TModel>(CriteriaBuilder criteriaBuilder,
             [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0,
             [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
             where TModel : ModelBase, new()
         {

             string query = new QueryGenerator()
                 .GetQuery
                 (
                     "Select * From {0}"
                     , criteriaBuilder.Criteria
                     , new TModel().TableName
                 );

             return await Run
                 (
                     () => _transactionConnection?.Query<TModel>(query)
                     , async () => await DbConnection.Instance.Connection.QueryAsync<TModel>(query)
                     , sourceLineNumber
                     , sourceFilePath
                 );
         }

         public async Task<List<TModel>> SelectQueryAsync<TModel>(string query,
             [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0,
             [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "") where TModel : ModelBase, new()
        {
            return await Run
               (
                   () => _transactionConnection?.Query<TModel>(query)
                   , async () => await DbConnection.Instance.Connection.QueryAsync<TModel>(query)
                   , sourceLineNumber
                   , sourceFilePath
               );
        }

         public SQLiteAsyncConnection Connection { get; set; }

        
        public async Task RunInTransactionAsync(Action action)
        {
            if (_transactionConnection != null)
            {
                throw new Exception("Queued or nested transactions currently not supported");
            }
            try
            {
                await DbConnection.Instance.Connection.RunInTransactionAsync
                (
                    connTran =>
                    {
                        _transactionConnection = connTran;
                        action();
                    }
                );
            }
            finally
            {
                _transactionConnection = null;
            }

        }
    }
}
