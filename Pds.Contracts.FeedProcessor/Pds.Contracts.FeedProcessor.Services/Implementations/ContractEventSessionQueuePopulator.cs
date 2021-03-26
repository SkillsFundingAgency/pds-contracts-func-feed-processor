using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using Pds.Contracts.FeedProcessor.Services.Models;
using System.Collections.Generic;
using System.Text;

namespace Pds.Contracts.FeedProcessor.Services.Implementations
{
    /// <summary>
    /// Contract event service bus session queue populator.
    /// </summary>
    /// <seealso cref="Pds.Contracts.FeedProcessor.Services.Interfaces.IContractEventSessionQueuePopulator" />
    public class ContractEventSessionQueuePopulator : IContractEventSessionQueuePopulator
    {
        /// <inheritdoc/>
        public void CreateContractEvents(IEnumerable<ContractEvent> contractEvents, ICollector<Message> messageCollector)
        {
            foreach (var contractEvent in contractEvents)
            {
                messageCollector.Add(new Message
                {
                    SessionId = contractEvent.ContractNumber,
                    Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(contractEvent))
                });
            }
        }
    }
}
