using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.BL.Models.TicketList;
using SalesApp.Core.Enums.TicketList;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.UI.Stats;

namespace SalesApp.Droid.Views.TicketList.Customer
{
    /// <summary>
    /// Class shows the list of customer tickets
    /// </summary>
    public class CustomerTicketsFragment : TicketFragmentBase, ISwipeRefreshFragment
    {
        /// <summary>
        /// Key to use for the customer that we are showing
        /// </summary>
        public const string CustomerBundleKey = "CustomerBundleKey";

        // <summary>
        /// Key to use for the auto-load
        /// </summary>
        public const string AutoLoadBundleKey = "AutoLoadBundleKey";

        private bool _autoLoad;

        /// <summary>
        /// Fetch new and updated tickets from the server
        /// </summary>
        /// <param name="fetchRemote">Whether to fetch remote first</param>
        /// <returns>An empty task</returns>
        public async Task SwipeRefresh(bool fetchRemote = false)
        {
            await this.vm.FetchTickets(fetchRemote);
        }

        /// <summary>
        /// Create and return this fragment's view
        /// </summary>
        /// <param name="inflater">The inflator</param>
        /// <param name="container">The container view</param>
        /// <param name="inState">The saved state if any</param>
        /// <returns>The inflated view</returns>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle inState)
        {
            base.OnCreateView(inflater, container, inState);
            this.vm.ItemClickAction = this.ListItemClicked;
            this.vm.TicketType = TicketType.Customer;
            this.vm.Customer = this.GetArgument<CustomerSearchResult>(CustomerBundleKey);
            this._autoLoad = this.Arguments.GetBoolean(AutoLoadBundleKey);

            if (this._autoLoad)
            {
                this.vm.FetchTickets();
            }


            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("Customer Ticket");

            return this.FragmentView;
        }

        /// <summary>
        /// Called when an item has been selected from the list
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event args</param>
        private void ListItemClicked(AbstractTicketBase item)
        {
            Intent intent = new Intent(this.Activity, typeof(TicketDetailView));
            intent.PutExtra(TicketDetailView.TicketBundleKey, JsonConvert.SerializeObject(item));
            intent.PutExtra(TicketDetailView.TicketTypeBundleKey, TicketDetailView.TicketTypeCustomer);

            this.StartActivity(intent);
        }
    }
}