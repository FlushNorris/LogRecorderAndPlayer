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
            LoggingHelper.LogElement(new LogElementDTO(
                guid: LoggingHelper.GetInstanceGUID(context, () => Guid.NewGuid()).GetValueOrDefault(),
                sessionGUID: LoggingHelper.GetSessionGUID(context, null, () => Guid.NewGuid()).GetValueOrDefault(),
                pageGUID: LoggingHelper.GetPageGUID(context, null, () => Guid.NewGuid()).GetValueOrDefault(),
                bundleGUID: LoggingHelper.GetBundleGUID(context, () => Guid.NewGuid()).GetValueOrDefault(),
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: LogType.OnAjaxRequestReceived,
                element: LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                element2: null,
                value: SerializationHelper.SerializeNameValueCollection(context.Request.Form, SerializationType.Json),
                times: 1,
                unixTimestampEnd: null
            ));
        }

        public static void LogResponse(HttpContext context, string response)
        {
            //Need to parse info from request to response... in this case where ashx is called from a external website
            var requestContainsInstanceGuid = context.Request.Params[Consts.GUIDTag] != null;

            LoggingHelper.LogElement(new LogElementDTO(
                guid: LoggingHelper.GetInstanceGUID(context, () => Guid.NewGuid()).GetValueOrDefault(),
                sessionGUID: LoggingHelper.GetSessionGUID(context, null, () => Guid.NewGuid()).GetValueOrDefault(),
                pageGUID: LoggingHelper.GetPageGUID(context, null, () => Guid.NewGuid()).GetValueOrDefault(),
                bundleGUID: LoggingHelper.GetBundleGUID(context, () => Guid.NewGuid()).GetValueOrDefault(),
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: LogType.OnAjaxResponseSend,
                element: LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                element2: !requestContainsInstanceGuid ? SerializationHelper.SerializeNameValueCollection(context.Request.Form, SerializationType.Json) : null,
                value: response,
                times: 1,
                unixTimestampEnd: null
            ));
        }
    }
}
