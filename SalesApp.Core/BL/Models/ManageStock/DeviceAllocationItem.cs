using System;

namespace SalesApp.Core.BL.Models.ManageStock
{
    public class DeviceAllocationItem : IEquatable<DeviceAllocationItem>
    {
        public string SerialNumber { get; set; }

        public DateTime DateAllocated { get; set; }

        public string HeaderText { get; set; }

        public string Name { get; set; }

        public Guid ProductTypeId { get; set; }

        public bool IsFooterItem { get; set; }

        public bool IsSelectable { get; set; }

        public bool Equals(DeviceAllocationItem other)
        {
            if (other == null)
            {
                return false;
            }

            return other.SerialNumber == this.SerialNumber;
        }
    }
}