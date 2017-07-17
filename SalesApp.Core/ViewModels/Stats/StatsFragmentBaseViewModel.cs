using System.Collections.ObjectModel;
using SalesApp.Core.Services.Stats;

namespace SalesApp.Core.ViewModels.Stats
{
    public abstract class StatsFragmentBaseViewModel : BaseViewModel
    {
        private ObservableCollection<Block> _summary;
        private ObservableCollection<string> _columns;

        protected StatsFragmentBaseViewModel()
        {
            this._summary = new ObservableCollection<Block>
            {
                new Block
                {
                    BottomValue = "-",
                    TopValue = "-",
                    Level = 0,
                    Caption = "-"
                },
                new Block
                {
                    BottomValue = "-",
                    TopValue = "-",
                    Level = 1,
                    Caption = "-"
                },
                new Block
                {
                    BottomValue = "-",
                    TopValue = "-",
                    Level = 2,
                    Caption = "-"
                }
            };
        }

        public ObservableCollection<string> Columns
        {
            get
            {
                return this._columns;
            }

            set
            {
                this.SetProperty(ref this._columns, value, () => this.Columns);
            }
        }

        public ObservableCollection<Block> Summary
        {
            get
            {
                return this._summary;
            }

            set
            {
                if (value == null || value.Count == 0)
                {
                    return;
                }

                this.SetProperty(ref this._summary, value, () => this.Summary);
            }
        }
    }
}
