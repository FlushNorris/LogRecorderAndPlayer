using System;
using System.Diagnostics;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
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
            context.PostMapRequestHandler += Context_PostMapRequestHandler;
            context.PreRequestHandlerExecute += Context_PreRequestHandlerExecute;
            context.PostRequestHandlerExecute += Context_PostRequestHandlerExecute; //læs session firsttime
            context.PostAcquireRequestState += Context_PostAcquireRequestState;


            //Ikke nødvendige på nuværende tidspunkt:
            //context.AcquireRequestState += Context_AcquireRequestState;
            //context.PostReleaseRequestState += Context_PostReleaseRequestState;
            //context.PostResolveRequestCache += Context_PostResolveRequestCache;
            //context.PostUpdateRequestCache += Context_PostUpdateRequestCache;
            //context.PreSendRequestHeaders += Context_PreSendRequestHeaders; //Was no commented earlier
            //context.PreSendRequestContent += Context_PreSendRequestContent; //Was no commented earlier
            //context.ReleaseRequestState += Context_ReleaseRequestState;
            //context.ResolveRequestCache += Context_ResolveRequestCache;
            //context.RequestCompleted += Context_RequestCompleted;
            //context.UpdateRequestCache += Context_UpdateRequestCache;
        }

        private void Context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication) sender;
            HttpContext context = app.Context;
            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            if (fileExtension != null && fileExtension.Equals(".aspx"))
            {
                var page = ((System.Web.UI.Page) context.CurrentHandler);
                LoggingHelper.SetupSession(context, page);
                LoggingHelper.SetupPage(context, page);

                LoggingPage.LogRequest(context, page);
            }
            if (fileExtension != null && fileExtension.Equals(".ashx"))
            {
                LoggingHandler.LogRequest(context);
            }

            //if (((HttpApplication)sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".aspx")
            //{
            //    try
            //    {
            //        if (app.Session != null)
            //        {
            //            app.Session["WHATTHEFUCK"] = "WOOHOOO";
            //        }
            //    }
            //    catch (System.Exception)
            //    {
            //    }

            //    if (context.Session != null)
            //    {
            //        context.Session["WHATTHEFUCK"] = "WOOHOOO";
            //    }
            //}
        }

        private void Context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            //SESSION KAN HENTES HER!!! :D
            //Og måske også sættes her... hmm
            //Skal være serializeable session value instanser, men det skal det jo alligevel for overhovedet at kunne være placeret i sessionen

            HttpApplication app = (HttpApplication) sender;

            LRAPHttpHandler resourceHttpHandler = HttpContext.Current.Handler as LRAPHttpHandler;

            if (resourceHttpHandler != null)
            {
                // set the original handler back
                HttpContext.Current.Handler = resourceHttpHandler.OriginalHandler;
            }

            //// -> at this point session state should be available

            //Debug.Assert(app.Session != null, "it did not work :(");

            ////Dette virkede helt fint, er bare kommenteret ud pga test
            ////app.Session["SomeSessionThingy"] = "Woohooo";

            //HttpApplication application = (HttpApplication)sender;
            //HttpContext context = application.Context;
            //string filePath = context.Request.FilePath;
            //string fileExtension =
            //    VirtualPathUtility.GetExtension(filePath);
            //if (fileExtension.Equals(".aspx"))
            //{
            //    //context.CurrentHandler      

            //    var page = ((System.Web.UI.Page)context.CurrentHandler);
            //    var pageType = page.GetType();

            //    //var viewStatePropertyDescriptor = pageType.GetProperty("ViewState", BindingFlags.Instance | BindingFlags.NonPublic);
            //    //var currentPageViewState = (StateBag)viewStatePropertyDescriptor.GetValue(HttpContext.Current.CurrentHandler);
            //    //currentPageViewState["WHAT"] = "ALTERED";

            //    //pageType.InvokeMember("SaveViewState", BindingFlags.InvokeMethod, null, page, null);

            //    //((System.Web.UI.Page) context.CurrentHandler).ViewState["WHAT"] = "ALTERED";

            //}
        }

        private void Context_PostResolveRequestCache(object sender, EventArgs e)
        {

        }

        private void Context_PostUpdateRequestCache(object sender, EventArgs e)
        {

        }

        private void Context_PreSendRequestContent(object sender, EventArgs e)
        {
            //HttpApplication app = (HttpApplication)sender;
            //HttpContext context = app.Context;
            //string filePath = context.Request.FilePath;
            //string fileExtension = VirtualPathUtility.GetExtension(filePath);

            //if (((HttpApplication)sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".aspx")
            //{
            //    try
            //    {
            //        if (app.Session != null)
            //        {
            //            app.Session["WHATTHEFUCK"] = "WOOHOOO";
            //        }
            //    }
            //    catch (System.Exception)
            //    {
            //    }

            //    if (context.Session != null)
            //    {
            //        context.Session["WHATTHEFUCK"] = "WOOHOOO";
            //    }
            //}
        }

        private void Context_PreSendRequestHeaders(object sender, EventArgs e)
        {
            //HttpApplication app = (HttpApplication)sender;
            //HttpContext context = app.Context;
            //string filePath = context.Request.FilePath;
            //string fileExtension = VirtualPathUtility.GetExtension(filePath);            

            //if (((HttpApplication)sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".aspx")
            //{
            //    try
            //    {
            //        if (app.Session != null)
            //        {
            //            app.Session["WHATTHEFUCK"] = "WOOHOOO";
            //        }
            //    }
            //    catch (System.Exception)
            //    {
            //    }

            //    if (context.Session != null)
            //    {
            //        context.Session["WHATTHEFUCK"] = "WOOHOOO";
            //    }
            //}

            //Bliver dette Response status overhovedet anvendt?
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
            HttpApplication app = (HttpApplication) sender;

            if (app.Context.Handler is IReadOnlySessionState || app.Context.Handler is IRequiresSessionState)
            {
                // no need to replace the current handler
                return;
            }

            // swap the current handler
            app.Context.Handler = new LRAPHttpHandler(app.Context.Handler);

            //try
            //{
            //    if (app.Session != null)
            //    {
            //        app.Session["WHATTHEFUCK"] = "WOOHOOO";
            //    }
            //}
            //catch (System.Exception)
            //{
            //}

            //var context = app.Context;
            //if (context.Session != null)
            //{
            //    context.Session["WHATTHEFUCK"] = "WOOHOOO";
            //}
        }

        private void Context_PostAcquireRequestState(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication) sender;

            LRAPHttpHandler resourceHttpHandler = HttpContext.Current.Handler as LRAPHttpHandler;

            if (resourceHttpHandler != null)
            {
                // set the original handler back
                HttpContext.Current.Handler = resourceHttpHandler.OriginalHandler;
            }

            //try
            //{
            //    if (app.Session != null)
            //    {
            //        app.Session["WHATTHEFUCK"] = "WOOHOOO";
            //    }
            //}
            //catch (System.Exception)
            //{
            //}

            //if (((HttpApplication) sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".aspx")
            //{
            //    var context = app.Context;
            //    if (context.Session != null)
            //    {
            //        context.Session["WHATTHEFUCK"] = "WOOHOOO";
            //    }
            //}


            // -> at this point session state should be available

            Debug.Assert(app.Session != null, "it did not work :(");
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            if (((HttpApplication) sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".axd")
            {
                return;
            }

            watcher = new StreamWatcher(this.context.Response.Filter); //Man in the middle... alike
            this.context.Response.Filter = watcher;

            HttpApplication app = (HttpApplication) sender;
            HttpContext context = app.Context;
            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            //if (filePath.ToLower() == "/logrecorderandplayerhandler.lrap")
            //{
            //    LoggingHelper.LogHandlerRequest(context.Request["request"]);                
            //    return;
            //}

            //try
            //{
            //    if (app.Session != null)
            //    {
            //        app.Session["WHATTHEFUCK"] = "WOOHOOO";
            //    }
            //}
            //catch (System.Exception)
            //{                
            //}

            //if (context.Session != null)
            //{
            //    context.Session["WHATTHEFUCK"] = "WOOHOOO";
            //}
        }

        private void Context_EndRequest(object sender, EventArgs e)
        {
            if (((HttpApplication) sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".axd")
            {
                return;
            }

            //TODO Look into this dynamic assembly.. it works, but is not in use atm
            //var weee = DynamicAssembly.LoadAssemblyInstances<ILogRecorderAndPlayer>("DynAssembly.dll").FirstOrDefault();
            //var ost = weee.DoStuff(6, 10);

            //var ccc = ConfigurationHelper.GetConfigurationSection();

            string response = watcher.ToString();

            this.context.Response.Filter = null;

            HttpApplication application = (HttpApplication) sender;
            HttpContext context = application.Context;
            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            if (filePath.ToLower() == "/logrecorderandplayerjs.lrap")
            {
                //context.Response.Status = "304 NOT MODIFIED";
                //context.Response.StatusCode = 304;
                //context.Response.StatusDescription = "NOT MODIFIED";
                ResponseHelper.Write(context.Response, "text/javascript", ResourceHelper.GetResourceContent("LogRecorderAndPlayer.JS.LogRecorderAndPlayer.js"), new TimeSpan(1, 0, 0));
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

            if (filePath.ToLower() == "/logrecorderandplayerhandler.lrap")
            {
                try
                {
                    var logResponse = LoggingHelper.LogHandlerRequest(context.Request["request"]);
                    var logResponseJSON = SerializationHelper.Serialize(logResponse, SerializationType.Json);
                    context.Response.ContentType = "application/json";
                    context.Response.Write(logResponseJSON);
                }
                catch (Exception ex)
                {
                    context.Response.Status = "500 Internal Server Error";
                    context.Response.StatusCode = 500;
                    context.Response.StatusDescription = "Internal Server Error";
                }
                
                return;
            }

            if (fileExtension.ToLower().Equals(".aspx"))
            {
                //context.CurrentHandler      
                var page = ((System.Web.UI.Page)context.CurrentHandler);

                if (page != null)
                {
                    //LoggingHelper.SetupPage(context, page); //Moved to Context_PreRequestHandlerExecute


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

                    //if (context.Session != null)
                    //{
                    //    context.Session["WHATTHEFUCK"] = "WOOHOOO22222222222";
                    //}

                    //if (page.Session != null)
                    //{
                    //    page.Session["WHATTHEFUCK"] = "WOOHOOO2222222222222";
                    //}

                    var sessionGUID = LoggingHelper.GetSessionGUID(context, page);
                    var pageGUID = LoggingHelper.GetPageGUID(context, page);
                    
                    LoggingPage.LogResponse(context, page, response);


                    //context.Response.Write("<script/>");
                    
                    string lrapScript = $"<script type=\"text/javascript\" src=\"/logrecorderandplayerjs.lrap\"></script><script type=\"text/javascript\">logRecorderAndPlayer.init(\"{sessionGUID}\", \"{pageGUID}\");</script>";

                    var newResponse = response.Insert(LoggingHelper.GetHtmlIndexForInsertingLRAPJS(response), lrapScript);
                    context.Response.Clear();
                    context.Response.Write(newResponse);
                    //context.Response.Write(response.Replace("beregninger.js", "beregningerXXX.js"));

//                    context.Response.Write("<hr><h1><font color=red>" + "HelloWorldModuleXXXARGH2: End of Request</font></h1>");                    
                    //context.Response.Write("<script type=\"text/javascript\" src=\"/logrecorderandplayerjs.lrap\"></script>");
                    //context.Response.Write($"<script type=\"text/javascript\">logRecorderAndPlayer.init(\"{sessionGUID}\", \"{pageGUID}\");</script>");                    
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
            if (fileExtension.ToLower().Equals(".ashx"))
            {
                LoggingHandler.LogResponse(context, response);                
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
