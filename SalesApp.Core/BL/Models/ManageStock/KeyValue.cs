using System;

namespace SalesApp.Core.BL.Models.ManageStock
{
    public class KeyValue : IEquatable<KeyValue>
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public bool Equals(KeyValue other)
        {
            return this.Key == other.Key && this.Value == other.Value;
        }
    }
}