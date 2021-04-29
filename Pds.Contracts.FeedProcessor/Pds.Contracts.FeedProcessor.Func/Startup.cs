using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Pds.Contracts.FeedProcessor.Func;
using Pds.Contracts.FeedProcessor.Services.DependencyInjection;
using Pds.Core.Logging;
using Pds.Core.Telemetry.ApplicationInsights;
using System.IO;

// See: https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection
[assembly: FunctionsStartup(typeof(Startup))]

namespace Pds.Contracts.FeedProcessor.Func
{
    /// <summary>
    /// The startup class.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Gets or sets application startup configuration.
        /// </summary>
        public IConfigurationRoot Configuration { get; set; }

        /// <inheritdoc/>
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            base.ConfigureAppConfiguration(builder);

            Configuration = builder.ConfigurationBuilder.Build();
        }

        /// <inheritdoc/>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                  .AddEnvironmentVariables()
                  .Build();

            builder.Services.AddLoggerAdapter();
            builder.Services.AddPdsApplicationInsightsTelemetry(BuildAppInsightsConfiguration);
            builder.Services.AddFeatureServices(configuration);
        }

        private void BuildAppInsightsConfiguration(PdsApplicationInsightsConfiguration options)
        {
            // TODO : Determine where the App insights keys are to be defined - Environment? do we need devops?
            options.InstrumentationKey = System.Environment.GetEnvironmentVariable("PdsApplicationInsights:InstrumentationKey");
            options.Environment = System.Environment.GetEnvironmentVariable("PdsApplicationInsights:Environment");
            options.Component = GetType().Assembly.GetName().Name;
        }
    }
}