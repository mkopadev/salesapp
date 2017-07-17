namespace SalesApp.Core.Api.Tickets
{
    public class TicketSubmissionResponse
    {
        public string Text { get; set; }
        public bool Success { get; set; }
        public string TicketId { get; set; }
        public string TicketNumber { get; set; }
    }
}