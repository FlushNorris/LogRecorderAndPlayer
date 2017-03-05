using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public static class TimeHelper
    {
        public static double UnixTimestamp(DateTime? dt = null) //Unix timestamp is seconds past epoch
        {
            if (dt == null)
                dt = DateTime.Now;
            
            return (double)(dt.Value - new DateTime(1970, 1, 1)).TotalMilliseconds / 1000.0;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
