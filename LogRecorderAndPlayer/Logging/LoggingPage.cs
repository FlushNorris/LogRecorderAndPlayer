using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Page = System.Web.UI.Page;

namespace LogRecorderAndPlayer
{
    public static class LoggingPage
    {
        public static void LogViewState(HttpContext context, Page page, bool before)
        {
            var logType = before ? LogType.OnPageViewStateBefore : LogType.OnPageViewStateAfter;
            NameValueCollection viewStateValues = LoggingHelper.GetViewStateValues(page);

            if (LoggingHelper.IsPlaying(context, requestForm:null))
            {
                var serverGUID = LoggingHelper.GetServerGUID(context, () => { throw new Exception(); }).Value;
                var pageGUID = LoggingHelper.GetPageGUID(context, page, () => { throw new Exception(); }).Value;

                if (LoggingHelper.FetchAndExecuteLogElement(serverGUID, pageGUID, logType, (logElement) =>
                {
                    if (logElement.Value != null)
                    {
                        NameValueCollection loggedViewStateValues = SerializationHelper.DeserializeNameValueCollection(logElement.Value, SerializationType.Json);
                        LoggingHelper.SetViewStateValues(page, loggedViewStateValues);
                    }
                    else if (viewStateValues != null)
                    {
                        throw new Exception("ViewState difference");
                    }

                    NamedPipeHelper.SetLogElementAsDone(serverGUID, pageGUID, logElement.GUID, jobStatus: new JobStatus() { Success = true }); //, async: false); //Non deadlock, because we would never call the webserver via namedpipe back again
                }))
                    return;
            }

            var postBackControlClientId = GetPostBackControlClientId(context, page, requestForm: null);

            LoggingHelper.LogElement(new LogElementDTO(
                guid: Guid.NewGuid(),
                sessionGUID: LoggingHelper.GetSessionGUID(context, page, () => new Guid()).Value,
                pageGUID: LoggingHelper.GetPageGUID(context, page, () => new Guid()).Value,
                bundleGUID: null,
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: logType,
                element: LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                element2: postBackControlClientId,
                value: viewStateValues != null ? SerializationHelper.SerializeNameValueCollection(viewStateValues, SerializationType.Json) : null,
                times: 1,
                unixTimestampEnd: null
            ));
        }

        public static void LogRequest(HttpContext context, Page page, NameValueCollection requestForm)
        {
            var logType = LogType.OnPageRequest;

            if (LoggingHelper.IsPlaying(context, requestForm))
            {
                var serverGUID = LoggingHelper.GetServerGUID(context, () => { throw new Exception(); }, requestForm).Value;
                var pageGUID = LoggingHelper.GetPageGUID(context, page, () => { throw new Exception(); }, requestForm).Value;

                if (LoggingHelper.FetchAndExecuteLogElement(serverGUID, pageGUID, logType, (logElement) =>
                {
                    var requestFormValues = SerializationHelper.DeserializeNameValueCollection(logElement.Value, SerializationType.Json);

                    LoggingHelper.SetRequestValues(context, requestFormValues, requestForm);

                    NamedPipeHelper.SetLogElementAsDone(serverGUID, pageGUID, logElement.GUID, new JobStatus() { Success = true }); //, async: false);
                }))
                    return;
            }

            var postBackControlClientId = GetPostBackControlClientId(context, page, requestForm);

            LoggingHelper.LogElement(new LogElementDTO(
                guid: Guid.NewGuid(),
                sessionGUID: LoggingHelper.GetSessionGUID(context, page, () => new Guid(), requestForm).Value,
                pageGUID: LoggingHelper.GetPageGUID(context, page, () => new Guid(), requestForm).Value,
                bundleGUID: null,
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: logType,
                element: LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                element2: postBackControlClientId,
                value: SerializationHelper.SerializeNameValueCollection(requestForm ?? context.Request.Form, SerializationType.Json),
                times: 1,
                unixTimestampEnd: null
            ));
        }        

