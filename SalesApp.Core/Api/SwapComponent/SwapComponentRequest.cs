namespace SalesApp.Core.Api.SwapComponent
{
    public class SwapComponentRequest
    {
        public int RequestType { get; set; }
        public string RequestValue { get; set; }
        public int IncomingComponentId { get; set; }
        public int OutgoingComponentId { get; set; }
        public string Reason { get; set; }
    }
}
