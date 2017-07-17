namespace SalesApp.Droid.Components.UIComponents
{
    public abstract class FragmentBase2 : FragmentBase
    {
        /// <summary>
        /// This method validates the content on the Fragment if needed. Any submitting forms should call this method to verify whether
        /// the content validates.
        /// </summary>
        /// <returns>True when valid, otherwise false</returns>
        public abstract bool Validate();
    }
}