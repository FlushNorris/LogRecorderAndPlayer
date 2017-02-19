using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public static class ConfigurationHelper
    {
        public static LRAPConfigurationSection GetConfigurationSection()
        {
            return System.Configuration.ConfigurationManager.GetSection("LRAPConfigurationSection") as LRAPConfigurationSection;
        }
    }
}
