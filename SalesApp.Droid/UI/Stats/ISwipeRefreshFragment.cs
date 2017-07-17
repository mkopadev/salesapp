using System.Threading.Tasks;

namespace SalesApp.Droid.UI.Stats
{
    public interface ISwipeRefreshFragment
    {
        Task SwipeRefresh(bool forceRemote = true);
    }
}