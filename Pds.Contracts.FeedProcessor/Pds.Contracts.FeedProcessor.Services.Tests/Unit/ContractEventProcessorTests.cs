using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.FeedProcessor.Services.Implementations;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using Pds.Contracts.FeedProcessor.Services.Models;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Tests.Unit
{
    [TestClass]
    public class ContractEventProcessorTests
    {
        private readonly IDeserilizationService<ContractProcessResult> _deserilizationService
            = Mock.Of<IDeserilizationService<ContractProcessResult>>(MockBehavior.Strict);

        private readonly IBlobStorageService _blobStorageService
            = Mock.Of<IBlobStorageService>(MockBehavior.Strict);

        private readonly ILoggerAdapter<ContractEventProcessor> _loggerAdapter
            = Mock.Of<ILoggerAdapter<ContractEventProcessor>>(MockBehavior.Strict);

        #region ProcessEventsAsync

        [TestMethod]
        public async Task ProcessEventsAsync_NoContracts_ReturnsExpectedResult()
        {
            // Arrange
            var feed = GetFeedEntry();
            var expected = GetZeroContractProcessResult();

            var actualBackingObject = GetZeroContractProcessResult();

            ILogger_Setup_LogInformation();

            Mock.Get(_deserilizationService)
                .Setup(p => p.DeserializeAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(actualBackingObject))
                .Verifiable();

            var processor = GetContractEventProcessor();

            // Act
            var result = await processor.ProcessEventsAsync(feed);

            // Assert
            result.Should().BeEquivalentTo(expected);
            Verify_All();
        }

        [TestMethod]
        public async Task ProcessEventsAsync_OneContract_ReturnsExpectedResult()
        {
            // Arrange
            var feed = GetFeedEntry();
            var expected = GetOneContractProcessResult();
            expected.ContactEvent.First().BookmarkId = feed.Id;
            expected.ContactEvent.First().ContractEventXml = GetFilename(feed, expected.ContactEvent.First());

            var actualBackingObject = GetOneContractProcessResult();

            ILogger_Setup_LogInformation();

            Mock.Get(_deserilizationService)
                .Setup(p => p.DeserializeAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(actualBackingObject))
                .Verifiable();

            Mock.Get(_blobStorageService)
                .Setup(p => p.Upload(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<bool>()))
                .Verifiable();

            var processor = GetContractEventProcessor();

            // Act
            var result = await processor.ProcessEventsAsync(feed);

            // Assert
            result.Should().BeEquivalentTo(expected);
            Verify_All();
        }

        [TestMethod]
        public void ProcessEventsAsync_NullArgument_RaisesException()
        {
            // Arrange
            FeedEntry feed = null;

            var processor = GetContractEventProcessor();

            // Act
            Func<Task> act = async () => await processor.ProcessEventsAsync(feed);

            // Assert
            act.Should().Throw<ArgumentNullException>("Because a null argument cannot be processed");
            Verify_All();
        }

        [TestMethod]
        public void ProcessEventsAsync_EmptyArgument_RaisesException()
        {
            // Arrange
            FeedEntry feed = new FeedEntry();

            var processor = GetContractEventProcessor();

            // Act
            Func<Task> act = async () => await processor.ProcessEventsAsync(feed);

            // Assert
            act.Should().Throw<ArgumentException>("Because an empty feed cannot be processed");
            Verify_All();
        }

        [TestMethod]
        public void ProcessEventsAsync_OnBlobStorageException_ReturnsExpectedResult()
        {
            // Arrange
            var feed = GetFeedEntry();
            var expected = GetOneContractProcessResult();
            expected.ContactEvent.First().BookmarkId = feed.Id;
            expected.ContactEvent.First().ContractEventXml = GetFilename(feed, expected.ContactEvent.First());

            var actualBackingObject = GetOneContractProcessResult();

            ILogger_Setup_LogInformation();
            ILogger_Setup_LogError();

            var raisedException = new Azure.RequestFailedException("Test azure error exception");

            Mock.Get(_deserilizationService)
                .Setup(p => p.DeserializeAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(actualBackingObject))
                .Verifiable();

            Mock.Get(_blobStorageService)
                .Setup(p => p.Upload(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<bool>()))
                .Throws(raisedException)
                .Verifiable();

            var processor = GetContractEventProcessor();

            // Act
            Func<Task> act = async () => await processor.ProcessEventsAsync(feed);

            // Assert
            act.Should().Throw<Azure.RequestFailedException>("Because any azure errors are critical stop errors.")
                .Which.Should().BeEquivalentTo(raisedException);

            Verify_All();
        }

        #endregion


        #region Arrange Helpers

        private ContractEventProcessor GetContractEventProcessor()
        => new ContractEventProcessor(_deserilizationService, _blobStorageService, _loggerAdapter);


        private FeedEntry GetFeedEntry()
        {
            return new FeedEntry()
            {
                Content = "Somecontent",
                Id = Guid.NewGuid(),
                Updated = DateTime.UtcNow
            };
        }

        private ContractProcessResult GetZeroContractProcessResult()
        {
            return new ContractProcessResult()
            {
                ContactEvent = new List<ContractEvent>(),
                Result = ContractProcessResultType.Successful
            };
        }

        private ContractProcessResult GetOneContractProcessResult()
        {
            return new ContractProcessResult()
            {
                ContactEvent = new List<ContractEvent>()
                {
                    new ContractEvent()
                    {
                        BookmarkId = Guid.Empty,
                        ParentContractNumber = "ParentTest123",
                        ContractNumber = "Test456",
                        ContractVersion = 789
                    }
                },
                Result = ContractProcessResultType.Successful
            };
        }

        private void ILogger_Setup_LogInformation()
        {
            Mock.Get(_loggerAdapter)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();
        }

        private void ILogger_Setup_LogError()
        {
            Mock.Get(_loggerAdapter)
                .Setup(p => p.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();
        }

        private string GetFilename(FeedEntry feedEntry, ContractEvent item)
        {
            // Filename format : [Entry.Updated]_[ContractNumber]_v[ContractVersion]_[Entry.BookmarkId].xml
            string filename = $"{feedEntry.Updated:yyyyMMddHHmmss}_{item.ContractNumber}_v{item.ContractVersion}_{item.BookmarkId}.xml";
            return filename;
        }

        #endregion

        #region Verify

        private void Verify_All()
        {
            Mock.Get(_blobStorageService)
                .VerifyAll();

            Mock.Get(_deserilizationService)
                .VerifyAll();

            Mock.Get(_loggerAdapter)
                .VerifyAll();
        }
        #endregion
    }
}
