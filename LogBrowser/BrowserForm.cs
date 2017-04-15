using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogRecorderAndPlayer;
using Microsoft.Win32;

namespace LogBrowser
{
    public partial class BrowserForm : Form //Both primary form and secondary form (to save space.. formwise(
    {
        public delegate void JobCompleted(BrowserForm browser, Guid? logElementGUID, JobStatus jobStatus);

        public event JobCompleted OnJobCompleted = null;

        public Guid ServerGUID { get; set; }
        public Guid PageGUID { get; set; }
        private string StartingURL { get; set; }

        private bool IsNavigating { get; set; } = true;

        public BrowserForm(Guid serverGUID, Guid sessionGUID, Guid pageGUID, string url)
        {
            if (serverGUID.Equals(new Guid()))
            {
                MessageBox.Show("Remember... we are in developer-mode!");
            }

            ServerGUID = serverGUID;
            PageGUID = pageGUID;
            StartingURL = LoggingHelper.PrepareUrlForLogPlayer(url, serverGUID, sessionGUID, pageGUID);

            InitializeComponent();
        }

        public string URL
        {
            get
            {
                var url = webBrowser?.Url;
                if (url == null)
                    return "NULL";
                return LoggingHelper.StripUrlForLRAP(url.ToString());
            }
        }

        private void RefreshUI()
        {
            Text = $"URL: {this.URL}";
        }

        private void BrowserForm_Load(object sender, EventArgs e)
        {
           
            webBrowser.Url = new Uri(StartingURL);
            var scriptManager = new ScriptManager(this);
            scriptManager.OnJobCompleted += ScriptManager_OnJobCompleted;
            webBrowser.ObjectForScripting = scriptManager;
        }

        private void ScriptManager_OnJobCompleted(Guid? logElementGUID, JobStatus jobStatus)
        {
            OnJobCompleted?.Invoke(this, logElementGUID, jobStatus);
        }

        private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            RefreshUI();
        }

        private void Browser_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            var webBrowser = (WebBrowser) sender;            
            if (webBrowser.Document != null)
            {
                foreach (HtmlElement tag in webBrowser.Document.All)
                {
                    if (tag.Id == null)
                    {
                        tag.Id = String.Empty;
                        switch (tag.TagName.ToUpper())
                        {
                            case "A":
                            {
                                var regExTargetBlank = new Regex("\\s+target\\s*=\\s*[\\\"\\']*_blank[\\\"\\']*([\\s>/])");
                                var mTargetBlank = regExTargetBlank.Match(tag.OuterHtml);

                                if (mTargetBlank.Success)
                                {
                                    var newOuterHtml = tag.OuterHtml.Substring(0, mTargetBlank.Index) + tag.OuterHtml.Substring(mTargetBlank.Index + mTargetBlank.Length - (mTargetBlank.Groups[1].Value != " " ? 1 : 0));
                                    var regExHREF = new Regex("\\s+href\\s*=\\s*[\\\"\\']*([^\\\"\\']+)[\\\"\\']*([\\s>/])");
                                    var mHREF = regExHREF.Match(newOuterHtml);
                                    if (mHREF.Success)
                                    {
                                        tag.OuterHtml = newOuterHtml.Substring(0, mHREF.Groups[1].Index) + $"javascript:window.external.OpenNewTab(&quot;{mHREF.Groups[1]}&quot;)" + newOuterHtml.Substring(mHREF.Groups[1].Index + mHREF.Groups[1].Length);
                                    }
                                }

                                //tag.MouseUp += new HtmlElementEventHandler(link_MouseUp);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            IsNavigating = false;
            OnJobCompleted?.Invoke(this, null, new JobStatus() {Success = true});
            //webBrowser.Url = new Uri("javascript:$('input').val('hest');alert('1');");
        }

        public void ResizeBrowser(BrowserResize browserResize, Guid logElementGUID)
        {
            //browser.ResizeBrowser(browserResize);
            //webBrowser.Url = new Uri("javascript:$('input').val('hest');alert('1');");

            var diffWidth = browserResize.width - webBrowser.Size.Width;
            var diffHeight = browserResize.height - webBrowser.Size.Height;

            this.Width += diffWidth;
            this.Height += diffHeight;

            OnJobCompleted?.Invoke(this, logElementGUID, new JobStatus() {Success = true});
        }

        private void WaitTillDocumentIsComplete(MethodInvoker invoker, double timeoutInMS = 30000)
        {
            var t = new Thread(() => //prevent deadlock (e.g. OnPageResponse followed by clientside event which has to wait for the pageresponse to end)
            {
                var start = DateTime.Now;
                var finished = false;
                do
                {
                    if (this.InvokeRequired) //but needs to run in on the main ui-thread
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            if (webBrowser.Document != null && !IsNavigating)
                            {
                                finished = true;
                            }
                        }));
                    }

                    if (!finished)
                    {
                        Thread.Sleep(300);
                    }
                    
                } while (!finished && (DateTime.Now - start).TotalMilliseconds < timeoutInMS) ;

                if (!finished)
                    throw new Exception("WaitTillDocumentIsComplete raised an timeout");

                if (this.InvokeRequired) //but needs to run in on the main ui-thread
                {
                    this.Invoke(new MethodInvoker(delegate
                    {
                        invoker.Invoke();
                    }));
                }
                
            });
            t.Start();
        }

