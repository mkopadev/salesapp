using SalesApp.Droid.UI.SwipableViews;

namespace SalesApp.Droid.UI.Stats
{
    public interface ISwipeRefreshable
    {
        void ScrollYChanged(ScrollInformation scrollInformation);

        void SetIsBusy(bool busy);
    }
}