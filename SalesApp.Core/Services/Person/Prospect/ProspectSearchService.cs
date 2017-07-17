using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Core.BL.Controllers.People;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.BL.Models.Syncing;
using SalesApp.Core.Enums.Syncing;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Search;

namespace SalesApp.Core.Services.Person.Prospect
{
    public class ProspectSearchService : ISearchService<ProspectSearchResult>
    {

        private ProspectsController prospectsController;
        private ILog Logger = LogManager.Get(typeof(ProspectSearchService));

        public bool SearchedOnline { get; private set; }

        public ProspectsController PropectsController
        {
            get
            {
                if (this.prospectsController == null)
                {
                    this.prospectsController = new ProspectsController();
                }

                return this.prospectsController;
            }
        }

        public async Task<List<ProspectSearchResult>> GetAllLocalAsync()
        {
            var all = await this.PropectsController.GetAllAsync();
            return this.SearchResultsFromModel(all, true);
        }

        public async Task<List<ProspectSearchResult>> SearchAsync(string queryString, bool alsoSearchOnline)
        {
            this.SearchedOnline = true; // NOT REALLY THOUGH ;)
            if (!queryString.IsBlank())
            {
                return await new SearchServiceHelper<ProspectSearchResult>()
                    .SearchAsync(queryString, this.DoSearchAsync);
            }
            else
            {
                return await this.GetAllLocalAsync();
            }
        }

        private async Task<ProspectSearchResult[]> DoSearchAsync(string queryString)
        {
            string query = "SELECT p.*, pf.ReminderTime AS ReminderTime, sr.Status AS SyncStatus FROM Prospect p LEFT JOIN ProspectFollowup pf ON p.Id = pf.ProspectId LEFT JOIN SyncRecord sr ON sr.RequestId=p.RequestId";
            string oderBy = " ORDER BY pf.ReminderTime ASC, p.DateCreated DESC";
            string where = " WHERE p.Converted = 0";

            if (!string.IsNullOrWhiteSpace(queryString))
            {
                where += string.Format(" AND (p.Phone LIKE '%{0}%' OR p.FirstName LIKE '%{0}%' OR p.LastName LIKE '%{0}%')", queryString);
            }

            query += where + oderBy;

            List<BL.Models.People.Prospect> prospects = await new QueryRunner().RunQuery<BL.Models.People.Prospect>(query);

            return this.SearchResultsFromModel(prospects, true).ToArray();
        }

        private List<ProspectSearchResult> SearchResultsFromModel(List<BL.Models.People.Prospect> modelList,  bool isLocalResult)
        {
            if (modelList == null || modelList.Count == 0)
            {
                return new List<ProspectSearchResult>();
            }

            List<ProspectSearchResult> results = new List<ProspectSearchResult>();

            foreach (var prospect in modelList)
            {
                if (prospect.SyncStatus == 0)
                {
                    // This means there is no corresponding sync record for this model, so treat it as having been fully synced
                    prospect.SyncStatus = RecordStatus.Synced;
                }

                ProspectSearchResult ps = new ProspectSearchResult();
                ps.Id = prospect.Id;
                ps.Phone = prospect.Phone;
                ps.FirstName = prospect.FirstName;
                ps.LastName = prospect.LastName;
                ps.DsrPhone = prospect.DsrPhone;
                ps.Product = prospect.Product;
                ps.Money = prospect.Money;
                ps.Need = prospect.Need;
                ps.Authority = prospect.Authority;
                ps.ReminderTime = prospect.ReminderTime;
                ps.Converted = prospect.Converted;
                ps.Channel = prospect.Channel;
                ps.SyncRecord = new SyncRecord { Status = prospect.SyncStatus };
                ps.Created = prospect.Created;
                ps.Modified = prospect.Modified;
                results.Add(ps);
            }

            return results;
        }
    }
}