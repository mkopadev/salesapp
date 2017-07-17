using Android.App;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.Syncing;

namespace SalesApp.Droid.People.Prospects
{
    public class ProspectListAdapter : PersonSectionListAdapter<ProspectItem>
    {
        public ProspectListAdapter(Activity activity) : base(activity)
        {
        }
    }

    public class ProspectItem :  IPersonItem
    {
        public ProspectSearchResult SearchResult { get; set; }

        public RecordStatus SyncStatus { get; set; }

        public ProspectItem(ProspectSearchResult searchResult)
        {
            this.SearchResult = searchResult;
        }

        public string GetFilterString()
        {
            return SearchResult.FullName + SearchResult.Phone;
        }

        public int GetProspectPotentialColor()
        {
            int weight = 0;

            if (SearchResult.Money) weight++;
            if (SearchResult.Need) weight++;
            if (SearchResult.Authority) weight++;

            if (weight <= 1)
                return Resource.Drawable.icon_cold;

            if (weight == 2)
                return Resource.Drawable.icon_warm;

            if (weight == 3)
                return Resource.Drawable.icon_hot;

            return weight;
        }
    }
}