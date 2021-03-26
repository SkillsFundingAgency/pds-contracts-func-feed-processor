using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
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
    /// Extracts, transforms and loads contract events from FCS atom feed.
    /// </summary>
    /// <seealso cref="Pds.Contracts.FeedProcessor.Services.Interfaces.IFeedProcessor" />
    public class FeedProcessor : IFeedProcessor
    {
        private readonly IFcsFeedReaderService _fcsFeedReader;
        private readonly IContractEventProcessor _eventProcessor;
        private readonly IFeedProcessorConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedProcessor" /> class.
        /// </summary>
        /// <param name="fcsFeedReader">The FCS feed reader.</param>
        /// <param name="eventProcessor">The event processor.</param>
        /// <param name="configuration">The configuration.</param>
        public FeedProcessor(IFcsFeedReaderService fcsFeedReader, IContractEventProcessor eventProcessor, IFeedProcessorConfiguration configuration)
        {
            _fcsFeedReader = fcsFeedReader;
            _eventProcessor = eventProcessor;
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task ExtractAndPopulateQueueAsync(ICollector<Message> queue)
        {
            var feedEntries = await _fcsFeedReader.ReadSelfPageAsync();
            var lastReadBookmarkEntry = await _configuration.GetLastReadBookmarkId();

            if (feedEntries.Any(e => e.Id == lastReadBookmarkEntry))
            {
                // extract all entries after that.
                var lastBookmark = feedEntries.Single(e => e.Id == lastReadBookmarkEntry);
                var newEntries = feedEntries.Skip(feedEntries.IndexOf(lastBookmark) + 1);

                await PopulateSessionQueue(queue, newEntries);
            }
            else
            {
                // Stroy 3.
            }
        }

        /// <inheritdoc/>
        public async Task ExtractAndPopulateQueueAsync(string payload, ICollector<Message> queueOutput)
        {
            var feedEntries = _fcsFeedReader.ExtractContractEventsFromFeedPageAsync(payload);
            await PopulateSessionQueue(queueOutput, feedEntries);
        }

        private async Task PopulateSessionQueue(ICollector<Message> queue, IEnumerable<FeedEntry> newEntries)
        {
            foreach (var item in newEntries)
            {
                var result = await _eventProcessor.ProcessEventsAsync(item);
                switch (result.Result)
                {
                    case ContractProcessResultType.Successful:
                        queue.Add(new Message
                        {
                            SessionId = result.ContactEvent.First().ContractNumber,
                            Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result.ContactEvent))
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
    }
}
