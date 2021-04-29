namespace Pds.Contracts.FeedProcessor.Services.Configuration
{
    /// <summary>
    /// Represents a combination of statuses that are valid for a status.
    /// </summary>
    public class ValidationServiceConfigurationStatuses
    {
        /// <summary>
        /// Gets or sets the parent contract status.
        /// </summary>
        public string ParentContractStatus { get; set; }

        /// <summary>
        /// Gets or sets the contract status.
        /// </summary>
        public string ContractStatus { get; set; }

        /// <summary>
        /// Gets or sets the amendment type.
        /// </summary>
        public string AmendmentType { get; set; }
    }
}
