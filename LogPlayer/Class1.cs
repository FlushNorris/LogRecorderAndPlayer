using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogRecorderAndPlayer;

namespace TestBrowser
{
    public enum LRAPSessionFlowType
    {
        Server = 0,
        Client = 1
    }

    public class LRAPSessionElement
    {
        public LRAPSession Session { get; set; }
        public LogElementInfo LogElementInfo { get; set; }
        
        public LRAPSessionElement(LRAPSession session, LogElementInfo logElementInfo)
        {
            this.Session = session;
            this.LogElementInfo = logElementInfo;
        }
    }


    //[Serializable]
    public class LRAPSession
    {        
        public Guid GUID { get; set; }
        public Dictionary<LRAPSessionFlowType, List<LRAPSessionElement>> Flows { get; set; }

        public int ServerRowIndex { get; set; }
        public int ClientRowIndex { get; set; }

        public LRAPSession()
        {
            Flows = new Dictionary<LRAPSessionFlowType, List<LRAPSessionElement>>();
        }
    }

    public class EventsTable : TableLayoutPanel
    {
        private int pageIndex = 1;
        private int pageElements = 100;

        public int PageIndex
        {
            get
            {
                return pageIndex;
            }
            set
            {
                pageIndex = value;
                Refresh();
            }
        }

        public int PageElements
        {
            get
            {
                return pageElements;
            }
            set
            {
                pageElements = value;
                Refresh();
            }
        }

        public int PageCount
        {
            get { return (SessionElementOrderedList.Count/PageElements) + 1; }
        }

        public void NextPage()
        {
            if (PageIndex < PageCount - 1)
            {
                PageIndex++;
            }
        }

        public void PrevPage()
        {
            if (PageIndex > 0)
            {
                PageIndex--;
            }
        }

        public EventsTable()
        {            
            this.DoubleBuffered = true;
        }

        private List<LRAPSession> Sessions { get; set; } = null;

        private List<List<LRAPSessionElement>> SessionElementOrderedList { get; set; } = null;

        private void BuildSessions(List<LogElementInfo> logElementInfos)
        {
            var dictSessions = new Dictionary<Guid, LRAPSession>();
            
            foreach (var logElementInfo in logElementInfos)
            {
                LRAPSession session = null;
                if (!dictSessions.TryGetValue(logElementInfo.SessionGUID, out session))
                {
                    session = new LRAPSession() { GUID = logElementInfo.SessionGUID };
                    dictSessions.Add(logElementInfo.SessionGUID, session);
                }
                List<LRAPSessionElement> flows = null;

                var flowKey = logElementInfo.ClientsideLogType ? LRAPSessionFlowType.Client : LRAPSessionFlowType.Server;
                if (!session.Flows.TryGetValue(flowKey, out flows))
                {
                    flows = new List<LRAPSessionElement>();
                    session.Flows.Add(flowKey, flows);
                }

                flows.Add(new LRAPSessionElement(session, logElementInfo));
            }

            Sessions = dictSessions.Values.ToList();
            SessionElementOrderedList = GetOrderedList(Sessions);
        }

        private List<List<LRAPSessionElement>> GetOrderedList(List<LRAPSession> sessions)
        {
            List<LRAPSessionElement> lstOrderedByTimestamp = new List<LRAPSessionElement>();

            foreach (var session in sessions)
            {
                foreach (var flowValues in session.Flows.Values)
                {
                    //Har brug for at gemme et columnIndex per LogElementInfo, men burde jo være en liste man enten appender til eller updatere den eksisterende column med den ekstra LogElementInfo

                    foreach (var sessionElement in flowValues)
                    {
                        //if (!CheckIfOrdered(lstOrderedByTimestamp))
                        //    throw new Exception("GRRR");

                        var idx = BinaryFindClosestIndex(lstOrderedByTimestamp, sessionElement.LogElementInfo.Timestamp);
                        if (idx == -1)
                            lstOrderedByTimestamp.Add(sessionElement);
                        else
                        {
                            var otherSessionElement = lstOrderedByTimestamp[idx];
                            if (otherSessionElement.LogElementInfo.Timestamp > sessionElement.LogElementInfo.Timestamp)
                                lstOrderedByTimestamp.Insert(idx, sessionElement);
                            else
                            {
                                if (idx == lstOrderedByTimestamp.Count - 1)
                                    lstOrderedByTimestamp.Add(sessionElement);
                                else
                                    lstOrderedByTimestamp.Insert(idx + 1, sessionElement);
                            }
                        }
                    }
                }
            }
            DateTime? lastTimestamp = null;
            bool? lastIsClient = null;
            
            List<List<LRAPSessionElement>> result = new List<List<LRAPSessionElement>>();
            List<LRAPSessionElement> curr = new List<LRAPSessionElement>();
            foreach (var sessionElement in lstOrderedByTimestamp)
            {
                if (lastTimestamp != null && (!lastTimestamp.Value.Equals(sessionElement.LogElementInfo.Timestamp) || lastIsClient.Value == sessionElement.LogElementInfo.ClientsideLogType))
                {
                    result.Add(curr);
                    curr = new List<LRAPSessionElement>();
                }
                lastTimestamp = sessionElement.LogElementInfo.Timestamp;
                lastIsClient = sessionElement.LogElementInfo.ClientsideLogType;
                curr.Add(sessionElement);
            }

            return result;
        }

