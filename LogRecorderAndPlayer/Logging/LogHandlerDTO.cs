using System;
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

    public class LogElementInfo
    {
        public DateTime Timestamp { get; set; }
        public Guid SessionGUID { get; set; }
        public Guid PageGUID { get; set; }
        public LogType LogType { get; set; }

        public bool ClientsideLogType
        {
            get
            {
                switch (LogType)
                {
                    case LogType.OnHandlerRequestSend:
                    case LogType.OnHandlerResponseReceived:
                    case LogType.OnBlur:
                    case LogType.OnFocus:
                    case LogType.OnChange:
                    case LogType.OnSelect:
                    case LogType.OnCopy:
                    case LogType.OnCut:
                    case LogType.OnPaste:
                    case LogType.OnKeyDown:
                    case LogType.OnKeyUp:
                    case LogType.OnKeyPress:
                    case LogType.OnMouseDown:
                    case LogType.OnMouseUp:
                    case LogType.OnClick:
                    case LogType.OnDblClick:
                    case LogType.OnSearch:
                    case LogType.OnResize:
                    case LogType.OnDragStart:
                    case LogType.OnDragEnd:
                    case LogType.OnDragOver:
                    case LogType.OnDrop:
                    case LogType.OnScroll:
                        return true;
                    case LogType.OnHandlerRequestReceived:
                    case LogType.OnHandlerResponseSend:
                    case LogType.OnPageRequest:
                    case LogType.OnPageResponse:
                    case LogType.OnPageSessionBefore:
                    case LogType.OnPageSessionAfter:
                    case LogType.OnPageViewStateBefore:
                    case LogType.OnPageViewStateAfter:
                    case LogType.OnWCFServiceRequest:
                    case LogType.OnWCFServiceResponse:
                    case LogType.OnDatabaseRequest:
                    case LogType.OnDatabaseResponse:
                    case LogType.OnHandlerSessionBefore:
                    case LogType.OnHandlerSessionAfter:
                        return false;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }

    public class LogElementsInfo
    {
        public List<LogElementInfo> LogElementInfos { get; set; } = new List<LogElementInfo>();

        public DateTime? Start
        {
            get
            {
                if (LogElementInfos == null || !LogElementInfos.Any())
                    return null;
                return LogElementInfos.Min(x => x.Timestamp);
            }
        }
        public DateTime? End
        {
            get
            {
                if (LogElementInfos == null || LogElementInfos.Any())
                    return null;
                return LogElementInfos.Max(x => x.Timestamp);
            }
        }

    }

    public enum LogType
    {
        OnHandlerRequestSend = 0,
        OnHandlerRequestReceived = 1,
        OnHandlerResponseSend = 2,
        OnHandlerResponseReceived = 3,
        OnBlur = 4,
        OnFocus = 5,
        OnChange = 6,
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
        OnPageSessionBefore = 27,
        OnPageSessionAfter = 28,
        OnPageViewStateBefore = 29,
        OnPageViewStateAfter = 30,
        OnWCFServiceRequest = 31,
        OnWCFServiceResponse = 32,
        OnDatabaseRequest = 33,
        OnDatabaseResponse = 34,
        OnHandlerSessionBefore = 35,
        OnHandlerSessionAfter = 36
    }

    public static class ContentType //https://msdn.microsoft.com/en-us/library/ms775147.aspx
    {
        public const string TextHtml = "text/html";
    }
}
