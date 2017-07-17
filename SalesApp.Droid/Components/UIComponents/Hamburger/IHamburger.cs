using System.Collections.Generic;
using Android.Widget;

namespace SalesApp.Droid.Components.UIComponents.Hamburger
{
    public interface IHamburger
    {

        void AddDrawerItems(List<NavItem> items, ListView drawerList);
          void SetupDrawer();
    }
}