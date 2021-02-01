using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.FeedProcessor.Services.Implementations;
using Pds.Contracts.FeedProcessor.Services.Models;
using System;
using System.Collections.Generic;

namespace Pds.Contracts.FeedProcessor.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractEventSessionQueuePopulatorTests
    {
        [TestMethod]
        public void CreateContractEvents_ReturnsExpectedResult()
        {
            // Arrange
            var dummyContractEvents = new List<ContractEvent>
            {
                new ContractEvent { ExampleFeedTime = DateTime.Now, ExampleMessage = "test", ExampleSequenceId = 0},
                new ContractEvent { ExampleFeedTime = DateTime.Now, ExampleMessage = "test", ExampleSequenceId = 1},
                new ContractEvent { ExampleFeedTime = DateTime.Now, ExampleMessage = "test", ExampleSequenceId = 2},
                new ContractEvent { ExampleFeedTime = DateTime.Now, ExampleMessage = "test", ExampleSequenceId = 3},
                new ContractEvent { ExampleFeedTime = DateTime.Now, ExampleMessage = "test", ExampleSequenceId = 4},
            };

            var mockCollector = Mock.Of<ICollector<Message>>(MockBehavior.Strict);
            Mock.Get(mockCollector)
                .Setup(c => c.Add(It.IsAny<Message>()))
                .Verifiable();

            // Act
            var populator = new ContractEventSessionQueuePopulator();
            populator.CreateContractEvents(dummyContractEvents, mockCollector);

            // Assert
            Mock.Get(mockCollector)
                .Verify(c => c.Add(It.IsAny<Message>()), Times.Exactly(5));
        }
    }
}