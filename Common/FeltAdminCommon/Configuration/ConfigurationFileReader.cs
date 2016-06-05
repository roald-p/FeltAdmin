using System;
using System.Configuration;
using System.IO;
using System.Xml;

using FeltAdmin.Diagnostics;

namespace FeltAdmin.Configuration
{
    public sealed class ConfigurationFileReader : IConfigurationFileReader
    {
        /// <summary>
        /// This is the prefix that is used when asking the ConfigurationFileReader for settings stored
        /// in the deliverable file.
        /// </summary>
        public static readonly string DeliverablePrefix = ".deliverable.";

        /// <summary>
        /// Cached deliverable file name. This does not change while a process is running.
        /// </summary>
        private static string s_deliverableFileName;

        private readonly Func<System.Configuration.Configuration> m_getConfiguration;

        private System.Configuration.Configuration m_runningConfig;

        public ConfigurationFileReader()
        {
            ////VirtualPathExtension virtualPathExtension = GetVirtualPathExtensionForWcfServices();

            ////if (HttpContext.Current != null)
            ////{
            ////    // 18.02.2001 - GFI: Rettet feil som førte til at man ikke fikk installert
            ////    //                   DIPS-WebService på Vista/IIS7 Integrated mode. Se link under for detaljer.
            ////    // http://mvolo.com/blogs/serverside/archive/2007/11/10/Integrated-mode-Request-is-not-available-in-this-context-in-Application_5F00_Start.aspx
            ////    this.m_getConfiguration = () => WebConfigurationManager.OpenWebConfiguration(HttpRuntime.AppDomainAppVirtualPath);
            ////}
            ////else if (virtualPathExtension != null)
            ////{
            ////    this.m_getConfiguration = () => WebConfigurationManager.OpenWebConfiguration(virtualPathExtension.VirtualPath);
            ////}
            ////else if (!string.IsNullOrWhiteSpace(HttpRuntime.AppDomainAppVirtualPath))
            ////{
            ////    this.m_getConfiguration = () => WebConfigurationManager.OpenWebConfiguration(HttpRuntime.AppDomainAppVirtualPath);
            ////}
            ////else
            ////{
            this.m_getConfiguration = () =>
            {
                try
                {
                    return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                }
                catch (Exception)
                {
                    var fileMap = new ExeConfigurationFileMap
                    { ExeConfigFilename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile };
                    return ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                }

            };
            ////}
        }

        ////private static VirtualPathExtension GetVirtualPathExtensionForWcfServices()
        ////{
        ////    // WCF services hosted in IIS...
        ////    VirtualPathExtension p;
        ////    try
        ////    {
        ////        OperationContext context = OperationContext.Current;

        ////        if (context != null)
        ////        {
        ////            p = context.Host.Extensions.Find<VirtualPathExtension>();
        ////        }
        ////        else
        ////        {
        ////            p = null;
        ////        }
        ////    }
        ////    catch (Exception)
        ////    {
        ////        p = null;
        ////    }

        ////    return p;
        ////}

        public string ConfigName { get; set; }

        #region IConfigurationFileReader Members

        /// <summary>
        /// Call this method to get a value from the configurationfile for the application.
        /// </summary>
        /// <remarks>If keyName is prefixed with .deliverable. (ConfigurationFileReader.DeliverablePrefix) the 
        /// value is fetched from attributes in the .deliverable file not the .config-file. </remarks>
        /// <param name="keyName">Key for the config-value to retrieve.</param>
        /// <returns>The value assosiated with the key in the configuration file. String.Empty is returned if no value is found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if keyName is null.</exception>
        public string GetValue(string keyName)
        {
            if (String.IsNullOrEmpty(keyName))
            {
                throw new ArgumentNullException("keyName");
            }

            string value;
            if (keyName.StartsWith(DeliverablePrefix, StringComparison.OrdinalIgnoreCase))
            {
                TryGetDeliverableAttribute(keyName.Substring(DeliverablePrefix.Length), out value);
            }
            else
            {
#if !PocketPC
                if (String.IsNullOrEmpty(this.ConfigName))
                {
                    this.m_runningConfig = this.m_getConfiguration();
                }
                else
                {
                    var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = this.ConfigName };
                    this.m_runningConfig = ConfigurationManager.OpenMappedExeConfiguration(
                        fileMap, ConfigurationUserLevel.None);
                }

                value = this.GetValueFromConfig(keyName);
#else
                value = DIPS.Core.Configuration.ConfigurationManager.AppSettings[keyName];
#endif
            }

