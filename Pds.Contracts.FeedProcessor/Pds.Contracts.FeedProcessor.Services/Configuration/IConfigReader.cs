using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Configuration
{
    /// <summary>
    /// Interface that outlines methods to allow retrieval and storage of configuration within an Azure Storage Table.
    /// </summary>
    public interface IConfigReader
    {
        /// <summary>
        /// Gets the given configuration element from the table storage.
        /// </summary>
        /// <typeparam name="T">The type to read the content as.</typeparam>
        /// <param name="key">The key to identify the element in table.</param>
        /// <returns>A type of <typeparamref name="T"/> with details from the database.</returns>
        /// <exception cref="KeyNotFoundException">Raised if the request key is not found.</exception>
        Task<T> GetConfigAsync<T>(string key);

        /// <summary>
        /// Sets the value for the given key within the table storage.
        /// If the value is already present, it will be overwritten.
        /// </summary>
        /// <typeparam name="T">The type to read the content as.</typeparam>
        /// <param name="key">The key to identify the element in table.</param>
        /// <param name="value">The value to storage within the table.</param>
        /// <returns>A type of <typeparamref name="T"/> with details from the database.</returns>
        /// <exception cref="InvalidOperationException">Raised if the save operation was not successful.</exception>
        Task<T> SetConfigAsync<T>(string key, T value);
    }
}
