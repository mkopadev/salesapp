using System.Threading.Tasks;
using SalesApp.Core.Enums.Api;

namespace SalesApp.Core.Api.People
{
    public class PeopleApis<T> : ApiBase where T : BL.Models.People.Person
    {
        public PeopleApis(string apiRelativePath) : base(apiRelativePath)
        {
        }

        public async Task<T> Search(string searchParams, ErrorFilterFlags filterFlags, ApiTimeoutEnum timeOut = ApiTimeoutEnum.Normal)
        {
            ServerResponse<T> response = await this.MakeGetCallAsync<T>(searchParams, null, filterFlags, timeOut);
            if (response == null || !response.IsSuccessStatus)
            {
                return null;
            }

            return response.GetObject();
        }

        public async override Task<PostResponse> AfterSyncUpProcessing(object model, PostResponse postResponse)
        {
            if (postResponse.Successful)
            {
                return postResponse;
            }

            string oldRelativePath = this.ApiRelativePath;

            try
            {
                this.ApiRelativePath = "persons/";
                BL.Models.People.Person syncedPerson = (BL.Models.People.Person)model;
                BL.Models.People.Person foundPerson = await Search(syncedPerson.Phone,ErrorFilterFlags.Ignore400Family);
                if (foundPerson != null && syncedPerson.PersonType == foundPerson.PersonType)
                {
                    postResponse.Successful = true;
                }
                return postResponse;
            }
            finally
            {
                this.ApiRelativePath = oldRelativePath;
            }
        }
    }
}