using Azure.Storage.Blobs;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using Pds.Core.Logging;
using System.IO;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Implementations
{
    /// <summary>
    /// Provides services to allow transfer of data to Azure blobs.
    /// </summary>
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly ILoggerAdapter<BlobStorageService> _loggerAdapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageService"/> class.
        /// </summary>
        /// <param name="blobContainerClient">Client to use to access blobs.</param>
        /// <param name="loggerAdapter">The logger to use for output logging.</param>
        public BlobStorageService(
            BlobContainerClient blobContainerClient,
            ILoggerAdapter<BlobStorageService> loggerAdapter)
        {
            _blobContainerClient = blobContainerClient;
            _loggerAdapter = loggerAdapter;
        }

        /// <inheritdoc />
        public async Task UploadAsync(string filename, byte[] contents, bool overwrite = true)
        {
            _loggerAdapter.LogInformation($"Uploading file [{filename}] to storage container [{_blobContainerClient.AccountName}/{_blobContainerClient.Name}].");
            var blob = _blobContainerClient.GetBlobClient(filename);

            using var ms = new MemoryStream(contents);

            await blob.UploadAsync(ms, overwrite);

            _loggerAdapter.LogInformation($"Upload of [{filename}] sucessful.");
        }
    }
}
