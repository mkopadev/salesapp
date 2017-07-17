using System;
using System.Threading.Tasks;

namespace SalesApp.Core.Extensions
{
    public static class AsyncTaskExtensions
    {
        public async static Task<TResult> RunTimed<TResult>(this Task<TResult> task, int timeout)
        {
            if (task != await Task.WhenAny(task, Task.Delay(timeout)))
            {
                throw new TimeoutException();
            }

            var res = task.Result;
            "Result in run timed ~".WriteLine(res);

            return res;
        }
    }
}