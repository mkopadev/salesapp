using System;
using System.Collections.Generic;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.Enums.Stats;
using SalesApp.Core.Framework;
using SQLiteNetExtensions.Attributes;

namespace SalesApp.Core.BL.Models.Stats.Sales
{
    [Preserve(AllMembers = true)]
    public class StatHeader : BusinessEntityBase
    {
        public Period Period { get; set; }

        public Guid UserId { get; set; }

        public DateTime From { get; set; }

        public DateTime To { get; set; }

        [OneToMany]
        public List<Stat> Stats { get; set; }
    }
}