namespace SalesApp.Core.Exceptions.Syncing
{
    public class NotQueuedException : SyncingException
    {
        public NotQueuedException(string tableName) : base(tableName)
        {

        }
    }
}