using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceModel;
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
            context.BeginRequest += Context_BeginRequest; //beforepage
            context.PostMapRequestHandler += Context_PostMapRequestHandler; //Before page
            context.PreRequestHandlerExecute += Context_PreRequestHandlerExecute; //Before page
            context.PostRequestHandlerExecute += Context_PostRequestHandlerExecute; //After page - læs session firsttime
            context.EndRequest += Context_EndRequest; //After page
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            if (((HttpApplication)sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".axd")
                return;

            //var requestCallback = new Func<string, string>(content => {
            //    throw new Exception("godt saa");
            //});

            //this.context.Request.Filter = new RequestFilter(this.context.Request.Filter, context.Request.ContentEncoding, requestCallback);

            watcher = new StreamWatcher(this.context.Response.Filter); //Man in the middle... alike
            this.context.Response.Filter = watcher;
        }

        private void Context_PostMapRequestHandler(object sender, EventArgs e)
        {
            if (((HttpApplication)sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".axd")
                return;

            HttpApplication app = (HttpApplication)sender;
            HttpContext context = app.Context;
            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            try
            {
                if (fileExtension.ToLower().Equals(".aspx"))
                {
                    var page = (Page)context.CurrentHandler;

                    if (page != null)
                    {
                        SetupPageEvent(page, "PreLoad");
                        SetupPageEvent(page, "InitComplete");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(filePath + " : " + ex.Message);
            }
        }

        private void Context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            if (((HttpApplication) sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".axd")
                return;

            HttpApplication app = (HttpApplication) sender;
            HttpContext context = app.Context;
            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            if (fileExtension.ToLower() == ".lrap")
                return;

            var page = context?.CurrentHandler as Page;
            var handler = context?.CurrentHandler as IHttpHandler;
            if (page != null || handler != null)
            {
                //https://github.com/snives/HttpModuleRewrite
                bool requestCallbackExecuted = false;
                var requestCallback = new Func<string, string>(content =>
                {
                    requestCallbackExecuted = true;
                    var requestForm = content != null ? HttpUtility.ParseQueryString(content) : null;

                    LoggingHelper.SetupSession(context, page, requestForm);
                    LoggingHelper.SetupPage(context, page, requestForm);

                    if (page != null)
                    {
                        LoggingPage.LogSession(context, page, requestForm, before: true);
                        LoggingPage.LogRequest(context, page, requestForm);
                    }
                    else if (handler != null)
                    {
                        LoggingHandler.LogSession(context, requestForm, before: true);
                        LoggingHandler.LogRequest(context, requestForm);
                    }


                    return requestForm?.ToString();
                });
                this.context.Request.Filter = new RequestFilter(this.context.Request.Filter, context.Request.ContentEncoding, requestCallback);

                if (this.context.Request.Form == null || !requestCallbackExecuted)
                {
                    requestCallback(null);
                }
            }
        }

        private void Context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            if (((HttpApplication) sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".axd")
            {
                return;
            }

            HttpApplication app = (HttpApplication) sender;
            HttpContext context = app.Context;
            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            if (fileExtension != null && fileExtension.Equals(".aspx"))
            {
                var page = ((System.Web.UI.Page) context.CurrentHandler);
                if (page != null)
                {
                    LoggingPage.LogViewState(context, page, before: false);
                    LoggingPage.LogSession(context, page, requestForm: null, before: false);
                }
            }
        }

        public void Page_InitComplete(object sender, EventArgs e)
        {
            //Player: Alter Before-ViewState here
        }

        public void Page_PreLoad(object sender, EventArgs e)
        {
            LoggingPage.LogViewState(HttpContext.Current, (Page) sender, before: true);
        }

        public void SetupPageEvent(Page page, string eventName)
        {
            var pageType = page.GetType();

            var eventInfo = pageType.GetEvent(eventName);
            var methodInfo = this.GetType().GetMethod($"Page_{eventName}");

            Delegate d = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);

            eventInfo.RemoveEventHandler(page, d);
            eventInfo.AddEventHandler(page, d);
        }        

        private void Context_EndRequest(object sender, EventArgs e)
        {
            //var x = new NamedPipeClient();
            //var x2 = x.TalkForServer();

            if (((HttpApplication) sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".axd")
            {
                return;
            }

            //TODO Look into this dynamic assembly.. it works, but is not in use atm
            //var weee = DynamicAssembly.LoadAssemblyInstances<ILogRecorderAndPlayer>("DynAssembly.dll").FirstOrDefault();
            //var ost = weee.DoStuff(6, 10);

            //var ccc = ConfigurationHelper.GetConfigurationSection();

            //var streamWatcher = this.context.Response.Filter is StreamWatcher;
            //if (streamWatcher != null)
            //{
            //    this.context.Response.Filter = watcher.Base;
            //}
            if (watcher == null)
                return; //e.g. Invalid web.config setting
            string response = watcher.ToString();

            HttpApplication application = (HttpApplication) sender;
            HttpContext context = application.Context;

            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            if (filePath.ToLower() == "/logrecorderandplayerjs.lrap")
            {
                ResponseHelper.Write(context.Response, "text/javascript", ResourceHelper.GetResourceContent("LogRecorderAndPlayer.JS.LogRecorderAndPlayer.js"), new TimeSpan(1, 0, 0));
                return;
            }

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

            if (fileExtension.ToLower() == ".lrap")
                return;

            var page = context?.CurrentHandler as Page;
            var handler = context?.CurrentHandler as IHttpHandler;
            if (page != null || handler != null)
            {
                try
                {
                    var pageType = context?.CurrentHandler.GetType();

                    var viewStatePropertyDescriptor = pageType.GetProperty("Page"); //, BindingFlags.Instance | BindingFlags.NonPublic);
                    var anotherPage = (Page)viewStatePropertyDescriptor.GetValue(context?.CurrentHandler);

                    if (anotherPage != null)
                    {
                        throw new Exception("WAUW");
                    }
                }
                catch (Exception)
                {
                }

                //TransferRequestHandler... hvad i alverden anvendes den til???
                var sessionGUID = LoggingHelper.GetSessionGUID(context, page, () => new Guid());
                var pageGUID = LoggingHelper.GetPageGUID(context, page, () => new Guid());
                var serverGUID = LoggingHelper.GetServerGUID(context, () => null); //or perhaps should be renamed to playerGUID

                if (page != null)
                    response = LoggingPage.LogResponse(context, page, response);
                else
                    response = LoggingHandler.LogResponse(context, response);
                
                if (context.Response.ContentType == ContentType.TextHtml) 
                {                    
                    string lrapPreScript = $"<script type=\"text/javascript\" src=\"/logrecorderandplayerjs.lrap?v={AssemblyHelper.RetrieveLinkerTimestamp().Ticks}\"></script><script type=\"text/javascript\">logRecorderAndPlayer.preInit(\"{sessionGUID}\", \"{pageGUID}\", \"{(serverGUID != null ? serverGUID.ToString() : "")}\");</script>";
                    var newResponse = response.Insert(LoggingHelper.GetHtmlIndexForInsertingLRAPJS(response), lrapPreScript);                    
                    string lrapPostScript = $"<script type=\"text/javascript\">logRecorderAndPlayer.postInit(\"{sessionGUID}\", \"{pageGUID}\", \"{(serverGUID != null ? serverGUID.ToString() : "")}\");</script>";
                    newResponse += lrapPostScript;
                    context.Response.Clear();
                    context.Response.Write(newResponse);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
