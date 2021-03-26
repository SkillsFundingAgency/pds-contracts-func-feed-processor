using AutoMapper;
using Microsoft.Extensions.Options;
using Pds.Contracts.FeedProcessor.Services.Configuration;
using Pds.Contracts.FeedProcessor.Services.Interfaces;
using Pds.Contracts.FeedProcessor.Services.Models;
using Pds.Core.ApiClient;
using Pds.Core.ApiClient.Interfaces;
using System.Collections.Generic;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Pds.Contracts.FeedProcessor.Services.Implementations
{
    /// <summary>
    /// FCS contract events ATOM feed reader.
    /// </summary>
    public class FcsFeedReaderService : BaseApiClient<FeedReaderOptions>, IFcsFeedReaderService
    {
        private readonly FeedReaderOptions _feedReaderOptions;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="FcsFeedReaderService" /> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="mapper">The AutoMapper for mapping values.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        public FcsFeedReaderService(
            IAuthenticationService<FeedReaderOptions> authenticationService,
            HttpClient httpClient,
            IMapper mapper,
            IOptions<FeedReaderOptions> configurationOptions) : base(authenticationService, httpClient, configurationOptions)
        {
            _feedReaderOptions = configurationOptions.Value;
            _mapper = mapper;
        }

        /// <inheritdoc/>
        public IList<FeedEntry> ExtractContractEventsFromFeedPageAsync(string payload)
        {
            var formatter = new Atom10FeedFormatter();
            var doc = XDocument.Parse(payload);
            using (var reader = doc.CreateReader())
            {
                formatter.ReadFrom(reader);
            }

            return _mapper.Map<IList<FeedEntry>>(formatter.Feed.Items);
        }

        /// <inheritdoc/>
        public async Task<IList<FeedEntry>> ReadSelfPageAsync()
        {
            var result = await Get<string>(_feedReaderOptions.FcsAtomFeedSelfPageEndpoint);
            return ExtractContractEventsFromFeedPageAsync(result);
        }
    }
}