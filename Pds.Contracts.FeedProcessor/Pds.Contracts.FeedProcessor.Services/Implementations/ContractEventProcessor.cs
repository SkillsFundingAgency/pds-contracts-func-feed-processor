using Pds.Contracts.FeedProcessor.Services.Interfaces;
using Pds.Contracts.FeedProcessor.Services.Models;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<IList<ContractProcessResult>> ProcessEventsAsync(FeedEntry feedEntry)
        {
            if (feedEntry is null)
            {
                throw new ArgumentNullException(nameof(feedEntry));
            }

            if (string.IsNullOrWhiteSpace(feedEntry.Content))
            {
                throw new ArgumentException(nameof(feedEntry));
            }

            _loggerAdapter.LogInformation($"[{nameof(ProcessEventsAsync)}] - [Bookmark:{feedEntry.Id}] - Parsing xml string to object.");

            // XML is compatiable with the schema, so, it should deserialise.
            var contractEvents = await _deserilizationService.DeserializeAsync(feedEntry.Content);
            var resultsGroup = contractEvents
                .GroupBy(c => c.Result)
                .Select(g => $"{g.Key}:{g.Count()}");

            _loggerAdapter.LogInformation($"[{nameof(ProcessEventsAsync)}] - [Bookmark:{feedEntry.Id}] - XML parsing completed with results [{string.Join(",", resultsGroup)}].");

            try
            {
                foreach (var item in contractEvents)
                {
                    item.ContractEvent.BookmarkId = feedEntry.Id;

                    if (item.Result == ContractProcessResultType.Successful)
                    {
                        _loggerAdapter.LogInformation($"[{nameof(ProcessEventsAsync)}] - [Bookmark:{feedEntry.Id}] - Saving XML to azure strorage.");

                        // Save xml to blob
                        // Filename format : [Entry.Updated]_[ContractNumber]_v[ContractVersion]_[Entry.BookmarkId].xml
                        string filename = $"{feedEntry.Updated:yyyyMMddHHmmss}_{item.ContractEvent.ContractNumber}_v{item.ContractEvent.ContractVersion}_{item.ContractEvent.BookmarkId}.xml";
                        await _blobStorageService.UploadAsync(filename, Encoding.UTF8.GetBytes(feedEntry.Content));

                        item.ContractEvent.ContractEventXml = filename;

                        _loggerAdapter.LogInformation($"[{nameof(ProcessEventsAsync)}] - [Bookmark:{feedEntry.Id}] - Saving XML completed.");
                    }
                    else
                    {
                        _loggerAdapter.LogWarning($"[{nameof(ProcessEventsAsync)}] - [Bookmark:{feedEntry.Id}] - Ignoring filesave - Result is unsuccessful [{item.Result}].");
                    }
                }
            }
            catch (Exception ex)
            {
                _loggerAdapter.LogError(ex, $"Failed to save XML file for Bookmark [{feedEntry.Id}].");
                throw;
            }

            return contractEvents;
        }
    }
}