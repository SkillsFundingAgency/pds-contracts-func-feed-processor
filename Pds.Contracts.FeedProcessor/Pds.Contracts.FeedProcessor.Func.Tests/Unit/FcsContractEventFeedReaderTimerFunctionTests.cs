using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.FeedProcessor.Func;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Func.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class FcsContractEventFeedReaderTimerFunctionTests
    {
        [TestMethod]
        public void RunAsyncTest()
        {
            // Arrange
            var mockFeedProcessor = Mock.Of<Services.Interfaces.IFeedProcessor>(MockBehavior.Strict);
            Mock.Get(mockFeedProcessor)
                .Setup(fp => fp.ExtractAndPopulateQueueAsync(It.IsAny<IAsyncCollector<Message>>()))
                .Returns(Task.CompletedTask);

            // Act
            var fcsContractEventFeedReaderTimerFunction = new FcsContractEventFeedReaderTimerFunction(mockFeedProcessor);
            Func<Task> function = () => fcsContractEventFeedReaderTimerFunction.RunAsync(null, null, null);

            // Assert
            function.Should().NotThrow();
            Mock.Get(mockFeedProcessor).VerifyAll();
        }
    }
}