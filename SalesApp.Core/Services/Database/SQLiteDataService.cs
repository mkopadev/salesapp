using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SalesApp.Core.Api;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Controllers.Synchronization;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.BL.Models.Syncing;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database.Querying;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Interfaces;
using SQLite.Net;
using SQLite.Net.Attributes;

namespace SalesApp.Core.Services.Database
{
    public abstract class SQLiteDataService<T> : IDataService<T> where T : BusinessEntityBase, new()
    {
        private QueryGenerator _queryGen;

        protected ILog Logger
        {
            get
            {
                return Resolver.Instance.Get<ILog>();
            }
        }

        private static Settings.Settings _LegacySettingsManager;

        /*[Obsolete("Dependancy injection for legacy settings has been implemented and below parameters do not need to be explicitly passed. Use parameterless constructor")]
        private SQLiteDataService(LanguagesEnum lang = default(LanguagesEnum), CountryCodes country = default(CountryCodes))
        {
        }*/

        /// <summary>
        /// Saves a model
        /// </summary>
        /// <typeparam name="T">The type of model to be saved</typeparam>
        /// <param name="model">The model</param>
        /// <returns>The saved model</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if call doesn't complete within specified time duration</exception>
        public virtual async Task<SaveResponse<T>> SaveAsync(T model)
        {
            try
            {
                SaveResponse<T> result = default(SaveResponse<T>);
                await DataAccess.Instance.Connection.RunInTransactionAsync(
                    async connTran =>
                        {
                            DataAccess.Instance.StartTransaction(connTran);
                            result = await this.SaveAsync(connTran, model);
                        });

                DataAccess.Instance.CommitTransaction();
                this.Logger.Debug("Code after transaction call");
                return result;
            }
            catch (Exception exception)
            {
                this.Logger.Debug(exception);
                throw;
            }
        }

        private async Task<SaveResponse<T>> OverwriteAsync(T model, CriteriaBuilder criteriaBuilder)
        {
            try
            {
                this.Logger.Debug("Overwriting items on table " + model.TableName);
                List<T> previousItems = await this.GetManyByCriteria(criteriaBuilder);

                if (previousItems == null)
                {
                    previousItems = new List<T>();
                }

                this.Logger.Debug("Found ~ previous items to be overwritten".GetFormated(previousItems.Count));

                await DataAccess.Instance.Connection.RunInTransactionAsync(
                    async tran =>
                        {
                            DataAccess.Instance.StartTransaction(tran);
                            foreach (var item in previousItems)
                            {
                                this.Logger.Debug("Deleting previous item");
                                await this.DeleteAsync(tran, item.Id);
                            }
                        });

                DataAccess.Instance.CommitTransaction();
                SaveResponse<T> response = await this.SaveAsync(model);

                this.Logger.Debug("Exiting overwrite");
                return response;
            }
            catch (Exception e)
            {
                this.Logger.Error(e);
                throw;
            }
        }

        protected async Task<SaveResponse<T>> OverwriteAsync(T model, string field)
        {
            try
            {
                PropertyInfo propertyInfo = model.GetType().GetRuntimeProperty(field);
                if (propertyInfo == null)
                {
                    throw new Exception("Unknown field '" + field + "' specified in Overwrite property method of '" + this.GetType().FullName + "' or a base class in its inheritance hierarchy.");
                }

                if (!propertyInfo.CanRead)
                {
                    throw new Exception("Property '~' specified in Overwrite method of class '~' or one of its base classes is write-only, cannot generate filter based on it.".GetFormated(field, this.GetType()));
                }

                object value = propertyInfo.GetValue(model);

                CriteriaBuilder criteriaBuilder = new CriteriaBuilder();

                var result = await this.OverwriteAsync(model, criteriaBuilder.Add(field, value));
                this.Logger.Debug("Returning id " + result.SavedModel.Id);

                return result;
            }
            catch (Exception e)
            {
                this.Logger.Error(e);
                throw;
            }
        }

        /// <summary>
        /// Saves a model
        /// </summary>
        /// <param name="connTran">Transaction object</param>
        /// <param name="model">The model</param>
        /// <returns>The saved model</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if call doesn't complete within specified time duration</exception>
        /// <exception cref="NotSavedException">Throws a NotSavedException if saving fails for any reason</exception>
        /// <remarks>This method is called by the SaveAsync(T model) overload. Calling it directly is unsafe as save data and sync 
        /// information will not be wrapped in a transaction which may introduce inconsistencies in data</remarks>
        public virtual async Task<SaveResponse<T>> SaveAsync(SQLiteConnection connTran, T model)
        {
            if (connTran == null)
            {
                return null;
            }

            await DataAccess.Instance.SaveAsync(model);

            var obj = model as SynchronizableModelBase;
            if (obj != null && typeof(T) != typeof(SyncRecord))
            {
                SyncingController syncingController = new SyncingController();
                return new SaveResponse<T>(model, await syncingController.SynchronizeOrQueue(connTran, obj));
            }

            return new SaveResponse<T>(model, default(PostResponse));
        }

