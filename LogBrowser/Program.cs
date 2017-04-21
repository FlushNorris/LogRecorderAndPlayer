using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using LogRecorderAndPlayer;

namespace LogBrowser
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //var server = new WCFServer(new Guid());
            //Console.WriteLine("!!!!");
            //Console.WriteLine($"Connected to : {server.ServerState} : {server.ServiceURL}");
            //Console.ReadKey(true);
            //return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainForm(args));
            Application.Run(new BrowserForm(new Guid(), new Guid(), new Guid(), "http://localhost:61027/FirstPage.aspx"));
        }
    }
}
