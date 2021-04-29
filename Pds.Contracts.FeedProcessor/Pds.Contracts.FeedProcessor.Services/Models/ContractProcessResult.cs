using System.Collections.Generic;
using System.Xml;

namespace Pds.Contracts.FeedProcessor.Services.Models
{
    /// <summary>
    /// Represents an result from <see cref="IContractEventProcessor"/>.
    /// </summary>
    public class ContractProcessResult
    {
        /// <summary>
        /// Gets or sets the result of the operation.
        /// </summary>
        public ContractProcessResultType Result { get; set; }

        /// <summary>
        /// Gets or sets the contact event.
        /// Value may be null if result was unsucessful.
        /// </summary>
        public ContractEvent ContractEvent { get; set; }

        /// <summary>
        /// Gets or sets the xml for this contract.
        /// Value may be null if the contract xml is invalid.
        /// </summary>
        public XmlDocument ContractXml { get; set; }
    }
}
