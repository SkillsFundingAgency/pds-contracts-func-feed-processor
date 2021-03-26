using Pds.Contracts.FeedProcessor.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Interfaces
{
    /// <summary>
    /// Example service.
    /// </summary>
    public interface IFcsFeedReaderService
    {
        /// <summary>
        /// Reads the self page.
        /// </summary>
        /// <returns>A collection of feed entries.</returns>
        Task<IList<FeedEntry>> ReadSelfPageAsync();

        /// <summary>
        /// Extracts the contract events from feed page asynchronous.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns> A <see cref="Task"/> representing the asynchronous operation.</returns>
        IList<FeedEntry> ExtractContractEventsFromFeedPageAsync(string payload);
    }
}