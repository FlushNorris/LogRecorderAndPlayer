using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogRecorderAndPlayer;

namespace TestBrowser
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
//            var elementsInfo = LoggingHelper.LoadElementsInfo(@"c:\LogRecorderAndPlayerJSONBAK\02 Kundesøgningsside load", LRAPLogType.JSON);
            var elementsInfo = LoggingHelper.LoadElementsInfo(@"c:\LogRecorderAndPlayerJSONBAK\ALL", LRAPLogType.JSON);


            DoubleBuffered = true;
            eventsTable1.SetupSessions(elementsInfo.LogElementInfos);
        }

        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                    eventsTable1.NextPage();
                    break;
                case Keys.Left:
                    eventsTable1.PrevPage();
                    break;
            }

            //MessageBox.Show("asdads");
        }

        #region .. Double Buffered function ..
        public static void SetDoubleBuffered(System.Windows.Forms.Control c)
        {
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;
            System.Reflection.PropertyInfo aProp = typeof(System.Windows.Forms.Control).GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
            aProp.SetValue(c, true, null);
        }

        #endregion


        #region .. code for Flucuring ..

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        #endregion
    }
}
