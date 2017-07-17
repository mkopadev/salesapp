using System.Collections.Generic;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.Device;

namespace SalesApp.Core.BL.Models.Security
{
    /// <summary>
    /// This class represents the Login Response from the API.
    /// </summary>
    public class LoginResponse : DsrProfile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginResponse"/> class.
        /// </summary>
        public LoginResponse()
        {
            this.AppCompatibility = AppCompatibility.Unknown;
        }

        public List<Permission> Permissions { get; set; }

        public LoginResponseCode Code { get; set; }

        public AppCompatibility AppCompatibility { get; set; }
    }
}