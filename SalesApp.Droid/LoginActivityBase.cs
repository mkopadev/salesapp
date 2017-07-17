using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Widget;
using SalesApp.Core.Api.OtaSettings;
using SalesApp.Core.Api.Security;
using SalesApp.Core.Auth;
using SalesApp.Core.BL.Controllers.Security;
using SalesApp.Core.BL.Models;
using SalesApp.Core.BL.Models.Security;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.Device;
using SalesApp.Core.Services.DependancyInjection;
using SalesApp.Core.Services.OtaSettings;
using SalesApp.Core.Services.Security;
using SalesApp.Core.Services.Settings;
using SalesApp.Core.Services.SharedPrefs;
using SalesApp.Droid.Api;
using SalesApp.Droid.Components.UIComponents;
using Uri = Android.Net.Uri;

namespace SalesApp.Droid
{
    public abstract class LoginActivityBase : ActivityBase2
    {
        protected string EnteredPin;
        protected int PinLength;
        protected Button LoginButton;
        protected EditText EditPin;
        protected TextView TxtPin1;
        protected TextView TxtPin2;
        protected TextView TxtPin3;
        protected TextView TxtPin4;
        protected TextView[] TxtPinArray;
        private LoginController _loginController;
        protected LoginService LoginService;
        protected RemoteOtaService _remoteOtaService;
        protected DsrProfile Dsr;
        protected Settings Settings = Settings.Instance;
        private bool _onlineLoginFailed;
        private bool _canLoginOffline = true;

        protected LoginController LoginController
        {
            get
            {
                if (_loginController == null)
                {
                    _loginController = new LoginController();
                }
                return _loginController;
            }
        }

        protected RemoteOtaService RemoteOtaService
        {
            get
            {
                OtaSettingsApi api = new OtaSettingsApi();
                LocalOtaService service = new LocalOtaService();

                return _remoteOtaService ?? (_remoteOtaService = new RemoteOtaService(api, service));
            }
        }

        public abstract void HideLoginResultMessage();

        protected void DisableLoginButton()
        {
            LoginButton.Text = GetString(Resource.String.logging_in_button);
            LoginButton.Enabled = false;
        }

        protected void EnableLoginButton()
        {
            LoginButton.Text = GetString(Resource.String.log_in_button);
            LoginButton.Enabled = true;
        }

        public abstract void ContinueToWelcome();


        public abstract void ShowLoginResultMessage(string text, bool isInformational);

        public void ShowLoginResultMessage(int resId)
        {
            ShowLoginResultMessage(GetString(resId));
        }

        public void ShowLoginResultMessage(int resId, bool isInformational)
        {
            this.ShowLoginResultMessage(GetString(resId), isInformational);
        }

        public void ShowLoginResultMessage(string text)
        {
            this.ShowLoginResultMessage(text, false);
        }

        public override void RetrieveScreenInput()
        {
            EnteredPin = EditPin.Text;
            PinLength = EnteredPin.Length;
        }

        protected void ChangeFocusColor()
        {
            // reset all to grey
            for (int x = 0; x <= 3; x++)
            {
                TxtPinArray[x].SetBackgroundResource(Resource.Drawable.background_pin);
            }

            var length = EditPin.Text.Length;

            // if it has focus, check length of the content and set the next item to grey
            if (EditPin.HasFocus)
            {
                // beware to not go beyond available boxes
                if (length < 4)
                {
                    TxtPinArray[EditPin.Text.Length].SetBackgroundResource(Resource.Drawable.background_pin_focus);
                }
            }
            else
            {
                HideKeyboard(true);
            }

            // regardless of focus, if 4 numbers, highlight them all
            if (length == 4)
            {
                for (int x = 0; x <= 3; x++)
                {
                    TxtPinArray[x].SetBackgroundResource(Resource.Drawable.background_pin_focus);
                }
            }
        }

