using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.BL.Controllers.Synchronization;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.BL.Models.Syncing;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.Syncing;
using SalesApp.Core.Events.CustomerRegistration;
using SalesApp.Core.Exceptions.Database;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;

namespace SalesApp.Core.BL.Controllers.People
{
    public class CustomersController : PeopleController<Customer>
    {
        public override async Task<List<Customer>> GetAllAsync()
        {
            this.Logger.Debug("Getting All customers");
            List<Customer> customers = await base.SelectQueryAsync("SELECT * FROM Customer");
            if (customers == null)
            {
                return null;
            }

            this.Logger.Debug("Ordering customers by first name");
            return customers.OrderBy(customer => customer.FirstName).ToList();
        }

        public async Task<Customer> SaveCustomerToDevice(
            Customer personRegistrationInfo,
            CustomerRegistrationCompletedEventArgs e, bool b)
        {
            ProspectsController prospectsController = new ProspectsController();
            try
            {
                Prospect prospect = await prospectsController
                    .GetByPhoneNumberAsync(personRegistrationInfo.Phone, false);
                this.Logger.Debug("Trying to save customer");

                // check whether the customer is on the device, if not save the customer
                Customer existingCustomer =
                    await this.GetSingleRecord(new CriteriaBuilder().Add("Phone", personRegistrationInfo.Phone));

                if (existingCustomer != null)
                {
                    personRegistrationInfo.Id = existingCustomer.Id;
                }

                await DataAccess.Instance.Connection.RunInTransactionAsync(
                    async connTran =>
                    {
                        DataAccess.Instance.StartTransaction(connTran);
                        await this.SaveAsync(connTran, personRegistrationInfo);
                        if (personRegistrationInfo.Id != default(Guid))
                        {
                            await
                                new CustomerProductController().SaveCustomerProductToDevice(
                                    connTran,
                                    personRegistrationInfo.Id,
                                    personRegistrationInfo.Product);

                            if (prospect != null)
                            {
                                this.Logger.Debug("There was a prospect with the same number so we convert to customer");
                                prospect.Converted = true;
                                await prospectsController.SaveAsync(connTran, prospect);
                            }

                            RecordStatus syncStatus = RecordStatus.Synced;

                            if (personRegistrationInfo.Channel == DataChannel.Fallback)
                            {
                                if (e.Succeeded)
                                {
                                    syncStatus = RecordStatus.FallbackSent;
                                    this.Logger.Debug("Status = " + syncStatus);
                                }
                                else
                                {
                                    syncStatus = RecordStatus.Pending;
                                    this.Logger.Debug("Status = " + syncStatus);
                                }
                            }

                            SyncingController syncController = new SyncingController();

                            SyncRecord syncRec = await syncController.GetSyncRecordAsync(personRegistrationInfo.RequestId);
                            this.Logger.Debug("Fetching sync record for customer");

                            if (syncRec == null)
                            {
                                this.Logger.Debug("The sync record is null so we generate one");
                                syncRec = new SyncRecord
                                {
                                    ModelId = personRegistrationInfo.Id,
                                    ModelType = personRegistrationInfo.TableName,
                                    Status = syncStatus,
                                    RequestId = personRegistrationInfo.RequestId
                                };
                            }
                            else
                            {
                                syncRec.Status = syncStatus;
                            }

                            await syncController.SaveAsync(connTran, syncRec);
                        }
                    });

                DataAccess.Instance.CommitTransaction();
                return personRegistrationInfo;
            }
            catch (DuplicateValuesException ex)
            {
                throw new DuplicateValuesException(ex.FieldName, ex.Value, ex.Count);
            }
            catch (Exception exception)
            {
                this.Logger.Error(exception);
                return null;
            }
        }
    }
}
