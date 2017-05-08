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

namespace LogSession
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

            if (ServerGUID == null || ProcessGUID == null || startingPageGUID == null || startingUrl == null)
            {
                MessageBox.Show("Invalid arguments");
                Close();
                return;
            }

            refreshTimer.Enabled = true;

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

        private TransferElementResponse ServiceInstanse_OnBrowserJob(LogElementDTO logElement)
        {
            //if (logElement.GUID.ToString().ToLower().IndexOf("caa1") == 0)
            //{
            //    MessageBox.Show("what?");
            //}

            switch (logElement.LogType)
            {
                case LogType.OnPageSessionBefore: //Burde jeg slå alle disse page pagerequest-events sammen?... nej, da der kan være event kald imellem eventsene
                    JumpToURL(logElement.PageGUID, logElement.Element);
                    break;
                case LogType.OnResize:
                    var browserResize = SerializationHelper.Deserialize<BrowserResize>(logElement.Value, SerializationType.Json);
                    FindBrowserAndExec(logElement.PageGUID, x => x.ResizeBrowser(browserResize, logElement.GUID));
                    break;
                default:
                    FindBrowserAndExec(logElement.PageGUID, x => x.PerformLogElement(logElement));
                    break;
            }

            return new TransferElementResponse() {Success = true};
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
                browser.OnHandlerJobCompleted += Browser_OnHandlerJobCompleted;
                Browsers.Add(browser);
                browser.Show();
            }
        }

        private void Browser_OnHandlerJobCompleted(BrowserForm browser, LogType logType, string handlerUrl, JobStatus jobStatus)
        {
            NamedPipeHelper.SetHandlerLogElementAsDone(ServerGUID.Value, browser.PageGUID, logType, handlerUrl, jobStatus); //, async: false); 
        }

        private void Browser_OnJobCompleted(BrowserForm browser, Guid? logElementGUID, JobStatus jobStatus)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    textBox1.AppendText(logElementGUID.ToString()+Environment.NewLine);
                }));
            }
            else
            {
                textBox1.AppendText(logElementGUID.ToString() + Environment.NewLine);
            }

            //MessageBox.Show("Send BrowserJobComplete to player");
            //MessageBox.Show($"jobcomplete: logElementGUID={logElementGUID}");
            NamedPipeHelper.SetLogElementAsDone(ServerGUID.Value, browser.PageGUID, logElementGUID, jobStatus); //, async: false); 

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
