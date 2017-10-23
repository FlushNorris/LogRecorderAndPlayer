using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using mshtml;

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
                var jsonSerialized = nvcSession[key];
                var typeAndValue = SerializationHelper.Deserialize<TypeAndValue>(jsonSerialized, SerializationType.Json);
                var v = SerializationHelper.DeserializeByType(Type.GetType(typeAndValue.TypeName), typeAndValue.ValueJSON, SerializationType.Json);
                page.Session[key] = v;
            }
        }
        public static void SetSessionValues(HttpContext context, NameValueCollection nvcSession)
        {
            if (context == null || context.Session == null)
                return;

            foreach (var key in nvcSession.AllKeys)
            {
                var jsonSerialized = nvcSession[key];
                var typeAndValue = SerializationHelper.Deserialize<TypeAndValue>(jsonSerialized, SerializationType.Json);
                var v = SerializationHelper.DeserializeByType(Type.GetType(typeAndValue.TypeName), typeAndValue.ValueJSON, SerializationType.Json);
                context.Session[key] = v;
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

        public static void SetViewStateValues(Page page, NameValueCollection nvcViewState)
        {
            if (page == null || page.Session == null)
                return;

            var viewstate = GetViewState(page);

            foreach (var key in nvcViewState.AllKeys)
            {
                var jsonSerialized = nvcViewState[key];
                var typeAndValue = SerializationHelper.Deserialize<TypeAndValue>(jsonSerialized, SerializationType.Json);
                var v = SerializationHelper.DeserializeByType(Type.GetType(typeAndValue.TypeName), typeAndValue.ValueJSON, SerializationType.Json);
                viewstate[key] = v;
            }
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

        private static void ExecuteViewStateValueMatchesOrdered(string html, Action<Match> f)
        {
            var regExs = Consts.ViewStateFormFields.Select(formField => new Regex($"<input.+\"({formField})\".*value=\"(.*)\".*/>"));
            var matches = regExs.Select(x => x.Match(html));

            matches.Where(x => x.Success).OrderBy(x => x.Index).ToList().ForEach(f);
        }

        public static NameValueCollection GetResponseViewState(string html)
        {
            var nvc = new NameValueCollection();

            ExecuteViewStateValueMatchesOrdered(html, m =>
            {
                var tagName = m.Groups[1].Value;
                var value = m.Groups[2].Value;
                nvc[tagName] = value;
            });

            return nvc;            
        }

        public static string SetResponseViewState(string html, NameValueCollection nvc)
        {            
            StringBuilder sb = new StringBuilder();
            int currPosition = 0;
            ExecuteViewStateValueMatchesOrdered(html, m =>
            {
                var tagName = m.Groups[1].Value;
                var valueObj = m.Groups[2];
                var value = nvc[tagName];
                if (value != null)
                {
                    sb.Append(html.Substring(currPosition, valueObj.Index - currPosition));
                    sb.Append(value);
                    currPosition = valueObj.Index + valueObj.Length;
                }
            });
            if (currPosition < html.Length)
                sb.Append(html.Substring(currPosition));

            return sb.ToString();            
        }

        //https://msdn.microsoft.com/en-us/library/system.web.httprequest.params(v=vs.110).aspx
        public static NameValueCollection ParamsWithSpecialRequestForm(HttpContext context, NameValueCollection requestForm)
        {
            var nvc = new NameValueCollection();
            if (context?.Request != null)
            {
                nvc.Add(context.Request.QueryString);
            }

            if (requestForm != null)
            {
                requestForm.AllKeys.Where(x => !Consts.LRAPFormFields.Contains(x) || !nvc.AllKeys.Contains(x)).ToList().ForEach(x => nvc.Add(x, requestForm[x]));
            }                
            else if (context?.Request != null)
            {
                context.Request.Form.AllKeys.Where(x => !Consts.LRAPFormFields.Contains(x) || !nvc.AllKeys.Contains(x)).ToList().ForEach(x => nvc.Add(x, context.Request.Form[x]));
            }

            if (context?.Request != null)
            {
                context.Request.Cookies.AllKeys.ToList().ForEach(x =>
                {
                    var cv = context.Request.Cookies[x];
                    if (cv != null)
                    {
                        if (!Consts.LRAPFormFields.Contains(x) || !nvc.AllKeys.Contains(x))
                            nvc.Add(x, cv.Value);
                    }
                });

                nvc.Add(context.Request.ServerVariables);
            }

            return nvc;
        }

        public static RequestParams BuildRequestParams(HttpContext context, NameValueCollection requestForm)
        {
            var requestParamsForm = requestForm != null ? requestForm : context.Request.Form;            
            var requestParams = new RequestParams()
            {
                Form = SerializationHelper.NameValueCollectionToDictionary(requestParamsForm),
                Cookies = SerializationHelper.HttpCookieCollectionToDictionary(context.Request.Cookies),
                QueryString = SerializationHelper.NameValueCollectionToDictionary(context.Request.QueryString),
                ServerVariables = SerializationHelper.NameValueCollectionToDictionary(context.Request.ServerVariables)
            };
            return requestParams;
        }
    }
}
