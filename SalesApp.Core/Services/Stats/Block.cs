using System;

namespace SalesApp.Core.Services.Stats
{
    public class Block
    {
        public string TopValue { get; set; }

        public string BottomValue { get; set; }

        public string Caption { get; set; }

        public int Level { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}