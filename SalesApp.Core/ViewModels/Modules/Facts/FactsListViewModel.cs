using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.Modules.Facts;

namespace SalesApp.Core.ViewModels.Modules.Facts
{
    public class FactsListViewModel : BaseViewModel
    {
        private ObservableCollection<Fact> _facts;
        private MvxCommand<Fact> _itemClickedCommand;

        public FactsListViewModel(IAssets assets)
        {
            string json = assets.GetAssetAsString("Facts/facts.json");
            List<Fact> facts = JsonConvert.DeserializeObject<List<Fact>>(json);

            this.Facts = new ObservableCollection<Fact>(facts);
        }

        public Action<Fact> LoadFactDetails { get; set; }

        public ObservableCollection<Fact> Facts
        {
            get
            {
                return _facts;
            }

            set
            {
                this.SetProperty(ref this._facts, value, () => this.Facts);
            }
        }

        public ICommand ItemClickCommand
        {
            get
            {
                this._itemClickedCommand = this._itemClickedCommand ?? new MvxCommand<Fact>(this.FactClicked);
                return this._itemClickedCommand;
            }
        }

        private void FactClicked(Fact fact)
        {
            this.LoadFactDetails(fact);
        }
    }
}