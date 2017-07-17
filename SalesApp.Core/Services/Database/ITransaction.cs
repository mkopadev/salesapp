namespace SalesApp.Core.Services.Database
{
    /// <summary>
    /// Allows database transactions to be controlled from outside the library
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// Requests all db requests in the transaction be written
        /// </summary>
        void Commit();

        /// <summary>
        /// Requests all db requests in transaction be dropped
        /// </summary>
        void Rollback();
    }
}