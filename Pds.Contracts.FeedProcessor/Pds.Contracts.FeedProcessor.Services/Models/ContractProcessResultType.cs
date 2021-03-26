namespace Pds.Contracts.FeedProcessor.Services.Models
{
    /// <summary>
    /// Represents the results from Contract processor.
    /// </summary>
    public enum ContractProcessResultType
    {
        /// <summary>
        /// The processing request failed.
        /// </summary>
        OperationFailed,

        /// <summary>
        /// The contract status is excluded/ignored.
        /// </summary>
        StatusValidationFailed,

        /// <summary>
        /// The contract funding type is excluded/ignored.
        /// </summary>
        FundingTypeValidationFailed,

        /// <summary>
        /// The validation process was successful.
        /// </summary>
        Successful
    }
}