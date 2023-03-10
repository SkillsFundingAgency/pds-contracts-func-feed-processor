using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.FeedProcessor.Services.Configuration;
using Pds.Contracts.FeedProcessor.Services.Implementations;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using Pds.Contracts.FeedProcessor.Services.Tests.Helper;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace Pds.Contracts.FeedProcessor.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractEventValidationServiceTests
    {
        private const string _validXmlDocument = "EDSK-9999-v1-Valid.xml";
        private const string _invalidSchemaXmlDocument = "EDSK-9999-v1-Invalid-SchemaNumber.xml";
        private const string _validXmlDocumentWithoutSchemaVersion = "EDSK-9999-v1-Valid-NoSchemaVersion.xml";
        private const string _partialXmlDocument = "ESIF-9999-v5-Partial.xml";

        private readonly SchemaValidationSettings _validationSettings = new SchemaValidationSettings();
        private readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory + "/Documents/11_06/";

        private readonly ILoggerAdapter<ContractEventValidationService> _logger
            = Mock.Of<ILoggerAdapter<ContractEventValidationService>>(MockBehavior.Strict);

        private readonly IFeedProcessorConfiguration _configuration
            = Mock.Of<IFeedProcessorConfiguration>(MockBehavior.Strict);

        public ContractEventValidationServiceTests()
        {
            _validationSettings.SchemaVersion = "11_06";
            _validationSettings.SchemaManifestFilename = "contract_corporate_schema_v11.06.xsd";
            _validationSettings.EnableSchemaVersionValidation = true;
        }

        #region Constructor

        [TestMethod, TestCategory("Unit")]
        public void Constructor_EnsureSchemaFile_IsLoaded()
        {
            // Arrange
            var options = Options.Create<SchemaValidationSettings>(_validationSettings);

            ILoggerAdapter_Setup_LogInformation();

            // Act
            Action act = () => new ContractEventValidationService(_configuration, options, _logger);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod, TestCategory("Unit")]
        public void Constructor_OnSchemaFileLoadError_MessageIsLogged_But_ProcessDoesNotAbort()
        {
            // Arrange
            _validationSettings.SchemaManifestFilename = "none-existant-file.xsd";
            var options = Options.Create<SchemaValidationSettings>(_validationSettings);

            ILoggerAdapter_Setup_LogInformation();
            ILoggerAdapter_Setup_LogErrorException();

            // Act
            Action act = () => new ContractEventValidationService(_configuration, options, _logger);

            // Assert
            act.Should().NotThrow();
        }

        [TestMethod, TestCategory("Unit")]
        public void Constructor_OnSchemaVersionMismatch_MessageIsLogged_But_ProcessDoesNotAbort()
        {
            // Arrange
            _validationSettings.SchemaVersion = "11_00";
            var options = Options.Create<SchemaValidationSettings>(_validationSettings);

            ILoggerAdapter_Setup_LogWarning();

            // Act
            Action act = () => new ContractEventValidationService(_configuration, options, _logger);

            // Assert
            act.Should().NotThrow();
        }
        #endregion

        #region ValidateXmlWithSchema

        [TestMethod, TestCategory("Unit")]
        public void ValidateXMLWithSchema_MissingSchemaFile_BypassesSchemaValidation()
        {
            // Arrange
            var filename = Path.Combine(_baseDirectory, _partialXmlDocument);
            var fileContents = File.ReadAllText(filename);

            XmlDocument expected = new XmlDocument();
            expected.LoadXml(fileContents);

            _validationSettings.SchemaVersion = "11_06";
            _validationSettings.SchemaManifestFilename = "none-existant-file.xsd";

            ILoggerAdapter_Setup_LogInformation();
            ILoggerAdapter_Setup_LogWarning();
            ILoggerAdapter_Setup_LogErrorException();

            var service = GetValidationService();

            // Act
            var actual = service.ValidateXmlWithSchema(fileContents);

            // Assert
            actual.Should().BeEquivalentTo(expected, "Because when the schema file is missing, XML validation failure is logged but process continues.");
            Verify_All();
        }

        [TestMethod, Ignore("Awaiting XML File.")]
        public void ValidateXmlWithSchema_WhenSchemaVersionValidationIsDiabled_FileWithoutSchemaVersionPasses()
        {
            // Arrange
            _validationSettings.EnableSchemaVersionValidation = false;
            var filename = Path.Combine(_baseDirectory, _validXmlDocumentWithoutSchemaVersion);
            var fileContents = File.ReadAllText(filename);

            ILoggerAdapter_Setup_LogInformation();
            ILoggerAdapter_Setup_LogWarningException();

            var service = GetValidationService();

            // Act
            var result = service.ValidateXmlWithSchema(fileContents);

            // Assert

            //result.Should().BeTrue("Because the schema version validation is turned off.");
            Assert.Fail("Need to compare xml documents");
            Verify_All();
        }

        [TestMethod, Ignore("Awaiting XML File.")]
        public void ValidateXMLWithSchema_ValidXML_ConformsToCurrentSchema()
        {
            // Arrange
            var filename = Path.Combine(_baseDirectory, _validXmlDocument);
            var fileContents = File.ReadAllText(filename);

            ILoggerAdapter_Setup_LogInformation();

            var service = GetValidationService();

            // Act
            Action act = () => service.ValidateXmlWithSchema(fileContents);

            // Assert
            act.Should().NotThrow("Because the xml is valid with the current schema.");
            Verify_All();
        }

        [TestMethod, Ignore("Awaiting XML File")]
        public void ValidateXMLWithSchema_InvalidXML_RaisesException()
        {
            // Arrange
            var expectedExceptionMessage = "The value of the 'schemaVersion' attribute does not equal its fixed value.";
            var filename = Path.Combine(_baseDirectory, _invalidSchemaXmlDocument);
            var fileContents = File.ReadAllText(filename);

            ILoggerAdapter_Setup_LogInformation();
            ILoggerAdapter_Setup_LogErrorException();


            var service = GetValidationService();

            // Act
            Action act = () => service.ValidateXmlWithSchema(fileContents);

            // Assert
            act.Should().Throw<XmlSchemaValidationException>("Because the XML is not valid against the current schema version")
                .And.Message.Should().Be(expectedExceptionMessage, "Because the xml schema version attribute is different than the expected value.");
            Verify_All();
        }

        #endregion


        #region ValidateFundingType

        [DataRow("1619FUND")]
        [DataRow("24+LOANS")]
        [DataRow("AEB")]
        [DataRow("AGE")]
        [DataRow("EDSK")]
        [DataRow("EOP")]
        [DataRow("ESF")]
        [DataRow("LEVY")]
        [DataRow("LOANS")]
        [DataRow("MAIN")]
        [DataRow("NCS")]
        [DataRow("NLA")]
        [DataRow("NLG")]
        [DataRow("16-18FU")]
        [DataRow("DADA")]
        [DataRow("CCF")]
        [DataRow("FECA")]
        [DataRow("19TRN2020")]
        [DataRow("AEB2021")]
        [DataRow("HTE-PGF")]
        [DataRow("SADF")]
        [DataRow("FE-PDGP")]
        [DataRow("SDFII")]
        [DataRow("SB")]
        [DataRow("MULT")]
        [DataRow("FE-ACA")]
        [DataRow("HTE-SIF")]
        [DataRow("FE-RCA")]
        [TestMethod, TestCategory("Unit")]
        public async Task ValidateFundingTypeAsync_CorrectFundingType_ReturnsTrue(string fundingType)
        {
            // Arrange
            ILoggerAdapter_Setup_LogInformation();
            IFeedProcessorConfiguration_Setup_GetValidationServiceFundingTypes();

            var service = GetValidationService();

            // Act
            var result = await service.ValidateFundingTypeAsync(fundingType);

            // Assert
            result.Should().BeTrue("Because the value is in the acceptable funding types.");
            Verify_All();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateFundingTypeAsync_InvalidFundingType_ReturnsFalse()
        {
            // Arrange
            string fundingType = "sometestvalue";

            ILoggerAdapter_Setup_LogInformation();
            ILoggerAdapter_Setup_LogWarning();
            IFeedProcessorConfiguration_Setup_GetValidationServiceFundingTypes();

            var service = GetValidationService();

            // Act
            var result = await service.ValidateFundingTypeAsync(fundingType);

            // Assert
            result.Should().BeFalse("Because the supplied funding type is not within the acceptable list.");
            Verify_All();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateFundingTypeAsync_MissingConfiguration_UsesDefault()
        {
            // Arrange
            string fundingType = "1619FUND";
            ValidationServiceConfigurationFundingTypes actual = null;
            var expected = GetDefaultFundingTypes();

            ILoggerAdapter_Setup_LogInformation();
            ILoggerAdapter_Setup_LogWarningException();

            Mock.Get(_configuration)
                .SetupSequence(p => p.GetValidationServiceFundingTypes())
                .Throws<KeyNotFoundException>()
                .Returns(Task.FromResult(expected));

            Mock.Get(_configuration)
                .Setup(p => p.SetValidationServiceFundingTypes(It.IsAny<ValidationServiceConfigurationFundingTypes>()))
                .Callback((ValidationServiceConfigurationFundingTypes config) => { actual = config; })
                .Returns(Task.CompletedTask)
                .Verifiable();

            var service = GetValidationService();

            // Act
            var result = await service.ValidateFundingTypeAsync(fundingType);

            // Assert
            result.Should().BeTrue("Because the supplied values are in the default configuration.");

            actual.Should().BeEquivalentTo(expected, "Because the default configuration and test configuration match.");
            Verify_All();
        }

        #endregion


        #region ValidateStatusType

        [DataRow("Draft", "Published to Provider", "None")]
        [DataRow("Draft", "Published to Provider", "Variation")]
        [DataRow("Approved", "Approved", "None")]
        [DataRow("Approved", "Approved", "Variation")]
        [DataRow("Approved", "Approved", "Notification")]
        [DataRow("Approved", "Modified", "None")]
        [DataRow("Approved", "Modified", "Variation")]
        [DataRow("Approved", "Modified", "Notification")]
        [DataRow("Approved", "Under Termination", "None")]
        [DataRow("Approved", "Under Termination", "Variation")]
        [DataRow("Approved", "Under Termination", "Notification")]
        [DataRow("Withdrawn", "Withdrawn By Agency", "None")]
        [DataRow("Withdrawn", "Withdrawn By Agency", "Variation")]
        [DataRow("Withdrawn", "Withdrawn By Provider", "None")]
        [DataRow("Withdrawn", "Withdrawn By Provider", "Variation")]
        [TestMethod, TestCategory("Unit")]
        public async Task ValidateContractStatusAsync_ValidStatuses_ReturnsTrue(string parentStatus, string contractStatus, string amendmentType)
        {
            // Arrange
            ILoggerAdapter_Setup_LogInformation();
            IFeedProcessorConfiguration_Setup_GetValidationServiceStatuses();

            var service = GetValidationService();

            // Act
            var result = await service.ValidateContractStatusAsync(parentStatus, contractStatus, amendmentType);

            // Assert
            result.Should().BeTrue("Because the values are within the acceptable range.");
            Verify_All();
        }

        [DataRow("Draft", "Published to Provider", "Notification")]
        [DataRow("Approved", "Closed", "None")]
        [DataRow("Approved", "Closed", "Variation")]
        [DataRow("Approved", "Closed", "Notification")]
        [DataRow("Approved", "Published to Provider", "None")]
        [DataRow("Withdrawn", "Approved", "Variation")]
        [DataRow("Withdrawn", "Closed", "Variation")]
        [DataRow("Withdrawn", "Under Termination", "Variation")]
        [DataRow("Withdrawn", "Auto-Withdrawn", "None")]
        [DataRow("Withdrawn", "Auto-Withdrawn", "Variation")]
        [DataRow("cLosed", "Closed", "None")]
        [DataRow("cLosed", "Closed", "Variation")]
        [DataRow("cLosed", "Closed", "Notification")]
        [DataRow("cLosed", "Terminated", "None")]
        [DataRow("cLosed", "Terminated", "Variation")]
        [DataRow("cLosed", "Terminated", "Notification")]
        [TestMethod, TestCategory("Unit")]
        public async Task ValidateContractStatusAsync_InvalidStatuses_ReturnsFalse(string parentStatus, string contractStatus, string amendmentType)
        {
            // Arrange
            ILoggerAdapter_Setup_LogInformation();
            ILoggerAdapter_Setup_LogWarning();
            IFeedProcessorConfiguration_Setup_GetValidationServiceStatuses();

            var service = GetValidationService();

            // Act
            var result = await service.ValidateContractStatusAsync(parentStatus, contractStatus, amendmentType);

            // Assert
            result.Should().BeFalse("Because the values are within the known rejection range.");
            Verify_All();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateContractStatusAsync_InvalidParentStatus_ReturnFalse()
        {
            // Arrange
            string parentStatus = "sometestvalue";
            string contractStatus = "Approved";
            string amendmentType = "Variation";

            ILoggerAdapter_Setup_LogInformation();
            ILoggerAdapter_Setup_LogWarning();
            IFeedProcessorConfiguration_Setup_GetValidationServiceStatuses();

            var service = GetValidationService();

            // Act
            var result = await service.ValidateContractStatusAsync(parentStatus, contractStatus, amendmentType);

            // Assert
            result.Should().BeFalse("Because the parentStatus is not acceptable.");
            Verify_All();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateContractStatusAsync_InvalidContractStatus_ReturnFalse()
        {
            // Arrange
            string parentStatus = "Approved";
            string contractStatus = "sometestvalue";
            string amendmentType = "Variation";

            ILoggerAdapter_Setup_LogInformation();
            ILoggerAdapter_Setup_LogWarning();
            IFeedProcessorConfiguration_Setup_GetValidationServiceStatuses();

            var service = GetValidationService();

            // Act
            var result = await service.ValidateContractStatusAsync(parentStatus, contractStatus, amendmentType);

            // Assert
            result.Should().BeFalse("Because the contractStatus is not acceptable.");
            Verify_All();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateContractStatusAsync_InvalidAmendmentType_ReturnsFalse()
        {
            // Arrange
            string parentStatus = "Approved";
            string contractStatus = "Approved";
            string amendmentType = "sometestvalue";

            ILoggerAdapter_Setup_LogInformation();
            ILoggerAdapter_Setup_LogWarning();
            IFeedProcessorConfiguration_Setup_GetValidationServiceStatuses();

            var service = GetValidationService();

            // Act
            var result = await service.ValidateContractStatusAsync(parentStatus, contractStatus, amendmentType);

            // Assert
            result.Should().BeFalse("Because the amendmentType is not acceptable.");
            Verify_All();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task ValidateContractStatusAsync_MissingConfiguration_UsesDefault()
        {
            // Arrange
            string parentStatus = "Approved";
            string contractStatus = "Approved";
            string amendmentType = "None";

            var expected = GetDefaultStatuses();
            ValidationServiceConfigurationStatusesCollection actual = null;

            ILoggerAdapter_Setup_LogInformation();
            ILoggerAdapter_Setup_LogWarningException();

            Mock.Get(_configuration)
                .SetupSequence(p => p.GetValidationServiceStatuses())
                .Throws<KeyNotFoundException>()
                .Returns(Task.FromResult(expected));

            Mock.Get(_configuration)
                .Setup(p => p.SetValidationServiceStatuses(It.IsAny<ValidationServiceConfigurationStatusesCollection>()))
                .Callback((ValidationServiceConfigurationStatusesCollection config) => { actual = config; })
                .Returns(Task.CompletedTask)
                .Verifiable();

            var service = GetValidationService();

            // Act
            var result = await service.ValidateContractStatusAsync(parentStatus, contractStatus, amendmentType);

            // Assert
            result.Should().BeTrue("Because the supplied values are in the default configuration.");
            actual.Should().BeEquivalentTo(expected, "Because the default configuration and test configuration match.");

            Verify_All();
        }
        #endregion


        #region Arrange Helpers

        private IContractEventValidationService GetValidationService()
        {
            var settingsOptions = Options.Create<SchemaValidationSettings>(_validationSettings);
            return new ContractEventValidationService(_configuration, settingsOptions, _logger);
        }

        private void ILoggerAdapter_Setup_LogInformation()
        {
            Mock.Get(_logger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();
        }

        private void ILoggerAdapter_Setup_LogWarning()
        {
            Mock.Get(_logger)
                .Setup(p => p.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();
        }

        private void ILoggerAdapter_Setup_LogWarningException()
        {
            Mock.Get(_logger)
                .Setup(p => p.LogWarning(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();
        }

        private void ILoggerAdapter_Setup_LogErrorException()
        {
            Mock.Get(_logger)
                .Setup(p => p.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();
        }

        private void IFeedProcessorConfiguration_Setup_GetValidationServiceFundingTypes()
        {
            var rtn = GetDefaultFundingTypes();
            Mock.Get(_configuration)
                .Setup(p => p.GetValidationServiceFundingTypes())
                .Returns(Task.FromResult(rtn))
                .Verifiable();
        }

        private void IFeedProcessorConfiguration_Setup_GetValidationServiceStatuses()
        {
            var rtn = GetDefaultStatuses();
            Mock.Get(_configuration)
                .Setup(p => p.GetValidationServiceStatuses())
                .Returns(Task.FromResult(rtn))
                .Verifiable();
        }


        private ValidationServiceConfigurationStatusesCollection GetDefaultStatuses()
            => new ValidationServiceConfigurationStatusesCollection()
            {
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "draft", ContractStatus = "published to provider", AmendmentType = "none" },
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "draft", ContractStatus = "published to provider", AmendmentType = "variation" },
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "approved", ContractStatus = "approved", AmendmentType = "none" },
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "approved", ContractStatus = "approved", AmendmentType = "variation" },
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "approved", ContractStatus = "approved", AmendmentType = "notification" },
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "approved", ContractStatus = "modified", AmendmentType = "none" },
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "approved", ContractStatus = "modified", AmendmentType = "variation" },
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "approved", ContractStatus = "modified", AmendmentType = "notification" },
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "approved", ContractStatus = "under termination", AmendmentType = "none" },
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "approved", ContractStatus = "under termination", AmendmentType = "variation" },
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "approved", ContractStatus = "under termination", AmendmentType = "notification" },
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "withdrawn", ContractStatus = "withdrawn by agency", AmendmentType = "none" },
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "withdrawn", ContractStatus = "withdrawn by agency", AmendmentType = "variation" },
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "withdrawn", ContractStatus = "withdrawn by provider", AmendmentType = "none" },
                new ValidationServiceConfigurationStatuses() { ParentContractStatus = "withdrawn", ContractStatus = "withdrawn by provider", AmendmentType = "variation" },
            };

        private ValidationServiceConfigurationFundingTypes GetDefaultFundingTypes()
            => new ValidationServiceConfigurationFundingTypes()
            {
                "1619fund",
                "24+loans",
                "aeb",
                "age",
                "edsk",
                "eop",
                "esf",
                "levy",
                "loans",
                "main",
                "ncs",
                "nla",
                "nlg",
                "16-18fu",
                "dada",
                "ccf",
                "feca",
                "19trn2020",
                "aeb2021",
                "hte-pgf",
                "sadf",
                "fe-pdgp",
                "sdfii",
                "sb",
                "mult",
                "fe-aca",
                "hte-sif",
                "fe-rca"
            };

        #endregion


        #region Verify All

        private void Verify_All()
        {
            Mock.Get(_logger)
                .VerifyAll();

            Mock.Get(_configuration)
                .VerifyAll();
        }

        #endregion
    }
}
