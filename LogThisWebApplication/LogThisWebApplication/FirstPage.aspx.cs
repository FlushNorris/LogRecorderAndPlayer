using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.EnterpriseServices;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AppModule.InterProcessComm;
using AppModule.NamedPipes;
using LogRecorderAndPlayer;
using IClientChannel = System.ServiceModel.IClientChannel;

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
            serverTextbox.Text = LogRecorderAndPlayer.TimeHelper.Now(Context).ToString("F");
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

        protected void btnAddPersonAndRedirect_OnClick(object sender, EventArgs e)
        {
            var db = ConfigurationManager.ConnectionStrings["db"].ConnectionString;

            var sql = @"declare @id uniqueidentifier;
                        set @id = NEWID();
                        INSERT INTO PERSON(ID, NAME, ADDRESS) VALUES(@id, @Name, @Address);
                        SELECT @id;";

            using (var conn = new SqlConnection(db))
            {
                conn.Open();
                using (var comm = new SqlCommandLRAP(sql, conn))
                {
                    comm.CreateAndAddInputParameter(SqlDbType.NVarChar, "@Name", txtPersonName.Text);
                    comm.CreateAndAddInputParameter(SqlDbType.NVarChar, "@Address", txtPersonAddress.Text);

                    var guid = (Guid) comm.ExecuteScalar();
                    Response.Redirect("SecondPage.aspx?id=" + HttpUtility.HtmlAttributeEncode(guid.ToString()));
                }
            }
        }
    }    
}