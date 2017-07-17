using System;
using Newtonsoft.Json;
using SalesApp.Core.Api.Attributes;
using SalesApp.Core.Framework;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database.Models;
using SQLite.Net.Attributes;

namespace SalesApp.Core.BL.Contracts
{
    /// <summary>
	/// Business entity base class. Provides the ID property.
	/// </summary>
    public abstract class BusinessEntityBase : ModelBase
    {
        protected static readonly ILog Logger = LogManager.Get(typeof(BusinessEntityBase));

        [Preserve]
        protected BusinessEntityBase()
        {
        }

        [JsonProperty("Created")]
        [Column("DateCreated")]
        public override DateTime Created { get; set; }

        [Column("DateUpdated")]
        public override DateTime Modified { get; set; }

        private string _tableName = string.Empty;

        [Ignore]
        [NotPosted]
        [JsonIgnore]
        public string TableName
        {
            get
            {
                if (string.IsNullOrEmpty(_tableName))
                {
                    _tableName = this.GetType().Name;
                }

                return _tableName;
            }
        }
    }
}