        public void SetupSessions(List<LogElementInfo> logElementInfos)
        {
            BuildSessions(logElementInfos);
            //Husk at collapse de sessions der er afsluttet før en anden starter.... udskudt, da vi ikke i test vil få så mange sessioner

            //Include legends

            Refresh();
        }

        //zoom level?        

        private int BinaryFindClosestIndex(List<LRAPSessionElement> orderedLst, DateTime timestamp, int? start = null, int? end = null, long smallestTicksSoFar = long.MaxValue)
        {
            start = start ?? 0;
            end = end ?? orderedLst.Count - 1;
            if (start > end) //empty list
                return -1;
            var m = (end.Value + start.Value)/2;

            var newTicksPrev = long.MaxValue;
            var newTicksNext = long.MaxValue;
            if (m > 0)
                newTicksPrev = Math.Abs(orderedLst[m - 1].LogElementInfo.Timestamp.Ticks - timestamp.Ticks);
            if (m < orderedLst.Count - 1)
                newTicksNext = Math.Abs(orderedLst[m + 1].LogElementInfo.Timestamp.Ticks - timestamp.Ticks);
            var newTicks0 = Math.Abs(orderedLst[m].LogElementInfo.Timestamp.Ticks - timestamp.Ticks);
            if (newTicks0 < newTicksNext) 
            {
                if (newTicks0 < newTicksPrev || start == m) //Found the smallest in an ordered list
                    return m;
                return BinaryFindClosestIndex(orderedLst, timestamp, start, m - 1, newTicks0);
            }
            if (newTicks0 <= newTicksPrev) //Go right
            {
                if (end == m)
                    return m;
                return BinaryFindClosestIndex(orderedLst, timestamp, m + 1, end, newTicks0);
            }
            
            //newTicks0>=newTicksNext && newTicks0>=newTicksPrev
            //should not happen in an ordered list
            throw new Exception("Are you sure this is an ordered list?");
        }       

        public void Refresh()
        {
            //Skal anvende minimum distance til at tegne resten

            //this.Visible = false;
            //this.SuspendLayout();
            this.Parent.SuspendLayout();
            this.RowCount = Sessions.SelectMany(x => x.Flows).Count()+1; //plus footer for taking last space            
            
            this.Controls.Clear();

            var row = 0;
            this.RowStyles.Clear();
            foreach (var session in Sessions)
            {
                if (session.Flows.ContainsKey(LRAPSessionFlowType.Server))
                {
                    this.Controls.Add(new Label() {Text = $"{session.GUID} ({LRAPSessionFlowType.Server}):"}, 0, row);
                    this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
                    session.ServerRowIndex = row++;
                }

                if (session.Flows.ContainsKey(LRAPSessionFlowType.Client))
                {
                    this.Controls.Add(new Label() {Text = $"{session.GUID} ({LRAPSessionFlowType.Client}):"}, 0, row);
                    this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
                    session.ClientRowIndex = row++;
                }
            }

            this.ColumnStyles.Clear();
            this.ColumnStyles.Add(new ColumnStyle()); //autosize for titles

            //PageIndex = 3 => 2
            //var numberOfPages = (SessionElementOrderedList.Count/PageElements) + 1;
            //SessionElementOrderedList.Count

            var numOfElements = SessionElementOrderedList.Count > (PageIndex+1)*PageElements ? PageElements : SessionElementOrderedList.Count%PageElements;
            this.ColumnCount = numOfElements + 1/*title*/ + 1/*fillout*/;

            for (int c = 0; c < numOfElements; c++)
            {
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 10));
            }

