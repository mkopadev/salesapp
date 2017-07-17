using System;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;

namespace SalesApp.Core.ViewModels.Commissions
{
    public class CommissionsViewModel : BaseViewModel
    {
        private MvxCommand previousCommand;

        public DateTime CurrentMonth { get; private set; }

        public Action PrevAction { get; set; }

        public ICommand PreviousCommand
        {
            get
            {
                this.previousCommand = this.previousCommand ?? new MvxCommand(this.PreviousButtonClick);
                return this.previousCommand;
            }
        }

        private void PreviousButtonClick()
        {
            if (this.PrevAction == null)
            {
                throw new NullReferenceException(string.Format("I, {0} dont know how to do with the button click.", this.GetType().FullName));
            }

            this.PrevAction();
        }
    }
}