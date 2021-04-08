using Pds.Contracts.FeedProcessor.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Interfaces
{
    /// <summary>
    /// An interface to provide functions for processing of Feed Events.
    /// </summary>
    public interface IContractEventProcessor
    {
        /// <summary>
        /// Creates the contract events in service bus session queue.
        /// </summary>
        /// <param name="feedEntry">The entry to process.</param>
        /// <returns>The processed and verified feed contents as a <see cref="ContractProcessResult"/>.</returns>
        Task<IList<ContractProcessResult>> ProcessEventsAsync(FeedEntry feedEntry);
    }
}
