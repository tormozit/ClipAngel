namespace ClipAngel
{
    partial class AppListEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppListEditorForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label2 = new System.Windows.Forms.Label();
            this.listViewChosenList = new System.Windows.Forms.ListView();
            this.SelectedListModule = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SelectedListApplication = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.listViewRunningList = new System.Windows.Forms.ListView();
            this.RunningListModule = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RunningListApplication = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonSelectClipboardOwner = new System.Windows.Forms.Button();
            this.buttonSelectTargetApplication = new System.Windows.Forms.Button();
            this.buttonAddByFile = new System.Windows.Forms.Button();
            this.buttonDeleteSelected = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.buttonRefresh = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.listViewChosenList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.listViewRunningList);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // listViewChosenList
            // 
            resources.ApplyResources(this.listViewChosenList, "listViewChosenList");
            this.listViewChosenList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.SelectedListModule,
            this.SelectedListApplication});
            this.listViewChosenList.FullRowSelect = true;
            this.listViewChosenList.HideSelection = false;
            this.listViewChosenList.Name = "listViewChosenList";
            this.listViewChosenList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewChosenList.UseCompatibleStateImageBehavior = false;
            this.listViewChosenList.View = System.Windows.Forms.View.Details;
            this.listViewChosenList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewChosenList_KeyDown);
            // 
            // SelectedListModule
            // 
            resources.ApplyResources(this.SelectedListModule, "SelectedListModule");
            // 
            // SelectedListApplication
            // 
            resources.ApplyResources(this.SelectedListApplication, "SelectedListApplication");
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // listViewRunningList
            // 
            resources.ApplyResources(this.listViewRunningList, "listViewRunningList");
            this.listViewRunningList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.RunningListModule,
            this.RunningListApplication});
            this.listViewRunningList.FullRowSelect = true;
            this.listViewRunningList.HideSelection = false;
            this.listViewRunningList.Name = "listViewRunningList";
            this.listViewRunningList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewRunningList.UseCompatibleStateImageBehavior = false;
            this.listViewRunningList.View = System.Windows.Forms.View.Details;
            this.listViewRunningList.DoubleClick += new System.EventHandler(this.listViewRunningList_DoubleClick);
            this.listViewRunningList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewRunningList_KeyDown);
            // 
            // RunningListModule
            // 
            resources.ApplyResources(this.RunningListModule, "RunningListModule");
            // 
            // RunningListApplication
            // 
            resources.ApplyResources(this.RunningListApplication, "RunningListApplication");
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
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonSelectClipboardOwner
            // 
            resources.ApplyResources(this.buttonSelectClipboardOwner, "buttonSelectClipboardOwner");
            this.buttonSelectClipboardOwner.Name = "buttonSelectClipboardOwner";
            this.toolTip1.SetToolTip(this.buttonSelectClipboardOwner, resources.GetString("buttonSelectClipboardOwner.ToolTip"));
            this.buttonSelectClipboardOwner.UseVisualStyleBackColor = true;
            this.buttonSelectClipboardOwner.Click += new System.EventHandler(this.buttonSelectClipboardOwner_Click);
            // 
            // buttonSelectTargetApplication
            // 
            resources.ApplyResources(this.buttonSelectTargetApplication, "buttonSelectTargetApplication");
            this.buttonSelectTargetApplication.Name = "buttonSelectTargetApplication";
            this.toolTip1.SetToolTip(this.buttonSelectTargetApplication, resources.GetString("buttonSelectTargetApplication.ToolTip"));
            this.buttonSelectTargetApplication.UseVisualStyleBackColor = true;
            this.buttonSelectTargetApplication.Click += new System.EventHandler(this.buttonSelectTargetApplication_Click);
            // 
            // buttonAddByFile
            // 
            resources.ApplyResources(this.buttonAddByFile, "buttonAddByFile");
            this.buttonAddByFile.Name = "buttonAddByFile";
            this.toolTip1.SetToolTip(this.buttonAddByFile, resources.GetString("buttonAddByFile.ToolTip"));
            this.buttonAddByFile.UseVisualStyleBackColor = true;
            this.buttonAddByFile.Click += new System.EventHandler(this.buttonAddByFile_Click);
            // 
            // buttonDeleteSelected
            // 
            this.buttonDeleteSelected.Cursor = System.Windows.Forms.Cursors.Default;
            this.buttonDeleteSelected.Image = global::ClipAngel.Properties.Resources.delete;
            resources.ApplyResources(this.buttonDeleteSelected, "buttonDeleteSelected");
            this.buttonDeleteSelected.Name = "buttonDeleteSelected";
            this.buttonDeleteSelected.UseVisualStyleBackColor = true;
            this.buttonDeleteSelected.Click += new System.EventHandler(this.buttonDeleteSelected_Click);
            // 
            // buttonAdd
            // 
            resources.ApplyResources(this.buttonAdd, "buttonAdd");
            this.buttonAdd.Name = "buttonAdd";
            this.toolTip1.SetToolTip(this.buttonAdd, resources.GetString("buttonAdd.ToolTip"));
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonRefresh
            // 
            resources.ApplyResources(this.buttonRefresh, "buttonRefresh");
            this.buttonRefresh.Name = "buttonRefresh";
            this.toolTip1.SetToolTip(this.buttonRefresh, resources.GetString("buttonRefresh.ToolTip"));
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // AppListEditorForm
            // 
            this.AcceptButton = this.buttonOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.buttonDeleteSelected);
            this.Controls.Add(this.buttonAddByFile);
            this.Controls.Add(this.buttonSelectTargetApplication);
            this.Controls.Add(this.buttonSelectClipboardOwner);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.MinimizeBox = false;
            this.Name = "AppListEditorForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.AppListEditorForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonSelectClipboardOwner;
        private System.Windows.Forms.Button buttonSelectTargetApplication;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView listViewChosenList;
        private System.Windows.Forms.ColumnHeader SelectedListApplication;
        private System.Windows.Forms.ListView listViewRunningList;
        private System.Windows.Forms.ColumnHeader RunningListApplication;
        private System.Windows.Forms.Button buttonAddByFile;
        private System.Windows.Forms.Button buttonDeleteSelected;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ColumnHeader SelectedListModule;
        private System.Windows.Forms.ColumnHeader RunningListModule;
        private System.Windows.Forms.Button buttonRefresh;
    }
}