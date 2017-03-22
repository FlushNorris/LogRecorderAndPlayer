using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestBrowser
{
    public enum LRAPSessionFlowType
    {
        Server = 0,
        Client = 1
    }

    [Serializable]
    public class LRAPSessionFlow
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    [Serializable]
    public class LRAPSession
    {
        public string Name { get; set; }
        public Dictionary<LRAPSessionFlowType, List<LRAPSessionFlow>> Flows { get; set; }

        public LRAPSession()
        {
            Flows = new Dictionary<LRAPSessionFlowType, List<LRAPSessionFlow>>();
        }
    }

    public class EventsTable : TableLayoutPanel
    {
        public List<LRAPSession> Sessions { get; set; } = null;

        public void SetupSessions()
        {
            Sessions = new List<LRAPSession>();

            var s1 = new LRAPSession();
            s1.Name = "Session1";            
            var f1 = new LRAPSessionFlow();
            f1.Start = new DateTime(2017, 1, 1, 12, 23, 45, 666);
            f1.End = new DateTime(2017, 1, 1, 12, 23, 50, 666);
            var lst = new List<LRAPSessionFlow>();
            lst.Add(f1);
            s1.Flows.Add(LRAPSessionFlowType.Server, lst);
            var f2 = new LRAPSessionFlow();
            f2.Start = new DateTime(2017, 1, 1, 12, 23, 50, 666);
            f2.End = new DateTime(2017, 1, 1, 12, 23, 59, 666);
            lst = new List<LRAPSessionFlow>();
            lst.Add(f2);
            s1.Flows.Add(LRAPSessionFlowType.Client, lst);
            Sessions.Add(s1);

            s1 = new LRAPSession();
            s1.Name = "Session2";
            f1 = new LRAPSessionFlow();
            f1.Start = new DateTime(2017, 1, 1, 12, 23, 45, 666);
            f1.End = new DateTime(2017, 1, 1, 12, 23, 50, 666);
            lst = new List<LRAPSessionFlow>();
            lst.Add(f1);
            s1.Flows.Add(LRAPSessionFlowType.Server, lst);
            f2 = new LRAPSessionFlow();
            f2.Start = new DateTime(2017, 1, 1, 12, 23, 50, 666);
            f2.End = new DateTime(2017, 1, 1, 12, 23, 59, 666);
            lst = new List<LRAPSessionFlow>();
            lst.Add(f2);
            s1.Flows.Add(LRAPSessionFlowType.Client, lst);
            Sessions.Add(s1);

            Refresh();
        }

        //zoom level?

        public void Refresh()
        {
            //Skal anvende minimum distance til at tegne resten

            this.RowCount = Sessions.SelectMany(x => x.Flows).Count()+1; //plus footer for taking last space
            this.ColumnCount = 10;

            var row = 0;
            this.RowStyles.Clear();
            foreach (var session in Sessions)
            {
                foreach (var sessionKey in session.Flows.Keys)
                {
                    this.Controls.Add(new Label() { Text = $"{sessionKey}{row}: {session.Name}" }, 0, row++);
                    this.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
                }
            }

            this.ColumnStyles.Clear();
            this.ColumnStyles.Add(new ColumnStyle()); //autosize for titles
            for (int c = 0; c < this.ColumnCount; c++)
            {
                this.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            }

            this.Controls.Add(new Panel() { BackColor = Color.BlueViolet, Margin = new Padding(0)}, 1, 1);
            this.Controls.Add(new Panel() { BackColor = Color.BlueViolet, Margin = new Padding(0) }, 3, 1);
            this.Controls.Add(new Panel() { BackColor = Color.Red, Margin = new Padding(0) }, 2, 2);

            //this.eventsTable1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            //this.eventsTable1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            //this.eventsTable1.RowStyles.Add(new System.Windows.Forms.RowStyle());

            this.RowStyles.Add(new System.Windows.Forms.RowStyle());

            this.AutoSize = true;
        }
    }
}
