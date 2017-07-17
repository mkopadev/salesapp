using System;

namespace SalesApp.Core.Events.DownSync
{
    /// <summary>
    /// Event to indicate that something went wrong while syncing
    /// </summary>
    public class SyncErrorEventArgs : SyncEventArgsBase
    {
        /// <summary>
        /// Gets or sets the specific error that occurred during sync
        /// </summary>
        public Exception Error { get; set; }
    }
}
