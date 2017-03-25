using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogRecorderAndPlayer;

namespace LogBrowser
{
    public partial class BrowserForm : Form
    {
        public BrowserForm()
        {
            InitializeComponent();
        }

        private void BrowserForm_Load(object sender, EventArgs e)
        {
            var res = NamedPipeClient.TalkToServer_Threading("Lord Helmet1");
            res += NamedPipeClient.TalkToServer_Threading("Lord Helmet2");
            res += NamedPipeClient.TalkToServer_Threading("Lord Helmet3");
        }

    }
}
