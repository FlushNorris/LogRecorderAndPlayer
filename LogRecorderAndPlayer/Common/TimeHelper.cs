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
            dtDateTime = dtDateTime.AddSeconds(Math.Floor(unixTimeStamp)); //.ToLocalTime();                
                                                                           //dtDateTime = dtDateTime.AddMilliseconds(123.4567); //anything below ms is ignored
            var secs = unixTimeStamp % 1;
            var ms = secs * 1000;
            var ns = ms * 1000;
            var ps100 = ns * 10;
            dtDateTime = dtDateTime.AddTicks((long)ps100);

            //dtDateTime = new DateTime(dtDateTime.Ticks + (long)ps100);

            //                var dtDateTime = new DateTime((long)(unixTimeStamp*1000/*seconds_to_ms*/*1000000/*ms_to_ns*/)); //, DateTimeKind.Utc);
            return dtDateTime;

            //var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            //dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            //return dtDateTime;
        }
    }

    public class DateTimeLRAP 
    {
        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }
    }
}
