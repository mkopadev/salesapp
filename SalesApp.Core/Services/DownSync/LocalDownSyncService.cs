using System.Threading.Tasks;
using SalesApp.Core.BL.Controllers.DownSync;
using SalesApp.Core.BL.Models.Syncing;
using SalesApp.Core.Services.Database.Querying;

namespace SalesApp.Core.Services.DownSync
{
    /// <summary>
    /// Class for SQLITE database related functions for Down sync
    /// </summary>
    public class LocalDownSyncService
    {
        /// <summary>
        /// The Down Sync controller
        /// </summary>
        private DownSyncController controller;

        /// <summary>
        /// Gets the Down sync controller
        /// </summary>
        private DownSyncController Controller
        {
            get
            {
                this.controller = this.controller ?? new DownSyncController();
                return this.controller;
            }
        }

        /// <summary>
        /// Gets an entity's down sync track record
        /// </summary>
        /// <param name="entity">The entity name to get</param>
        /// <returns>The tracking record matching the given entity</returns>
        public async Task<DownSyncTracker> Get(string entity)
        {
            CriteriaBuilder criteriaBuilder = new CriteriaBuilder();
            criteriaBuilder.Add("Entity", entity);
            return await this.Controller.GetSingleByCriteria(criteriaBuilder);
        }

        /// <summary>
        /// Save a record to the database give the record
        /// </summary>
        /// <param name="record">The record to save</param>
        /// <returns>Returns a void task</returns>
        public async Task Save(DownSyncTracker record)
        {
            if (record == null)
            {
                return;
            }

            DownSyncTracker existing = await this.Get(record.Entity);

            if (existing.Entity == null)
            {
                // new record, save it
                await this.controller.SaveAsync(record);
                return;
            }

            // this is not a new record, just update the existing
            existing.ServerTimestamp = record.ServerTimestamp;
            existing.IsInitial = record.IsInitial;
            await this.controller.SaveAsync(existing);
        }
    }
}