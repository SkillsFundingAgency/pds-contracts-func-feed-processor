using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using Pds.Contracts.FeedProcessor.Services.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pds.Contracts.FeedProcessor.Func.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class FcsAtomFeedProcessorFunctionTests
    {
        [TestMethod]
        public void Run_ThrowsArgumentNullException_WhenCollectorIsNull()
        {
            // Arrange
            var mockFeedReaderService = Mock.Of<IFcsFeedReaderService>();
            Mock.Get(mockFeedReaderService)
                .Setup(e => e.GetContractEvents(It.IsAny<string>()))
                .Returns(Enumerable.Empty<ContractEvent>())
                .Verifiable();

            var mockQueuePopulator = Mock.Of<IContractEventSessionQueuePopulator>();
            Mock.Get(mockQueuePopulator)
                .Setup(p => p.CreateContractEvents(It.IsAny<IEnumerable<ContractEvent>>(), It.IsAny<ICollector<Message>>()))
                .Verifiable();

            var mockReq = Mock.Of<HttpRequest>(MockBehavior.Loose);

            // Act
            var function = new FcsAtomFeedProcessorFunction(mockFeedReaderService, mockQueuePopulator);
            Action act = () => { function.Run(mockReq, null, null); };

            // Assert
            act.Should().Throw<ArgumentNullException>();
            Mock.Get(mockFeedReaderService).VerifyNoOtherCalls();
            Mock.Get(mockQueuePopulator).VerifyNoOtherCalls();
        }

        [TestMethod]
        public void Run_ThrowsArgumentNullException_WhenHttpRequestIsNull()
        {
            // Arrange
            var mockFeedReaderService = Mock.Of<IFcsFeedReaderService>();
            Mock.Get(mockFeedReaderService)
                .Setup(e => e.GetContractEvents(It.IsAny<string>()))
                .Returns(Enumerable.Empty<ContractEvent>())
                .Verifiable();

            var mockQueuePopulator = Mock.Of<IContractEventSessionQueuePopulator>();
            Mock.Get(mockQueuePopulator)
                .Setup(p => p.CreateContractEvents(It.IsAny<IEnumerable<ContractEvent>>(), It.IsAny<ICollector<Message>>()))
                .Verifiable();

            var mockCollector = Mock.Of<ICollector<Message>>(MockBehavior.Strict);

            // Act
            var function = new FcsAtomFeedProcessorFunction(mockFeedReaderService, mockQueuePopulator);
            Action act = () => { function.Run(null, mockCollector, null); };

            // Assert
            act.Should().Throw<ArgumentNullException>();
            Mock.Get(mockFeedReaderService).VerifyNoOtherCalls();
            Mock.Get(mockQueuePopulator).VerifyNoOtherCalls();
        }

        [TestMethod]
        public void Run_DoesNotThrowsException()
        {
            // Arrange
            var mockFeedReaderService = Mock.Of<IFcsFeedReaderService>(MockBehavior.Strict);
            Mock.Get(mockFeedReaderService)
                .Setup(e => e.GetContractEvents(It.IsAny<string>()))
                .Returns(Enumerable.Empty<ContractEvent>())
                .Verifiable();

            var mockQueuePopulator = Mock.Of<IContractEventSessionQueuePopulator>(MockBehavior.Strict);
            Mock.Get(mockQueuePopulator)
                .Setup(p => p.CreateContractEvents(It.IsAny<IEnumerable<ContractEvent>>(), It.IsAny<ICollector<Message>>()))
                .Verifiable();

            var mockReq = CreateMockRequest("test");
            var mockCollector = Mock.Of<ICollector<Message>>(MockBehavior.Strict);

            // Act
            var function = new FcsAtomFeedProcessorFunction(mockFeedReaderService, mockQueuePopulator);
            Action act = () => { function.Run(mockReq, mockCollector, null); };

            // Assert
            act.Should().NotThrow();
            Mock.Get(mockFeedReaderService).Verify();
            Mock.Get(mockQueuePopulator).Verify();
            Mock.Get(mockReq).Verify();
        }

        private HttpRequest CreateMockRequest(string body)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            sw.Write(body);
            sw.Flush();

            ms.Position = 0;

            var mockReq = Mock.Of<HttpRequest>(MockBehavior.Strict);
            Mock.Get(mockReq)
                .Setup(x => x.Body)
                .Returns(ms)
                .Verifiable();

            return mockReq;
        }
    }
}