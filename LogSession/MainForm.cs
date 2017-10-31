using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogRecorderAndPlayer;
using LogRecorderAndPlayer.Common;

namespace LogSession
{
    public partial class MainForm : Form
    {
        private string[] Arguments { get; set; } = null;
        private Guid? ServerGUID { get; set; } //PlayerGUID
        private Guid? ProcessGUID { get; set; } //or SessionGUID        
        private List<BrowserForm> Browsers { get; set; } = null;
        private PlayerCommunicationServer Server { get; set; } = null;
        private string BaseUrl = null;

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
            if (Arguments.Length > 4)
            {
                Guid tmp;
                if (Guid.TryParse(Arguments[0], out tmp))
                    ServerGUID = tmp;
                if (Guid.TryParse(Arguments[1], out tmp))
                    ProcessGUID = tmp;
                if (Guid.TryParse(Arguments[2], out tmp))
                    startingPageGUID = tmp;
                BaseUrl = Arguments[3];
                startingUrl = Arguments[4];
            }

            if (ServerGUID == null || ProcessGUID == null || startingPageGUID == null || BaseUrl == null || startingUrl == null)
            {
                MessageBox.Show("Invalid arguments");
                Close();
                return;
            }

            MessageBox.Show("Ready for debug attach!");

            refreshTimer.Enabled = true;

            //Send back process id related to guid
            PlayerCommunicationHelper.SendSyncSession(ServerGUID.Value, ProcessGUID.Value, Process.GetCurrentProcess().Id);

            this.Text = $"Session: {ProcessGUID.Value} Page: {0}";

            //MessageBox.Show($"Starting session-server {ProcessGUID.Value}");
            Server = new PlayerCommunicationServer(ProcessGUID.Value);
            Server.ServiceInstance.OnBrowserJob += ServiceInstanse_OnBrowserJob;

            PerformURLRequest(startingPageGUID.Value, BaseUrl.TrimEnd('/') + '/' + startingUrl.TrimStart('/'), RequestMethod.GET);

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
            //if (logElement.LogType == LogType.OnPageRequest)
            //{
            //    MessageBox.Show("what?");
            //}

            switch (logElement.LogType)
            {
                case LogType.OnResize:
                    var browserResize = JsonHelper.Deserialize<BrowserResize>(logElement.Value);
                    FindBrowserAndExec(logElement.PageGUID, x =>
                    {
                        //MessageBox.Show($"Found Browser by pageGUID {logElement.PageGUID}");
                        x.ResizeBrowser(browserResize, logElement.GUID);                        
                    }); //Hvis det er en redirect, så er PageGUID blevet ændret i response-html'et, men ikke opdateret i browseren endnu. Hvor længe skal jeg vente på at det sker?
                    break;
                case LogType.OnPageRequest:
                    var requestParams = JsonHelper.Deserialize<RequestParams>(logElement.Value);
                    RequestMethod requestMethod;
                    if (!Enum.TryParse(requestParams.ServerVariables["REQUEST_METHOD"], true, out requestMethod))
                        throw new Exception("PageRequest does not have a valid request method");
                    PerformURLRequest(logElement.PageGUID, BaseUrl.TrimEnd('/') + '/' + logElement.Element.TrimStart('/'), requestMethod);
                    break;
                default:
                    FindBrowserAndExec(logElement.PageGUID, x => x.PerformLogElement(logElement));
                    break;
            }

            return new TransferElementResponse() {Success = true};
        }

        private BrowserForm FindBrowser(Guid pageGUID)
        {
            return Browsers.FirstOrDefault(x => x.PageGUID.Equals(pageGUID));
        }

        private void FindBrowserAndExec(Guid pageGUID, Action<BrowserForm> fn, double timeoutInMS = 300000)
        {            
            var t = new Thread(() => 
            {
                var start = DateTime.Now;
                var finished = false;
                //var counter = 0;
                do
                {
                    if (this.InvokeRequired) //but needs to run in on the main ui-thread
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            var browser = FindBrowser(pageGUID);
                            if (browser != null)
                            {
                                fn(browser);
                                finished = true;
                            }                                                       
                        }));
                    }
                    else
                    {
                        var browser = FindBrowser(pageGUID);
                        if (browser != null)
                        {
                            fn(browser);
                            finished = true;
                        }
                    }

                    if (!finished)
                    {
                        Thread.Sleep(300);
                    }

                    //if (counter % 33 == 0)
                    //{
                    //    this.Invoke(new MethodInvoker(delegate
                    //    {
                    //        MessageBox.Show(finished ? "Found it!" : "Still looking....");
                    //    }));
                    //}

                    //counter++;

                } while (!finished && (DateTime.Now - start).TotalMilliseconds < timeoutInMS);

                if (!finished)
                {
                    if (this.InvokeRequired) //but needs to run in on the main ui-thread
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            MessageBox.Show($"Error: Browser({pageGUID}) is not found");
                        }));
                    }
                    else
                    {
                        MessageBox.Show($"Error: Browser({pageGUID}) is not found");
                    }
                }

            });
            t.IsBackground = true;
            t.Start();

        }

        private void PerformURLRequest(Guid pageGUID, string url, RequestMethod requestMethod)
        {
            var browser = FindBrowser(pageGUID);
            if (browser == null)
            {                
                browser = new BrowserForm(ServerGUID.Value, ProcessGUID.Value, pageGUID, url); //Processed as 'GET'
                browser.FormClosing += Browser_FormClosing;
                browser.OnJobCompleted += Browser_OnJobCompleted;
                browser.OnHandlerJobCompleted += Browser_OnHandlerJobCompleted;
                Browsers.Add(browser);
                browser.Show();
            }
            else
            {
                browser.PerformURLRequest(url, requestMethod);
            }
        }

        private LogElementDTO Browser_OnHandlerJobCompleted(BrowserForm browser, LogType logType, string handlerUrl, JobStatus jobStatus)
        {
            return PlayerCommunicationHelper.SetHandlerLogElementAsDone(ServerGUID.Value, browser.PageGUID, logType, handlerUrl, jobStatus); //, async: false); 
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
            PlayerCommunicationHelper.SetLogElementAsDone(ServerGUID.Value, null, browser.PageGUID, logElementGUID, jobStatus); //, async: false); 

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

                //TODO.. include this section again when everything else is working correctly
                //var dialogResult = MessageBox.Show("Are you sure you want to close this session?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                //if (dialogResult == DialogResult.No)
                //{
                //    e.Cancel = true;
                //    return;
                //}                

                //TODO.. include this section again when everything else is working correctly
                //if (!PlayerCommunicationHelper.SendClosingSession(ServerGUID.Value, ProcessGUID.Value, Process.GetCurrentProcess().Id))
                //{
                //    e.Cancel = true;
                //    MessageBox.Show("Player does not allow closing the browser at this point");
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occured while closing form");
                e.Cancel = false;
            }
        }
    }

    public enum RequestMethod //Inspired by http://docs.spring.io/spring-framework/docs/2.5.1/api/org/springframework/web/bind/annotation/RequestMethod.html
    {
        GET = 0,
        POST = 1,
        PUT = 2,
        DELETE = 3,
        HEAD = 4,
        OPTIONS = 5
    }
}
