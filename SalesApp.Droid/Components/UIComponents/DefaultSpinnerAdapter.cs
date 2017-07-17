using System.Linq;
using Android.Content;
using SalesApp.Droid.UI;

namespace SalesApp.Droid.Components.UIComponents
{
    public class DefaultSpinnerAdapter
    {
        internal HintAdapter GetAdapter(string[] content, Context ctx)
        {
            HintAdapter adapter = new HintAdapter(ctx, content.ToList());

            // adapter.SetDropDownViewResource(Resource.Layout.spinner_default_dropdown);
            return adapter;
        }
    }
}