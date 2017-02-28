using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LogRecorderAndPlayer
{
    public static class LoggingHandler
    {
        public static void LogRequest(HttpContext context)
        {
            var requestGUID = context.Request.Params[Consts.GUIDTag];
            var guid = new Guid(requestGUID ?? Guid.NewGuid().ToString());
            var sessionGUID = new Guid(context.Request.Params[Consts.SessionGUIDTag] ?? Guid.NewGuid().ToString());
            var pageGUID = new Guid(context.Request.Params[Consts.PageGUIDTag] ?? Guid.NewGuid().ToString());
            var requestBundleGUID = context.Request.Params[Consts.BundleGUIDTag];
            Guid? bundleGUID = requestBundleGUID != null ? (Guid?)(new Guid(requestBundleGUID)) : null;            

            LoggingHelper.LogElement(new LogHandlerDTO()
            {
                GUID = guid,
                SessionGUID = sessionGUID,
                PageGUID = pageGUID,
                BundleGUID = bundleGUID,
                ProgressGUID = null,
                Timestamp = DateTime.Now, //TODO Look into this
                LogType = LogType.OnAjaxRequestReceived,
                Element = LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                Element2 = null,
                Value = SerializationHelper.SerializeNameValueCollection(context.Request.Form, SerializationType.Json)
            });
        }

        public static void LogResponse(HttpContext context, string response)
        {
            //Need to parse info from request to response... in this case where ashx is called from a external website
            var requestGUID = context.Request.Params[Consts.GUIDTag];
            var guid = new Guid(requestGUID ?? Guid.NewGuid().ToString());
            var sessionGUID = new Guid(context.Request.Params[Consts.SessionGUIDTag] ?? Guid.NewGuid().ToString());
            var pageGUID = new Guid(context.Request.Params[Consts.PageGUIDTag] ?? Guid.NewGuid().ToString());
            var requestBundleGUID = context.Request.Params[Consts.BundleGUIDTag];
            Guid? bundleGUID = requestBundleGUID != null ? (Guid?)(new Guid(requestBundleGUID)) : null;

            LoggingHelper.LogElement(new LogHandlerDTO()
            {
                GUID = guid,
                SessionGUID = sessionGUID,
                PageGUID = pageGUID,
                BundleGUID = bundleGUID,
                ProgressGUID = null,
                Timestamp = DateTime.Now, //TODO Look into this
                LogType = LogType.OnAjaxResponseSend,
                Element = LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                Element2 = requestGUID == null ? SerializationHelper.SerializeNameValueCollection(context.Request.Form, SerializationType.Json) : null,
                Value = response
            });
        }

    }
}
