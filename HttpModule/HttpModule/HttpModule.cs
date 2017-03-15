using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;

namespace HttpModule
{
    public class HttpModule : IHttpModule
    {
        private StreamWatcher watcher;
        private HttpApplication context;

        public void Init(HttpApplication context)
        {
            this.context = context;
            context.BeginRequest += Context_BeginRequest;
            context.AuthenticateRequest += Context_AuthenticateRequest;
            context.PostAuthenticateRequest += Context_PostAuthenticateRequest;
            context.AuthorizeRequest += Context_AuthorizeRequest;
            context.PostAuthorizeRequest += Context_PostAuthorizeRequest;
            context.ResolveRequestCache += Context_ResolveRequestCache;
            context.PostResolveRequestCache += Context_PostResolveRequestCache;
            context.AcquireRequestState += Context_AcquireRequestState;
            context.PostAcquireRequestState += Context_PostAcquireRequestState;
            context.PreRequestHandlerExecute += Context_PreRequestHandlerExecute;

            /////

            context.Disposed += Context_Disposed;
            context.EndRequest += Context_EndRequest;
            context.Error += Context_Error;
            context.LogRequest += Context_LogRequest;

            context.MapRequestHandler += Context_MapRequestHandler;
            context.PostLogRequest += Context_PostLogRequest;
            context.PostMapRequestHandler += Context_PostMapRequestHandler; //
            context.PostReleaseRequestState += Context_PostReleaseRequestState;

            context.PostRequestHandlerExecute += Context_PostRequestHandlerExecute; //læs session firsttime
            context.PostUpdateRequestCache += Context_PostUpdateRequestCache;
            context.PreSendRequestContent += Context_PreSendRequestContent; //Was no commented earlier
            context.PreSendRequestHeaders += Context_PreSendRequestHeaders; //Was no commented earlier
            context.ReleaseRequestState += Context_ReleaseRequestState;
            context.RequestCompleted += Context_RequestCompleted;
            context.UpdateRequestCache += Context_UpdateRequestCache;
        }

        private void Context_PostLogRequest(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_PostLogRequest", watcher);
        }

        private void Context_PostAuthorizeRequest(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_PostAuthorizeRequest", watcher);
        }

        private void Context_PostAuthenticateRequest(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_PostAuthenticateRequest", watcher);
        }

        private void Context_MapRequestHandler(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_MapRequestHandler", watcher);
        }

        private void Context_LogRequest(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_LogRequest", watcher);
        }

        private void Context_Error(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_Error", watcher);
        }

        private void Context_Disposed(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_Disposed", watcher);
        }

        private void Context_AuthorizeRequest(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_AuthorizeRequest", watcher);
        }

        private void Context_AuthenticateRequest(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_AuthenticateRequest", watcher);
        }

        public void Dispose()
        {
        }

        private void Log(string name, HttpContext context, Page page, StreamWatcher watcher)
        {
            var f = System.IO.File.CreateText($"c:\\HttpModuleTest\\{DateTime.Now.ToString("HHmmssfff")}_{name}.txt");
            f.Write(BuildLogText(name, context, page, watcher));
            f.Close();
        }

        private void LogPageEvent(string name, Page page)
        {
            var f = System.IO.File.CreateText($"c:\\HttpModuleTest\\{DateTime.Now.ToString("HHmmssfff")}_PageEvent_{name}.txt");
            f.Write(TestViewState(page, name));
            f.Close();
        }

