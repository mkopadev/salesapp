using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.Stats.Units;
using SalesApp.Core.Services.Database;

namespace SalesApp.Core.BL.Controllers.Stats
{
    public class UnitsController : SQLiteDataService<DsrUnitsInfo>
    {
        public async Task<List<DsrUnitsInfo>> GetAllAsync(string sql)
        {
            List<DsrUnitsInfo> unists = await new QueryRunner().RunQuery<DsrUnitsInfo>(sql);
            return unists;
        }
    }
}
