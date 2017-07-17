using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SalesApp.Core.Enums.Stats
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Period
    {
        Day = 1,
        Week = 2,
        Month = 3,
        Quarter = 4,
        Year = 5,
        Unknown = 6
    }
}