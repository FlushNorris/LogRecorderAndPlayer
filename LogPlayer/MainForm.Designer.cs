namespace LogPlayer
{
    partial class MainForm
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
            this.eventsTable1 = new LogPlayer.EventsTable();
            this.txtBaseUrl = new System.Windows.Forms.TextBox();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // eventsTable1
            // 
            this.eventsTable1.ColumnCount = 3;
            this.eventsTable1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.eventsTable1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.eventsTable1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.eventsTable1.Location = new System.Drawing.Point(6, 257);
            this.eventsTable1.Margin = new System.Windows.Forms.Padding(2);
            this.eventsTable1.Name = "eventsTable1";
            this.eventsTable1.PageElements = 100;
            this.eventsTable1.PageIndex = 0;
            this.eventsTable1.RowCount = 3;
            this.eventsTable1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.eventsTable1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.eventsTable1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.eventsTable1.Size = new System.Drawing.Size(376, 251);
            this.eventsTable1.TabIndex = 0;
            // 
            // txtBaseUrl
            // 
            this.txtBaseUrl.Location = new System.Drawing.Point(308, 6);
            this.txtBaseUrl.Margin = new System.Windows.Forms.Padding(2);
            this.txtBaseUrl.Name = "txtBaseUrl";
            this.txtBaseUrl.Size = new System.Drawing.Size(169, 20);
            this.txtBaseUrl.TabIndex = 3;
            this.txtBaseUrl.Text = "http://localhost:61027";
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(6, 6);
            this.txtPath.Margin = new System.Windows.Forms.Padding(2);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(192, 20);
            this.txtPath.TabIndex = 4;
            this.txtPath.Text = "c:\\WebApplicationJSON";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(6, 31);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(377, 221);
            this.textBox1.TabIndex = 5;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(204, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Load logdata";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(420, 525);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.txtBaseUrl);
            this.Controls.Add(this.eventsTable1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
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
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
    }
}