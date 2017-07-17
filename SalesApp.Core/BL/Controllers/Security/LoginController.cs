using System;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;

namespace SalesApp.Core.BL.Controllers.Security
{
    public class LoginController : SQLiteDataService<DsrProfile> 
    {
        public async Task<DsrProfile> GetByDsrPhoneNumberAsync(string dsrPhone)
        {
            this.Logger.Debug(string.Format("Searching for dsr with phone number {0}", dsrPhone));

            return await this.SelectSingleOrNothing(
                    new[]
                    {
                        new Criterion("DsrPhone", dsrPhone)
                    });
        }

        public async Task<DsrProfile> GetDsr(Guid id)
        {
            return await this.GetByIdAsync(id);
        }

        public async override Task<SaveResponse<DsrProfile>> SaveAsync(DsrProfile model)
        {
            SaveResponse<DsrProfile> response = null;
            await DataAccess.Instance.Connection.RunInTransactionAsync(
            async connTran =>
            {
                DataAccess.Instance.StartTransaction(connTran);
                this.Logger.Debug("Deleting previous permissions");
                await DataAccess.Instance.DeleteAllAsync<DsrProfile>();
                response = await base.SaveAsync(connTran, model);
            });

            DataAccess.Instance.CommitTransaction();
            return response;
        }
    }
}