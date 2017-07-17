using System;
using SalesApp.Core.Enums.Stats;

namespace SalesApp.Core.BL.Models.Stats.Reporting
{
    /// <summary>
    /// This class represents a cache to store the user selection.
    /// </summary>
    public class SelectionCache
    {
        public static readonly int CountryLevel = 2;
        public static readonly int TopLevel = 2;
        public static readonly int BottomLevel = 0;

        private int selectedLevel = CountryLevel;
        private string selectedItemId = null;
        private Period selectedPeriodType = Period.Day;
        private string selectedPeriod = null;

        public int SelectedLevel
        {
            get { return this.selectedLevel; }
            set { this.selectedLevel = value; }
        }

        public string SelectedItemId
        {
            get { return this.selectedItemId; }
            set { this.selectedItemId = value; }
        }

        public Period SelectedPeriodType
        {
            get { return this.selectedPeriodType; }
            set { this.selectedPeriodType = value; }
        }

        public string SelectedPeriod
        {
            get { return this.selectedPeriod; }
            set { this.selectedPeriod = value; }
        }

        [Obsolete("Need to change when API changes are done")]
        public void LevelUp()
        {
            switch (this.selectedLevel)
            {
                case (int)SalesAreaHierarchy.Country:
                    this.selectedLevel = (int)SalesAreaHierarchy.Country;
                    break;
                case (int)SalesAreaHierarchy.Region:
                    this.selectedLevel = (int)SalesAreaHierarchy.Country;
                    break;
                case (int)SalesAreaHierarchy.ServiceCentre:
                    this.selectedLevel = (int)SalesAreaHierarchy.Region;
                    break;
            }
        }

        [Obsolete("Need to change when API changes are done")]
        public void LevelDown()
        {
            switch (this.selectedLevel)
            {
                case (int)SalesAreaHierarchy.Country:
                    this.selectedLevel = (int)SalesAreaHierarchy.Region;
                    break;
                case (int)SalesAreaHierarchy.Region:
                    this.selectedLevel = (int)SalesAreaHierarchy.ServiceCentre;
                    break;
                case (int)SalesAreaHierarchy.ServiceCentre:
                    this.selectedLevel = (int)SalesAreaHierarchy.ServiceCentre;
                    break;
            }
        }
    }
}
