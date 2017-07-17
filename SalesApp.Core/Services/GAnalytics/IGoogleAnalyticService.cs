namespace SalesApp.Core.Services.GAnalytics
{
    public interface IGoogleAnalyticService
    {
        void Initialize();

        void TrackScreen(string pageName);

        void TrackEvent(string gACategory, string title, string eventToTrack);
    }
}