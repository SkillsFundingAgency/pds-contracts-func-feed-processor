using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Pds.Contracts.FeedProcessor.Services.Configuration;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using System;
using System.Linq;
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
        private readonly IContractEventSessionQueuePopulator _queuePopulator;
        private readonly IFeedProcessorConfiguration _configuration;
        private readonly ILogger<FeedProcessor> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedProcessor" /> class.
        /// </summary>
        /// <param name="fcsFeedReader">The FCS feed reader.</param>
        /// <param name="queuePopulator">The queue populator.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public FeedProcessor(
            IFcsFeedReaderService fcsFeedReader,
            IContractEventSessionQueuePopulator queuePopulator,
            IFeedProcessorConfiguration configuration,
            ILogger<FeedProcessor> logger)
        {
            _fcsFeedReader = fcsFeedReader;
            _queuePopulator = queuePopulator;
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task ExtractAndPopulateQueueAsync(IAsyncCollector<Message> queue)
        {
            var lastReadBookmarkEntry = await _configuration.GetLastReadBookmarkId();
            var lastReadPage = await _configuration.GetLastReadPage();
            var numberOfPagesToProcess = await _configuration.GetNumberOfPagesToProcess();

            _logger.LogInformation($"{nameof(ExtractAndPopulateQueueAsync)} - Starting to process contract events, Last read bookmark is [{lastReadBookmarkEntry}] and last read page is [{lastReadPage}] will be processing upto a maximum of [{numberOfPagesToProcess}] pages in this run.");

            var selfPage = await _fcsFeedReader.ReadSelfPageAsync();
            if (selfPage.Entries.Any(e => e.Id == lastReadBookmarkEntry))
            {
                // extract all entries after that.
                var lastBookmark = selfPage.Entries.Single(e => e.Id == lastReadBookmarkEntry);
                var newEntries = selfPage.Entries.Skip(selfPage.Entries.IndexOf(lastBookmark) + 1);

                _logger.LogInformation($"{nameof(ExtractAndPopulateQueueAsync)} - On [Self page] found [{newEntries.Count()}] new contract events to process.");

                if (newEntries.Any())
                {
                    await _queuePopulator.PopulateSessionQueue(queue, newEntries);
                    await _configuration.SetLastReadPage(selfPage.CurrentPageNumber);
                }
            }
            else
            {
                // Story 3.
                await ReadArchives(queue, lastReadBookmarkEntry, lastReadPage, numberOfPagesToProcess);
            }
        }

        /// <inheritdoc/>
        public async Task ExtractAndPopulateQueueAsync(string payload, IAsyncCollector<Message> queueOutput)
        {
            var feedEntries = _fcsFeedReader.ExtractContractEventsFromFeedPageAsync(payload);
            await _queuePopulator.PopulateSessionQueue(queueOutput, feedEntries.Entries);
        }

        private async Task ReadArchives(IAsyncCollector<Message> queue, Guid lastReadBookmarkEntry, int lastReadPage, int numberOfPagesToProcess)
        {
            // Go to last read page.
            var thisPage = await _fcsFeedReader.ReadPageAsync(lastReadPage);
            if (lastReadBookmarkEntry != Guid.Empty && !thisPage.Entries.Any(e => e.Id == lastReadBookmarkEntry))
            {
                throw new InvalidOperationException($"{nameof(ExtractAndPopulateQueueAsync)} - Last read bookmark [{lastReadBookmarkEntry}] cannot be found on last read page [{lastReadPage}] abort processing contract events.");
            }

            // extract all entries after last read bookmark.
            var lastBookmark = thisPage.Entries.SingleOrDefault(e => e.Id == lastReadBookmarkEntry);
            var newEntries = lastBookmark is null ? thisPage.Entries : thisPage.Entries.Skip(thisPage.Entries.IndexOf(lastBookmark) + 1);

            _logger.LogInformation($"{nameof(ExtractAndPopulateQueueAsync)} - On [{lastReadPage}] found [{newEntries.Count()}] new contract events to process.");

            if (newEntries.Any())
            {
                numberOfPagesToProcess--;
                await _queuePopulator.PopulateSessionQueue(queue, newEntries);
                await _configuration.SetLastReadPage(thisPage.CurrentPageNumber);
            }

            while (numberOfPagesToProcess > 0 && !thisPage.IsSelfPage)
            {
                numberOfPagesToProcess--;
                thisPage = thisPage.NextPageNumber > 0 ? await _fcsFeedReader.ReadPageAsync(thisPage.NextPageNumber) : await _fcsFeedReader.ReadSelfPageAsync();

                // double check to ensure when we are on self page, we have completed previous pages and we are not missing any big load of events since we read.
                int prevPage = 0;
                if (thisPage.IsSelfPage && thisPage.PreviousPageNumber != (prevPage = await _configuration.GetLastReadPage()))
                {
                    thisPage = await _fcsFeedReader.ReadPageAsync(prevPage + 1);
                }

                _logger.LogInformation($"{nameof(ExtractAndPopulateQueueAsync)} - On [{(thisPage.IsSelfPage ? "Self" : thisPage.CurrentPageNumber.ToString())}] found [{thisPage.Entries.Count()}] new contract events to process.");

                if (thisPage.Entries.Any())
                {
                    await _queuePopulator.PopulateSessionQueue(queue, thisPage.Entries);
                    await _configuration.SetLastReadPage(thisPage.CurrentPageNumber);
                }
            }
        }
    }
}