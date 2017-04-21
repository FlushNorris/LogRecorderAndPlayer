using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogRecorderAndPlayer;

namespace TestBrowser
{
    public partial class Form2 : Form
    {
        private NamedPipeServer Server { get; set; } = null;
        private Guid ServerGUID { get; set; }
        private List<TransferElementSession> Sessions { get; set; } = new List<TransferElementSession>();

        public Form2()
        {
            ServerGUID = Guid.NewGuid();
            InitializeComponent();
            //MessageBox.Show($"Starting player-server {ServerGUID}");
            Server = new NamedPipeServer(ServerGUID);
            Server.ServiceInstanse.OnSyncSession += ServiceInstanse_OnSyncSession;
            Server.ServiceInstanse.OnClosingSession += ServiceInstanse_OnClosingSession;
            Server.ServiceInstanse.OnBrowserJobComplete += ServiceInstanse_OnBrowserJobComplete;
            Server.ServiceInstanse.OnFetchLogElement += ServiceInstanse_OnFetchLogElement;
        }

        private TransferElementResponse ServiceInstanse_OnFetchLogElement(TransferElementFetchLogElement fetchLogElement)
        {
            var fetchLogElementResponse = eventsTable1.FetchLogElement(fetchLogElement.PageGUID, fetchLogElement.LogType);
            return new TransferElementResponse() {Success = true, Data = fetchLogElementResponse };
        }

        private TransferElementResponse ServiceInstanse_OnBrowserJobComplete(TransferElementBrowserJob namedPipeBrowserJob) //For clientside-events
        {            
            //MessageBox.Show($"Received BrowserJobComplete {(namedPipeBrowserJob.LogElementGUID != null ? namedPipeBrowserJob.LogElementGUID.Value.ToString() : "null")}");
            Guid? logElementGUID = namedPipeBrowserJob.LogElementGUID;
            if (logElementGUID == null && !String.IsNullOrWhiteSpace(namedPipeBrowserJob.HandlerUrl) && namedPipeBrowserJob.LogType.HasValue)
            {
                var logElementDTO = eventsTable1.FetchLogElement(namedPipeBrowserJob.PageGUID, namedPipeBrowserJob.LogType.Value, namedPipeBrowserJob.HandlerUrl).LogElementDTO;
                logElementGUID = logElementDTO?.GUID;
                if (logElementGUID == null)
                    return new TransferElementResponse() {Success = false, Message = $"LogElement could not be located ({namedPipeBrowserJob.LogType.Value} : {namedPipeBrowserJob.HandlerUrl})"};
            }

            if (logElementGUID != null) //BrowserJobComplete with LogElementGUID = null means that browser has been spawned with new url
            {
                var newEvent = eventsTable1.SetSessionElementAsDone(logElementGUID.Value);
                //MessageBox.Show($"ServiceInstanse_OnBrowserJobComplete2: {namedPipeBrowserJob.LogElementGUID.Value} : {newEvent}");
                if (newEvent == EventsTable.NewEvent.None)
                {

                }
            }

            return new TransferElementResponse() { Success = true };
        }

        private TransferElementResponse ServiceInstanse_OnClosingSession(TransferElementSession namedPipeSession)
        {
            var browser = Sessions.First(x => x.ProcessGUID.Equals(namedPipeSession.ProcessGUID));
            Sessions.Remove(browser);
            return new TransferElementResponse() { Success = true };
        }

        private TransferElementResponse ServiceInstanse_OnSyncSession(TransferElementSession namedPipeSession)
        {
            var browser = Sessions.First(x => x.ProcessGUID.Equals(namedPipeSession.ProcessGUID) && x.ProcessId == -1);
            browser.ProcessId = namedPipeSession.ProcessId;                        

            return new TransferElementResponse() {Success = true};
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var browser in Sessions.Where(x => x.ProcessId != -1))
            {
                Process.GetProcessById(browser.ProcessId).Kill();
            }

            if (Server != null)
            {
                Server.Dispose();
                Server = null;
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            var elementsInfo = LoggingHelper.LoadElementsInfo(txtPath.Text.TrimEnd('\\'), LRAPLogType.JSON);            

            DoubleBuffered = true;
            eventsTable1.SetupSessions(elementsInfo.LogElementInfos);
            eventsTable1.OnLoadLogElement += EventsTable1_OnLoadLogElement;
            eventsTable1.OnPlayElement += EventsTable1_OnPlayElement;
        }

        private LogElementDTO EventsTable1_OnLoadLogElement(LRAPSessionElement element)
        {
            return LoggingHelper.LoadElement(LRAPLogType.JSON, element.LogElementInfo);            
        }

        private void EventsTable1_OnPlayElement(LogElementDTO logElement)
        {
            var session = Sessions.FirstOrDefault(x => x.ProcessGUID.Equals(logElement.SessionGUID));

            if (session == null) //TODO Check if the logElement is both the first one of a bundle... and able to spawn a new session/browser
            {
                SpawnSession(logElement.SessionGUID, logElement.PageGUID, txtBaseUrl.Text.TrimEnd('/') + '/' + logElement.Element.TrimStart('/'));
            }
            else
            {
                if (LogTypeHelper.IsClientsideUserEvent(logElement.LogType))
                {
                    NamedPipeHelper.SendBrowserJob_ASYNC(session, logElement);
                }
                else
                {
                    //Ignore serverside.. they are handled by the webserver
                }

                //TODO Spawn this a spawn session/browser event occurs more than once?!
                //MessageBox.Show("Error: Session is already open?");
            }
        }

        private void SpawnSession(Guid processGUID, Guid pageGUID, string url)
        {
            var session = new TransferElementSession() { ProcessGUID = processGUID, ProcessId = -1 };

            Sessions.Add(session);

            var browserApp = ConfigurationManager.AppSettings["BrowserApp"];

            var psi = new ProcessStartInfo(browserApp, $"{ServerGUID} {session.ProcessGUID} {pageGUID} \"{url}\"");
            Process.Start(psi);
        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                    eventsTable1.NextPage();
                    break;
                case Keys.Left:
                    eventsTable1.PrevPage();
                    break;
            }
        }

        #region .. Double Buffered function ..
        public static void SetDoubleBuffered(System.Windows.Forms.Control c)
        {
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;
            System.Reflection.PropertyInfo aProp = typeof(System.Windows.Forms.Control).GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
            aProp.SetValue(c, true, null);
        }

        #endregion

        #region .. code for Flucuring ..

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        #endregion
    }
}
