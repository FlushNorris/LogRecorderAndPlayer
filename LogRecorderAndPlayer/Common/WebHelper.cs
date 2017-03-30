using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace LogRecorderAndPlayer
{
    public class WebContext
    {
        public HttpContext HttpContext { get; set; }
    }

    public class TypeAndValue
    {
        public string TypeName { get; set; }
        public string ValueJSON { get; set; }
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
                    var v = page.Session[sessionKey];
                    var typeAndValue = new TypeAndValue() {TypeName = v.GetType().ToString(), ValueJSON = SerializationHelper.Serialize(v, SerializationType.Json)};

                    nvcSession[sessionKey] = SerializationHelper.Serialize(typeAndValue, SerializationType.Json);
                }
                catch (Exception ex)
                {
                    nvcSession[sessionKey] = "Unable to serialize value";
                }
            }
            return nvcSession;
        }

        public static void SetSessionValues(Page page, NameValueCollection nvcSession)
        {
            if (page == null || page.Session == null)
                return;

            foreach (var key in nvcSession.AllKeys)
            {
                var jsonSerialized = nvcSession[key]; // Got dammit... need type of cause to deserialize... yawn!
                var typeAndValue = SerializationHelper.Deserialize<TypeAndValue>(jsonSerialized, SerializationType.Json);
                var v = SerializationHelper.DeserializeByType(Type.GetType(typeAndValue.TypeName), typeAndValue.ValueJSON, SerializationType.Json);
                page.Session[key] = v;
            }
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
                    var v = viewstate[viewStateKey];
                    var typeAndValue = new TypeAndValue() { TypeName = v.GetType().ToString(), ValueJSON = SerializationHelper.Serialize(v, SerializationType.Json) };

                    nvcViewState[viewStateKey] = SerializationHelper.Serialize(typeAndValue, SerializationType.Json);
                }
                catch (Exception ex)
                {
                    nvcViewState[viewStateKey] = "Unable to serialize value";
                }
            }
            return nvcViewState;
        }

        public static string RemoveQryStrElement(string url, string tag)
        {
            var regEx = new Regex("[?&]" + tag + "=[^$?&]*[$?&]*");
            var m = regEx.Match(url);
            if (m.Success)
            {
                var m1 = m.Groups[1];
                var s = m1.Value;
                var sep = s[s.Length - 1];
                return url.Substring(0, m1.Index) + (sep == '&' ? s[0] + url.Substring(m1.Index + s.Length) : "");
            }
            return url;
        }

        public static string AddQryStrElement(string url, string tag, string value)
        {
            return url + (url.IndexOf('?') == -1 ? "?" : "&") + tag + "=" + value;
        }

    }
}
