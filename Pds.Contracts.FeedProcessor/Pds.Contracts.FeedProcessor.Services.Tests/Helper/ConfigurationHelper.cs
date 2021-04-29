using Microsoft.Extensions.Configuration;
using System;

namespace Pds.Contracts.FeedProcessor.Services.Tests.Helper
{
    /// <summary>
    /// Loads and provides functions to bind configuration from the <see cref="AppDomain.CurrentDomain.BaseDirectory"/>.
    /// </summary>
    public class ConfigurationHelper
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationHelper"/> class.
        /// </summary>
        public ConfigurationHelper()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        /// <summary>
        /// Gets the base configuration object.
        /// </summary>
        public IConfiguration Configuration => _configuration;

        /// <summary>
        /// Gets configuration with the specified section name.
        /// The <typeparamref name="T"/> must have a parameterless constructor.
        /// </summary>
        /// <typeparam name="T">The type to bind the configuration to.</typeparam>
        /// <param name="sectionName">The configuration section to bind from.</param>
        /// <returns>An instance of <typeparamref name="T"/> with the configuration details.</returns>
        public T GetConfiguration<T>(string sectionName)
            where T : new()
        {
            var rtn = new T();

            _configuration.GetSection(sectionName).Bind(rtn);

            return rtn;
        }
    }
}
