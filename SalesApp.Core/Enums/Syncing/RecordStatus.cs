using SalesApp.Core.Framework;

namespace SalesApp.Core.Enums.Syncing
{
    /// <summary>
    /// ENUM represents the status of a record that is synced between the device and the server
    /// </summary>
    [Preserve(AllMembers = true)]
    public enum RecordStatus
    {
        /// <summary>
        /// The record had not yet being synced upwards
        /// </summary>
        Pending = 1,

        /// <summary>
        /// The record has been successfully synced upwards
        /// </summary>
        Synced = 2,

        /// <summary>
        /// Sync encountered an error
        /// </summary>
        InError = 3,

        /// <summary>
        /// The record has been synced by sending an SMS to the server
        /// </summary>
        FallbackSent = 4
    }
}