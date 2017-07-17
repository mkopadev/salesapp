using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.Api;
using SalesApp.Core.Api.OtaSettings;
using SalesApp.Core.BL.Models.OtaSettings;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Exceptions.API;
using SalesApp.Core.Services.RemoteServices;

namespace SalesApp.Core.Services.OtaSettings
{
    /// <summary>
    /// Class for requesting for new settings from the server
    /// </summary>
    public class RemoteOtaService : RemoteServiceBase<OtaSettingsApi, OtaSetting, OtaServerResponse>
    {
        /// <summary>
        /// The local service
        /// </summary>
        private readonly LocalOtaService localOtaService;

        /// <summary>
        /// The API
        /// </summary>
        private readonly OtaSettingsApi otaSettingsApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteOtaService"/> class.
        /// </summary>
        /// <param name="api">The API </param>
        /// <param name="localOtaService">The local service for saving to database</param>
        public RemoteOtaService(OtaSettingsApi api, LocalOtaService localOtaService)
        {
            this.localOtaService = localOtaService;
            this.otaSettingsApi = api;
        }

        /// <summary>
        /// Request new settings from the server
        /// </summary>
        /// <param name="requestParams">The request parameters</param>
        /// <returns>An empty task</returns>
        public async Task FetchOtaSettingsAsync(string requestParams)
        {
            string jsonString = string.Empty;

            try
            {
                this.Logger.Debug("Reqesting OTA Settings in a few.");
                ServerResponse<OtaServerResponse> serverResponse =
                    await this.otaSettingsApi.MakeGetCallAsync<OtaServerResponse>(requestParams, null, ErrorFilterFlags.AllowEmptyResponses, ApiTimeoutEnum.VeryLong);

                this.Logger.Debug("OTA Settings Request Done.");

                if (serverResponse == null)
                {
                    this.Logger.Debug("Empty response!");
                }
                else if (serverResponse.IsSuccessStatus)
                {
                    jsonString = serverResponse.RawResponse;
                    this.Logger.Debug("Raw Json : " + jsonString);
                    OtaServerResponse otaServerResponse = serverResponse.GetObject();

                    string serverTimeStamp = otaServerResponse.ServerTimestamp;

                    // Save the header settings
                    OtaSetting serverTimeStampSetting = new OtaSetting
                    {
                        GroupName = LocalOtaService.Communication,
                        Name = "ServerTimeStamp",
                        Value = serverTimeStamp
                    };

                    List<SettingsGroup> settingGroups = otaServerResponse.SettingsGroups;

                    await this.localOtaService.SetSettingsValue(serverTimeStampSetting);

                    foreach (var settingsGroup in settingGroups)
                    {
                        List<OtaSetting> settings = settingsGroup.Settings;
                        foreach (var s in settings)
                        {
                            s.GroupName = settingsGroup.Name;
                            await this.localOtaService.SetSettingsValue(s);
                        }
                    }
                }
            }
            catch (JsonReaderException jsonReaderException)
            {
                this.Logger.Error("Invalid JSON could not be parsed!");
                this.Logger.Error("JSON: " + jsonString);
                this.Logger.Error(jsonReaderException);
            }
            catch (NotConnectedToInternetException notConnectedToInternetException)
            {
                this.Logger.Error("Unable to connect internet. Could connection have dropped?");
                this.Logger.Error(notConnectedToInternetException);
            }
            catch (TaskCanceledException taskCanceled)
            {
                this.Logger.Error("Timeout may have occured or task may have been explicitly canceled by user.");
                this.Logger.Error(taskCanceled);
            }
            catch (Exception exception)
            {
                this.Logger.Error(exception);
            }
        }
    }
}
