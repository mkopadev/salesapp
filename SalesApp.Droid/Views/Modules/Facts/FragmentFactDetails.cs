using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.Modules.Facts;
using SalesApp.Core.ViewModels.Modules.Facts;

namespace SalesApp.Droid.Views.Modules.Facts
{
    public class FragmentFactDetails : ModuleFragmentBase, IPreviousNavigator
    {
        public const string FactDetailsBundleKey = "FactDetailsBundleKey";

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle inState)
        {
            base.OnCreateView(inflater, container, inState);
            View view = this.BindingInflate(Resource.Layout.fragment_fact_details, null);

            FactDetailsViewModel viewModel = new FactDetailsViewModel();

            if (this.Arguments != null)
            {
                string json = this.Arguments.GetString(FactDetailsBundleKey);

                Fact fact = JsonConvert.DeserializeObject<Fact>(json);
                viewModel.Fact = fact;
                this.FragmentLoadStateListener.TitleChanged(fact.Title);
            }

            this.ViewModel = viewModel;

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
            this.FragmentLoadStateListener.RequestOrintation(ScreenOrientation.Nosensor);
        }

        public bool Previous()
        {
            // load fact list fragment
            Fragment fragment = new FragmentFactsList();
            this.Activity.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.main_content, fragment, ModulesView.ModuleDetailFragmentTag).Commit();

            return true;
        }
    }
}