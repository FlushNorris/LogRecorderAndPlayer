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
            string startingUrl = null;
            if (Arguments.Length > 3)
            {
                Guid tmp;
                if (Guid.TryParse(Arguments[0], out tmp))
                    ServerGUID = tmp;
                if (Guid.TryParse(Arguments[1], out tmp))
                    ProcessGUID = tmp;
                if (Guid.TryParse(Arguments[2], out tmp))
                    startingPageGUID = tmp;
                startingUrl = Arguments[3];
            }

            MessageBox.Show($"ServerGUID = {ServerGUID}");

            if (ServerGUID == null || ProcessGUID == null || startingPageGUID == null || startingUrl == null)
            {
                MessageBox.Show("Invalid arguments");
                Close();
                return;
            }

            //Send back process id related to guid
            NamedPipeHelper.SendSyncSession(ServerGUID.Value, ProcessGUID.Value, Process.GetCurrentProcess().Id);

            this.Text = $"Session: {ProcessGUID.Value} Page: {0}";

            //MessageBox.Show($"Starting session-server {ProcessGUID.Value}");
            Server = new NamedPipeServer(ProcessGUID.Value);
            Server.ServiceInstanse.OnBrowserJob += ServiceInstanse_OnBrowserJob;

            JumpToURL(startingPageGUID.Value, startingUrl);

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
                case LogType.OnPageSessionBefore: //Burde jeg slå alle disse page pagerequest-events sammen?... nej, da der kan være event kald imellem eventsene
                    JumpToURL(logElement.PageGUID, logElement.Element);
                    break;
                case LogType.OnResize:
                    var browserResize = SerializationHelper.Deserialize<BrowserResize>(logElement.Value, SerializationType.Json);
                    FindBrowserAndExec(logElement.PageGUID, x => x.ResizeBrowser(browserResize, logElement.GUID));
                    break;
                case LogType.OnScroll:
                    var browserScroll = SerializationHelper.Deserialize<BrowserScroll>(logElement.Value, SerializationType.Json);
                    FindBrowserAndExec(logElement.PageGUID, x => x.ScrollBrowser(browserScroll, logElement.GUID));
                    break;
                case LogType.OnMouseDown:
                    var browserMouseDown = SerializationHelper.Deserialize<BrowserMouseDown>(logElement.Value, SerializationType.Json);
                    browserMouseDown.element = logElement.Element;
                    FindBrowserAndExec(logElement.PageGUID, x => x.MouseDownBrowser(browserMouseDown, logElement.GUID));
                    break;
                case LogType.OnFocus:
                    MessageBox.Show("OnFocus");
                    var browserFocus = SerializationHelper.Deserialize<BrowserFocus>(logElement.Value, SerializationType.Json);
                    browserFocus.element = logElement.Element;
                    FindBrowserAndExec(logElement.PageGUID, x => x.FocusBrowser(browserFocus, logElement.GUID));
                    break;
                default:
                    MessageBox.Show($"Supportere ikke {logElement.LogType} endnu...");
                    throw new Exception($"Supportere ikke {logElement.LogType} endnu...");
                    break;
            }

            return new NamedPipeServerResponse() {Success = true};
        }

        private void FindBrowserAndExec(Guid pageGUID, Action<BrowserForm> fn)
        {
            var browser = Browsers.FirstOrDefault(x => x.PageGUID.Equals(pageGUID));
            if (browser == null)
                throw new Exception($"Error: Browser({pageGUID}) is not found");

            fn(browser);
        }

        private void JumpToURL(Guid pageGUID, string url)
        {
            var browser = Browsers.FirstOrDefault(x => x.PageGUID.Equals(pageGUID));
            if (browser == null)
            {                
                browser = new BrowserForm(ServerGUID.Value, ProcessGUID.Value, pageGUID, url);
                browser.FormClosing += Browser_FormClosing;
                browser.OnJobCompleted += Browser_OnJobCompleted;          
                Browsers.Add(browser);
                browser.Show();
            }
        }

        private void Browser_OnJobCompleted(BrowserForm browser, Guid? logElementGUID)
        {
            MessageBox.Show("Send BrowserJobComplete to player");
            NamedPipeHelper.SetLogElementAsDone(ServerGUID.Value, browser.PageGUID, logElementGUID, async: false); 

//            NamedPipeHelper.SendBrowserJobComplete(ServerGUID.Value, new NamedPipeBrowserJob() { PageGUID = browser.PageGUID, LogElementGUID = logElementGUID });
        }       

        private void Browser_FormClosing(object sender, FormClosingEventArgs e)
        {
            var browser = (BrowserForm) sender;
            Browsers.Remove(browser);
            //Warning window etc etc...
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (ProcessGUID == null || ServerGUID == null)
                    return;

                var dialogResult = MessageBox.Show("Are you sure you want to close this session?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dialogResult == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                if (!NamedPipeHelper.SendClosingSession(ServerGUID.Value, ProcessGUID.Value, Process.GetCurrentProcess().Id))
                {
                    e.Cancel = true;
                    MessageBox.Show("Player does not allow closing the browser at this point");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error occured while closing form");
                e.Cancel = false;
            }
        }
    }
}
