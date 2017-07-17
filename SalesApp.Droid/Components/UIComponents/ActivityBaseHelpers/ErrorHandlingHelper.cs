using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Widget;
using Newtonsoft.Json;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Extensions;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.RemoteServices.ErrorHandling;
using SalesApp.Core.Services.RemoteServices.ErrorHandling.UiNotificationTypes;
using SalesApp.Droid.Components.UIComponents.CustomInfo;
using SalesApp.Droid.UI.Utils;
using Dialog = SalesApp.Core.Services.RemoteServices.ErrorHandling.UiNotificationTypes.Dialog;

namespace SalesApp.Droid.Components.UIComponents.ActivityBaseHelpers
{
    // TODO need proper Core/DeviceImplementation rewrite
    public class ErrorHandlingHelper
    {
        private Activity _context;

        public ErrorHandlingHelper(Activity context)
        {
            this._context = context;
        }

        private void SetNotifierContent(UiNotifierBase notifier, ErrorDescriber describer)
        {
            var content = new[]
            {
                new { Story = Resource.String.api_errors_400, ExType = typeof(HttpResponse400Exception) },
                new { Story = Resource.String.api_errors_500, ExType = typeof(HttpResponse500Exception) },
                new { Story = Resource.String.api_errors_timeout_story, ExType = typeof(TaskCanceledException) },
                new { Story = Resource.String.api_errors_parse_story, ExType = typeof(JsonReaderException) },
                new { Story = Resource.String.api_errors_connection_story,ExType = typeof(NotConnectedToInternetException) },
                new { Story = Resource.String.api_errors_no_data_story,ExType = typeof(HttpResponse204Exception) }
            }
            .FirstOrDefault(item => item.ExType == describer.ExceptionType);

            if (content == null && !describer.ErrorCode.IsBlank())
            {
                notifier.Story = string.Format(this._context.GetString(Resource.String.api_errors_200), describer.ErrorCode);
            }
            else if (content != null)
            {
                notifier.Story = string.Format(
                    this._context.GetString(content.Story),
                    describer.HttpStatusCode > 299 ? describer.HttpStatusCode.ToString() : string.Empty);
            }
            else
            {
                notifier.Story = this._context.GetString(Resource.String.api_errors_unknown_error_occured);
            }
        }

        public void ErrorOccured(ErrorDescriber describer)
        {
            UiNotifierBase notifier = Activator.CreateInstance(describer.UiNotifierType) as UiNotifierBase ??
                                      new Flash();

            this.SetNotifierContent(notifier, describer);
            
            string story = notifier.Story.ToString();
            this.Notify(describer, story);
        }

        private ILog _logger;

        private ILog Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = Resolver.Instance.Get<ILog>();
                    _logger.Initialize(this.GetType().FullName);
                }
                return _logger;
            }
        }

        void ShowToast(string story)
        {
            Toast.MakeText(this._context, story, ToastLength.Long).Show();
        }

        void ShowDialog(ErrorDescriber describer, string story)
        {
            AlertDialogBuilder builder = AlertDialogBuilder.Instance;
            builder.SetText
                (string.Empty, story)
                .AddButton
                (
                    Resource.String.api_errors_ok
                    , () =>
                    {
                        AlertDialogBuilder.Instance.Close();
                    }
                );
            if (describer.RetrySupported)
            {
                builder.AddButton
                    (
                        Resource.String.api_errors_retry
                        , async () =>
                        {
                            await describer.RetryAction(describer.SuccessCallback);
                        }
                    );
            }

            builder.Show(this._context);
        }

        private void ShowFragment(ErrorDescriber describer, string story)
        {
            ActivityBase activityBase = this._context as ActivityBase;

            if (activityBase == null)
            {
                return;
            }

            var custInfoFragment =
                                activityBase.SupportFragmentManager.FindFragmentByTag(new CustomInfoFragment().FragmentTag);
            if (custInfoFragment != null)
            {
                activityBase.SupportFragmentManager.BeginTransaction().Remove(custInfoFragment).Commit();
            }

            if (!activityBase.ErrorFragmentContainerExists)
            {
                Logger.Error("Container for error displaying fragment has not been set or could not be found. Error will be displayed in a dialog");
                describer.UiNotifierType = typeof(Dialog);
                Notify(describer, story);
                describer.UiNotifierType = typeof(FullOverlay);
                return;
            }

            CustomInfoFragment customInfoFragment = new CustomInfoFragment();
            customInfoFragment.PositiveAction += () =>
            {
                activityBase.SupportFragmentManager.BeginTransaction()
                    .Remove(
                        activityBase.SupportFragmentManager.FindFragmentByTag(customInfoFragment.FragmentTag))
                    .CommitAllowingStateLoss();
            };
            if (describer.RetrySupported)
            {
                customInfoFragment.NegativeAction += () => describer.RetryAction(describer.SuccessCallback);
            }

            activityBase.SupportFragmentManager.BeginTransaction()
                .Add(activityBase.ErrorFragmentContainerId, customInfoFragment, customInfoFragment.FragmentTag)
                .CommitAllowingStateLoss();
            customInfoFragment.SetContent("", story, activityBase.GetString(Resource.String.api_errors_ok), describer.RetrySupported ? activityBase.GetString(Resource.String.api_errors_retry) : "");
        }

        private void ShowSnackbar(ErrorDescriber describer, string story)
        {
            ActivityBase activityBase = this._context as ActivityBase;

            if (activityBase == null)
            {
                return;
            }

            if (!activityBase.ErrorSnackbarContainerExists)
            {
                Logger.Error("Container for error displaying Snackbar has not been set or could not be found. Error will be displayed in a dialog");
                describer.UiNotifierType = typeof(Dialog);
                Notify(describer, story);
                describer.UiNotifierType = typeof(Sticky);
                return;
            }

            activityBase.HideSnackbar();
            activityBase.ShowSnackbar(activityBase.FindViewById(activityBase.ErrorSnackbarContainerId),story,x => activityBase.HideSnackbar(),Resource.String.api_errors_ok);
        }

        private void Notify(ErrorDescriber describer, string story)
        {
            this._context.RunOnUiThread(() =>
            {
                if (describer.UiNotifierType == typeof(BackgroundNotifier))
                {
                    var message = string.Format("Logging background API error: {0}, - {1}", story, this._context.Title);
                    Logger.Error(message);
                }
                else if (describer.UiNotifierType == typeof(Flash))
                {
                    ShowToast(story);
                }
                else if (describer.UiNotifierType == typeof(Sticky))
                {
                    ShowSnackbar(describer, story);
                }
                else if (describer.UiNotifierType == typeof(FullOverlay))
                {
                    ShowFragment(describer, story);
                }
                else
                {
                    ShowDialog(describer, story);
                }
            });
        }
    }
}