using System;

namespace SalesApp.Core.Api
{
    public class PostResponse
    {
        public string ResponseText { get; set; }
        public bool Successful { get; set; }
        public Guid RequestId { get; set; }
    }
}
