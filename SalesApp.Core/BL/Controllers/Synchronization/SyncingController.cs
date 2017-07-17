using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.Api;
using SalesApp.Core.Api.DownSync;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Controllers.TicketList;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.BL.Models.Syncing;
using SalesApp.Core.BL.Models.TicketList;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.MultiCountry;
using SalesApp.Core.Enums.Syncing;
using SalesApp.Core.Events.DownSync;
using SalesApp.Core.Exceptions.Syncing;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.DownSync;
using SQLite.Net;

namespace SalesApp.Core.BL.Controllers.Synchronization
{
    /// <summary>
    /// A class for helping perform both down and up syncing
    /// </summary>
    public class SyncingController : SQLiteDataService<SyncRecord>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SyncingController"/> class.
        /// </summary>
        /// <param name="lang">The language to use</param>
        /// <param name="country">The country to use</param>
        public SyncingController(LanguagesEnum lang, CountryCodes country)
        {
            this._lang = lang;
            this._country = country;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncingController"/> class.
        /// Default constructor
        /// </summary>
        public SyncingController() : this(LegacySettingsManager.DsrLanguage, LegacySettingsManager.DsrCountryCode)
        {
        }

        /// <summary>
        /// Gets the down sync service
        /// </summary>
        public LocalDownSyncService DownSyncService
        {
            get
            {
                return new LocalDownSyncService();
            }
        }

        /// <summary>
        /// Gets or sets the handler to be notified of a sync progress
        /// </summary>
        public event EventHandler<SyncStatusEventArgs> SyncStatusHandler
        {
            add
            {
                if (this.SyncStatus == null || !this.SyncStatus.GetInvocationList().Contains(value))
                {
                    this.SyncStatus += value;
                }
            }

            remove
            {
                this.SyncStatus -= value;
            }
        }

        /// <summary>
        /// Gets or sets the handler to be notified of a sync has just started
        /// </summary>
        public event EventHandler<SyncStartedEventArgs> SyncStartedHandler
        {
            add
            {
                if (this.SyncStarted == null || !this.SyncStarted.GetInvocationList().Contains(value))
                {
                    this.SyncStarted += value;
                }
            }

            remove
            {
                this.SyncStarted -= value;
            }
        }

        /// <summary>
        /// Gets or sets the handler to be notified of a sync has just finished
        /// </summary>
        public event EventHandler<SyncCompleteEventArgs> SyncCompleteHandler
        {
            add
            {
                if (this.SyncCompleted == null || !this.SyncCompleted.GetInvocationList().Contains(value))
                {
                    this.SyncCompleted += value;
                }
            }

            remove
            {
                this.SyncCompleted -= value;
            }
        }

        /// <summary>
        /// Gets or sets the handler to be notified of a sync error
        /// </summary>
        public event EventHandler<SyncErrorEventArgs> SyncErroredHandler
        {
            add
            {
                if (this.SyncErrored == null || !this.SyncErrored.GetInvocationList().Contains(value))
                {
                    this.SyncErrored += value;
                }
            }

            remove
            {
                this.SyncErrored -= value;
            }
        }

        /// <summary>
        /// Event to be invoked when sync is ongoing to report its progress status
        /// </summary>
        private event EventHandler<SyncStatusEventArgs> SyncStatus;

        /// <summary>
        /// Event to be invoked when a sync has just started
        /// </summary>
        private event EventHandler<SyncStartedEventArgs> SyncStarted;

        /// <summary>
        /// Event to be invoked when a sync completes successfully
        /// </summary>
        private event EventHandler<SyncCompleteEventArgs> SyncCompleted;

        /// <summary>
        /// event to be invoked when there was an error during sync
        /// </summary>
        private event EventHandler<SyncErrorEventArgs> SyncErrored;

        /// <summary>
        /// Up sync completed event
        /// </summary>
        private event EventHandler<EventArgs> SyncAttemptCompleted;

        /// <summary>
        /// Prospects table name
        /// </summary>
        private const string TableProspects = "Prospect";

        /// <summary>
        /// Customers table name
        /// </summary>
        private const string TableCustomer = "Customer";

        /// <summary>
        /// The language to use with the API
        /// </summary>
        private readonly LanguagesEnum _lang;

        /// <summary>
        /// The country code to use with the API
        /// </summary>
        private readonly CountryCodes _country;


        /// <summary>
        /// This is a generic down sync method for syncing both prospects and customers
        /// </summary>
        /// <typeparam name="TResponse">Specifies the type of sync to perform</typeparam>
        /// <typeparam name = "TModel" > Specifies the type of model for which to do the sync</typeparam>
        /// <returns>A server response representing new and updated methods</returns>
        public async Task SyncDownAsync<TResponse, TModel>() where TModel : BusinessEntityBase
        {
            Type t = typeof(TModel);

            var api = this.GetDownSyncApi(t);

            if (api == null)
            {
                var error = string.Format("We dont know how to down sync type {0} yet!", typeof(TModel));
                throw new InvalidOperationException(error);
            }

            try
            {
                if (!Resolver.Instance.Get<IConnectivityService>().HasConnection())
                {
                    this.Logger.Debug("No connectivity");
                    return;
                }

                string userId = Resolver.Instance.Get<ISalesAppSession>().UserId.ToString();

                string serverTimestamp = string.Empty;
                DownSyncTracker tracker = await this.DownSyncService.Get(t.FullName);

                if (tracker != default(DownSyncTracker))
                {
                    serverTimestamp = tracker.ServerTimestamp;
                }

                DateTime result;
                DateTime.TryParse(serverTimestamp, out result);

                if (result != default(DateTime))
                {
                    serverTimestamp = result.ToString("s");
                }

                string urlParams = string.Format("?servertimestamp={0}&userId={1}", serverTimestamp, userId);

                if (t == typeof(DsrTicket))
                {
                    urlParams += "&requestType=Dsr";
                }

                if (t == typeof(CustomerTicket))
                {
                    urlParams += "&requestType=Customer";
                }

                if (this.SyncStarted != null)
                {
                    this.SyncStarted.Invoke(this, new SyncStartedEventArgs());
                }

                ServerResponse<TResponse> serverResponse = await api.MakeGetCallAsync<TResponse>(urlParams);

                var downSyncServerResponse = serverResponse.GetObject() as DownSyncServerResponse<TModel>;
                Logger.Debug("Down sync response = " + serverResponse.RawResponse);
                await this.HandleDownSyncResponse(downSyncServerResponse);

                if (downSyncServerResponse != null)
                {
                    await this.DownSyncService.Save(new DownSyncTracker
                    {
                        ServerTimestamp = downSyncServerResponse.ServerTimeStamp == null ? TimeService.Get().Now.ToString() : downSyncServerResponse.ServerTimeStamp,
                        Entity = t.FullName,
                        IsInitial = true
                    });
                }

                if (this.SyncCompleted != null)
                {
                    this.SyncCompleted.Invoke(this, new SyncCompleteEventArgs());
                }
            }
            catch (Exception ex)
            {
                if (this.SyncErrored != null)
                {
                    this.SyncErrored.Invoke(this, new SyncErrorEventArgs { Error = ex });
                }

                this.Logger.Error(ex);
                throw;
            }
        }

        public async Task<PostResponse> SyncUpAsync(object synchronizableModel, ApiBase apiBaseInstance)
        {
            try
            {
                if (!Resolver.Instance.Get<IConnectivityService>().HasConnection())
                {
                    this.Logger.Debug("No connectivity");
                    return null;
                }

                string syncableJson = JsonConvert.SerializeObject(synchronizableModel);
                if (syncableJson == string.Empty)
                {
                    this.Logger.Debug("Unsyncable model");
                    return default(PostResponse);
                }

                ServerResponse<PostResponse> response =
                    await apiBaseInstance.PostJsonAsync<PostResponse>(syncableJson);

                return response.GetObject();
            }
            catch (Exception exception)
            {
                this.Logger.Error(exception);
                throw;
            }
        }

        public async Task<PostResponse> SynchronizeOrQueue(SQLiteConnection connTran, object synchronizableModel)
        {
            return await this.Synchronize(connTran, synchronizableModel);
        }

        private async Task<PostResponse> Synchronize(SQLiteConnection connTran, object synchronizableModel)
        {
            SynchronizableModelBase syncModel = synchronizableModel as SynchronizableModelBase;
            if (syncModel == null)
            {
                throw new NullReferenceException("Unable to cast passed model into a syncable model.");
            }

            ApiBase apiBaseInstance = new SyncRegistry().GetApiBaseInstance(syncModel.TableName);
            if (apiBaseInstance == null)
            {
                Exception ex = new ApiControllerNotFoundException(syncModel.TableName);
                this.Logger.Error(ex);
                throw ex;
            }

            SyncRecord syncRec = await this.GetSyncRecordAsync(syncModel.RequestId);
            if (syncModel.DontSync)
            {
                if (syncRec == null)
                {
                    syncRec = new SyncRecord
                    {
                        ModelId = syncModel.Id,
                        ModelType = syncModel.TableName,
                        RequestId = syncModel.RequestId,
                        Status = syncModel.Channel == DataChannel.Fallback ? RecordStatus.FallbackSent : RecordStatus.Synced
                    };

                    this.Logger.Debug("Sync Status " + syncRec.Status);
                    await this.SaveAsync(connTran, syncRec);
                }

                return new PostResponse
                {
                    RequestId = syncModel.RequestId,
                    Successful = true,
                    ResponseText = string.Empty
                };
            }

            PostResponse postResponse = await this.SyncUpAsync(synchronizableModel, apiBaseInstance);

            if (postResponse != null && postResponse.Successful)
            {
                this.Logger.Debug("Synced successfully");
                return postResponse;
            }

            if (postResponse != null)
            {
                this.Logger.Debug("Synced successfully but the server response had an error");
                postResponse = await apiBaseInstance.AfterSyncUpProcessing(synchronizableModel, postResponse);
                if (postResponse.Successful)
                {
                    return postResponse;
                }
            }

            if (connTran == null)
            {
                return postResponse;
            }

            this.Logger.Debug("Didn't sync so we try and queue");
            syncRec = new SyncRecord
            {
                ModelType = syncModel.TableName,
                RequestId = syncModel.RequestId,
                ModelId = syncModel.Id,
                Status = RecordStatus.Pending
            };

            bool queued = this.QueueRecordForSyncing(connTran, syncRec);

            if (queued)
            {
                if (postResponse != null)
                {
                    return postResponse;
                }

                Guid requestId = syncModel.RequestId;

                return new PostResponse
                {
                    RequestId = requestId,
                    Successful = false,
                    ResponseText = string.Empty
                };
            }

            throw new NotQueuedException(syncModel.TableName);
        }

        /// <summary>
        /// Searches for a record in the indexing table
        /// </summary>
        /// <param name="requestId">The unique request id of the record</param>
        /// <returns>The record if found or null if the record does not exist in the indexing table</returns>
        public async Task<SyncRecord> GetSyncRecordAsync(Guid requestId)
        {
            string sql = string.Format("SELECT * FROM SyncRecord WHERE RequestId ='{0}' ORDER BY Id DESC LIMIT 1", requestId);
            List<SyncRecord> syncRecords = await this.SelectQueryAsync(sql);
            if (syncRecords != null && syncRecords.Count > 0)
            {
                return syncRecords[0];
            }

            return null;
        }

        private bool QueueRecordForSyncing(SQLiteConnection connTran, SyncRecord syncableRecord)
        {
            return this.SaveAsync(connTran, syncableRecord).Result.SavedModel.Id != default(Guid);
        }

        public async Task<List<Guid>> PushAllAsync(string modelType)
        {
            string sql = "SELECT * FROM SyncRecord WHERE Status IN(" + (int)RecordStatus.Pending + "," + (int)RecordStatus.InError + ") AND ModelType ='" + modelType + "'";
            List<SyncRecord> syncableRecords = await this.SelectQueryAsync(sql);

            // Push all of them upwards
            List<Guid> successes = new List<Guid>();
            foreach (var syncRec in syncableRecords)
            {
                Guid result = await this.PushSingleAsync(syncRec);
                if (result != default(Guid))
                {
                    successes.Add(result);
                }
            }

            return successes;
        }

        public async Task<Guid> PushSingleAsync(SyncRecord syncRec)
        {
            if (syncRec == null)
            {
                return default(Guid);
            }

            if (syncRec.Status == RecordStatus.Synced)
            {
                return syncRec.RequestId;
            }

            object syncronizableModelBase = await GetSynchronizableModelBase(syncRec.ModelType,
                syncRec.RequestId);

            if (syncronizableModelBase == null)
            {
                "The syncronizableModelBase is null".WriteLine();
                return default(Guid);
            }

            PostResponse response = await this.PushObject(syncronizableModelBase);

            if (response == null)
            {
                syncRec.Status = RecordStatus.InError;
                syncRec.StatusMessage = "Unknown server response";
            }
            else
            {
                if (syncRec.RequestId != response.RequestId)
                {
                    syncRec = await GetSyncRecordAsync(response.RequestId);
                }

                if (syncRec == null)
                {
                    return default(Guid);
                }

                if (response.Successful)
                {
                    syncRec.Status = RecordStatus.Synced;
                }
                else
                {
                    syncRec.Status = RecordStatus.InError;
                    syncRec.StatusMessage = response.ResponseText;
                }
            }

            syncRec.SyncAttemptCount += 1;
            await this.SaveAsync(syncRec);

            if (this.SyncAttemptCompleted != null)
            {
                this.SyncAttemptCompleted.Invoke(syncRec, EventArgs.Empty);
            }

            if (syncRec.Status == RecordStatus.Synced)
            {
                return syncRec.RequestId;
            }

            return default(Guid);
        }

        private async Task<object> GetSynchronizableModelBase(string tableName, Guid requestId)
        {
            CriteriaBuilder criteriaBuilder = new CriteriaBuilder();
            Criterion criterion = new Criterion("RequestId", requestId);
            criteriaBuilder.Add(criterion);

            if (tableName.AreEqual(TableProspects))
            {
                return await new ProspectsController(_lang, _country)
                    .GetSingleByCriteria(criteriaBuilder);
            }

            if (tableName.AreEqual(TableCustomer))
            {
                return await new CustomersController()
                    .GetSingleByCriteria(criteriaBuilder);
            }

            string error =
                string.Format(
                    "Unable to retrieve record from table {0} for syncing as specific controller for the table has not been setup in SyncingController", tableName);

            throw new Exception(error);
        }

        private async Task<PostResponse> PushObject(object syncronizableModelBase)
        {
            return await this.Synchronize(null, syncronizableModelBase);
        }

        /// <summary>
        /// Handle prospect down sync response
        /// </summary>
        /// <param name="prospectResponse">The response</param>
        /// <returns>a void task</returns>
        private async Task HandleResponse(DownSyncServerResponse<Prospect> prospectResponse)
        {
            if (prospectResponse == null)
            {
                return;
            }

            foreach (var prospect in prospectResponse.Package)
            {
                Prospect existingProspect = null;
                try
                {
                    existingProspect = await new ProspectsController().GetSingleByCriteria(
                        new CriteriaBuilder().Add("Phone", prospect.Phone));
                }
                catch (Exception e)
                {
                    this.Logger.Error(e);
                }

                if (existingProspect != null && existingProspect.Id != default(Guid))
                {
                    prospect.Converted = existingProspect.Converted;
                    prospect.Id = default(Guid);
                    prospect.Id = existingProspect.Id;
                    continue;
                }

                prospect.DsrPhone = prospectResponse.DsrPhone;
            }

            await new ProspectsController().SaveBulkAsync(prospectResponse.Package);
        }

        /// <summary>
        /// Handle prospect down sync response
        /// </summary>
        /// <param name="customerResponse">The response</param>
        /// <returns>a void task</returns>
        private async Task HandleResponse(DownSyncServerResponse<Customer> customerResponse)
        {
            if (customerResponse == null)
            {
                return;
            }

            foreach (var customer in customerResponse.Package)
            {
                Customer cust = null;
                try
                {
                    cust = await new CustomersController().GetSingleByCriteria(
                        new CriteriaBuilder().Add("Phone", customer.Phone));
                }
                catch (Exception e)
                {
                    this.Logger.Error(e);
                }

                if (cust != null && cust.Id != default(Guid))
                {
                    // update Id so that the record is updated
                    customer.Id = cust.Id;
                }

                customer.DsrPhone = customerResponse.DsrPhone;
                customer.Product.DisplayName = customer.Product.ProductName;
            }

            await new CustomersController().SaveBulkAsync(customerResponse.Package);
        }

        /// <summary>
        /// Handle prospect down sync response
        /// </summary>
        /// <param name="customerTicketResponse">The response</param>
        /// <returns>a void task</returns>
        private async Task HandleResponse(DownSyncServerResponse<CustomerTicket> customerTicketResponse)
        {
            if (customerTicketResponse == null)
            {
                return;
            }

            await new CustomerTicketController().SaveBulkAsync(customerTicketResponse.Package);
        }

        /// <summary>
        /// Handle prospect down sync response
        /// </summary>
        /// <param name="dsrTicketResponse">The response</param>
        /// <returns>a void task</returns>
        private async Task HandleResponse(DownSyncServerResponse<DsrTicket> dsrTicketResponse)
        {
            if (dsrTicketResponse == null)
            {
                return;
            }

            await new DsrTicketController().SaveBulkAsync(dsrTicketResponse.Package);
        }

        /// <summary>
        /// Returns the API end-point to use for down syncing
        /// </summary>
        /// <param name="t">The type for which to perform a down sync</param>
        /// <returns>The API instance to use for syncing</returns>
        private ApiBase GetDownSyncApi(Type t)
        {
            if (t == typeof(Customer))
            {
                return new DownSyncApi("customers/fetch");
            }

            if (t == typeof(Prospect))
            {
                return new DownSyncApi("prospects/fetch");
            }

            if (t == typeof(DsrTicket) || t == typeof(CustomerTicket))
            {
                return new DownSyncApi("TicketList");
            }

            return null;
        }

        /// <summary>
        /// This method handles different down sync server responses in a common place.
        /// </summary>
        /// <typeparam name="TModel">The type model for which to handle the response</typeparam>
        /// <param name="response">The response itself</param>
        /// <returns>Returns a void Task</returns>
        private async Task HandleDownSyncResponse<TModel>(DownSyncServerResponse<TModel> response) where TModel : class
        {
            if (response == null)
            {
                return;
            }

            for (int i = 0; i < response.Package.Count; i++)
            {
                SynchronizableModelBase item = response.Package[i] as SynchronizableModelBase;
                if (item == null)
                {
                    break;
                }

                item.DontSync = true;
                response.Package[i] = item as TModel;
            }

            Type t = typeof(TModel);

            if (t == typeof(Prospect))
            {
                // handle prospect down sync
                var prospectResponse = response as DownSyncServerResponse<Prospect>;
                await this.HandleResponse(prospectResponse);
            }

            if (t == typeof(Customer))
            {
                // handle customer down sync
                var customerResponse = response as DownSyncServerResponse<Customer>;
                await this.HandleResponse(customerResponse);
            }

            if (t == typeof(CustomerTicket))
            {
                // handle customer ticket down sync
                var customerTicketResponse = response as DownSyncServerResponse<CustomerTicket>;
                await this.HandleResponse(customerTicketResponse);
            }

            if (t == typeof(DsrTicket))
            {
                // handle customer dsr down sync
                var dsrTicketResponse = response as DownSyncServerResponse<DsrTicket>;
                await this.HandleResponse(dsrTicketResponse);
            }
        }

        /*private T GetSyncableObject<T>(string syncableJson)
        {
            return JsonConvert.DeserializeObject<T>(syncableJson);
        }*/

        /*private string GetSyncableJson<T>(T obj)
        {
            Type type = obj.GetType();
            bool isSyncable = false;
            Dictionary<string, string> postData = new Dictionary<string, string>();
            while (type != null)
            {
                foreach (PropertyInfo propInfo in type.GetTypeInfo().DeclaredProperties)
                {
                    if (propInfo.GetCustomAttribute<NotPostedAttribute>() == null)
                    {
                        object value = propInfo.GetValue(obj);

                        if (value == null)
                        {
                            value = string.Empty;
                        }

                        if (!isSyncable)
                        {
                            if (propInfo.Name.AreEqual("RequestId"))
                            {
                                isSyncable = !value.ToString().AreEqual(default(Guid).ToString());
                                if (!isSyncable)
                                {
                                    return string.Empty;
                                }
                            }
                        }

                        if (propInfo.Name.AreEqual("Id"))
                        {
                            if (value.ToString().AreEqual(default(Guid).ToString()))
                            {
                                Logger.Debug("Attempt to synchronize a model not locally saved was made");
                                throw new UnsavedModelException(string.Empty);
                            }
                        }

                        postData.Add(propInfo.Name, value.ToString());
                    }
                }

                type = type.GetTypeInfo().BaseType;
            }

            return JsonConvert.SerializeObject(postData);
        }*/
    }
}