using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using NamedPipesAssembly;

namespace NamedPipesClient
{
    public class XForm //: Control
    {
        private Whatever whatever = null;
        public XForm() : base()
        {
            whatever = new Whatever();
            whatever.Run();
        }
    }

    public class Whatever : IMyCallbackService
    {
        public void Run()
        {
            // Consume the service
            var factory = new DuplexChannelFactory<ISimpleService>(new InstanceContext(this), new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/SimpleService"));
            var proxy = factory.CreateChannel();

            Console.WriteLine(proxy.ProcessData());
        }

        public void NotifyClient()
        {
            Console.WriteLine("Notification from Server");
        }
    }

    static class Program
    {
        private static Whatever whatever = null;
        static void Main()
        {
            new XForm();
            //Application.Run();
            //whatever = new Whatever();
            //whatever.Run();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void MainX()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ClientForm());
        }
    }
}
