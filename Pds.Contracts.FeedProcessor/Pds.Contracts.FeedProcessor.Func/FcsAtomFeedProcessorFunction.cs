using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Func
{
    /// <summary>
    /// Example timer triggered Azure Function.
    /// </summary>
    public class FcsAtomFeedProcessorFunction
    {
        private readonly IFeedProcessor _feedProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FcsAtomFeedProcessorFunction" /> class.
        /// </summary>
        /// <param name="feedProcessor">The feed processor.</param>
        public FcsAtomFeedProcessorFunction(IFeedProcessor feedProcessor)
        {
            _feedProcessor = feedProcessor;
        }

        /// <summary>
        /// Entry point to the Azure Function.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="queueOutput">The queue output.</param>
        /// <param name="log">The logger.</param>
        /// <returns>
        /// A <see cref="Task" /> representing the asynchronous operation.
        /// </returns>
        [FunctionName("FCSAtomFeedProcessorFunctionHttpFunction")]
        public async Task RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "feed")] HttpRequest req,
            [ServiceBus("%ContractEventsSessionQueue%", Connection = "ServiceBusConnection")] IAsyncCollector<Message> queueOutput,
            ILogger log)
        {
            using var reader = new StreamReader(req.Body);
            var payload = reader.ReadToEnd();
            log?.LogInformation($"HttpRequest trigger: FCSAtomFeedProcessorFunctionHttpFunction function.");

            await _feedProcessor.ExtractAndPopulateQueueAsync(payload, queueOutput);

            log?.LogInformation($"HttpRequest trigger: FCSAtomFeedProcessorFunction: function completed creating messages.");
        }
    }
}