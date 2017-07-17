using Android.Content;
using Newtonsoft.Json;

namespace SalesApp.Droid.People.Prospects
{
    public class ProspectDetailsHelper
    {
        public Intent GetPropsectDetailIntent(Context context, ProspectItem prospectListItem)
        {
            Intent intent = new Intent(context, typeof(ProspectDetailActivity));
            intent.PutExtra(ProspectDetailActivity.ExistingProspectId, JsonConvert.SerializeObject(prospectListItem));
            return intent;
        }
    }
}