using System.Diagnostics;

namespace SalesApp.Core.Extensions
{
    public static class DebuggingExtensions
    {
        public static void WriteLine(this string str,params object[] args)
        {
            Debug.WriteLine(str.GetFormated(args));
        }

        
    }
}
