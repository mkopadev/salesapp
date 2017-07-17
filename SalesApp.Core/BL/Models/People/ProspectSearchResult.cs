using SalesApp.Core.BL.Models.Syncing;
using SalesApp.Core.Services.Search;

namespace SalesApp.Core.BL.Models.People
{
    public class ProspectSearchResult : Prospect, ISearchResult
    {
        public bool IsLocalRecord { get; set; }

        public int Score { get; set; }

        public string UniqueValue
        {
            get { return Phone; }
        }

        public string DisplayText
        {
            get { return FullName; }
        }

        public SyncRecord SyncRecord { get; set; } 
    }
}