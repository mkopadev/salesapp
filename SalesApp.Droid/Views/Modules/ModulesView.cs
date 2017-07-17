using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using SalesApp.Core.ViewModels.Modules;
using SalesApp.Droid.Framework;
using SalesApp.Droid.Services.GAnalytics;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace SalesApp.Droid.Views.Modules
{
    [Activity(Theme = "@style/AppTheme.NoActionBar", NoHistory = false)]
    public class ModulesView : MvxNavigationViewBase<ModulesViewModel>, IFragmentLoadStateListener
    {
        private const string ModuleSelectionFragmentTag = "ModuleSelectionFragmentTag";
        public const string ModuleDetailFragmentTag = "ModuleDetailFragmentTag";
        private const string RegistrationDialogFragmentTag = "RegistrationDialogFragmentTag";

        /// <summary>
        /// Create an instance of this activity, optionally restore the state from <paramref name="savedState"/>
        /// </summary>
        /// <param name="savedState">The saved state to restore the activity from</param>
        protected override void OnCreate(Bundle savedState)
        {
            base.OnCreate(savedState);
            SetContentView(Resource.Layout.layout_modules);
            AddToolbar(Resource.String.modules_screen_title, true);

            TextView registrationBar = FindViewById<TextView>(Resource.Id.top_registration_bar);

            if (registrationBar != null)
            {
                registrationBar.Click += (sender, args) =>
                {
                    ShowRegistrationDialog();
                };
            }

            Fragment fragment = SupportFragmentManager.FindFragmentByTag(ModuleDetailFragmentTag);

            if (fragment == null)
            {
                ShowModuleSelectionScreen();
            }
            else
            {
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.main_content, fragment, ModuleDetailFragmentTag).Commit();
            }

            // restore any dialogs
            if (savedState != null)
            {
                Fragment registrationDialog = this.SupportFragmentManager.GetFragment(savedState, RegistrationDialogFragmentTag);
                if (registrationDialog != null)
                {
                    // We need to reshow the dialog
                    this.ShowRegistrationDialog();
                }
            }

            GoogleAnalyticService.Instance.TrackEvent(GetString(Resource.String.module_screen),
                this.GetString(Resource.String.module_action), GetString(Resource.String.module_label));
        }

        private void ShowRegistrationDialog()
        {
            FragmentTransaction trans = GetFragmentManager().BeginTransaction();
            Fragment previousDialog = GetFragmentManager().FindFragmentByTag(RegistrationDialogFragmentTag);

            if (previousDialog != null)
            {
                trans.Remove(previousDialog);
            }

            DialogFragment newDiaolog = new RegistrationDialogFragment();
            newDiaolog.SetStyle(DialogFragment.StyleNoTitle, 0);
            newDiaolog.Show(trans, RegistrationDialogFragmentTag);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            Fragment registrationDialog = GetFragmentManager().FindFragmentByTag(RegistrationDialogFragmentTag);
            if (registrationDialog != null)
            {
                SupportFragmentManager.PutFragment(outState, RegistrationDialogFragmentTag, registrationDialog);
            }
        }

        public override void OnBackPressed()
        {
            bool showingDetails = !MenuDrawerToggle.DrawerIndicatorEnabled;

            if (showingDetails)
            {
                IPreviousNavigator fragment = SupportFragmentManager.FindFragmentByTag(ModuleDetailFragmentTag) as IPreviousNavigator;

                if (NavigateUp(fragment))
                {
                    return;
                }
                
                ShowModuleSelectionScreen();
                return;
            }

            base.OnBackPressed();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            bool showingDetails = !MenuDrawerToggle.DrawerIndicatorEnabled;
            if (item.ItemId == Android.Resource.Id.Home && showingDetails)
            {
                IPreviousNavigator navigator = SupportFragmentManager.FindFragmentByTag(ModuleDetailFragmentTag) as IPreviousNavigator;

                if (navigator == null || !navigator.Previous())
                {
                    ShowModuleSelectionScreen();
                    return true;
                }
            }

            return base.OnOptionsItemSelected(item);
        }

        private bool NavigateUp(IPreviousNavigator navigator)
        {
            if (navigator == null)
            {
                return false;
            }

            bool navigated = navigator.Previous();

            return navigated;
        }

        private void ShowModuleSelectionScreen()
        {
            // Load the modules selection screen
            var fragment = (ModuleSelectionFragment)SupportFragmentManager.FindFragmentByTag(ModuleSelectionFragmentTag);
            if (fragment == null)
            {
                fragment = new ModuleSelectionFragment();
            }
            
            ReplaceFragment(fragment, Resource.Id.main_content, ModuleSelectionFragmentTag);
            ViewModel.CanRegister = false;
        }

        public void TitleChanged(string newTitle)
        {
            SetScreenTitle(newTitle);
        }

        public void IndicatorStateChanged(bool newState)
        {
            if (MenuDrawerToggle == null)
            {
                return;
            }

            MenuDrawerToggle.DrawerIndicatorEnabled = newState;
        }

        public void CanRegisterChanged(bool canRegister)
        {
            if (ViewModel.CanRegister == canRegister)
            {
                return;
            }

            ViewModel.CanRegister = canRegister;
        }

        public void RequestOrintation(ScreenOrientation newOrientation)
        {
            RequestedOrientation = newOrientation;
        }
    }
}