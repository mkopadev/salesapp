using System.Net.Http;

namespace SalesApp.Core.Exceptions.API
{
    public class UnauthorizedHttpException : HttpRequestException
    {
        public UnauthorizedHttpException() : base("Access denied")
        {
        }

        public UnauthorizedHttpException(string message) : base(message) { }
    }
}
