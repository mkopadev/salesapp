using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using SalesApp.Core.BL.Controllers.Security;
using SalesApp.Core.Enums.Security;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Droid.People.Customers;
using SalesApp.Droid.People.Prospects;
using SalesApp.Droid.UI.Stats;
using SalesApp.Droid.UI.Stats.Reporting;
using SalesApp.Droid.Views.Commissions;
using SalesApp.Droid.Views.Modules;
using SalesApp.Droid.Views.TicketList;

namespace SalesApp.Droid.Components.UIComponents.Hamburger
{
    public class SalesAppHamburger
    {
        private static Activity _activity;

        public SalesAppHamburger(Activity activity)
        {
            _activity = activity;
        }

        private static List<NavItem> _listItems = new List<NavItem>();
        
        public static async Task<List<NavItem>> GetDefaultItems()
        {
            _listItems.Clear();
            
            await AddItem(new NavItem(0, 0, null, Permissions.ActionHamburgerLogo));
            await AddItem(new NavItem(Resource.String.home, Resource.Drawable.home, typeof(HomeView), Permissions.ScreenAppHome));
            await AddItem(new NavItem(Resource.String.modules_screen_title, Resource.Drawable.resources, typeof(ModulesView), Permissions.Modules));
            await AddItem(new NavItem(Resource.String.prospect_list, Resource.Drawable.prospect_list, typeof(ProspectListView), Permissions.RegisterProspect));
            await AddItem(new NavItem(Resource.String.customer_list, Resource.Drawable.customer_list, typeof(CustomerListView), Permissions.RegisterCustomer));
            await AddItem(new NavItem(Resource.String.customer_status, Resource.Drawable.green_check, typeof(CustomerSearchActivity), Permissions.ScreenCheckCustomerStatus));
            await AddItem(new NavItem(Resource.String.stats, Resource.Drawable.stats, typeof(StatsView), Permissions.IndividualStats));
            await AddItem(new NavItem(Resource.String.reporting, Resource.Drawable.reporting, typeof(ReportingLevelStatsView), Permissions.ViewAggregatedStats));
            await AddItem(new NavItem(Resource.String.ticket_list_screen_title, Resource.Drawable.ticket_list, typeof(TicketListView), Permissions.TicketList));
            await AddItem(new NavItem(Resource.String.commissions, Resource.Drawable.commissions, typeof(CommissionsView), Permissions.Commissions));

            return _listItems;
        }

        public static async Task AddItem(NavItem item)
        {
            await AddItem
                (
                    item.Title,
                    item.Icon,
                    item.Target,
                    item.VisibilityPermission
                );
        }

        public static async Task AddItem(int title, int icon, Type target, Permissions visibilityPermission)
        {
            try
            {
                
                bool allowed = await PermissionsController.Instance.Allowed(visibilityPermission);
                if (!allowed)
                {
                    return;
                }

                _listItems.Add
                    (
                        new NavItem
                            (
                                title,
                                icon,
                                target,
                                visibilityPermission
                            )
                    );
            }
            catch (Exception exception)
            {
                Resolver.Instance.Get<ILog>().Error(exception);
                new ReusableScreens(_activity)
                    .ShowInfo
                    (
                        Resource.String.role_based_error_actionbar,
                        Resource.String.role_based_error_title,
                        Resource.String.role_based_error_message,
                        Resource.String.ok
                    );
            }
            
        }
    }

    public class NavItem
    {
        public int Title { get; set; }
        public int Icon { get; set; }

        public Type Target { get; set; }

        public Permissions VisibilityPermission { get; private set; }

        public NavItem(int title, int icon, Type target, Permissions visibilityPermission)
        {
            Title = title;
            Icon = icon;
            Target = target;
            VisibilityPermission = visibilityPermission;
        }
    }
}
