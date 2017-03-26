using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestBrowser.Properties;

namespace TestBrowser
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            button1.Image = Resources.start;

            var textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            textBox1.Location = new System.Drawing.Point(205, 283);
            textBox1.Name = "textBox1";
            textBox1.Text = "???";
            textBox1.Size = new System.Drawing.Size(100, 31);
            textBox1.TabIndex = 0;

            Controls.Add(textBox1);
            textBox1.BringToFront();



            //var qwe = new ResourceManager("LogPlayer.Resources", Assembly.GetExecutingAssembly());
            //var q = qwe.GetObject("StartEvent");
            //var x = Resources.ResourceManager.GetObject("StartEvent");
            //Bitmap bmp = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("LogPlayer.Resources.start.png"));

            //            int zIndex = Controls.GetChildIndex(textBox1);

        }
    }
}
