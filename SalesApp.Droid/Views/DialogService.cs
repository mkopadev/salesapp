using System.Threading.Tasks;
using Android.Support.V7.App;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;
using SalesApp.Core.ViewModels.Dialog;

namespace SalesApp.Droid.Views
{
    public class DialogService : IDialogService
    {
        public Task<bool?> ShowAsync(string message, string oKButtonContent, string cancelButtonContent)
        {
            var tcs = new TaskCompletionSource<bool?>();

            var mvxTopActivity = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();

            AlertDialog.Builder builder = new AlertDialog.Builder(mvxTopActivity.Activity);
                   builder.SetMessage(message)
                   .SetCancelable(false)
                   .SetPositiveButton(oKButtonContent, (s, args) =>
                   {
                       tcs.SetResult(true);
                   })
                   .SetNegativeButton(cancelButtonContent, (s, args) =>
                   {
                       tcs.SetResult(false);
                   });

            builder.Create().Show();
            return tcs.Task;
        }
    }
}