        private void WaitTillWebBrowserIsNoLongerInUse(Func<WebBrowser, bool> f, double timeoutInMS = 30000)
        {
            var t = new Thread(() => //prevent deadlock (e.g. OnPageResponse followed by clientside event which has to wait for the pageresponse to end)
            {
                var start = DateTime.Now;
                var finished = false;
                do
                {
                    try
                    {
                        if (this.InvokeRequired) //but needs to run in on the main ui-thread
                        {
                            this.Invoke(new MethodInvoker(delegate
                            {                                
                                //webBrowser.Url = new Uri(url);
                                finished = f(webBrowser);
                            }));
                        }
                    }
                    catch (COMException) //e.g. The requested resource is in use. (Exception from HRESULT: 0x800700AA) = which is raised if an alert-box is shown
                    {
                        Thread.Sleep(300);
                        Application.DoEvents();
                    }
                } while (!finished && (DateTime.Now - start).TotalMilliseconds < timeoutInMS);

                if (!finished)
                    throw new Exception("WaitTillWebBrowserIsNoLongerInUse raised an timeout");
            });
            t.Start();
        }

        public void PerformLogElement(LogElementDTO logElement)
        {
            //function playEventFor(elementPath, logElement /*json*/) {

            var jsonLogElement = SerializationHelper.Serialize(logElement, SerializationType.Json);

            WaitTillDocumentIsComplete(delegate
            {
                //WaitTillWebBrowserIsNoLongerInUse($"javascript:logRecorderAndPlayer.pushLogElementGUID(\"{logElementGUID}\");window.scrollTo({browserScroll.left}, {browserScroll.top});window.external.SetLogElementAsDone(\"{logElementGUID}\");");
                WaitTillWebBrowserIsNoLongerInUse(webBrowser =>
                {                    
                    webBrowser.Document.InvokeScript("eval", new object[] {$"logRecorderAndPlayer.playLogElement({jsonLogElement})"});
                    return true;
                });
            });
        }       

