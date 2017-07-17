using SalesApp.Core.BL.Contracts;

namespace SalesApp.Core.BL.Models.Syncing
{
    /// <summary>
    /// Class for tracking down sync server time stamp
    /// </summary>
    public class DownSyncTracker : BusinessEntityBase
    {
        /// <summary>
        /// Gets or sets the entity for which syncing is to be done
        /// </summary>
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the server time stamp
        /// </summary>
        public string ServerTimestamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not this is the first sync ever
        /// </summary>
        public bool IsInitial { get; set; }
    }
}
