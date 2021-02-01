using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace Pds.Contracts.FeedProcessor.Func
{
    /// <summary>
    /// Example timer triggered Azure Function.
    /// </summary>
    public class FcsAtomFeedProcessorFunction
    {
        private readonly IFcsFeedReaderService _feedReaderService;
        private readonly IContractEventSessionQueuePopulator _queuePopulator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FcsAtomFeedProcessorFunction" /> class.
        /// </summary>
        /// <param name="feedReaderService">The example service.</param>
        /// <param name="queuePopulator">The queue populator.</param>
        public FcsAtomFeedProcessorFunction(IFcsFeedReaderService feedReaderService, IContractEventSessionQueuePopulator queuePopulator)
        {
            _feedReaderService = feedReaderService;
            _queuePopulator = queuePopulator;
        }

        /// <summary>
        /// Entry point to the Azure Function.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="collector">The collector.</param>
        /// <param name="log">The logger.</param>
        [FunctionName("FCSAtomFeedProcessorFunction")]
        public void Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "feed")] HttpRequest req,
            [ServiceBus("%ContractEventsSessionQueue%", Connection = "ServiceBusConnection")] ICollector<Message> collector,
            ILogger log)
        {
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            if (collector is null)
            {
                throw new ArgumentNullException(nameof(collector));
            }

            using var reader = new StreamReader(req.Body);
            var payload = reader.ReadToEnd();
            log?.LogInformation($"HttpRequest trigger: FCSAtomFeedProcessorFunction  function with payload: {payload}.");

            var contractEvents = _feedReaderService.GetContractEvents(payload);
            _queuePopulator.CreateContractEvents(contractEvents, collector);

            log?.LogInformation($"HttpRequest trigger: FCSAtomFeedProcessorFunction: function completed creating {contractEvents.Count()} messages.");
        }
    }
}