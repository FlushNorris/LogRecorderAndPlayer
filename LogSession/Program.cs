using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using LogRecorderAndPlayer;

namespace LogSession
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
            Application.Run(new MainForm(args));
            //var serverGUID = new Guid("18e3f838-567e-4aaa-9b52-38587eabd579");
            //var sessionGUID = new Guid();
            //var pageGUID = new Guid();
            //var url = "http://localhost:61027/FirstPage.aspx";

            //var args2 = new string[] {serverGUID.ToString(), sessionGUID.ToString(), pageGUID.ToString(), url};            

            //Application.Run(new BrowserForm(serverGUID, new Guid(), new Guid(), "http://localhost:61027/FirstPage.aspx"));
            //Application.Run(new MainForm(args2));
        }
    }
}
