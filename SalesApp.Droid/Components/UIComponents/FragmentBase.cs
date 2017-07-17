using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;

//using Android.App;

namespace SalesApp.Droid.Components.UIComponents
{

    public abstract class FragmentBase : Fragment
    {
        protected ILog Logger { get; set; }


        protected FragmentBase()
        {
            Logger = Resolver.Instance.Get<ILog>();
            Logger.Initialize(this.GetType().FullName);
        }

        public void HideKeyboard()
        {
            if (Activity == null)
            {
                return;
            }
            if (Activity.Window == null || Activity.Window.CurrentFocus == null)
            {
                return;
            }
            if (Activity.Window.CurrentFocus.WindowToken == null)
            {
                return;
            }

            var inputMethodManager = (InputMethodManager) Activity.GetSystemService(AppCompatActivity.InputMethodService);
            var token = Activity.Window.CurrentFocus.WindowToken;
            if (token != null)
            {
                inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.NotAlways);
            }
        }

        public event EventHandler<EventArgs> Completed; 
        protected View view { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
        }

        protected abstract void InitializeUI(bool isOnUiThread = false);
        

        public abstract void UpdateUI(bool calledFromUiThread = false);

        protected abstract void SetEventHandlers();

        protected ActivityBase OwnerActivity
        {
            get
            {
                return Activity as ActivityBase;
            }
        }

        public abstract void SetViewPermissions();

        protected void FireCompleted(bool skipped)
        {
            if (this.Completed != null)
            {
                this.Completed.Invoke(skipped,EventArgs.Empty);
            }
        }

        

        
    }
}