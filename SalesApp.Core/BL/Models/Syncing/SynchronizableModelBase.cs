using System;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Enums.Api;
using SalesApp.Core.Enums.Syncing;
using SalesApp.Core.Framework;
using SalesApp.Core.Services.Database;
using SQLite.Net.Attributes;

namespace SalesApp.Core.BL.Models.Syncing
{
    public abstract class SynchronizableModelBase : BusinessEntityBase
    {
        [Preserve]
        private Guid _requestId;

        public virtual DataChannel Channel { get; set; }

        public RecordStatus SyncStatus { get; set; }

        [Preserve]
        public Guid RequestId
        {
            get
            {
                if (this._requestId == default(Guid))
                {
                    SequentialGuid.GuidGen++;
                    this._requestId = SequentialGuid.GuidGen.CurrentGuid;
                }

                return this._requestId;
            }

            set
            {
                this._requestId = value;
            }
        }

        [Ignore]
        public bool DontSync { get;  set; }
    }
}