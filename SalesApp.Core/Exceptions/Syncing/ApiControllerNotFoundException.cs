namespace SalesApp.Core.Exceptions.Syncing
{
    public class ApiControllerNotFoundException : SyncingException
    {
        public ApiControllerNotFoundException(string tableName) : base(tableName)
        {
            
        }
    }
}