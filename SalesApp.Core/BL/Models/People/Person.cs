using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SalesApp.Core.Api.Attributes;
using SalesApp.Core.BL.Models.Syncing;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Extensions;
using SQLite.Net.Attributes;

namespace SalesApp.Core.BL.Models.People
{
    public class Person : SynchronizableModelBase
    {
        private PersonTypeEnum _personType;

        [Ignore]
        [JsonConverter(typeof(StringEnumConverter))]
        public PersonTypeEnum PersonType
        {
            get { return this._personType; }
            set { this._personType = value.ToString().ToEnumValue<PersonTypeEnum>(); }
        }

        [Ignore]
        [NotPosted]
        public string FullName
        {
            get
            {
                return string.Format("{0} {1}", this.FirstName, this.LastName);
            }
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the customer's national ID number;
        /// </summary>
        public string NationalId { get; set; }
    }
}