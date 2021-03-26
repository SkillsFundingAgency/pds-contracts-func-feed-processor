using System;

namespace Pds.Contracts.FeedProcessor.Services.Configuration
{
    /// <summary>
    /// Options for blob storage access to store orignial raw contract event xml.
    /// </summary>
    public class BlobAccessOptions
    {
        /// <summary>
        /// Gets or sets the XML storage container.
        /// </summary>
        /// <value>
        /// The XML storage container.
        /// </value>
        public string XmlStorageContainer { get; set; }

        /// <summary>
        /// Gets or sets the retry count.
        /// </summary>
        /// <value>
        /// The retry count.
        /// </value>
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the delay.
        /// </summary>
        /// <value>
        /// The delay.
        /// </value>
        public TimeSpan Delay { get; set; }
    }
}