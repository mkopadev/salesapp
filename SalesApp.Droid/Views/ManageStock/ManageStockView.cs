using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using Newtonsoft.Json;
using SalesApp.Core.Services.ManageStock;
using SalesApp.Core.Services.Product;
using SalesApp.Core.ViewModels.ManageStock;
using SalesApp.Droid.Framework;
using Fragment = Android.Support.V4.App.Fragment;

namespace SalesApp.Droid.Views.ManageStock
{
    [Activity(NoHistory = false, ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(HomeView), Theme = "@style/AppTheme.SmallToolbar")]
    public class ManageStockView : MvxViewBase<ManageStockViewModel>
    {
        public const string MainContentFragmentTag = "MainContentFragmentTag";
        private const string ViewModelBundleKey = "ViewModelBundleKey";

        private Button _btnNext, _btnPrev;

        protected override void OnCreate(Bundle savedState)
        {
            // restore view model from the saved state
            if (savedState != null)
            {
                string json = savedState.GetString(ViewModelBundleKey);
                ManageStockViewModel viewModel = JsonConvert.DeserializeObject<ManageStockViewModel>(json);
                this.ViewModel = viewModel;
            }

            base.OnCreate(savedState);
            this.ViewModel.RemoteProductService = new RemoteProductService("StockAllocation/Products");
            this.ViewModel.DsrStockAllocationService = new DsrStockAllocationService();

            SetContentView(Resource.Layout.layout_stock_allocation);
            this.AddToolbar(Resource.String.manage_stock, true);

            _btnNext = FindViewById<Button>(Resource.Id.btnNext);
            _btnPrev = FindViewById<Button>(Resource.Id.btnPrev);

            _btnNext.Click += GoNextOnClick;
            _btnPrev.Click += GoPrevOnClick;

            Fragment mainFragment = this.SupportFragmentManager.FindFragmentByTag(MainContentFragmentTag);

            if (mainFragment == null)
            {
                mainFragment = new FragmentStockSelectTask();
            }

            this.SupportFragmentManager.BeginTransaction()
                    .Replace(Resource.Id.main_content, mainFragment, MainContentFragmentTag).Commit();

            string snackbarMessage = this.GetString(Resource.String.turn_on_net_to_manage_stock);

            View mainContentView = this.FindViewById(Resource.Id.main_content);
            Snackbar snackbar = Snackbar.Make(mainContentView, snackbarMessage, Snackbar.LengthIndefinite);
            BindableSnackBar bindableSnackBar = new BindableSnackBar(snackbar);

            var set = this.CreateBindingSet<ManageStockView, ManageStockViewModel>();
            set.Bind(bindableSnackBar).For(target => target.Visible).To(source => source.ShowNoInternetAlert);
            set.Apply();

        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            string json = JsonConvert.SerializeObject(this.ViewModel);

            outState.PutString(ViewModelBundleKey, json);
        }

        public override void OnBackPressed()
        {
            if (this.GoPrevious())
            {
                return;
            }

            base.OnBackPressed();
        }

        private void GoPrevOnClick(object sender, EventArgs eventArgs)
        {
            GoPrevious();
        }

        private bool GoPrevious()
        {
            Fragment frag = SupportFragmentManager.FindFragmentByTag(MainContentFragmentTag);
            IPreviousNavigator navigator = frag as IPreviousNavigator;

            if (navigator == null)
            {
                return false;
            }

            return navigator.Previous();
        }

        private void GoNextOnClick(object sender, EventArgs eventArgs)
        {
            Fragment frag = SupportFragmentManager.FindFragmentByTag(MainContentFragmentTag);
            INextNavigator navigator = frag as INextNavigator;

            if (navigator == null)
            {
                return;
            }

            navigator.Next();
        }
    }
}