using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.TicketList;
using SalesApp.Core.ViewModels.TicketList;
using SalesApp.Droid.Framework;

namespace SalesApp.Droid.Views.TicketList
{
    /// <summary>
    /// This is the screen that shows the log files to the user. The user can select a file and either delete it or share it.
    /// </summary>
    [Activity(Label = "@string/ticket_detail_screen_title", NoHistory = false, ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(TicketListView), Theme = "@style/AppTheme.SmallToolbar")]
    public class TicketDetailView : MvxViewBase<TicketDetailViewModel>
    {
        /// <summary>
        /// Bundle key for the ticket GUID
        /// </summary>
        public const string TicketBundleKey = "TicketBundleKey";

        /// <summary>
        /// Bundle key for the ticket type
        /// </summary>
        public const string TicketTypeBundleKey = "TicketTypeBundleKey";

        /// <summary>
        /// DSR ticket type
        /// </summary>
        public const string TicketTypeDsr = "Dsr";

        /// <summary>
        /// Customer ticket type
        /// </summary>
        public const string TicketTypeCustomer = "Customer";

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    /*Intent intent = new Intent(this, typeof(TicketListView));
                    StartActivity(intent);*/
                    this.Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        /// <summary>
        /// Android's on create method
        /// </summary>
        /// <param name="bundle">Bundle to use in re-creating the activity</param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.ticket_detail_layout);

            this.AddToolbar(Resource.String.ticket_detail_screen_title, true);

            string ticketJson = this.Intent.GetStringExtra(TicketBundleKey);
            string ticketType = this.Intent.GetStringExtra(TicketTypeBundleKey);

            this.ViewModel.Ticket = JsonConvert.DeserializeObject<CustomerTicket>(ticketJson);

            if (ticketType == TicketTypeCustomer)
            {
                this.ViewModel.ProductVisible = true;
                this.ViewModel.SecondLabel = this.GetString(Resource.String.ticket_list_name);
            }

            if (ticketType == TicketTypeDsr)
            {
                this.ViewModel.ProductVisible = false;
                this.ViewModel.SecondLabel = this.GetString(Resource.String.ticket_list_issue);

                // uppdate the binding
                TextView issue = this.FindViewById<TextView>(Resource.Id.name_text);
                var set = this.CreateBindingSet<TicketDetailView, TicketDetailViewModel>();
                
                set.Bind(issue).To("Ticket.Issue").TwoWay();

                set.Apply();
            }
        }
    }
}