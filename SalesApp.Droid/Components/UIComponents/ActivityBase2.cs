using Android.Views;

namespace SalesApp.Droid.Components.UIComponents
{
    public interface IActivity2
    {
        /// <summary>
        ///     Returns true if connection to the network exists or false if it doesn't
        /// </summary>
        bool ConnectedToNetwork { get; }

        void SetViewPermissions();

        /// <summary>
        ///     Attempts to force the keyboard to be displayed.
        /// </summary>
        /// <param name="focusedView">The UI element in focus that the keyboard should send keypresses to</param>
        void ShowKeyboard(View focusedView);

        void HideKeyboard();

        /// <summary>
        ///     This method is used to initialise the screen components when loading the screen.
        ///     It sets the values of the views which are not set in the XML layouts.
        ///     This abstract implementation enforces a uniform place to handle this.
        /// </summary>
        void InitializeScreen();

        /// <summary>
        ///     This method is used to retrieve the input from screen components and assign to activity variables.
        ///     This abstract implementation enforces a uniform place to handle this functionality.
        /// </summary>
        void RetrieveScreenInput();

        /// <summary>
        ///     This method is used to update the interface with dynamic calculated values.
        ///     It can be used on screen initialisation and text change listeners.
        ///     This abstract implementation enforces a uniform place to handle this functionality.
        /// </summary>
        void UpdateScreen();

        /// <summary>
        ///     This method is used to set the Listeners to all screen elements.
        ///     This abstract implementation enforces a uniform place to handle this functionality.
        /// </summary>
        void SetListeners();

        /// <summary>
        ///     This method is used to validate screen input against the system.
        ///     This abstract implementation enforces a uniform place to handle this functionality.
        /// </summary>
        /// <returns>True if validation is successful</returns>
        bool Validate();

        void SetScreenTitle(string title);
        void SetScreenTitle(int resId);
    }

    public abstract class ActivityBase2 : ActivityBase
    {
        /// <summary>
        ///     This method is used to initialise the screen components when loading the screen.
        ///     It sets the values of the views which are not set in the XML layouts.
        ///     This abstract implementation enforces a uniform place to handle this.
        /// </summary>
        public abstract void InitializeScreen();

        /// <summary>
        ///     This method is used to retrieve the input from screen components and assign to activity variables.
        ///     This abstract implementation enforces a uniform place to handle this functionality.
        /// </summary>
        public abstract void RetrieveScreenInput();

        /// <summary>
        ///     This method is used to update the interface with dynamic calculated values.
        ///     It can be used on screen initialisation and text change listeners.
        ///     This abstract implementation enforces a uniform place to handle this functionality.
        /// </summary>
        public abstract void UpdateScreen();

        /// <summary>
        ///     This method is used to set the Listeners to all screen elements.
        ///     This abstract implementation enforces a uniform place to handle this functionality.
        /// </summary>
        public abstract void SetListeners();

        /// <summary>
        ///     This method is used to validate screen input against the system.
        ///     This abstract implementation enforces a uniform place to handle this functionality.
        /// </summary>
        /// <returns>True if validation is successful</returns>
        public abstract bool Validate();

    }
}