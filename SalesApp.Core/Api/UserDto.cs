using System;

namespace SalesApp.Core.Api
{
    public class UserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Hash { get; set; }
        public Guid Id { get; set; }

    }
}