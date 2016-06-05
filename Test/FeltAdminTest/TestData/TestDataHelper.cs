using System.Xml.Serialization;
using FeltAdmin.Helpers;
using System.IO;
using System.Reflection;

namespace FeltAdminTest.TestData
{
    public class TestDataHelper
    {
        public static Settings GetSettings()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "FeltAdminTest.TestData.FeltAdmin.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var mySerializer = new XmlSerializer(typeof(Settings));
                    return (Settings)mySerializer.Deserialize(reader);
                }
            }
        }
    }
}
