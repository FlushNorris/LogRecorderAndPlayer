﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Page = System.Web.UI.Page;

namespace LogRecorderAndPlayer
{
    public static class LoggingPage
    {
        public static void LogRequest(HttpContext context, Page page)
        {
            var postBackControlClientId = GetPostBackControlClientId(context, page);

            LoggingHelper.LogElement(new LogHandlerDTO()
            {
                GUID = Guid.NewGuid(),
                SessionGUID = LoggingHelper.GetSessionGUID(context, page).Value,
                PageGUID = LoggingHelper.GetPageGUID(context, page).Value,
                BundleGUID = null,
                ProgressGUID = null,
                Timestamp = DateTime.Now, //TODO Look into this
                LogType = LogType.OnPageRequest,
                Element = context.Request.RawUrl,
                Element2 = postBackControlClientId,
                Value = SerializationHelper.SerializeNameValueCollection(context.Request.Form, SerializationType.Json)
            });
        }

        public static void LogResponse(HttpContext context, Page page, string response)
        {
            var postBackControlClientId = GetPostBackControlClientId(context, page);

            LoggingHelper.LogElement(new LogHandlerDTO()
            {
                GUID = Guid.NewGuid(),
                SessionGUID = LoggingHelper.GetSessionGUID(context, page).Value,
                PageGUID = LoggingHelper.GetPageGUID(context, page).Value,
                BundleGUID = null,
                ProgressGUID = null,
                Timestamp = DateTime.Now, //TODO Look into this
                LogType = LogType.OnPageResponse,
                Element = context.Request.RawUrl,
                Element2 = postBackControlClientId,
                Value = response
            });
        }

        /// <summary>
        /// Gets the ID of the post back control.
        /// 
        /// See: http://geekswithblogs.net/mahesh/archive/2006/06/27/83264.aspx
        /// </summary>
        /// <param name = "page">The page.</param>
        /// <returns></returns>
        private static string GetPostBackControlClientId(HttpContext context, Page page)
        {
            if (!page.IsPostBack)
                return string.Empty;

            Control control = null;
            // first we will check the "__EVENTTARGET" because if post back made by the controls
            // which used "_doPostBack" function also available in Request.Form collection.
            string controlName = context.Request.Params["__EVENTTARGET"];
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