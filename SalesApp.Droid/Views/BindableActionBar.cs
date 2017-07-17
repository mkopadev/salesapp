using SalesApp.Droid.Views.Modules;

namespace SalesApp.Droid.Views
{
    public class BindableActionBar
    {
        private IFragmentLoadStateListener _listener;
        private string _title;

        public BindableActionBar(IFragmentLoadStateListener listener)
        {
            this._listener = listener;
        }

        public string ActionBarTitle
        {
            get
            {
                return this._title;
            }

            set
            {
                this._listener.TitleChanged(value);
            }
        }
    }
}