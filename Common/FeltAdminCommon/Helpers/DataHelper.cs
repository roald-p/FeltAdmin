using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using FeltAdmin.Viewmodels;

namespace FeltAdmin.Helpers
{
    public static class DataHelper
    {
        private static object syncLock = new object();

        private static Data s_data;

        public static Data GetData()
        {
            if (s_data != null)
            {
                return s_data;
            }

            lock (syncLock)
            {
                if (s_data != null)
                {
                    return s_data;
                }

                if (File.Exists("FeltData.xml"))
                {
                    var mySerializer = new XmlSerializer(typeof(Data));
                    using (var myFileStream = new FileStream("FeltData.xml", FileMode.Open))
                    {
                        s_data = (Data)mySerializer.Deserialize(myFileStream);
                    }

                    return s_data;
                }

                s_data = new Data();
                return s_data;
            }
        }

        public static void SaveData()
        {
            var ser = new XmlSerializer(typeof(Data));
            using (var writer = new StreamWriter("FeltData.xml"))
            {
                ser.Serialize(writer, s_data);
            }
        }
    }
}
