using System;
using SalesApp.Core.BL.Contracts;
using SQLiteNetExtensions.Attributes;

namespace SalesApp.Core.BL.Models.Syncing
{
    public class ModelChannel : BusinessEntityBase
    {
        public string ModelName { get; set; }

        [ForeignKey(typeof(SyncChannel))]
        public Guid ChannelId { get; set; }

        [ManyToOne]
        public SyncChannel Channel { get; set; }
    }
}