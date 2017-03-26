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

namespace LogBrowser
{
    public partial class BrowserForm : Form
    {
        private string[] Arguments { get; set; }= null;
        private Guid? ServerGUID { get; set; }
        private Guid? ProcessGUID { get; set; }

        public BrowserForm(string[] args)
        {
            Arguments = args;
            InitializeComponent();
        }

        private void BrowserForm_Load(object sender, EventArgs e)
        {
            if (Arguments.Length == 1 && Arguments[0] == "developer")
                return;

            if (Arguments.Length > 1)
            {
                Guid tmp;
                if (Guid.TryParse(Arguments[0], out tmp))
                    ServerGUID = tmp;
                if (Guid.TryParse(Arguments[1], out tmp))
                    ProcessGUID = tmp;

            }

            if (ServerGUID == null || ProcessGUID == null)
            {
                MessageBox.Show("Invalid arguments");
                Close();
            }

            //Send back process id related to guid

            var browser = new NamedPipeBrowser() {ProcessGUID = ProcessGUID.Value, ProcessId = Process.GetCurrentProcess().Id};
            var serverRequest = new NamedPipeServerRequest() {Type = NamedPipeServerRequestType.SyncBrowser, Data = browser };
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

            //var res = NamedPipeClient.TalkToServer_Threading("Lord Helmet1");
            //res += NamedPipeClient.TalkToServer_Threading("Lord Helmet2");
            //res += NamedPipeClient.TalkToServer_Threading("Lord Helmet3");
        }

        private void BrowserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ProcessGUID == null || ServerGUID == null)
                return;

            var dialogResult = MessageBox.Show("Are you sure you want to close this browser?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (dialogResult == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            var browser = new NamedPipeBrowser() { ProcessGUID = ProcessGUID.Value, ProcessId = Process.GetCurrentProcess().Id };
            var serverRequest = new NamedPipeServerRequest() { Type = NamedPipeServerRequestType.ClosingBrowser, Data = browser };
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
