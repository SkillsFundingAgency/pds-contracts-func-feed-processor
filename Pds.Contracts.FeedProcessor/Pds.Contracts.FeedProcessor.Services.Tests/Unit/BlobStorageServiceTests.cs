using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.FeedProcessor.Services.Implementations;
using Pds.Core.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class BlobStorageServiceTests
    {
        private readonly BlobClient _blobClient = Mock.Of<BlobClient>(MockBehavior.Strict);
        private readonly BlobContainerClient _blobContainerClient = Mock.Of<BlobContainerClient>(MockBehavior.Strict);
        private readonly ILoggerAdapter<BlobStorageService> _logger = Mock.Of<ILoggerAdapter<BlobStorageService>>(MockBehavior.Strict);

        private readonly string _testAccountName = "testAccountName";
        private readonly string _testContainerName = "testContainerName";
        private readonly string _filename = "some-test-file.xml";

        #region Upload

        [TestMethod]
        public async Task UploadAsync_UploadsRequestFile_As_AzureBlob()
        {
            // Arrange
            var data = new byte[10];
            bool overwrite = false;

            var response = Mock.Of<Azure.Response<BlobContentInfo>>();

            Mock.Get(_blobClient)
                .Setup(p => p.UploadAsync(It.IsAny<Stream>(), overwrite, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response)
                .Verifiable();

            SetupBlobContainerClient(_filename);
            ILoggerAdapter_Setup_LogInformation();

            var sut = GetBlobStorageService();

            // Act
            await sut.UploadAsync(_filename, data, overwrite);

            // Assert
            Verify_All();
        }

        [TestMethod]
        public void UploadAsync_UploadsRequestFile_As_AzureBlob_AllowsExceptionToBeRaised()
        {
            // Arrange
            var data = new byte[10];
            bool overwrite = false;
            var exception = new Azure.RequestFailedException("Test message.");

            Mock.Get(_blobClient)
                .Setup(p => p.UploadAsync(It.IsAny<Stream>(), overwrite, It.IsAny<CancellationToken>()))
                .Throws(exception)
                .Verifiable();

            SetupBlobContainerClient(_filename);
            ILoggerAdapter_Setup_LogInformation();

            var sut = GetBlobStorageService();

            // Act
            Func<Task> act = async () => await sut.UploadAsync(_filename, data, overwrite);

            // Assert
            act.Should().Throw<Azure.RequestFailedException>();

            Verify_All();
        }

        #endregion

        #region Arrange Helpers

        private BlobStorageService GetBlobStorageService()
            => new BlobStorageService(_blobContainerClient, _logger);

        private void SetupBlobContainerClient(string filename)
        {
            Mock.Get(_blobContainerClient)
                .Setup(p => p.GetBlobClient(filename))
                .Returns(_blobClient)
                .Verifiable();

            Mock.Get(_blobContainerClient)
                .Setup(p => p.AccountName)
                .Returns(_testAccountName)
                .Verifiable();

            Mock.Get(_blobContainerClient)
                .Setup(p => p.Name)
                .Returns(_testContainerName)
                .Verifiable();
        }

        private void ILoggerAdapter_Setup_LogInformation()
        {
            Mock.Get(_logger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();
        }

        #endregion

        #region Verify

        private void Verify_All()
        {
            Mock.Get(_logger)
                .Verify();

            Mock.Get(_blobContainerClient)
                .Verify();

            Mock.Get(_blobClient)
                .Verify();
        }

        #endregion
    }
}
