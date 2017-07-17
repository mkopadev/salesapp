using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.BL.Controllers.Synchronization;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.BL.Models.Syncing;
using SalesApp.Core.Enums.Syncing;
using SalesApp.Core.Exceptions.Database;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;
using SQLite.Net;

namespace SalesApp.Core.BL.Controllers.People
{
    public class CustomerRegistrationStepsStatusController : SQLiteDataService<CustomerRegistrationStepsStatus>
    {
        private ProductsController _productsController;

        private ProductsController ProductsController
        {
            get
            {
                _productsController = _productsController ?? new ProductsController();
                return _productsController;
            }
        }

        public async Task<bool> IsProductActiveAsync(Guid customerId)
        {
            CriteriaBuilder criteriaBuilder = new CriteriaBuilder();
            List<CustomerRegistrationStepsStatus> allForCustomer = await this.GetManyByCriteria(
                criteriaBuilder
                    .Add("CustomerId", customerId));

            if (allForCustomer == null || allForCustomer.Count == 0)
            {
                return false;
            }

            Logger.Debug("Steps for customer ~".GetFormated(customerId));
            Logger.Debug("Steps count ~".GetFormated(allForCustomer.Count));

            foreach (var step in allForCustomer)
            {
                Logger.Debug("Step: ~ Value: ~".GetFormated(step.StepName, step.StepStatus));
            }

            if (allForCustomer.Count == 0)
            {
                Logger.Debug("Found no steps");
                return false;
            }

            bool active = allForCustomer.All(step => step.StepStatus.AreEqual("done") != false);
            Logger.Debug("Customer ~ IsRejected = ~".GetFormated(customerId, active));
            return active;
        }

        public async Task<List<CustomerRegistrationStepsStatus>> SaveAsync(CustomerStatus status, Customer customer)
        {
            List<CustomerRegistrationStepsStatus> result = null;
            await DataAccess.Instance.Connection.RunInTransactionAsync(
                    async connTran =>
                    {
                        DataAccess.Instance.StartTransaction(connTran);
                        result = await this.SaveAsync(connTran, status, customer);
                    });

            DataAccess.Instance.CommitTransaction();
            return result;
        }

        /// <summary>
        /// Saves customer status and if necessary the related product.
        /// If the status details for the product already exist, they are deleted
        /// </summary>
        /// <param name="status">The status as supplied by the server</param>
        /// <param name="customer">The customer</param>
        /// <param name="connTran">The connection object for wrapping db calls in a transaction</param>
        /// <returns>A list of all saved items or null if the status was null</returns>
        /// <exception cref="NotSavedException">NotSavedException is thrown if even one item could not be saved</exception>
        public async Task<List<CustomerRegistrationStepsStatus>> SaveAsync(SQLiteConnection connTran, CustomerStatus status, Customer customer)
        {
            if (status == null || status.CustomerNotFound.IsBlank() == false)
            {
                return null;
            }

            if (status.CustomerProduct.IsBlank())
            {
                status.CustomerProduct = "Product III";
            }

            CriteriaBuilder criteriaBuilder = new CriteriaBuilder();
            criteriaBuilder.Add("DisplayName", status.CustomerProduct);

            string query = this.ProductsController.QueryBuilder(criteriaBuilder);
            List<Product> products = await this.ProductsController.SelectQueryAsync(query);

            Product product = null;
            if (products != null)
            {
                product = products.OrderByDescending(prod => prod.Modified).FirstOrDefault();
            }

            List<CustomerRegistrationStepsStatus> resultList = new List<CustomerRegistrationStepsStatus>();

            if (product == null)
            {
                product = new Product
                {
                    DisplayName = status.CustomerProduct
                };

                await this.ProductsController.SaveAsync(connTran, product);
            }
            else
            {
                string sql =
                    string.Format(
                        "Delete from CustomerRegistrationStepsStatus where ProductId='{0}' and CustomerId='{1}'",
                        product.Id, customer.Id);
                await DataAccess.Instance.RunQueryAsync(sql);
            }

            if (status.Steps != null)
            {
                foreach (var step in status.Steps)
                {
                    CustomerRegistrationStepsStatus registrationStepsStatus = new CustomerRegistrationStepsStatus
                    {
                        CustomerId = customer.Id,
                        ProductId = product.Id,
                        RequestStatus = status.RequestStatus,
                        StepName = step.StepName,
                        StepNumber = step.StepNumber,
                        StepStatus = step.StepStatus,
                        AdditionalInfo = status.AdditionalInfo
                    };

                    var saveResponse = await base.SaveAsync(connTran, registrationStepsStatus);
                    if (saveResponse.SavedModel != null)
                    {
                        resultList.Add(saveResponse.SavedModel);
                    }
                    else
                    {
                        throw new NotSavedException();
                    }
                }
            }

            SyncingController syncingController = new SyncingController();
            SyncRecord syncRecord = await syncingController.GetSyncRecordAsync(customer.RequestId);

            if (syncRecord != null)
            {
                syncRecord.Status = RecordStatus.Synced;
                await syncingController.SaveAsync(connTran, syncRecord);
            }

            return resultList;
        }
    }
}