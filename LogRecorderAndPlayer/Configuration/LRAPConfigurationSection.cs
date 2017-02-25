using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public enum LRAPConfigurationSectionLogType
    {
        CSV = 0,
        JSON = 1,
        DB = 2
    }

    public class LRAPConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("logType", IsRequired = true)]
        public LRAPConfigurationSectionLogType LogType
        {
            get
            {
                return (LRAPConfigurationSectionLogType)this["logType"];
            }
            set
            {                
                this["logType"] = value;
            }
        }

        [ConfigurationProperty("filePath", DefaultValue = null)]
        public string FilePath
        {
            get
            {
                var v = (string) this["filePath"];
                if (v == null)
                    throw new Exception("Invalid filepath");
                return v;
            }
            set
            {
                this["filePath"] = value;
            }
        }
    }
}
