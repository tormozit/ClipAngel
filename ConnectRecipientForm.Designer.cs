namespace ClipAngel
{
    partial class ConnectRecipientForm
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
            this.components = new System.ComponentModel.Container();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.QuantityOfRecipients = new System.Windows.Forms.Label();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxChannelName = new System.Windows.Forms.TextBox();
            this.buttonReset = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxRecipients = new System.Windows.Forms.TextBox();
            this.checkBoxShow = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.textBoxSenderName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox.Location = new System.Drawing.Point(13, 93);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(608, 545);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(89, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Recipients:";
            // 
            // QuantityOfRecipients
            // 
            this.QuantityOfRecipients.Location = new System.Drawing.Point(147, 34);
            this.QuantityOfRecipients.Name = "QuantityOfRecipients";
            this.QuantityOfRecipients.Size = new System.Drawing.Size(20, 13);
            this.QuantityOfRecipients.TabIndex = 3;
            this.QuantityOfRecipients.Text = "QuantityOfRecipients";
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Location = new System.Drawing.Point(13, 29);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(60, 23);
            this.buttonRefresh.TabIndex = 4;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(89, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Channel name:";
            // 
            // textBoxChannelName
            // 
            this.textBoxChannelName.Location = new System.Drawing.Point(167, 6);
            this.textBoxChannelName.Name = "textBoxChannelName";
            this.textBoxChannelName.ReadOnly = true;
            this.textBoxChannelName.Size = new System.Drawing.Size(175, 20);
            this.textBoxChannelName.TabIndex = 5;
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(13, 6);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(60, 20);
            this.buttonReset.TabIndex = 4;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(72, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(546, 26);
            this.label3.TabIndex = 6;
            this.label3.Text = "Below is your private connection QR code. DO NOT SHOW IT to untrusted cameras.\r\nO" +
    "pen ClipAndroid app on device and enter \"Connect to channel\" mode. Then aim devi" +
    "ce camera to this QR code.";
            // 
            // textBoxRecipients
            // 
            this.textBoxRecipients.Location = new System.Drawing.Point(167, 31);
            this.textBoxRecipients.Name = "textBoxRecipients";
            this.textBoxRecipients.ReadOnly = true;
            this.textBoxRecipients.Size = new System.Drawing.Size(376, 20);
            this.textBoxRecipients.TabIndex = 5;
            // 
            // checkBoxShow
            // 
            this.checkBoxShow.AutoSize = true;
            this.checkBoxShow.Location = new System.Drawing.Point(13, 69);
            this.checkBoxShow.Name = "checkBoxShow";
            this.checkBoxShow.Size = new System.Drawing.Size(53, 17);
            this.checkBoxShow.TabIndex = 7;
            this.checkBoxShow.Text = "Show";
            this.checkBoxShow.UseVisualStyleBackColor = true;
            this.checkBoxShow.CheckedChanged += new System.EventHandler(this.checkBoxShow_CheckedChanged);
            // 
            // textBoxSenderName
            // 
            this.textBoxSenderName.Location = new System.Drawing.Point(434, 6);
            this.textBoxSenderName.Name = "textBoxSenderName";
            this.textBoxSenderName.ReadOnly = true;
            this.textBoxSenderName.Size = new System.Drawing.Size(109, 20);
            this.textBoxSenderName.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(356, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(73, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Sender name:";
            // 
            // ConnectRecipientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(633, 650);
            this.Controls.Add(this.textBoxSenderName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkBoxShow);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxRecipients);
            this.Controls.Add(this.textBoxChannelName);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.QuantityOfRecipients);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox);
            this.Name = "ConnectRecipientForm";
            this.Text = "Connect recipient";
            this.Load += new System.EventHandler(this.ConnectRecipientForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label QuantityOfRecipients;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxChannelName;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxRecipients;
        private System.Windows.Forms.CheckBox checkBoxShow;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox textBoxSenderName;
        private System.Windows.Forms.Label label4;
    }
}