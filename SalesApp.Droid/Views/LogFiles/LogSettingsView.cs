using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Java.IO;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.ViewModels.Logging;
using SalesApp.Droid.Framework;
using SalesApp.Droid.UI.Utils;
using Uri = Android.Net.Uri;

namespace SalesApp.Droid.Views.LogFiles
{
    /// <summary>
    /// This is the screen that shows the log files to the user. The user can select a file and either delete it or share it.
    /// </summary>
    [Activity(Label = "@string/log_files_screen_title", NoHistory = false, ScreenOrientation = ScreenOrientation.Portrait, ParentActivity = typeof(HomeView), Theme = "@style/AppTheme.SmallToolbar")]
    public class LogSettingsView : MvxViewBase<LogSettingsViewModel>
    {
        /// <summary>
        /// A reference to the list view
        /// </summary>
        private ListView filesListView;

        /// <summary>
        /// Gets an implementation of the settings interface
        /// </summary>
        private ILogSettings LogSettingss
        {
            get
            {
                return Resolver.Instance.Get<ILogSettings>();
            }
        }

        /// <summary>
        /// Shows an android dialog for confirming deletion of a log file
        /// </summary>
        /// <param name="positiveCallback">The positive action (Delete in this case)</param>
        /// <param name="negativeAction">The negative action (Cancel in this case)</param>
        public void ShowDeleteDialog(Action positiveCallback, Action negativeAction)
        {
            AlertDialogBuilder.Instance
                .AddButton(
                Resource.String.log_files_delete,
                () =>
                {
                    positiveCallback();
                    this.filesListView.ClearChoices();
                })
                .AddButton(Resource.String.cancel, negativeAction)
                .SetText(string.Empty, string.Format(this.GetString(Resource.String.log_files_delete_prompt), this.ViewModel.SelectedLogFile))
                .Show(this);
        }

        /// <summary>
        /// Opens and android dialog for sharing a log file with social apps
        /// </summary>
        /// <param name="logFile">The log file to share</param>
        public void OpenSocialShareDialog(LogFile logFile)
        {
            // open list of share apps to share the file with
            var shareIntent = new Intent(Intent.ActionSend);
            shareIntent.SetType("text/plain");
            shareIntent.PutExtra(Intent.ExtraSubject, GetString(Resource.String.log_file_));

            var file = new File(logFile.FilePath);
            var fileUri = Uri.FromFile(file);

            shareIntent.PutExtra(Intent.ExtraStream, fileUri);
            this.StartActivity(Intent.CreateChooser(shareIntent, GetString(Resource.String.share_log_file_)));
        }

        /// <summary>
        /// Android's on create method
        /// </summary>
        /// <param name="bundle">Bundle to use in re-creating the activity</param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetContentView(Resource.Layout.layout_logs);
            this.filesListView = this.FindViewById<ListView>(Resource.Id.fileList);

            this.AddToolbar(Resource.String.log_files_screen_title, true);

            ViewModel.ButtonsEnabled = false;
            this.ViewModel.AllowExtendedLogging = this.LogSettingss.FileExtensiveLogs;
            this.ViewModel.ShowDeleteDialog = this.ShowDeleteDialog;
            this.ViewModel.ShowSocialShareDialog = this.OpenSocialShareDialog;
        }
    }
}