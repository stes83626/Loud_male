using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace update_space
{
    public class clsSetting
    {

        public class AppSettings
        {
            public int ID { get; set; }
        }

        public static AppSettings LoadSettings(string filePath)
        {
            var serializer = new XmlSerializer(typeof(AppSettings));
            using (var reader = new StreamReader(filePath))
            {
                return (AppSettings)serializer.Deserialize(reader);
            }
        }
    }
}
