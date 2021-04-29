using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
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

        private const string _validationServiceFundingTypes = "ValidationServiceFundingTypes";
        private const string _validationServiceStatuses = "ValidationServiceStatuses";

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


        #region Validation Service Statuses

        [TestMethod]
        public async Task GetValidationServiceStatuses_WhenKeyAndDataExist_ReturnsExpectedValue()
        {
            // Arrange
            var expected = new ValidationServiceConfigurationStatusesCollection()
            {
                new ValidationServiceConfigurationStatuses() {AmendmentType = "a", ContractStatus = "b", ParentContractStatus = "c" }
            };

            var configString = JsonConvert.SerializeObject(expected);

            Mock.Get(_mockReader)
                .Setup(m => m.GetConfigAsync<string>(_validationServiceStatuses))
                .ReturnsAsync(configString);

            var config = new FeedProcessorConfiguration(_mockReader);

            // Act
            var result = await config.GetValidationServiceStatuses();

            // Assert
            result.Should().BeEquivalentTo(expected, "Because settings are saved as string but deserilised before being returned.");
            Mock.Get(_mockReader).VerifyAll();
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("String_NotDeserialisable")]
        public void GetValidationServiceStatuses_WhenKeyOrDataDoNotExist_RaisesException(string input)
        {
            // Arrange
            string rtn = null;

            Mock.Get(_mockReader)
                .Setup(m => m.GetConfigAsync<string>(_validationServiceStatuses))
                .ReturnsAsync(rtn);

            var config = new FeedProcessorConfiguration(_mockReader);

            // Act
            Func<Task<ValidationServiceConfigurationStatusesCollection>> act = async () => await config.GetValidationServiceStatuses();

            // Assert
            act.Should().Throw<JsonSerializationException>("Because the table storage element does not exist and/or the content is not valid.");
            Mock.Get(_mockReader).VerifyAll();
        }

        [TestMethod]
        public async Task SetValidationServiceStatuses_ObjectIsPersisted()
        {
            // Arrange
            var data = new ValidationServiceConfigurationStatusesCollection()
            {
                new ValidationServiceConfigurationStatuses() {AmendmentType = "a", ContractStatus = "b", ParentContractStatus = "c" }
            };

            var expected = JsonConvert.SerializeObject(data);

            Mock.Get(_mockReader)
                .Setup(m => m.SetConfigAsync<string>(_validationServiceStatuses, expected))
                .ReturnsAsync(expected);

            var config = new FeedProcessorConfiguration(_mockReader);

            // Act
            await config.SetValidationServiceStatuses(data);

            // Assert
            Mock.Get(_mockReader).VerifyAll();
        }

        #endregion


        #region Validation Funding Types

        [TestMethod]
        public async Task GetValidationServiceFundingTypes_WhenKeyAndDataExist_ReturnsExpectedValue()
        {
            // Arrange
            var expected = new ValidationServiceConfigurationFundingTypes()
            {
                "First",
                "Second"
            };

            var configString = JsonConvert.SerializeObject(expected);

            Mock.Get(_mockReader)
                .Setup(m => m.GetConfigAsync<string>(_validationServiceFundingTypes))
                .ReturnsAsync(configString);

            var config = new FeedProcessorConfiguration(_mockReader);

            // Act
            var result = await config.GetValidationServiceFundingTypes();

            // Assert
            result.Should().BeEquivalentTo(expected, "Because settings are saved as json and deserialised before being returned.");
            Mock.Get(_mockReader).VerifyAll();
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("String_NotDeserialisable")]
        public void GetValidationServiceFundingTypes_WhenKeyOrDataDoNotExist_RaisesException(string input)
        {
            // Arrange
            string rtn = null;

            Mock.Get(_mockReader)
                .Setup(m => m.GetConfigAsync<string>(_validationServiceFundingTypes))
                .ReturnsAsync(rtn);

            var config = new FeedProcessorConfiguration(_mockReader);

            // Act
            Func<Task<ValidationServiceConfigurationFundingTypes>> act = async () => await config.GetValidationServiceFundingTypes();

            // Assert
            act.Should().Throw<JsonSerializationException>("Because the settings are missing or malformed.");
            Mock.Get(_mockReader).VerifyAll();
        }

        [TestMethod]
        public async Task SetValidationServiceFundingTypes_ObjectIsPersisted()
        {
            // Arrange
            var data = new ValidationServiceConfigurationFundingTypes()
            {
                "First",
                "Second"
            };

            var expected = JsonConvert.SerializeObject(data);

            Mock.Get(_mockReader)
                .Setup(m => m.SetConfigAsync<string>(_validationServiceFundingTypes, expected))
                .ReturnsAsync(expected);

            var config = new FeedProcessorConfiguration(_mockReader);

            // Act
            await config.SetValidationServiceFundingTypes(data);

            // Assert
            Mock.Get(_mockReader).VerifyAll();
        }

        #endregion
    }
}