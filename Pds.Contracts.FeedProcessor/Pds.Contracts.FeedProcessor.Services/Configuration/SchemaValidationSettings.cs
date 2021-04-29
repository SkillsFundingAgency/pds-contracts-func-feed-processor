namespace Pds.Contracts.FeedProcessor.Services.Configuration
{
    /// <summary>
    /// Represents a collection of settings for the ValidationService.
    /// </summary>
    public class SchemaValidationSettings
    {
        /// <summary>
        /// Gets or sets the version of the currenly active schema.
        /// </summary>
        public string SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the name of the manifest file where the current schema is located.
        /// </summary>
        public string SchemaManifestFilename { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether schema version validation is enabled or disabled.
        /// </summary>
        public bool EnableSchemaVersionValidation { get; set; } = false;
    }
}
