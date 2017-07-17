namespace SalesApp.Core.Logging
{
    public interface ILogSettings
    {
        bool FileExtensiveLogs { get; set; }

        string LastOnTimestamp { get; set; }
    }
}