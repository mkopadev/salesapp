using Android.App;
using Android.OS;
using Android.Views;

namespace SalesApp.Droid.Adapters.Auth.ResetPin
{
    public class ResetPinWaitFragment : Fragment
    {
        
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.layout_progress, container, false);

            return view;
        }

        public interface ResetPinWaitFragmentListener
        {

            void onWaitSelected();
        }
        
    }
}