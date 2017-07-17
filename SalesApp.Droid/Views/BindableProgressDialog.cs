using Android.Content;
using SalesApp.Droid.UI.Utils;

namespace SalesApp.Droid.Views
{
    public class BindableProgressDialog
    {
        private bool _visible;
        private string _message;

        private ProgressDialogBuilder _dialog;
        private Context _context;

        public BindableProgressDialog(Context context)
        {
            this._dialog = new ProgressDialogBuilder();
            this._context = context;
        }

        public bool Visible
        {
            get
            {
                return this._visible;
            }

            set
            {
                if (value)
                {
                    this._dialog.Show(this._context);
                }
                else
                {
                    if (this.Visible)
                    {
                        this._dialog.Dismiss();
                    }
                }

                this._visible = value;
            }
        }

        public string Message
        {
            get
            {
                return this._message;
            }

            set
            {
                this._dialog.SetMessage(value);
            }
        }
    }
}