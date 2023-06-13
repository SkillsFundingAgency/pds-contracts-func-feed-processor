using Microsoft.Extensions.Options;
using Pds.Contracts.FeedProcessor.Services.Configuration;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace Pds.Contracts.FeedProcessor.Services.Implementations
{
    /// <summary>
    /// Provides functionality for contract event validation.
    /// </summary>
    public class ContractEventValidationService : IContractEventValidationService
    {
        private readonly SchemaValidationSettings _options;
        private readonly IFeedProcessorConfiguration _configReader;
        private readonly ILoggerAdapter<ContractEventValidationService> _logger;

        private readonly XmlSchema _xmlSchema = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractEventValidationService"/> class.
        /// </summary>
        /// <param name="configReader">Azure config loader.</param>
        /// <param name="options">Configuration options for the validation service.</param>
        /// <param name="logger">Logger Adapater to allow logging within the service.</param>
        public ContractEventValidationService(
            IFeedProcessorConfiguration configReader,
            IOptions<SchemaValidationSettings> options,
            ILoggerAdapter<ContractEventValidationService> logger)
        {
            _options = options.Value;
            _configReader = configReader;
            _logger = logger;

            if (_options.SchemaVersion == "11_06")
            {
                _logger.LogInformation($"[{nameof(ContractEventValidationService)}] Loading schema version 11.06.");
                _xmlSchema = ReadSchemaFile(_options.SchemaManifestFilename);
            }
            else
            {
                _logger.LogWarning($"[{nameof(ContractEventValidationService)}] - Active schema version is missing - Schema not loaded");
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateContractStatusAsync(string parentContractStatus, string contractStatus, string amendmentType)
        {
            _logger.LogInformation($"[{nameof(ValidateContractStatusAsync)}] Attempting to validate contract status '{parentContractStatus}/{contractStatus}/{amendmentType}'.");

            IList<ValidationServiceConfigurationStatuses> statusTernaries = await GetContractStatusesAsync();

            if (statusTernaries.Any(
                    p => p.ParentContractStatus.ToLower() == parentContractStatus.ToLower() &&
                    p.ContractStatus.ToLower() == contractStatus.ToLower() &&
                    p.AmendmentType.ToLower() == amendmentType.ToLower()))
            {
                _logger.LogInformation($"[{nameof(ValidateContractStatusAsync)}] Contract status '{parentContractStatus}/{contractStatus}/{amendmentType}' is valid..");
                return true;
            }

            _logger.LogWarning($"[{nameof(ValidateContractStatusAsync)}] Contract status '{parentContractStatus}/{contractStatus}/{amendmentType}' is NOT valid.");
            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateFundingTypeAsync(string fundingType)
        {
            _logger.LogInformation($"[{nameof(ValidateFundingTypeAsync)}] Attempting to validate funding type '{fundingType}'.");

            IList<string> acceptable = await GetFundingTypesAsync();

            if (acceptable.Contains(fundingType.ToLower()))
            {
                _logger.LogInformation($"[{nameof(ValidateFundingTypeAsync)}] Funding type '{fundingType}' is valid.");
                return true;
            }

            _logger.LogWarning($"[{nameof(ValidateFundingTypeAsync)}] Funding type is NOT valid.");
            return false;
        }

        /// <inheritdoc/>
        public XmlDocument ValidateXmlWithSchema(string contents)
        {
            _logger.LogInformation($"[{nameof(ValidateXmlWithSchema)}] Attempting to validate xml string.");

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(contents);

                if (_xmlSchema != null)
                {
                    var settings = new XmlReaderSettings();
                    settings.Schemas.Add(_xmlSchema);
                    settings.ValidationType = ValidationType.Schema;

                    xmlDocument.Schemas.Add(_xmlSchema);
                    xmlDocument.Validate(XmlValidationEventHandler);

                    _logger.LogInformation($"[{nameof(ValidateXmlWithSchema)}] Xml validation was successful.");
                }
                else
                {
                    // Schema file is not accessible or not found.
                    // The schema file may not be present if the code is being downloaded from github.
                    // The file should always be present in Azure.
                    _logger.LogWarning($"[{nameof(ValidateXmlWithSchema)}] XML validation failed - Embedded schema file not found.");
                }

                return xmlDocument;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"One or more errors occurred during schema validation. {ex.Message}");
                throw;
            }
        }


        #region Get Statuses

        private async Task<ValidationServiceConfigurationStatusesCollection> GetContractStatusesAsync()
        {
            try
            {
                return await _configReader.GetValidationServiceStatuses();
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.LogWarning(knfe, $"[{nameof(ContractEventValidationService)}.{nameof(GetContractStatusesAsync)}] KeyNotFoundException when requesting azure configuration for 'ValidationService'.  Using defaults.");

                // Raised if the required key is not defined in Azure
                // If this is not configured, we revert to default values.
                await _configReader.SetValidationServiceStatuses(GetDefaultStatuses());
                return await GetContractStatusesAsync();
            }
        }

        private async Task<ValidationServiceConfigurationFundingTypes> GetFundingTypesAsync()
        {
            try
            {
                return await _configReader.GetValidationServiceFundingTypes();
            }
            catch (KeyNotFoundException knfe)
            {
                _logger.LogWarning(knfe, $"[{nameof(ContractEventValidationService)}.{nameof(GetFundingTypesAsync)}] KeyNotFoundException when requesting azure configuration for 'ValidationService'.  Using defaults.");

                // Raised if the required key is not defined in Azure
                // If this is not configured, we revert to default values.
                await _configReader.SetValidationServiceFundingTypes(GetDefaultFundingTypes());
                return await GetFundingTypesAsync();
            }
        }

        #endregion


        #region Default config

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
                "fe-rca",
                "fe-ctf",
                "aeb2023"
            };

        #endregion

        private XmlSchema ReadSchemaFile(string embeddedFileName)
        {
            try
            {
                var assembly = typeof(ContractEventValidationService).Assembly;
                var resourceName = assembly.GetManifestResourceNames().First(s => s.EndsWith(embeddedFileName, StringComparison.CurrentCultureIgnoreCase));

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    return XmlSchema.Read(stream, SchemaValidationEventHandler);
                }
            }
            catch (Exception err)
            {
                _logger.LogError(err, $"[{nameof(ContractEventValidationService)}] - Failed to read embeded schema file '{embeddedFileName}");
            }

            return null;
        }

        private void SchemaValidationEventHandler(object sender, ValidationEventArgs e)
        {
            // One or more validatione errors when reading the schema file
            _logger.LogError(e.Exception, e.Message);
        }

        private void XmlValidationEventHandler(object send, ValidationEventArgs e)
        {
            if (_options.EnableSchemaVersionValidation == false)
            {
                _logger.LogWarning(e.Exception, $"[{nameof(ValidateXmlWithSchema)}] schema validation is turned OFF. Validation message: {e.Message}");
            }
            else
            {
                throw e.Exception;
            }
        }
    }
}
