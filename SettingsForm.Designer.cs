namespace ClipAngel
{
    partial class SettingsForm
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
            System.Windows.Forms.Button buttonReset;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonClearFilter = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.propertyGrid1 = new ClipAngel.FilteredPropertyGrid();
            this.contexMenuLabel = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contexMenuGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.gridContextMenuCopy = new System.Windows.Forms.ToolStripMenuItem();
            buttonReset = new System.Windows.Forms.Button();
            this.contexMenuLabel.SuspendLayout();
            this.contexMenuGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonReset
            // 
            resources.ApplyResources(buttonReset, "buttonReset");
            buttonReset.Name = "buttonReset";
            buttonReset.UseVisualStyleBackColor = true;
            buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click_1);
            // 
            // buttonClearFilter
            // 
            resources.ApplyResources(this.buttonClearFilter, "buttonClearFilter");
            this.buttonClearFilter.Name = "buttonClearFilter";
            this.buttonClearFilter.TabStop = false;
            this.toolTip1.SetToolTip(this.buttonClearFilter, resources.GetString("buttonClearFilter.ToolTip"));
            this.buttonClearFilter.UseVisualStyleBackColor = true;
            this.buttonClearFilter.Click += new System.EventHandler(this.buttonClearFilter_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // textBoxFilter
            // 
            resources.ApplyResources(this.textBoxFilter, "textBoxFilter");
            this.textBoxFilter.Name = "textBoxFilter";
            this.toolTip1.SetToolTip(this.textBoxFilter, resources.GetString("textBoxFilter.ToolTip"));
            this.textBoxFilter.TextChanged += new System.EventHandler(this.comboBoxFilter_TextChanged);
            // 
            // propertyGrid1
            // 
            resources.ApplyResources(this.propertyGrid1, "propertyGrid1");
            this.propertyGrid1.BrowsableProperties = null;
            this.propertyGrid1.HiddenAttributes = null;
            this.propertyGrid1.HiddenProperties = null;
            this.propertyGrid1.Name = "propertyGrid1";
            // 
            // contexMenuLabel
            // 
            this.contexMenuLabel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem});
            this.contexMenuLabel.Name = "contexMenuLabel";
            resources.ApplyResources(this.contexMenuLabel, "contexMenuLabel");
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            resources.ApplyResources(this.copyToolStripMenuItem, "copyToolStripMenuItem");
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // contexMenuGrid
            // 
            this.contexMenuGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gridContextMenuCopy});
            this.contexMenuGrid.Name = "contexMenuGrid";
            resources.ApplyResources(this.contexMenuGrid, "contexMenuGrid");
            // 
            // gridContextMenuCopy
            // 
            this.gridContextMenuCopy.Name = "gridContextMenuCopy";
            resources.ApplyResources(this.gridContextMenuCopy, "gridContextMenuCopy");
            this.gridContextMenuCopy.Click += new System.EventHandler(this.gridContextMenuCopy_Click);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.buttonOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.textBoxFilter);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonClearFilter);
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(buttonReset);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.Load += new System.EventHandler(this.Settings_Load);
            this.contexMenuLabel.ResumeLayout(false);
            this.contexMenuGrid.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        //private System.Windows.Forms.PropertyGrid propertyGrid1;
        private FilteredPropertyGrid propertyGrid1;
        private System.Windows.Forms.Button buttonClearFilter;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxFilter;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ContextMenuStrip contexMenuLabel;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contexMenuGrid;
        private System.Windows.Forms.ToolStripMenuItem gridContextMenuCopy;
    }
}