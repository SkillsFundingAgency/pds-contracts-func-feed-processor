using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Func
{
    /// <summary>
    /// Timer triggered FCS contract events Atom feed reader.
    /// </summary>
    public class FcsContractEventFeedReaderTimerFunction
    {
        private readonly IFeedProcessor _feedProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="FcsContractEventFeedReaderTimerFunction" /> class.
        /// </summary>
        /// <param name="feedProcessor">The feed processor.</param>
        public FcsContractEventFeedReaderTimerFunction(IFeedProcessor feedProcessor)
        {
            _feedProcessor = feedProcessor;
        }

        /// <summary>
        /// Reads FCS atom feed by timer.
        /// </summary>
        /// <param name="timer">Triggered timer.</param>
        /// <param name="queueOutput">The contract events service bus session queue collector.</param>
        /// <param name="log">The log.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName(nameof(FcsContractEventFeedReaderTimerFunction))]
        public async Task RunAsync(
            [TimerTrigger("%TimerInterval%")] TimerInfo timer,
            [ServiceBus("%ContractEventsSessionQueue%", Connection = "ServiceBusConnection")] IAsyncCollector<Message> queueOutput,
            ILogger log)
        {
            log?.LogInformation($"C# Timer trigger function {nameof(FcsContractEventFeedReaderTimerFunction)} executed at: {DateTime.Now}");

            await _feedProcessor.ExtractAndPopulateQueueAsync(queueOutput);

            log?.LogInformation($"C# Timer trigger function {nameof(FcsContractEventFeedReaderTimerFunction)} Completed at: {DateTime.Now}");
        }
    }
}