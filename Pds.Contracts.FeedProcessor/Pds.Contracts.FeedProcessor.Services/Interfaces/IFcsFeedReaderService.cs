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
        /// Gets the contract events.
        /// </summary>
        /// <param name="exampleAtomFeed">The example atom feed.</param>
        /// <returns>A readonly collection of contract events.</returns>
        IEnumerable<ContractEvent> GetContractEvents(string exampleAtomFeed);
    }
}