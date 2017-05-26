using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public static class Consts
    {
        public static readonly string GUIDTag = "lrap-guid";
        public static readonly string SessionGUIDTag = "lrap-sessionguid";
        public static readonly string PageGUIDTag = "lrap-pageguid";
        public static readonly string BundleGUIDTag = "lrap-bundleguid";
        public static readonly string ServerGUIDTag = "lrap-serverguid"; //For the namedpipe-connection to the LogPlayer
        public static readonly string NowTimestampTag = "lrap-nowtimestamp";
        public static readonly string NowSetTimestampTag = "lrap-nowsettimestamp";

        public static readonly string[] LRAPFormFields = new string[]
        {
            GUIDTag,
            SessionGUIDTag,
            PageGUIDTag,
            BundleGUIDTag,
            ServerGUIDTag,
            NowTimestampTag,
            NowSetTimestampTag
        };

        public static readonly string[] ViewStateFormFields = new string[]
        {
            "__VIEWSTATE",
            "__VIEWSTATEGENERATOR",
            "__EVENTVALIDATION"
        };        
    }
}
