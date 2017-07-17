using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.Droid.BindingContext;
using Newtonsoft.Json;
using SalesApp.Core.Api.Chama;
using SalesApp.Core.BL.Controllers.Chama;
using SalesApp.Core.BL.Models.Chama;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Services.Settings;
using SalesApp.Core.ViewModels.Chama;
using SalesApp.Droid.Enums;
using SalesApp.Droid.People.UnifiedUi.Customer;
using SalesApp.Droid.People.UnifiedUi.Prospect;
using SalesApp.Droid.UI.Wizardry;
using SalesApp.Droid.Views;
using SalesApp.Droid.Views.Chama;

namespace SalesApp.Droid.People.UnifiedUi
{
    public abstract class FragmentGroupSelection : WizardStepFragment
    {
        private bool _enableNextButton;
        private bool _showSelctionUi;

        private Group _newGroup;

        private const string DialogFragmentTag = "DialogFragmentTag";
        private const string ViewModelBundleKey = "ViewModelBundleKey";
        private SpinnerDialogFragment _dialogFragment;

        private GroupSelectionViewModel _vm;

        protected abstract Lead Lead { get; }

        private string ShowToast
        {
            get { return null; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    Toast.MakeText(Activity, value, ToastLength.Long).Show();
            }
        }

        public bool ShowSelectionUi
        {
            get { return this._showSelctionUi; }

            set
            {
                if (value)
                {
                    this._dialogFragment = FragmentManager.FindFragmentByTag(DialogFragmentTag) as SpinnerDialogFragment;

                    if (this._dialogFragment == null)
                    {
                        this._dialogFragment = new SpinnerDialogFragment { ViewModel = this.ViewModel };
                        this._dialogFragment.Show(this.Activity.SupportFragmentManager, DialogFragmentTag);
                    }
                    else
                    {
                        this._dialogFragment.ViewModel = this.ViewModel;
                    }
                }
                else
                {
                    if (this._dialogFragment == null)
                    {
                        return;
                    }

                    this._dialogFragment.Dismiss();
                }

                this._showSelctionUi = value;
            }
        }

        public bool EnableNextButton
        {
            get
            {
                return this._enableNextButton;
            }

            set
            {
                this._enableNextButton = value;
                this.WizardActivity.ButtonNextEnabled = value;
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle inState)
        {
            base.OnCreateView(inflater, container, inState);

            if (inState == null)
            {
                var relativePath = "v1/chama/groups";
                _vm = new GroupSelectionViewModel(new ChamaController(), new ChamaApi(relativePath));
                _vm.InitialLoad(this.Lead);
            }
            else
            {
                string vmJson = inState.GetString(ViewModelBundleKey);
                _vm = JsonConvert.DeserializeObject<GroupSelectionViewModel>(vmJson);
            }

            this.ViewModel = _vm;
            this.WizardActivity.ButtonNext.Visibility = ViewStates.Visible;
            this.WizardActivity.ButtonNextEnabled = false;
            this.FragmentView = this.BindingInflate(Resource.Layout.fragment_group_selection, container, false);

            BindableProgressDialog progressDialog = new BindableProgressDialog(this.Activity);

            var set = this.CreateBindingSet<FragmentGroupSelection, GroupSelectionViewModel>();
            set.Bind(this).For(v => v.ShowSelectionUi).To(vm => vm.ShowSelectionUi);
            set.Bind(this).For(v => v.EnableNextButton).To(vm => vm.EnableNextButton);
            set.Bind(progressDialog).For(target => target.Visible).To(source => source.IsBusy);
            set.Bind(progressDialog).For(target => target.Message).To(source => source.ProgressDialogMessage);
            set.Bind(this).For(target => target.ShowToast).To(source => source.SuccessMessage);
            set.Apply();

            return this.FragmentView;
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            GroupSelectionViewModel vm = (GroupSelectionViewModel)this.ViewModel;

            vm.FilterText = string.Empty;

            string vmJson = JsonConvert.SerializeObject(this.ViewModel);
            outState.PutString(ViewModelBundleKey, vmJson);
        }

        public override string GetData()
        {
            this.Lead.GroupInfo = _vm.SelectedGroups;
            return JsonConvert.SerializeObject(this.Lead);
        }

        public override Type GetNextFragment()
        {
            switch (this.WizardActivity.WizardType)
            {
                case WizardTypes.CustomerRegistration:
                    var photoFeatureEnabled = Settings.Instance.PhotoFeatureEnabled;

                    if (photoFeatureEnabled == 1)
                    {
                        return typeof(CustomerPhotoFragment);
                    }

                    return typeof(FragmentCustomerConfirmationScreen);

                case WizardTypes.ProspectRegistration:
                    return typeof(FragmentScoreProspect);
            }

            return null;
        }

        public override int StepTitle
        {
            get
            {
                return 0;
            }
        }

        public override bool BeforeGoNext()
        {
            return true;
        }

        public override bool Validate()
        {
            return true;
        }
    }
}