using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Core.BL.Models.OtaSettings;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database;
using SalesApp.Core.Services.Database.Querying;

namespace SalesApp.Core.BL.Controllers.OtaSettings
{
    /// <summary>
    /// Controller for OTA Settings
    /// </summary>
    public class OtaSettingsController : SQLiteDataService<OtaSetting>
    {
        private ILog Logger = LogManager.Get(typeof(OtaSettingsController));

        public async new Task SaveBulkAsync(List<OtaSetting> settings)
        {
            this.Logger.Debug("Logging data");
            try
            {
                foreach (var s in settings)
                {
                    CriteriaBuilder criteriaBuilder = new CriteriaBuilder();
                    criteriaBuilder.Add("GroupName", s.GroupName).Add("Name", s.Name);
                    string sql = this.QueryBuilder(criteriaBuilder);
                    List<OtaSetting> existingOtaSettings = await this.SelectQueryAsync(sql);

                    OtaSetting existing = (existingOtaSettings == null || existingOtaSettings.Count == 0) ? null : existingOtaSettings[0];

                    if (existing == null || existing.Id == default(Guid))
                    {
                        this.Logger.Debug(string.Format("Saving OtaSetting {0} for the first time.", s.Name));
                        await this.SaveAsync(s);
                    }
                    else
                    {
                        this.Logger.Debug(
                            string.Format("Updating existing OtaSetting {0} with {1}", existing.Value, s.Value));
                        existing.Value = s.Value;
                        await this.SaveAsync(existing);
                    }
                }
            }
            catch (Exception exception)
            {
                this.Logger.Debug(exception);
                throw;
            }
        }
    }
}