        public virtual async Task<T> GetSingleByCriteria(CriteriaBuilder criteriaBuilder)
        {
            List<T> result = await this.GetManyByCriteria(criteriaBuilder);
            if (result == null || result.Count == 0)
            {
                return new T();
            }

            return result.SingleOrDefault() ?? new T();
        }

        public virtual async Task<T> GetSingleRecord(CriteriaBuilder criteriaBuilder)
        {
            List<T> result = await this.GetManyByCriteria(criteriaBuilder);
            if (result == null || result.Count == 0)
            {
                return new T();
            }

            this.Logger.Debug("Cache size " + result.Count);
            return result.ElementAt(0) ?? new T();
        }

        public virtual async Task<List<T>> GetManyByCriteria(CriteriaBuilder criteriaBuilder, [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            List<T> result = new List<T>();
            string query = this.QueryBuilder(criteriaBuilder); // this.QueryGen.GetQuery(string.Format("Select * From {0} ", this.TableName), criteriaBuilder.Criteria, this.TableName));
            try
            {
                await DataAccess.Instance.Connection.RunInTransactionAsync(
                    async connTran =>
                    {
                        DataAccess.Instance.StartTransaction(connTran);
                        result = await DataAccess.Instance.SelectQueryAsync<T>(query);
                    });

                DataAccess.Instance.CommitTransaction();
                return result;
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex);
                throw;
            }
        }

        public string QueryBuilder(CriteriaBuilder criteriaBuilder)
        {
            string query = this.QueryGen.GetQuery(string.Format("Select * From {0} ", this.TableName), criteriaBuilder.Criteria, this.TableName);
            return query;
        }

        /// <summary>
        /// Returns a list of all entities of the specified type from the database
        /// </summary>
        /// <typeparam name="T">The model type</typeparam>
        /// <returns>List of entities that exist in the database</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if call doesn't complete within specified time duration</exception> 
        public virtual async Task<List<T>> GetAllAsync()
        {
            try
            {
                List<T> result = null;
                await DataAccess.Instance.Connection.RunInTransactionAsync(
                   async connTran =>
                   {
                       DataAccess.Instance.StartTransaction(connTran);
                       result = await DataAccess.Instance.GetAllAsync<T>();
                   });
                DataAccess.Instance.CommitTransaction();

                return result;
            }
            catch (Exception ex)
            {
                this.Logger.Debug(ex);
                throw;
            }
        }

        public virtual List<T> GetAll()
        {
            return DataAccess.Instance.Connection.Table<T>().ToListAsync().Result;
        }

        /// <summary>
        /// Returns a single entity whose id is matches the one passed into the method
        /// </summary>
        /// <param name="id">The id to search for</param>
        /// <returns>A object that matches the id or null if none was found</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if call doesn't complete within specified time duration</exception> 
        public async Task<T> GetByIdAsync(Guid id)
        {
            try
            {
                T obj = null;
                await DataAccess.Instance.Connection.RunInTransactionAsync(
                   async connTran =>
                   {
                       DataAccess.Instance.StartTransaction(connTran);
                       CriteriaBuilder criteria = new CriteriaBuilder();
                       criteria.Add("Id", id);
                       obj = await DataAccess.Instance.GetSingle<T>(criteria);
                   });
                DataAccess.Instance.CommitTransaction();

                return obj;
            }
            catch (Exception ex)
            {
                this.Logger.Debug(ex);
                return null;
            }
        }

        /// <summary>
        /// Deletes the entity whose id matches the provided id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>True if an entity was delete and false if none was.</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if call doesn't complete within specified time duration</exception> 
        public async Task<int> DeleteAsync(Guid id)
        {
            int result = 0;
            await DataAccess.Instance.Connection.RunInTransactionAsync(
                    async (tran) =>
                    {
                        DataAccess.Instance.StartTransaction(tran);
                        result = await DataAccess.Instance.DeleteAsync<T>(id);
                    });

            DataAccess.Instance.CommitTransaction();
            return result;
        }

        public async Task<int> DeleteAsync(SQLiteConnection tran, Guid id)
        {
            int rows = await DataAccess.Instance.DeleteAsync<T>(id);
            return rows;
        }

        /// <summary>
        /// Deletes the entity passed
        /// </summary>
        /// <param name="model">The entity to delete</param>
        /// <returns>True if an entity was delete and false if none was.</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if call doesn't complete within specified time duration</exception> 
        public async Task<int> DeleteAsync(T model)
        {
            return await this.DeleteAsync(model.Id);
        }