        private void button1_Click(object sender, EventArgs e)
        {            

            //            var result = webBrowser.Document.InvokeScript("testInvokeScript", new object[] { "hest" });
//            var result = webBrowser.Document.InvokeScript("logRecorderAndPlayer_PlayEvent", new object[] { "hest" });
            var le = new LogElementDTO(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null, 666, LogType.OnCut, "element", "element2", "value", 1337, null);
//            var lst = LoggingHelper.LoadElements(@"c:\WebApplicationJSON", LRAPLogType.JSON).Where(x => x.LogType == LogType.OnKeyPress).ToList();

            var leJSON = "null"; //SerializationHelper.Serialize(lst[0], SerializationType.Json);
            //MessageBox.Show($"length = {leJSON.Length}");

            var testLength = ""; //new String('c', 40*1024*1024); // approx max 40MB (aka more than enough for my logelements :D)

            var result = webBrowser.Document.InvokeScript("eval", new object[] { $"logRecorderAndPlayer.playEventFor(logRecorderAndPlayer.LogType.OnKeyPress, '#someId', {leJSON}, '{testLength}')" });
            if (result != null)
                MessageBox.Show(result.ToString());
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
//            var w = new WebBrowser();
//            w.Url = new Uri("http://localhost:61027/FirstPage.aspx");
            //w.DocumentText = "<html><body><input type='text' value='something'/></body></html>";

            //var r = w.DocumentText;
            //textBox1.Text = r;

            //var result = webBrowser.Document.InvokeScript("eval", new object[] { $"logRecorderAndPlayer.getJQueryElementByElementPath('#form1,3!DIV,3!INPUT:not([type])').val('WHAT')" });            
        }

        private void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            IsNavigating = true;
        }

        //private void link_MouseUp(object sender, HtmlElementEventArgs e)
        //{
        //    var link = (HtmlElement)sender;
        //    mshtml.HTMLAnchorElementClass a = (mshtml.HTMLAnchorElementClass)link.DomElement;
        //    switch (e.MouseButtonsPressed)
        //    {
        //        case MouseButtons.Left:
        //            {
        //                if ((a.target != null && a.target.ToLower() == "_blank") || e.ShiftKeyPressed || e.MouseButtonsPressed == MouseButtons.Middle)
        //                {
        //                    AddTab(a.href);
        //                }
        //                else
        //                {
        //                    CurrentBrowser.TryNavigate(a.href);
        //                }
        //                break;
        //            }
        //        case MouseButtons.Right:
        //            {
        //                CurrentBrowser.ContextMenuStrip = null;
        //                var contextTag = new ContextTag();
        //                contextTag.Element = a;
        //                contextHtmlLink.Tag = contextTag;
        //                contextHtmlLink.Show(Cursor.Position);
        //                break;
        //            }
        //    }
        //}

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    webBrowser.Url = new Uri("javascript:$('input').val('hest');alert('1');");
        //    //webBrowser.Url = new Uri("javascript:window.external.ShowMessage($('input').length+\"\");");
        //    //webBrowser.Url = new Uri("javascript:setTimeout(\"window.external.ShowMessage('server 3s');\", 3000);");

        //    //webBrowser.Url = new Uri();
        //}
    }

    [ComVisible(true)]
    public class ScriptManager
    {
        public delegate void JobCompleted(Guid? logElementGUID, JobStatus jobStatus);
        public event JobCompleted OnJobCompleted = null;

        BrowserForm _form;
        public ScriptManager(BrowserForm form)
        {
            _form = form;
        }
        public void ShowMessage(object obj)
        {
            MessageBox.Show(obj.ToString());
        }

        public void OpenNewTab(object obj)
        {
            MessageBox.Show($"OpenNewTab: {obj.ToString()}");
        }

        public void DebugMethod(object obj)
        {
            MessageBox.Show("Debug : " + obj.ToString());
        }

        public void SetLogElementAsDone(object obj, bool error, string errorMessage)
        {
            Guid logElementGUID;
            if (!Guid.TryParse(obj.ToString(), out logElementGUID))
                throw new Exception($"SetLogElementAsDone called with invalid arguments ({obj})");

            //MessageBox.Show($"Player.OnDone {error} : {errorMessage ?? "null"}");

            OnJobCompleted?.Invoke(logElementGUID, new JobStatus() {Success = !error, Message = errorMessage});

            //if (error)
            //{
            //    MessageBox.Show("fejl!");
            //}
            //else
            //{
            //    MessageBox.Show("ok!");
            //}
            //MessageBox.Show(errorMessage ?? "null");
            ////MessageBox.Show($"error = {error}");            
            //return;

            ////MessageBox.Show("XXXXX");

            //OnJobCompleted?.Invoke(logElementGUID);
        }

        public void SetHandlerLogElementAsDone(string sessionGUID, string pageGUID, string handlerUrl, bool error, string errorMessage)
        {
            
        }
    }

}

