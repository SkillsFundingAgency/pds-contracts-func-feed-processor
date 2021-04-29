using System;

namespace Pds.Contracts.FeedProcessor.Services.Models
{
    /// <summary>
    /// Represents an entry from an Atom Feed.
    /// </summary>
    public class FeedEntry
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the updated date time for an entry.
        /// </summary>
        public DateTime Updated { get; set; }

        /// <summary>
        /// Gets or sets the contents for the entry.
        /// </summary>
        public string Content { get; set; }
    }
}
