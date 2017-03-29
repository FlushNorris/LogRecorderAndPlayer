using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;

namespace LogRecorderAndPlayer
{
    public static class LoggingHelper
    {
        public static void SetupSession(HttpContext context, System.Web.UI.Page page)
        {
            var v = GetSessionGUID(context, page);
            if (v == null)
            {
                v = Guid.NewGuid();
                if (!SetSessionGUID(context.Session, v.Value))
                {
                    SetSessionGUIDCookie(context, v.Value);
                }
            }
            SetSessionGUIDInViewState(page, v.Value);
        }

        private static void SetSessionGUIDCookie(HttpContext context, Guid guid)
        {
            if (context == null)
                return;

            if (context.Request != null && context.Request.Cookies != null)
            {
                context.Request.Cookies.Remove(Consts.SessionGUIDTag);
                context.Request.Cookies.Add(new HttpCookie(Consts.SessionGUIDTag, guid.ToString()));
            }

            if (context.Response != null && context.Response.Cookies != null)
            {
                context.Response.Cookies.Remove(Consts.SessionGUIDTag);
                context.Response.Cookies.Add(new HttpCookie(Consts.SessionGUIDTag, guid.ToString()));
            }
        }

        private static Guid? GetSessionGUIDCookie(HttpContext context)
        {
            if (context == null)
                return null;

            if (context.Request != null && context.Request.Cookies != null)
            {
                try
                {
                    var v = context.Request.Cookies[Consts.SessionGUIDTag];
                    if (v == null)
                        return null;
                    return new Guid(v.Value);
                }
                catch{}
            }

            if (context.Response != null && context.Response.Cookies != null)
            {
                try
                {
                    var v = context.Response.Cookies[Consts.SessionGUIDTag];
                    if (v == null)
                        return null;
                    return new Guid(v.Value);
                }
                catch { }
            }

            return null;
        }

        public static void SetupPage(HttpContext context, System.Web.UI.Page page)
        {
            var pageGUID = GetPageGUID(context, page);
            if (pageGUID == null)
            {
                SetPageGUID(page, Guid.NewGuid());
            }
        }

        public static Guid? GetPageGUID(HttpContext context, System.Web.UI.Page page, Func<Guid?> defaultValue = null)
        {
            if (context == null || context.Handler == null)
                return defaultValue != null ? defaultValue() : null;

            var requestPageGUID = context.Request?.Params[Consts.PageGUIDTag];
            if (requestPageGUID != null)
                return new Guid(requestPageGUID);

            if (page == null)
                return defaultValue != null ? defaultValue() : null;

            //Couldn't use the querystring, because it resulted in a redirect on postback
            //var qryStr = HttpUtility.ParseQueryString(page.ClientQueryString);
            //var tag = "lrap-pageid";
            //var v = qryStr[tag];
            //if (v != null)
            //{
            //    return new Guid(v);
            //}

            string v = page.IsPostBack ? context.Request.Form[Consts.PageGUIDTag] : null;
            if (v != null)
            {
                return new Guid(v);
            }

            var viewState = WebHelper.GetViewState(page);

            var pageGUID = viewState[Consts.PageGUIDTag] as Guid?;
            if (pageGUID == null)
                return defaultValue != null ? defaultValue() : null;
            return pageGUID;
        }

        private static void SetPageGUID(System.Web.UI.Page page, Guid pageGUID)
        {
            if (page == null)
                return;

            var viewState = WebHelper.GetViewState(page);
            viewState[Consts.PageGUIDTag] = Guid.NewGuid();
        }

        public static Guid? GetInstanceGUID(HttpContext context, Func<Guid?> defaultValue = null)
        {
            if (context == null)
                return defaultValue != null ? defaultValue() : null;

            var requestGUID = context.Request?.Params[Consts.GUIDTag];
            if (requestGUID != null)
                return new Guid(requestGUID);

            return defaultValue != null ? defaultValue() : null;
        }

        public static Guid? GetBundleGUID(HttpContext context, Func<Guid?> defaultValue = null)
        {
            if (context == null)
                return defaultValue != null ? defaultValue() : null;

            var requestGUID = context.Request?.Params[Consts.BundleGUIDTag];
            if (requestGUID != null)
                return new Guid(requestGUID);

            return defaultValue != null ? defaultValue() : null;
        }

