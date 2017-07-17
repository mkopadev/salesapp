using Newtonsoft.Json;
using SalesApp.Core.BL.Contracts;
using SQLite.Net.Attributes;

namespace SalesApp.Core.BL.Models.OtaSettings
{
    /// <summary>
    /// Class represents a setting sent over the air
    /// </summary>
    public class OtaSetting : BusinessEntityBase
    {
        /// <summary>
        /// Gets or sets the name of the setting
        /// </summary>
        [Indexed(Name = "UniqueSetting", Order = 2, Unique = true)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the group of the setting
        /// </summary>
        [Indexed(Name = "UniqueSetting", Order = 1, Unique = true)]
        public string GroupName { get; set; }

        /// <summary>
        /// Sets the value that the server sent back. Could be a string or a complex object
        /// </summary>
        [Ignore]
        [JsonProperty("Value")]
        public object ServerValue
        {
            set
            {
                if (!(value is string))
                {
                    // serialize it
                    this.Value = JsonConvert.SerializeObject(value);
                }
                else
                {
                    // store it directly
                    this.Value = value.ToString();
                }
            }
        }

        /// <summary>
        /// Gets or sets the setting value
        /// </summary>
        [JsonIgnore]
        public string Value { get; set; }
    }
}
