using Android.App;
using Android.Views;
using SalesApp.Core.Enums.Security;
using SalesApp.Droid.Framework;

namespace SalesApp.Droid.BusinessLogic.Models.Security
{
    public class ViewPermission
    {
        public int ViewId { get; set; }
        public Permissions[] ViewPermissions { get; set; }
        private View _view;
        public ViewStates OriginalVisibility { get; set; }
        private int _retrieveCount = 0;
        

        public ViewPermission(int viewId, params Permissions[] viewPermissions)
        {
            this.ViewPermissions = viewPermissions;
            this.ViewId = viewId;
        }

        public View GetView(Activity activity)
        {
            ViewGroup activityRoot = activity.Window.DecorView.FindViewById(Android.Resource.Id.Content) as ViewGroup; // .FindViewById(Android.Resource.Id.Content) as ViewGroup;
            if (activityRoot == null)
            {
                return null;
            }
            if (activityRoot.ChildCount == 0)
            {
                activityRoot = activityRoot.RootView as ViewGroup;
            }
            if (activityRoot == null)
            {
                return null;
            }
            _view = activityRoot.GetView(ViewId);
            if (_view != null && _retrieveCount == 0)
            {
                _retrieveCount++;
                OriginalVisibility = _view.Visibility;
            }
            return _view;
        }

        public void SetVisibility(Activity activity, ViewStates viewState)
        {
            View view = GetView(activity);
            if (view == null)
            {
                return;
            }
            view.Visibility = viewState;
        }
    }
}