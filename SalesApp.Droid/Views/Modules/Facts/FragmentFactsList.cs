using Android.Content.PM;
using Android.OS;
using Android.Views;
using MvvmCross.Binding.Droid.BindingContext;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.Modules.Facts;
using SalesApp.Core.ViewModels.Modules.Facts;
using SalesApp.Droid.Services.GAnalytics;

namespace SalesApp.Droid.Views.Modules.Facts
{
    public class FragmentFactsList : ModuleFragmentBase
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle inState)
        {
            base.OnCreateView(inflater, container, inState);
            View view = this.BindingInflate(Resource.Layout.fragment_facts_list, null);

            IAssets assets = new AndroidAssets(this.Activity);
            FactsListViewModel viewModel = new FactsListViewModel(assets) { LoadFactDetails = ShowDetails };
            this.ViewModel = viewModel;

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
            string title = this.GetString(Resource.String.module_facts);
            this.FragmentLoadStateListener.TitleChanged(title);
            this.FragmentLoadStateListener.RequestOrintation(ScreenOrientation.Nosensor);
        }

        private void ShowDetails(Fact fact)
        {
            FragmentFactDetails fragment = new FragmentFactDetails();
            Bundle bundle = new Bundle();
            bundle.PutString(FragmentFactDetails.FactDetailsBundleKey, JsonConvert.SerializeObject(fact));
            fragment.Arguments = bundle;

            GaTracking(fact);

            this.Activity.SupportFragmentManager.BeginTransaction().Replace(Resource.Id.main_content, fragment, ModulesView.ModuleDetailFragmentTag).Commit();
            this.FragmentLoadStateListener.TitleChanged(fact.Title);
        }

        private void GaTracking(Fact fact)
        {
            string category = Activity.GetString(Resource.String.module_facts);
            string label = Activity.GetString(Resource.String.facts_label);
            string action = fact.Title;

            GoogleAnalyticService.Instance.TrackEvent(category, label, action);
        }
    }
}