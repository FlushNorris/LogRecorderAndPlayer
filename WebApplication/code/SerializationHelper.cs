using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace WebApplication
{
    public enum SerializationType
    {
        Xml = 1,
        Json = 2
    }

    public static class SerializationHelper
    {
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

        public static string SerializeNameValueCollection(NameValueCollection nvc, SerializationType serializationType)
        {
            var dict = new Dictionary<string, object>();
            foreach (var key in nvc.AllKeys)
            {
                dict.Add(key, nvc[key]);
            }
            return Serialize(dict, serializationType);
        }    
    }
}
