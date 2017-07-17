using System;

namespace SalesApp.Core.Api
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime MessageDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}