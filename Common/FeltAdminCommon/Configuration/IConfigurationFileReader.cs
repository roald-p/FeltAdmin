namespace FeltAdmin.Configuration
{
    public interface IConfigurationFileReader
    {
        /// <summary>
        /// Get av value from appSettings in the configuration-file
        /// If keyName is an empty string an ArgumentNullExcepiton is thrown.
        /// If no key exists an empty string is returned.
        /// </summary>
        /// <remarks>If keyName is prefixed with .deliverable. (ConfigurationFileReader.DeliverablePrefix) the 
        /// value is fetched from attributes in the .deliverable file not the .config-file. </remarks>
        /// <param name="keyName">Name of the key for which a value must be fetched.</param>
        /// <returns>Configured value.</returns>
        string GetValue(string keyName);

        /// <summary>
        /// Use this method to get a configuration value, checking if the value is found in
        /// the config-fiel.
        /// </summary>
        /// <remarks>If keyName is prefixed with .deliverable. (ConfigurationFileReader.DeliverablePrefix) the 
        /// value is fetched from attributes in the .deliverable file not the .config-file. </remarks>
        /// <param name="keyName">Name of the key for which a value must be fetched.</param>
        /// <param name="value">Configured value will be returned in this parameter.</param>
        /// <returns>true if key exists in config-file; false otherwise.</returns>
        bool TryGetValue(string keyName, out string value);
    }
}
