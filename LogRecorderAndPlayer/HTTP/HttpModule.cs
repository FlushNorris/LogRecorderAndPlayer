using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using LogRecorderAndPlayer.Common;

namespace LogRecorderAndPlayer
{
    public class LRAPHttpModule : IHttpModule
    {
        private StreamWatcher watcher;
        private HttpApplication context;

        public void Init(HttpApplication context)
        {
            this.context = context;
            context.BeginRequest += Context_BeginRequest;
            context.EndRequest += Context_EndRequest;

            context.AcquireRequestState += Context_AcquireRequestState;
            context.PostAcquireRequestState += Context_PostAcquireRequestState;
            context.PostMapRequestHandler += Context_PostMapRequestHandler;
            context.PostReleaseRequestState += Context_PostReleaseRequestState;
            context.PostRequestHandlerExecute += Context_PostRequestHandlerExecute; //læs session firsttime
            context.PostResolveRequestCache += Context_PostResolveRequestCache;
            context.PostUpdateRequestCache += Context_PostUpdateRequestCache;
            context.PreRequestHandlerExecute += Context_PreRequestHandlerExecute;
            context.PreSendRequestHeaders += Context_PreSendRequestHeaders;
            context.PreSendRequestContent += Context_PreSendRequestContent;
            context.ReleaseRequestState += Context_ReleaseRequestState;
            context.ResolveRequestCache += Context_ResolveRequestCache;
            context.RequestCompleted += Context_RequestCompleted;
            context.UpdateRequestCache += Context_UpdateRequestCache;
        }

        private void Context_ReleaseRequestState1(object sender, EventArgs e)
        {

        }

        private void Context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;
            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            if (fileExtension != null && fileExtension.Equals(".aspx"))
            {
                var page = ((System.Web.UI.Page)context.CurrentHandler);
                LoggingHelper.SetupSession(context, page);               

                //page.ClientScript.RegisterClientScriptBlock(page.GetType(), "LogRecorderAndPlayerScript", "<script type=\"text/javascript\" src=\"/logrecorderandplayerjs.lrap\"></script>");
                ////New GUID generated on every request, but clientside (setPageGUID) ignores all except the first delivered
                //page.ClientScript.RegisterClientScriptBlock(page.GetType(), "LogRecorderAndPlayerSessionID", $"<script type=\"text/javascript\">logRecorderAndPlayer.setPageGUID(\"{LoggingHelper.GetPageGUID(page)}\");</script>");                
            }
        }

        private void Context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            //SESSION KAN HENTES HER!!! :D
            //Og måske også sættes her... hmm
            //Skal være serializeable session value instanser, men det skal det jo alligevel for overhovedet at kunne være placeret i sessionen

            HttpApplication app = (HttpApplication)sender;            

            LRAPHttpHandler resourceHttpHandler = HttpContext.Current.Handler as LRAPHttpHandler;

            if (resourceHttpHandler != null)
            {
                // set the original handler back
                HttpContext.Current.Handler = resourceHttpHandler.OriginalHandler;
            }

            // -> at this point session state should be available

            Debug.Assert(app.Session != null, "it did not work :(");

