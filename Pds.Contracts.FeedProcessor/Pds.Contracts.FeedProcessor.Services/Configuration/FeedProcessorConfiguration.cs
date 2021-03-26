using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Configuration
{
    /// <summary>
    /// Feed processor configurations.
    /// </summary>
    /// <seealso cref="Pds.Contracts.FeedProcessor.Services.Configuration.IFeedProcessorConfiguration" />
    public class FeedProcessorConfiguration : IFeedProcessorConfiguration
    {
        private const string _lastReadBookmarkId = "LastReadBookmarkId";
        private const string _lastReadPage = "LastReadPage";

        private const string _validationServiceFundingTypes = "ValidationServiceFundingTypes";
        private const string _validationServiceStatuses = "ValidationServiceStatuses";

        private readonly IConfigReader _configReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedProcessorConfiguration"/> class.
        /// </summary>
        /// <param name="configReader">The configuration reader.</param>
        public FeedProcessorConfiguration(IConfigReader configReader)
        {
            _configReader = configReader;
        }

        /// <inheritdoc/>
        public async Task<Guid> GetLastReadBookmarkId()
        {
            return await _configReader.GetConfigAsync<Guid>(_lastReadBookmarkId).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SetLastReadBookmarkId(Guid lastReadBookmarkId)
        {
            await _configReader.SetConfigAsync(_lastReadBookmarkId, lastReadBookmarkId);
        }

        /// <inheritdoc/>
        public async Task<int> GetLastReadPage()
        {
            return await _configReader.GetConfigAsync<int>(_lastReadPage).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SetLastReadPage(int lastReadPage)
        {
            await _configReader.SetConfigAsync(_lastReadPage, lastReadPage);
        }

        /// <inheritdoc/>
        public async Task<ValidationServiceConfigurationStatusesCollection> GetValidationServiceStatuses()
        {
            string data = await _configReader.GetConfigAsync<string>(_validationServiceStatuses);
            if (data == null)
            {
                throw new JsonSerializationException("Configuration settings are not valid.");
            }

            return JsonConvert.DeserializeObject<ValidationServiceConfigurationStatusesCollection>(data);
        }

        /// <inheritdoc/>
        public async Task SetValidationServiceStatuses(ValidationServiceConfigurationStatusesCollection settings)
        {
            string data = JsonConvert.SerializeObject(settings);
            await _configReader.SetConfigAsync(_validationServiceStatuses, data);
        }

        /// <inheritdoc/>
        public async Task<ValidationServiceConfigurationFundingTypes> GetValidationServiceFundingTypes()
        {
            string data = await _configReader.GetConfigAsync<string>(_validationServiceFundingTypes);
            if (data == null)
            {
                throw new JsonSerializationException("Configuration settings are not valid.");
            }

            return JsonConvert.DeserializeObject<ValidationServiceConfigurationFundingTypes>(data);
        }

        /// <inheritdoc/>
        public async Task SetValidationServiceFundingTypes(ValidationServiceConfigurationFundingTypes settings)
        {
            string data = JsonConvert.SerializeObject(settings);
            await _configReader.SetConfigAsync(_validationServiceFundingTypes, data);
        }
    }
}