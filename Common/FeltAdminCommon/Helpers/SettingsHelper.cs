using FeltAdmin.Database.Engine;
using System.IO;
using System.Xml.Serialization;

namespace FeltAdmin.Helpers
{
    public static class SettingsHelper
    {
        private static object syncLock = new object();

        ////private static Settings s_settings;

        public static Settings GetSettings()
        {
            var databasePath = DatabaseApi.GetActiveCompetition();
            ////if (s_settings != null)
            ////{
            ////    return s_settings;
            ////}

            lock (syncLock)
            {
                ////if (s_settings != null)
                ////{
                ////    return s_settings;
                ////}
                var settingsfile = Path.Combine(databasePath, "FeltAdmin.xml");
                if (File.Exists(settingsfile))
                {
                    var mySerializer = new XmlSerializer(typeof(Settings));
                    using (var myFileStream = new FileStream(settingsfile, FileMode.Open))
                    {
                        return (Settings)mySerializer.Deserialize(myFileStream);
                    }
                }

                return new Settings();
            }
        }

        public static void SaveSettings(Settings settings)
        {
            var databasePath = DatabaseApi.GetActiveCompetition();
            var settingsfile = Path.Combine(databasePath, "FeltAdmin.xml");
            var ser = new XmlSerializer(typeof(Settings));
            using (var writer = new StreamWriter(settingsfile))
            {
                ser.Serialize(writer, settings);
            }
        }
    }
}
