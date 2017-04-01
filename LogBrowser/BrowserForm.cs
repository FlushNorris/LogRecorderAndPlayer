
using System;
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

        public BrowserForm(Guid serverGUID, Guid pageGUID, string url)
        {
            ServerGUID = serverGUID;
            PageGUID = pageGUID;
            StartingURL = LoggingHelper.PrepareUrlForLogPlayer(url, serverGUID, pageGUID);
            
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
            webBrowser.ObjectForScripting = new ScriptManager(this);
        }

        private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            RefreshUI();
        }

        private void Browser_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            var webBrowser = (WebBrowser)sender;
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
            OnJobCompleted?.Invoke(this, null);
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

        public void ScrollBrowser(BrowserScroll browserScroll, Guid logElementGUID)
        {
            webBrowser.Url = new Uri($"javascript:window.scrollTo({browserScroll.left}, {browserScroll.top});");
            //When does this event complete? Shouldn't we let the LRAP-JS determine this? And to a callback via window.external...
            //Clientside then needs to know it is playing, it doesn't now... playing=1 are only in the url at the first page atm, should be added to the session and used in the lrap-js-init function!
            //blah blah blah

            //OnJobCompleted?.Invoke(this, logElementGUID);
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
    }

}

