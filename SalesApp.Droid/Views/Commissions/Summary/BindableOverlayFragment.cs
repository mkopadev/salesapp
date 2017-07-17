using Android.Support.V4.App;

namespace SalesApp.Droid.Views.Commissions.Summary
{
    public class BindableOverlayFragment
    {
        private Fragment _fragment;
        private FragmentActivity _context;
        private int _container;

        private bool _visible;

        public BindableOverlayFragment(Fragment fragment, FragmentActivity activity, int container)
        {
            this._fragment = fragment;
            this._context = activity;
            this._container = container;
        }

        public bool Visible
        {
            get
            {
                return this._visible;
            }

            set
            {
                if (value && !this._visible)
                {
                    this._context.SupportFragmentManager.BeginTransaction()
                 .Replace(_container, _fragment, "OverlayFragment")
                         .Commit();
                }
            }
        }
    }
}