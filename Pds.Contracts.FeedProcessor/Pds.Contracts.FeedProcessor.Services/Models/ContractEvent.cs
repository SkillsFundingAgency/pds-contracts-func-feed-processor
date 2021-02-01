using System;
using System.Collections.Generic;
using System.Text;

namespace Pds.Contracts.FeedProcessor.Services.Models
{
    /// <summary>
    /// Atom feed.
    /// </summary>
    public class ContractEvent
    {
        /// <summary>
        /// Gets or sets the example message.
        /// </summary>
        /// <value>
        /// The example message.
        /// </value>
        public string ExampleMessage { get; set; }

        /// <summary>
        /// Gets or sets the example feed time.
        /// </summary>
        /// <value>
        /// The example feed time.
        /// </value>
        public DateTime ExampleFeedTime { get; set; }

        /// <summary>
        /// Gets the example sequence identifier.
        /// </summary>
        /// <value>
        /// The example sequence identifier.
        /// </value>
        public int ExampleSequenceId { get; set; }
    }
}
