using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.TicketList;
using SalesApp.Core.Enums.TicketList;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.UI.Stats;

namespace SalesApp.Droid.Views.TicketList.Dsr
{
    /// <summary>
    /// Class shows the list of DSR tickets
    /// </summary>
    public class DsrTicketsFragment : TicketFragmentBase, ISwipeRefreshFragment
    {
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
            this.vm.TicketType = TicketType.Dsr;
            this.vm.FetchTickets();


            // App trackking
            GoogleAnalyticService.Instance.TrackScreen("DSR Ticket");

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
            intent.PutExtra(TicketDetailView.TicketTypeBundleKey, TicketDetailView.TicketTypeDsr);

            this.StartActivity(intent);
        }
    }
}