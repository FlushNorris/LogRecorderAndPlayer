﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogRecorderAndPlayer
{
    public class LogHandlerDTO
    {
        public LogElementDTO[] LogElements { get; set; }
    }

    public class LogElementDTO
    {
        public Guid GUID { get; set; } //To ensure we do not have dublets, if we need to log again due to an error
        public Guid SessionGUID { get; set; }
        public Guid PageGUID { get; set; }
        public Guid? BundleGUID { get; set; } //Bundle Ajax requests
        public Guid? ProgressGUID { get; set; } //For LogPlayer
        public double UnixTimestamp { get; set; }
        public LogType LogType { get; set; }
        public string Element { get; set; } //(#ElementId [+ TagPath], HandlerUrl, PageUrl) 
        public string Element2 { get; set; } //e.g. postBackControlClientId or JSON-RequestForm for handlers which isn't connected request/response-wise by a LRAPGUID
        public string Value { get; set; }
        public int Times { get; set; }
        public double? UnixTimestampEnd { get; set; }

        public LogElementDTO() { }

        public LogElementDTO(Guid guid, Guid sessionGUID, Guid pageGUID, Guid? bundleGUID, Guid? progressGUID, double unixTimestamp, LogType logType, string element, string element2, string value, int times, double? unixTimestampEnd)
        {
            GUID = guid;
            SessionGUID = sessionGUID;
            PageGUID = pageGUID;
            BundleGUID = bundleGUID;
            ProgressGUID = progressGUID;
            UnixTimestamp = unixTimestamp;
            LogType = logType;
            Element = element;
            Element2 = element2;
            Value = value;
            Times = times;
            UnixTimestampEnd = unixTimestampEnd;
        }
    }

    public enum LogType
    {
        OnAjaxRequestSend=0,
        OnAjaxRequestReceived=1,
        OnAjaxResponseSend=2,
        OnAjaxResponseReceived=3,
        OnBlur=4,
        OnFocus=5,
        OnChange=6,
        OnSelect = 7,
        OnCopy = 8,
        OnCut = 9,
        OnPaste = 10,
        OnKeyDown = 11,
        OnKeyUp = 12,
        OnKeyPress = 13,
        OnMouseDown = 14,
        OnMouseUp = 15,
        OnClick = 16,
        OnDblClick = 17,
        OnSearch = 18,
        OnResize = 19,
        OnDragStart = 20,
        OnDragEnd = 21,
        OnDragOver = 22,
        OnDrop = 23,
        OnScroll = 24,
        OnPageRequest = 25,
        OnPageResponse = 26,
        OnWCFServiceRequest = 27,
        OnWCFServiceResponse = 28,
        OnDatabaseRequest = 29,
        OnDatabaseResponse = 30
    }
}
