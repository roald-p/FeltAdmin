namespace FeltAdmin.Configuration
{
    public static class ConfigurationFileReaderProvider
    {
        /// <summary>
        /// Cached reference to service-implementation.
        /// </summary>
        private static IConfigurationFileReader service = new ConfigurationFileReader();

        /// <summary>
        /// Gets an instance of IConfigurationFileReader.
        /// </summary>
        /// <remarks>Unless this property has been set to a specific class it will always return an instance of
        /// ConfigurationFileReader.
        /// The setter property is only intended to be used in test code.
        /// </remarks>
        public static IConfigurationFileReader Service
        {
            get
            {
                return service;
            }

            internal set
            {
                service = value;
                if (value == null)
                {
                    service = new ConfigurationFileReader();
                }
            }
        }
    }
}
