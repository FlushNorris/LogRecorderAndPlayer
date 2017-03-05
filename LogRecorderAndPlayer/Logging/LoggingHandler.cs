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
            LoggingHelper.LogElement(new LogElementDTO()
            {
                GUID = LoggingHelper.GetInstanceGUID(context, () => Guid.NewGuid()).GetValueOrDefault(),
                SessionGUID = LoggingHelper.GetSessionGUID(context, null, () => Guid.NewGuid()).GetValueOrDefault(),
                PageGUID = LoggingHelper.GetPageGUID(context, null, () => Guid.NewGuid()).GetValueOrDefault(),
                BundleGUID = LoggingHelper.GetBundleGUID(context, () => Guid.NewGuid()).GetValueOrDefault(),
                ProgressGUID = null,
                UnixTimestamp = TimeHelper.UnixTimestamp(),
                LogType = LogType.OnAjaxRequestReceived,
                Element = LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                Element2 = null,
                Value = SerializationHelper.SerializeNameValueCollection(context.Request.Form, SerializationType.Json)
            });
        }

        public static void LogResponse(HttpContext context, string response)
        {
            //Need to parse info from request to response... in this case where ashx is called from a external website
            var requestContainsInstanceGuid = context.Request.Params[Consts.GUIDTag] != null;

            LoggingHelper.LogElement(new LogElementDTO()
            {
                GUID = LoggingHelper.GetInstanceGUID(context, () => Guid.NewGuid()).GetValueOrDefault(),
                SessionGUID = LoggingHelper.GetSessionGUID(context, null, () => Guid.NewGuid()).GetValueOrDefault(),
                PageGUID = LoggingHelper.GetPageGUID(context, null, () => Guid.NewGuid()).GetValueOrDefault(),
                BundleGUID = LoggingHelper.GetBundleGUID(context, () => Guid.NewGuid()).GetValueOrDefault(),
                ProgressGUID = null,
                UnixTimestamp = TimeHelper.UnixTimestamp(),
                LogType = LogType.OnAjaxResponseSend,
                Element = LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                Element2 = !requestContainsInstanceGuid ? SerializationHelper.SerializeNameValueCollection(context.Request.Form, SerializationType.Json) : null,
                Value = response
            });
        }

    }
}
