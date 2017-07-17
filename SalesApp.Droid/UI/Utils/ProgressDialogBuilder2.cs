using Android.Support.V4.App;

namespace SalesApp.Droid.UI.Utils
{
    public class ProgressDialogBuilder2
    {
        private static string Message { get; set; }

        private static string Title { get; set; }

        public static ProgressDialogBuilder2 Instance = new ProgressDialogBuilder2();

        private ProgressDialogBuilder2()
        {

        }

        public ProgressDialogBuilder2 SetText(string message, string title = null)
        {
            Message = message;
            Title = title;
            return this;
        }

        private ProgressDialogFragement alert;
        public void show(FragmentManager fm)
        {
            alert = ProgressDialogFragement.NewInstance(Message, Title);
            alert.Show(fm, "Dialog");
        }

        public void Dismiss()
        {
            alert.Dismiss();
        }
    }
}