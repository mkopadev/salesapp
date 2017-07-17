using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using SalesApp.Core.BL.Models.Modules;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.ViewModels.Modules
{
    public class ModuleSelectionViewModel : BaseViewModel
    {
        private ObservableCollection<Module> _modules;
        private IDeviceResource _deviceResource = Resolver.Instance.Get<IDeviceResource>();
        private MvxCommand<Module> _itemClickedCommand;
        private string _actionBarTitle;

        public Action<Module> LoadModule { get; set; }

        public ModuleSelectionViewModel()
        {
            this.Modules = new ObservableCollection<Module>
            {
                new Module { ModuleName = "Videos", ModuleIcon = this._deviceResource.VideosModuleIcon },
                new Module { ModuleName = "Facts", ModuleIcon = this._deviceResource.FactsModuleIcon },
                new Module { ModuleName = "Calculator", ModuleIcon = this._deviceResource.CalculatorModuleIcon }
            };
        }

        public string ActionBarTitle
        {
            get
            {
                return this._actionBarTitle;
            }

            set
            {
                this.SetProperty(ref this._actionBarTitle, value, () => this.ActionBarTitle);
            }
        }

        public ObservableCollection<Module> Modules
        {
            get
            {
                return this._modules;
            }

            set
            {
                this.SetProperty(ref this._modules, value, () => this.Modules);
            }
        }

        public ICommand ItemClickCommand
        {
            get
            {
                this._itemClickedCommand = this._itemClickedCommand ?? new MvxCommand<Module>(this.ModuleClicked);
                return this._itemClickedCommand;
            }
        }

        private void ModuleClicked(Module item)
        {
            this.LoadModule(item);
            this.ActionBarTitle = item.ModuleName;
        }
    }
}