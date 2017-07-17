using System;

namespace SalesApp.Core.Framework
{
    /// <summary>
    /// Implementation of the Preserve Attribute.
    /// </summary>
    public sealed class PreserveAttribute : Attribute
    {

        // #pragma warning disable SA1401 // Fields must be private
        /// <summary>
        /// Preserve all members in a class.
        /// </summary>
        public bool AllMembers;

        /// <summary>
        /// Preserve members when conditional statement is met.
        /// </summary>
        public bool Conditional;

        // #pragma warning restore SA1401 // Fields must be private
    }
}
