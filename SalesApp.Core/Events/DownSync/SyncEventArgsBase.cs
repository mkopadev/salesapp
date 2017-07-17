using System;

namespace SalesApp.Core.Events.DownSync
{
    /// <summary>
    /// A base class for all down sync events
    /// </summary>
    public class SyncEventArgsBase : EventArgs
    {
        /// <summary>
        /// Gets or sets the type of sync that is taking place
        /// </summary>
        public Type SyncType { get; set; }
    }
}
