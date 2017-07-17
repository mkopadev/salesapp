using Android.App;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.DAL;
using Message = SalesApp.Core.BL.Message;

namespace SalesApp.Droid
{
    [Activity(Label = "Notification Details")]
    public class BroadcastMessageDetails : Activity
    {
        BroadcastMessageRepository messagesRepository = new BroadcastMessageRepository();
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.layout_message_detail);

            string data = Intent.GetStringExtra("selected_message");

            if(!string.IsNullOrEmpty(data))
            {
                var message = JsonConvert.DeserializeObject<Message>(data);
                var subjectTextField = FindViewById<TextView>(Resource.Id.textName).Text = message.Subject;
                var messageTextField = FindViewById<TextView>(Resource.Id.textMessageText).Text = message.Body;
                var fromTextField = FindViewById<TextView>(Resource.Id.messageSender).Text = "From: " + message.From;
                var dateSentTextField = FindViewById<TextView>(Resource.Id.dateSent).Text = "Sent on: " + message.MessageDate.ToString("d MMM yyyy HH:mm");
            }
        }

        public override void OnBackPressed()
        {
            if (SharedFields.BroadcastMessageListAdapter != null)
                SharedFields.BroadcastMessageListAdapter.NotifyDataSetChanged();

            base.OnBackPressed();
        }
    }
}