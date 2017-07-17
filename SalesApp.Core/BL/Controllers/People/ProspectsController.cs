using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SalesApp.Core.BL.Models.People;
using SalesApp.Core.Enums.MultiCountry;
using SalesApp.Core.Logging;
using SalesApp.Core.Services.Database;

namespace SalesApp.Core.BL.Controllers.People
{
    public class ProspectsController : PeopleController<Prospect>
    {
        private ProspectFollowUpsController _followUpsController;

        /// <summary>
        /// Controller to help save and retrieve prospect products
        /// </summary>
        private ProspectProductController prospectProductController;

        private ILog Logger = LogManager.Get(typeof(ProspectsController));

        public ProspectsController()
        {
        }

        public ProspectsController(LanguagesEnum lang, CountryCodes country) : base()
        {
        }

        /// <summary>
        /// Gets the prospect product controller
        /// </summary>
        private ProspectProductController ProspectProductController
        {
            get
            {
                return this.prospectProductController ?? (this.prospectProductController = new ProspectProductController());
            }
        }

        private ProspectFollowUpsController FollowUpsController
        {
            get
            {
                if (_followUpsController == null)
                {
                    _followUpsController = new ProspectFollowUpsController();
                }

                return _followUpsController;
            }
        }

        public async override Task<List<Prospect>> GetAllAsync()
        {
            string query = "SELECT p.*, pf.ReminderTime AS ReminderTime, sr.Status AS SyncStatus FROM Prospect p LEFT JOIN ProspectFollowup pf ON p.Id = pf.ProspectId LEFT JOIN SyncRecord sr ON sr.RequestId=p.RequestId WHERE p.Converted = 0 ORDER BY pf.ReminderTime ASC, p.DateCreated DESC";
            List<Prospect> prospects = await new QueryRunner().RunQuery<Prospect>(query);
            return prospects;
        }

        public override List<Prospect> GetAll()
        {
            List<Prospect> prospects = base.GetAll();
            foreach (var prospect in prospects)
            {
                prospect.ReminderTime = FollowUpsController.GetReminderDate(prospect.Id).Result;
            }

            return prospects.OrderBy(prospect => prospect.ReminderTime).ToList();
        }

        public async Task<SaveResponse<Prospect>> SaveProspectAsync(Prospect model)
        {
            await base.SaveAsync(model);
            return null;
        }

        public async override Task<SaveResponse<Prospect>> SaveAsync(Prospect model)
        {
            try
            {
                SaveResponse<Prospect> saveResult = null;
                if (!this.ValidateBasicInfo(model))
                {
                    return default(SaveResponse<Prospect>);
                }

                await DataAccess.Instance.Connection.RunInTransactionAsync(
                    async connTran =>
                    {
                        DataAccess.Instance.StartTransaction(connTran);
                        if (model.Id == default(Guid))
                        {
                            model.Converted = false;
                        }

                        this.Logger.Debug("Before Validation");

                        this.Logger.Debug("After Validation");
                        saveResult = await base.SaveAsync(connTran, model);

                        // don't save prospect product when aproduct doesnt have a prospect
                        // This is important when saving prospect from api(Downsync)
                        // Update by Martin Lugaliki
                        if (model.Product != null)
                        {
                            ProspectProduct prospectProduct = JsonConvert.DeserializeObject<ProspectProduct>(JsonConvert.SerializeObject(model.Product));
                            prospectProduct.ProspectId = model.Id;
                            await new ProspectProductController().SaveAsync(connTran, prospectProduct);
                        }
                    });

                DataAccess.Instance.CommitTransaction();
                return saveResult;
            }
            catch (Exception exception)
            {
                this.Logger.Error(exception);
                throw;
            }
        }
    }
}
