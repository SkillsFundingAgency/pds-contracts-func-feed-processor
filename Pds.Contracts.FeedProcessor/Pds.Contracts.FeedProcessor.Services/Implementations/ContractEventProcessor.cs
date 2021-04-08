using Pds.Contracts.FeedProcessor.Services.Interfaces;
using Pds.Contracts.FeedProcessor.Services.Models;
using Pds.Core.Logging;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Implementations
{
    /// <summary>
    /// Implements the <see cref="IContractEventProcessor"/> to allow events to be processed.
    /// </summary>
    /// <seealso cref="Pds.Contracts.FeedProcessor.Services.Interfaces.IContractEventProcessor" />
    public class ContractEventProcessor : IContractEventProcessor
    {
        private readonly IDeserilizationService<ContractProcessResult> _deserilizationService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILoggerAdapter<ContractEventProcessor> _loggerAdapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractEventProcessor"/> class.
        /// </summary>
        /// <param name="deserilizationService">A service to allow deserialisation of XML to <see cref="ContractEvent"/> objects.</param>
        /// <param name="blobStorageService">A service to allow blobs to be uploaded.</param>
        /// <param name="loggerAdapter">A service to allow logging.</param>
        public ContractEventProcessor(
            IDeserilizationService<ContractProcessResult> deserilizationService,
            IBlobStorageService blobStorageService,
            ILoggerAdapter<ContractEventProcessor> loggerAdapter)
        {
            _deserilizationService = deserilizationService;
            _blobStorageService = blobStorageService;
            _loggerAdapter = loggerAdapter;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Raised if <paramref name="feedEntry"/> is null.</exception>
        /// <exception cref="ArgumentException">Raised if the content property of <param name="feedEntry"/> is null or empty.</exception>
        /// <exception cref="Azure.RequestFailedException">Raised if the XML contents cannot be saved to azure storage.</exception>
        public async Task<ContractProcessResult> ProcessEventsAsync(FeedEntry feedEntry)
        {
            if (feedEntry is null)
            {
                throw new ArgumentNullException(nameof(feedEntry));
            }

            if (string.IsNullOrWhiteSpace(feedEntry.Content))
            {
                throw new ArgumentException(nameof(feedEntry));
            }

            _loggerAdapter.LogInformation($"[{nameof(ProcessEventsAsync)}] - Parsing xml string to object.");

            // XML is compatiable with the schema, so, it should deserialise.
            var result = await _deserilizationService.DeserializeAsync(feedEntry.Content);

            _loggerAdapter.LogInformation($"[{nameof(ProcessEventsAsync)}] - XML parsing completed - Result : {result.Result}.");

            if (result.Result == ContractProcessResultType.Successful)
            {
                try
                {
                    foreach (var item in result.ContactEvents)
                    {
                        item.BookmarkId = feedEntry.Id;

                        _loggerAdapter.LogInformation($"[{nameof(ProcessEventsAsync)}] - Saving XML to azure strorage.");

                        // Save xml to blob
                        // Filename format : [Entry.Updated]_[ContractNumber]_v[ContractVersion]_[Entry.BookmarkId].xml
                        string filename = $"{feedEntry.Updated:yyyyMMddHHmmss}_{item.ContractNumber}_v{item.ContractVersion}_{item.BookmarkId}.xml";
                        await _blobStorageService.UploadAsync(filename, Encoding.UTF8.GetBytes(feedEntry.Content));

                        item.ContractEventXml = filename;

                        _loggerAdapter.LogInformation($"[{nameof(ProcessEventsAsync)}] - Saving XML completed.");
                    }
                }
                catch (Exception ex)
                {
                    _loggerAdapter.LogError(ex, "Failed to save XML file.");
                    throw;
                }
            }

            return result;
        }
    }
}
