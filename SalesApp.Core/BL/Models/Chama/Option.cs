using System;
using SalesApp.Core.Enums.Chama;

namespace SalesApp.Core.BL.Models.Chama
{
    public class Option : IEquatable<Option>
    {
        public int OptionId { get; set; }

        public string OptionName { get; set; }

        public GroupStatus Status { get; set; }

        public bool Equals(Option other)
        {
            if (other == null)
            {
                return false;
            }

            return this.OptionId == other.OptionId;
        }
    }
}
