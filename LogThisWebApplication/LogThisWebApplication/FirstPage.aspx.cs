﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LogThisWebApplication
{
    public partial class FirstPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void serverButton_OnClick(object sender, EventArgs e)
        {
            serverTextbox.Text = DateTime.Now.ToString("HH:mm:ss:fff");
        }
    }
}