        private string BuildLogText(string name, HttpContext context, Page page, StreamWatcher watcher)
        {
            var sb = new StringBuilder();
            try
            {
                sb.AppendLine($"context: {context != null}");
                if (context != null)
                {
                    sb.AppendLine($"context.Session: {context.Session != null}");
                    if (context.Session != null)
                    {
                        sb.AppendLine($"context.Session[\"HttpModuleTest\"]: {(context.Session["HttpModuleTest"] ?? "null")}");
                        var value = $"{name}{DateTime.Now.ToString("HH:mm:ss:fff")}";
                        sb.AppendLine($"Setting context.Session[\"{value}\"] = \"{value}\"");
                        context.Session[value] = value;
                        sb.AppendLine($"Done setting context.Session[\"{value}\"] = \"{value}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"context: Error");
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.StackTrace);
            }
            sb.AppendLine($"page: {page != null}");
            try
            {
                if (page != null && page.IsValid)
                {
                    try
                    {

                        sb.AppendLine($"page.Session: {page.Session != null}");
                        if (page.Session != null)
                        {
                            sb.AppendLine($"page.Session[\"HttpModuleTest\"]: {(page.Session["HttpModuleTest"] ?? "null")}");
                            var value = $"{name}{DateTime.Now.ToString("HH:mm:ss:fff")}";
                            sb.AppendLine($"Setting page.Session[\"{value}\"] = \"{value}\"");
                            page.Session[value] = value;
                            sb.AppendLine($"Done setting page.Session[\"{value}\"] = \"{value}\"");
                        }
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"Page-Session: Error");
                        sb.AppendLine(ex.Message);
                        sb.AppendLine(ex.StackTrace);
                    }
                }
            }
            catch (Exception ex2)
            {
                sb.AppendLine("Page: Error");
                sb.AppendLine(ex2.Message);
                sb.AppendLine(ex2.StackTrace);
            }

            if (page != null)
            {
                SetupPageEvent(page, "InitComplete");
                SetupPageEvent(page, "LoadComplete");
                SetupPageEvent(page, "PreInit");
                SetupPageEvent(page, "PreLoad");
                SetupPageEvent(page, "PreRenderComplete");
                SetupPageEvent(page, "SaveStateComplete");
                SetupPageEvent(page, "DataBinding");
                SetupPageEvent(page, "Disposed");
                SetupPageEvent(page, "Init");
                SetupPageEvent(page, "Load");
                SetupPageEvent(page, "PreRender");
                SetupPageEvent(page, "Unload");
                SetupPageEvent(page, "AbortTransaction");
                SetupPageEvent(page, "CommitTransaction");
                SetupPageEvent(page, "Error");
            }
            if (page != null)
            {
                sb.Append(TestViewState(page, name));
            }

            if (watcher != null)
            {
                try
                {
                    sb.AppendLine($"watcher.response.length = {watcher.ToString().Trim().Length}");
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"watcher: Error");
                    sb.AppendLine(ex.Message);
                    sb.AppendLine(ex.StackTrace);
                }
            }

            return sb.ToString();
        }

        public string TestViewState(Page page, string name)
        {
            var sb = new StringBuilder();
            try
            {
                var pageType = page.GetType();
                var viewStatePropertyDescriptor = pageType.GetProperty("ViewState", BindingFlags.Instance | BindingFlags.NonPublic);
                var currentPageViewState = (StateBag) viewStatePropertyDescriptor.GetValue(HttpContext.Current.CurrentHandler);
                if (currentPageViewState != null)
                {
                    sb.AppendLine($"currentPageViewState[\"HttpModuleTest\"]: {(currentPageViewState["HttpModuleTest"] ?? "null")}");
                    var value = $"{name}{DateTime.Now.ToString("HH:mm:ss:fff")}";
                    sb.AppendLine($"Setting currentPageViewState[\"{value}\"] = \"{value}\"");
                    currentPageViewState[value] = value;
                    sb.AppendLine($"Done setting currentPageViewState[\"{value}\"] = \"{value}\"");
                }
                sb.AppendLine($"SaveViewState call");
                //                    pageType.InvokeMember("SaveViewState", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, null, this, null);

                pageType.InvokeMember("SaveViewState", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, null, page, null);
                sb.AppendLine($"SaveViewState called");
            }
            catch (Exception exvs)
            {
                sb.AppendLine("Page-ViewState: Error");
                sb.AppendLine(exvs.Message);
                sb.AppendLine(exvs.StackTrace);
            }
            return sb.ToString();
        }

