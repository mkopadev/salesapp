using System.Threading.Tasks;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Services.Connectivity;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.Api.Person
{
    public class CustomerStatusApi : ApiBase
    {
        public CustomerStatusApi()
            : base("customerstatus")
        {
        }

        public async Task<CustomerStatus> GetAsync(string phone)
        {
            if (!Resolver.Instance.Get<IConnectivityService>().HasConnection())
            {
                return null;
            }

            ServerResponse<CustomerStatus> serverResponse =
                await this.MakeGetCallAsync<CustomerStatus>("/Get?phoneNumber=" + phone);

            if (serverResponse.IsSuccessStatus)
            {
                return serverResponse.GetObject();
            }

            return null;
        }
    }
}