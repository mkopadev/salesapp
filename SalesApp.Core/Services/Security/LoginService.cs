using System;
using System.Threading.Tasks;
using SalesApp.Core.BL.Controllers.Security;
using SalesApp.Core.BL.Models;
using SalesApp.Core.Logging;

namespace SalesApp.Core.Services.Security
{
    public class LoginService
    {
        public const string IsFirstLogin = "IsFirstLogin";
        private static readonly ILog Logger = LogManager.Get(typeof(LoginService));

        /// <summary>
        /// Returns whether the login information is still valid by checking the hours since last login.
        /// </summary>
        /// <param name="today">Today's date</param>
        /// <param name="lastOnlineLogin">DateTime of last login</param>
        /// <param name="validDays">Valid difference in days between last login and today</param>
        /// <returns>True if login is still valid, otherwise false</returns>
        public bool LoginValid(DateTime lastOnlineLogin, DateTime today,  int validDays)
        {
            int hoursSinceLastLogin = (int)(today - lastOnlineLogin).TotalHours;
            Logger.Verbose("Hourse since last login: " + hoursSinceLastLogin);
            int maxAllowedHours = (validDays * 24);
            Logger.Verbose("Maximum allowed hours: " + maxAllowedHours);
            return hoursSinceLastLogin <= maxAllowedHours;
        }

        public async Task<bool> GetLoginValidityAsync()
        {
            Settings.Settings legacySettings = Settings.Settings.Instance;
            DsrProfile dsr = await new LoginController().GetByDsrPhoneNumberAsync(legacySettings.DsrPhone);
            return this.LoginValid(
                dsr.LastOnlineLogin,
                    DateTime.Now,
                    legacySettings.ExpirePeriodInDays);
        }
    }
}
