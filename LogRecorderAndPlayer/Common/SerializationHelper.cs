using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace LogRecorderAndPlayer
{
    public enum SerializationType
    {
        Xml = 1,
        Json = 2
    }

    public static class SerializationHelper
    {
        public static object DeserializeByType(Type type, string content, SerializationType serializationType = SerializationType.Xml)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var serializer = GetSerializer(type, serializationType);
                return serializer.ReadObject(ms);
            }
        }

        public static T Deserialize<T>(string content, SerializationType serializationType = SerializationType.Xml)
        {
//            var obj = Activator.CreateInstance<T>();

            T obj = default(T);

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                var serializer = GetSerializer(typeof(T)/*obj.GetType()*/, serializationType);
                obj = (T)serializer.ReadObject(ms);
            }

            return obj;
        }

        public static string Serialize<T>(T obj, SerializationType serializationType = SerializationType.Xml)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            var serializer = GetSerializer(obj.GetType(), serializationType);
            string retVal;

            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                retVal = Encoding.UTF8.GetString(ms.ToArray());
            }

            return retVal;
        }        

        private static XmlObjectSerializer GetSerializer(Type type, SerializationType serializationType)
        {
            XmlObjectSerializer serializer;

            switch (serializationType)
            {
                case SerializationType.Xml:
                    serializer = new DataContractSerializer(type);
                    break;
                case SerializationType.Json:
                    serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(type);
                    break;
                default:
                    throw new ArgumentException("Cannot find XmlObjectSerializer for " + serializationType);
            }

            return serializer;
        }

        public static NameValueCollection DeserializeNameValueCollection(string data, SerializationType serializationType)
        {
            if (data == null)
                return null;

            var dict = Deserialize<Dictionary<string, object>>(data, serializationType);
            var nvc = new NameValueCollection();
            foreach (var key in dict.Keys)
            {
                nvc.Add(key, (string)dict[key]);
            }
            return nvc;
        }

        public static string SerializeNameValueCollection(NameValueCollection nvc, SerializationType serializationType)
        {
            if (nvc == null)
                return null;

            return Serialize(NameValueCollectionToDictionary(nvc), serializationType);
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

        //For Request.Params.. but we got way too much info for logging
        //private static string BuildASHXLoggingValue(NameValueCollection nvc)
        //{
        //    var dict = new Dictionary<string, object>();
        //    foreach (var key in nvc.AllKeys)
        //    {
        //        if (key == Consts.GUIDTag ||
        //            key == Consts.SessionGUIDTag ||
        //            key == Consts.PageGUIDTag ||
        //            key == Consts.BundleGUIDTag ||
        //            Consts.ForbiddenRequestParams.Contains(key))
        //            continue;

        //        dict.Add(key, nvc[key]);                
        //    }
        //    return new JavaScriptSerializer().Serialize(dict);
        //}        

    }
}
