using Android.App;
using Android.Content;
using Android.Net;

namespace SalesApp.Droid.Services.Phone
{
    public class CallService
    {
        public void Dial(string phoneNumber, Activity activity)
        {
            var uri = Uri.Parse("tel:" + phoneNumber.Trim());
            Intent intent = new Intent(Intent.ActionDial, uri);
            activity.StartActivity(intent);
        }
    }
}