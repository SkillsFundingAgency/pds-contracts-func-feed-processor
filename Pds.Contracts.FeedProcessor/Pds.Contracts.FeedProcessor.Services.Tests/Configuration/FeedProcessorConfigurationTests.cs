using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Configuration.Tests
{
    [TestClass, TestCategory("Unit")]
    public class FeedProcessorConfigurationTests
    {
        private const string LastReadBookmarkId = "LastReadBookmarkId";
        private const string LastReadPage = "LastReadPage";
        private const string NumberOfPagesToProcess = "NumberOfPagesToProcess";

        private readonly IConfigReader _mockReader;

        public FeedProcessorConfigurationTests()
        {
            _mockReader = Mock.Of<IConfigReader>(MockBehavior.Strict);
        }

        [TestMethod]
        public async Task GetLastReadBookmarkIdTestAsync()
        {
            // Arrange
            var expected = Guid.NewGuid();
            Mock.Get(_mockReader).Setup(c => c.GetConfigAsync<Guid>(LastReadBookmarkId)).ReturnsAsync(expected);
            var config = new FeedProcessorConfiguration(_mockReader);

            // Act
            var result = await config.GetLastReadBookmarkId();

            // Assert
            result.Should().Be(expected);
            Mock.Get(_mockReader).VerifyAll();
        }

        [TestMethod]
        public async Task SetLastReadBookmarkIdTestAsync()
        {
            // Arrange
            var expected = Guid.NewGuid();
            Mock.Get(_mockReader).Setup(c => c.SetConfigAsync(LastReadBookmarkId, expected)).ReturnsAsync(expected);
            var config = new FeedProcessorConfiguration(_mockReader);

            // Act
            await config.SetLastReadBookmarkId(expected);

            // Assert
            Mock.Get(_mockReader).VerifyAll();
        }

        [TestMethod]
        public async Task GetLastReadPageTestAsync()
        {
            // Arrange
            var expected = 10;
            Mock.Get(_mockReader).Setup(c => c.GetConfigAsync<int>(LastReadPage)).ReturnsAsync(expected);
            var config = new FeedProcessorConfiguration(_mockReader);

            // Act
            var result = await config.GetLastReadPage();

            // Assert
            result.Should().Be(expected);
            Mock.Get(_mockReader).VerifyAll();
        }

        [TestMethod]
        public async Task SetLastReadPageTestAsync()
        {
            // Arrange
            var expected = 11;
            Mock.Get(_mockReader).Setup(c => c.SetConfigAsync(LastReadPage, expected)).ReturnsAsync(expected);
            var config = new FeedProcessorConfiguration(_mockReader);

            // Act
            await config.SetLastReadPage(expected);

            // Assert
            Mock.Get(_mockReader).VerifyAll();
        }

        [TestMethod]
        public async Task GetNumberOfPagesToProcessTestAsync()
        {
            // Arrange
            var expected = 10;
            Mock.Get(_mockReader).Setup(c => c.GetConfigAsync<int>(NumberOfPagesToProcess)).ReturnsAsync(expected);
            var config = new FeedProcessorConfiguration(_mockReader);

            // Act
            var result = await config.GetNumberOfPagesToProcess();

            // Assert
            result.Should().Be(expected);
            Mock.Get(_mockReader).VerifyAll();
        }
    }
}