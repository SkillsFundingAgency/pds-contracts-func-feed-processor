using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Configuration
{
    /// <summary>
    /// Wrapper class to access azure table storage.
    /// </summary>
    public class AzureTableStorageConfigReader : IConfigReader
    {
        private readonly CloudTable _table;
        private readonly TableAccessOptions _configOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableStorageConfigReader" /> class.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="options">The options.</param>
        public AzureTableStorageConfigReader(CloudTable table, IOptions<AzureStorageAccountOptions> options)
        {
            _table = table;
            _configOptions = options.Value.TableAccessOptions;
        }

        /// <inheritdoc/>
        public async Task<T> GetConfigAsync<T>(string key)
        {
            var getOperation = TableOperation.Retrieve<FeedProcessorConfigItem<T>>(_configOptions.PartitionKey, key);
            var result = await _table.ExecuteAsync(getOperation);
            var feedProcessorConfig = result?.Result as FeedProcessorConfigItem<T> ?? throw new KeyNotFoundException($"Missing config key:{key} in azure config table: {_table.Name}");

            return feedProcessorConfig.Data;
        }

        /// <inheritdoc/>
        public async Task<T> SetConfigAsync<T>(string key, T value)
        {
            var setOperation = TableOperation.InsertOrReplace(new FeedProcessorConfigItem<T>
            {
                PartitionKey = _configOptions.PartitionKey,
                RowKey = key,
                Data = value
            });

            var result = await _table.ExecuteAsync(setOperation);
            var feedProcessorConfig = result?.Result as FeedProcessorConfigItem<T> ?? throw new InvalidOperationException($"Unable to add config key:{key} with value: [{JsonConvert.SerializeObject(value)}] to azure config table: {_table.Name}");

            return feedProcessorConfig.Data;
        }
    }
}