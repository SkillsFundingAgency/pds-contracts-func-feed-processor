using Pds.Core.ApiClient;

namespace Pds.Contracts.FeedProcessor.Services.Configuration
{
    /// <summary>
    /// Feed reader options.
    /// </summary>
    public class FeedReaderOptions : BaseApiClientConfiguration
    {
        /// <summary>
        /// Gets or sets the FCS atom feed self page endpoint.
        /// </summary>
        /// <value>
        /// The FCS atom feed self page endpoint.
        /// </value>
        public string FcsAtomFeedSelfPageEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the HTTP policy options.
        /// </summary>
        /// <value>
        /// The HTTP policy options.
        /// </value>
        public HttpPolicyOptions HttpPolicyOptions { get; set; }
    }
}