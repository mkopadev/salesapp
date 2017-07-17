namespace SalesApp.Core.Api.DownSync
{
    /// <summary>
    /// This is the class for performing down sync
    /// </summary>
    public class DownSyncApi : ApiBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DownSyncApi"/> class.
        /// </summary>
        /// <param name="relativePath">The relative path of the url</param>
        public DownSyncApi(string relativePath) : base(relativePath)
        {
        }
    }
}