        public override void SetListeners()
        {
            EditPin.FocusChange += delegate
            {
                ChangeFocusColor();
            };

            EditPin.AfterTextChanged += delegate
            {
                // grab all text from edit text
                RetrieveScreenInput();
                UpdateScreen();
                // assign pin numbers to TextViews for show
                for (int x = 0; x <= 3; x++)
                {
                    TxtPinArray[x].Text = PinLength >= x + 1 ? EnteredPin[x].ToString() : string.Empty;
                }

                ChangeFocusColor();

                this.LoginButton.Enabled = PinLength >= 4;
                // if 4 numbers, hide the keyboard and focus the login button
                if (PinLength == 4)
                {
                    EditPin.ClearFocus();
                    HideKeyboard(true);
                    this.LoginButton.RequestFocus();
                }
            };
        }

        /// <summary>
        /// If <paramref name="withRetry"/> is true, this method will try online login three times and if they all fail, the next time it is called with
        /// <paramref name="withRetry"/> as true, it will automatically try offline login.
        /// If however <paramref name="withRetry"/> is false the method will only try online login once and wont revert to offline if it fails.
        /// </summary>
        /// <param name="withRetry">A boolean indicating whether or not to try 3 times</param>
        /// <returns>An empty task</returns>
        protected async Task LoginOnline(bool withRetry = false)
        {
            this.HideLoginResultMessage();

            // change the login text and disable the button
            this.DisableLoginButton();

            var loginApis = new LoginApis();
            LoginResponse loginResponse;
            if (withRetry)
            {
                // the previous 3 online login tries failed, so we automatically switch to online
                if (this._onlineLoginFailed && this._canLoginOffline)
                {
                    this.LoginOffline();
                    this.EnableLoginButton();
                    return;
                }

                loginResponse = await loginApis.Login(Settings.Instance.DsrPhone, this.EnteredPin, ErrorFilterFlags.DisableErrorHandling);
            }
            else
            {
                loginResponse = await loginApis.Login(Settings.Instance.DsrPhone, this.EnteredPin, true, ErrorFilterFlags.DisableErrorHandling);
            }
            
            bool loginSuccess = false;
            switch (loginResponse.Code)
            {
                case LoginResponseCode.HttpError:
                case LoginResponseCode.Unknown:
                    if (withRetry && this._canLoginOffline)
                    {
                        this.ShowLoginResultMessage(Resource.String.online_login_failed);
                        this._onlineLoginFailed = true;
                    }
                    else
                    {
                        this.ShowLoginResultMessage(Resource.String.something_wrong_try_again_login);
                    }
                    break;
                case LoginResponseCode.Unauthorized:
                    this.ShowLoginResultMessage(Resource.String.wrong_pin);
                    break;
                case LoginResponseCode.WrongParameters:
                    this.ShowLoginResultMessage(Resource.String.something_wrong_try_again);
                    break;
                case LoginResponseCode.Success:
                    loginSuccess = true;
                    break;
            }

            // grab profile from response, we're still good
            if (loginSuccess)
            {
                ISharedPrefService sharedPreferences = Resolver.Instance.Get<ISharedPrefService>();
                sharedPreferences.Save(LoginService.IsFirstLogin, false);
                
                // check whether the app is up to date
                switch (loginResponse.AppCompatibility)
                {
                    case AppCompatibility.UpdateAvailable:
                        this.ShowDialog(this.GetString(Resource.String.update_preferred), this.GetString(Resource.String.update), this.GetString(Resource.String.not_now),
                            (sender, args) =>
                            {
                                this.GoToPlayStore();
                            },
                            (sender, args) =>
                            {
                                Task.Run(async () =>
                                {
                                    try
                                    {
                                        await this.ProceedWithLogin(loginResponse);
                                    }
                                    catch(Exception exception)
                                    {
                                        Logger.Debug(exception);
                                        throw;
                                    }
                                });
                            });
                        break;

                    case AppCompatibility.UpdateRequired:
                        this.ShowDialog(this.GetString(Resource.String.update_required), this.GetString(Resource.String.update),
                            (sender, args) =>
                            {
                                this.GoToPlayStore();
                            });
                        break;

                    case AppCompatibility.UpToDate:
                        await this.ProceedWithLogin(loginResponse);
                        break;

                    case AppCompatibility.Unknown:
                        await this.ProceedWithLogin(loginResponse);
                        break;
                }
            }

            this.EnableLoginButton();
        }

