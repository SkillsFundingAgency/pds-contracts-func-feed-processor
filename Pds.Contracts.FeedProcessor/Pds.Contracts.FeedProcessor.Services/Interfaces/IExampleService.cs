using System.Threading.Tasks;

namespace Pds.Contracts.FeedProcessor.Services.Interfaces
{
    /// <summary>
    /// Example service.
    /// </summary>
    public interface IExampleService
    {
        /// <summary>
        /// Hello.
        /// </summary>
        /// <returns>The hello string.</returns>
        Task<string> Hello();
    }
}