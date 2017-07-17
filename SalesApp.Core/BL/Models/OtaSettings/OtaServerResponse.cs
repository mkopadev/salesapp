using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesApp.Core.BL.Models.OtaSettings
{
    /// <summary>
    /// The server response expected when the app requests OTA settings
    /// </summary>
    public class OtaServerResponse
    {
        /// <summary>
        /// Gets or sets the app version for these settings
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// Gets or sets the time stamp representing when lastly the settings were updated on the server
        /// </summary>
        public string ServerTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the settings groups which contain the specific settings
        /// </summary>
        [JsonProperty("ConfigGroups")]
        public List<SettingsGroup> SettingsGroups { get; set; }
    }
}