            //Dette virkede helt fint, er bare kommenteret ud pga test
            //app.Session["SomeSessionThingy"] = "Woohooo";

            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;
            string filePath = context.Request.FilePath;
            string fileExtension =
                VirtualPathUtility.GetExtension(filePath);
            if (fileExtension.Equals(".aspx"))
            {
                //context.CurrentHandler      

                var page = ((System.Web.UI.Page)context.CurrentHandler);
                var pageType = page.GetType();

                //var viewStatePropertyDescriptor = pageType.GetProperty("ViewState", BindingFlags.Instance | BindingFlags.NonPublic);
                //var currentPageViewState = (StateBag)viewStatePropertyDescriptor.GetValue(HttpContext.Current.CurrentHandler);
                //currentPageViewState["WHAT"] = "ALTERED";

                //pageType.InvokeMember("SaveViewState", BindingFlags.InvokeMethod, null, page, null);

                //((System.Web.UI.Page) context.CurrentHandler).ViewState["WHAT"] = "ALTERED";

            }
        }

        private void Context_PostResolveRequestCache(object sender, EventArgs e)
        {

        }

        private void Context_PostUpdateRequestCache(object sender, EventArgs e)
        {

        }

        private void Context_PreSendRequestContent(object sender, EventArgs e)
        {            
        }

        private void Context_PreSendRequestHeaders(object sender, EventArgs e)
        {
            context.Response.Status = "200 OK";
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            //context.Response.HeadersWritten = true;
        }

        private void Context_RequestCompleted(object sender, EventArgs e)
        {
        }

        private void Context_ResolveRequestCache(object sender, EventArgs e)
        {
        }

        private void Context_UpdateRequestCache(object sender, EventArgs e)
        {
        }

        private void Context_ReleaseRequestState(object sender, EventArgs e)
        {
        }

        private void Context_PostReleaseRequestState(object sender, EventArgs e)
        {
        }

        private void Context_AcquireRequestState(object sender, EventArgs e)
        {
        }

        private void Context_PostMapRequestHandler(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            if (app.Context.Handler is IReadOnlySessionState || app.Context.Handler is IRequiresSessionState)
            {
                // no need to replace the current handler
                return;
            }

            // swap the current handler
            app.Context.Handler = new LRAPHttpHandler(app.Context.Handler);
        }

        private void Context_PostAcquireRequestState(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            LRAPHttpHandler resourceHttpHandler = HttpContext.Current.Handler as LRAPHttpHandler;

            if (resourceHttpHandler != null)
            {
                // set the original handler back
                HttpContext.Current.Handler = resourceHttpHandler.OriginalHandler;
            }

            

            // -> at this point session state should be available

            Debug.Assert(app.Session != null, "it did not work :(");
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            if (((HttpApplication) sender).Context.Request.CurrentExecutionFilePathExtension == ".axd")
            {
                return;
            }                

            watcher = new StreamWatcher(this.context.Response.Filter); //Man in the middle... alike
            this.context.Response.Filter = watcher;

            HttpApplication application = (HttpApplication) sender;
            HttpContext context = application.Context;
            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            if (filePath.ToLower() == "/logrecorderandplayerhandler.lrap")
            {
                LoggingHelper.LogHandlerRequest(context.Request["request"]);                
                return;
            }

            if (fileExtension.Equals(".aspx"))
            {
                //context.Response.Write("<h1><font color=red>" +
                //    "HelloWorldModuleXXX: Beginning of Request" +
                //    "</font></h1><hr>");

                var page = ((System.Web.UI.Page)context.CurrentHandler);
                if (page != null)
                {
                    var pageType = page.GetType();

                    var viewStatePropertyDescriptor = pageType.GetProperty("ViewState", BindingFlags.Instance | BindingFlags.NonPublic);
                    var currentPageViewState = (StateBag)viewStatePropertyDescriptor.GetValue(HttpContext.Current.CurrentHandler);
                    //currentPageViewState["WHAT"] = "ALTERED";
                }
            }            
        }

        private void Context_EndRequest(object sender, EventArgs e)
        {
            if (((HttpApplication)sender).Context.Request.CurrentExecutionFilePathExtension == ".axd")
            {
                return;
            }

            var weee = DynamicAssembly.LoadAssemblyInstances<ILogRecorderAndPlayer>("DynAssembly.dll").FirstOrDefault();
            var ost = weee.DoStuff(6, 10);

            //instantiatedTypes[0].DoStuff()

            //var logRecorderAndPlayer = ass.CreateInstance("DynAssembly.ClassDyn") as ILogRecorderAndPlayer;

            //System.Configuration

            var ccc = ConfigurationHelper.GetConfigurationSection();

            string value = watcher.ToString();

            this.context.Response.Filter = null;

            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;
            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            if (filePath.ToLower() == "/logrecorderandplayerjs.lrap")
            {
                context.Response.Clear();
                context.Response.Write(ResourceHelper.GetResourceContent("LogRecorderAndPlayer.JS.LogRecorderAndPlayer.js"));
                return;
            }

            //if (filePath.ToLower() == "/logrecorderandplayer.aspx")
            //{
            //    context.Response.Clear();
            //    context.Response.Write(ResourceHelper.GetResourceContent("LogRecorderAndPlayer.JS.LogRecorderAndPlayer.js"));

            //    context.Response.Status = "200 OK";
            //    context.Response.StatusCode = 200;
            //    context.Response.StatusDescription = "OK";
            //}

            //if (filePath.ToLower() == "/logrecorderandplayerjs.aspx")
            //{
            //    context.Response.Clear();
            //    context.Response.Write(ResourceHelper.GetResourceContent("LogRecorderAndPlayer.JS.LogRecorderAndPlayer.js"));
            //    //context.Response.Status = "200 OK";
            //    //context.Response.StatusCode = 200;
            //    //context.Response.StatusDescription = "OK";
            //    return;
            //}


            if (fileExtension.Equals(".aspx"))
            {
                //context.CurrentHandler      
                var page = ((System.Web.UI.Page)context.CurrentHandler);

                if (page != null)
                {
                    LoggingHelper.SetupPage(context, page);
                    //context.Session is null in Context_EndRequest, and sometimes I got an exception for accessing the page.Session as well... 
                    //hmm, but not not anymore? If it continues to fails, split this method up and write the session value in the temporary viewstate in Context_PreRequestHandlerExecute

                    //page.ClientScript.RegisterClientScriptBlock(page.GetType(), "LogRecorderAndPlayerScript", "<script type=\"text/javascript\" src=\"/logrecorderandplayerjs.lrap\"></script>");
                    //New GUID generated on every request, but clientside (setPageGUID) ignores all except the first delivered
                    //page.ClientScript.RegisterClientScriptBlock(page.GetType(), "LogRecorderAndPlayerSessionID", $"<script type=\"text/javascript\">logRecorderAndPlayer.setPageGUID(\"{LoggingHelper.GetPageGUID(page)}\");</script>");


                    //var vvv = LoggingHelper.GetPageGUID(page);
                    
                    //var pageType = page.GetType();

                    //var viewStatePropertyDescriptor = pageType.GetProperty("ViewState", BindingFlags.Instance | BindingFlags.NonPublic);
                    //var currentPageViewState = (StateBag)viewStatePropertyDescriptor.GetValue(HttpContext.Current.CurrentHandler);
                    //currentPageViewState["WHAT"] = "ALTERED";

                    //((System.Web.UI.Page) context.CurrentHandler).ViewState["WHAT"] = "ALTERED";
                    context.Response.Write("<hr><h1><font color=red>" + "HelloWorldModuleXXXARGH2: End of Request</font></h1>");

                    context.Response.Write("<script type=\"text/javascript\" src=\"/logrecorderandplayerjs.lrap\"></script>");
                    var sessionGUID = LoggingHelper.GetSessionGUID(page);
                    var pageGUID = LoggingHelper.GetPageGUID(context, page);
                    context.Response.Write($"<script type=\"text/javascript\">logRecorderAndPlayer.setPageGUID(\"{pageGUID}\");</script>");
                }
                else
                {
                    //var thingy = new LoggingThingy() { Id = 0, Title = "Some title", Timestamp = DateTime.Now };
                    context.Response.Clear();
                    context.Response.Write("OK"); //SerializationHelper.Serialize(thingy, SerializationType.Json));

                    //context.Response.Status = "200 OK";
                    //context.Response.StatusCode = 200;
                    //context.Response.StatusDescription = "OK";

                }
            }

            //if (fileExtension.Equals(".ashx"))
            //{
            //    context.Response.Clear();
            //    context.Response.Write("OK"); //SerializationHelper.Serialize(thingy, SerializationType.Json));

            //    //context.Response.Status = "200 OK";
            //    //context.Response.StatusCode = 200;
            //    //context.Response.StatusDescription = "OK";
            //}

            if (false) //value.IndexOf("Wee") != -1)
            {
                context.Response.Clear();
                context.Response.Write(@"<h1><font color=red>HelloWorldModuleXXX: Beginning of Request</font></h1><hr>

<!DOCTYPE html>

<html xmlns=""http://www.w3.org/1999/xhtml"">
<head><title>

</title></head>
<body>
    <form method=""post"" action=""./WebForm1.aspx"" id=""form1"">
<div class=""aspNetHidden"">
<input type=""hidden"" name=""__VIEWSTATE"" id=""__VIEWSTATE"" value=""hFPq3fmRvjnDCArJOxqmP3UrAjNfEY4esSsXz7gMv81pj7mSUjSsbhXTGmc1//3/RKQOkHN9H+oAIWxhwAfywmlBIr5UBZ6OrPwp/VioRCpcGmGuQj9Z0p5ZuiEixxk6"" />
</div>

<div class=""aspNetHidden"">

	<input type=""hidden"" name=""__VIEWSTATEGENERATOR"" id=""__VIEWSTATEGENERATOR"" value=""B6E7D48B"" />
	<input type=""hidden"" name=""__EVENTVALIDATION"" id=""__EVENTVALIDATION"" value=""dJRkr8GSn7xq3HZARTcN/jGZEYAJj0sM3XZFLOZYnEnjCuZPYAX7VMzRSLoupjUn2OZvWdmGva0RuxMQYH/zHclAqhlzt2erxe9L3WXsMxv3+/JBrjOPmAOcCm8iT5cp"" />
</div>
    <div>
        <input type=""submit"" name=""someBtn"" value=""Weee222"" id=""someBtn"" />
    </div>
    </form>
</body>
</html>");
            }
        }

        public void Dispose()
        {
        }
    }
}
