using Android.Support.V4.App;
using Java.Lang;
using SalesApp.Core.Logging;

namespace SalesApp.Droid.People.SwapComponents
{
    class SwapFragmentsAdapter: FragmentPagerAdapter
    {
         private static readonly ILog Logger = LogManager.Get(typeof (SwapFragmentsAdapter));
        private SwapComponentsActivity.SwapFragmentHolder[] pageFragments;
        //private SalesStatsFragment _mFragmentAtPos0;

        public SwapFragmentsAdapter(FragmentManager fm, SwapComponentsActivity.SwapFragmentHolder[] fragments) : base(fm)
        {
            pageFragments = fragments;
        }
       
        public override int Count
        {
            get
            {
                return pageFragments.Length;
            }
        }

        public override Fragment GetItem(int position)
        {
            Logger.Verbose(string.Format("Paging GetItem: {0}, Fragment: {1}", position, pageFragments[position].Fragment));

            return pageFragments[position].Fragment;
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            return new String(pageFragments[position].Label);
        }
    }
}