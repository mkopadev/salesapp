namespace SalesApp.Core.Events.DownSync
{
    /// <summary>
    /// This class represents event that indicate the progress status of a down sync operation
    /// </summary>
    public class SyncStatusEventArgs : SyncEventArgsBase
    {
        /// <summary>
        /// Gets or sets the total number of items to be processed
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets the number of items processed so far
        /// </summary>
        public int Processed { get; set; }

        /// <summary>
        /// Gets the progress as a percentage
        /// </summary>
        public int PercentProcessed
        {
            get
            {
                float percent = ((float)this.Processed / this.Total) * 100;
                return (int)percent;
            }
        }
    }
}
