using Android.App;
using Android.OS;
using MK.Solar.Components.UIComponents.List;
using Mkopa.Core.BL.Models.TicketList;

namespace MK.Solar.Tickets
{
    [Activity(Label = "Ticket List")]
    public class TicketListActivity : ListActivityBase<AbstractTicketBase>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
        }
    }
}