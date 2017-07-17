using SalesApp.Core.Enums;

namespace SalesApp.Core.BL.Models.Stats.Reporting
{
    /// <summary>
    /// This class represents the ReportingLevel stats.
    /// [This class does not yet extend the default model, currently it is not stored in the database.]
    /// </summary>
    public class ReportingLevelEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingLevelEntity"/> class.
        /// </summary>
        public ReportingLevelEntity()
        {
            this.Status = ServiceReturnStatus.InitialData;
            this.Parent = new ParentItem();
            this.Name = "-";
            this.StatsType = "-";
            this.ReportStatsType = "-";
            this.Sales = new[] { new Sale(), new Sale(), new Sale() };
            this.ReportStats = new ReportStat[0];
        }

        /// <summary>
        /// Returns whether the object contains valid data.
        /// </summary>
        public bool HasValidData
        {
            get { return this.Status == ServiceReturnStatus.Success; }
        }

        /// <summary>
        /// The status of this object, coming from the server.
        /// </summary>
        public ServiceReturnStatus Status { get; set; }

        /// <summary>
        /// Parent item of the current item.
        /// </summary>
        public ParentItem Parent { get; set; }

        /// <summary>
        /// Name of the stats entity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type of the stats entity (Level name).
        /// </summary>
        public string StatsType { get; set; }

        /// <summary>
        /// Type of the stats this entity is reporting over in the list/child.
        /// </summary>
        public string ReportStatsType { get; set; }

        /// <summary>
        /// Represents a block of sales for the current entity.
        /// </summary>
        public Sale[] Sales { get; set; }

        /// <summary>
        /// Represents the children of the entity and their stats.
        /// </summary>
        public ReportStat[] ReportStats { get; set; }

        /// <summary>
        /// This class represents a salesblock on the reporting level stats.
        /// </summary>
        public class Sale
        {
            public Sale()
            {
                this.Name = "-";
                this.Value = "-";
            }

            /// <summary>
            /// Name of the sales block.
            /// </summary>
            public string Name { get;set; }

            /// <summary>
            /// Value of the sales block.
            /// </summary>
            public string Value { get; set; }
        }

        public class ParentItem
        {
            /// <summary>
            /// Id of the parent.
            /// </summary>
            public string ParentId { get; set; }

            /// <summary>
            /// Level of the parent.
            /// </summary>
            public int? Level { get; set; }
        }

        public class Item
        {
            /// <summary>
            /// Id of the parent.
            /// </summary>
            public string ItemId { get; set; }

            /// <summary>
            /// Level of the parent.
            /// </summary>
            public int? Level { get; set; }
        }
    }
}
