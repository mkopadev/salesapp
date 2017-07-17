using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;

namespace SalesApp.Core.ViewModels.Logging
{
    /// <summary>
    /// This is the portable view model that contains the data and actions to support the logs view
    /// </summary>
    public class LogSettingsViewModel : BaseViewModel
    {
        private bool buttonsEnabled;
        private bool allowExtendedLogging;
        private ObservableCollection<LogFile> files;
        private MvxCommand<LogFile> itemSelectedCommand;
        private LogFile selectedLogFile;

        /// <summary>
        /// Gets or sets the Action for showing a delete prompt. To be implemented and instantiated in each platform specific-project.
        /// </summary>
        public Action<Action, Action> ShowDeleteDialog { get; set; }

        /// <summary>
        /// Gets or sets the Action for showing social share dialog. To be implemented and instantiated in each platform specific-project.
        /// </summary>
        public Action<LogFile> ShowSocialShareDialog { get; set; }

        /// <summary>
        /// Gets or sets a reference to the file that the user selected
        /// </summary>
        public LogFile SelectedLogFile
        {
            get
            {
                return this.selectedLogFile;
            }

            set
            {
                if (this.selectedLogFile == null)
                {
                    this.selectedLogFile = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to enable the buttons or not
        /// </summary>
        public bool ButtonsEnabled
        {
            get
            {
                return this.buttonsEnabled;
            }

            set
            {
                this.SetProperty(ref this.buttonsEnabled, value, () => this.ButtonsEnabled);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow extended logging or not
        /// </summary>
        public bool AllowExtendedLogging
        {
            get
            {
                return this.allowExtendedLogging;
            }

            set
            {
                this.SetProperty(ref this.allowExtendedLogging, value, () => this.AllowExtendedLogging);

                // persist it
                Resolver.Instance.Get<ILogSettings>().FileExtensiveLogs = value;
            }
        }

        /// <summary>
        /// Gets or sets a list of files in the Sales App logs directory
        /// </summary>
        public ObservableCollection<LogFile> Files
        {
            get
            {
                if (this.files == null)
                {
                    this.files = Resolver.Instance.Get<ILog>().GetLogFiles();
                }

                return this.files;
            }

            set
            {
                this.SetProperty(ref this.files, value, () => this.Files);
            }
        }

        /// <summary>
        /// Gets a command to be run when the user selects an item in the view
        /// </summary>
        public ICommand ItemSelectedCommand
        {
            get
            {
                this.itemSelectedCommand = this.itemSelectedCommand ?? new MvxCommand<LogFile>(this.DoSelectItem);
                return this.itemSelectedCommand;
            }
        }

        /// <summary>
        /// Gets a command to be run when the user confirms deletion from the prompt
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                return new MvxCommand(
                    () =>
                        {
                            if (this.selectedLogFile == null)
                            {
                                return;
                            }

                            // this.DialogService.ShowDialog();
                            if (this.ShowDeleteDialog == null)
                            {
                                throw new NullReferenceException("You have not specified a method in your view to implement 'ShowDeleteDialog' functionality");
                            }

                            this.ShowDeleteDialog(this.DeleteAction, () => { });
                        });
            }
        }

        /// <summary>
        /// Gets a command to be run when the user clicks on the share button from the view
        /// </summary>
        public ICommand ShareCommand
        {
            get
            {
                return new MvxCommand(
                    () =>
                    {
                        if (this.selectedLogFile == null)
                        {
                            return;
                        }

                        if (this.ShowSocialShareDialog == null)
                        {
                            throw new NullReferenceException("You have not specified a method in your view to implement 'ShowSocialShareDialog' functionality");
                        }

                        this.ShowSocialShareDialog(this.SelectedLogFile);
                    });
            }
        }

        /// <summary>
        /// This is the actual function that deletes the file from the folder
        /// </summary>
        public void DeleteAction()
        {
            Resolver.Instance.Get<ILog>().DeleteFile(this.SelectedLogFile);

            this.Files.Remove(this.SelectedLogFile);

            this.ButtonsEnabled = false;
        }

        /// <summary>
        /// This is the actual function that effects the file selection. It updates the currently selected file as well as enables the buttons.
        /// </summary>
        /// <param name="item">The actual file that was selected</param>
        private void DoSelectItem(LogFile item)
        {
            this.ButtonsEnabled = true;
            this.selectedLogFile = item;
        }
    }
}
