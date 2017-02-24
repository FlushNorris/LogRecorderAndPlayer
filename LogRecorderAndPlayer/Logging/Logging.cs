using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public static class LoggingHelper
    {
        public static void LogHandlerRequest(string request)
        {
            SerializationHelper.Deserialize<Int32>(request, SerializationType.Json);
        }
    }
}
