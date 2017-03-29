using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogRecorderAndPlayer;

namespace LogBrowser
{
    public partial class MainForm : Form
    {
        private string[] Arguments { get; set; } = null;
        private Guid? ServerGUID { get; set; } //PlayerGUID
        private Guid? ProcessGUID { get; set; } //or SessionGUID        
        private List<BrowserForm> Browsers { get; set; } = null;
        private NamedPipeServer Server { get; set; } = null;

        public MainForm(string[] args) //Primary CTOR
        {
            BrowerEmulationHelper.SetBrowserFeatureControl();
            Browsers = new List<BrowserForm>();
            Arguments = args;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Guid? startingPageGUID = null;
            Guid? logElementGUID = null;
            string startingUrl = null;
            if (Arguments.Length > 2)
            {
                Guid tmp;
                if (Guid.TryParse(Arguments[0], out tmp))
                    ServerGUID = tmp;
                if (Guid.TryParse(Arguments[1], out tmp))
                    ProcessGUID = tmp;
                if (Guid.TryParse(Arguments[2], out tmp))
                    startingPageGUID = tmp;
                if (Guid.TryParse(Arguments[3], out tmp))
                    logElementGUID = tmp;
                startingUrl = Arguments[4];

            }

            if (ServerGUID == null || ProcessGUID == null || startingPageGUID == null || startingUrl == null || logElementGUID == null)
            {
                MessageBox.Show("Invalid arguments");
                Close();
                return;
            }

            //Send back process id related to guid
            SendToPlayer(NamedPipeServerRequestType.SyncSession);

            this.Text = $"Session: {ProcessGUID.Value} Page: {0}";

            Server = new NamedPipeServer(ProcessGUID.Value);
            Server.ServiceInstanse.OnBrowserJob += ServiceInstanse_OnBrowserJob;

            JumpToURL(startingPageGUID.Value, logElementGUID.Value, startingUrl);

            RefreshUI();
        }
        private void RefreshUI()
        {
            txtSession.Text = ProcessGUID.Value.ToString();
            lstPages.Items.Clear();
            foreach (var browser in Browsers)
            {
                lstPages.Items.Add($"{browser.PageGUID}: {browser.URL}");
            }
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshUI();
        }

        private NamedPipeServerResponse ServiceInstanse_OnBrowserJob(LogElementDTO logElement)
        {
            switch (logElement.LogType)
            {
                case LogType.OnPageSessionBefore: //Burde jeg slå alle disse page pagerequest-events sammen?
                    JumpToURL(logElement.PageGUID, logElement.GUID, logElement.Element);
                    break;
            }
            return new NamedPipeServerResponse() {Success = true};
        }

        private void JumpToURL(Guid pageGUID, Guid logElementGUID, string url)
        {
            var browser = Browsers.FirstOrDefault(x => x.PageGUID.Equals(pageGUID));
            if (browser == null)
            {
                browser = new BrowserForm(pageGUID, logElementGUID, url);
                browser.FormClosing += Browser_FormClosing;
                browser.OnPageLoaded += Browser_OnPageLoaded;          
                Browsers.Add(browser);
                browser.Show();
            }
        }

        private void Browser_OnPageLoaded(BrowserForm browser, Guid logElementGUID)
        {
            SendToPlayer(NamedPipeServerRequestType.BrowserJobComplete, new NamedPipeBrowserJob() {SessionGUID = ProcessGUID.Value, LogElementGUID = logElementGUID});
        }

        private void SendToPlayer(NamedPipeServerRequestType requestType, object data = null)
        {
            if (requestType == NamedPipeServerRequestType.SyncSession || requestType == NamedPipeServerRequestType.BrowserJobComplete)
            {
                data = data ?? new NamedPipeSession() {ProcessGUID = ProcessGUID.Value, ProcessId = Process.GetCurrentProcess().Id};
                
                var serverRequest = new NamedPipeServerRequest() {Type = requestType, Data = data };
                var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
                string error;
                var serverResponseJSON = NamedPipeClient.SendRequest_Threading(ServerGUID.Value, serverRequestJSON, out error);
                NamedPipeServerResponse serverResponse = null;
                if (error == null)
                    serverResponse = SerializationHelper.Deserialize<NamedPipeServerResponse>(serverResponseJSON, SerializationType.Json);

                if (error != null || !serverResponse.Success)
                {
                    MessageBox.Show($"Error occured while syncing with player ({error ?? serverResponse.Message})");
                    Close();
                }
            }
        }

        private void Browser_FormClosing(object sender, FormClosingEventArgs e)
        {
            var browser = (BrowserForm) sender;
            Browsers.Remove(browser);
            //Warning window etc etc...
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ProcessGUID == null || ServerGUID == null)
                return;

            var dialogResult = MessageBox.Show("Are you sure you want to close this session?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (dialogResult == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            var session = new NamedPipeSession() { ProcessGUID = ProcessGUID.Value, ProcessId = Process.GetCurrentProcess().Id };
            var serverRequest = new NamedPipeServerRequest() { Type = NamedPipeServerRequestType.ClosingSession, Data = session };
            var serverRequestJSON = SerializationHelper.Serialize(serverRequest, SerializationType.Json);
            string error;
            var serverResponseJSON = NamedPipeClient.SendRequest_Threading(ServerGUID.Value, serverRequestJSON, out error);
            if (error != null)
            {
                MessageBox.Show("Failed to communicate with server");
                return;
            }

            var serverResponse = SerializationHelper.Deserialize<NamedPipeServerResponse>(serverResponseJSON, SerializationType.Json);
            if (!serverResponse.Success)
            {
                e.Cancel = true;
                MessageBox.Show("Player does not allow closing the browser at this point");
            }
        }
    }
}
