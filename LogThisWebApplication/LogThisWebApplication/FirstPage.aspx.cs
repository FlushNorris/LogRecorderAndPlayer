using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LogThisWebApplication
{
    public partial class FirstPage : System.Web.UI.Page
    {
        public static Control GetPostBackControl(System.Web.UI.Page page)
        {
            Control control = null;

            string ctrlname = page.Request.Params.Get("__EVENTTARGET");
            if (ctrlname != null && ctrlname != string.Empty)
            {
                control = page.FindControl(ctrlname);
            }
            else
            {
                foreach (string ctl in page.Request.Form)
                {
                    Control c = page.FindControl(ctl);
                    if (c is System.Web.UI.WebControls.Button)
                    {
                        control = c;
                        break;
                    }
                }
            }
            return control;
        }

        public static int pageLoadCounter = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Request.Form["WHAT"] = "BUTT";
            var obj = GetPostBackControl(this);
            serverTextbox.Text = "weee";
        }

        protected void serverButton_OnClick(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(10000);
            serverTextbox.Text = DateTime.Now.ToString("HH:mm:ss:fff");
        }

        protected void serverButton2_OnClick(object sender, EventArgs e)
        {
            serverTextbox.Text = DateTime.Now.ToString("HH:mm:ss:fff")+" part 2";
        }

        public static int pageRedirectCounter = 0;

        protected void redirectButton_OnClick(object sender, EventArgs e)
        {
            pageRedirectCounter++;
            System.Threading.Thread.Sleep(5000);
            Response.Redirect("SecondPage.aspx");
        }
    }
}