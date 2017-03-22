namespace TestBrowser
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.eventsTable1 = new TestBrowser.EventsTable();
            this.SuspendLayout();
            // 
            // eventsTable1
            // 
            this.eventsTable1.ColumnCount = 3;
            this.eventsTable1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.eventsTable1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.eventsTable1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.eventsTable1.Location = new System.Drawing.Point(0, 0);
            this.eventsTable1.Name = "eventsTable1";
            this.eventsTable1.RowCount = 3;
            this.eventsTable1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.eventsTable1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.eventsTable1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.eventsTable1.Sessions = null;
            this.eventsTable1.Size = new System.Drawing.Size(868, 588);
            this.eventsTable1.TabIndex = 0;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(868, 588);
            this.Controls.Add(this.eventsTable1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private EventsTable eventsTable1;
    }
}