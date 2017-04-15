using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LogRecorderAndPlayer;

namespace LogThisWebApplication
{
    public class HandlerRequestData
    {
        public string SomeValue { get; set; }
    }

    public class HandlerResponseData
    {
        public string SomeValue { get; set; }
    }

    /// <summary>
    /// Summary description for TestHandler
    /// </summary>
    public class TestHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var request = SerializationHelper.Deserialize<HandlerRequestData>(context.Request["request"], SerializationType.Json);
            int someNumber;
            var response = new HandlerResponseData();
            if (int.TryParse(request.SomeValue, out someNumber))
                response.SomeValue = $"You wrote a number ({someNumber})";
            else
                response.SomeValue = $"You didn't write a number ({request.SomeValue})";

            context.Response.ContentType = "application/json";
            context.Response.Write(SerializationHelper.Serialize(response, SerializationType.Json));
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}