using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public static class AssemblyHelper
    {
        public static DateTime RetrieveLinkerTimestamp() //based on https://blog.codinghorror.com/determining-build-date-the-hard-way/
        {
            string filePath = Assembly.GetExecutingAssembly().Location;

            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            var b = new byte[2048];
            System.IO.FileStream s = null;
            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                    s.Close();
            }
            var dt = new System.DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(System.BitConverter.ToInt32(b, System.BitConverter.ToInt32(b, peHeaderOffset) + linkerTimestampOffset));
            return dt.AddHours(System.TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
        }
    }
}
