using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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

        public static void SetNow(HttpContext context, DateTime dt)
        {
            if (context == null)
                return;

            if (context.Session != null)
            {
                context.Session[Consts.NowSetTimestampTag] = TimeHelper.UnixTimestamp(DateTime.Now).ToString(CultureInfo.InvariantCulture);
                context.Session[Consts.NowTimestampTag] = TimeHelper.UnixTimestamp(dt).ToString(CultureInfo.InvariantCulture);
            }

            if (context.Request?.Headers != null)
            {
                context.Request.Headers[Consts.NowSetTimestampTag] = TimeHelper.UnixTimestamp(DateTime.Now).ToString(CultureInfo.InvariantCulture);
                context.Request.Headers[Consts.NowTimestampTag] = TimeHelper.UnixTimestamp(dt).ToString(CultureInfo.InvariantCulture);
            }
        }

        public static DateTime Now(HttpContext context = null)
        {
            context = context ?? HttpContext.Current;

            if (!LoggingHelper.IsPlaying(context, null))
                return DateTime.Now;

            DateTime? nowTimestamp = null;
            DateTime? nowSetTimestamp = null;

            if (context.Session != null)
            {
                nowTimestamp = TimeHelper.UnixTimeStampToDateTime(Double.Parse(context.Session[Consts.NowTimestampTag] as string, CultureInfo.InvariantCulture));
                nowSetTimestamp = TimeHelper.UnixTimeStampToDateTime(Double.Parse(context.Session[Consts.NowSetTimestampTag] as string, CultureInfo.InvariantCulture));
            }
            else if (context.Request?.Headers != null)
            {
                nowTimestamp = TimeHelper.UnixTimeStampToDateTime(Double.Parse(context.Request.Headers[Consts.NowTimestampTag] as string, CultureInfo.InvariantCulture));
                nowSetTimestamp = TimeHelper.UnixTimeStampToDateTime(Double.Parse(context.Request.Headers[Consts.NowSetTimestampTag] as string, CultureInfo.InvariantCulture));
            }
            
            if (nowSetTimestamp == null || nowTimestamp == null)
                throw new Exception("Failed to fetch LRAP-Now");

            return nowTimestamp.Value + (DateTime.Now - nowSetTimestamp.Value);
        }
    }
}
