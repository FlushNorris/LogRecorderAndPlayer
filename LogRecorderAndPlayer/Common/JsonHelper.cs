using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LogRecorderAndPlayer.Common
{
    public enum JsonHelperJavascriptSerializer
    {
        Newtonsoft = 0,
        [Obsolete("Should not be used for future development", true)] JavascriptSerializer = 2,
        JsonDotNet = 3
    }

    public static class JsonHelper
    {
        public static string Serialize<T>(T obj, JsonHelperJavascriptSerializer serializerType = JsonHelperJavascriptSerializer.Newtonsoft)
        {
            string retVal;
            switch (serializerType)
            {
                case JsonHelperJavascriptSerializer.JsonDotNet:
                    var settings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.None,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };
                    retVal = JsonConvert.SerializeObject(obj, settings);
                    break;
                case JsonHelperJavascriptSerializer.Newtonsoft:
                    retVal = JsonConvert.SerializeObject(obj, GetNewtonsoftSerializerSettings());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serializerType), serializerType, null);
            }
            return retVal;
        }

        public static T Deserialize<T>(string json)
        {
            // We're using Newtonsoft for deserialization because it provides superior 
            // control and information in case of deserialization errors.
            return JsonConvert.DeserializeObject<T>(json, GetNewtonsoftSerializerSettings());
        }

        public static object DeserializeByType(Type type, string content)
        {
            return JsonConvert.DeserializeObject(content, type);
        }

        private static JsonSerializerSettings GetNewtonsoftSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
                // We need to use the MicrosoftDateFormat to be compatible with the Microsoft serializer.
                // See http://www.newtonsoft.com/json/help/html/SerializeDateFormatHandling.htm
                DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
            };
        }
    }
}