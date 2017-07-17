namespace SalesApp.Core.BL.Models.Stats.Reporting
{
    public class ReportStat
    {
        /// <summary>
        /// Date of the Report Stat.
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Rank of the item in the stat list.
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// Amount of Sales of the item.
        /// </summary>
        public int Sales { get; set; }

        /// <summary>
        /// Amount of prospect of the item.
        /// </summary>
        public int Prospects { get; set; }

        /// <summary>
        /// Name of the item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Id of the item (unique).
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Id of the item (unique).
        /// </summary>
        public int? Level { get; set; }

        /// <summary>
        /// No. of DSRs of the item.
        /// </summary>
        public int Dsrs { get; set; }
    }
}
