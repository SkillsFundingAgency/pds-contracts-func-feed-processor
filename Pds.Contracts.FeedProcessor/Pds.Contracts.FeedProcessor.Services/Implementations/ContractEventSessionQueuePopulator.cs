using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pds.Contracts.FeedProcessor.Services.Configuration;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using Pds.Contracts.FeedProcessor.Services.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Implementations
{
    /// <summary>
    /// Contract event service bus session queue populator.
    /// </summary>
    /// <seealso cref="Pds.Contracts.FeedProcessor.Services.Interfaces.IContractEventSessionQueuePopulator" />
    public class ContractEventSessionQueuePopulator : IContractEventSessionQueuePopulator
    {
        private readonly IContractEventProcessor _eventProcessor;
        private readonly IFeedProcessorConfiguration _configuration;
        private readonly ILogger<ContractEventSessionQueuePopulator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractEventSessionQueuePopulator" /> class.
        /// </summary>
        /// <param name="eventProcessor">The event processor.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public ContractEventSessionQueuePopulator(IContractEventProcessor eventProcessor, IFeedProcessorConfiguration configuration, ILogger<ContractEventSessionQueuePopulator> logger)
        {
            _eventProcessor = eventProcessor;
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task PopulateSessionQueue(IAsyncCollector<Message> queue, IEnumerable<FeedEntry> newEntries)
        {
            if (newEntries.Any())
            {
                var resultTypes = new List<ContractProcessResultType>();
                foreach (var item in newEntries)
                {
                    var result = await _eventProcessor.ProcessEventsAsync(item);
                    foreach (var contractEvent in result.ContactEvents)
                    {
                        resultTypes.Add(result.Result);
                        switch (result.Result)
                        {
                            case ContractProcessResultType.Successful:
                                await queue.AddAsync(new Message
                                {
                                    SessionId = contractEvent.ContractNumber,
                                    Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(contractEvent))
                                });
                                break;

                            case ContractProcessResultType.StatusValidationFailed:
                            case ContractProcessResultType.FundingTypeValidationFailed:
                            default:
                                break;
                        }

                        await _configuration.SetLastReadBookmarkId(item.Id);
                    }
                }

                var successCount = resultTypes.Count(r => r == ContractProcessResultType.Successful);
                var ignoredCount = resultTypes.Count(r => r != ContractProcessResultType.Successful);
                _logger.LogInformation($"{nameof(PopulateSessionQueue)} - Completed processing and created [{successCount}] contract events messages in queue and ignored [{ignoredCount}] contract events.");
            }
        }
    }
}