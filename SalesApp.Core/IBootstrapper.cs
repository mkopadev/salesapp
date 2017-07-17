namespace SalesApp.Core
{
    /// <summary>
    /// This Interface represents the Bootstrapper for device specific types. It can be used to load specific type configurations. 
    /// This could be replaced by a IoC framework later.
    /// </summary>
    public interface IBootstrapper
    {
        /// <summary>
        /// This method is run once during the application startup and allows for injecting specific device implementations.
        /// </summary>
        void Bootstrap();
    }
}