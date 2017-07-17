using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Java.Lang;
using SalesApp.Droid.Views.TicketList.Customer;
using SalesApp.Droid.Views.TicketList.Dsr;

namespace SalesApp.Droid.Views.TicketList
{
    /// <summary>
    /// Pager adapter for the ticket list pager
    /// </summary>
    public class TicketListPagerAdapter : FragmentPagerAdapter
    {
        /// <summary>
        /// Represents the position of the customer tickets fragment
        /// </summary>
        private const int CustomerTicketsTab = 0;

        /// <summary>
        /// Represents the position of the DSR tickets fragment
        /// </summary>
        private const int MyTicketsTab = 1;

        /// <summary>
        /// Context to use for retrieving android resources
        /// </summary>
        private Context _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketListPagerAdapter"/> class
        /// </summary>
        /// <param name="fm">The fragment manager</param>
        public TicketListPagerAdapter(FragmentManager fm, Context context) : base(fm)
        {
            this._context = context;
        }

        /// <summary>
        /// Gets the number of pages (tabs)
        /// </summary>
        public override int Count
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        /// Returns a fragment given a position
        /// </summary>
        /// <param name="position">The position</param>
        /// <returns>Returns the fragment</returns>
        public override Fragment GetItem(int position)
        {
            Bundle bundle = new Bundle();
            bundle.PutBoolean(TicketFragmentBase.HasSnackBarBundleKey, true);
            Fragment fragment = null;

            switch (position)
            {
                case CustomerTicketsTab:
                    fragment =  new CustomerTicketsFragment();
                    bundle.PutBoolean(CustomerTicketsFragment.AutoLoadBundleKey, true);
                    break;
                case MyTicketsTab:
                    fragment = new DsrTicketsFragment();
                    break;
            }

            if (fragment == null)
            {
                throw new IllegalArgumentException(string.Format("I, {0} dont know what fragment to return for page {1}", this.GetType().FullName, position));
            }

            fragment.Arguments = bundle;
            return fragment;
        }

        /// <summary>
        /// Returns the title to show for the given tab
        /// </summary>
        /// <param name="position">The tabs position</param>
        /// <returns>The title</returns>
        public override ICharSequence GetPageTitleFormatted(int position)
        {
            string customerTicketsTitle = this._context.GetString(Resource.String.ticket_list_customer_tickets);
            string myTicketsTitle = this._context.GetString(Resource.String.ticket_list_my_tickets);

            switch (position)
            {
                case CustomerTicketsTab:
                    return new String(customerTicketsTitle);
                case MyTicketsTab:
                    return new String(myTicketsTitle);
            }

            return base.GetPageTitleFormatted(position);
        }
    }
}