namespace ClipAngel
{
    partial class Tips
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Tips));
            this.DontShowTipsOnStart = new System.Windows.Forms.CheckBox();
            this.labelTip = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // DontShowTipsOnStart
            // 
            resources.ApplyResources(this.DontShowTipsOnStart, "DontShowTipsOnStart");
            this.DontShowTipsOnStart.Name = "DontShowTipsOnStart";
            this.DontShowTipsOnStart.UseVisualStyleBackColor = true;
            // 
            // labelTip
            // 
            resources.ApplyResources(this.labelTip, "labelTip");
            this.labelTip.Name = "labelTip";
            this.labelTip.Click += new System.EventHandler(this.labelTip_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ClipAngel.Properties.Resources.Tip_32;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // Tips
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.labelTip);
            this.Controls.Add(this.DontShowTipsOnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Tips";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Tips_FormClosed);
            this.Load += new System.EventHandler(this.Tips_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Tips_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox DontShowTipsOnStart;
        private System.Windows.Forms.Label labelTip;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}