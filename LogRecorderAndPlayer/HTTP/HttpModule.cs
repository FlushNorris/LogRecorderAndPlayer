using System;
using System.Web;
using System.Web.UI;
using LogRecorderAndPlayer.Common;
using LogRecorderAndPlayer.Data;
using LogRecorderAndPlayer.HTTP;

namespace LogRecorderAndPlayer
{
    public class LRAPHttpModule : IHttpModule
    {
        private ResponseFilter responseWatcher;
        private HttpApplication context;

        private LRAPConfigurationSection _configuration = null;
        private LRAPConfigurationSection Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = ConfigurationHelper.GetConfigurationSection();
                }
                return _configuration;
            }
        }

        public void Init(HttpApplication context)
        {
            this.context = context;
            context.BeginRequest += Context_BeginRequest; //beforepage
            context.PostMapRequestHandler += Context_PostMapRequestHandler; //Before page
            context.PreRequestHandlerExecute += Context_PreRequestHandlerExecute; //Before page
            context.PostRequestHandlerExecute += Context_PostRequestHandlerExecute; //After page - læs session firsttime
            context.EndRequest += Context_EndRequest; //After page
        }

        private bool AllowApplicationObject(HttpApplication application)
        {
            return AllowRequestObject(application.Context.Request);
        }

        private bool AllowRequestObject(HttpRequest request)
        {
            var filePathExt = request.CurrentExecutionFilePathExtension.ToLower();            
            return !String.IsNullOrEmpty(filePathExt) && filePathExt != ".axd";
        }

        private bool IsWebpage(HttpRequest request)
        {
            var filePath = request.FilePath;
            var fileExtension = VirtualPathUtility.GetExtension(filePath);
            return fileExtension.ToLower().Equals(".aspx");
        }

        private bool IsLRAP(HttpRequest request)
        {
            var filePath = request.FilePath;
            var fileExtension = VirtualPathUtility.GetExtension(filePath);
            return fileExtension.ToLower().Equals(".lrap");
        }

        private void Context_BeginRequest(object sender, EventArgs e)
        {
            if (!Configuration.Enabled || !AllowApplicationObject(sender as HttpApplication))
                return;

            responseWatcher = new ResponseFilter(this.context.Response.Filter); 
            this.context.Response.Filter = responseWatcher;
        }

        private void Context_PostMapRequestHandler(object sender, EventArgs e)
        {
            if (!Configuration.Enabled || !AllowApplicationObject(sender as HttpApplication))
                return;

            HttpApplication app = (HttpApplication)sender;
            HttpContext context = app.Context;

            try
            {
                if (IsWebpage(context.Request))
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
                throw new Exception(context.Request.FilePath + " : " + ex.Message);
            }
        }

        private void Context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            if (!Configuration.Enabled || !AllowApplicationObject(sender as HttpApplication))
                return;

            HttpApplication app = (HttpApplication) sender;
            HttpContext context = app.Context;

            if (IsLRAP(context.Request))
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
                    LoggingHelper.SetupPage(context, page, page != null ? LogType.OnPageRequest : LogType.OnHandlerRequestReceived, requestForm);                    

                    if (page != null)
                    {
                        LoggingPage.LogRequest(context, page, requestForm);
                        LoggingPage.LogSession(context, page, requestForm, before: true);
                    }
                    else if (handler != null)
                    {
                        LoggingHandler.LogRequest(context, requestForm);
                        LoggingHandler.LogSession(context, requestForm, before: true);
                    }

                    return requestForm?.ToString();
                });
                this.context.Request.Filter = new RequestFilter(this.context.Request.Filter, context.Request.ContentEncoding, requestCallback);

                if (this.context.Request.Form == null || !requestCallbackExecuted) //Ensure requestCallback is being called to log session and request etc
                {
                    requestCallback(null);
                }                
            }
        }

        private void Context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            if (!Configuration.Enabled || !AllowApplicationObject(sender as HttpApplication))
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
            //TODO Player: Alter Before-ViewState here
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
            if (!Configuration.Enabled || !AllowApplicationObject(sender as HttpApplication))
            {
                return;
            }

            if (responseWatcher == null)
                return; //e.g. Invalid web.config setting
            string response = responseWatcher.ToString();


            HttpApplication application = (HttpApplication) sender;
            HttpContext context = application.Context;

            if (IsLRAP(context.Request))
            {
                LRAPHttpManager.ProcessLRAPFile(context);
                return;
            }            

            var page = context?.CurrentHandler as Page;
            var handler = context?.CurrentHandler as IHttpHandler;
            if (page != null || handler != null)
            {
                var sessionGUID = LoggingHelper.GetSessionGUID(context, page, () => new Guid());
                var pageGUID = LoggingHelper.GetPageGUID(context, page, () => new Guid());
                var serverGUID = LoggingHelper.GetServerGUID(context, () => null); 

                if (page != null)
                    response = LoggingPage.LogResponse(context, page, response);
                else
                    response = LoggingHandler.LogResponse(context, response);
                
                if (context.Response.ContentType == ContentType.TextHtml)
                {
                    var lrapValues = new LRAPValues(sessionGUID, pageGUID, serverGUID);
                    var newResponse = LRAPHttpManager.InsertLRAPScript(response, lrapValues);
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
