using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication.handlers
{
    public class GenericHandlerRequest
    {
        public string SomeText { get; set; }
        public int SomeValue { get; set; }
    }

    public class GenericHandlerResponse
    {
        public int SomeRandomInt { get; set; }
        public DateTime SomeTimestamp { get; set; }
    }

    /// <summary>
    /// Summary description for testHandler
    /// </summary>
    public class testGenericHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var request = SerializationHelper.Deserialize<GenericHandlerRequest>(context.Request["request"], SerializationType.Json);

            var rand = new Random((int)DateTime.Now.Ticks);

            var response = new GenericHandlerResponse()
            {
                SomeRandomInt = rand.Next(request.SomeValue) + 1, //1..SomeValue
                SomeTimestamp = DateTime.Now
            };

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