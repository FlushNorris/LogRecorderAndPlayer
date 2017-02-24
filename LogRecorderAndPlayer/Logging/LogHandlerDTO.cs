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
        public Guid SessionGUID { get; set; }
        public Guid PageGUID { get; set; }
        public Guid BundleGUID { get; set; } //Bundle Ajax requests
        public Guid? ProgressGUID { get; set; } //For LogPlayer
        public DateTime Timestamp { get; set; }
        public LogType LogType { get; set; }
        public string Element { get; set; } //(#ElementId [+ TagPath], HandlerUrl)
    }

    public enum LogType
    {
        OnAjaxRequestSend=0,
        OnAjaxRequestReceived=1,
        OnAjaxResponseSend=2,
        OnAjaxResponseReceived=3,
        OnBlur=4,
        OnFocus=5
    }
}
