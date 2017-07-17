using Newtonsoft.Json;
using SalesApp.Core.Api.Attributes;
using SQLite.Net.Attributes;

namespace SalesApp.Core.Services.Search
{
    public interface ISearchResult
    {
        [Ignore]
        [NotPosted]
        [JsonIgnore]
        bool IsLocalRecord { get; set; }

        [Ignore]
        [NotPosted]
        [JsonIgnore]
        int Score { get; set; }

        [Ignore]
        [NotPosted]
        [JsonIgnore]
        string UniqueValue { get; }

        [Ignore]
        [NotPosted]
        [JsonIgnore]
        string DisplayText { get; }
    }
}