using System;

namespace SalesApp.Core.Auth
{
    public interface ISalesAppSession
    {
        string FirstName { get; set; }

        string LastName { get; set; }

        void Clear();

        Guid UserId { get; set; }

        string UserHash { get; set; }
    }
}