        public static void LogSession(HttpContext context, Page page, NameValueCollection requestForm, bool before)
        {
            var logType = before ? LogType.OnPageSessionBefore : LogType.OnPageSessionAfter;
            NameValueCollection sessionValues = LoggingHelper.GetSessionValues(page);

            if (LoggingHelper.IsPlaying(context, requestForm))
            {
                var serverGUID = LoggingHelper.GetServerGUID(context, () => { throw new Exception(); }, requestForm).Value;
                var pageGUID = LoggingHelper.GetPageGUID(context, page, () => { throw new Exception(); }, requestForm).Value;

                if (LoggingHelper.FetchAndExecuteLogElement(serverGUID, pageGUID, logType, (logElement) =>
                {
                    if (logElement.Value != null)
                    {
                        NameValueCollection loggedSessionValues = SerializationHelper.DeserializeNameValueCollection(logElement.Value, SerializationType.Json);
                        LoggingHelper.SetSessionValues(page, loggedSessionValues);
                    }
                    else if (sessionValues != null)
                    {
                        throw new Exception("Session difference");
                    }

                    NamedPipeHelper.SetLogElementAsDone(serverGUID, pageGUID, logElement.GUID, new JobStatus() { Success = true }); //, async: false);
                }))
                    return;
            }

            var postBackControlClientId = GetPostBackControlClientId(context, page, requestForm);
           
            LoggingHelper.LogElement(new LogElementDTO(
                guid: Guid.NewGuid(),
                sessionGUID: LoggingHelper.GetSessionGUID(context, page, () => new Guid(), requestForm).Value,
                pageGUID: LoggingHelper.GetPageGUID(context, page, () => new Guid(), requestForm).Value,
                bundleGUID: null,
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: logType,
                element: LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                element2: postBackControlClientId,
                value: sessionValues != null ? SerializationHelper.SerializeNameValueCollection(sessionValues, SerializationType.Json) : null,
                times: 1,
                unixTimestampEnd: null
            ));
        }

        public static string LogResponse(HttpContext context, Page page, string response)
        {
            var logType = LogType.OnPageResponse;

            if (LoggingHelper.IsPlaying(context, requestForm:null))
            {
                var serverGUID = LoggingHelper.GetServerGUID(context, () => { throw new Exception(); }).Value;
                var pageGUID = LoggingHelper.GetPageGUID(context, page, () => { throw new Exception(); }).Value;

                string newResponse = null;
                if (LoggingHelper.FetchAndExecuteLogElement(serverGUID, pageGUID, logType, (logElement) =>
                {
                    //Skal vel bare replace viewstate med den fra response... oooog... hmm, ja burde jo være fint nok at diverse lrap-værdier er i response
                    var responseViewState = WebHelper.GetResponseViewState(response);
                    newResponse = WebHelper.SetResponseViewState(logElement.Value, responseViewState);

                    context.Response.Clear();
                    context.Response.Write(newResponse);

                    NamedPipeHelper.SetLogElementAsDone(serverGUID, pageGUID, logElement.GUID, new JobStatus() {Success = true}); //, async: false);
                }))
                    return newResponse;
            }

            var postBackControlClientId = GetPostBackControlClientId(context, page, requestForm: null);

            LoggingHelper.LogElement(new LogElementDTO(
                guid: Guid.NewGuid(),
                sessionGUID: LoggingHelper.GetSessionGUID(context, page, () => new Guid()).Value,
                pageGUID: LoggingHelper.GetPageGUID(context, page, () => new Guid()).Value,
                bundleGUID: null,
                progressGUID: null,
                unixTimestamp: TimeHelper.UnixTimestamp(),
                logType: logType,
                element: LoggingHelper.StripUrlForLRAP(context.Request.RawUrl),
                element2: postBackControlClientId,
                value: response,
                times: 1,
                unixTimestampEnd: null
            ));

            return response;
        }

        /// <summary>
        /// Gets the ID of the post back control.
        /// 
        /// See: http://geekswithblogs.net/mahesh/archive/2006/06/27/83264.aspx
        /// </summary>
        /// <param name = "page">The page.</param>
        /// <returns></returns>
        private static string GetPostBackControlClientId(HttpContext context, Page page, NameValueCollection requestForm)
        {
            if (page == null || !page.IsPostBack)
                return string.Empty;

            Control control = null;
            // first we will check the "__EVENTTARGET" because if post back made by the controls
            // which used "_doPostBack" function also available in Request.Form collection.
            var requestParams = requestForm != null ? WebHelper.ParamsWithSpecialRequestForm(context, requestForm) : context.Request?.Params;
            string controlName = requestParams["__EVENTTARGET"];
            if (!String.IsNullOrEmpty(controlName))
            {
                control = page.FindControl(controlName);
            }
            else
            {
                // if __EVENTTARGET is null, the control is a button type and we need to
                // iterate over the form collection to find it

                // ReSharper disable TooWideLocalVariableScope
                string controlId;
                Control foundControl;
                // ReSharper restore TooWideLocalVariableScope

                foreach (string ctl in context.Request.Form)
                {
                    // handle ImageButton they having an additional "quasi-property" 
                    // in their Id which identifies mouse x and y coordinates
                    if (ctl.EndsWith(".x") || ctl.EndsWith(".y"))
                    {
                        controlId = ctl.Substring(0, ctl.Length - 2);
                        foundControl = page.FindControl(controlId);
                    }
                    else
                    {
                        foundControl = page.FindControl(ctl);
                    }

                    if (!(foundControl is IButtonControl)) continue;

                    control = foundControl;
                    break;
                }
            }

            return control == null ? String.Empty : control.ClientID;
        }
    }
}
