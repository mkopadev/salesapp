using Android.App;
using Android.OS;
using Android.Views;
using MvvmCross.Droid.Support.V4;
using Newtonsoft.Json;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Droid.Framework;

namespace SalesApp.Droid.Components.UIComponents
{
    /// <summary>
    /// A base class for fragment that utilize the MVVM pattern
    /// </summary>
    public abstract class MvxFragmentBase : MvxFragment
    {
        private ILog _logger;

        protected ILog Logger
        {
            get
            {
                if (this._logger == null)
                {
                    this._logger = Resolver.Instance.Get<ILog>();
                    this._logger.Initialize(this.GetType().FullName);
                }

                return this._logger;
            }
        }

        private ActivityBase _activityBase;

        public ActivityBase ActivityBase
        {
            get
            {
                return this._activityBase;
            }
        }

        /// <summary>
        /// Gets or sets the view inflated and returned by OnCreateView
        /// </summary>
        public View FragmentView { get; set; }

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
            if (this.Arguments == null)
            {
                return default(T);
            }

            return this.Arguments.GetJsonObject<T>(key);
        }

        /// <summary>
        /// Returns the View Model for this fragment as type T.
        /// We use a generic method instead of a generic class because generic Fragment in android usually leads to TypeLoadException upon recreation by the android system.
        /// Its upon developers to make sure that their view models are convertible to T
        /// </summary>
        /// <typeparam name="T">The type of view model</typeparam>
        /// <returns>This view model as T</returns>
        public T GetTypeSafeViewModel<T>() where T:class 
        {
            return this.ViewModel as T;
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            this._activityBase = activity as ActivityBase;
        }

        public string FragmentTag
        {
            get
            {
                return this.GetType().FullName;
            }
        }
    }
}