        public void SetupPageEvent(Page page, string eventName)
        {
            var pageType = page.GetType();

            var eventInfo = pageType.GetEvent(eventName); //"SaveStateComplete");
            var methodInfo = this.GetType().GetMethod($"Page_{eventName}");//"FirstPage_SaveStateCompleteX"); //, BindingFlags.Instance | BindingFlags.NonPublic);            
            //var obj = new FirstPage();
            Delegate d = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);

            eventInfo.RemoveEventHandler(page, d);
            eventInfo.AddEventHandler(page, d);
        }

        public void Page_InitComplete(object sender, EventArgs e)
        {
            LogPageEvent("Page_InitComplete", (Page)sender);
        }

        public void Page_LoadComplete(object sender, EventArgs e)
        {
            LogPageEvent("Page_LoadComplete", (Page)sender);
        }

        public void Page_PreInit(object sender, EventArgs e)
        {
            LogPageEvent("Page_PreInit", (Page)sender);
        }

        public void Page_PreLoad(object sender, EventArgs e)
        {
            LogPageEvent("Page_PreLoad", (Page)sender);
        }

        public void Page_PreRenderComplete(object sender, EventArgs e)
        {
            LogPageEvent("Page_PreRenderComplete", (Page)sender);
        }

        public void Page_SaveStateComplete(object sender, EventArgs e)
        {
            LogPageEvent("Page_SaveStateComplete", (Page)sender);
        }

        public void Page_DataBinding(object sender, EventArgs e)
        {
            LogPageEvent("Page_DataBinding", (Page)sender);
        }

        public void Page_Disposed(object sender, EventArgs e)
        {
            LogPageEvent("Page_Disposed", (Page)sender);
        }

        public void Page_Init(object sender, EventArgs e)
        {
            LogPageEvent("Page_Init", (Page)sender);
        }

        public void Page_Load(object sender, EventArgs e)
        {
            LogPageEvent("Page_Load", (Page)sender);
        }

        public void Page_PreRender(object sender, EventArgs e)
        {
            LogPageEvent("Page_PreRender", (Page)sender);
        }

        public void Page_Unload(object sender, EventArgs e)
        {
            LogPageEvent("Page_Unload", (Page)sender);
        }

        public void Page_AbortTransaction(object sender, EventArgs e)
        {
            LogPageEvent("Page_AbortTransaction", (Page)sender);
        }

        public void Page_CommitTransaction(object sender, EventArgs e)
        {
            LogPageEvent("Page_CommitTransaction", (Page)sender);
        }

        public void Page_Error(object sender, EventArgs e)
        {
            LogPageEvent("Page_Error", (Page)sender);
        }

        private void Context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            //Skal vide:
            //Context exist
            //Page exist
            //Read Session
            //Read ViewState
            //Read response
            //Write Session
            //Write Viewstate
            //Write response

            LogEvent(sender, e, "Context_PreRequestHandlerExecute", watcher);


