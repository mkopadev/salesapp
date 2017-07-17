using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesApp.Core.Api.People.Customers;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.Syncing;
using SalesApp.Core.Extensions;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.RemoteServices;
using SalesApp.Core.Services.Search;

namespace SalesApp.Core.Services.Person.Customer
{
    public class CustomerSearchService :
        RemoteServiceBase<CustomerSearchApi, CustomerSearchResult, List<CustomerSearchResult>> ,ISearchService<CustomerSearchResult>
    {

        public bool SearchedOnline { get; private set; }

        private bool AlsoSearchOnline { get; set; }

        public async Task<List<CustomerSearchResult>> GetAllLocalAsync()
        {
            return await this.SearchAsync(string.Empty, false);
        }

        public async Task<List<CustomerSearchResult>> SearchAsync(string queryString, bool alsoSearchOnline)
        {
            if (queryString.IsBlank())
            {
                this.AlsoSearchOnline = false;
                return (await this.DoSearchAsync(string.Empty)).ToList();
            }
            else
            {
                this.AlsoSearchOnline = alsoSearchOnline;
                return await new SearchServiceHelper<CustomerSearchResult>().SearchAsync(queryString, DoSearchAsync);
            }
        }

        private async Task<CustomerSearchResult[]> DoSearchAsync(string queryString)
        {
            CustomerSearchResult[] onDeviceCustomers = (await this.GetLocalResultsAsync(queryString)).ToArray();
            if (onDeviceCustomers == null)
            {
                onDeviceCustomers = new CustomerSearchResult[0];
            }

            SearchedOnline = false;
            if (!Resolver.Instance.Get<IConnectivityService>().HasConnection() || !AlsoSearchOnline)
            {
                return onDeviceCustomers;
            }

            CustomerSearchResult[] onlineCustomers = this.PostOnlineSearchActions(await this.GetAsync(queryString, ErrorFilterFlags.DisableErrorHandling)).ToArray();
            SearchedOnline = true;
            if (onlineCustomers == null || onlineCustomers.Length == 0)
            {
                return onDeviceCustomers;
            }

            if (onDeviceCustomers.Length == 0)
            {
                return onlineCustomers;
            }

            return GetUniques(onDeviceCustomers, onlineCustomers);
        }

        private List<CustomerSearchResult> PostOnlineSearchActions(List<CustomerSearchResult> results)
        {
            if (results == null || results.Count == 0)
            {
                return new List<CustomerSearchResult>();
            }

            foreach (var item in results)
            {
                item.ProductIsActive = item.Product != null && !item.Product.SerialNumber.IsBlank();
                item.SyncStatus = RecordStatus.Synced;
            }

            return results;
        }

        private CustomerSearchResult[] GetUniques(
            CustomerSearchResult[] onDeviceCustomers,
            CustomerSearchResult[] onlineCustomers)
        {
            List<string> duplicatedCustomers = onDeviceCustomers.Select(onDev => onDev.Phone)
                .Intersect(
                    onlineCustomers.Select(
                            onlineCust => onlineCust.Phone)).ToList();

            if (duplicatedCustomers == null || duplicatedCustomers.Count == 0)
            {
                 List<CustomerSearchResult> combined = onDeviceCustomers.ToList();
                combined.AddRange(onlineCustomers);
                return combined.ToArray();
            }

            CustomerSearchResult[] onlineTrimmed = onlineCustomers
                    .Where(
                        cust => !duplicatedCustomers.Contains(cust.Phone)).ToArray();

            if (onlineTrimmed == null || onlineTrimmed.Length == 0)
            {
                return onDeviceCustomers;
            }

            List<CustomerSearchResult> all = onDeviceCustomers.ToList();
            all.AddRange(onlineTrimmed);
            return all.ToArray();
        }

        private async Task<List<CustomerSearchResult>> GetLocalResultsAsync(string queryString)
        {
            List<CustomerSearchResult> customerSearchResults;
            string sql = "SELECT c.*, (SELECT s.Status FROM SyncRecord s WHERE s.RequestId = c.RequestId ORDER BY  DateTime(s.DateUpdated) DESC LIMIT 1) as SyncStatus FROM Customer c";
            if (!queryString.IsBlank())
            {
                string newSql = string.Format(" WHERE Phone LIKE '%{0}%' OR FirstName LIKE '%{1}%' OR LastName LIKE '%{2}%'", queryString, queryString, queryString);
                string tql = sql + newSql + " ORDER BY DateUpdated DESC";
                customerSearchResults = await new QueryRunner().RunQuery<CustomerSearchResult>(tql);
            }
            else
            {
                string query = sql + " ORDER BY DateUpdated DESC";
                customerSearchResults = await new QueryRunner().RunQuery<CustomerSearchResult>(query);
            }

            if (customerSearchResults == null || customerSearchResults.Count == 0)
            {
                return new List<CustomerSearchResult>();
            }

            string stepsQuery = "SELECT CustomerId, StepStatus FROM CustomerRegistrationStepsStatus";
            List<CustomerRegistrationStepsStatus> regSteps = await new QueryRunner().RunQuery<CustomerRegistrationStepsStatus>(stepsQuery);

            if (regSteps == null || regSteps.Count == 0)
            {
                return customerSearchResults;
            }

            for (int i = 0; i < customerSearchResults.Count; i++)
            {
                List<CustomerRegistrationStepsStatus> statuses = regSteps.FindAll(x => x.CustomerId == customerSearchResults.ElementAt(i).Id);
                if (statuses == null || statuses.Count == 0)
                {
                    continue;
                }

                if (statuses.All(step => step.StepStatus.AreEqual("done")))
                {
                    customerSearchResults.ElementAt(i).ProductIsActive = statuses.All(step => step.StepStatus.AreEqual("done"));
                }
                else if (statuses.Any(step => step.StepStatus.AreEqual("Blocked")))
                {
                    // rejected customer
                    customerSearchResults.ElementAt(i).IsRejected = statuses.Any(step => step.StepStatus.AreEqual("Blocked"));
                }
            }

            return customerSearchResults;
        }
    }
}