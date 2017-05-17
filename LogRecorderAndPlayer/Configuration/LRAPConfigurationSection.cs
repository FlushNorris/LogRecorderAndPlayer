using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public enum LRAPLogType
    {
        CSV = 0,
        JSON = 1,
        DB = 2
    }

    public class LRAPConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("enabled", IsRequired = true)]
        public bool Enabled
        {
            get
            {
                return (bool)this["enabled"];
            }
            set
            {
                this["enabled"] = value;
            }
        }        

        [ConfigurationProperty("logType", IsRequired = true)]
        public LRAPLogType LogType
        {
            get
            {
                return (LRAPLogType)this["logType"];
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

        [ConfigurationProperty("solutionAssembly", DefaultValue = null)]
        public string SolutionAssembly
        {
            get
            {
                var v = (string)this["solutionAssembly"];
                return v;
            }
            set
            {
                this["solutionAssembly"] = value;
            }
        }

    }
}
