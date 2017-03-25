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

namespace NamedPipesClient
{
    public partial class ClientForm : Form
    {
        public ClientForm()
        {
            whatever = new Whatever();
            whatever.Run();
            InitializeComponent();
        }

        private static Whatever whatever = null;

        private void Form1_Load(object sender, EventArgs e)
        {
            //new ClientForm().Run();
            //Run();
        }

        //public void Run()
        //{
        //    // Consume the service
        //    var factory = new DuplexChannelFactory<ISimpleService>(new InstanceContext(this), new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/SimpleService"));
        //    var proxy = factory.CreateChannel();

        //    proxy.ProcessData();
        //    //textBox1.AppendText + Environment.NewLine);
        //}

        //public void NotifyClient()
        //{
        //    //textBox1.AppendText("NotifyClient" + Environment.NewLine);
        //}
    }

    
}
