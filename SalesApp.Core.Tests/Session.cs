using System;
using SalesApp.Core.Auth;

namespace SalesApp.Core.Tests
{
    public class Session : ISalesAppSession
    {
        public Session()
        {
            FirstName = "Njihia";
            LastName = "Njatha";
            UserHash =
                "MDcyMzc0NzMwNzo2MjIxMzAxYTUzZTM0OTE5YmFiNzhjZjY1Y2VkYjU5NjE3YTFiMDk4MzViMDFjNDUzMjEwZDI5Zjc5MTUwYmIy";
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public Guid UserId
        {
            get { return new Guid("61c466f3-6f5f-e411-80d8-00155d83e77c"); }
            set { }
        }

        public string UserHash { get; set; }
    }
}