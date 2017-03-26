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
        private List<Browser> Browsers { get; set; } = new List<Browser>();

        public Form2()
        {
            ServerGUID = Guid.NewGuid();
            InitializeComponent();
            Server = new NamedPipeServer(ServerGUID);
            Server.ServiceInstanse.OnSyncBrowser += ServiceInstanse_OnSyncBrowser;
        }

        private NamedPipeServerResponse ServiceInstanse_OnSyncBrowser(NamedPipeBrowser namedPipeBrowser)
        {
            var browser = Browsers.First(x => x.ProcessGUID.Equals(namedPipeBrowser.ProcessGUID) && x.ProcessId == -1);
            browser.ProcessId = namedPipeBrowser.ProcessId;
            return new NamedPipeServerResponse() {Success = true};
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var browser in Browsers.Where(x => x.ProcessId != -1))
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
            var elementsInfo = LoggingHelper.LoadElementsInfo(@"c:\LogRecorderAndPlayerJSONBAK\ALL", LRAPLogType.JSON);


            DoubleBuffered = true;
            eventsTable1.SetupSessions(elementsInfo.LogElementInfos);
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
            var browser = new Browser() {ProcessGUID = Guid.NewGuid(), ProcessId = -1};

            Browsers.Add(browser);

            var browserApp = ConfigurationManager.AppSettings["BrowserApp"];

            var psi = new ProcessStartInfo(browserApp, $"{ServerGUID} {browser.ProcessGUID}");
            Process.Start(psi);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var browser = Browsers.FirstOrDefault(x => x.ProcessId != -1);
            if (browser != null)
            {
                Browsers.Remove(browser);
                Process.GetProcessById(browser.ProcessId).Kill();
            }
        }
    }

    public class Browser : NamedPipeBrowser
    {
        
    }
}
