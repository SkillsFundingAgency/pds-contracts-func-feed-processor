using System.Threading.Tasks;
using System.Xml;

namespace Pds.Contracts.FeedProcessor.Services.Interfaces
{
    /// <summary>
    /// Functionality to validate a contract event from the atom feed.
    /// </summary>
    public interface IContractEventValidationService
    {
        /// <summary>
        /// Validates the given xml against the schema.
        /// </summary>
        /// <param name="contents">The xml.</param>
        /// <returns>True if the schema is valid, false otherwise.</returns>
        /// <exception cref="System.Xml.Schema.XmlSchemaValidationException">Raised if the schema is not valid.</exception>
        XmlDocument ValidateXmlWithSchema(string contents);

        /// <summary>
        /// Validates the status of the given contract.
        /// </summary>
        /// <param name="parentContractStatus">Status of the parent contract.</param>
        /// <param name="contractStatus">Status of the contract.</param>
        /// <param name="amendmentType">Amendment type for the contract.</param>
        /// <returns>True if the statuses are valid, false otherwise.</returns>
        Task<bool> ValidateContractStatusAsync(string parentContractStatus, string contractStatus, string amendmentType);

        /// <summary>
        /// Validates the funding type of the given contract.
        /// </summary>
        /// <param name="fundingType">The contract event to validate.</param>
        /// <returns>True if the funding type is valid, false otherwise.</returns>
        Task<bool> ValidateFundingTypeAsync(string fundingType);
    }
}
