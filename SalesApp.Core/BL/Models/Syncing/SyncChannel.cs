using SalesApp.Core.BL.Contracts;

namespace SalesApp.Core.BL.Models.Syncing
{
    public class SyncChannel : BusinessEntityBase
    {
        public string ChannelName { get; set; }
        public int ChannelCode { get; set; }
    }
}