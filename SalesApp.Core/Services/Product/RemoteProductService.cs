using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.Api;
using SalesApp.Core.Api.Product;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Services.RemoteServices;

namespace SalesApp.Core.Services.Product
{
    public class RemoteProductService : RemoteServiceBase<ProductApi, BL.Product, List<BL.Product>>
    {
        private LocalProductService _localProductService;

        public RemoteProductService(string url)
        {
            this.Api = new ProductApi(url);
        }

        private ProductApi ProductApi
        {
            get
            {
                return this.Api;
            }
        }

        public virtual async Task<List<BL.Product>> GetProducts(string urlParams = null)
        {
            try
            {
                this.Logger.Debug("Fetching products data from server");
                ServerResponse<List<BL.Product>> serverResponse = await this.ProductApi.GetAsync(urlParams, ErrorFilterFlags.AllowEmptyResponses | ErrorFilterFlags.IgnoreNoInternetError);
                if (serverResponse == null)
                {
                    this.Logger.Verbose("No results.");
                }
                else if (serverResponse.IsSuccessStatus)
                {
                    List<BL.Product> products = serverResponse.GetObject();
                    await this.LocalProductService.SetAsync(products);
                    return products;
                }
            }
            catch (JsonReaderException jsonReaderException)
            {
                this.Logger.Error(jsonReaderException);
            }
            catch (NotConnectedToInternetException nctiex)
            {
                this.Logger.Error(nctiex);
            }
            catch (TaskCanceledException taskCanceled)
            {
                this.Logger.Error(taskCanceled);
            }
            catch (Exception exception)
            {
                this.Logger.Error(exception);
                throw;
            }

            return new List<BL.Product>();
        }

        private LocalProductService LocalProductService
        {
            get
            {
                this._localProductService = this._localProductService ?? new LocalProductService();
                return this._localProductService;
            }
        }
    }
}
