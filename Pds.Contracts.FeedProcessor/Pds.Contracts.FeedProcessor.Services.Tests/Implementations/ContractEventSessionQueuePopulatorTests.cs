using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.FeedProcessor.Services.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Implementations.Tests
{
    [TestClass, TestCategory("Unit")]
    public class ContractEventSessionQueuePopulatorTests
    {
        [TestMethod]
        public async Task PopulateSessionQueueTestAsync()
        {
            // Arrange
            var dummyEntries = new[]
                {
                    new FeedEntry
                    {
                        Id = new Guid("d2619398-19dc-44e8-b4a9-917796baf6c2"),
                        Updated = DateTime.Parse("2021-01-01T01:01:01Z"),
                        Content = @"<Content type=""application/vnd.test.v1+atom+xml"">Test content string</Content>"
                    },
                    new FeedEntry
                    {
                        Id = new Guid("b1ca5999-f34f-405c-84fd-a6e7d94bd1ac"),
                        Updated = DateTime.Parse("2021-02-02T02:02:02Z"),
                        Content = @"<Content type=""application/vnd.test.v1+atom+xml"">Test content string 2</Content>"
                    },
                    new FeedEntry
                    {
                        Id = new Guid("60b04b11-117b-4c6a-9e0a-e2112b88fa64"),
                        Updated = DateTime.Parse("2021-02-02T02:02:02Z"),
                        Content = @"<Content type=""application/vnd.test.v1+atom+xml"">Test content string 2</Content>"
                    }
                };

            var expectedResults = new[]
            {
                new ContractProcessResult
                {
                    Result = ContractProcessResultType.Successful,
                    ContactEvents = new[]
                    {
                        new ContractEvent
                        {
                            BookmarkId = new Guid("d2619398-19dc-44e8-b4a9-917796baf6c2"),
                            ContractNumber = "Contract number 1",
                            ContractVersion = 1
                        }
                    }
                },
                new ContractProcessResult
                {
                    Result = ContractProcessResultType.Successful,
                    ContactEvents = new[]
                    {
                        new ContractEvent
                        {
                            BookmarkId = new Guid("d2619398-19dc-44e8-b4a9-917796baf6c2"),
                            ContractNumber = "Contract number 2.1",
                            ContractVersion = 1
                        },
                        new ContractEvent
                        {
                            BookmarkId = new Guid("d2619398-19dc-44e8-b4a9-917796baf6c2"),
                            ContractNumber = "Contract number 2.2",
                            ContractVersion = 1
                        }
                    }
                },
                new ContractProcessResult
                {
                    Result = ContractProcessResultType.StatusValidationFailed,
                    ContactEvents = new[]
                    {
                        new ContractEvent
                        {
                            BookmarkId = new Guid("60b04b11-117b-4c6a-9e0a-e2112b88fa64"),
                            ContractNumber = "Contract number 3",
                            ContractVersion = 1
                        }
                    }
                }
            };

            var mockEventProcessor = Mock.Of<Interfaces.IContractEventProcessor>(MockBehavior.Strict);
            Mock.Get(mockEventProcessor)
                .SetupSequence(e => e.ProcessEventsAsync(It.Is<FeedEntry>(f => dummyEntries.Any(d => d.Id == f.Id))))
                .ReturnsAsync(expectedResults[0])
                .ReturnsAsync(expectedResults[1])
                .ReturnsAsync(expectedResults[2])
                ;

            var mockConfiguration = Mock.Of<Configuration.IFeedProcessorConfiguration>(MockBehavior.Strict);
            Mock.Get(mockConfiguration)
                .Setup(c => c.SetLastReadBookmarkId(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);

            var mockLogger = Mock.Of<ILogger<ContractEventSessionQueuePopulator>>(MockBehavior.Strict);
            Mock.Get(mockLogger)
                .Setup(l => l.Log(LogLevel.Information, 0, It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            var mockCollector = Mock.Of<IAsyncCollector<Message>>();
            Mock.Get(mockCollector)
                .Setup(c => c.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var queuePopulator = new ContractEventSessionQueuePopulator(mockEventProcessor, mockConfiguration, mockLogger);
            await queuePopulator.PopulateSessionQueue(mockCollector, dummyEntries);

            // Assert
            Mock.Get(mockEventProcessor).VerifyAll();
            Mock.Get(mockLogger).VerifyAll();
            Mock.Get(mockConfiguration)
                .Verify(c => c.SetLastReadBookmarkId(It.IsAny<Guid>()), Times.Exactly(4));
            Mock.Get(mockCollector)
                .Verify(c => c.AddAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        }
    }
}