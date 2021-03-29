using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using Pds.Contracts.FeedProcessor.Services.Models;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace Pds.Contracts.FeedProcessor.Services.Implementations
{
    /// <summary>
    /// Provides functionality to deserialize a contract conforming to v11.03 schema.
    /// </summary>
    public class Deserializer_v1103 : IDeserilizationService<ContractProcessResult>
    {
        /// <summary>
        /// The xml namespace used by conrtacts.
        /// </summary>
        public const string _contractEvent_Namespace = "urn:sfa:schemas:contract";

        private const string _auditApiUser = "System - Contract.FeedProcessor.Func";

        private readonly IContractEventValidationService _validationService;
        private readonly IAuditService _auditService;
        private readonly ILoggerAdapter<Deserializer_v1103> _loggerAdapter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Deserializer_v1103"/> class.
        /// </summary>
        /// <param name="validationService">Service for validation of contract event.</param>
        /// /// <param name="auditService">Service for creating audit entries.</param>
        /// <param name="loggerAdapter">Logger adapter for internal process logging.</param>
        public Deserializer_v1103(
            IContractEventValidationService validationService,
            IAuditService auditService,
            ILoggerAdapter<Deserializer_v1103> loggerAdapter)
        {
            _validationService = validationService;
            _auditService = auditService;
            _loggerAdapter = loggerAdapter;
        }

        /// <inheritdoc/>
        public async Task<ContractProcessResult> DeserializeAsync(string xml)
        {
            var rtn = new ContractProcessResult() { Result = ContractProcessResultType.OperationFailed };

            _loggerAdapter.LogInformation($"[{nameof(Deserializer_v1103)}.{nameof(DeserializeAsync)}] - Called to deserilise xml string.");

            // Ensure xml is valid.
            _validationService.ValidateXmlWithSchema(xml);

            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);

            var ns = new XmlNamespaceManager(new NameTable());
            ns.AddNamespace("c", _contractEvent_Namespace);

            // The content element may be in either case.
            var details = (document["content"] ?? document["Content"])["contract"];

            var contractList = new List<ContractEvent>();

            var contracts = details.SelectNodes("c:contracts/c:contract", ns);
            foreach (XmlElement contract in contracts)
            {
                var contractStatus = contract.SelectSingleNode("c:contractStatus/c:status", ns).InnerText;
                var parentStatus = contract.SelectSingleNode("c:contractStatus/c:parentStatus", ns).InnerText;

                var amendmentType = contract.SelectSingleNode("c:amendmentType", ns)?.InnerText ?? "None";
                var fundingType = contract.SelectSingleNode("c:fundingType/c:fundingTypeCode", ns).InnerText;

                ContractEvent evt = DeserializeContractEvent(contract, ns);

                // Validate statuses
                if (await _validationService.ValidateContractStatusAsync(parentStatus, contractStatus, amendmentType) == false)
                {
                    rtn.Result = ContractProcessResultType.StatusValidationFailed;

                    string msg = $"Contract event for Contract [{evt.ContractNumber}] Version [{evt.ContractVersion}] with parent status [{parentStatus}], status [{contractStatus}], and amendment type [{amendmentType}] has been ignored.";
                    _loggerAdapter.LogWarning(msg);
                    await _auditService.TrySendAuditAsync(new Audit.Api.Client.Models.Audit()
                    {
                        Action = Audit.Api.Client.Enumerations.ActionType.ContractFeedEventFilteredOut,
                        Message = msg,
                        Severity = Audit.Api.Client.Enumerations.SeverityLevel.Information,
                        Ukprn = evt.UKPRN,
                        User = _auditApiUser
                    });
                }
                else if (await _validationService.ValidateFundingTypeAsync(fundingType) == false)
                {
                    rtn.Result = ContractProcessResultType.FundingTypeValidationFailed;

                    string msg = $"Contract event for Contract [{evt.ContractNumber}] Version [{evt.ContractVersion}] with funding type [{fundingType}] has been ignored.";
                    _loggerAdapter.LogWarning(msg);
                    await _auditService.TrySendAuditAsync(new Audit.Api.Client.Models.Audit()
                    {
                        Action = Audit.Api.Client.Enumerations.ActionType.ContractFeedEventFilteredOut,
                        Message = msg,
                        Severity = Audit.Api.Client.Enumerations.SeverityLevel.Information,
                        Ukprn = evt.UKPRN,
                        User = _auditApiUser
                    });
                }

                contractList.Add(evt);
            }

            rtn.ContactEvents = contractList;
            if (rtn.Result == ContractProcessResultType.OperationFailed)
            {
                rtn.Result = ContractProcessResultType.Successful;
            }

            _loggerAdapter.LogInformation($"[{nameof(Deserializer_v1103)}.{nameof(DeserializeAsync)}] - Deserialistion completed.");
            return rtn;
        }

        private ContractEvent DeserializeContractEvent(XmlElement contractElement, XmlNamespaceManager ns)
        {
            var evt = new ContractEvent();

            // UKPRN must always be present
            var ukprnNode = contractElement.SelectSingleNode("c:contractor/c:ukprn", ns);
            if (ukprnNode == null)
            {
                throw new InvalidOperationException($"[{nameof(Deserializer_v1103)}] required node 'ukprn' is missing.");
            }

            evt.UKPRN = int.Parse(ukprnNode.InnerText);

            evt.ContractNumber = contractElement.SelectSingleNode("c:contractNumber", ns).InnerText;
            evt.ContractVersion = int.Parse(contractElement.SelectSingleNode("c:contractVersionNumber", ns).InnerText);

            var parentContractNumberNode = contractElement.SelectSingleNode("c:parentContractNumber", ns);
            if (parentContractNumberNode == null)
            {
                throw new InvalidOperationException($"[{nameof(DeserializeContractEvent)} required node 'parentContractNumber' is missing.");
            }

            evt.ParentContractNumber = parentContractNumberNode.InnerText;

            var contractStatus = contractElement.SelectSingleNode("c:contractStatus/c:status", ns).InnerText;
            evt.Status = ParseContractStatus(contractStatus);

            var parentStatus = contractElement.SelectSingleNode("c:contractStatus/c:parentStatus", ns).InnerText;
            evt.ParentStatus = Enum.Parse<ContractParentStatus>(parentStatus);

            evt.ContractPeriodValue = contractElement.SelectSingleNode("c:period/c:period", ns).InnerText;

            evt.Value = decimal.Parse(contractElement.SelectSingleNode("c:contractValue", ns)?.InnerText ?? "0");

            var amendmentType = contractElement.SelectSingleNode("c:amendmentType", ns)?.InnerText ?? "None";
            evt.AmendmentType = Enum.Parse<ContractAmendmentType>(amendmentType);
            evt.Type = contractElement.SelectSingleNode("c:contractType", ns)?.InnerText;

            var fundingType = contractElement.SelectSingleNode("c:fundingType/c:fundingTypeCode", ns).InnerText;
            evt.FundingType = ParseContractFundingType(fundingType);

            // Start date can be null
            string startDate = contractElement.SelectSingleNode("c:startDate", ns)?.InnerText;
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                evt.StartDate = DateTime.Parse(startDate);
            }

            // End date can be null
            string endDate = contractElement.SelectSingleNode("c:endDate", ns)?.InnerText;
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                evt.EndDate = DateTime.Parse(endDate);
            }

            string signedOn = contractElement.SelectSingleNode("c:ContractApprovalDate", ns)?.InnerText;
            if (!string.IsNullOrWhiteSpace(signedOn))
            {
                evt.SignedOn = DateTime.Parse(signedOn).Date;
            }

            evt.ContractAllocations = ExtractAllocations(contractElement, ns);

            return evt;
        }

        private List<ContractAllocation> ExtractAllocations(XmlElement mainNode, XmlNamespaceManager ns)
        {
            var allocations = new List<ContractAllocation>();
            var allocationElements = mainNode.SelectNodes("c:contractAllocations/c:contractAllocation", ns);

            foreach (XmlElement allocationElement in allocationElements)
            {
                var fundingStreamPeriodCodeNode = allocationElement.SelectSingleNode($"c:fundingStreamPeriodCode", ns);
                if (fundingStreamPeriodCodeNode == null)
                {
                    throw new InvalidOperationException($"[{nameof(Deserializer_v1103)}] required field 'fundingStreamPeriodCode' is missing.");
                }

                var allocation = new ContractAllocation()
                {
                    ContractAllocationNumber = allocationElement.SelectSingleNode($"c:contractAllocationNumber", ns).InnerText,

                    // Funding Stream Period Code must always be present
                    FundingStreamPeriodCode = fundingStreamPeriodCodeNode.InnerText,

                    LEPArea = allocationElement.SelectSingleNode($"c:ProcurementAttrs/c:LEPName", ns)?.InnerText,
                    TenderSpecTitle = allocationElement.SelectSingleNode($"c:ProcurementAttrs/c:TenderSpecTitle", ns)?.InnerText
                };

                allocations.Add(allocation);
            }

            return allocations;
        }

        private ContractStatus ParseContractStatus(string status)
        {
            return status.ToLower() switch
            {
                "draft" => ContractStatus.Draft,
                "approved" => ContractStatus.Approved,
                "unassigned" => ContractStatus.Unassigned,
                "in review" => ContractStatus.InReview,
                "awaiting internal approval" => ContractStatus.AwaitingInternalApproval,
                "published to provider" => ContractStatus.PublishedToProvider,
                "withdrawn by provider" => ContractStatus.WithdrawnByProvider,
                "withdrawn by agency" => ContractStatus.WithdrawnByAgency,
                "closed" => ContractStatus.Closed,
                "under termination" => ContractStatus.UnderTermination,
                "terminated" => ContractStatus.Terminated,
                "modified" => ContractStatus.Modified,

                _ => throw new InvalidOperationException($"Status [{status}] is not valid."),
            };
        }

        private ContractParentStatus ParseParentContractStatus(string status)
        {
            return Enum.Parse<ContractParentStatus>(status);
        }

        private ContractFundingType ParseContractFundingType(string fundingType)
        {
            if (fundingType == string.Empty)
            {
                return ContractFundingType.Unknown;
            }

            return fundingType.ToLower() switch
            {
                "main" => ContractFundingType.Mainstream,
                "esf" => ContractFundingType.Esf,
                "24+loans" => ContractFundingType.TwentyFourPlusLoan,
                "age" => ContractFundingType.Age,
                "eop" => ContractFundingType.Eop,
                "eof" => ContractFundingType.Eof,
                "levy" => ContractFundingType.Levy,
                "ncs" => ContractFundingType.Ncs,
                "1619fund" => ContractFundingType.SixteenNineteenFunding,
                "aeb" => ContractFundingType.Aebp,
                "nla" => ContractFundingType.Nla,
                "loans" => ContractFundingType.AdvancedLearnerLoans,
                "edsk" => ContractFundingType.EducationAndSkillsFunding,
                "nlg" => ContractFundingType.NonLearningGrant,
                "16-18fu" => ContractFundingType.SixteenEighteenForensicUnit,
                "dada" => ContractFundingType.DanceAndDramaAwards,
                "ccf" => ContractFundingType.CollegeCollaborationFund,
                "feca" => ContractFundingType.FurtherEducationConditionAllocation,
                "19trn2020" => ContractFundingType.ProcuredNineteenToTwentyFourTraineeship,
                _ => throw new InvalidOperationException($"ContractFundingType [{fundingType}] is not valid.")
            };
        }
    }
}
