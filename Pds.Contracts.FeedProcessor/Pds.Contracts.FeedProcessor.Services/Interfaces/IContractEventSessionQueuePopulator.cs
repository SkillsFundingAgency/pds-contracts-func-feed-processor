using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Pds.Contracts.FeedProcessor.Services.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pds.Contracts.FeedProcessor.Services.Interfaces
{
    public interface IContractEventSessionQueuePopulator
    {
        /// <summary>
        /// Creates the contract events in service bus session queue.
        /// </summary>
        /// <param name="contractEvents">The contract events.</param>
        /// <param name="messageCollector">The message collector.</param>
        void CreateContractEvents(IEnumerable<ContractEvent> contractEvents, ICollector<Message> messageCollector);
    }
}
