using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Framework;

namespace SalesApp.Core.BL.Models.Security
{
    [Preserve(AllMembers = true)]
    public class Permission : BusinessEntityBase
    {
        public uint PermissionId { get; set; }

        public string Name { get; set; }
    }
}