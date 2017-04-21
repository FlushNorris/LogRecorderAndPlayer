using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LogRecorderAndPlayer;
using LogThisWebApplication.WebService;

namespace LogThisWebApplication
{
    public partial class PageCallingWebService : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnCallWebService_OnClick(object sender, EventArgs e)
        {
            var client = new WebService1SoapClient();
            LogRecorderAndPlayer.LoggingClientBase.SetupClientBase(client, Context);
            txtSomeResult.Text = client.SomeFunction(txtSomeInput.Text);
        }
    }
}