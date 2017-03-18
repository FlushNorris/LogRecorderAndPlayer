using System;
using System.Diagnostics;
using System.Reflection;
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
            context.PostMapRequestHandler += Context_PostMapRequestHandler;//Before page
            context.PreRequestHandlerExecute += Context_PreRequestHandlerExecute; //Before page
            context.PostRequestHandlerExecute += Context_PostRequestHandlerExecute; //After page - læs session firsttime
            context.EndRequest += Context_EndRequest; //After page
        }

        private void Context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            if (((HttpApplication)sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".axd")
                return;

            HttpApplication app = (HttpApplication) sender;
            HttpContext context = app.Context;
            string filePath = context.Request.FilePath;
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            if (fileExtension.ToLower() == ".lrap")
                return;

            var page = context?.CurrentHandler as Page;
            var handler = context?.CurrentHandler as IHttpHandler; //WebPageHttpHandler Class https://msdn.microsoft.com/en-us/library/hh396384(v=vs.111).aspx

            if (page != null || handler != null)
            {
                if (page != null || context.CurrentHandler.GetType().FullName == "System.Web.WebPages.WebPageHttpHandler") //System.Web.WebPages.WebPageHttpHandler == IHttpHandler 
                {
                    LoggingHelper.SetupSession(context, page);
                    LoggingHelper.SetupPage(context, page);

                    LoggingPage.LogSession(context, page, before: true);
                    LoggingPage.LogRequest(context, page);
                }
                else
                {
                    LoggingHandler.LogRequest(context);
                }
            }


            //if (fileExtension != null && fileExtension.Equals(".aspx"))
            //{
            //    var page = ((System.Web.UI.Page) context.CurrentHandler);
            //    LoggingHelper.SetupSession(context, page);
            //    LoggingHelper.SetupPage(context, page);                

            //    LoggingPage.LogSession(context, page, before:true);
            //    LoggingPage.LogRequest(context, page);

            //}
            //if (fileExtension != null && fileExtension.Equals(".ashx"))
            //{
            //    LoggingHandler.LogRequest(context);
            //}





        }

        private void Context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            if (((HttpApplication)sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".axd")
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
                    LoggingPage.LogSession(context, page, before: false);
                }
            }            
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

        public void Page_InitComplete(object sender, EventArgs e)
        {
            //Player: Alter Before-ViewState here
        }

        public void Page_PreLoad(object sender, EventArgs e)
        {
            LoggingPage.LogViewState(HttpContext.Current, (Page)sender, before: true);
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

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            if (((HttpApplication)sender).Context.Request.CurrentExecutionFilePathExtension.ToLower() == ".axd")
                return;

            watcher = new StreamWatcher(this.context.Response.Filter); //Man in the middle... alike
            this.context.Response.Filter = watcher;            
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

            //var streamWatcher = this.context.Response.Filter is StreamWatcher;
            //if (streamWatcher != null)
            //{
            //    this.context.Response.Filter = watcher.Base;
            //}

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
            var handler = context?.CurrentHandler as IHttpHandler; //WebPageHttpHandler Class https://msdn.microsoft.com/en-us/library/hh396384(v=vs.111).aspx

            if (page != null || handler != null)
            {
                if (page != null || context.CurrentHandler.GetType().FullName == "System.Web.WebPages.WebPageHttpHandler") //Handler 
                {
                    var sessionGUID = LoggingHelper.GetSessionGUID(context, page, () => new Guid());
                    var pageGUID = LoggingHelper.GetPageGUID(context, page, () => new Guid());

                    LoggingPage.LogResponse(context, page, response);

                    string lrapScript = $"<script type=\"text/javascript\" src=\"/logrecorderandplayerjs.lrap\"></script><script type=\"text/javascript\">logRecorderAndPlayer.init(\"{sessionGUID}\", \"{pageGUID}\");</script>";

                    var newResponse = response.Insert(LoggingHelper.GetHtmlIndexForInsertingLRAPJS(response), lrapScript);
                    context.Response.Clear();
                    context.Response.Write(newResponse);
                }
                else
                {
                    LoggingHandler.LogResponse(context, response);
                }
            }

            //if (fileExtension.ToLower().Equals(".aspx"))
            //{
            //    var page = ((System.Web.UI.Page)context.CurrentHandler);

            //    if (page != null)
            //    {
            //        var sessionGUID = LoggingHelper.GetSessionGUID(context, page, () => new Guid());
            //        var pageGUID = LoggingHelper.GetPageGUID(context, page, () => new Guid());
                    
            //        LoggingPage.LogResponse(context, page, response);
                    
            //        string lrapScript = $"<script type=\"text/javascript\" src=\"/logrecorderandplayerjs.lrap\"></script><script type=\"text/javascript\">logRecorderAndPlayer.init(\"{sessionGUID}\", \"{pageGUID}\");</script>";

            //        //var lrapJSInsertIndex = ;
            //        //if (lrapJSInsertIndex != -1) //Means that is doesn't make sense to add LRAP.js since eg. jQuery is not located on page (Statuscode=302 redirect has specialized "Object moved"-html)
            //        //{
            //        //}
            //        var newResponse = response.Insert(LoggingHelper.GetHtmlIndexForInsertingLRAPJS(response), lrapScript);
            //        context.Response.Clear();
            //        context.Response.Write(newResponse);
            //    }
            //    else
            //    {
            //        context.Response.Clear();
            //        context.Response.Write("OK");
            //    }
            //}

            //if (context.Response.ContentType == "text/html") //or something
            //{
                
            //}

            //if (fileExtension.ToLower().Equals(".ashx"))
            //{
            //    LoggingHandler.LogResponse(context, response);                
            //}
        }

        public void Dispose()
        {
        }
    }
}
