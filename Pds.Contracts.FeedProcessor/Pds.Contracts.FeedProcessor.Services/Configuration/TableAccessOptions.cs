using System;

namespace Pds.Contracts.FeedProcessor.Services.Configuration
{
    /// <summary>
    /// Options to access table storage.
    /// </summary>
    public class TableAccessOptions
    {
        /// <summary>
        /// Gets or sets the partition key.
        /// </summary>
        /// <value>
        /// The partition key.
        /// </value>
        public string PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the configuration table.
        /// </summary>
        /// <value>
        /// The name of the configuration table.
        /// </value>
        public string ConfigTableName { get; set; }

        /// <summary>
        /// Gets or sets the delta back off.
        /// </summary>
        /// <value>
        /// The delta back off.
        /// </value>
        public TimeSpan DeltaBackOff { get; set; }

        /// <summary>
        /// Gets or sets the maximum attempts.
        /// </summary>
        /// <value>
        /// The maximum attempts.
        /// </value>
        public int MaxAttempts { get; set; }
    }
}