﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;

namespace LogRecorderAndPlayer
{
    public class HttpModule : IHttpModule
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
            //context.PostAuthenticateRequest
            //context.PostAuthorizeRequest
            //context.PostLogRequest
            //context.MapRequestHandler
            //context.LogRequest

            //    application.PostAcquireRequestState += new EventHandler(Application_PostAcquireRequestState);
            //    application.PostMapRequestHandler += new EventHandler(Application_PostMapRequestHandler);
        }

        private void Context_ReleaseRequestState1(object sender, EventArgs e)
        {

        }

        private void Context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
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

                var viewStatePropertyDescriptor = pageType.GetProperty("ViewState", BindingFlags.Instance | BindingFlags.NonPublic);
                var currentPageViewState = (StateBag)viewStatePropertyDescriptor.GetValue(HttpContext.Current.CurrentHandler);
                currentPageViewState["WHAT"] = "ALTERED";

                //((System.Web.UI.Page) context.CurrentHandler).ViewState["WHAT"] = "ALTERED";

            }
        }

        private void Context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            //SESSION KAN HENTES HER!!! :D
            //Og måske også sættes her... hmm
            //Skal være serializeable session value instanser, men det skal det jo alligevel for overhovedet at kunne være placeret i sessionen

            HttpApplication app = (HttpApplication)sender;            

            HttpHandler resourceHttpHandler = HttpContext.Current.Handler as HttpHandler;

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

                var viewStatePropertyDescriptor = pageType.GetProperty("ViewState", BindingFlags.Instance | BindingFlags.NonPublic);
                var currentPageViewState = (StateBag)viewStatePropertyDescriptor.GetValue(HttpContext.Current.CurrentHandler);
                currentPageViewState["WHAT"] = "ALTERED";

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
            app.Context.Handler = new MyHttpHandler(app.Context.Handler);
        }

        private void Context_PostAcquireRequestState(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;

            MyHttpHandler resourceHttpHandler = HttpContext.Current.Handler as MyHttpHandler;

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
            watcher = new StreamWatcher(this.context.Response.Filter); //Man in the middle... alike
            this.context.Response.Filter = watcher;

            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;
            string filePath = context.Request.FilePath;
            string fileExtension =
                VirtualPathUtility.GetExtension(filePath);
            if (fileExtension.Equals(".aspx"))
            {
                context.Response.Write("<h1><font color=red>" +
                    "HelloWorldModuleXXX: Beginning of Request" +
                    "</font></h1><hr>");
            }
        }
        private void Context_EndRequest(object sender, EventArgs e)
        {
            string value = watcher.ToString();

            this.context.Response.Filter = null;

            HttpApplication application = (HttpApplication)sender;
            HttpContext context = application.Context;
            string filePath = context.Request.FilePath;
            string fileExtension =
                VirtualPathUtility.GetExtension(filePath);
            if (fileExtension.Equals(".aspx"))
            {
                //context.CurrentHandler      

                var page = ((System.Web.UI.Page)context.CurrentHandler);
                if (page != null)
                {
                    var pageType = page.GetType();

                    var viewStatePropertyDescriptor = pageType.GetProperty("ViewState", BindingFlags.Instance | BindingFlags.NonPublic);
                    var currentPageViewState = (StateBag)viewStatePropertyDescriptor.GetValue(HttpContext.Current.CurrentHandler);
                    currentPageViewState["WHAT"] = "ALTERED";

                    //((System.Web.UI.Page) context.CurrentHandler).ViewState["WHAT"] = "ALTERED";
                    context.Response.Write("<hr><h1><font color=red>" + "HelloWorldModuleXXXARGH2: End of Request</font></h1>");
                }
                else
                {
                    var thingy = new LoggingThingy() { Id = 0, Title = "Some title", Timestamp = DateTime.Now };
                    context.Response.Clear();
                    context.Response.Write(SerializationHelper.Serialize(thingy, SerializationType.Json));

                    context.Response.Status = "200 OK";
                    context.Response.StatusCode = 200;
                    context.Response.StatusDescription = "OK";

                }
            }

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