            return value ?? String.Empty;
        }

        /// <summary>
        /// Gets the value assosiated with specific key without throwing exception if key does not exist.
        /// </summary>
        /// <param name="keyName">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, 
        /// if the key is found; otherwise, the default value for the type of the value parameter. 
        /// This parameter is passed uninitialized.</param>
        /// <returns>true if the key was found; otherwise, false.</returns>
        public bool TryGetValue(string keyName, out string value)
        {
            value = this.GetValue(keyName);
            return !String.IsNullOrEmpty(value);
        }

        #endregion

        private string GetValueFromConfig(string keyName)
        {
            string value = null;
                KeyValueConfigurationElement element = this.m_runningConfig.AppSettings.Settings[keyName];
                if (element != null)
                {
                    value = element.Value;
                }
            return value;
        }

        /// <summary>
        /// Gets the name of the deliverable file.
        /// </summary>
        /// <returns>Filename (no path).</returns>
        internal static string GetDeliverableFileName()
        {
            if (String.IsNullOrEmpty(s_deliverableFileName))
            {
#if !PocketPC

                string configFileName = Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
                string configFilePath = Path.GetDirectoryName(
                    AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

                int configIdx = configFileName.LastIndexOf(".config", StringComparison.OrdinalIgnoreCase);

                if (configIdx > 0)
                {
                    string appName = configFileName.Substring(
                        0,
                        configFileName.LastIndexOf(".config", StringComparison.OrdinalIgnoreCase));

                    s_deliverableFileName = Path.Combine(configFilePath, appName + ".deliverable");

                    Log.Trace("Deliverable definition in file: {0}", s_deliverableFileName);
                }
                else
                {
                    s_deliverableFileName = null;
                    Log.Trace("No .config file found. Could not create deliverable file name from: " + configFileName);
                }

#else
                string assemblyPath = Assembly.GetCallingAssembly().GetName().CodeBase;
                s_deliverableFileName = Path.GetDirectoryName(assemblyPath) + "\\App.deliverable";
#endif
            }

            return s_deliverableFileName;
        }

        /// <summary>
        /// Gets the value assosiated with specific attributeName from the deliverable file 
        /// without throwing exception if key does not exist.
        /// </summary>
        /// <param name="attributeName">The attributeName of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified attributeName, 
        /// if the key is found; otherwise, the default value for the type of the value parameter. 
        /// This parameter is passed uninitialized.</param>
        private static void TryGetDeliverableAttribute(string attributeName, out string value)
        {
            value = null;
            string deliverableFileName = GetDeliverableFileName();

            if (String.IsNullOrEmpty(deliverableFileName) || !File.Exists(deliverableFileName))
            {
                return;
            }

#if !PocketPC
            Log.Trace("Opening .deliverable: {0}", deliverableFileName);
#endif
            using (var fs = new FileStream(GetDeliverableFileName(), FileMode.Open, FileAccess.Read))
            {
                using (XmlReader reader = XmlReader.Create(fs))
                {
                    // Changed from reader.Read() to reader.ReadToFollowing(...)
                    // to prevent error in case of XML declaration in the .deliverable file.
                    // ex. <?xml version="1.0" standalone="yes" ?>
                    if (reader.ReadToFollowing("Deliverable"))
                    {
                        value = reader.GetAttribute(attributeName);
                    }
#if !PocketPC
                    Log.Trace(
                        "Retrieved .deliverable attribute: {0}. Value: {1}.", attributeName, value ?? "<no value>");
#endif
                    return;
                }
            }
        }
    }
}
