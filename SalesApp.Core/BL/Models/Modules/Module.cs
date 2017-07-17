namespace SalesApp.Core.BL.Models.Modules
{
    /// <summary>
    /// Class represents a module, currently Videos, Facts and Calculator
    /// </summary>
    public class Module
    {
        /// <summary>
        /// The name of the module
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// The icon representing the module
        /// </summary>
        public int ModuleIcon { get; set; }
    }
}