        public static Guid? GetSessionGUID(HttpContext context, System.Web.UI.Page page, Func<Guid?> defaultValue = null)
        {
            if (context == null || context.Handler == null)
                return defaultValue != null ? defaultValue() : null;

            var requestSessionGUID = context.Request?.Params[Consts.SessionGUIDTag];
            if (requestSessionGUID != null)
            {
                try
                {
                    return new Guid(requestSessionGUID);
                }
                catch{}
            }

            Guid? v = null;

            //Var vist visse problemer med at kalde context.Session... internal exception, undersøg dette. Eller pludselig får jeg følelsen af det var noget med Page-objectet som skulle testes af Page.IsValid før man spurgte ind til mere.
            //Dette er pga WebPageHttpHandler som bliver anvendt som en WebPage, men er en HttpHandler... og dvs har ikke et Session object selv, men må ty til page-objectet
            if (page == null)
            {
                try
                {
                    if (context.Session != null)
                    {
                        v = context.Session[Consts.SessionGUIDTag] as Guid?;
                        if (v != null)
                            return v;
                    }
                }
                catch{}
                v = GetSessionGUIDCookie(context);
                if (v != null)
                    return v;
                return defaultValue != null ? defaultValue() : null;
            }

            var viewState = WebHelper.GetViewState(page);

            v = viewState[Consts.SessionGUIDTag] as Guid?;
            if (v != null)
                return v;

            var sessionGUID = page.Session[Consts.SessionGUIDTag] as Guid?;
            if (sessionGUID == null)
                return defaultValue != null ? defaultValue() : null;
            return sessionGUID;
        }

        public static bool SetSessionGUID(HttpSessionState session, Guid sessionGUID)
        {
            if (session == null)
                return false;
            session[Consts.SessionGUIDTag] = sessionGUID;
            return true;
        }

        public static Guid? GetSessionGUID(HttpSessionState session)
        {
            if (session == null)
                return null;
            return session[Consts.SessionGUIDTag] as Guid?;
        }

        private static void SetSessionGUIDInViewState(System.Web.UI.Page page, Guid sessionGUID)
        {
            if (page == null)
                return;
            var viewState = WebHelper.GetViewState(page);
            viewState[Consts.SessionGUIDTag] = sessionGUID;
        }

        public static LogHandlerResponse LogHandlerRequest(string requestJSON)
        {
            var request = SerializationHelper.Deserialize<LogHandlerDTO>(requestJSON, SerializationType.Json);
            var now = DateTime.Now;
            var logHandlerResponse = new LogHandlerResponse()
            {
                ServerTimeStart = TimeHelper.UnixTimestamp()
            };

            var logElementResponse = new LogElementResponse();
            foreach (var logElement in request.LogElements)
            {
                logElement.Element = HttpUtility.HtmlDecode(logElement.Element); //Has to be encoded when sending from client to server, due to asp.net default security
                logElement.Value = logElement.Value != null ? HttpUtility.HtmlDecode(logElement.Value) : null; //Has to be encoded when sending from client to server, due to asp.net default security
                logElementResponse += LogElement(logElement);
            }

            logHandlerResponse.Success = logElementResponse.Success;
            logHandlerResponse.Message = logElementResponse.Message;
            logHandlerResponse.ServerTimeEnd = TimeHelper.UnixTimestamp();

            return logHandlerResponse;
        }

        public static LogElementResponse LogElement(LogElementDTO logElement)
        {
            try
            {
                var config = ConfigurationHelper.GetConfigurationSection();
                GetLoggingPersister(config.LogType).LogElement(config.FilePath, logElement);

                return new LogElementResponse() {Success = true};
            }
            catch (Exception ex)
            {
                return new LogElementResponse() {Success = false, Message = ex.Message + " ::: " + ex.StackTrace};
            }
        }

        private static string GetExtensionByLogType(LRAPLogType logType)
        {
            switch (logType)
            {
                case LRAPLogType.CSV:
                    return "csv";
                case LRAPLogType.JSON:
                    return "json";
                default:
                    return null;
            }
        }

        public static IEnumerable<LogElementDTO> LoadElements(string path, LRAPLogType logType, DateTime? from = null, DateTime? to = null)
        {
            return GetLoggingPersister(logType).LoadLogElements(path, from, to);
        }

