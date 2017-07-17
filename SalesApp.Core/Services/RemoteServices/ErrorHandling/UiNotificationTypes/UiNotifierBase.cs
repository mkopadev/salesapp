namespace SalesApp.Core.Services.RemoteServices.ErrorHandling.UiNotificationTypes
{
    public abstract class UiNotifierBase
    {
        public object Title { get; set; }
        public object Story { get; set; }
    }
}