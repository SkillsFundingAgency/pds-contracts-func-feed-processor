using FluentAssertions;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.FeedProcessor.Services.DependencyInjection;
using Pds.Contracts.FeedProcessor.Services.Tests.Helper;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Configuration.Tests
{
    [TestClass, TestCategory("Unit")]
    public class AzureTableStorageConfigReaderTests
    {
        private readonly AzureStorageAccountOptions _mockConfig;
        private readonly ConfigurationHelper _configurationHelper = new ConfigurationHelper();

        public AzureTableStorageConfigReaderTests()
        {
            _mockConfig = new AzureStorageAccountOptions()
            {
                BlobAccessOptions = new BlobAccessOptions()
                {
                    Delay = TimeSpan.FromSeconds(15),
                    RetryCount = 3,
                    XmlStorageContainer = "xmlContainer"
                },
                ConnectionString = "constr",
                TableAccessOptions = new TableAccessOptions()
                {
                    ConfigTableName = "tblName",
                    DeltaBackOff = TimeSpan.FromSeconds(15),
                    MaxAttempts = 3,
                    PartitionKey = "paritionkey"
                }
            };
        }

        #region GetConfigAsync

        [TestMethod, TestCategory("Unit")]
        public async Task GetConfigAsync_ReturnsExpectedResult_Test()
        {
            // Arrange
            string key = "getConfigTestKey";
            string expected = "SomeExpectedValue";

            var result = new TableResult()
            {
                Result = new FeedProcessorConfigItem<string>() { Data = expected }
            };

            TableOperation op = TableOperation.Retrieve(_mockConfig.TableAccessOptions.PartitionKey, key);

            CloudTable table = GetMockCloudTable();
            Mock.Get(table)
                .Setup(p => p.ExecuteAsync(It.IsAny<TableOperation>()))
                .ReturnsAsync(result);

            AzureTableStorageConfigReader reader = GetAzureTableStorageConfigReader(table);

            // Act
            var actual = await reader.GetConfigAsync<string>(key);

            // Assert
            actual.Should().Be(expected, "Because with a specified key expected data can be retrieved.");
        }

        [TestMethod, TestCategory("Unit")]
        public void GetConfigAsync_WhenKeyNotFound_ThrowsException_Test()
        {
            // Arrange
            string key = "getConfigTestKey";
            TableResult result = null;

            CloudTable table = GetMockCloudTable();
            Mock.Get(table)
                .Setup(p => p.ExecuteAsync(It.IsAny<TableOperation>()))
                .ReturnsAsync(result);

            AzureTableStorageConfigReader reader = GetAzureTableStorageConfigReader(table);

            // Act
            Func<Task<string>> act = async () => await reader.GetConfigAsync<string>(key);

            // Assert
            act.Should().Throw<KeyNotFoundException>("Because when the key is missing, an exception should be raised.");
        }

        [TestMethod, TestCategory("Integration"), Ignore("Needs further investigation fails in Dev but passes locally")]
        public async Task GetConfig_Integration_ReturnsExpectedResult_Test()
        {
            // Arrange
            var key = "GetConfigIntegrationTest";
            var expectedData = "IntegrationTestDataContent";
            var cloudTable = GetActualCloudTable();
            var options = Options.Create<AzureStorageAccountOptions>(GetActualAzureStorageOptions());

            var setting = new FeedProcessorConfigItem<string>
            {
                PartitionKey = options.Value.TableAccessOptions.PartitionKey,
                RowKey = key,
                Data = expectedData
            };

            var setOperation = TableOperation.InsertOrReplace(setting);
            await cloudTable.ExecuteAsync(setOperation);

            // Act
            var reader = new AzureTableStorageConfigReader(cloudTable, options);
            var actual = await reader.GetConfigAsync<string>(key);

            // Assert
            actual.Should().Be(expectedData);

            // Cleanup
            var removeOperation = TableOperation.Delete(setting);
            await cloudTable.ExecuteAsync(removeOperation);
        }

        #endregion

        #region SetCOnfigAsync

        [TestMethod, TestCategory("Unit")]
        public async Task SetConfigAsync_ReturnsExpectedResult_Test()
        {
            // Arrange
            string key = "setConfigTestKey";
            string expected = "TestExpectedValue";

            var result = new TableResult()
            {
                Result = new FeedProcessorConfigItem<string>() { Data = expected }
            };

            CloudTable table = GetMockCloudTable();
            Mock.Get(table)
                .Setup(p => p.ExecuteAsync(It.IsAny<TableOperation>()))
                .Callback((TableOperation op) =>
                {
                    if (op.OperationType != TableOperationType.InsertOrReplace)
                    {
                        // Ensure the correct operation is carried out
                        Assert.Fail();
                    }
                })
                .ReturnsAsync(result);

            AzureTableStorageConfigReader reader = GetAzureTableStorageConfigReader(table);

            // Act
            var actual = await reader.SetConfigAsync<string>(key, expected);

            // Assert
            actual.Should().Be(expected);
            Mock.Get(table).VerifyAll();
        }

        [TestMethod, TestCategory("Unit")]
        public void SetConfigAsync_WhenFailedToSave_RaisesException()
        {
            // Arrange
            string key = "setConfigTestKey";
            string value = "TestExpectedValue";

            TableResult result = null;

            CloudTable table = GetMockCloudTable();
            Mock.Get(table)
                .Setup(p => p.ExecuteAsync(It.IsAny<TableOperation>()))
                .ReturnsAsync(result);

            AzureTableStorageConfigReader reader = GetAzureTableStorageConfigReader(table);

            // Act
            Func<Task<string>> act = async () => await reader.SetConfigAsync<string>(key, value);

            // Assert
            act.Should().Throw<InvalidOperationException>();
            Mock.Get(table).VerifyAll();
        }

        [TestMethod, TestCategory("Integration"), Ignore("Needs further investigation fails in Dev but passes locally")]
        public async Task SetConfigAsync_Integration_ReturnsExpectedResult_Test()
        {
            // Arrange
            var key = "SetConfigIntegrationKey";
            var expected = "TestExpectedValue";
            var table = GetActualCloudTable();
            var options = Options.Create<AzureStorageAccountOptions>(GetActualAzureStorageOptions());

            var reader = new AzureTableStorageConfigReader(table, options);

            // Act
            var actual = await reader.SetConfigAsync<string>(key, expected);

            // Assert
            actual.Should().Be(expected);

            // cleanup
            var setting = new FeedProcessorConfigItem<string>
            {
                PartitionKey = options.Value.TableAccessOptions.PartitionKey,
                RowKey = key,
                Data = expected,
                ETag = "*"
            };
            var removeOperation = TableOperation.Delete(setting);
            await table.ExecuteAsync(removeOperation);
        }

        #endregion

        #region Arrange Helpers

        private AzureTableStorageConfigReader GetAzureTableStorageConfigReader(CloudTable table)
        {
            var storage = Options.Create<AzureStorageAccountOptions>(_mockConfig);
            return new AzureTableStorageConfigReader(table, storage);
        }

        private CloudTable GetMockCloudTable()
        {
            Uri emptyAddress = new Uri("http://www.testuri.com");
            TableClientConfiguration tableClientConfiguration = new TableClientConfiguration();
            return new Mock<CloudTable>(MockBehavior.Strict, emptyAddress, tableClientConfiguration).Object;
        }


        private CloudTable GetActualCloudTable()
        {
            var services = new ServiceCollection();
            services.AddLoggerAdapter();
            services.AddFeatureServices(_configurationHelper.Configuration);

            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<CloudTable>();
        }

        private AzureStorageAccountOptions GetActualAzureStorageOptions()
        {
            return _configurationHelper.GetConfiguration<AzureStorageAccountOptions>(nameof(AzureStorageAccountOptions));
        }
        #endregion
    }
}