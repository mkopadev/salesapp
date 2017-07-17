using System;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Enums.Syncing;
using SalesApp.Core.Framework;

namespace SalesApp.Core.BL.Models.Syncing
{
    /// <summary>
    /// This class represents a record that describes the sync status of a record.
    /// Absence of a <see cref="SyncRecord"/> for a record should be treated as the record having been fully synced.
    /// </summary>
    [Preserve(AllMembers = true)]
    public class SyncRecord : BusinessEntityBase
    {
        /// <summary>
        /// The model in which the record belongs
        /// </summary>
        public string ModelType { get; set; }

        /// <summary>
        /// The request id that was initially used to try the up sync
        /// </summary>
        public Guid RequestId { get; set; }

        /// <summary>
        /// The status of the sync for this record.
        /// If a record does not have a corresponding sync record, it should be treated as having been fully synced.
        /// </summary>
        public RecordStatus Status { get; set; }

        /// <summary>
        /// A message for the failed sync if any
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// The number of tries that we did before giving up
        /// </summary>
        public int SyncAttemptCount { get; set; }

        /// <summary>
        /// The id of the record that this sync record refers to.
        /// </summary>
        public Guid ModelId { get; set; }
    }
}
