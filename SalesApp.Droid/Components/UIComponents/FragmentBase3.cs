//using Android.App;

using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Newtonsoft.Json;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Droid.Framework;

namespace SalesApp.Droid.Components.UIComponents
{
    public abstract class FragmentBase3 : Fragment
    {
        private ILog logger;

        protected FragmentBase3()
        {
            Initialized = false;
        }

        public View view { get; set; }

        protected bool Initialized { get; set; }

        protected ILog Logger
        {
            get
            {
                if (this.logger == null)
                {
                    this.logger = Resolver.Instance.Get<ILog>();
                    this.logger.Initialize(this.GetType().FullName);
                }

                return this.logger;
            }
        }

        /// <summary>
        /// Allows for easy setting a complex object as an argument, converting it to JSON.
        /// </summary>
        /// <typeparam name="T">Type to store</typeparam>
        /// <param name="key">Key to use in the argument bundle</param>
        /// <param name="argument">Actual object to store</param>
        public void SetArgument<T>(string key, T argument)
        {
            // create the bundle if not exist
            if (this.Arguments == null)
            {
                var bundle = new Bundle();
                bundle.PutString(key, JsonConvert.SerializeObject(argument));
                this.Arguments = bundle;
            }
            else
            {
                this.Arguments.PutString(key, JsonConvert.SerializeObject(argument));
            }
        }

        /// <summary>
        /// Allows for easy grabbing an complex argument object when available. Does not catch type cast exceptions, use with care.
        /// </summary>
        /// <typeparam name="T">Type of the object to return</typeparam>
        /// <param name="key">Key of the argument</param>
        /// <returns>Argument objet or default(T) when no arguments are present</returns>
        public T GetArgument<T>(string key)
        {
            return this.Arguments.GetJsonObject<T>(key);
        }

        /// <summary>
        /// This method validates the content on the Fragment if needed. Any submitting forms should call this method to verify whether
        /// the content validates.
        /// </summary>
        /// <returns>True when valid, otherwise false</returns>
        public abstract bool Validate();

        /// <summary>
        /// This method is used to initialise the screen components when loading the screen.
        /// It sets the values of the views which are not set in the XML layouts.
        /// This abstract implementation enforces a uniform place to handle this.
        /// </summary>
        /// <param name="isOnUiThread">Boolean to indicate whether the parameter is called from the UI thread</param>
        protected abstract void InitializeUI(bool isOnUiThread = false);

        /// <summary>
        /// This method is used to set the Listeners to all screen elements.
        /// This abstract implementation enforces a uniform place to handle this functionality.
        /// </summary>
        protected abstract void SetEventHandlers();

        /// <summary>
        /// This method is used to update the interface with dynamic calculated values.
        /// It can be used on screen initialisation and text change listeners.
        /// This abstract implementation enforces a uniform place to handle this functionality.
        /// </summary>
        /// <param name="calledFromUiThread">Boolean to indicate whether the parameter is called from the UI thread</param>
        public abstract void UpdateUI(bool calledFromUiThread = false);

        /// <summary>
        /// This method is used to set the permissions on view if needed. Based upon user authorization 
        /// certain screen elements might need to be disabled.
        /// </summary>
        public abstract void SetViewPermissions();

        public virtual string FragmentTag
        {
            get { return this.GetType().FullName; }
        }
    }
}