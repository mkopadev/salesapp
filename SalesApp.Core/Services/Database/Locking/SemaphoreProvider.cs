namespace SalesApp.Core.Services.Database.Locking
{
    public static class SemaphoreProvider
    {
         public static DatabaseSemaphore DbSemaphore = new DatabaseSemaphore();
    }
}