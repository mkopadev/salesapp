using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;
using SQLite.Net;

namespace SalesApp.Core.BL.Controllers.People
{
    /// <summary>
    /// Class to help save/retrieve customer products into the local database
    /// </summary>
    public class CustomerProductController : SQLiteDataService<CustomerProduct>
    {
        /// <summary>
        /// Saves the customers selected product to the device
        /// </summary>
        /// <param name="customerId">The customer's ID</param>
        /// <param name="tran">The transaction in which to run</param>
        /// <param name="product">The product that a customer selects</param>
        /// <returns>A void task</returns>
        public async Task SaveCustomerProductToDevice(Guid customerId, SQLiteConnection tran, Product product)
        {
            CustomerProduct cutomerProduct = JsonConvert.DeserializeObject<CustomerProduct>(JsonConvert.SerializeObject(product));
            cutomerProduct.CustomerId = customerId;
            await this.SaveAsync(tran, cutomerProduct);
        }

        public async Task SaveCustomerProductToDevice(SQLiteConnection contran, Guid customerId, Product product)
        {
            CustomerProduct cutomerProduct = JsonConvert.DeserializeObject<CustomerProduct>(JsonConvert.SerializeObject(product));
            CustomerProduct cp = await GetCustomerProduct(customerId);
            if (cp != null && cp.CustomerId != default(Guid))
            {
                cp.DisplayName = product.DisplayName;
                cp.ProductTypeId = product.ProductTypeId;
                cp.Created = product.Created;
                cp.DateAcquired = product.DateAcquired;
                cp.Modified = product.Modified;
                cp.SerialNumber = product.SerialNumber;
                await this.SaveAsync(contran, cp);
            }
            else
            {
                cutomerProduct.CustomerId = customerId;
                await this.SaveAsync(contran, cutomerProduct);
            }
        }

        public async Task SaveCustomerProductToDevice(CustomerProduct customerProduct)
        {
            await this.SaveAsync(customerProduct);
        }

        private async Task<CustomerProduct> GetCustomerProduct(Guid custId)
        {
            CustomerProduct cp = await this.GetSingleRecord(new CriteriaBuilder().Add("CustomerId", custId));

            return cp;
        }
    }
}