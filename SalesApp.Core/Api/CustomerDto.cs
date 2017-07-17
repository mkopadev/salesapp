namespace SalesApp.Core.Api
{
    public class CustomerDto
    {
        public string DsrPhone { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string NationalId { get; set; }
        public ProductDto Product { get; set; }
    }
}