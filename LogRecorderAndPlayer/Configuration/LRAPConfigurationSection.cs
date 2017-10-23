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
        Custom = 2
    }

    public class LRAPConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("enabled")]
        public bool Enabled //Used by HttpModule
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

        [ConfigurationProperty("logType")]
        public LRAPLogType LogType //Used by HttpModule
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
        public string FilePath //Used by HttpModule
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
        public string SolutionAssembly //Used by LogPlayer and HttpModule
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

        [ConfigurationProperty("sessionApp", DefaultValue = null)]
        public string SessionApp //Used by LogPlayer
        {
            get
            {
                var v = (string)this["sessionApp"];
                return v;
            }
            set
            {
                this["sessionApp"] = value;
            }
        }

        [ConfigurationProperty("logStackTrace", DefaultValue = false)]
        public bool LogStackTrace 
        {
            get
            {
                var v = (bool)this["logStackTrace"];
                return v;
            }
            set
            {
                this["logStackTrace"] = value;
            }
        }        
    }
}
