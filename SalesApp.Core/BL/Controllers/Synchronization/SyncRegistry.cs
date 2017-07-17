using SalesApp.Core.Api;
using SalesApp.Core.Api.People;
using SalesApp.Core.Api.Person;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Extensions;

namespace SalesApp.Core.BL.Controllers.Synchronization
{
    public class SyncRegistry
    {
        public ApiBase GetApiBaseInstance(string modelType)
        {
            if (modelType.AreEqual("Customer"))
            {
                return new CustomerApi();
            }

            if (modelType.AreEqual("Prospect"))
            {
                return new PeopleApis<Prospect>("prospects");
            }

            return null;
        }
    }
}