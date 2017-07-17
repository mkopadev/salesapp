namespace SalesApp.Core.Enums.People
{
    /// <summary>
    /// Represents different states that a customer photo can be in
    /// </summary>
    public enum PhotoUploadStatus
    {
        /// <summary>
        /// Not yet decided
        /// </summary>
        OnHold = 1,

        /// <summary>
        /// Waiting to be uploaded next
        /// </summary>
        Pending = 2,

        /// <summary>
        /// Uploaded successfully
        /// </summary>
        Successfull = 3,

        /// <summary>
        /// Upload failed one or more times
        /// </summary>
        Failed = 4
    }
}