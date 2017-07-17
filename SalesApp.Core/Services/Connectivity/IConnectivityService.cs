namespace SalesApp.Core.Services.Connectivity
{
    public interface IConnectivityService
    {
        /// <summary>
        /// This method can be used to check whether the device has an internet connection.
        /// </summary>
        /// <returns>True if the user has an internet connection, otherwise false</returns>
        bool HasConnection();
    }
}