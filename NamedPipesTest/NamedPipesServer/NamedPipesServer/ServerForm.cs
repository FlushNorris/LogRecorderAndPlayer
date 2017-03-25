using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NamedPipesAssembly;

namespace NamedPipesServer
{
    public partial class ServerForm : Form
    {
        public ServerForm()
        {
            InitializeComponent();
        }

        private ServiceHost ServiceHost { get; set; }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Create a service host with an named pipe endpoint
            var uri = "net.pipe://localhost";
            textBox1.Text = uri;
            ServiceHost = new ServiceHost(typeof(SimpleService), new Uri(uri));
            ServiceHost.AddServiceEndpoint(typeof(ISimpleService), new NetNamedPipeBinding(), "SimpleService");
            ServiceHost.Open();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ServiceHost != null)
            {
                ServiceHost.Close();
                ServiceHost = null;
            }
        }
    }


//    [ServiceBehavior(UseSynchronizationContext = true)] // (InstanceContextMode = InstanceContextMode.PerCall)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class SimpleService : ISimpleService
    {
        public string ProcessData()
        {
            // Get a handle to the call back channel
            var callback = OperationContext.Current.GetCallbackChannel<IMyCallbackService>();
            
            System.Threading.Thread.Sleep(3000);

            callback.NotifyClient();

            System.Threading.Thread.Sleep(3000);

            return DateTime.Now.ToString();
        }
    }
}
