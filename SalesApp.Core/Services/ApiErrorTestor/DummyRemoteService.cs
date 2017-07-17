using System.Threading.Tasks;
using Mkopa.Core.Api;

namespace Mkopa.Core.Services.ApiErrorTestor
{
    public class DummyRemoteService : ApiBase
    {
        class MyClass
        {
            public string OneThing { get; set; }
            public int AnotherThing { get; set; }
        }

        public DummyRemoteService() : base("wherever")
        {
        }

        public async Task GetStuff()
        {
            await MakeGetCallAsync<MyClass>("who cares?", StuffGotten);
        }

        private void StuffGotten(ServerResponse<MyClass> response)
        {
            Logger.Debug("We got the stuff " + response.RawResponse);
        }

    }
}