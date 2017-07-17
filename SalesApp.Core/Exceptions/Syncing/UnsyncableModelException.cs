namespace SalesApp.Core.Exceptions.Syncing
{
    public class UnsyncableModelException : SyncingException
    {
        public UnsyncableModelException(string tableName)
            : base(tableName)
        {
            
        }
    }
}