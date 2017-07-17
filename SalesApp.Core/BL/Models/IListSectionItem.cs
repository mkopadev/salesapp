namespace SalesApp.Core.BL.Models
{
    /// <summary>
    /// An interface to be implemented by List items that want to support sections within the list
    /// </summary>
    public interface IListSectionItem
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not the item is a section header
        /// </summary>
        bool IsSectionHeader { get; set; }

        /// <summary>
        /// Gets or sets the header title of the section
        /// </summary>
        string SectionHeader { get; }
    }
}