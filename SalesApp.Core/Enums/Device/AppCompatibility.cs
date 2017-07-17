using Newtonsoft.Json;
using SalesApp.Core.Api.Json;

namespace SalesApp.Core.Enums.Device
{
    [JsonConverter(typeof(TolerantEnumConverter))]
    public enum AppCompatibility
    {
        UpToDate,
        UpdateAvailable,
        UpdateRequired,
        Unknown
    }
}