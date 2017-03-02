using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
                SetSessionGUID(page, v.Value);
            }
            SetSessionGUIDInViewState(page, v.Value);
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
            if (context == null)
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

            string v = page.IsPostBack ? context.Request.Form["LRAP-PAGEGUID"] : null;
            if (v != null)
            {
                return new Guid(v);
            }

            var viewState = GetViewState(page);

            var pageGUID = viewState["LRAP_PageGUID"] as Guid?;
            if (pageGUID == null)
                return defaultValue != null ? defaultValue() : null;
            return pageGUID;
        }

        private static StateBag GetViewState(System.Web.UI.Page page)
        {
            var pageType = page.GetType();

            var viewStatePropertyDescriptor = pageType.GetProperty("ViewState", BindingFlags.Instance | BindingFlags.NonPublic);
            var viewState = (StateBag)viewStatePropertyDescriptor.GetValue(HttpContext.Current.CurrentHandler);

            return viewState;
        }

        private static void SetPageGUID(System.Web.UI.Page page, Guid pageGUID)
        {
            var viewState = GetViewState(page);

            viewState["LRAP_PageGUID"] = Guid.NewGuid();
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
            if (context == null)
                return defaultValue != null ? defaultValue() : null;

            var requestSessionGUID = context.Request?.Params[Consts.SessionGUIDTag];
            if (requestSessionGUID != null)
                return new Guid(requestSessionGUID);

            if (page == null)
                return defaultValue != null ? defaultValue() : null;

            var viewState = GetViewState(page);

            var v = viewState["SessionGUID"] as Guid?;
            if (v != null)
                return v;

            var sessionGUID = page.Session["LRAP-SessionGUID"] as Guid?;
            if (sessionGUID == null)
                return defaultValue != null ? defaultValue() : null;
            return sessionGUID;
        }

        private static void SetSessionGUID(System.Web.UI.Page page, Guid sessionGUID)
        {
            page.Session["LRAP-SessionGUID"] = sessionGUID;
        }

        private static void SetSessionGUIDInViewState(System.Web.UI.Page page, Guid sessionGUID)
        {
            var viewState = GetViewState(page);
            viewState["LRAP-SessionGUID"] = sessionGUID;
        }

        public static void LogHandlerRequest(string request)
        {
            var logElements = SerializationHelper.Deserialize<LogHandlerDTO[]>(request, SerializationType.Json);
            foreach (var logElement in logElements)
            {
                logElement.Element = HttpUtility.HtmlDecode(logElement.Element); //Has to be encoded when sending from client to server, due to asp.net default security
                LogElement(logElement);
            }
        }

        public static void LogElement(LogHandlerDTO logElement)
        {           
            var config = ConfigurationHelper.GetConfigurationSection();
            switch (config.LogType)
            {
                case LRAPConfigurationSectionLogType.CSV:
                    LoggingToCSV.LogElement(config.FilePath, logElement);
                    break;
                case LRAPConfigurationSectionLogType.JSON:
                    LoggingToJSON.LogElement(config.FilePath, logElement);
                    break;
                case LRAPConfigurationSectionLogType.DB:
                    break;
                default:
                    throw new Exception("Unknown LogType");
            }
        }

        public static string StripUrlForLRAP(string url)
        {
            url = url.Trim();
            var i = url.IndexOf('?');
            if (i == -1 || i == url.Length-1)
                return url;
            var query = url.Substring(i + 1).Trim();
            url = url.Substring(0, i);

            var queryBuilder = HttpUtility.ParseQueryString(query);
            queryBuilder.Remove(Consts.GUIDTag);
            queryBuilder.Remove(Consts.SessionGUIDTag);
            queryBuilder.Remove(Consts.PageGUIDTag);
            queryBuilder.Remove(Consts.BundleGUIDTag);

            query = queryBuilder.ToString();
           
            return $"{url}{(String.IsNullOrWhiteSpace(query)?"":"?")}{query}";
        }
    }
}
