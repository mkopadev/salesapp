using SalesApp.Core.Enums;

namespace SalesApp.Core.Api.SwapComponent
{
    public class SwapComponentResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string Successful { get; set; }

        public ServiceReturnStatus Status { get; set; }

    }
}
