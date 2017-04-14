using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LogRecorderAndPlayer
{
    public static class LoggingHandler
    {
        public static void LogRequest(HttpContext context, NameValueCollection requestForm)
        {
            LoggingHelper.LogElement(new LogElementDTO(
                guid: LoggingHelper.GetInstanceGUID(context, () => new Guid(), requestForm).GetValueOrDefault(),
                sessionGUID: LoggingHelper.GetSessionGUID(context, null, () => new Guid(), requestForm).GetValueOrDefault(),
                pageGUID: LoggingHelper.GetPageGUID(context, null, () => new Guid(), requestForm).GetValueOrDefault(),
                bundleGUID: LoggingHelper.GetBundleGUID(context, () => new Guid(), requestForm).GetValueOrDefault(),
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: LogType.OnHandlerRequestReceived,
                element: LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                element2: null,
                value: SerializationHelper.SerializeNameValueCollection(requestForm ?? context.Request.Form, SerializationType.Json),
                times: 1,
                unixTimestampEnd: null
            ));
        }

        public static string LogResponse(HttpContext context, string response)
        {
            //Need to parse info from request to response... in this case where ashx is called from a external website
            var requestContainsInstanceGuid = context.Request.Params[Consts.GUIDTag] != null;

            LoggingHelper.LogElement(new LogElementDTO(
                guid: LoggingHelper.GetInstanceGUID(context, () => new Guid()).GetValueOrDefault(),
                sessionGUID: LoggingHelper.GetSessionGUID(context, null, () => new Guid()).GetValueOrDefault(),
                pageGUID: LoggingHelper.GetPageGUID(context, null, () => new Guid()).GetValueOrDefault(),
                bundleGUID: LoggingHelper.GetBundleGUID(context, () => new Guid()).GetValueOrDefault(),
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: LogType.OnHandlerResponseSend,
                element: LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                element2: !requestContainsInstanceGuid ? SerializationHelper.SerializeNameValueCollection(context.Request.Form, SerializationType.Json) : null,
                value: response,
                times: 1,
                unixTimestampEnd: null
            ));

            return response;
        }

        public static void LogSession(HttpContext context, NameValueCollection requestForm, bool before)
        {
            var sessionValues = LoggingHelper.GetSessionValues(context);
            if (sessionValues == null)
                return;

            LoggingHelper.LogElement(new LogElementDTO(
                guid: Guid.NewGuid(),
                sessionGUID: LoggingHelper.GetSessionGUID(context, null, () => new Guid(), requestForm).Value,
                pageGUID: LoggingHelper.GetPageGUID(context, null, () => new Guid(), requestForm).Value,
                bundleGUID: null,
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: before ? LogType.OnHandlerSessionBefore : LogType.OnHandlerSessionAfter,
                element: LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                element2: null,
                value: SerializationHelper.SerializeNameValueCollection(sessionValues, SerializationType.Json),
                times: 1,
                unixTimestampEnd: null
            ));
        }
    }
}
