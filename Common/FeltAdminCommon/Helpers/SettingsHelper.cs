using FeltAdmin.Database.Engine;
using System.IO;
using System.Xml.Serialization;

namespace FeltAdmin.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FeltAdmin.Diagnostics;

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

        public static bool IsTemplateAvaliable()
        {
            try
            {
            var templatePath = DatabaseApi.GetActiveCompetition();
            if (string.IsNullOrEmpty(templatePath))
            {
                return false;
            }

            var templatedir = Directory.GetParent(templatePath);
            if (templatedir != null)
            {
                if (Directory.GetFiles(templatedir.FullName, "*.template").ToList().Count > 0)
                {
                    return true;
                }
            }
           
            return false;
            }
            catch (Exception e)
            {

                Log.Error(e, "IsTemplateAvaliable");
                return false;
            }
        }

        public static void SaveSettingsAsTemplate(string fileName,Settings settings)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                var ser = new XmlSerializer(typeof(Settings));
                using (var writer = new StreamWriter(fileName))
                {
                    ser.Serialize(writer, settings);
                }
            }
           
        }

        public static string GetTemplateDir()
        {
            var templatePath = DatabaseApi.GetActiveCompetition();
            if (string.IsNullOrEmpty(templatePath))
            {
                return string.Empty;
            }

            var templatedir = Directory.GetParent(templatePath);

            return templatedir.FullName;
        }

        public static IEnumerable<string> GetTemplatesNames()
        {
            List<string> Dirs = new List<string>();
            var dirName=GetTemplateDir();
            if (Directory.Exists(dirName))
            {
                var files=Directory.GetFiles(dirName, "*.template", SearchOption.TopDirectoryOnly);
                Dirs.AddRange(files);
            }

            return Dirs;
        }

        public static Settings GetTemplateSettings(string templateName)
        {
            string templateFileName = templateName + ".template";
            string templatedir = GetTemplateDir();
            var settingsfile = Path.Combine(templatedir, templateFileName);
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
}
