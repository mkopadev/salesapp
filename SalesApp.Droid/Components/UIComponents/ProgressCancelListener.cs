using Android.Content;
using Java.Lang;

namespace SalesApp.Droid.Components.UIComponents
{
    public class ProgressCancelListener : Object, IDialogInterfaceOnCancelListener
    {
        public void OnCancel(IDialogInterface dialog)
        {
            if (dialog == null)
            {
                return;
            }

            dialog.Dismiss();
        }
    }
}