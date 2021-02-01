using Newtonsoft.Json;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using Pds.Contracts.FeedProcessor.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Implementations
{
    /// <summary>
    /// Example service.
    /// </summary>
    public class FcsFeedReaderService : IFcsFeedReaderService
    {
        /// <inheritdoc/>
        public IEnumerable<ContractEvent> GetContractEvents(string exampleAtomFeed)
        { 
            for (int i = 0; i < 5; i++)
            {
                yield return new ContractEvent
                {
                    ExampleSequenceId = i,
                    ExampleFeedTime = DateTime.Now,
                    ExampleMessage = exampleAtomFeed
                };
            }
        }
    }
}