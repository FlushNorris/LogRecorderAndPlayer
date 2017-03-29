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
        }

        private NamedPipeServerResponse ServiceInstanse_OnBrowserJobComplete(NamedPipeBrowserJob namedPipeBrowserJob)
        {
            eventsTable1.SetSessionElementAsDone(namedPipeBrowserJob.SessionGUID, namedPipeBrowserJob.LogElementGUID);
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
            eventsTable1.OnPlayElement += EventsTable1_OnPlayElement;
        }

        private LogElementDTO EventsTable1_OnPlayElement(LRAPSessionElement element)
        {                       
            if (!Sessions.Any(x => x.ProcessGUID.Equals(element.LogElementInfo.SessionGUID)))
            {
                var logElementDTO = LoggingHelper.LoadElement(LRAPLogType.JSON, element.LogElementInfo);

                SpawnSession(element.LogElementInfo.SessionGUID, logElementDTO.PageGUID, logElementDTO.GUID, txtBaseUrl.Text.TrimEnd('/') + '/' + logElementDTO.Element.TrimStart('/'));
                return logElementDTO;
            }
            else
            {
                MessageBox.Show("Error: Session is already open?");
                return null;
            }
        }

        private void SpawnSession(Guid processGUID, Guid pageGUID, Guid logElementGUID, string url)
        {
            var session = new Session() { ProcessGUID = processGUID, ProcessId = -1 };

            Sessions.Add(session);

            var browserApp = ConfigurationManager.AppSettings["BrowserApp"];

            var psi = new ProcessStartInfo(browserApp, $"{ServerGUID} {session.ProcessGUID} {pageGUID} {logElementGUID} \"{url}\"");
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
