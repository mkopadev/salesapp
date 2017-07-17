using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Droid.Components.UIComponents.Hamburger;

namespace SalesApp.Droid.Components.UIComponents
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ActivityWithNavigation : ActivityBase2
    {
        public static readonly ILog Logger = Resolver.Instance.Get<ILog>();

        private ListView _drawerList;
        private DrawerToggle _drawerToggle;
        private DrawerLayout _drawerLayout;

        public DrawerToggle MenuDrawerToggle
        {
            get
            {
                return this._drawerToggle;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Logger.Initialize(this.GetType().FullName);
            base.OnCreate(savedInstanceState);
        }

        /// <summary>
        /// This method is called when the drawer is opened, it can be overriden in a implementing class if needed.
        /// </summary>
        protected virtual void OnDrawerOpened()
        {
        }

        /// <summary>
        /// This method is called when the drawer is closed, it can be overriden in a implementing class if needed.
        /// </summary>
        /// <param name="view"></param>
        protected virtual void OnDrawerClosed(View view)
        {
        }

        protected override void OnResume()
        {
            base.OnResume();
            SetupDrawer();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            
            _drawerToggle.OnConfigurationChanged(newConfig);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (_drawerToggle.OnOptionsItemSelected(item))
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private async Task AddDrawerItems(List<NavItem> items)
        {
            try
            {
                SalesAppHamburgerAdapter hamburgerAdapter = null;
                await Task.Run(() =>
                {
                    hamburgerAdapter = new SalesAppHamburgerAdapter(this, items);
                });

                RunOnUiThread (() => { _drawerList.Adapter = hamburgerAdapter; });

            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                throw;
            }
        }

        public void SetupDrawer()
        {
            _drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_navigation);
            _drawerList = FindViewById<ListView>(Resource.Id.drawer_list);

            _drawerToggle = new DrawerToggle(this, _drawerLayout, Resource.String.drawer_open,
                                Resource.String.drawer_close);
            _drawerToggle.DrawerIndicatorEnabled = true;

            if (_drawerLayout != null)
            {
                //_drawerLayout.SetScrimColor(Color.DimGray);
                _drawerLayout.SetDrawerListener(_drawerToggle);
            }

            EnableHomeButton();
            
            _drawerToggle.SyncState();
           
            Task.Run
                (
                    async () =>
                    {
                        await AddDrawerItems(await SalesAppHamburger.GetDefaultItems());
                        
                    }
                );

            _drawerList.ItemClick += (sender, args) => SelectItem(sender as ListView, args);
        }

        private void SelectItem(ListView sender, AdapterView.ItemClickEventArgs args)
        {
            // find the item clicked
            SalesAppHamburgerAdapter hamburgerAdapter = (SalesAppHamburgerAdapter) _drawerList.Adapter;

            NavItem item = hamburgerAdapter.GetItem(args.Position);

            // close the drawer
            _drawerLayout.CloseDrawer((int) GravityFlags.Left);

            // start the target
            if (item.Target != null && ( (item.Target.IsSubclassOf(typeof(ActivityBase))) || (item.Target.IsSubclassOf(typeof(ActivityBase2)))))
            {
                // if already on target screen, ignore
                if (GetType() == item.Target)
                    return;

                // start the target activity
                var intent = new Intent(this, item.Target);
                intent.AddFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
                
                // if on Welcome Activity, do not Finish the activity
                if (GetType() != typeof(HomeView))
                    Finish();
            }
        }

        /// <summary>
        /// This class takes care of the default actions for the navigation drawer toggle.
        /// </summary>
        public class DrawerToggle : ActionBarDrawerToggle
        {
            private readonly ActivityWithNavigation _owner;

            public DrawerToggle(ActivityWithNavigation activity, DrawerLayout layout, int openRes, int closeRes) : base(activity, layout, openRes, closeRes)
            {
                _owner = activity;
            }

            public override void OnDrawerOpened(View drawerView)
            {
                base.OnDrawerOpened(drawerView);
                _owner.InvalidateOptionsMenu();
                _owner.OnDrawerOpened();
            }

            public override void OnDrawerClosed(View view)
            {
                base.OnDrawerClosed(view);
                _owner.InvalidateOptionsMenu();
                _owner.OnDrawerClosed(view);
            }
        }
    }
}