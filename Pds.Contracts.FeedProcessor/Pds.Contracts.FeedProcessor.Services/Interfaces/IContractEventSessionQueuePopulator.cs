using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Pds.Contracts.FeedProcessor.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Interfaces
{
    /// <summary>
    /// Interface to support population of contract events to session queue.
    /// </summary>
    public interface IContractEventSessionQueuePopulator
    {
        /// <summary>
        /// Populates the session queue.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <param name="newEntries">The new entries.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task PopulateSessionQueue(IAsyncCollector<Message> queue, IEnumerable<FeedEntry> newEntries);
    }
}
