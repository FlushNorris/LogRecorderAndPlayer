using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogRecorderAndPlayer;

namespace TestBrowser
{
    public partial class Form2 : Form
    {
        private NamedPipeServer Server { get; set; } = null;
        private Guid ServerGUID { get; set; }
        private List<Session> Sessions { get; set; } = new List<Session>();

        public Form2()
        {
            ServerGUID = Guid.NewGuid();
            InitializeComponent();
            Server = new NamedPipeServer(ServerGUID);
            Server.ServiceInstanse.OnSyncSession += ServiceInstanse_OnSyncSession;
            Server.ServiceInstanse.OnClosingSession += ServiceInstanse_OnClosingSession;
            Server.ServiceInstanse.OnBrowserJobComplete += ServiceInstanse_OnBrowserJobComplete;
            Server.ServiceInstanse.OnFetchLogElement += ServiceInstanse_OnFetchLogElement;
        }

        private NamedPipeServerResponse ServiceInstanse_OnFetchLogElement(NamedPipeFetchLogElement fetchLogElement)
        {
            var logElementDTO = eventsTable1.FetchLogElement(fetchLogElement.PageGUID, fetchLogElement.LogType);
            return new NamedPipeServerResponse() {Success = true, Data = logElementDTO};
        }

        private NamedPipeServerResponse ServiceInstanse_OnBrowserJobComplete(NamedPipeBrowserJob namedPipeBrowserJob)
        {
            if (namedPipeBrowserJob.LogElementGUID.HasValue) //BrowserJobComplete with LogElementGUID = null means that browser has been spawned with new url
            {
                eventsTable1.SetSessionElementAsDone(namedPipeBrowserJob.LogElementGUID.Value);
            }
            return new NamedPipeServerResponse() { Success = true };
        }

        private NamedPipeServerResponse ServiceInstanse_OnClosingSession(NamedPipeSession namedPipeSession)
        {
            var browser = Sessions.First(x => x.ProcessGUID.Equals(namedPipeSession.ProcessGUID));
            Sessions.Remove(browser);
            return new NamedPipeServerResponse() { Success = true };
        }

        private NamedPipeServerResponse ServiceInstanse_OnSyncSession(NamedPipeSession namedPipeSession)
        {
            var browser = Sessions.First(x => x.ProcessGUID.Equals(namedPipeSession.ProcessGUID) && x.ProcessId == -1);
            browser.ProcessId = namedPipeSession.ProcessId;                        

            return new NamedPipeServerResponse() {Success = true};
        }

        private void SendSessionJob(Guid processGUID)
        {
            //NamedPipeClient.SendRequest_Threading(namedPipeSession.ProcessGUID, ) //Problemet er jo lidt at spawnsession sker pga en url... 
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
//            var elementsInfo = LoggingHelper.LoadElementsInfo(@"c:\LogRecorderAndPlayerJSONBAK\02 Kundesøgningsside load", LRAPLogType.JSON);
            var elementsInfo = LoggingHelper.LoadElementsInfo(txtPath.Text.TrimEnd('\\'), LRAPLogType.JSON);            

            //Burde kunne starte fra OnPageSesssionBefore-event

            //var xx = elementsInfo.LogElementInfos.Where(x => x.LogType == LogType.OnPageSessionBefore).ToList();

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
            if (!Sessions.Any(x => x.ProcessGUID.Equals(logElement.SessionGUID))) //TODO Check if the logElement is both the first one of a bundle... and able to spawn a new session/browser
            {
                SpawnSession(logElement.SessionGUID, logElement.PageGUID, txtBaseUrl.Text.TrimEnd('/') + '/' + logElement.Element.TrimStart('/'));
            }
            else
            {
                if (LogTypeHelper.IsClientsideEvent(logElement.LogType))
                {
                    //throw new Exception("Clientside events is not implemented yet.... to be continued :)");
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
            var session = new Session() { ProcessGUID = processGUID, ProcessId = -1 };

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

    public class Session : NamedPipeSession
    {
        
    }
}
