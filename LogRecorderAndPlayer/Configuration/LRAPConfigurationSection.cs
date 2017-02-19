using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public class LRAPConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("dude", IsRequired = true)]
        public string Dude
        {
            get
            {
                return (string)this["dude"];
            }
            set
            {
                this["dude"] = value;
            }
        }
    }
}
