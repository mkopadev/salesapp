using Android.Content;
using Android.Support.V4.App;
using Java.Lang;
using SalesApp.Droid.UI.Stats.Rankings;
using SalesApp.Droid.UI.Stats.Sales;

namespace SalesApp.Droid.UI.Stats
{
    public class StatsFragmentAdapter : FragmentPagerAdapter
    {
        private Context _context;

        public StatsFragmentAdapter(FragmentManager fm, Context context) : base(fm)
        {
            this._context = context;
        }
       
        public override int Count
        {
            get
            {
                return 3;
            }
        }

        public override Fragment GetItem(int position)
        {
            switch (position)
            {
                case StatsView.SalesTab:
                    var salesTabFragment = new SalesStatsListFragment();
                    return salesTabFragment;
                case StatsView.RankingTab:
                    var rankingTabFragment = new RankingStatsListFragment();
                    return rankingTabFragment;
                case StatsView.UnitsTab:
                    var unitsTabFragment = new UnitsStatsFragment();
                    return unitsTabFragment;
            }

            return null;
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            switch (position)
            {
               case StatsView.SalesTab:
                    string salesTitle = this._context.GetString(Resource.String.sales);
                    return new String(salesTitle);
                case StatsView.RankingTab:
                    string rankingsTitle = this._context.GetString(Resource.String.rankings);
                    return new String(rankingsTitle);
                case StatsView.UnitsTab:
                    string unitsTitle = this._context.GetString(Resource.String.units);
                    return new String(unitsTitle);
            }

            return null;
        }
    }
}