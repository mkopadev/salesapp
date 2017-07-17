using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesApp.Core.BL.Models.OtaSettings
{
    public class SettingsGroup
    {
        public string Name { get; set; }

        [JsonProperty("Value")]
        public List<OtaSetting> Settings { get; set; }
    }
}