        protected async Task LoginOffline()
        {
            // grab the user from local resource
            Dsr = await LoginController.GetByDsrPhoneNumberAsync(Settings.DsrPhone);

            // no local profile present
            if (Dsr == null)
            {
                ShowLoginResultMessage(Resource.String.no_offline_profile);
                this._canLoginOffline = false;
                return;
            }

            // profile exists, continue

            // check whether it has been too long since the user logged in online
            if (!LoginService.LoginValid(Dsr.LastOnlineLogin, DateTime.Today, Settings.ExpirePeriodInDays))
            {
                ShowLoginResultMessage(string.Format(GetString(Resource.String.logging_in_too_long), Settings.ExpirePeriodInDays));
                this._canLoginOffline = false;
                return;
            }

            // check the amount of times logged in offline
            if (Dsr.OfflineLoginAttempts >= Settings.OfflineLoginAttempts)
            {
                ShowLoginResultMessage(string.Format(GetString(Resource.String.logging_in_offline_expire), Dsr.OfflineLoginAttempts));
                this._canLoginOffline = false;
                return;
            }

            // we're still ok, check the hash
            IHashing hashing = Resolver.Instance.Get<IHashing>();
            string hash = hashing.HashPassword(Settings.DsrPhone, EnteredPin);

            if (hash != Dsr.PinHash)
            {
                Dsr.OfflineLoginAttempts++;
                await LoginController.SaveAsync(Dsr);
                ShowLoginResultMessage(Resource.String.wrong_pin);
                return;
            }

            // seem to be right PIN, so continue
            Dsr.LastOfflineLogin = DateTime.Now;
            Dsr.OfflineLoginAttempts = 0;
            await LoginController.SaveAsync(Dsr);
            ContinueToWelcome();
        }

        private async Task ProceedWithLogin(LoginResponse loginResponse)
        {
            // check if local profile exists
            this.Dsr = await this.LoginController.GetDsr(loginResponse.Id);

            // create new profile
            if (this.Dsr == null)
            {
                this.Dsr = new DsrProfile
                {
                    DsrPhone = Settings.Instance.DsrPhone,
                    FirstName = loginResponse.FirstName,
                    LastName = loginResponse.LastName,
                    LastOfflineLogin = loginResponse.LastOfflineLogin,
                    LastOnlineLogin = loginResponse.LastOnlineLogin,
                    OfflineLoginAttempts = loginResponse.OfflineLoginAttempts,
                    PinHash = loginResponse.PinHash,
                    Id = loginResponse.Id
                };
            }
            else
            {
                // update existing profile
                this.Dsr.DsrPhone = Settings.Instance.DsrPhone;
                this.Dsr.FirstName = loginResponse.FirstName;
                this.Dsr.LastName = loginResponse.LastName;
                this.Dsr.PinHash = loginResponse.PinHash;
                this.Dsr.Id = loginResponse.Id;
            }

            // when dsr is present we have valid user
            this.CreateUserSession();
            this.Dsr.LastOnlineLogin = DateTime.Now;
            this.Dsr.LastOfflineLogin = DateTime.Now;
            this.Dsr.OfflineLoginAttempts = 0;

            await this.LoginController.SaveAsync(this.Dsr);

            // fetch and update settings via OTA
            var serverTimestamp = Settings.Instance.ServerTimeStamp;
            string carrier = this.OperatorName;
            var requestParams = string.Format("?carrier={0}&servertimestamp={1}", carrier, serverTimestamp);
            await this.RemoteOtaService.FetchOtaSettingsAsync(requestParams);

            // continue to new screen
            this.ContinueToWelcome();
        }

        private void GoToPlayStore()
        {
            string appPackageName = this.PackageName;
            try
            {
                this.StartActivity(new Intent(Intent.ActionView, Uri.Parse("market://details?id=" + appPackageName)));
            }
            catch (ActivityNotFoundException anfe)
            {
                this.StartActivity(new Intent(Intent.ActionView, Uri.Parse("https://play.google.com/store/apps/details?id=" + appPackageName)));
            }
        }

        protected void CreateUserSession()
        {
            ISalesAppSession session = Resolver.Instance.Get<ISalesAppSession>();
            session.FirstName = Dsr.FirstName;
            session.LastName = Dsr.LastName;
            session.UserId = Dsr.Id;
        }

    }
}