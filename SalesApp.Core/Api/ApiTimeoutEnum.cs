namespace SalesApp.Core.Api
{
    /// <summary>
    /// Different timeouts for API calls
    /// </summary>
    public enum ApiTimeoutEnum
    {
        /// <summary>
        /// 1.5 seconds
        /// </summary>
        Tiny = 1500,

        /// <summary>
        /// 3 seconds
        /// </summary>
        VeryShort = 3000,

        /// <summary>
        /// 5 seconds
        /// </summary>
        Short = 5000,

        /// <summary>
        /// 30 seconds
        /// </summary>
        Normal = 30000,

        /// <summary>
        /// 45 seconds
        /// </summary>
        Long = 45000,

        /// <summary>
        /// 60 seconds
        /// </summary>
        VeryLong = 60000
    }
}
