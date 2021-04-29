using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Interfaces
{
    /// <summary>
    /// Provides functionality to deserialise an XML string to a given type.
    /// </summary>
    /// <typeparam name="T">The type to deserialise to.</typeparam>
    public interface IDeserilizationService<T>
        where T : class
    {
        /// <summary>
        /// Deserialises the given XML string to a list containing instances of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="xml">Xml to deserilise.</param>
        /// <returns>Object with the contents from xml.</returns>
        Task<IList<T>> DeserializeAsync(string xml);
    }
}
