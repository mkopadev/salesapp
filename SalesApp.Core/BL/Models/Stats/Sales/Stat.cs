using System;
using SalesApp.Core.BL.Contracts;
using SalesApp.Core.BL.Controllers.Stats;
using SalesApp.Core.Framework;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace SalesApp.Core.BL.Models.Stats.Sales
{
    [Preserve(AllMembers = true)]
    public class Stat : BusinessEntityBase
    {
        private StatHeader _header;

        public DateTime Date { get; set; }

        public int Sales { get; set; }

        public int Prospects { get; set; }

        [ForeignKey(typeof(StatHeader))]
        public Guid StatHeaderId { get; set; }

        [Ignore]
        public StatHeader Header
        {
            get
            {
                if (this.StatHeaderId == default(Guid))
                {
                    return null;
                }

                if (this._header == null || this._header.Id != this.StatHeaderId)
                {
                    this._header = new StatHeaderController().GetByIdAsync(this.StatHeaderId).Result;
                }

                return this._header;
            }

            set
            {
                this._header = value;
            }
        }
    }
}