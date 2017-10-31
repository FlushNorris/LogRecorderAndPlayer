using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LogRecorderAndPlayer.Common
{
    public class CollectionHelper
    {
        public static NameValueCollection DeserializeNameValueCollection(string data)
        {
            if (data == null)
                return null;

            var dict = JsonHelper.Deserialize<Dictionary<string, object>>(data);
            var nvc = new NameValueCollection();
            foreach (var key in dict.Keys)
            {
                nvc.Add(key, (string)dict[key]);
            }
            return nvc;
        }

        public static string SerializeNameValueCollection(NameValueCollection nvc)
        {
            if (nvc == null)
                return null;

            return JsonHelper.Serialize(NameValueCollectionToDictionary(nvc));
            //return new JavaScriptSerializer().Serialize(dict);
        }

        public static Dictionary<string, string> NameValueCollectionToDictionary(NameValueCollection nvc)
        {
            var dict = new Dictionary<string, string>();
            foreach (var key in nvc.AllKeys)
                dict.Add(key, nvc[key]);
            return dict;
        }

        public static Dictionary<string, string> HttpCookieCollectionToDictionary(HttpCookieCollection nvc)
        {
            var dict = new Dictionary<string, string>();
            foreach (var key in nvc.AllKeys)
                dict.Add(key, nvc[key].Value);
            return dict;
        }
    }
}
