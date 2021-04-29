namespace Pds.Contracts.FeedProcessor.Services.Configuration
{
    /// <summary>
    /// Configuration settings for Azure storage accounts.
    /// </summary>
    public class AzureStorageAccountOptions
    {
        /// <summary>
        /// Gets or sets connection string to access azure storage services.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets settings specific to blob storage.
        /// </summary>
        public BlobAccessOptions BlobAccessOptions { get; set; }

        /// <summary>
        /// Gets or sets settings specific to azure storage tables.
        /// </summary>
        public TableAccessOptions TableAccessOptions { get; set; }
    }
}
