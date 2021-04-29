using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Interfaces
{
    /// <summary>
    /// Interface that provides feed processing functionalities.
    /// </summary>
    public interface IFeedProcessor
    {
        /// <summary>
        /// Extracts the FCS atom feed and populates the queue with contract events.
        /// </summary>
        /// <param name="queue">The contract events session queue.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ExtractAndPopulateQueueAsync(IAsyncCollector<Message> queue);

        /// <summary>
        /// Extracts the and populate queue asynchronous.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <param name="queueOutput">The queue output.</param>
        /// <returns></returns>
        Task ExtractAndPopulateQueueAsync(string payload, IAsyncCollector<Message> queueOutput);
    }
}