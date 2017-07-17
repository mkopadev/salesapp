namespace SalesApp.Core.Services.Device
{
    public interface IInformation
    {
        /// <summary>
        /// Major version of the core.
        /// </summary>
        int Major { get; }


        /// <summary>
        /// Minor version of the core.
        /// </summary>
        int Minor { get; }

        /// <summary>
        /// Build No of the core.
        /// </summary>
        int Build { get; }

        /// <summary>
        /// Revision No of the core.
        /// </summary>
        int Revision { get; }

        /// <summary>
        /// The version of the core.
        /// </summary>
        string CoreVersion { get; }

        /// <summary>
        /// The device specific version. It can be different between Android, iOS and Windows.
        /// </summary>
        /// <returns></returns>
        string DeviceAppVersion { get; }

        /// <summary>
        /// Returns a user readable build time of the current running app.
        /// </summary>
        string BuildTime { get; }

        /// <summary>
        /// Device platform name, eg. Android, Windows, iOS.
        /// </summary>
        string DevicePlatform { get; }

        /// <summary>
        /// Device specific software version, eg. Android 4.03, Windows 8.1 Phone, etc.
        /// </summary>
        string DeviceSoftwareVersion { get; }

        /// <summary>
        /// Screen resolution of the phone.
        /// </summary>
        string ScreenResolution { get; }

    }
}