        public static LogElementsInfo LoadElementsInfo(string path, LRAPLogType logType, DateTime? from = null, DateTime? to = null)
        {
            return GetLoggingPersister(logType).LoadLogElementsInfo(path, from, to);
        }

        public static LogElementDTO LoadElement(LRAPLogType logType, LogElementInfo logElementInfo)
        {
            return GetLoggingPersister(logType).LoadLogElement(logElementInfo);
        }

        private static ILoggingPersister GetLoggingPersister(LRAPLogType logType)
        {
            switch (logType)
            {
                case LRAPLogType.CSV:
                    return new LoggingToCSV();
                case LRAPLogType.JSON:
                    return new LoggingToJSON();
                default:
                    throw new NotImplementedException();
            }
        }

        public static string StripUrlForLRAP(string url)
        {
            url = url.Trim();
            var i = url.IndexOf('?');
            if (i == -1 || i == url.Length - 1)
                return url;
            var query = url.Substring(i + 1).Trim();
            url = url.Substring(0, i);

            var queryBuilder = HttpUtility.ParseQueryString(query);
            queryBuilder.Remove(Consts.GUIDTag);
            queryBuilder.Remove(Consts.SessionGUIDTag);
            queryBuilder.Remove(Consts.PageGUIDTag);
            queryBuilder.Remove(Consts.BundleGUIDTag);

            query = queryBuilder.ToString();

            return $"{url}{(String.IsNullOrWhiteSpace(query) ? "" : "?")}{query}";
        }

        public static int GetHtmlIndexForInsertingLRAPJS(string html)
        {
            Regex r = new Regex("(?i)<\\s*script[^>]+src\\s*=(\\s*\\S*jquery[^>/]+)(/*)>");
            var m = r.Matches(html);

            int indexToInsertLRAP = -1;
            if (m.Count > 0)
            {
                var mLast = m[m.Count - 1];
                var mSlashCheck = mLast.Groups[2];
                bool seperatedEndTag = mSlashCheck.Value != "/";
                if (seperatedEndTag)
                {
                    var rEndTag = new Regex("<\\s*/\\s*script[^>/]*>");
                    var mEndTag = rEndTag.Match(html, mSlashCheck.Index);
                    if (mEndTag.Success)
                    {
                        indexToInsertLRAP = mEndTag.Index + mEndTag.Length;
                    }
                }
                else
                {
                    indexToInsertLRAP = mLast.Index + mLast.Length;
                }
            }

            if (indexToInsertLRAP == -1)
            {
                indexToInsertLRAP = html.Length;
            }

            return indexToInsertLRAP;
        }

        public static NameValueCollection GetSessionValues(Page page)
        {
            var nvc = WebHelper.GetSessionValues(page);
            if (nvc == null)
                return null;
            nvc.Remove(Consts.SessionGUIDTag);
            return nvc;
        }

        public static NameValueCollection GetSessionValues(HttpContext context)
        {
            var nvc = WebHelper.GetSessionValues(context);
            if (nvc == null)
                return null;
            nvc.Remove(Consts.SessionGUIDTag);
            return nvc;
        }

        public static NameValueCollection GetViewStateValues(Page page)
        {
            var nvc = WebHelper.GetViewStateValues(page);
            nvc.Remove(Consts.SessionGUIDTag);
            nvc.Remove(Consts.BundleGUIDTag);
            nvc.Remove(Consts.GUIDTag);
            nvc.Remove(Consts.PageGUIDTag);
            return nvc;
        }

        public static Guid? GetProgressGUID(HttpContext context)
        {
            var r = context.Request[Consts.ProgressGUIDTag];
            if (String.IsNullOrWhiteSpace(r))
                return null;
            return new Guid(r);
        }
    }

    public class LogHandlerResponse
    {
        public bool Success;
        public string Message;

        public double ServerTimeStart;
        public double ServerTimeEnd;
    }

    public class LogElementResponse
    {
        public bool Success;
        public string Message;
        public object Object;

        public static LogElementResponse operator +(LogElementResponse left, LogElementResponse right)
        {
            return new LogElementResponse()
            {
                Success = (left?.Success ?? true) && (right?.Success ?? true),
                Message = (left?.Success ?? true) ? left?.Message : right?.Message
            };
        }
    }
}
