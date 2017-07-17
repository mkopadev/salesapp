using System;

namespace SalesApp.Core.Exceptions.Syncing
{
    public abstract class SyncingException : Exception
    {
        public string TableName { get; set; }

        public SyncingException(string tableName)
        {
            this.TableName = TableName;
        }
    }
}