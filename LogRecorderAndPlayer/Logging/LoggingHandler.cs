﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
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
            var logType = LogType.OnHandlerRequestReceived;
            RequestParams requestParams = WebHelper.BuildRequestParams(context, requestForm);

            var newLogElement = new LogElementDTO(
                guid: LoggingHelper.GetInstanceGUID(context, () => new Guid(), requestForm).GetValueOrDefault(),
                sessionGUID: LoggingHelper.GetSessionGUID(context, null, () => new Guid(), requestForm).GetValueOrDefault(),
                pageGUID: LoggingHelper.GetPageGUID(context, null, () => new Guid(), requestForm).GetValueOrDefault(),
                bundleGUID: LoggingHelper.GetBundleGUID(context, () => new Guid(), requestForm).GetValueOrDefault(),
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: logType,
                element: LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                element2: null,
                value: SerializationHelper.Serialize(requestParams, SerializationType.Json),
                times: 1,
                unixTimestampEnd: null,
                instanceTime: DateTime.Now,
                stackTrace: null
            );

            if (LoggingHelper.IsPlaying(context, requestForm))
            {
                var serverGUID = LoggingHelper.GetServerGUID(context, () => { throw new Exception(); }, requestForm).Value;
                var sessionGUID = LoggingHelper.GetSessionGUID(context, null, null, requestForm);
                var pageGUID = LoggingHelper.GetPageGUID(context, null, null, requestForm);

                if (LoggingHelper.FetchAndExecuteLogElement(serverGUID, sessionGUID, pageGUID, logType, (logElement) =>
                {
                    TimeHelper.SetNow(context, logElement.InstanceTime);

                    //                    var requestFormValues = SerializationHelper.DeserializeNameValueCollection(logElement.Value, SerializationType.Json);
                    requestParams = SerializationHelper.Deserialize<RequestParams>(logElement.Value, SerializationType.Json);

                    var headers = context?.Request?.Headers;
                    if (headers != null)
                    {
                        var json1 = SerializationHelper.Serialize(newLogElement, SerializationType.Json);
                        var xxx = SerializationHelper.Deserialize<LogElementDTO>(json1, SerializationType.Json);
                        var what = TimeHelper.UnixTimestamp(xxx.InstanceTime).ToString(CultureInfo.InvariantCulture);

                        headers[Consts.NowTimestampTag] = TimeHelper.UnixTimestamp(logElement.InstanceTime).ToString(CultureInfo.InvariantCulture);
                    }
                    //LoggingHelper.SetRequestValues(context, requestParams.Form, requestForm);

                    PlayerCommunicationHelper.SetLogElementAsDone(serverGUID, sessionGUID, pageGUID, logElement.GUID, new JobStatus() { Success = true }); //, async: false);
                }))
                    return;
            }

            LoggingHelper.LogElement(newLogElement);
        }

        public static string LogResponse(HttpContext context, string response)
        {
            var logType = LogType.OnHandlerResponseSend;

            //Need to parse info from request to response... in this case where ashx is called from a external website
            var requestContainsInstanceGuid = context.Request.Params[Consts.GUIDTag] != null;

            var newLogElement = new LogElementDTO(
                guid: LoggingHelper.GetInstanceGUID(context, () => new Guid()).GetValueOrDefault(),
                sessionGUID: LoggingHelper.GetSessionGUID(context, null, () => new Guid()).GetValueOrDefault(),
                pageGUID: LoggingHelper.GetPageGUID(context, null, () => new Guid()).GetValueOrDefault(),
                bundleGUID: LoggingHelper.GetBundleGUID(context, () => new Guid()).GetValueOrDefault(),
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: logType,
                element: LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                element2: !requestContainsInstanceGuid ? SerializationHelper.SerializeNameValueCollection(context.Request.Form, SerializationType.Json) : null,
                value: response,
                times: 1,
                unixTimestampEnd: null,
                instanceTime: DateTime.Now,
                stackTrace: null
            );

            if (LoggingHelper.IsPlaying(context, requestForm: null))
            {
                var serverGUID = LoggingHelper.GetServerGUID(context, () => { throw new Exception(); }).Value;
                var sessionGUID = LoggingHelper.GetSessionGUID(context, null, null);
                var pageGUID = LoggingHelper.GetPageGUID(context, null, null);

                string newResponse = null;

                if (LoggingHelper.FetchAndExecuteLogElement(serverGUID, sessionGUID, pageGUID, logType, (logElement) =>
                {
                    TimeHelper.SetNow(context, logElement.InstanceTime);

                    newResponse = response;
                    //newResponse = logElement.Value;

                    //context.Response.Clear();
                    //context.Response.Write(newResponse);

                    PlayerCommunicationHelper.SetLogElementAsDone(serverGUID, sessionGUID, pageGUID, logElement.GUID, new JobStatus() { Success = true }); //, async: false);
                }))
                    return newResponse;
            }

            LoggingHelper.LogElement(newLogElement);

            return response;
        }

        public static void LogSession(HttpContext context, NameValueCollection requestForm, bool before)
        {
            var logType = before ? LogType.OnHandlerSessionBefore : LogType.OnHandlerSessionAfter;
            var sessionValues = LoggingHelper.GetSessionValues(context);

            var newLogElement = new LogElementDTO(
                guid: Guid.NewGuid(),
                sessionGUID: LoggingHelper.GetSessionGUID(context, null, () => new Guid(), requestForm).Value,
                pageGUID: LoggingHelper.GetPageGUID(context, null, () => new Guid(), requestForm).Value,
                bundleGUID: null,
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: logType,
                element: LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                element2: null,
                value: sessionValues != null ? SerializationHelper.SerializeNameValueCollection(sessionValues, SerializationType.Json) : null,
                times: 1,
                unixTimestampEnd: null,
                instanceTime: DateTime.Now,
                stackTrace: null
            );

            if (LoggingHelper.IsPlaying(context, requestForm))
            {
                var serverGUID = LoggingHelper.GetServerGUID(context, () => { throw new Exception(); }, requestForm).Value;
                var sessionGUID = LoggingHelper.GetSessionGUID(context, null, null, requestForm);
                var pageGUID = LoggingHelper.GetPageGUID(context, null, null, requestForm);

                if (LoggingHelper.FetchAndExecuteLogElement(serverGUID, sessionGUID, pageGUID, logType, (logElement) =>
                {
                    TimeHelper.SetNow(context, logElement.InstanceTime);

                    if (logElement.Value != null)
                    {
                        var loggedSessionValues = SerializationHelper.DeserializeNameValueCollection(logElement.Value, SerializationType.Json);
                        LoggingHelper.SetSessionValues(context, loggedSessionValues);
                    }
                    else if (sessionValues != null)
                    {
                        throw new Exception("Session difference");
                    }

                    PlayerCommunicationHelper.SetLogElementAsDone(serverGUID, sessionGUID, pageGUID, logElement.GUID, new JobStatus() { Success = true }); //, async: false);
                }))
                    return;
            }
            
            LoggingHelper.LogElement(newLogElement);
        }
    }
}
