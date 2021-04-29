using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.FeedProcessor.Services.Configuration;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using Pds.Contracts.FeedProcessor.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Implementations.Tests
{
    [TestClass, TestCategory("Unit")]
    public class FeedProcessorTests
    {
        private readonly ILogger<FeedProcessor> _mockLogger;
        private readonly IFeedProcessorConfiguration _mockFeedProcessorConfiguration;
        private readonly IFcsFeedReaderService _mockFeedReader;
        private readonly IContractEventSessionQueuePopulator _mockQueuePopulator;
        private readonly IAsyncCollector<Message> _mockCollector;

        public FeedProcessorTests()
        {
            _mockLogger = Mock.Of<ILogger<FeedProcessor>>(MockBehavior.Strict);
            _mockFeedProcessorConfiguration = Mock.Of<Configuration.IFeedProcessorConfiguration>(MockBehavior.Strict);
            _mockFeedReader = Mock.Of<Interfaces.IFcsFeedReaderService>(MockBehavior.Strict);
            _mockQueuePopulator = Mock.Of<Interfaces.IContractEventSessionQueuePopulator>(MockBehavior.Strict);
            _mockCollector = Mock.Of<IAsyncCollector<Message>>(MockBehavior.Strict);
        }

        [TestMethod]
        public async Task ExtractAndPopulateQueueAsync_SelfPage_TestAsync()
        {
            // Arrange
            var dummyLastReadBookmarkId = Guid.NewGuid();
            var dummyFeedPage = new Models.FeedPage
            {
                Entries = new[]
                {
                    new FeedEntry { Id = Guid.NewGuid() },
                    new FeedEntry { Id = dummyLastReadBookmarkId },
                    new FeedEntry { Id = Guid.NewGuid() },
                    new FeedEntry { Id = Guid.NewGuid() },
                }
            };

            SetupMocks(dummyLastReadBookmarkId, dummyFeedPage);
            Mock.Get(_mockFeedProcessorConfiguration)
                .Setup(c => c.SetLastReadPage(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            var feedProcessor = new FeedProcessor(_mockFeedReader, _mockQueuePopulator, _mockFeedProcessorConfiguration, _mockLogger);
            await feedProcessor.ExtractAndPopulateQueueAsync(_mockCollector);

            // Assert
            Mock.Get(_mockFeedProcessorConfiguration).VerifyAll();
            Mock.Get(_mockFeedReader).VerifyAll();
            Mock.Get(_mockQueuePopulator)
                .Verify(q => q.PopulateSessionQueue(It.IsAny<IAsyncCollector<Message>>(), It.Is<IEnumerable<FeedEntry>>(m => m.Count() == 2)));
            Mock.Get(_mockLogger)
                .Verify(l => l.Log(LogLevel.Information, 0, It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Exactly(2));
        }

        [TestMethod]
        public void ExtractAndPopulateQueueAsync_ReadArchives_ThrowsException_TestAsync()
        {
            // Arrange
            var dummyLastReadBookmarkId = Guid.NewGuid();
            var dummyFeedPage = new Models.FeedPage
            {
                Entries = new[]
                {
                    new FeedEntry { Id = Guid.NewGuid() },
                    new FeedEntry { Id = Guid.NewGuid() },
                    new FeedEntry { Id = Guid.NewGuid() },
                }
            };

            SetupMocks(dummyLastReadBookmarkId, dummyFeedPage);

            Mock.Get(_mockFeedReader)
                .Setup(r => r.ReadPageAsync(It.IsAny<int>()))
                .ReturnsAsync(dummyFeedPage);

            // Act
            var feedProcessor = new FeedProcessor(_mockFeedReader, _mockQueuePopulator, _mockFeedProcessorConfiguration, _mockLogger);
            Func<Task> act = async () => await feedProcessor.ExtractAndPopulateQueueAsync(_mockCollector);

            // Assert
            act.Should().Throw<InvalidOperationException>();
            Mock.Get(_mockFeedProcessorConfiguration).VerifyAll();
            Mock.Get(_mockFeedReader).VerifyAll();
            Mock.Get(_mockQueuePopulator).VerifyNoOtherCalls();
            Mock.Get(_mockLogger)
                .Verify(l => l.Log(LogLevel.Information, 0, It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [TestMethod]
        public async Task ExtractAndPopulateQueueAsync_ReadArchives_TestAsync()
        {
            // Arrange
            var dummyLastReadBookmarkId = Guid.NewGuid();
            var dummySelfFeedPage = new Models.FeedPage
            {
                IsSelfPage = true,
                Entries = new[]
                {
                    new FeedEntry { Id = Guid.NewGuid() },
                    new FeedEntry { Id = Guid.NewGuid() },
                    new FeedEntry { Id = Guid.NewGuid() },
                },
                PreviousPageNumber = 10
            };

            var missedPreviousSelfFeedPage = new Models.FeedPage
            {
                IsSelfPage = true,
                Entries = new[]
                {
                    new FeedEntry { Id = Guid.NewGuid() },
                    new FeedEntry { Id = Guid.NewGuid() },
                    new FeedEntry { Id = Guid.NewGuid() },
                },
                PreviousPageNumber = 9
            };

            var dummyFeedPage = new Models.FeedPage
            {
                Entries = new[]
                {
                    new FeedEntry { Id = Guid.NewGuid() },
                    new FeedEntry { Id = dummyLastReadBookmarkId },
                    new FeedEntry { Id = Guid.NewGuid() },
                    new FeedEntry { Id = Guid.NewGuid() },
                },
                NextPageNumber = 12
            };

            var lastDummyFeedPage = new Models.FeedPage
            {
                Entries = new[]
                {
                    new FeedEntry { Id = Guid.NewGuid() },
                    new FeedEntry { Id = Guid.NewGuid() },
                    new FeedEntry { Id = Guid.NewGuid() },
                },
            };

            SetupMocks(dummyLastReadBookmarkId, dummySelfFeedPage);

            Mock.Get(_mockFeedReader)
                .Setup(r => r.ReadPageAsync(10))
                .ReturnsAsync(dummyFeedPage);
            Mock.Get(_mockFeedReader)
                .Setup(r => r.ReadPageAsync(12))
                .ReturnsAsync(missedPreviousSelfFeedPage);
            Mock.Get(_mockFeedReader)
                .Setup(r => r.ReadPageAsync(11))
                .ReturnsAsync(lastDummyFeedPage);

            Mock.Get(_mockFeedProcessorConfiguration)
                .Setup(c => c.SetLastReadPage(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            var feedProcessor = new FeedProcessor(_mockFeedReader, _mockQueuePopulator, _mockFeedProcessorConfiguration, _mockLogger);
            await feedProcessor.ExtractAndPopulateQueueAsync(_mockCollector);

            // Assert
            Mock.Get(_mockFeedProcessorConfiguration).VerifyAll();
            Mock.Get(_mockFeedReader).VerifyAll();
            Mock.Get(_mockQueuePopulator)
                .Verify(q => q.PopulateSessionQueue(It.IsAny<IAsyncCollector<Message>>(), It.IsAny<IEnumerable<FeedEntry>>()), Times.Exactly(3));
            Mock.Get(_mockLogger)
                .Verify(l => l.Log(LogLevel.Information, 0, It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Exactly(4));
        }

        private void SetupMocks(Guid dummyLastReadBookmarkId, FeedPage dummyFeedPage)
        {
            Mock.Get(_mockLogger)
                .Setup(l => l.Log(LogLevel.Information, 0, It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));

            Mock.Get(_mockFeedProcessorConfiguration)
                .Setup(c => c.GetLastReadBookmarkId())
                .ReturnsAsync(dummyLastReadBookmarkId);
            Mock.Get(_mockFeedProcessorConfiguration)
                .Setup(c => c.GetLastReadPage())
                .ReturnsAsync(10);
            Mock.Get(_mockFeedProcessorConfiguration)
                .Setup(c => c.GetNumberOfPagesToProcess())
                .ReturnsAsync(6);

            Mock.Get(_mockFeedReader)
                .Setup(r => r.ReadSelfPageAsync())
                .ReturnsAsync(dummyFeedPage);

            Mock.Get(_mockQueuePopulator)
                .Setup(q => q.PopulateSessionQueue(It.IsAny<IAsyncCollector<Message>>(), It.IsAny<IEnumerable<FeedEntry>>()))
                .Returns(Task.CompletedTask);
        }
    }
}