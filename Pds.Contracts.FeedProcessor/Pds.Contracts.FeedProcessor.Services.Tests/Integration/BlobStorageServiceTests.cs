using Azure.Storage.Blobs;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.FeedProcessor.Services.Configuration;
using Pds.Contracts.FeedProcessor.Services.DependencyInjection;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using Pds.Contracts.FeedProcessor.Services.Tests.Helper;
using Pds.Core.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Tests.Integration
{
    [TestClass, TestCategory("Integration")]
    public class BlobStorageServiceTests
    {
        private const string _dataContents = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

        private readonly string _filename = "Pds.Contracts.FeedProcessor.Services.Tests.Integration.BlobStorageServiceTests.IntegrationTestFile.txt";

        private readonly AzureStorageAccountOptions _storageSettings;
        private readonly ConfigurationHelper _configurationHelper;

        public BlobStorageServiceTests()
        {
            _configurationHelper = new ConfigurationHelper();
            _storageSettings = _configurationHelper.GetConfiguration<AzureStorageAccountOptions>(nameof(AzureStorageAccountOptions));
        }

        #region Upload

        [TestMethod]
        public async Task UploadAsync_IntegrationTest()
        {
            // Arrange
            var data = Encoding.UTF8.GetBytes(_dataContents);
            var client = new BlobContainerClient(_storageSettings.ConnectionString, _storageSettings.BlobAccessOptions.XmlStorageContainer);

            // Remove file if it's left over from a previous failed test.
            var previous = client.GetBlobClient(_filename);
            previous.DeleteIfExists();

            var sut = GetBlobStorageService();

            // Act
            await sut.UploadAsync(_filename, data, true);

            // Assert
            var actual = client.GetBlobClient(_filename);
            using var streamReader = new StreamReader(actual.OpenRead());
            string actualData = streamReader.ReadToEnd();

            actualData.Should().Be(_dataContents);
            actual.DeleteIfExists();
        }

        #endregion


        #region Arrange Helpers

        private IBlobStorageService GetBlobStorageService()
        {
            var services = new ServiceCollection();
            services.AddLoggerAdapter();
            services.AddFeatureServices(_configurationHelper.Configuration);

            var provider = services.BuildServiceProvider();

            return provider.GetRequiredService<IBlobStorageService>();
        }

        #endregion
    }
}
