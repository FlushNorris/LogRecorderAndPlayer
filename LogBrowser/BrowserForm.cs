

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
        public delegate void JobCompleted(BrowserForm browser, Guid? logElementGUID);

        public event JobCompleted OnJobCompleted = null;

        public Guid ServerGUID { get; set; }
        public Guid PageGUID { get; set; }
        private string StartingURL { get; set; }

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
            //var t = new Thread(() =>
            //{
            //    try
            //    {
            //        if (this.InvokeRequired)
            //        {
            //            this.Invoke(new MethodInvoker(delegate
            //            {
            //                throw new NotImplementedException();
            //            }));
            //        }
            //    }
            //    catch (NotImplementedException nex)
            //    {
            //        if (this.InvokeRequired)
            //        {
            //            this.Invoke(new MethodInvoker(delegate
            //            {
            //                MessageBox.Show(nex.GetType().ToString());
            //            }));
            //        }
            //    }

            //});
            //t.Start();
            webBrowser.Url = new Uri(StartingURL);
            var scriptManager = new ScriptManager(this);
            scriptManager.OnJobCompleted += ScriptManager_OnJobCompleted;
            webBrowser.ObjectForScripting = scriptManager;
        }

        private void ScriptManager_OnJobCompleted(Guid? logElementGUID)
        {
            OnJobCompleted?.Invoke(this, logElementGUID);
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
            //MessageBox.Show("Complete!!");
            OnJobCompleted?.Invoke(this, null);
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

            OnJobCompleted?.Invoke(this, logElementGUID);
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
                            if (webBrowser.Document != null)
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

        private void WaitTillWebBrowserIsNoLongerInUse(string url, double timeoutInMS = 30000)
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
                                webBrowser.Url = new Uri(url);
                                finished = true;
                            }));
                        }
                    }
                    catch (COMException coex) //e.g. The requested resource is in use. (Exception from HRESULT: 0x800700AA) = which is raised if an alert-box is shown
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

        public void ScrollBrowser(BrowserScroll browserScroll, Guid logElementGUID)
        {
            WaitTillDocumentIsComplete(delegate
            {
                WaitTillWebBrowserIsNoLongerInUse($"javascript:logRecorderAndPlayer.pushLogElementGUID(\"{logElementGUID}\");window.scrollTo({browserScroll.left}, {browserScroll.top});window.external.SetLogElementAsDone(\"{logElementGUID}\");");
            }); //Ja ja... men hvad så med sideload -> postbackevent -> sideload -> Gør noget clientside. I dette tilfælde vil documentet være complete
            return;
            MessageBox.Show("ScrollBrowser2");
            //Faktisk vil det være nødvendigt at LoggingPage.LogRequest fortalte browseren at et request var på vej, da alle efterfølgende clientevents relateret til den samme pageGUID skal vente på webBrowser_DocumentCompleted er kaldt!
            return;

            MessageBox.Show($"javascript:logRecorderAndPlayer.pushLogElementGUID(\"{logElementGUID}\");window.scrollTo({browserScroll.left}, {browserScroll.top});");
            //WaitTillWebBrowserIsNoLongerInUse($"javascript:logRecorderAndPlayer.pushLogElementGUID(\"{logElementGUID}\");window.scrollTo({browserScroll.left}, {browserScroll.top});");
            MessageBox.Show("!!!!!");

//            webBrowser.Url = new Uri("javascript:alert('1337');");
//            webBrowser.Document.InvokeScript("logRecorderAndPlayer.testmethod");

            //webBrowser.Url = new Uri("javascript:$('input').val('hest');alert('1');");
            //webBrowser.Url = new Uri($"javascript:alert(logRecorderAndPlayer);"); //alert(logRecorderAndPlayer.pushLogElementGUID);logRecorderAndPlayer.pushLogElementGUID(\"{logElementGUID}\");alert(1337);alert(window);alert(window.scrollTo);window.scrollTo({browserScroll.left}, {browserScroll.top});");
//            MessageBox.Show("Scroll is called 2");

            return;
            //When does this event complete? Shouldn't we let the LRAP-JS determine this? And to a callback via window.external...
            //Clientside then needs to know it is playing, it doesn't now... playing=1 are only in the url at the first page atm, should be added to the session and used in the lrap-js-init function!
            //blah blah blah.. this shouldn't compile!!

            //Problemet med clientside events er jo at alle scrollTo events vil blive udført!.... hov, jeg skal jo bare oprette et event i slutningen til dette player-formål          

            //OnJobCompleted?.Invoke(this, logElementGUID);
        }

        public void MouseDownBrowser(BrowserMouseDown browserMouseDown, Guid logElementGUID)
        {
            WaitTillDocumentIsComplete(delegate
            {
                //browserMouseDown.attributes
                //browserMouseDown.events

                //jQueryEvent:
                StringBuilder sbJS = new StringBuilder();
                sbJS.Append("javascript:try{");
                sbJS.Append($"logRecorderAndPlayer.runEventsFor(\"{browserMouseDown.element}\")");

                //foreach (var e in browserMouseDown.events) //Currently max count = 2
                //{
                //    if (e.IndexOf("jQueryEvent:") == 0)
                //        sbJS.Append("logRecorderAndPlayer.runJQueryEvents('mousedown');"); //Check if the same amount of event-methods are executed, as when we recorded the flow
                //    else
                //        sbJS.Append(e); //could contain "return <statement>", therefore run everything in a try/finally to ensure that the SetLogElementAsDone are being executed
                //}

                sbJS.Append($"}}finally{{window.external.SetLogElementAsDone(\"{logElementGUID}\");}}");

                WaitTillWebBrowserIsNoLongerInUse(sbJS.ToString());
            }); //Ja ja... men hvad så med sideload -> postbackevent -> sideload -> Gør noget clientside. I dette tilfælde vil documentet være complete
            return;
        }
        public void FocusBrowser(BrowserFocus browserFocus, Guid logElementGUID)
        {
            WaitTillDocumentIsComplete(delegate
            {
                //browserMouseDown.attributes
                //browserMouseDown.events

                //jQueryEvent:
                StringBuilder sbJS = new StringBuilder();
                sbJS.Append("javascript:try{");
                //Ja, kan ikke compile pga denne kommentar som du skal læse i morgen!... ja, runEventsFor skal køre ala følgende kode... test getElementByElementPath som noget af det første!                

                sbJS.Append($"logRecorderAndPlayer.runEventsFor(logRecorderAndPlayer.LogType.OnFocus, \"{browserFocus.element}\");");
                //sbJS.Append($"logRecorderAndPlayer.getJQueryElement(\"{browserFocus.element}\");");

                //foreach (var e in browserFocus.events) //Currently max count = 2
                //{
                //    if (e.IndexOf("jQueryEvent:") == 0)
                //        sbJS.Append("logRecorderAndPlayer.runJQueryEvents('focus');"); //Check if the same amount of event-methods are executed, as when we recorded the flow
                //    else
                //        sbJS.Append(e); //could contain "return <statement>", therefore run everything in a try/finally to ensure that the SetLogElementAsDone are being executed
                //}

                sbJS.Append($"}}finally{{window.external.SetLogElementAsDone(\"{logElementGUID}\");}}");

                WaitTillWebBrowserIsNoLongerInUse(sbJS.ToString());
            }); //Ja ja... men hvad så med sideload -> postbackevent -> sideload -> Gør noget clientside. I dette tilfælde vil documentet være complete
            return;
        }

        /*
         * 
         *         public static LogElementsInfo LoadElementsInfo(string path, LRAPLogType logType, DateTime? from = null, DateTime? to = null)
        {
            return GetLoggingPersister(logType).LoadLogElementsInfo(path, from, to);
        }

        public static LogElementDTO LoadElement(LRAPLogType logType, LogElementInfo logElementInfo)
        {
            return GetLoggingPersister(logType).LoadLogElement(logElementInfo);
        }

         * 
         * */

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
        public delegate void JobCompleted(Guid? logElementGUID);
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

        public void SetLogElementAsDone(object obj, bool error, string errorMessage)
        {
            if (error)
            {
                MessageBox.Show("fejl!");
            }
            else
            {
                MessageBox.Show("ok!");
            }
            MessageBox.Show(errorMessage ?? "null");
            //MessageBox.Show($"error = {error}");            
            return;


            Guid logElementGUID;
            if (!Guid.TryParse(obj.ToString(), out logElementGUID))
                throw new Exception($"SetLogElementAsDone called with invalid arguments ({obj})");

            //MessageBox.Show("XXXXX");

            OnJobCompleted?.Invoke(logElementGUID);
        }        
    }

}

