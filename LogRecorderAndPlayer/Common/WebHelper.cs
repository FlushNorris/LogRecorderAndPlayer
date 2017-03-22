using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using System.Web.UI;

namespace LogRecorderAndPlayer
{
    public class WebContext
    {
        public HttpContext HttpContext { get; set; }
    }

    public static class WebHelper
    {
        public static WebContext GetContext(HttpContext httpContext)
        {
            return new WebContext() {HttpContext = httpContext};
        }

        public static HttpContext ResumeContext(WebContext webContext, HttpContext httpContext)
        {
            if (httpContext != null)
                return httpContext;
            return webContext.HttpContext;
        }

        public static StateBag GetViewState(System.Web.UI.Page page)
        {
            var pageType = page.GetType();

            var viewStatePropertyDescriptor = pageType.GetProperty("ViewState", BindingFlags.Instance | BindingFlags.NonPublic);
            var viewState = (StateBag)viewStatePropertyDescriptor.GetValue(HttpContext.Current.CurrentHandler);

            return viewState;
        }

        public static NameValueCollection GetSessionValues(Page page)
        {
            if (page == null || page.Session == null)
                return null;

            var nvcSession = new NameValueCollection();
            foreach (string sessionKey in page.Session)
            {
                try
                {
                    nvcSession[sessionKey] = SerializationHelper.Serialize(page.Session[sessionKey], SerializationType.Json);
                }
                catch (Exception ex)
                {
                    nvcSession[sessionKey] = "Unable to serialize value";
                }
            }
            return nvcSession;
        }

        public static NameValueCollection GetSessionValues(HttpContext context)
        {
            if (context == null || context.Session == null)
                return null;

            var nvcSession = new NameValueCollection();
            foreach (string sessionKey in context.Session)
            {
                try
                {
                    nvcSession[sessionKey] = SerializationHelper.Serialize(context.Session[sessionKey], SerializationType.Json);
                }
                catch (Exception ex)
                {
                    nvcSession[sessionKey] = "Unable to serialize value";
                }
            }
            return nvcSession;
        }

        public static NameValueCollection GetViewStateValues(Page page)
        {
            var nvcViewState = new NameValueCollection();
            var viewstate = GetViewState(page);
            foreach (string viewStateKey in viewstate.Keys)
            {
                try
                {
                    nvcViewState[viewStateKey] = SerializationHelper.Serialize<object>(viewstate[viewStateKey], SerializationType.Json);
                }
                catch (Exception ex)
                {
                    nvcViewState[viewStateKey] = "Unable to serialize value";
                }
            }
            return nvcViewState;
        }
    }
}
