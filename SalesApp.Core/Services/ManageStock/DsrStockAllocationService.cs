using System.Threading.Tasks;
using SalesApp.Core.Api;
using SalesApp.Core.Api.ManageStock;
using SalesApp.Core.BL.Models.ManageStock;

namespace SalesApp.Core.Services.ManageStock
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class DsrStockAllocationService
    {
        private StockManagementApi _currentDsrStockApi;
        private StockManagementApi _stockAllocationApi;
        private StockManagementApi _recieveAllocatedStockApi;

        public DsrStockAllocationService()
        {
            this._currentDsrStockApi = new StockManagementApi("StockAllocation");
            this._stockAllocationApi = new StockManagementApi("StockAllocation/Allocate");
            this._recieveAllocatedStockApi = new StockManagementApi("StockAllocation/Receive");
        }

        public virtual async Task<ServerResponse<DsrStockServerResponseObject>> GetCurrentDsrStock(string queryParams)
        {
            return await this._currentDsrStockApi.MakeGetCallAsync<DsrStockServerResponseObject>(queryParams);
        }

        public virtual async Task<ManageStockPostApiResponse> AllocateUnitsToDsr(SelectedProduct selectedProduct)
        {
            ServerResponse<ManageStockPostApiResponse> result = await this._stockAllocationApi.PostObjectAsync<ManageStockPostApiResponse, SelectedProduct>(selectedProduct);

            if (result == null)
            {
                return null;
            }

            return result.GetObject();
        }

        public virtual async Task<ManageStockPostApiResponse> ReceiveStockFromDsr(SelectedProduct selectedProducts)
        {
            ServerResponse<ManageStockPostApiResponse> result = await this._recieveAllocatedStockApi.PostObjectAsync<ManageStockPostApiResponse, SelectedProduct>(selectedProducts);

            if (result == null)
            {
                return null;
            }

            return result.GetObject();
        }
    }
}