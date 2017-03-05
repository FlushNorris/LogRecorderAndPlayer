using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LogRecorderAndPlayer
{
    public static class ResponseHelper
    {
        public static void Write(HttpResponse response, string contentType, string content, TimeSpan? cache = null)
        {
            response.Clear();
            response.ContentType = contentType;
            if (cache != null)
            {                 
                response.Cache.SetCacheability(HttpCacheability.Public);
                response.Cache.SetExpires(DateTime.Now.Add(cache.Value));
                response.Cache.SetMaxAge(cache.Value);
                response.AddHeader("Last-Modified", AssemblyHelper.RetrieveLinkerTimestamp().ToLongDateString());
            }
            response.Write(content);
        }
    }
}
