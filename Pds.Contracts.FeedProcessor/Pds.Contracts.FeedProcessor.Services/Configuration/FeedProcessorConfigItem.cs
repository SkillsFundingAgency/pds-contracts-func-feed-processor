using Microsoft.Azure.Cosmos.Table;

namespace Pds.Contracts.FeedProcessor.Services.Configuration
{
    /// <summary>
    /// An extension of the table entity object that allows configuration data to be stored.
    /// </summary>
    /// <typeparam name="T">Type of value to be stored and retreived.</typeparam>
    /// <seealso cref="Microsoft.Azure.Cosmos.Table.TableEntity" />
    public class FeedProcessorConfigItem<T> : TableEntity
    {
        /// <summary>
        /// Gets or sets the configuration data element.
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public T Data { get; set; }
    }
}
