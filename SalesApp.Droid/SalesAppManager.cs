using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.Api;
using SalesApp.Core.BL;
using SalesApp.Core.BL.Managers;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Settings;
using SalesApp.Droid.Api;

namespace SalesApp.Droid
{
    /// <summary>
    /// Business layer for sales app. 
    /// </summary>
    public class SalesAppManager : ISalesAppManager
    {
        private readonly IConnectivityService _connectivityService = Resolver.Instance.Get<IConnectivityService>();
        private static readonly ILog Log = LogManager.Get(typeof(SalesAppManager));

        private readonly SalesAppApi api;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SalesAppManager"/> class.
        /// </summary>
        public SalesAppManager()
        {
            api = new SalesAppApi();
        }

        /// <summary>
        ///     Checks the status of a person (prospect or customer).
        /// </summary>
        /// <param name="phone">The phone.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">phone</exception>
        public async Task<Person> CheckPerson(string phone)
        {
            Log.Verbose("CheckPerson");
            
            if (phone == null) throw new ArgumentNullException("phone");


            Person person =  await api.CheckPerson(phone);
            return person;
        }

        /// <summary>
        /// Registers a prospect.
        /// </summary>
        /// <param name="prospect">The prospect.</param>
        /// <returns>Status.</returns>
        /// <exception cref="System.ArgumentNullException">prospect</exception>
        public async Task<Status> RegisterProspect(Prospect prospect)
        {
            if (prospect == null) throw new ArgumentNullException("prospect");

            var prospectDto = new ProspectDto
            {
                DsrPhone = prospect.DsrPhone,
                FirstName = prospect.FirstName,
                LastName = prospect.LastName,
                Phone = prospect.Phone,
                Means = prospect.Money,
                Authority = prospect.Authority,
                Need = prospect.Need
            };

            Status status = null;
            StatusDto statusDto = await api.RegisterProspect(prospectDto);
            if (statusDto != null)
            {
                status = new Status
                {
                    Success = statusDto.Successful,
                    Text = statusDto.ResponseText
                };
            }
            return status;
        }

        /// <summary>
        /// Registers a customer.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <returns>Status..</returns>
        /// <exception cref="System.ArgumentNullException">customer</exception>
        public async Task<Status> RegisterCustomer(Customer customer)
        {
            if (customer == null) throw new ArgumentNullException("customer");
            if (customer.Product == null) throw new ArgumentException("customer.Product");

            Product product = customer.Product;
            var customerDto = new CustomerDto
            {
                DsrPhone = customer.DsrPhone,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Phone = customer.Phone,
                NationalId = customer.NationalId,
                Product = new ProductDto
                    {
                        ProductTypeId = product.ProductTypeId,
                        DisplayName = product.DisplayName,
                        SerialNumber = product.SerialNumber
                    }
            };

            Status status = null;
            StatusDto statusDto = await api.RegisterCustomer(customerDto);
            if (statusDto != null)
            {
                status = new Status
                {
                    Success = statusDto.Successful,
                    Text = statusDto.ResponseText
                };
            }
            return status;
        }

        /// <summary>
        /// Lists the products for the user.
        /// </summary>
        /// <returns>Products.</returns>
        public async Task<List<Product>> ListProducts()
        {
            Log.Verbose("List Products.");
            List<Product> products = new List<Product>();
            var settings = Settings.Instance;
            var dsrPhone = settings.DsrPhone;

            if (_connectivityService.HasConnection())
            {
                Log.Verbose("Connection exists, get products from API.");
                var productDtos = await api.GetProducts(dsrPhone);
                // only process the api call, when there is a result
                if (productDtos != null)
                {
                    products = productDtos.Select(productDto => new Product
                    {
                        DisplayName = productDto.DisplayName,
                        ProductTypeId = productDto.ProductTypeId,
                        SerialNumber = productDto.SerialNumber,
                        DateAcquired = productDto.DateAcquired
                    }).ToList();    
                }
                
            }
            
            return products;
        }

        /// <summary>
        /// Lists the messages.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Message>> ListMessages(DateTime lastFetchDate)
        {
            var messageDtos = await api.GetMessages(lastFetchDate);

            var messages = messageDtos.Select(messageDto => new Message()
            {
                MessageId = messageDto.Id,
                Body = messageDto.Body,
                ExpiryDate = messageDto.ExpiryDate,
                From = messageDto.From,
                IsRead = false,
                MessageDate = messageDto.MessageDate.AddHours(3),
                Subject = messageDto.Subject
            }).ToList();

            return messages;
        }
    }
}