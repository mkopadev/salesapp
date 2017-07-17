using System.Collections.Generic;

namespace SalesApp.Core.Api.DownSync
{
    /// <summary>
    /// This class represents the expected format of data received from the server when the app performs a down sync
    /// </summary>
    /// <typeparam name="T">The entity type to sync (Customer, Prospect, etc)</typeparam>
    public class DownSyncServerResponse<T>
    {
        /// <summary>
        /// Gets or sets the owner of the device that initialized a sync
        /// </summary>
        public string DsrPhone { get; set; }

        /// <summary>
        /// Gets or sets a unique ID for the user who initialized the sync
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the server time stamp which helps the server to send only changed records
        /// </summary>
        public string ServerTimeStamp { get; set; }

        /// <summary>
        /// Gets or sets a list of type T containing the data from the server
        /// </summary>
        public List<T> Package { get; set; }
    }
}
