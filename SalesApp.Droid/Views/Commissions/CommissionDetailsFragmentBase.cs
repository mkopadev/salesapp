namespace SalesApp.Droid.Views.Commissions
{
    public class CommissionDetailsFragmentBase : CommissionsFragmentBase
    {
        public override void OnResume()
        {
            base.OnResume();
            this.parentActivity.MenuDrawerToggle.DrawerIndicatorEnabled = false;
            this.parentActivity.SwipeRefreshLayout.Enabled = false;
        }
    }
}