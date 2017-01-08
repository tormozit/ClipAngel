namespace ClipAngel
{
    partial class Settings
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
            this.HistoryDepthNumber = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.GlobalHotkeyShowWindow = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.NumberOfClips = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Autostart = new System.Windows.Forms.CheckBox();
            this.MaxClipSizeKB = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // HistoryDepthNumber
            // 
            this.HistoryDepthNumber.Location = new System.Drawing.Point(175, 10);
            this.HistoryDepthNumber.Name = "HistoryDepthNumber";
            this.HistoryDepthNumber.Size = new System.Drawing.Size(97, 20);
            this.HistoryDepthNumber.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "History depth number";
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(197, 239);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.button1_Click);
            // 
            // GlobalHotkeyShowWindow
            // 
            this.GlobalHotkeyShowWindow.Location = new System.Drawing.Point(175, 89);
            this.GlobalHotkeyShowWindow.Name = "GlobalHotkeyShowWindow";
            this.GlobalHotkeyShowWindow.ReadOnly = true;
            this.GlobalHotkeyShowWindow.Size = new System.Drawing.Size(97, 20);
            this.GlobalHotkeyShowWindow.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(151, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Global hotkey to show window";
            // 
            // NumberOfClips
            // 
            this.NumberOfClips.Location = new System.Drawing.Point(175, 37);
            this.NumberOfClips.Name = "NumberOfClips";
            this.NumberOfClips.ReadOnly = true;
            this.NumberOfClips.Size = new System.Drawing.Size(97, 20);
            this.NumberOfClips.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Numer of clips";
            // 
            // Autostart
            // 
            this.Autostart.AutoSize = true;
            this.Autostart.Location = new System.Drawing.Point(12, 117);
            this.Autostart.Name = "Autostart";
            this.Autostart.Size = new System.Drawing.Size(68, 17);
            this.Autostart.TabIndex = 7;
            this.Autostart.Text = "Autostart";
            this.Autostart.UseVisualStyleBackColor = true;
            this.Autostart.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // MaxClipSizeKB
            // 
            this.MaxClipSizeKB.Location = new System.Drawing.Point(175, 63);
            this.MaxClipSizeKB.Name = "MaxClipSizeKB";
            this.MaxClipSizeKB.Size = new System.Drawing.Size(97, 20);
            this.MaxClipSizeKB.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Max clip size KB";
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 265);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.MaxClipSizeKB);
            this.Controls.Add(this.Autostart);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.NumberOfClips);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.GlobalHotkeyShowWindow);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.HistoryDepthNumber);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Settings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Settings_FormClosing);
            this.Load += new System.EventHandler(this.Settings_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox HistoryDepthNumber;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TextBox GlobalHotkeyShowWindow;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox NumberOfClips;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox Autostart;
        private System.Windows.Forms.TextBox MaxClipSizeKB;
        private System.Windows.Forms.Label label4;
    }
}