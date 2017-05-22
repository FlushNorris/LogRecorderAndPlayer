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

namespace LogPlayer
{
    public partial class MainForm : Form
    {
        private PlayerCommunicationServer Server { get; set; } = null;
        private Guid ServerGUID { get; set; }
        private List<TransferElementSession> Sessions { get; set; } = new List<TransferElementSession>();

        private List<Tuple<LogElementDTO, LogElementDTO, AdditionalData>> LogElementHistory { get; set; } = new List<Tuple<LogElementDTO, LogElementDTO, AdditionalData>>();

        public MainForm()
        {
            ServerGUID = Guid.NewGuid();
            InitializeComponent();
            //MessageBox.Show($"Starting player-server {ServerGUID}");
            Server = new PlayerCommunicationServer(ServerGUID);
            Server.ServiceInstance.OnSyncSession += ServiceInstanse_OnSyncSession;
            Server.ServiceInstance.OnClosingSession += ServiceInstanse_OnClosingSession;
            Server.ServiceInstance.OnBrowserJobComplete += ServiceInstanse_OnBrowserJobComplete;
            Server.ServiceInstance.OnFetchLogElement += ServiceInstanse_OnFetchLogElement;
            Server.ServiceInstance.OnLogElementHistory += ServiceInstance_OnLogElementHistory;
        }
       
        private TransferElementResponse ServiceInstance_OnLogElementHistory(LogElementDTO previousLogElement, LogElementDTO nextLogElement, AdditionalData additionalData)
        {
            LogElementHistory.Add(new Tuple<LogElementDTO, LogElementDTO, AdditionalData>(previousLogElement, nextLogElement, additionalData));
            return new TransferElementResponse() { Success = true, Data = null };
        }

        private TransferElementResponse ServiceInstanse_OnFetchLogElement(TransferElementFetchLogElement fetchLogElement)
        {
            var fetchLogElementResponse = eventsTable1.FetchLogElement(fetchLogElement.PageGUID, fetchLogElement.LogType);
            return new TransferElementResponse() {Success = true, Data = fetchLogElementResponse};
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

            return new TransferElementResponse() {Success = true};
        }

        private TransferElementResponse ServiceInstanse_OnClosingSession(TransferElementSession namedPipeSession)
        {
            var browser = Sessions.First(x => x.ProcessGUID.Equals(namedPipeSession.ProcessGUID));
            Sessions.Remove(browser);
            return new TransferElementResponse() {Success = true};
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
                try
                {
                    Process.GetProcessById(browser.ProcessId).Kill();
                }
                catch{}
            }

            if (Server != null)
            {
                Server.Dispose();
                Server = null;
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.AppendText($"ServerGUID = {ServerGUID}");

            //var elementsInfo = LoggingHelper.LoadElementsInfo(txtPath.Text.TrimEnd('\\'), LRAPLogType.JSON);            

            DoubleBuffered = true;
            //eventsTable1.SetupSessions(elementsInfo.LogElementInfos);
            eventsTable1.OnLoadLogElement += EventsTable1_OnLoadLogElement;
            eventsTable1.OnPlayElement += EventsTable1_OnPlayElement;
        }

        private LogElementDTO EventsTable1_OnLoadLogElement(LRAPSessionElement element)
        {
            return LoggingHelper.LoadElement(LRAPLogType.JSON, element.LogElementInfo);            
        }

        private PlayElementResponse EventsTable1_OnPlayElement(LogElementDTO logElement, bool ableToWaitForExecution)
        {
            var session = Sessions.FirstOrDefault(x => x.ProcessGUID.Equals(logElement.SessionGUID));

            if (session == null) //TODO Check if the logElement is both the first one of a bundle... and able to spawn a new session/browser
            {
                var baseUrl = txtBaseUrl.Text.TrimEnd('/');
                var url = logElement.Element.TrimStart('/');
                url = LoggingHelper.SolutionLoggingPlayer?.FinalizeUrl(LogElementHistory, baseUrl + '/' + url);
                url = url.Substring(baseUrl.Length);

                SpawnSession(logElement.SessionGUID, logElement.PageGUID, baseUrl, url);

                return PlayElementResponse.InProgress;
            }
            else
            {
                if (LogTypeHelper.IsClientsideUserEvent(logElement.LogType))
                {
                    PlayerCommunicationHelper.SendBrowserJob_ASYNC(session, logElement);
                    return PlayElementResponse.InProgress;
                }
                else
                {
                    if (logElement.LogType == LogType.OnPageRequest) // || logElement.LogType == LogType.OnHandlerRequestReceived)
                    {
                        if (ableToWaitForExecution)
                            return PlayElementResponse.WaitingToBeExecuted;

                        var baseUrl = txtBaseUrl.Text.TrimEnd('/');
                        var url = logElement.Element.TrimStart('/');
                        url = LoggingHelper.SolutionLoggingPlayer?.FinalizeUrl(LogElementHistory, baseUrl + '/' + url);
                        url = url.Substring(baseUrl.Length);
                        logElement.Element = url;

                        PlayerCommunicationHelper.SendBrowserJob_ASYNC(session, logElement);
                        return PlayElementResponse.InProgress;
                        //RequestMethods:
                        //GET:   Open new tab/window, unless logElement.PageGUID are found within the current LogSession
                        //OTHER:
                        //  Refresh: location.reload(true)
                        //  History: pt ignoreres denne sektion, selvom det er forkert... ved ikke helt præcist hvordan dette skal ordnes

                        //refresh and history-change should result in an existing pageGUID, but that pageGUID/browserWindow might in fact have been closed for some time at this point :(



                        //var value = LoggingPage.DeserializeRequestValue(logElement);
                        //var requestMethod = value.ServerVariables["REQUEST_METHOD"].ToUpper();
                        //if (requestMethod == "POST") //Cannot rely on requestMethod to be able to determine if it can be ignored from being executed by LogPlayer, because refresh/F5 is able to reproduce any requestMethod
                        //{
                        //    //Ignore event
                        //}
                    }
                    else
                    {
                        return PlayElementResponse.Ignored;
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

        private void SpawnSession(Guid processGUID, Guid pageGUID, string baseUrl, string url)
        {
            var session = new TransferElementSession() { ProcessGUID = processGUID, ProcessId = -1 };

            Sessions.Add(session);
            
            var sessionApp = ConfigurationHelper.GetConfigurationSection().SessionApp;

            var psi = new ProcessStartInfo(sessionApp, $"{ServerGUID} {session.ProcessGUID} {pageGUID} \"{baseUrl}\" \"{url}\"");
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

        private void button1_Click(object sender, EventArgs e)
        {
            var elementsInfo = LoggingHelper.LoadElementsInfo(txtPath.Text.TrimEnd('\\'), LRAPLogType.JSON);

            eventsTable1.SetupSessions(elementsInfo.LogElementInfos);
        }
    }
}
