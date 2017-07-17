using Android.Support.Design.Widget;

namespace SalesApp.Droid.Views
{
    public class BindableSnackBar
    {
        private bool _visible;
        private Snackbar _snackbar;

        public BindableSnackBar(Snackbar snackbar)
        {
            this._snackbar = snackbar;
        }

        public bool Visible
        {
            get
            {
                return this._visible;
            }

            set
            {
                this._visible = value;
                if (value)
                {
                    // show
                    this._snackbar.Show();
                }
                else
                {
                    // hide
                    this._snackbar.Dismiss();
                }
            }
        }
    }
}