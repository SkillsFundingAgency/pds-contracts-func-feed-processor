using System;
using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Configuration
{
    /// <summary>
    /// Interface that provices feed processing configurations.
    /// </summary>
    public interface IFeedProcessorConfiguration
    {
        /// <summary>
        /// Gets the last read bookmark identifier.
        /// </summary>
        /// <returns>The last read bookmark id from config.</returns>
        Task<Guid> GetLastReadBookmarkId();

        /// <summary>
        /// Sets the last read bookmark identifier.
        /// </summary>
        /// <param name="lastReadBookmarkId">The last read bookmark identifier.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SetLastReadBookmarkId(Guid lastReadBookmarkId);

        /// <summary>
        /// Gets the last read page.
        /// </summary>
        /// <returns>Last read page number.</returns>
        Task<int> GetLastReadPage();

        /// <summary>
        /// Sets the last read page.
        /// </summary>
        /// <param name="lastReadPage">The last read page.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SetLastReadPage(int lastReadPage);

        /// <summary>
        /// Gets the ValidationServiceStatuses from configuration.
        /// </summary>
        /// <returns>A collection of statues from configuration.</returns>
        Task<ValidationServiceConfigurationStatusesCollection> GetValidationServiceStatuses();

        /// <summary>
        /// Sets the ValidationServiceStatuses configuration in table storage.
        /// </summary>
        /// <param name="settings">The configuration to save.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SetValidationServiceStatuses(ValidationServiceConfigurationStatusesCollection settings);

        /// <summary>
        /// Gets the funding types from configuration.
        /// </summary>
        /// <returns>A collection of funding types.</returns>
        Task<ValidationServiceConfigurationFundingTypes> GetValidationServiceFundingTypes();

        /// <summary>
        /// Sets the funding types in table storage.
        /// </summary>
        /// <param name="settings">The configuration to save.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SetValidationServiceFundingTypes(ValidationServiceConfigurationFundingTypes settings);
    }
}