using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using Java.Lang;
using SalesApp.Core.Logging;
using Exception = System.Exception;

namespace SalesApp.Droid.Components.UIComponents.Hamburger
{

    class SalesAppHamburgerAdapter : ArrayAdapter<NavItem>
    {
        private static readonly ILog Logger = LogManager.Get(typeof(SalesAppHamburgerAdapter));
        private Context _mContext;
        private List<NavItem> _mNavItems;

        public SalesAppHamburgerAdapter(Context context, List<NavItem> navItems): base(context, Resource.Layout.layout_hamburger_item, navItems)
        {
            _mContext = context;
            _mNavItems = navItems;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            try
            {
                View view = convertView;
                DrawerItemHolder holder;

                if (position == 0)
                {
                    var lh = (LayoutInflater) _mContext.GetSystemService(Context.LayoutInflaterService);
                    view = lh.Inflate(Resource.Layout.layout_hamburger_header, parent, false);
                    return view;
                }

                if (view == null)
                {

                    var li = (LayoutInflater) _mContext.GetSystemService(Context.LayoutInflaterService);
                    view = li.Inflate(Resource.Layout.layout_hamburger_item, parent, false);

                    holder = new DrawerItemHolder
                    {
                        IconView = (ImageView) view.FindViewById(Resource.Id.drawer_item_icon),
                        TitleView = (TextView) view.FindViewById(Resource.Id.drawer_item_title)
                    };
                    
                    view.Tag = holder;
                }
                else
                {
                    holder = view.Tag as DrawerItemHolder;
                }

                NavItem item = _mNavItems[position];
                if (holder != null)
                {
                    holder.TitleView.SetText(item.Title, TextView.BufferType.Normal);
                    holder.IconView.SetImageResource(item.Icon);
                }

                return view;
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        class DrawerItemHolder : Object
        {
            public int Holderid;
            public TextView TitleView, SubtitleView;
            public ImageView IconView;
        }
    }
}