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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.HistoryDepthNumber = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.GlobalHotkeyShow = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.NumberOfClips = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.MaxClipSizeKB = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.Language = new System.Windows.Forms.ComboBox();
            this.GlobalHotkeyIncrementalPaste = new System.Windows.Forms.TextBox();
            this.checkBoxClipListSimpleDraw = new System.Windows.Forms.CheckBox();
            this.checkBoxShowSizeColumn = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoCheckUpdate = new System.Windows.Forms.CheckBox();
            this.checkBoxMoveCopiedClipToTop = new System.Windows.Forms.CheckBox();
            this.checkBoxWindowAutoPosition = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxUserSettingsPath = new System.Windows.Forms.TextBox();
            this.textBoxDatabaseSize = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.checkBoxAutostart = new System.Windows.Forms.CheckBox();
            this.checkBoxClearFiltersOnClose = new System.Windows.Forms.CheckBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.cultureManager1 = new Infralution.Localization.CultureManager(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // HistoryDepthNumber
            // 
            resources.ApplyResources(this.HistoryDepthNumber, "HistoryDepthNumber");
            this.HistoryDepthNumber.Name = "HistoryDepthNumber";
            this.toolTip1.SetToolTip(this.HistoryDepthNumber, resources.GetString("HistoryDepthNumber.ToolTip"));
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.button1_Click);
            // 
            // GlobalHotkeyShow
            // 
            resources.ApplyResources(this.GlobalHotkeyShow, "GlobalHotkeyShow");
            this.GlobalHotkeyShow.Name = "GlobalHotkeyShow";
            this.toolTip1.SetToolTip(this.GlobalHotkeyShow, resources.GetString("GlobalHotkeyShow.ToolTip"));
            this.GlobalHotkeyShow.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HotkeyTextBox_KeyDown);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // NumberOfClips
            // 
            resources.ApplyResources(this.NumberOfClips, "NumberOfClips");
            this.NumberOfClips.Name = "NumberOfClips";
            this.NumberOfClips.ReadOnly = true;
            this.toolTip1.SetToolTip(this.NumberOfClips, resources.GetString("NumberOfClips.ToolTip"));
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // MaxClipSizeKB
            // 
            resources.ApplyResources(this.MaxClipSizeKB, "MaxClipSizeKB");
            this.MaxClipSizeKB.Name = "MaxClipSizeKB";
            this.toolTip1.SetToolTip(this.MaxClipSizeKB, resources.GetString("MaxClipSizeKB.ToolTip"));
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // Language
            // 
            resources.ApplyResources(this.Language, "Language");
            this.Language.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Language.FormattingEnabled = true;
            this.Language.Items.AddRange(new object[] {
            resources.GetString("Language.Items"),
            resources.GetString("Language.Items1"),
            resources.GetString("Language.Items2")});
            this.Language.Name = "Language";
            this.toolTip1.SetToolTip(this.Language, resources.GetString("Language.ToolTip"));
            // 
            // GlobalHotkeyIncrementalPaste
            // 
            resources.ApplyResources(this.GlobalHotkeyIncrementalPaste, "GlobalHotkeyIncrementalPaste");
            this.GlobalHotkeyIncrementalPaste.Name = "GlobalHotkeyIncrementalPaste";
            this.toolTip1.SetToolTip(this.GlobalHotkeyIncrementalPaste, resources.GetString("GlobalHotkeyIncrementalPaste.ToolTip"));
            this.GlobalHotkeyIncrementalPaste.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HotkeyTextBox_KeyDown);
            // 
            // checkBoxClipListSimpleDraw
            // 
            resources.ApplyResources(this.checkBoxClipListSimpleDraw, "checkBoxClipListSimpleDraw");
            this.tableLayoutPanel1.SetColumnSpan(this.checkBoxClipListSimpleDraw, 2);
            this.checkBoxClipListSimpleDraw.Name = "checkBoxClipListSimpleDraw";
            this.toolTip1.SetToolTip(this.checkBoxClipListSimpleDraw, resources.GetString("checkBoxClipListSimpleDraw.ToolTip"));
            this.checkBoxClipListSimpleDraw.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowSizeColumn
            // 
            resources.ApplyResources(this.checkBoxShowSizeColumn, "checkBoxShowSizeColumn");
            this.tableLayoutPanel1.SetColumnSpan(this.checkBoxShowSizeColumn, 2);
            this.checkBoxShowSizeColumn.Name = "checkBoxShowSizeColumn";
            this.toolTip1.SetToolTip(this.checkBoxShowSizeColumn, resources.GetString("checkBoxShowSizeColumn.ToolTip"));
            // 
            // checkBoxAutoCheckUpdate
            // 
            resources.ApplyResources(this.checkBoxAutoCheckUpdate, "checkBoxAutoCheckUpdate");
            this.tableLayoutPanel1.SetColumnSpan(this.checkBoxAutoCheckUpdate, 2);
            this.checkBoxAutoCheckUpdate.Name = "checkBoxAutoCheckUpdate";
            this.toolTip1.SetToolTip(this.checkBoxAutoCheckUpdate, resources.GetString("checkBoxAutoCheckUpdate.ToolTip"));
            this.checkBoxAutoCheckUpdate.UseVisualStyleBackColor = true;
            // 
            // checkBoxMoveCopiedClipToTop
            // 
            resources.ApplyResources(this.checkBoxMoveCopiedClipToTop, "checkBoxMoveCopiedClipToTop");
            this.tableLayoutPanel1.SetColumnSpan(this.checkBoxMoveCopiedClipToTop, 2);
            this.checkBoxMoveCopiedClipToTop.Name = "checkBoxMoveCopiedClipToTop";
            this.toolTip1.SetToolTip(this.checkBoxMoveCopiedClipToTop, resources.GetString("checkBoxMoveCopiedClipToTop.ToolTip"));
            // 
            // checkBoxWindowAutoPosition
            // 
            resources.ApplyResources(this.checkBoxWindowAutoPosition, "checkBoxWindowAutoPosition");
            this.tableLayoutPanel1.SetColumnSpan(this.checkBoxWindowAutoPosition, 2);
            this.checkBoxWindowAutoPosition.Name = "checkBoxWindowAutoPosition";
            this.toolTip1.SetToolTip(this.checkBoxWindowAutoPosition, resources.GetString("checkBoxWindowAutoPosition.ToolTip"));
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // textBoxUserSettingsPath
            // 
            resources.ApplyResources(this.textBoxUserSettingsPath, "textBoxUserSettingsPath");
            this.textBoxUserSettingsPath.Name = "textBoxUserSettingsPath";
            this.textBoxUserSettingsPath.ReadOnly = true;
            // 
            // textBoxDatabaseSize
            // 
            resources.ApplyResources(this.textBoxDatabaseSize, "textBoxDatabaseSize");
            this.textBoxDatabaseSize.Name = "textBoxDatabaseSize";
            this.textBoxDatabaseSize.ReadOnly = true;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.textBoxDatabaseSize, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.textBoxUserSettingsPath, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.GlobalHotkeyShow, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.MaxClipSizeKB, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.NumberOfClips, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.HistoryDepthNumber, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.Language, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxShowSizeColumn, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.GlobalHotkeyIncrementalPaste, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxClipListSimpleDraw, 0, 11);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxMoveCopiedClipToTop, 0, 10);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxWindowAutoPosition, 0, 12);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxAutostart, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxAutoCheckUpdate, 0, 13);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxClearFiltersOnClose, 0, 14);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // checkBoxAutostart
            // 
            resources.ApplyResources(this.checkBoxAutostart, "checkBoxAutostart");
            this.tableLayoutPanel1.SetColumnSpan(this.checkBoxAutostart, 2);
            this.checkBoxAutostart.Name = "checkBoxAutostart";
            this.checkBoxAutostart.UseVisualStyleBackColor = true;
            // 
            // checkBoxClearFiltersOnClose
            // 
            resources.ApplyResources(this.checkBoxClearFiltersOnClose, "checkBoxClearFiltersOnClose");
            this.tableLayoutPanel1.SetColumnSpan(this.checkBoxClearFiltersOnClose, 2);
            this.checkBoxClearFiltersOnClose.Name = "checkBoxClearFiltersOnClose";
            this.checkBoxClearFiltersOnClose.UseVisualStyleBackColor = true;
            this.checkBoxClearFiltersOnClose.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged_1);
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // cultureManager1
            // 
            this.cultureManager1.ManagedControl = this;
            // 
            // Settings
            // 
            this.AcceptButton = this.buttonOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Settings_FormClosing);
            this.Load += new System.EventHandler(this.Settings_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox HistoryDepthNumber;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TextBox GlobalHotkeyShow;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox NumberOfClips;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox MaxClipSizeKB;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox Language;
        private Infralution.Localization.CultureManager cultureManager1;
        private System.Windows.Forms.CheckBox checkBoxMoveCopiedClipToTop;
        private System.Windows.Forms.CheckBox checkBoxShowSizeColumn;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox GlobalHotkeyIncrementalPaste;
        private System.Windows.Forms.CheckBox checkBoxClipListSimpleDraw;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox checkBoxWindowAutoPosition;
        private System.Windows.Forms.TextBox textBoxDatabaseSize;
        private System.Windows.Forms.TextBox textBoxUserSettingsPath;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox checkBoxAutostart;
        private System.Windows.Forms.CheckBox checkBoxAutoCheckUpdate;
        private System.Windows.Forms.CheckBox checkBoxClearFiltersOnClose;
    }
}