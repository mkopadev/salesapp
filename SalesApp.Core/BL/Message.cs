using System;
using SalesApp.Core.BL.Contracts;

namespace SalesApp.Core.BL
{
    public class Message : BusinessEntityBase
    {
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime MessageDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        public bool IsRead { get; set; }

        

        public int MessageId { get; set; }
    }
}