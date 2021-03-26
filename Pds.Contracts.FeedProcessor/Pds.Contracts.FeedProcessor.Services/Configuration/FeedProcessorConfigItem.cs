using Microsoft.WindowsAzure.Storage.Table;

namespace Pds.Contracts.FeedProcessor.Services.Configuration
{
    /// <summary>
    /// An extension of the table entity object that allows configuration data to be stored.
    /// </summary>
    /// <typeparam name="T">The type for the configuration data.</typeparam>
    public class FeedProcessorConfigItem<T> : TableEntity
    {
        /// <summary>
        /// Gets or sets the configuration data element.
        /// </summary>
        public T Data { get; set; }
    }
}
