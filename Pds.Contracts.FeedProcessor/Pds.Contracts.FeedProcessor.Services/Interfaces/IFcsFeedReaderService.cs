using Pds.Contracts.FeedProcessor.Services.Models;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
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
        Task<FeedPage> ReadSelfPageAsync();

        /// <summary>
        /// Reads the specified page.
        /// </summary>
        /// <param name="pageNumber">The page number.</param>
        /// <returns>A collection of feed entries from the page.</returns>
        Task<FeedPage> ReadPageAsync(int pageNumber);

        /// <summary>
        /// Extracts the contract events from feed page asynchronous.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns> A <see cref="Task"/> representing the asynchronous operation.</returns>
        FeedPage ExtractContractEventsFromFeedPageAsync(string payload);
    }
}