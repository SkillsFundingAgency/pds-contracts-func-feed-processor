namespace Pds.Contracts.FeedProcessor.Services.Interfaces
{
    /// <summary>
    /// Functionality to allow files to be saved to a blob store.
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// Uploads the given <paramref name="contents"/> to the currently assigned azure storage contrainer with the filename <paramref name="filename"/>.
        /// </summary>
        /// <param name="filename">Name of the file in Azure.</param>
        /// <param name="contents">Contents to upload.</param>
        /// <param name="overwrite">Overwrite the contents if they exists.</param>
        /// <exception cref="Azure.RequestFailedException">Raised on upload failure.</exception>
        void Upload(string filename, byte[] contents, bool overwrite = true);
    }
}
