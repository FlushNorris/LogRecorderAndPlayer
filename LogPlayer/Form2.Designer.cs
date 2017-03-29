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
            this.txtBaseUrl = new System.Windows.Forms.TextBox();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // eventsTable1
            // 
            this.eventsTable1.ColumnCount = 3;
            this.eventsTable1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.eventsTable1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.eventsTable1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.eventsTable1.Location = new System.Drawing.Point(12, 57);
            this.eventsTable1.Name = "eventsTable1";
            this.eventsTable1.PageElements = 100;
            this.eventsTable1.PageIndex = 0;
            this.eventsTable1.RowCount = 3;
            this.eventsTable1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.eventsTable1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.eventsTable1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.eventsTable1.Size = new System.Drawing.Size(751, 483);
            this.eventsTable1.TabIndex = 0;
            // 
            // txtBaseUrl
            // 
            this.txtBaseUrl.Location = new System.Drawing.Point(429, 12);
            this.txtBaseUrl.Name = "txtBaseUrl";
            this.txtBaseUrl.Size = new System.Drawing.Size(334, 31);
            this.txtBaseUrl.TabIndex = 3;
            this.txtBaseUrl.Text = "http://localhost:61027";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(12, 12);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(381, 31);
            this.txtPath.TabIndex = 4;
            this.txtPath.Text = "c:\\WebApplicationJSON";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(841, 598);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.txtBaseUrl);
            this.Controls.Add(this.eventsTable1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form2_FormClosing);
            this.Load += new System.EventHandler(this.Form2_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form2_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private EventsTable eventsTable1;
        private System.Windows.Forms.TextBox txtBaseUrl;
        private System.Windows.Forms.TextBox txtPath;
    }
}