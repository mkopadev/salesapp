using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using SalesApp.Core.BL.Controllers.Security;
using SalesApp.Core.Extensions;
using SalesApp.Droid.BusinessLogic.Models.Security;

namespace SalesApp.Droid.BusinessLogic.Controllers.Security
{
    public class UiPermissionsController
    {
        private readonly Activity _activity;
      
        private ViewPermission[] ViewPermissions { get; set; }
        private readonly object _locker;

        

        public UiPermissionsController(Activity activity)
        {
            _activity = activity;
            if (_locker == null)
            {
                _locker = new object();
            }
            ViewPermissions = new ViewPermission[0];
        }


        public void RegisterViews(params ViewPermission[] viewPermissions)
        {
            this.ViewPermissions = viewPermissions;
            foreach (var variable in viewPermissions)
            {
                variable.SetVisibility(_activity,ViewStates.Gone);
            }
            SetViewsVisibilty();
        }





        private async Task DoTheVisibilitySettingOfTheViews()
        {
            if (ViewPermissions == null)
            {
                return;
            }
            Dictionary<View,bool> viewsAndVisibility = new Dictionary<View, bool>();
            foreach (ViewPermission viewAndItsRelatedPermission in ViewPermissions)
            {
                
                View currentView = viewAndItsRelatedPermission.GetView(_activity);
                if (currentView != null)
                {
                    bool allowed =
                        await PermissionsController.Instance.Allowed(viewAndItsRelatedPermission.ViewPermissions);
                    viewsAndVisibility.Add(currentView,allowed);
                }
            }
            _activity.RunOnUiThread
                (
                    () =>
                    {
                        foreach (KeyValuePair<View, bool> keyValuePair in viewsAndVisibility)
                        {
                            keyValuePair.Key.Visibility = keyValuePair.Value ? ViewStates.Visible : ViewStates.Gone;
                        }
                    }
                );
        }


        public void SetViewsVisibilty()
        {
            lock (_locker)
            {
                AsyncHelper.RunSync(async () => await DoTheVisibilitySettingOfTheViews());
            }

        }
    }
}