using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using LogRecorderAndPlayer.Common;
using LogRecorderAndPlayer.Data;

namespace LogRecorderAndPlayer.HTTP
{
    class LRAPHttpManager
    {
        public static bool ProcessLRAPFile(HttpContext context)
        {
            string filePath = context.Request.FilePath;

            if (filePath.ToLower() == "/logrecorderandplayerjs.lrap")
            {
                ResponseHelper.Write(context.Response, "text/javascript", ResourceHelper.GetResourceContent("LogRecorderAndPlayer.JS.LogRecorderAndPlayer.js"), new TimeSpan(1, 0, 0));
                return true;
            }
            
            if (filePath.ToLower() == "/logrecorderandplayerhandler.lrap")
            {
                try
                {
                    var logResponse = LoggingHelper.LogHandlerRequest(context.Request["request"]);
                    var logResponseJSON = JsonHelper.Serialize(logResponse);
                    context.Response.ContentType = "application/json";
                    context.Response.Write(logResponseJSON);
                }
                catch (Exception)
                {
                    context.Response.Status = "500 Internal Server Error";
                    context.Response.StatusCode = 500;
                    context.Response.StatusDescription = "Internal Server Error";
                }
                return true;
            }

            return false;
        }

        public static string InsertLRAPScript(string html, LRAPValues lrapValues)
        {
            var lrapPreScript = $"<script type=\"text/javascript\" src=\"/logrecorderandplayerjs.lrap?v={AssemblyHelper.RetrieveLinkerTimestamp().Ticks}\"></script><script type=\"text/javascript\">logRecorderAndPlayer.preInit(\"{lrapValues.SessionGUID}\", \"{lrapValues.PageGUID}\", \"{(lrapValues.ServerGUID != null ? lrapValues.ServerGUID.ToString() : "")}\");</script>";
            var newHTML = html.Insert(LoggingHelper.GetHtmlIndexForInsertingLRAPJS(html), lrapPreScript);
            var lrapPostScript = $"<script type=\"text/javascript\">logRecorderAndPlayer.postInit(\"{lrapValues.SessionGUID}\", \"{lrapValues.PageGUID}\", \"{(lrapValues.ServerGUID != null ? lrapValues.ServerGUID.ToString() : "")}\");</script>";
            newHTML += lrapPostScript;
            return newHTML;
        }
    }
}
