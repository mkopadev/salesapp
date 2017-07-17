using System;
using SalesApp.Core.BL.Contracts;

namespace SalesApp.Core.BL.Models.People
{
    public class ProspectFollowup : BusinessEntityBase
    {
        public Guid ProspectId { get; set; }
        public DateTime ReminderTime { get; set; }
    }
}