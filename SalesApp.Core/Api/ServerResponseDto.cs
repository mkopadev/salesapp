using SalesApp.Core.Api.ServerResponseObjects;

namespace SalesApp.Core.Api
{
    /// <summary>
    /// A generic holder for as simple message response from an API call
    /// </summary>
    public class ServerResponseDto : ServerResponseObjectsBase
    {
        /// <summary>
        /// Title of the message
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Body of the message
        /// </summary>
        public string Message { get; set; }
    }
}