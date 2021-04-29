using System.Collections.Generic;

namespace Pds.Contracts.FeedProcessor.Services.Models
{
    /// <summary>
    /// Atom feed page.
    /// </summary>
    public class FeedPage
    {
        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        /// <value>
        /// The entries.
        /// </value>
        public IList<FeedEntry> Entries { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is self page.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is self page; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelfPage { get; set; }

        /// <summary>
        /// Gets or sets the previous page number.
        /// </summary>
        /// <value>
        /// The previous page number.
        /// </value>
        public int PreviousPageNumber { get; set; }

        /// <summary>
        /// Gets or sets the next page number.
        /// </summary>
        /// <value>
        /// The next page number.
        /// </value>
        public int NextPageNumber { get; set; }

        /// <summary>
        /// Gets or sets the current page number.
        /// </summary>
        /// <value>
        /// The current page number.
        /// </value>
        public int CurrentPageNumber { get; set; }
    }
}