        public async Task<T> SelectSingleOrNothing(Criterion[] fieldsAndValues)
        {
            List<T> list = await this.SelectQueryAsync(fieldsAndValues);

            if (list == null)
            {
                this.Logger.Debug("In method SingleOrNothing. The result was null");
                return default(T);
            }

            // if the list contains less or more then 1 item, return default
            if (list.Count != 1)
            {
                return default(T);
            }

            return list[0];
        }

        /// <summary>
        /// Takes in an array with fields and values to search for and returns a list of matching items or null if none match specified criteria
        /// </summary>
        /// <param name="fieldsAndValues">Array of fields and values to search for</param>
        /// <returns>List of results matching criteria or null if none are found</returns>
        /// <exception cref="TimeoutException">Throws a TimeoutException if call doesn't complete within specified time duration</exception> 
        public async Task<List<T>> SelectQueryAsync(Criterion[] fieldsAndValues)
        {
            const string queryPrefix = "Select * From {0} Where ";
            string query = queryPrefix;
            foreach (var criterion in fieldsAndValues)
            {
                if (query != queryPrefix)
                {
                    query += string.Format(" {0} ", criterion.Conjunction);
                }
                else
                {
                    query = string.Format(queryPrefix, TableName);
                }

                query += string.Format("{0} = '{1}'", criterion.Field, criterion.Value);
            }

            return await this.SelectQueryAsync(query);
        }

        public async Task<List<T>> SelectQueryAsync(string query)
        {
            try
            {
                List<T> list = await DataAccess.Instance.SelectQueryAsync<T>(query);
                return list;
            }
            catch (Exception exception)
            {
                this.Logger.Verbose(exception);
                return default(List<T>);
            }
        }

        public async Task<int> DeleteWithCriteriaAsync(Criterion[] fieldsAndValues)
        {
            int result = 0;
            await DataAccess.Instance.Connection.RunInTransactionAsync(
                    async (connTran) =>
                    {
                        DataAccess.Instance.StartTransaction(connTran);
                        result = await this.DeleteWithCriteriaAsync(connTran, fieldsAndValues);
                    });

            DataAccess.Instance.CommitTransaction();

            return result;
        }

        public async Task<int> DeleteWithCriteriaAsync(SQLiteConnection tran, Criterion[] fieldsAndValues)
        {
            try
            {
                string query = this.QueryGen.GetQuery(
                        string.Format("Delete From {0} ", this.TableName),
                        fieldsAndValues, this.TableName);

                this.Logger.Debug("DeleteWithCriteriaAsync: query is '~'".GetFormated(query));

                int rows = await DataAccess.Instance.RunQueryAsync(query);

                return rows;
            }
            catch (Exception e)
            {
                this.Logger.Error(e);
                throw;
            }
        }

        private string _tableName = string.Empty;

        [Ignore]
        private string TableName
        {
            get
            {
                if (string.IsNullOrEmpty(_tableName))
                {
                    _tableName = Activator.CreateInstance<T>().TableName;
                }

                return _tableName;
            }
        }

        protected static Settings.Settings LegacySettingsManager
        {
            get
            {
                _LegacySettingsManager = _LegacySettingsManager ?? Settings.Settings.Instance;
                return _LegacySettingsManager;
            }
        }

        private QueryGenerator QueryGen
        {
            get
            {
                if (_queryGen == null)
                {
                    _queryGen = new QueryGenerator();
                }

                return _queryGen;
            }
        }

        /// <summary>
        /// Method for saving a list of items
        /// </summary>
        /// <param name="models">The list of models to be saved</param>
        /// <returns>An empty task</returns>
        public async Task SaveBulkAsync(List<T> models)
        {
            CustomerProductController controller = new CustomerProductController();

            foreach (var model in models)
            {
                await this.SaveAsync(model);
                Customer customer = model as Customer;
                if (customer == null)
                {
                    continue;
                }

                SequentialGuid.GuidGen++;
                CustomerProduct cp = new CustomerProduct
                {
                    Id = SequentialGuid.GuidGen.CurrentGuid,
                    CustomerId = customer.Id,
                    DisplayName = customer.Product.DisplayName,
                    ProductTypeId = customer.Product.ProductTypeId,
                    DateAcquired = customer.Product.DateAcquired,
                    SerialNumber = customer.Product.SerialNumber
                };

                await controller.SaveCustomerProductToDevice(cp);
            }
        }

        public async Task SaveBulkInASingleTransaction(List<T> permissions)
        {
            await DataAccess.Instance.Connection.RunInTransactionAsync(
                   async (connTran) =>
                   {
                       foreach (T p in permissions)
                       {
                           await this.SaveAsync(connTran, p);
                       }
                   });
        }
    }
}