            if (((HttpApplication) sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".axd")
            {
                return;
            }

            //HttpApplication app = (HttpApplication)sender;
            //HttpContext context = app.Context;
            //string filePath = context.Request.FilePath;
            //string fileExtension = VirtualPathUtility.GetExtension(filePath);

            //Page page = null;
            //if (fileExtension != null && fileExtension.Equals(".aspx"))
            //{
            //    page = (Page)context.CurrentHandler;


            //    //LoggingHelper.SetupSession(context, page);
            //    //LoggingHelper.SetupPage(context, page);
            //    //LoggingPage.LogRequest(context, page);
            //}

            //Log("Context_PreRequestHandlerExecute", context, page);

            //if (fileExtension != null && fileExtension.Equals(".ashx"))
            //{
            //    //LoggingHandler.LogRequest(context);
            //}

            //string response = watcher.ToString(); //Too early to get the response-html


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
            if (((HttpApplication) sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".axd")
            {
                return;
            }

            //SESSION KAN HENTES HER!!! :D
            //Og måske også sættes her... hmm
            //Skal være serializeable session value instanser, men det skal det jo alligevel for overhovedet at kunne være placeret i sessionen

            HttpApplication app = (HttpApplication) sender;

            var httpHandler = HttpContext.Current.Handler as HttpHandler;

            if (httpHandler != null)
            {
                // set the original handler back
                HttpContext.Current.Handler = httpHandler.OriginalHandler;
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

            //string response = watcher.ToString(); //Also too early for getting the response-html

            LogEvent(sender, e, "Context_PostRequestHandlerExecute", watcher);
        }

        private void LogEvent(object sender, EventArgs e, string name, StreamWatcher watcher)
        {
            try
            {
                if (((HttpApplication)sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".aspx")
                {
                    
                    HttpApplication app = (HttpApplication)sender;
                    HttpContext context = app.Context;
                    string filePath = context.Request.FilePath;
                    string fileExtension = VirtualPathUtility.GetExtension(filePath);
                    Page page = null;
                    if (fileExtension != null && fileExtension.Equals(".aspx"))
                        page = ((System.Web.UI.Page)context.CurrentHandler);
                    Log($"HttpModuleTest_{name}", context, page, watcher);
                }
            }
            catch (Exception ex)
            {
                
                
            }
        }

        private void Context_PostResolveRequestCache(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_PostResolveRequestCache", watcher);
        }

        private void Context_PostUpdateRequestCache(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_PostUpdateRequestCache", watcher);
        }

        private void Context_PreSendRequestContent(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_PreSendRequestContent", watcher);

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
            LogEvent(sender, e, "Context_PreSendRequestHeaders", watcher);

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
            LogEvent(sender, e, "Context_RequestCompleted", watcher);
        }

        private void Context_ResolveRequestCache(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_ResolveRequestCache", watcher);
        }

        private void Context_UpdateRequestCache(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_UpdateRequestCache", watcher);
        }

        private void Context_ReleaseRequestState(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_ReleaseRequestState", watcher);
        }

        private void Context_PostReleaseRequestState(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_PostReleaseRequestState", watcher);
        }

        private void Context_AcquireRequestState(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_AcquireRequestState", watcher);
        }

        private void Context_PostMapRequestHandler(object sender, EventArgs e)
        {
            LogEvent(sender, e, "Context_PostMapRequestHandler", watcher);

            HttpApplication app = (HttpApplication)sender;

            if (app.Context.Handler is IReadOnlySessionState || app.Context.Handler is IRequiresSessionState)
            {
                // no need to replace the current handler
                return;
            }

            // swap the current handler
            app.Context.Handler = new HttpHandler(app.Context.Handler);

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
            LogEvent(sender, e, "Context_PostAcquireRequestState", watcher);

            HttpApplication app = (HttpApplication)sender;

            var httpHandler = HttpContext.Current.Handler as HttpHandler;

            if (httpHandler != null)
            {
                // set the original handler back
                HttpContext.Current.Handler = httpHandler.OriginalHandler;
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
            if (((HttpApplication)sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".axd")
            {
                return;
            }

            watcher = new StreamWatcher(this.context.Response.Filter); 
            this.context.Response.Filter = watcher;

            LogEvent(sender, e, "Context_BeginRequest", watcher);

            HttpApplication app = (HttpApplication)sender;
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
            if (((HttpApplication)sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".axd")
            {
                return;
            }

            var httpHandler = HttpContext.Current.Handler as HttpHandler;

            if (httpHandler != null)
            {
                // set the original handler back
                HttpContext.Current.Handler = httpHandler.OriginalHandler;
            }

            LogEvent(sender, e, "Context_EndRequest", watcher);

            //TODO Look into this dynamic assembly.. it works, but is not in use atm
            //var weee = DynamicAssembly.LoadAssemblyInstances<ILogRecorderAndPlayer>("DynAssembly.dll").FirstOrDefault();
            //var ost = weee.DoStuff(6, 10);

            //var ccc = ConfigurationHelper.GetConfigurationSection();

            string response = watcher.ToString();

            this.context.Response.Filter = null;

            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;
            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);


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

            
        }

    }
}
