namespace SalesApp.Core.Api.People.Customers
{
    public class CustomerSearchApi : ApiBase
    {

        public CustomerSearchApi()
            : this("customers/search?query=")
        {
        }

        private CustomerSearchApi(string apiRelativePath) : base(apiRelativePath)
        {
        }
    }
}