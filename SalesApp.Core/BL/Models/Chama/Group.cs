using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SalesApp.Core.Enums.Chama;

namespace SalesApp.Core.BL.Models.Chama
{
    public class Group : IEquatable<Group>
    {
        public int GroupId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<Option> Options { get; set; }

        public Option SelectedOption { get; set; }

        public int ParentId { get; set; }

        public bool Searching { get; set; }

        public GroupStatus Status { get; set; }

        [JsonIgnore]
        public bool Searchable
        {
            get
            {
                if (this.Searching)
                {
                    return true;
                }

                if (this.Options == null)
                {
                    return false;
                }

                return this.Options.Count >= 10;
            }
        }

        public bool Equals(Group other)
        {
            if (other == null)
            {
                return false;
            }

            return this.GroupId == other.GroupId;
        }
    }
}
