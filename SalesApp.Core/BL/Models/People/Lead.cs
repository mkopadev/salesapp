using System.Collections.Generic;
using SalesApp.Core.BL.Models.Chama;
using SQLite.Net.Attributes;

namespace SalesApp.Core.BL.Models.People
{
    /// <summary>
    /// This class represents a person who is either a customer or a prospect
    /// </summary>
    public abstract class Lead : Person
    {
        /// <summary>
        /// Gets or sets the phone number for DSR who registered the client
        /// </summary>
        public string DsrPhone { get; set; }

        /// <summary>
        /// Gets or sets the product that the client is interested in
        /// </summary>
        [Ignore]
        public Product Product { get; set; }

        [Ignore]
        public List<GroupKeyValue> GroupInfo { get; set; }

        public string Groups { get; set; }
    }
}