            int startIdx = PageElements*PageIndex;
            int endIdx = startIdx + numOfElements - 1;
            int colorIdx = 0;
            int colIndex = 1; //skip title
            for(var idx = startIdx;idx<=endIdx;idx++)
            {
                var sessionElementsAtPosition = SessionElementOrderedList[idx];
                foreach (var sessionElement in sessionElementsAtPosition)
                {
                    var rowIndex = sessionElement.LogElementInfo.ClientsideLogType ? sessionElement.Session.ClientRowIndex : sessionElement.Session.ServerRowIndex;                    
                    var panel = new Panel() { BackColor = GetColorByLogType(sessionElement.LogElementInfo.LogType), Margin = new Padding(0) };
                    panel.Click += Panel_Click;
                    panel.Tag = sessionElement;
                    this.Controls.Add(panel, colIndex, rowIndex);
                }
                colIndex++;
            }

            this.RowStyles.Add(new RowStyle()); //Fill out bottom
            this.ColumnStyles.Add(new ColumnStyle()); //Fill out the right

            this.AutoSize = true;
            this.Parent.ResumeLayout();
            //this.ResumeLayout();
            //this.Visible = true;
        }

        private bool CheckIfOrdered(List<LRAPSessionElement> lstOrderedByTimestamp)
        {
            if (lstOrderedByTimestamp.Count == 0)
                return true;

            var last = lstOrderedByTimestamp[0];
            for (int i = 1; i < lstOrderedByTimestamp.Count; i++)
            {
                var curr = lstOrderedByTimestamp[i];
                if (last.LogElementInfo.Timestamp > curr.LogElementInfo.Timestamp)
                    return false;
            }

            return true;
        }

        private void Panel_Click(object sender, EventArgs e)
        {
            var panel = (Panel) sender;
            var sessionElement = (LRAPSessionElement) panel.Tag;
            Console.WriteLine($"{sessionElement.LogElementInfo.LogType} : {sessionElement.LogElementInfo.Timestamp.ToString("yyyyMMddHHmmssfff")}");
        }

        private Color GetColorByLogType(LogType logType)
        {
            switch (logType)
            {
                case LogType.OnHandlerRequestSend:
                    return Color.Red;
                case LogType.OnHandlerResponseReceived:
                    return Color.LightCoral;
                case LogType.OnBlur:
                    return Color.Blue;
                case LogType.OnFocus:
                    return Color.LightBlue;
                case LogType.OnChange:
                    return Color.Orange;
                case LogType.OnSelect:
                    return Color.LightSalmon;
                case LogType.OnCopy:
                    return Color.Gray;
                case LogType.OnCut:
                    return Color.Gray;
                case LogType.OnPaste:
                    return Color.Gray;
                case LogType.OnKeyDown:
                    return Color.DarkGreen;
                case LogType.OnKeyUp:
                    return Color.LightGreen;
                case LogType.OnKeyPress:
                    return Color.ForestGreen;
                case LogType.OnMouseDown:
                    return Color.SaddleBrown;
                case LogType.OnMouseUp:
                    return Color.SandyBrown;
                case LogType.OnClick:
                    return Color.Brown;
                case LogType.OnDblClick:
                    return Color.RosyBrown;
                case LogType.OnSearch:                    
                case LogType.OnResize:
                case LogType.OnDragStart:
                case LogType.OnDragEnd:
                case LogType.OnDragOver:
                case LogType.OnDrop:
                    return Color.White;
                case LogType.OnScroll:
                    return Color.LightCyan;
                //Server
                case LogType.OnHandlerRequestReceived:
                    return Color.Red;
                case LogType.OnHandlerResponseSend:
                    return Color.DarkRed;
                case LogType.OnPageRequest:
                case LogType.OnPageSessionBefore:
                case LogType.OnPageViewStateBefore:
                    return Color.Blue;
                case LogType.OnPageResponse:
                case LogType.OnPageSessionAfter:
                case LogType.OnPageViewStateAfter:
                    return Color.DarkBlue;
                case LogType.OnWCFServiceRequest:
                    return Color.Green;
                case LogType.OnWCFServiceResponse:
                    return Color.DarkGreen;
                case LogType.OnDatabaseRequest:
                    return Color.Yellow;
                case LogType.OnDatabaseResponse:
                    return Color.YellowGreen;
                case LogType.OnHandlerSessionBefore:
                    return Color.Purple;
                case LogType.OnHandlerSessionAfter:
                    return Color.MediumOrchid;
                default:
                    throw new NotImplementedException();
            }
        }       
    }
}
