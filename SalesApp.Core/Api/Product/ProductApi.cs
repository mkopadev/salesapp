using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Core.Enums.Api;

namespace SalesApp.Core.Api.Product
{
    public class ProductApi : ApiBase
    {
        public ProductApi(string relativePath) : base(relativePath)
        {
        }

        public async Task<ServerResponse<List<BL.Product>>> GetAsync(string urlParams = null, ErrorFilterFlags flags = ErrorFilterFlags.EnableErrorHandling)
        {
            ServerResponse<List<BL.Product>> serverResponse = await this.MakeGetCallAsync<List<BL.Product>>(urlParams, null, flags);
            if (serverResponse.IsSuccessStatus)
            {
                return serverResponse;
            }

            return null;
        }
    }
}
