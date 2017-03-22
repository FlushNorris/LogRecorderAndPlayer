using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TestBrowser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //private System.Windows.Controls.WebBrowser WebBrowser { get; set; }

        private void Form1_Load(object sender, EventArgs e)
        {
            //WebBrowser = new System.Windows.Controls.WebBrowser();            

            //var elementHost = new ElementHost();
            //elementHost.Dock = DockStyle.Fill;
            //elementHost.Child = WebBrowser;
            //this.Controls.Add(elementHost);


//            webBrowser1 is Uninitialized,so throws a null pointer exception when it executing webBrowser1.Document.Write(@"....").
//The solution is to add "webBrowser1.Navigate("about: blank");" before "webBrowser1.ObjectForScripting =..."

            //WebBrowser.Source = new Uri("http://jquery.com/");
            webBrowser1.Url = new Uri("http://jquery.com/");
            webBrowser1.ObjectForScripting = new ScriptManager(this);
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //WebBrowser.InvokeScript("alert($(\"input\").size());");

            //MessageBox.Show(this, "asdasd");
            //webBrowser1.Url = new Uri("javascript:alert('client '+$(\"input\").size());window.external.ShowMessage('server');"); //"setTimeout(function(){window.external.ShowMessage('server 3s');}, 3000);");
            webBrowser1.Url = new Uri("javascript:alert(setTimeout(\"window.external.ShowMessage('server 3s');\", 3000);");
            //System.Windows.Controls.WebBrowser x = null;            
            //System.Windows.Controls.WebBrowser
            //webBrowser1.we

            //Skal på en eller anden måde få svar fra browseren om den er klar til et eller andet
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            HtmlDocument doc;
            doc = (HtmlDocument)webBrowser1.Document;
            //doc.AttachEventHandler();
            //HtmlElementEventHandler
            //HTMLDocumentEvents2_Event
            //mshtml.HTMLDocumentEvents2_Event iEvent;
            //iEvent = (mshtml.HTMLDocumentEvents2_Event)doc;
            //iEvent.onclick += new mshtml.HTMLDocumentEvents2_onclickEventHandler(ClickEventHandler);

        }
    }

    [ComVisible(true)]
    public class ScriptManager
    {
        Form1 _form;
        public ScriptManager(Form1 form)
        {
            _form = form;
        }
        public void ShowMessage(object obj)
        {
            MessageBox.Show(obj.ToString());
        }
    }
}
