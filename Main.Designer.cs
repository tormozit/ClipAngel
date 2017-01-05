namespace ClipAngel
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.titleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.typeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clipBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dataSet1 = new ClipAngel.DataSet1();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.Delete = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ClearFilter = new System.Windows.Forms.Button();
            this.Filter = new System.Windows.Forms.ComboBox();
            this.Text = new System.Windows.Forms.TextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.Position = new System.Windows.Forms.ToolStripStatusLabel();
            this.Chars = new System.Windows.Forms.ToolStripStatusLabel();
            this.Size = new System.Windows.Forms.ToolStripStatusLabel();
            this.Type = new System.Windows.Forms.ToolStripStatusLabel();
            this.Created = new System.Windows.Forms.ToolStripStatusLabel();
            this.Window = new System.Windows.Forms.TextBox();
            this.Application = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.clipBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSet1)).BeginInit();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AllowUserToResizeColumns = false;
            this.dataGridView.AllowUserToResizeRows = false;
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.AutoGenerateColumns = false;
            this.dataGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.titleDataGridViewTextBoxColumn,
            this.typeDataGridViewTextBoxColumn});
            this.dataGridView.DataSource = this.clipBindingSource;
            this.dataGridView.Location = new System.Drawing.Point(1, 3);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.ReadOnly = true;
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.Size = new System.Drawing.Size(183, 326);
            this.dataGridView.TabIndex = 0;
            this.dataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_RowEnter);
            // 
            // titleDataGridViewTextBoxColumn
            // 
            this.titleDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.titleDataGridViewTextBoxColumn.DataPropertyName = "Title";
            this.titleDataGridViewTextBoxColumn.HeaderText = "Title";
            this.titleDataGridViewTextBoxColumn.Name = "titleDataGridViewTextBoxColumn";
            this.titleDataGridViewTextBoxColumn.ReadOnly = true;
            this.titleDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // typeDataGridViewTextBoxColumn
            // 
            this.typeDataGridViewTextBoxColumn.DataPropertyName = "Type";
            this.typeDataGridViewTextBoxColumn.HeaderText = "Type";
            this.typeDataGridViewTextBoxColumn.MinimumWidth = 30;
            this.typeDataGridViewTextBoxColumn.Name = "typeDataGridViewTextBoxColumn";
            this.typeDataGridViewTextBoxColumn.ReadOnly = true;
            this.typeDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.typeDataGridViewTextBoxColumn.Width = 30;
            // 
            // clipBindingSource
            // 
            this.clipBindingSource.DataMember = "Clip";
            this.clipBindingSource.DataSource = this.dataSet1;
            // 
            // dataSet1
            // 
            this.dataSet1.DataSetName = "DataSet1";
            this.dataSet1.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Delete});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(554, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStripMain";
            // 
            // Delete
            // 
            this.Delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.Delete.Image = ((System.Drawing.Image)(resources.GetObject("Delete.Image")));
            this.Delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(23, 22);
            this.Delete.Text = "Delete";
            this.Delete.ToolTipText = "Deletes active clip";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
            this.splitContainer1.Panel1.Controls.Add(this.ClearFilter);
            this.splitContainer1.Panel1.Controls.Add(this.Filter);
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.AutoScroll = true;
            this.splitContainer1.Panel2.Controls.Add(this.Text);
            this.splitContainer1.Panel2.Controls.Add(this.statusStrip);
            this.splitContainer1.Panel2.Controls.Add(this.Window);
            this.splitContainer1.Panel2.Controls.Add(this.Application);
            this.splitContainer1.Size = new System.Drawing.Size(554, 355);
            this.splitContainer1.SplitterDistance = 184;
            this.splitContainer1.TabIndex = 3;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Image = global::ClipAngel.Properties.Resources.Filter;
            this.pictureBox1.Location = new System.Drawing.Point(3, 332);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(11, 20);
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // ClearFilter
            // 
            this.ClearFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ClearFilter.Location = new System.Drawing.Point(167, 331);
            this.ClearFilter.Name = "ClearFilter";
            this.ClearFilter.Size = new System.Drawing.Size(16, 23);
            this.ClearFilter.TabIndex = 6;
            this.ClearFilter.Text = "X";
            this.ClearFilter.UseVisualStyleBackColor = true;
            this.ClearFilter.Click += new System.EventHandler(this.ClearFilter_Click);
            // 
            // Filter
            // 
            this.Filter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Filter.FormattingEnabled = true;
            this.Filter.Location = new System.Drawing.Point(17, 332);
            this.Filter.Name = "Filter";
            this.Filter.Size = new System.Drawing.Size(150, 21);
            this.Filter.TabIndex = 6;
            this.Filter.SelectedIndexChanged += new System.EventHandler(this.Filter_SelectedIndexChanged);
            this.Filter.TextChanged += new System.EventHandler(this.Filter_TextChanged);
            // 
            // Text
            // 
            this.Text.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Text.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.clipBindingSource, "Text", true));
            this.Text.Location = new System.Drawing.Point(4, 29);
            this.Text.Multiline = true;
            this.Text.Name = "Text";
            this.Text.ReadOnly = true;
            this.Text.Size = new System.Drawing.Size(359, 300);
            this.Text.TabIndex = 5;
            this.Text.Text = "Text";
            this.Text.Click += new System.EventHandler(this.Text_Click);
            this.Text.CursorChanged += new System.EventHandler(this.Text_CursorChanged);
            // 
            // statusStrip
            // 
            this.statusStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusStrip.AutoSize = false;
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Position,
            this.Chars,
            this.Size,
            this.Type,
            this.Created});
            this.statusStrip.Location = new System.Drawing.Point(0, 333);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(366, 22);
            this.statusStrip.TabIndex = 4;
            this.statusStrip.Text = "statusStrip";
            this.statusStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.statusStrip1_ItemClicked);
            // 
            // Position
            // 
            this.Position.AutoSize = false;
            this.Position.AutoToolTip = true;
            this.Position.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.Position.Name = "Position";
            this.Position.RightToLeftAutoMirrorImage = true;
            this.Position.Size = new System.Drawing.Size(80, 17);
            this.Position.Text = "Position";
            this.Position.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Position.ToolTipText = "Current position in text";
            this.Position.Click += new System.EventHandler(this.toolStripStatusLabel1_Click);
            // 
            // Chars
            // 
            this.Chars.AutoSize = false;
            this.Chars.AutoToolTip = true;
            this.Chars.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.Chars.Name = "Chars";
            this.Chars.Size = new System.Drawing.Size(50, 17);
            this.Chars.Text = "Chars";
            this.Chars.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Chars.ToolTipText = "Numer of chars in text";
            // 
            // Size
            // 
            this.Size.AutoSize = false;
            this.Size.AutoToolTip = true;
            this.Size.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.Size.Name = "Size";
            this.Size.Size = new System.Drawing.Size(60, 17);
            this.Size.Text = "Size";
            this.Size.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Size.ToolTipText = "Size in bytes";
            // 
            // Type
            // 
            this.Type.AutoSize = false;
            this.Type.AutoToolTip = true;
            this.Type.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.Type.Name = "Type";
            this.Type.Size = new System.Drawing.Size(30, 17);
            this.Type.Text = "Type";
            this.Type.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Type.ToolTipText = "Type";
            // 
            // Created
            // 
            this.Created.AutoSize = false;
            this.Created.AutoToolTip = true;
            this.Created.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.Created.Name = "Created";
            this.Created.Size = new System.Drawing.Size(110, 17);
            this.Created.Text = "Created";
            this.Created.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Created.ToolTipText = "Created";
            this.Created.Click += new System.EventHandler(this.toolStripStatusLabel1_Click_1);
            // 
            // Window
            // 
            this.Window.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Window.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.clipBindingSource, "Window", true));
            this.Window.Location = new System.Drawing.Point(3, 3);
            this.Window.Name = "Window";
            this.Window.ReadOnly = true;
            this.Window.Size = new System.Drawing.Size(231, 20);
            this.Window.TabIndex = 2;
            this.Window.Text = "Window";
            this.Window.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // Application
            // 
            this.Application.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Application.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.clipBindingSource, "Application", true));
            this.Application.Location = new System.Drawing.Point(240, 3);
            this.Application.Name = "Application";
            this.Application.ReadOnly = true;
            this.Application.Size = new System.Drawing.Size(123, 20);
            this.Application.TabIndex = 2;
            this.Application.Text = "Application";
            this.Application.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 380);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "Main";
            this.Load += new System.EventHandler(this.Main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.clipBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSet1)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton Delete;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox Application;
        private System.Windows.Forms.TextBox Window;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel Position;
        private System.Windows.Forms.ToolStripStatusLabel Chars;
        private System.Windows.Forms.ToolStripStatusLabel Created;
        private System.Windows.Forms.ToolStripStatusLabel Size;
        private System.Windows.Forms.ToolStripStatusLabel Type;
        private System.Windows.Forms.TextBox Text;
        private System.Windows.Forms.BindingSource clipBindingSource;
        private DataSet1 dataSet1;
        private System.Windows.Forms.ComboBox Filter;
        private System.Windows.Forms.Button ClearFilter;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.DataGridViewTextBoxColumn titleDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn;
    }
}

