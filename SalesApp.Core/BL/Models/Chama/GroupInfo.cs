using System;
using System.Collections.Generic;

namespace SalesApp.Core.BL.Models.Chama
{
    public class GroupInfo
    {
        public DateTime ServerTimeStamp { get; set; }

        public List<Group> Package { get; set; }
    }
}
