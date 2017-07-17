using System;
using System.Threading.Tasks;
using SalesApp.Core.Api.Person;
using SalesApp.Core.Enums.Syncing;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Search;

namespace SalesApp.Core.BL.Models.People
{
    /// <summary>
    /// Represents a customer search result
    /// </summary>
    public class CustomerSearchResult : Customer, ISearchResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not the result was found in the local database
        /// </summary>
        public bool IsLocalRecord { get; set; }

        /// <summary>
        /// Gets or sets the score - How close is it to the search query
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Gets the unique value to use when merging local and upstream results
        /// </summary>
        public string UniqueValue
        {
            get { return this.Phone; }
        }

        /// <summary>
        /// Gets the value to display on the UI
        /// </summary>
        public string DisplayText
        {
            get { return FullName; }
        }

        /// <summary>
        /// Gets or sets the sync status
        /// </summary>
        public RecordStatus SyncStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is active or not
        /// </summary>
        public bool ProductIsActive { get; set; }

        /// <summary>
        /// Gets or sets the customer status
        /// </summary>
        [Obsolete("Don't access this property directly, it's only used for caching. Instead use GetStatusAsync(). Also don't set this property to private, cache will fail.")]
        public CustomerStatus Status { get; set; }

        /// <summary>
        /// The method to use for upstream search
        /// </summary>
        /// <returns>Returns the customer status</returns>
        public async Task<CustomerStatus> GetStatusAsync()
        {
            try
            {
                // if no connection, return default CustomerStatus
                if (!Resolver.Instance.Get<IConnectivityService>().HasConnection())
                {
                    return new CustomerStatus();
                }

                // if Status is already loaded, return
                if (this.Status != default(CustomerStatus))
                {
                    return Status;
                }

                // Need to get the Status from APU
                this.Status = await new CustomerStatusApi().GetAsync(this.Phone);

                // return result from the API
                return this.Status;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }
    }
}