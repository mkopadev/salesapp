using System;
using SQLite.Net.Attributes;

namespace SalesApp.Core.Services.Database.Models
{
    public abstract class ModelBase
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public  virtual DateTime Created { get; set; }

        public virtual DateTime Modified { get; set; }

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

        private string _tableName;

    }
}