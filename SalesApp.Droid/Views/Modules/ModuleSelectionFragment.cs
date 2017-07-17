using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V4;
using SalesApp.Core.BL.Models.Modules;
using SalesApp.Core.ViewModels.Modules;
using SalesApp.Droid.Services.GAnalytics;
using SalesApp.Droid.Views.Modules.Calculator;
using SalesApp.Droid.Views.Modules.Facts;
using SalesApp.Droid.Views.Modules.Videos;
using Fragment = Android.Support.V4.App.Fragment;

namespace SalesApp.Droid.Views.Modules
{
    public class ModuleSelectionFragment : MvxFragment
    {
        private IFragmentLoadStateListener _fragmentLoadStateListener;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            this.ViewModel = new ModuleSelectionViewModel { LoadModule = this.LoadModule };
            BindableActionBar bab = new BindableActionBar(this._fragmentLoadStateListener);

            var set = this.CreateBindingSet<ModuleSelectionFragment, ModuleSelectionViewModel>();
            set.Bind(bab).For(obj => obj.ActionBarTitle).To(x => x.ActionBarTitle);
            set.Apply();

            return this.BindingInflate(Resource.Layout.fragment_module_selection, null);
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            this._fragmentLoadStateListener = activity as IFragmentLoadStateListener;
        }

        public override void OnResume()
        {
            base.OnResume();
            this._fragmentLoadStateListener.IndicatorStateChanged(true);
            string newTitle = GetString(Resource.String.modules_screen_title);
            this._fragmentLoadStateListener.TitleChanged(newTitle);
            this._fragmentLoadStateListener.RequestOrintation(ScreenOrientation.Nosensor);
        }

        private void LoadModule(Module module)
        {
            Fragment fragment = null;
            string label = string.Empty;
            string moduleAction = string.Empty;
            if (module.ModuleName.ToLower().Contains("calculator"))
            {
                fragment = new CalculatorModuleFragment();
                label = Activity.GetString(Resource.String.calculator_label);
                moduleAction = Activity.GetString(Resource.String.calculator_action);
            }
            else if (module.ModuleName.ToLower().Contains("videos"))
            {
                fragment = new VideoComponentFragment();
                label = Activity.GetString(Resource.String.video_label);
                moduleAction = Activity.GetString(Resource.String.video_action);
            }
            else if (module.ModuleName.ToLower().Contains("facts"))
            {
                fragment = new FragmentFactsList();
                label = Activity.GetString(Resource.String.facts_label);
                moduleAction = Activity.GetString(Resource.String.facts_action);
            }

            if (fragment == null)
            {
                return;
            }


            GoogleAnalyticService.Instance.TrackEvent(GetString(Resource.String.module_screen),
                moduleAction, label);

            this.Activity.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.main_content, fragment, ModulesView.ModuleDetailFragmentTag).Commit();
        }
    }
}