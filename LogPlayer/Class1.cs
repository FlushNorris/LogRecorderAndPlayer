using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogRecorderAndPlayer;
using TestBrowser.Properties;

namespace TestBrowser
{
    public enum LRAPSessionFlowType
    {
        Server = 0,
        Client = 1
    }

    public enum SessionElementState
    {
        Waiting = 0,
        Playing = 1,
        Played = 2
    }

    public enum SessionState
    {
        WaitingForStartingElement = 0,
        Playing = 1
        //Ready = 2 //Ready to receive another event, but only when all the other events at the same time is complete
    }

    public enum EventsState
    {
        Stopped,
        Playing
    }

    public class LRAPSessionElement
    {
        public int Index { get; set; } //Index in SessionElementOrderedList (not unique)
        public SessionElementState State { get; set; }
        public LRAPSession Session { get; set; }
        public LogElementInfo LogElementInfo { get; set; }

        public string GetToolTipMessage()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Session: {Session.GUID}");
            sb.AppendLine($"Clientside: {LogElementInfo.ClientsideLogType}");
            sb.AppendLine($"LogType: {LogElementInfo.LogType}");
            return sb.ToString();
        }
        
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

        public SessionState State { get; set; }

        public LRAPSession()
        {
            Flows = new Dictionary<LRAPSessionFlowType, List<LRAPSessionElement>>();
        }
    }   

    public class EventsTable : TableLayoutPanel
    {
        public delegate void PlayElement(LogElementDTO logElement);
        public delegate LogElementDTO LoadLogElement(LRAPSessionElement element);
        public event LoadLogElement OnLoadLogElement = null;
        public event PlayElement OnPlayElement = null;

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

        private EventsState CurrentState { get; set; } = EventsState.Stopped;
        private int StartIndex { get; set; } = -1;
        private int CurrentIndex { get; set; } = -1;

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
            int orderedListIndex = 0;
            foreach (var sessionElement in lstOrderedByTimestamp)
            {
                if (lastTimestamp != null && (!lastTimestamp.Value.Equals(sessionElement.LogElementInfo.Timestamp) || lastIsClient.Value == sessionElement.LogElementInfo.ClientsideLogType))
                {
                    result.Add(curr);
                    orderedListIndex++;
                    curr = new List<LRAPSessionElement>();
                }
                lastTimestamp = sessionElement.LogElementInfo.Timestamp;
                lastIsClient = sessionElement.LogElementInfo.ClientsideLogType;
                sessionElement.Index = orderedListIndex;
                sessionElement.State = SessionElementState.Waiting;
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

        private bool IsValidStartingEvent(LRAPSessionElement sessionElement)
        {
            return sessionElement.LogElementInfo.LogType == LogType.OnPageSessionBefore;
        }

        public void Refresh()
        {
            if (this.Parent == null || Sessions == null)
                return;

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
                    this.Controls.Add(new Label() {Text = $"{session.GUID} ({LRAPSessionFlowType.Server}):", AutoSize = true }, 0, row);
                    this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
                    session.ServerRowIndex = row++;
                }

                if (session.Flows.ContainsKey(LRAPSessionFlowType.Client))
                {
                    this.Controls.Add(new Label() {Text = $"{session.GUID} ({LRAPSessionFlowType.Client}):", AutoSize = true}, 0, row);
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

                    if (IsValidStartingEvent(sessionElement))
                    {
                        var btn = new Button();
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.Margin = new Padding(0);
                        btn.Size = new System.Drawing.Size(10, 20);
                        btn.Text = "";
                        btn.Image = Resources.start;
                        btn.UseVisualStyleBackColor = true;
                        btn.Tag = sessionElement;
                        btn.MouseHover += Panel_MouseHover;
                        btn.MouseLeave += Panel_MouseLeave;
                        btn.Click += Btn_Click;
                        this.Controls.Add(btn, colIndex, rowIndex);
                    }
                    else
                    {
                        var panel = new Panel() {BackColor = GetColorByLogType(sessionElement.LogElementInfo.LogType), Margin = new Padding(0)};
                        panel.MouseHover += Panel_MouseHover;
                        panel.MouseLeave += Panel_MouseLeave;
                        panel.Tag = sessionElement;
                        this.Controls.Add(panel, colIndex, rowIndex);
                    }
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

        private void Btn_Click(object sender, EventArgs e)
        {
            if (CurrentState == EventsState.Playing)
            {
                MessageBox.Show("Already playing");
                return;
            }

            var ctrl = (Control)sender;
            var sessionElement = (LRAPSessionElement)ctrl.Tag;         
                        
            StartPlayer(sessionElement.Index);
        }

        private void StartPlayer(int index)
        {
            StartIndex = index;
            CurrentIndex = index;
            CurrentState = EventsState.Playing;

            var lst = SessionElementOrderedList[CurrentIndex];
            Sessions.ForEach(x => x.State = SessionState.WaitingForStartingElement);
            foreach (var sessionElement in lst.Where(x => IsValidStartingEvent(x))) //TODO: Check if sessionElements are able to start
            {
                sessionElement.Session.State = SessionState.Playing;
                sessionElement.State = SessionElementState.Playing;

                var logElementDTO = OnLoadLogElement?.Invoke(sessionElement);
                if (logElementDTO != null)
                {
                    sessionElement.LogElementInfo.GUID = logElementDTO.GUID;
                    OnPlayElement?.Invoke(logElementDTO);
                }
            }
        }

        private void StopPlayer()
        {
            throw new NotImplementedException();
        }

        public LogElementDTO FetchLogElement(Guid pageGUID, LogType logType, int? currentIndex = null)
        {
            currentIndex = currentIndex ?? CurrentIndex;

            if (SessionElementOrderedList.Count <= currentIndex)
                return null;

            var lst = SessionElementOrderedList[CurrentIndex];
            var sessionElement = lst.First(x => x.LogElementInfo.PageGUID.Equals(pageGUID)); //Skal være på samme linie som denne.. eller de efterfølgende "bundlede" events er ikke nødvendigvis på samme index
            switch (sessionElement.State)
            {
                case SessionElementState.Played:
                    //Look at the next event if it matches the LogType for the current PageGUID
                    return FetchLogElement(pageGUID, logType, currentIndex + 1);
                case SessionElementState.Playing:
                    if (sessionElement.LogElementInfo.LogType == logType)
                    {
                        var logElementDTO = OnLoadLogElement?.Invoke(sessionElement);
                        sessionElement.LogElementInfo.GUID = logElementDTO.GUID;
                        return logElementDTO;
                    }
                    throw new Exception("Shouldn't happen, unless the running events are in a different order now");
                case SessionElementState.Waiting:
                    throw new Exception("Shouldn't happen");
                default:
                    throw new NotImplementedException();
            }
        }

        public void SetSessionElementAsDone(Guid elementGUID) //Visse events kan godt være forud... f.eks. PageRequest-events som jo kommer i bølger, de vil naturligt nok være efter CurrentIndex
        {
            var lst = SessionElementOrderedList[CurrentIndex];

            var logElementInfo = lst.First(x => x.LogElementInfo.GUID.Equals(elementGUID)); 
            logElementInfo.State = SessionElementState.Played;

            //GetOrderedList styrer at vi kan have 2 events kørende samtidigt på en session, men kun hvis den ene er server og den anden klient. Dette lyder ikke specielt holdbart i forbindelse med threading på serversiden. (Beskriv dette i rapporten)

            //Hvordan fanden skal jeg fortælle LogPlayeren-UI'et at serverside-httpmodule/sqlcommandlrap/osv var færdig .... hov, via namedpipes selvfølgelig, jeg er vist træt

            StartNewEventsIfPossible();
        }

        private void StartNewEventsIfPossible() //REMEMBER ONLY START CLIENTSIDE EVENTS!!! Serverside-events should have time to be executed before doing anything else...
        {
            //Er jo lidt noget rod, for det første event som vil være et pagerequest event... skal ikke markeres som done, men siden skal kaldes med pageguid

            var lst = SessionElementOrderedList[CurrentIndex];
            if (lst.Where(x => x.State == SessionElementState.Playing).Any())
                return; //Still waiting for at least one element to complete

            CurrentIndex++;
            if (SessionElementOrderedList.Count <= CurrentIndex)
                return; //Completed everything!

            lst = SessionElementOrderedList[CurrentIndex];
            foreach (var sessionElement in lst.Where(x => x.Session.State == SessionState.Playing || x.Session.State == SessionState.WaitingForStartingElement && IsValidStartingEvent(x)))
            {
                sessionElement.Session.State = SessionState.Playing;
                sessionElement.State = SessionElementState.Playing;
                var logElementDTO = OnLoadLogElement?.Invoke(sessionElement);
                if (logElementDTO != null)
                {
                    sessionElement.LogElementInfo.GUID = logElementDTO.GUID;
                    OnPlayElement?.Invoke(logElementDTO);
                }
            }
        }

        private Panel HoverPanel = null;

        private void Panel_MouseLeave(object sender, EventArgs e)
        {
            var ctrl = (Control)sender;
            var sessionElement = (LRAPSessionElement)ctrl.Tag;

            if (HoverPanel != null)
            {
                this.Parent.Controls.Remove(HoverPanel);
                HoverPanel = null;
            }
        }

        private void Panel_MouseHover(object sender, EventArgs e)
        {
            var ctrl = (Control)sender;
            var sessionElement = (LRAPSessionElement)ctrl.Tag;

            if (HoverPanel != null)
            {
                this.Parent.Controls.Remove(HoverPanel);
                HoverPanel = null;
            }            

            HoverPanel = new Panel() { BackColor = Color.Beige, AutoSize = true };
            var lbl = new Label() { Text = sessionElement.GetToolTipMessage(), AutoSize = true };
            HoverPanel.Top = ctrl.Top + this.Top + 30;
            HoverPanel.Left = ctrl.Left + this.Left + 0;
            HoverPanel.Controls.Add(lbl);
            this.Parent.Controls.Add(HoverPanel);
            HoverPanel.BringToFront();
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
