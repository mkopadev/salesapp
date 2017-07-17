using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.People;
using SalesApp.Core.Extensions.Data;
using SQLite.Net.Attributes;

namespace SalesApp.Core.BL.Models.People
{
    /// <summary>
    /// Class that represents a client who is just a prospect
    /// </summary>
    public class Prospect : Lead, ICastable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Prospect"/> class.
        /// </summary>
        public Prospect()
        {
            this.PersonType = PersonTypeEnum.Prospect;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the prospect has the money
        /// </summary>
        [JsonProperty("Means")]
        public bool Money { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the prospect authority to spend the money
        /// </summary>
        public bool Authority { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the prospect has the need for the product
        /// </summary>
        public bool Need { get; set; }

        /// <summary>
        /// Gets or sets the reminder time for the prospect
        /// </summary>
        // [Ignore]
        public DateTime ReminderTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the prospect has been converted to a customer
        /// </summary>
        public bool Converted { get; set; }

        /// <summary>
        /// Gets or sets the channel used to register the prospect
        /// </summary>
        [Ignore]
        public override DataChannel Channel { get; set; }

        /// <summary>
        /// Gets the prospect score as an integer
        /// </summary>
        public int Score
        {
            get
            {
                List<bool> scores = new List<bool> { this.Money, this.Authority, this.Need };
                return scores.Count(score => score);
            }
        }
    }
}