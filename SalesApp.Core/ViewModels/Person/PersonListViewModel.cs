namespace SalesApp.Core.ViewModels.Person
{
    /// <summary>
    /// Base view model to be used with lists of models of type person
    /// </summary>
    public abstract class PersonListViewModel : BaseViewModel
    {
        private bool _toolBarVisible;

        public bool ToolBarVisible
        {
            get { return this._toolBarVisible; }
            set { this.SetProperty(ref this._toolBarVisible, value, () => this.ToolBarVisible); }
        }

        protected PersonListViewModel()
        {
            this.ToolBarVisible = true;
        }
    }
}