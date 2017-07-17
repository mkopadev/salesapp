namespace SalesApp.Core.Exceptions.Syncing
{
    public class UnsavedModelException : SyncingException
    {
        public UnsavedModelException(string tableName) : base(tableName)
        {
            
        }
    }
}