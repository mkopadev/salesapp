using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Controllers.Security;
using SalesApp.Core.BL.Models.Security;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.Security;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.Device;

namespace SalesApp.Core.Api.Security
{
    public class LoginApis : ApiBase
    {
        /// <summary>
        /// The maximum number of tries for online login before automatically reverting to offline
        /// </summary>
        private readonly int MaxOnlineTries = 2;

        public LoginApis() : base("login")
        {
        }

        /// <summary>
        /// This is the method that is used for post device regiatration login.
        /// It will automatically try to login three time before returning
        /// </summary>
        /// <param name="phone">The phone number of the individual logging in</param>
        /// <param name="pin">The associated pin</param>
        /// <param name="filterFlags">Flags to help ignore some API errors</param>
        /// <returns>Null if login failed or on success a DsrProfile object containing details of user who's logged in</returns>
        public async Task<LoginResponse> Login(string phone, string pin, ErrorFilterFlags filterFlags)
        {
            for (int i = 0; i < this.MaxOnlineTries; i++)
            {
                LoginResponse response = await this.Login(phone, pin, false, filterFlags);

                if (response.Code == LoginResponseCode.Success || response.Code == LoginResponseCode.Unauthorized)
                {
                    if (response.Code == LoginResponseCode.Success)
                    {
                        this.Logger.Debug(string.Format("Online login succeeded at attempt {0}", i));
                    }
                    else
                    {
                        this.Logger.Debug(string.Format("Wrong credentials at attempt {0}", i));
                    }

                    return response;
                }

                this.Logger.Debug(string.Format("Online login failed at attempt {0}", i));
            }

            this.Logger.Debug(string.Format("Online login failed after {0} attempts", this.MaxOnlineTries));
            return new LoginResponse() { Code = LoginResponseCode.Unknown };
        }

        /// <summary>
        /// Attempts to perform an online login
        /// </summary>
        /// <param name="phone">The phone number of the individual logging in</param>
        /// <param name="pin">The associated pin</param>
        /// <param name="isFirstTime">Flag for whether it is the first time the individual is log</param>
        /// <param name="filterFlags">Flags to help ignore some API errors</param>
        /// <returns>Null if login failed or on success a DsrProfile object containing details of user who's logged in</returns>
        public async Task<LoginResponse> Login(string phone, string pin, bool isFirstTime, ErrorFilterFlags filterFlags)
        {
            try
            {
                if (pin == null)
                {
                    return new LoginResponse { Code = LoginResponseCode.WrongParameters };
                }

                if (phone == null)
                {
                    return new LoginResponse { Code = LoginResponseCode.WrongParameters };
                }

                IHashing hashing = Resolver.Instance.Get<IHashing>();
                string hash = hashing.HashPassword(phone, pin);

                string credentials = string.Format("{0}:{1}", phone, hash);
                byte[] bytes = hashing.GetBytes(credentials);

                string base64 = Convert.ToBase64String(bytes);

                this.RemoveHeader("Authorization");
                this.AddHeader("Authorization", " Basic " + base64);
                ServerResponse<LoginResponse> response = await PostObjectAsync<LoginResponse, LoginDto>(
                        new LoginDto
                        {
                            Hash = hash,
                            IsFirstLogin = isFirstTime,
                            DeviceInformation = Resolver.Instance.Get<IInformation>()
                        },
                        null,
                        filterFlags);

                this.Logger.Debug("Call to login api completed");

                if (response == null)
                {
                    this.Logger.Debug("Response is null");
                    return new LoginResponse() { Code = LoginResponseCode.HttpError };
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    this.Logger.Debug("HttpStatusCode.Unauthorized");
                    return new LoginResponse() { Code = LoginResponseCode.Unauthorized };
                }

                if (!response.IsSuccessStatus)
                {
                    this.Logger.Debug("IsSuccessStatus = false");
                    return new LoginResponse() { Code = LoginResponseCode.HttpError };
                }

                this.Logger.Debug("Persisting user hash");
                Resolver.Instance.Get<ISalesAppSession>().UserHash = base64;

                this.Logger.Debug("deserializing response text to object");
                LoginResponse loginResponse = response.GetObject();

                if (loginResponse.Permissions == null || !loginResponse.Permissions.Any())
                {
                    this.Logger.Debug("Looks like we don't yet support permissions. Lets fake some.");
                    var vals = Enum.GetNames(typeof(Permissions));
                    loginResponse.Permissions = new List<Permission>();
                    foreach (string value in vals)
                    {
                        this.Logger.Debug(string.Format("Faking permission: {0}", value));

                        loginResponse.Permissions.Add(
                                new Permission
                                {
                                    Name = value,
                                    PermissionId = (uint)Enum.Parse(typeof(Permissions), value)
                                });
                    }
                }

                this.Logger.Debug(string.Format("Updating permissions total permissions count {0}", loginResponse.Permissions.Count));
                await PermissionsController.Instance.UpdatePermissionsAsync(loginResponse.Permissions);

                this.Logger.Debug("Login went smoothly... Exiting method and returning result");
                loginResponse.Code = LoginResponseCode.Success;
                return loginResponse;
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex);
                return new LoginResponse() { Code = LoginResponseCode.Unknown };
            }
        }
    }
}