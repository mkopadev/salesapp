using System.Collections.Generic;

namespace SalesApp.Core.Services.Stats
{
    public class Row
    {
        /// <summary>
        /// String of row titles.
        /// </summary>
        public List<string> Items { get; set; }

        /// <summary>
        /// Boolean indicating the row is selected/highlighted.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Allows for setting a Tag on the list item.
        /// </summary>
        public object Tag { get; set; }
    }
}