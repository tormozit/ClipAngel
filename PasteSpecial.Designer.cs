namespace ClipAngel
{
    partial class PasteSpecial
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
            System.Windows.Forms.ToolTip toolTip1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PasteSpecial));
            this.AllUpperCase = new System.Windows.Forms.RadioButton();
            this.AllLowerCase = new System.Windows.Forms.RadioButton();
            this.SentenceCase = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.CaseNoChange = new System.Windows.Forms.RadioButton();
            this.FromCamelCase = new System.Windows.Forms.RadioButton();
            this.LowerCamelCase = new System.Windows.Forms.RadioButton();
            this.UpperCamelCase = new System.Windows.Forms.RadioButton();
            this.textBoxNumberOfSpacesInTab = new System.Windows.Forms.TextBox();
            this.NormalizeEndOfLines = new System.Windows.Forms.CheckBox();
            this.NormalizeFreeSpace = new System.Windows.Forms.CheckBox();
            this.DelimiterForTextJoin = new System.Windows.Forms.TextBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelDelimiterForTextJoin = new System.Windows.Forms.Label();
            this.checkBoxReplaceTabWithSpaces = new System.Windows.Forms.CheckBox();
            this.ReplaceEOL = new System.Windows.Forms.CheckBox();
            this.checkBoxPasteIntoNewClip = new System.Windows.Forms.CheckBox();
            toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolTip1
            // 
            toolTip1.Popup += new System.Windows.Forms.PopupEventHandler(this.toolTip1_Popup);
            // 
            // AllUpperCase
            // 
            resources.ApplyResources(this.AllUpperCase, "AllUpperCase");
            this.AllUpperCase.Name = "AllUpperCase";
            toolTip1.SetToolTip(this.AllUpperCase, resources.GetString("AllUpperCase.ToolTip"));
            this.AllUpperCase.UseVisualStyleBackColor = true;
            this.AllUpperCase.CheckedChanged += new System.EventHandler(this.AllUpper_CheckedChanged);
            // 
            // AllLowerCase
            // 
            resources.ApplyResources(this.AllLowerCase, "AllLowerCase");
            this.AllLowerCase.Name = "AllLowerCase";
            toolTip1.SetToolTip(this.AllLowerCase, resources.GetString("AllLowerCase.ToolTip"));
            this.AllLowerCase.UseVisualStyleBackColor = true;
            this.AllLowerCase.CheckedChanged += new System.EventHandler(this.AllLowerCase_CheckedChanged);
            // 
            // SentenceCase
            // 
            resources.ApplyResources(this.SentenceCase, "SentenceCase");
            this.SentenceCase.Name = "SentenceCase";
            toolTip1.SetToolTip(this.SentenceCase, resources.GetString("SentenceCase.ToolTip"));
            this.SentenceCase.UseVisualStyleBackColor = true;
            this.SentenceCase.CheckedChanged += new System.EventHandler(this.SentenceCase_CheckedChanged);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.CaseNoChange);
            this.groupBox1.Controls.Add(this.FromCamelCase);
            this.groupBox1.Controls.Add(this.LowerCamelCase);
            this.groupBox1.Controls.Add(this.UpperCamelCase);
            this.groupBox1.Controls.Add(this.SentenceCase);
            this.groupBox1.Controls.Add(this.AllUpperCase);
            this.groupBox1.Controls.Add(this.AllLowerCase);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            toolTip1.SetToolTip(this.groupBox1, resources.GetString("groupBox1.ToolTip"));
            // 
            // CaseNoChange
            // 
            resources.ApplyResources(this.CaseNoChange, "CaseNoChange");
            this.CaseNoChange.Checked = true;
            this.CaseNoChange.Name = "CaseNoChange";
            this.CaseNoChange.TabStop = true;
            toolTip1.SetToolTip(this.CaseNoChange, resources.GetString("CaseNoChange.ToolTip"));
            this.CaseNoChange.UseVisualStyleBackColor = true;
            this.CaseNoChange.CheckedChanged += new System.EventHandler(this.CaseNoChange_CheckedChanged);
            // 
            // FromCamelCase
            // 
            resources.ApplyResources(this.FromCamelCase, "FromCamelCase");
            this.FromCamelCase.Name = "FromCamelCase";
            toolTip1.SetToolTip(this.FromCamelCase, resources.GetString("FromCamelCase.ToolTip"));
            this.FromCamelCase.UseVisualStyleBackColor = true;
            this.FromCamelCase.CheckedChanged += new System.EventHandler(this.LowerCamelCase_CheckedChanged);
            // 
            // LowerCamelCase
            // 
            resources.ApplyResources(this.LowerCamelCase, "LowerCamelCase");
            this.LowerCamelCase.Name = "LowerCamelCase";
            toolTip1.SetToolTip(this.LowerCamelCase, resources.GetString("LowerCamelCase.ToolTip"));
            this.LowerCamelCase.UseVisualStyleBackColor = true;
            this.LowerCamelCase.CheckedChanged += new System.EventHandler(this.LowerCamelCase_CheckedChanged);
            // 
            // UpperCamelCase
            // 
            resources.ApplyResources(this.UpperCamelCase, "UpperCamelCase");
            this.UpperCamelCase.Name = "UpperCamelCase";
            toolTip1.SetToolTip(this.UpperCamelCase, resources.GetString("UpperCamelCase.ToolTip"));
            this.UpperCamelCase.UseVisualStyleBackColor = true;
            this.UpperCamelCase.CheckedChanged += new System.EventHandler(this.UpperCamelCase_CheckedChanged);
            // 
            // textBoxNumberOfSpacesInTab
            // 
            resources.ApplyResources(this.textBoxNumberOfSpacesInTab, "textBoxNumberOfSpacesInTab");
            this.textBoxNumberOfSpacesInTab.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::ClipAngel.Properties.Settings.Default, "NumberOfSpacesInTab", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBoxNumberOfSpacesInTab.Name = "textBoxNumberOfSpacesInTab";
            this.textBoxNumberOfSpacesInTab.Text = global::ClipAngel.Properties.Settings.Default.NumberOfSpacesInTab;
            toolTip1.SetToolTip(this.textBoxNumberOfSpacesInTab, resources.GetString("textBoxNumberOfSpacesInTab.ToolTip"));
            this.textBoxNumberOfSpacesInTab.TextChanged += new System.EventHandler(this.textBoxNumberOfSpacesInTab_TextChanged);
            // 
            // NormalizeEndOfLines
            // 
            resources.ApplyResources(this.NormalizeEndOfLines, "NormalizeEndOfLines");
            this.NormalizeEndOfLines.Checked = global::ClipAngel.Properties.Settings.Default.NormalizeEOF;
            this.NormalizeEndOfLines.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::ClipAngel.Properties.Settings.Default, "NormalizeEOF", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.NormalizeEndOfLines.Name = "NormalizeEndOfLines";
            toolTip1.SetToolTip(this.NormalizeEndOfLines, resources.GetString("NormalizeEndOfLines.ToolTip"));
            this.NormalizeEndOfLines.UseVisualStyleBackColor = true;
            this.NormalizeEndOfLines.CheckedChanged += new System.EventHandler(this.NormalizeEndOfLines_CheckedChanged);
            // 
            // NormalizeFreeSpace
            // 
            resources.ApplyResources(this.NormalizeFreeSpace, "NormalizeFreeSpace");
            this.NormalizeFreeSpace.Checked = global::ClipAngel.Properties.Settings.Default.NormalizeNonPrintable;
            this.NormalizeFreeSpace.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::ClipAngel.Properties.Settings.Default, "NormalizeNonPrintable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.NormalizeFreeSpace.Name = "NormalizeFreeSpace";
            toolTip1.SetToolTip(this.NormalizeFreeSpace, resources.GetString("NormalizeFreeSpace.ToolTip"));
            this.NormalizeFreeSpace.UseVisualStyleBackColor = true;
            this.NormalizeFreeSpace.CheckedChanged += new System.EventHandler(this.NormalizeFreeSpace_CheckedChanged);
            // 
            // DelimiterForTextJoin
            // 
            resources.ApplyResources(this.DelimiterForTextJoin, "DelimiterForTextJoin");
            this.DelimiterForTextJoin.Name = "DelimiterForTextJoin";
            toolTip1.SetToolTip(this.DelimiterForTextJoin, resources.GetString("DelimiterForTextJoin.ToolTip"));
            this.DelimiterForTextJoin.TextChanged += new System.EventHandler(this.DelimiterForTextJoin_TextChanged);
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            toolTip1.SetToolTip(this.buttonCancel, resources.GetString("buttonCancel.ToolTip"));
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Name = "buttonOK";
            toolTip1.SetToolTip(this.buttonOK, resources.GetString("buttonOK.ToolTip"));
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // richTextBox1
            // 
            resources.ApplyResources(this.richTextBox1, "richTextBox1");
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            toolTip1.SetToolTip(this.richTextBox1, resources.GetString("richTextBox1.ToolTip"));
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            toolTip1.SetToolTip(this.label1, resources.GetString("label1.ToolTip"));
            // 
            // labelDelimiterForTextJoin
            // 
            resources.ApplyResources(this.labelDelimiterForTextJoin, "labelDelimiterForTextJoin");
            this.labelDelimiterForTextJoin.Name = "labelDelimiterForTextJoin";
            toolTip1.SetToolTip(this.labelDelimiterForTextJoin, resources.GetString("labelDelimiterForTextJoin.ToolTip"));
            // 
            // checkBoxReplaceTabWithSpaces
            // 
            resources.ApplyResources(this.checkBoxReplaceTabWithSpaces, "checkBoxReplaceTabWithSpaces");
            this.checkBoxReplaceTabWithSpaces.Checked = global::ClipAngel.Properties.Settings.Default.ReplaceTABWithSpaces;
            this.checkBoxReplaceTabWithSpaces.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::ClipAngel.Properties.Settings.Default, "ReplaceTABWithSpaces", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxReplaceTabWithSpaces.Name = "checkBoxReplaceTabWithSpaces";
            toolTip1.SetToolTip(this.checkBoxReplaceTabWithSpaces, resources.GetString("checkBoxReplaceTabWithSpaces.ToolTip"));
            this.checkBoxReplaceTabWithSpaces.UseVisualStyleBackColor = true;
            this.checkBoxReplaceTabWithSpaces.CheckedChanged += new System.EventHandler(this.checkBoxReplaceTabWithSpaces_CheckedChanged);
            // 
            // ReplaceEOL
            // 
            resources.ApplyResources(this.ReplaceEOL, "ReplaceEOL");
            this.ReplaceEOL.Checked = global::ClipAngel.Properties.Settings.Default.ReplaceEOFWithSpace;
            this.ReplaceEOL.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::ClipAngel.Properties.Settings.Default, "ReplaceEOFWithSpace", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ReplaceEOL.Name = "ReplaceEOL";
            toolTip1.SetToolTip(this.ReplaceEOL, resources.GetString("ReplaceEOL.ToolTip"));
            this.ReplaceEOL.UseVisualStyleBackColor = true;
            this.ReplaceEOL.CheckedChanged += new System.EventHandler(this.checkBoxReplaceEOL_CheckedChanged);
            // 
            // checkBoxPasteIntoNewClip
            // 
            resources.ApplyResources(this.checkBoxPasteIntoNewClip, "checkBoxPasteIntoNewClip");
            this.checkBoxPasteIntoNewClip.Checked = global::ClipAngel.Properties.Settings.Default.PasteIntoNewClip;
            this.checkBoxPasteIntoNewClip.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::ClipAngel.Properties.Settings.Default, "PasteIntoNewClip", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBoxPasteIntoNewClip.Name = "checkBoxPasteIntoNewClip";
            toolTip1.SetToolTip(this.checkBoxPasteIntoNewClip, resources.GetString("checkBoxPasteIntoNewClip.ToolTip"));
            this.checkBoxPasteIntoNewClip.UseVisualStyleBackColor = true;
            // 
            // PasteSpecial
            // 
            this.AcceptButton = this.buttonOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.labelDelimiterForTextJoin);
            this.Controls.Add(this.DelimiterForTextJoin);
            this.Controls.Add(this.textBoxNumberOfSpacesInTab);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkBoxReplaceTabWithSpaces);
            this.Controls.Add(this.ReplaceEOL);
            this.Controls.Add(this.checkBoxPasteIntoNewClip);
            this.Controls.Add(this.NormalizeEndOfLines);
            this.Controls.Add(this.NormalizeFreeSpace);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PasteSpecial";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            toolTip1.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.Load += new System.EventHandler(this.SpecialPaste_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.CheckBox NormalizeFreeSpace;
        private System.Windows.Forms.RadioButton AllUpperCase;
        private System.Windows.Forms.RadioButton AllLowerCase;
        private System.Windows.Forms.RadioButton SentenceCase;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton CaseNoChange;
        private System.Windows.Forms.CheckBox ReplaceEOL;
        private System.Windows.Forms.CheckBox checkBoxPasteIntoNewClip;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxReplaceTabWithSpaces;
        private System.Windows.Forms.TextBox textBoxNumberOfSpacesInTab;
        private System.Windows.Forms.RadioButton LowerCamelCase;
        private System.Windows.Forms.RadioButton UpperCamelCase;
        private System.Windows.Forms.RadioButton FromCamelCase;
        private System.Windows.Forms.CheckBox NormalizeEndOfLines;
        private System.Windows.Forms.TextBox DelimiterForTextJoin;
        private System.Windows.Forms.Label labelDelimiterForTextJoin;
    }
}