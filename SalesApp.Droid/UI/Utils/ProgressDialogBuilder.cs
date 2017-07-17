using Android.App;
using Android.Content;

namespace SalesApp.Droid.UI.Utils
{

    public class ProgressDialogBuilder
    {
        private Context _context;
        private ProgressDialog progressDialog;

        private string Message { get; set; }

        private string Title { get; set; }

        /*public ProgressDialogBuilder Instance
        {
            get { return new ProgressDialogBuilder(); }
        }

        private bool Cancellable { get; set; }

        public ProgressDialogBuilder()
        {
        }*/

        public ProgressDialogBuilder SetText(string title, string message)
        {
            Title = title;
            Message = message;
            return this;
        }

        public ProgressDialogBuilder SetMessage(string message)
        {
            Message = message;
            return this;
        }

        public ProgressDialog Show(Context context, bool cancel = false)
        {
            progressDialog = new ProgressDialog(context);
            progressDialog.SetMessage(Message);
            if (!string.IsNullOrEmpty(Title))
            {
                progressDialog.SetTitle(Title);
            }

            progressDialog.Indeterminate = cancel;

            progressDialog.Show();
            return progressDialog;
        }

        public void Dismiss()
        {
            progressDialog.Dismiss();
        }
    }
}