using Android.App;
using Android.Content;
using Android.Net;
using Android.Views;
using Android.Views.InputMethods;

namespace SalesApp.Droid
{
	public class BaseView
	{
		protected Context context;

		public BaseView (Context context)
		{
			this.context = context;
		}

		public void DisplayNetworkRequiredAlert()
		{
			// Unable to log in as no network service
			var connectivityDialog = new AlertDialog.Builder(context);
			connectivityDialog.SetMessage(context.Resources.GetString(Resource.String.network_connection_required));
			connectivityDialog.SetNegativeButton(context.Resources.GetString(Resource.String.ok), delegate { });

			// Show the alert dialog to the user and wait for response.
			connectivityDialog.Show();
		}

		public void DisplayUnsuccessfulRetryAlert(string actionDescription)
		{
			var connectivityDialog = new AlertDialog.Builder(context);
			connectivityDialog.SetMessage(string.Format(context.Resources.GetString(Resource.String.action_unsuccessful_retry), actionDescription));
			connectivityDialog.SetNegativeButton(context.Resources.GetString(Resource.String.cancel), delegate { ;
				((Activity)context).Finish();
			});
			connectivityDialog.SetNeutralButton(context.Resources.GetString(Resource.String.retry), delegate {});
			// Show the alert dialog to the user and wait for response.
			connectivityDialog.Show();
		}

        /// <summary>
        /// Returns true if connection to the network exists or false if it doesn't
        /// </summary>
        public bool ConnectedToNetwork
        {
            get
            {
                var connMan = (ConnectivityManager)context.ApplicationContext.GetSystemService(Activity.ConnectivityService);
                return connMan.ActiveNetworkInfo != null && connMan.ActiveNetworkInfo.IsConnected;
            }
        }

        /// <summary>
        /// Attempts to force the keyboard to be displayed.
        /// </summary>
        /// <param name="focusedView">The UI element in focus that the keyboard should send keypresses to</param>
        public void ShowKeyboard(View focusedView)
        {
            InputMethodManager imm = (InputMethodManager)context.GetSystemService(Activity.InputMethodService);
            imm.ShowSoftInput(focusedView, ShowFlags.Forced);
            imm.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);
        }
	}
}

