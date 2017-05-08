﻿using System;
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
                    if (logElement.LogType == LogType.OnPageRequest) // || logElement.LogType == LogType.OnHandlerRequestReceived)
                    {
                        var value = LoggingPage.DeserializeRequestValue(logElement);
                        var requestMethod = value.ServerVariables["REQUEST_METHOD"].ToUpper();
                        if (requestMethod == "POST")
                        {
                            //Ignore event
                        }
                    }

                    //Ignore serverside.. they are handled by the webserver.... eh, this means we do no accept new pagerequests, we should only ignore postback events
                    //Typer pagerequests:

                    //Manuel url, tryk på anchor eller redirect via js-kode: !postback (burde jeg håndtere.. og checke om en af de allerede åbne browser sub-vinduer ikke længere har events, for ellers overtag en af disse)
                    //ved html-event (f.eks. click) vil denne aktion blive udført automatisk, så hvordan undersøger jeg om et pagerequest sker af sig selv eller det er nået jeg skal ordne?
                    //Skal jeg undersøge det via tid??
                    //ved form-submit får jeg et form-submit-event (nej, form-submit-eventet er på nuværende tidspunkt undladt i loggen, da det sker automatisk som en del af et flow udfra et klik.. 
                    //                                              men jeg kunne selvfølgelig logge at det er sket og hvis ikke, så er det jo noget logplayeren skal udføre? præcis samme problem... suk)

                    //Så fremgangsmåden er måske at udvidde loggningen ikke med henblik på at afspille det igen direkte, men lade automatikken ordne det inden for rimelig tid.. dvs opsætning af logging skal også ske for LogPlayeren)
                    //  Er det opsat i dag i Play-mode?
                    //    Nu er jeg pludselig i tvivl om det er vigtigt eller ej, da f.eks. form-submit jo kun ............ om igen

                    ///////////////////////////////

                    //Manuel url, tryk på anchor eller redirect via js-kode: !postback (burde jeg håndtere.. og checke om en af de allerede åbne browser sub-vinduer ikke længere har events, for ellers overtag en af disse)
                    //unload event (burde det også blive logget til information om siden er relevant?) Den får jeg jo både ved postback og url-redirect
                    //

                    //form-post via knap eller js-kode: postback

                    /////////////////////////////////////////////////

                    // Jeg får et page-request event her.. skal vide om jeg skal åbne nyt browser-vindue, overtage et eksisterende eller intet gøre da det er et form-post event, form-post event vil ALTID være i samme vindue.
                    // REQUEST_METHOD er placeret i logElement.Value for pagerequest objektet.. "GET", "PUT", "POST", "PATCH", "DELETE"  (http://www.restapitutorial.com/lessons/httpmethods.html)
                    // Burde jo være i første event, da vi har fingrene i OnPageSessionBefore med ret stor sikkerhed.. så det burde måske være det første event?
                    //   OnPageRequest kommer nu før OnPageSession, giver mest mening.

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
