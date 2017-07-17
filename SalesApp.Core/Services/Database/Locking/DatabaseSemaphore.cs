using System.Threading;
using System.Threading.Tasks;

namespace SalesApp.Core.Services.Database.Locking
{
    public class DatabaseSemaphore
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);

       

        

        public async Task WaitAsync(int sourceLineNumber, string sourceFilePath)
        {
            await _semaphore.WaitAsync();
            DataAccess.Instance.Logger.Log($"LockAsync on line {sourceLineNumber}, file {sourceFilePath}",false);
        }

        public void Wait(int sourceLineNumber, string sourceFilePath)
        {
            _semaphore.Wait();
            DataAccess.Instance.Logger.Log($"Lock on line {sourceLineNumber}, file {sourceFilePath}",false);
        }


        public void Release(int sourceLineNumber, string sourceFilePath)
        {
            _semaphore.Release();
            DataAccess.Instance.Logger.Log($"Unlock on line {sourceLineNumber}, file {sourceFilePath}",false);
        }

        
    }
}