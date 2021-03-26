using System;

namespace Pds.Contracts.FeedProcessor.Services.Models
{
    /// <summary>
    /// FCS atom feed.
    /// </summary>
    public class FcsAtomFeed
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the updated.
        /// </summary>
        /// <value>
        /// The updated.
        /// </value>
        public DateTime Updated { get; set; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public FeedEntry[] FeedEntries { get; set; }
    }
}