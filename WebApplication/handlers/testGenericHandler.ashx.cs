using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;

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
    public class testGenericHandler : IHttpHandler //, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            var sb = new StringBuilder();

            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                foreach (string key in HttpContext.Current.Session.Keys)
                {
                    if (true) //key.IndexOf("HttpModuleTest_") == 0)
                    {
                        sb.AppendLine(key + " = " + HttpContext.Current.Session[key]);
                    }
                }

                HttpContext.Current.Session["HttpModuleTestHandler"] = "HttpModuleTestHandler";
            }
            else
            {
                sb.AppendLine("No session");
            }            

            var f = System.IO.File.CreateText($"c:\\HttpModuleTest\\{DateTime.Now.ToString("HHmmssfff")}_GenericHandlerSessionCheck.txt");
            f.Write(sb.ToString());
            f.Close();

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