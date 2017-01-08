using System;
using System.Runtime.InteropServices;

namespace ClipAngel
{
    public delegate int WindowProcDelegate(IntPtr hw, IntPtr uMsg, IntPtr wParam, IntPtr lParam);


    /// <summary>
    /// Windows User32 DLL declarations
    /// </summary>
    public class User32
    {
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(
            IntPtr hWndRemove,  // handle to window to remove
            IntPtr hWndNewNext  // handle to next window
            );

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

    }

    /// <summary>
    /// Windows Event Messages sent to the WindowProc
    /// </summary>
    public enum Msgs
    {
        WM_NULL = 0x0000,
        WM_CREATE = 0x0001,
        WM_DESTROY = 0x0002,
        WM_MOVE = 0x0003,
        WM_SIZE = 0x0005,
        WM_ACTIVATE = 0x0006,
        WM_SETFOCUS = 0x0007,
        WM_KILLFOCUS = 0x0008,
        WM_ENABLE = 0x000A,
        WM_SETREDRAW = 0x000B,
        WM_SETTEXT = 0x000C,
        WM_GETTEXT = 0x000D,
        WM_GETTEXTLENGTH = 0x000E,
        WM_PAINT = 0x000F,
        WM_CLOSE = 0x0010,
        WM_QUERYENDSESSION = 0x0011,
        WM_QUIT = 0x0012,
        WM_QUERYOPEN = 0x0013,
        WM_ERASEBKGND = 0x0014,
        WM_SYSCOLORCHANGE = 0x0015,
        WM_ENDSESSION = 0x0016,
        WM_SHOWWINDOW = 0x0018,
        WM_WININICHANGE = 0x001A,
        WM_SETTINGCHANGE = 0x001A,
        WM_DEVMODECHANGE = 0x001B,
        WM_ACTIVATEAPP = 0x001C,
        WM_FONTCHANGE = 0x001D,
        WM_TIMECHANGE = 0x001E,
        WM_CANCELMODE = 0x001F,
        WM_SETCURSOR = 0x0020,
        WM_MOUSEACTIVATE = 0x0021,
        WM_CHILDACTIVATE = 0x0022,
        WM_QUEUESYNC = 0x0023,
        WM_GETMINMAXINFO = 0x0024,
        WM_PAINTICON = 0x0026,
        WM_ICONERASEBKGND = 0x0027,
        WM_NEXTDLGCTL = 0x0028,
        WM_SPOOLERSTATUS = 0x002A,
        WM_DRAWITEM = 0x002B,
        WM_MEASUREITEM = 0x002C,
        WM_DELETEITEM = 0x002D,
        WM_VKEYTOITEM = 0x002E,
        WM_CHARTOITEM = 0x002F,
        WM_SETFONT = 0x0030,
        WM_GETFONT = 0x0031,
        WM_SETHOTKEY = 0x0032,
        WM_GETHOTKEY = 0x0033,
        WM_QUERYDRAGICON = 0x0037,
        WM_COMPAREITEM = 0x0039,
        WM_GETOBJECT = 0x003D,
        WM_COMPACTING = 0x0041,
        WM_COMMNOTIFY = 0x0044,
        WM_WINDOWPOSCHANGING = 0x0046,
        WM_WINDOWPOSCHANGED = 0x0047,
        WM_POWER = 0x0048,
        WM_COPYDATA = 0x004A,
        WM_CANCELJOURNAL = 0x004B,
        WM_NOTIFY = 0x004E,
        WM_INPUTLANGCHANGEREQUEST = 0x0050,
        WM_INPUTLANGCHANGE = 0x0051,
        WM_TCARD = 0x0052,
        WM_HELP = 0x0053,
        WM_USERCHANGED = 0x0054,
        WM_NOTIFYFORMAT = 0x0055,
        WM_CONTEXTMENU = 0x007B,
        WM_STYLECHANGING = 0x007C,
        WM_STYLECHANGED = 0x007D,
        WM_DISPLAYCHANGE = 0x007E,
        WM_GETICON = 0x007F,
        WM_SETICON = 0x0080,
        WM_NCCREATE = 0x0081,
        WM_NCDESTROY = 0x0082,
        WM_NCCALCSIZE = 0x0083,
        WM_NCHITTEST = 0x0084,
        WM_NCPAINT = 0x0085,
        WM_NCACTIVATE = 0x0086,
        WM_GETDLGCODE = 0x0087,
        WM_SYNCPAINT = 0x0088,
        WM_NCMOUSEMOVE = 0x00A0,
        WM_NCLBUTTONDOWN = 0x00A1,
        WM_NCLBUTTONUP = 0x00A2,
        WM_NCLBUTTONDBLCLK = 0x00A3,
        WM_NCRBUTTONDOWN = 0x00A4,
        WM_NCRBUTTONUP = 0x00A5,
        WM_NCRBUTTONDBLCLK = 0x00A6,
        WM_NCMBUTTONDOWN = 0x00A7,
        WM_NCMBUTTONUP = 0x00A8,
        WM_NCMBUTTONDBLCLK = 0x00A9,
        WM_NCXBUTTONDOWN = 0x00AB,
        WM_NCXBUTTONUP = 0x00AC,
        WM_KEYDOWN = 0x0100,
        WM_KEYUP = 0x0101,
        WM_CHAR = 0x0102,
        WM_DEADCHAR = 0x0103,
        WM_SYSKEYDOWN = 0x0104,
        WM_SYSKEYUP = 0x0105,
        WM_SYSCHAR = 0x0106,
        WM_SYSDEADCHAR = 0x0107,
        WM_KEYLAST = 0x0108,
        WM_IME_STARTCOMPOSITION = 0x010D,
        WM_IME_ENDCOMPOSITION = 0x010E,
        WM_IME_COMPOSITION = 0x010F,
        WM_IME_KEYLAST = 0x010F,
        WM_INITDIALOG = 0x0110,
        WM_COMMAND = 0x0111,
        WM_SYSCOMMAND = 0x0112,
        WM_TIMER = 0x0113,
        WM_HSCROLL = 0x0114,
        WM_VSCROLL = 0x0115,
        WM_INITMENU = 0x0116,
        WM_INITMENUPOPUP = 0x0117,
        WM_MENUSELECT = 0x011F,
        WM_MENUCHAR = 0x0120,
        WM_ENTERIDLE = 0x0121,
        WM_MENURBUTTONUP = 0x0122,
        WM_MENUDRAG = 0x0123,
        WM_MENUGETOBJECT = 0x0124,
        WM_UNINITMENUPOPUP = 0x0125,
        WM_MENUCOMMAND = 0x0126,
        WM_CTLCOLORMSGBOX = 0x0132,
        WM_CTLCOLOREDIT = 0x0133,
        WM_CTLCOLORLISTBOX = 0x0134,
        WM_CTLCOLORBTN = 0x0135,
        WM_CTLCOLORDLG = 0x0136,
        WM_CTLCOLORSCROLLBAR = 0x0137,
        WM_CTLCOLORSTATIC = 0x0138,
        WM_MOUSEMOVE = 0x0200,
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_LBUTTONDBLCLK = 0x0203,
        WM_RBUTTONDOWN = 0x0204,
        WM_RBUTTONUP = 0x0205,
        WM_RBUTTONDBLCLK = 0x0206,
        WM_MBUTTONDOWN = 0x0207,
        WM_MBUTTONUP = 0x0208,
        WM_MBUTTONDBLCLK = 0x0209,
        WM_MOUSEWHEEL = 0x020A,
        WM_XBUTTONDOWN = 0x020B,
        WM_XBUTTONUP = 0x020C,
        WM_XBUTTONDBLCLK = 0x020D,
        WM_PARENTNOTIFY = 0x0210,
        WM_ENTERMENULOOP = 0x0211,
        WM_EXITMENULOOP = 0x0212,
        WM_NEXTMENU = 0x0213,
        WM_SIZING = 0x0214,
        WM_CAPTURECHANGED = 0x0215,
        WM_MOVING = 0x0216,
        WM_DEVICECHANGE = 0x0219,
        WM_MDICREATE = 0x0220,
        WM_MDIDESTROY = 0x0221,
        WM_MDIACTIVATE = 0x0222,
        WM_MDIRESTORE = 0x0223,
        WM_MDINEXT = 0x0224,
        WM_MDIMAXIMIZE = 0x0225,
        WM_MDITILE = 0x0226,
        WM_MDICASCADE = 0x0227,
        WM_MDIICONARRANGE = 0x0228,
        WM_MDIGETACTIVE = 0x0229,
        WM_MDISETMENU = 0x0230,
        WM_ENTERSIZEMOVE = 0x0231,
        WM_EXITSIZEMOVE = 0x0232,
        WM_DROPFILES = 0x0233,
        WM_MDIREFRESHMENU = 0x0234,
        WM_IME_SETCONTEXT = 0x0281,
        WM_IME_NOTIFY = 0x0282,
        WM_IME_CONTROL = 0x0283,
        WM_IME_COMPOSITIONFULL = 0x0284,
        WM_IME_SELECT = 0x0285,
        WM_IME_CHAR = 0x0286,
        WM_IME_REQUEST = 0x0288,
        WM_IME_KEYDOWN = 0x0290,
        WM_IME_KEYUP = 0x0291,
        WM_MOUSEHOVER = 0x02A1,
        WM_MOUSELEAVE = 0x02A3,
        WM_CUT = 0x0300,
        WM_COPY = 0x0301,
        WM_PASTE = 0x0302,
        WM_CLEAR = 0x0303,
        WM_UNDO = 0x0304,
        WM_RENDERFORMAT = 0x0305,
        WM_RENDERALLFORMATS = 0x0306,
        WM_DESTROYCLIPBOARD = 0x0307,
        WM_DRAWCLIPBOARD = 0x0308,
        WM_PAINTCLIPBOARD = 0x0309,
        WM_VSCROLLCLIPBOARD = 0x030A,
        WM_SIZECLIPBOARD = 0x030B,
        WM_ASKCBFORMATNAME = 0x030C,
        WM_CHANGECBCHAIN = 0x030D,
        WM_HSCROLLCLIPBOARD = 0x030E,
        WM_QUERYNEWPALETTE = 0x030F,
        WM_PALETTEISCHANGING = 0x0310,
        WM_PALETTECHANGED = 0x0311,
        WM_HOTKEY = 0x0312,
        WM_PRINT = 0x0317,
        WM_PRINTCLIENT = 0x0318,
        WM_HANDHELDFIRST = 0x0358,
        WM_HANDHELDLAST = 0x035F,
        WM_AFXFIRST = 0x0360,
        WM_AFXLAST = 0x037F,
        WM_PENWINFIRST = 0x0380,
        WM_PENWINLAST = 0x038F,
        WM_APP = 0x8000,
        WM_USER = 0x0400
    }

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
            this.TypeImg = new System.Windows.Forms.DataGridViewImageColumn();
            this.titleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStripDataGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.clipBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dbDataSet = new ClipAngel.dbDataSet();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ClearFilter = new System.Windows.Forms.Button();
            this.Filter = new System.Windows.Forms.ComboBox();
            this.contextMenuStripFilter = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.activateListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanelData = new System.Windows.Forms.TableLayoutPanel();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.ImageControl = new System.Windows.Forms.PictureBox();
            this.textBoxUrl = new System.Windows.Forms.RichTextBox();
            this.labelClipSource = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.Position = new System.Windows.Forms.ToolStripStatusLabel();
            this.Chars = new System.Windows.Forms.ToolStripStatusLabel();
            this.Size = new System.Windows.Forms.ToolStripStatusLabel();
            this.Type = new System.Windows.Forms.ToolStripStatusLabel();
            this.Created = new System.Windows.Forms.ToolStripStatusLabel();
            this.Application = new System.Windows.Forms.TextBox();
            this.Window = new System.Windows.Forms.TextBox();
            this.clipsTableAdapter = new ClipAngel.dbDataSetTableAdapters.ClipsTableAdapter();
            this.tableAdapterManager = new ClipAngel.dbDataSetTableAdapters.TableAdapterManager();
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteENTERToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteTextCTRLENTERToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendPasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteAsTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStripNotifyIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.contextMenuStripDataGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.clipBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStripFilter.SuspendLayout();
            this.tableLayoutPanelData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImageControl)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.MainMenu.SuspendLayout();
            this.contextMenuStripNotifyIcon.SuspendLayout();
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
            this.dataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dataGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.ColumnHeadersVisible = false;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TypeImg,
            this.titleDataGridViewTextBoxColumn});
            this.dataGridView.ContextMenuStrip = this.contextMenuStripDataGrid;
            this.dataGridView.DataSource = this.clipBindingSource;
            this.dataGridView.Location = new System.Drawing.Point(1, 0);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.ReadOnly = true;
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.RowTemplate.Height = 19;
            this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView.Size = new System.Drawing.Size(299, 394);
            this.dataGridView.TabIndex = 0;
            this.dataGridView.RowEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_RowEnter);
            this.dataGridView.SelectionChanged += new System.EventHandler(this.dataGridView_SelectionChanged);
            this.dataGridView.DoubleClick += new System.EventHandler(this.dataGridView_DoubleClick);
            this.dataGridView.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.dataGridView_PreviewKeyDown);
            // 
            // TypeImg
            // 
            this.TypeImg.Frozen = true;
            this.TypeImg.HeaderText = "TypeImg";
            this.TypeImg.Name = "TypeImg";
            this.TypeImg.ReadOnly = true;
            this.TypeImg.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.TypeImg.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.TypeImg.Width = 16;
            // 
            // titleDataGridViewTextBoxColumn
            // 
            this.titleDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.titleDataGridViewTextBoxColumn.DataPropertyName = "Title";
            this.titleDataGridViewTextBoxColumn.HeaderText = "Title";
            this.titleDataGridViewTextBoxColumn.Name = "titleDataGridViewTextBoxColumn";
            this.titleDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // contextMenuStripDataGrid
            // 
            this.contextMenuStripDataGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.deleteToolStripMenuItem1});
            this.contextMenuStripDataGrid.Name = "contextMenuStripDataGrid";
            this.contextMenuStripDataGrid.Size = new System.Drawing.Size(210, 70);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(209, 22);
            this.toolStripMenuItem2.Text = "Paste (ENTER)";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.pasteOriginalToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(209, 22);
            this.toolStripMenuItem3.Text = "Paste  text (CTRL+ENTER)";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.pasteAsTextToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem1
            // 
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            this.deleteToolStripMenuItem1.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem1.Size = new System.Drawing.Size(209, 22);
            this.deleteToolStripMenuItem1.Text = "Delete";
            this.deleteToolStripMenuItem1.Click += new System.EventHandler(this.Delete_Click);
            // 
            // clipBindingSource
            // 
            this.clipBindingSource.DataMember = "Clips";
            this.clipBindingSource.DataSource = this.dbDataSet;
            // 
            // dbDataSet
            // 
            this.dbDataSet.DataSetName = "dbDataSet";
            this.dbDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn2.DataPropertyName = "Title";
            this.dataGridViewTextBoxColumn2.HeaderText = "Title";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
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
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanelData);
            this.splitContainer1.Panel2.Controls.Add(this.labelClipSource);
            this.splitContainer1.Panel2.Controls.Add(this.statusStrip);
            this.splitContainer1.Panel2.Controls.Add(this.Application);
            this.splitContainer1.Panel2.Controls.Add(this.Window);
            this.splitContainer1.Size = new System.Drawing.Size(699, 420);
            this.splitContainer1.SplitterDistance = 300;
            this.splitContainer1.TabIndex = 3;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pictureBox1.Image = global::ClipAngel.Properties.Resources.Filter;
            this.pictureBox1.Location = new System.Drawing.Point(3, 397);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(11, 20);
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // ClearFilter
            // 
            this.ClearFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ClearFilter.Location = new System.Drawing.Point(283, 396);
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
            this.Filter.ContextMenuStrip = this.contextMenuStripFilter;
            this.Filter.FormattingEnabled = true;
            this.Filter.Location = new System.Drawing.Point(17, 397);
            this.Filter.Name = "Filter";
            this.Filter.Size = new System.Drawing.Size(266, 21);
            this.Filter.TabIndex = 6;
            this.Filter.TextChanged += new System.EventHandler(this.Filter_TextChanged);
            this.Filter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Filter_KeyDown);
            this.Filter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Filter_KeyPress);
            this.Filter.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Filter_KeyUp);
            // 
            // contextMenuStripFilter
            // 
            this.contextMenuStripFilter.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem,
            this.activateListToolStripMenuItem});
            this.contextMenuStripFilter.Name = "contextMenuStripFilter";
            this.contextMenuStripFilter.Size = new System.Drawing.Size(182, 48);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // activateListToolStripMenuItem
            // 
            this.activateListToolStripMenuItem.Name = "activateListToolStripMenuItem";
            this.activateListToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F9)));
            this.activateListToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.activateListToolStripMenuItem.Text = "Activate list";
            this.activateListToolStripMenuItem.Click += new System.EventHandler(this.activateListToolStripMenuItem_Click);
            // 
            // tableLayoutPanelData
            // 
            this.tableLayoutPanelData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelData.ColumnCount = 1;
            this.tableLayoutPanelData.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelData.Controls.Add(this.richTextBox, 0, 0);
            this.tableLayoutPanelData.Controls.Add(this.ImageControl, 0, 1);
            this.tableLayoutPanelData.Controls.Add(this.textBoxUrl, 0, 2);
            this.tableLayoutPanelData.Location = new System.Drawing.Point(0, 26);
            this.tableLayoutPanelData.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanelData.Name = "tableLayoutPanelData";
            this.tableLayoutPanelData.RowCount = 3;
            this.tableLayoutPanelData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanelData.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelData.Size = new System.Drawing.Size(391, 369);
            this.tableLayoutPanelData.TabIndex = 8;
            // 
            // richTextBox
            // 
            this.richTextBox.DetectUrls = false;
            this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.richTextBox.HideSelection = false;
            this.richTextBox.Location = new System.Drawing.Point(2, 2);
            this.richTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.ReadOnly = true;
            this.richTextBox.Size = new System.Drawing.Size(387, 168);
            this.richTextBox.TabIndex = 7;
            this.richTextBox.Text = "";
            this.richTextBox.Click += new System.EventHandler(this.RichText_Click);
            // 
            // ImageControl
            // 
            this.ImageControl.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ImageControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImageControl.Location = new System.Drawing.Point(2, 174);
            this.ImageControl.Margin = new System.Windows.Forms.Padding(2);
            this.ImageControl.Name = "ImageControl";
            this.ImageControl.Size = new System.Drawing.Size(387, 168);
            this.ImageControl.TabIndex = 8;
            this.ImageControl.TabStop = false;
            // 
            // textBoxUrl
            // 
            this.textBoxUrl.DetectUrls = false;
            this.textBoxUrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxUrl.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxUrl.HideSelection = false;
            this.textBoxUrl.Location = new System.Drawing.Point(2, 346);
            this.textBoxUrl.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxUrl.Multiline = false;
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.ReadOnly = true;
            this.textBoxUrl.Size = new System.Drawing.Size(387, 21);
            this.textBoxUrl.TabIndex = 9;
            this.textBoxUrl.Text = "";
            this.textBoxUrl.Click += new System.EventHandler(this.textBoxUrl_Click);
            // 
            // labelClipSource
            // 
            this.labelClipSource.AutoSize = true;
            this.labelClipSource.Location = new System.Drawing.Point(0, 7);
            this.labelClipSource.Margin = new System.Windows.Forms.Padding(0);
            this.labelClipSource.Name = "labelClipSource";
            this.labelClipSource.Size = new System.Drawing.Size(44, 13);
            this.labelClipSource.TabIndex = 6;
            this.labelClipSource.Text = "Source:";
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
            this.statusStrip.Location = new System.Drawing.Point(0, 398);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.ShowItemToolTips = true;
            this.statusStrip.Size = new System.Drawing.Size(395, 22);
            this.statusStrip.TabIndex = 4;
            this.statusStrip.Text = "statusStrip";
            // 
            // Position
            // 
            this.Position.AutoSize = false;
            this.Position.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.Position.Name = "Position";
            this.Position.Size = new System.Drawing.Size(90, 17);
            this.Position.Text = "Position";
            this.Position.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Position.ToolTipText = "Text selection offset and length";
            // 
            // Chars
            // 
            this.Chars.AutoSize = false;
            this.Chars.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.Chars.Name = "Chars";
            this.Chars.Size = new System.Drawing.Size(70, 17);
            this.Chars.Text = "Chars";
            this.Chars.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Chars.ToolTipText = "Chars in text";
            // 
            // Size
            // 
            this.Size.AutoSize = false;
            this.Size.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.Size.Name = "Size";
            this.Size.Size = new System.Drawing.Size(70, 17);
            this.Size.Text = "Size";
            this.Size.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Size.ToolTipText = "Clip size in bytes";
            // 
            // Type
            // 
            this.Type.AutoSize = false;
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
            this.Created.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.Created.Name = "Created";
            this.Created.Size = new System.Drawing.Size(110, 17);
            this.Created.Text = "Created";
            this.Created.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.Created.ToolTipText = "Created time";
            // 
            // Application
            // 
            this.Application.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Application.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.clipBindingSource, "Application", true));
            this.Application.Location = new System.Drawing.Point(308, 3);
            this.Application.Name = "Application";
            this.Application.ReadOnly = true;
            this.Application.Size = new System.Drawing.Size(83, 20);
            this.Application.TabIndex = 2;
            this.Application.Text = "Application";
            // 
            // Window
            // 
            this.Window.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Window.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.clipBindingSource, "Window", true));
            this.Window.Location = new System.Drawing.Point(47, 3);
            this.Window.Name = "Window";
            this.Window.ReadOnly = true;
            this.Window.Size = new System.Drawing.Size(255, 20);
            this.Window.TabIndex = 2;
            this.Window.Text = "Window";
            // 
            // clipsTableAdapter
            // 
            this.clipsTableAdapter.ClearBeforeFill = true;
            // 
            // tableAdapterManager
            // 
            this.tableAdapterManager.BackupDataSetBeforeUpdate = false;
            this.tableAdapterManager.ClipsTableAdapter = this.clipsTableAdapter;
            this.tableAdapterManager.UpdateOrder = ClipAngel.dbDataSetTableAdapters.TableAdapterManager.UpdateOrderOption.InsertUpdateDelete;
            // 
            // MainMenu
            // 
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem1,
            this.clipToolStripMenuItem,
            this.settingsToolStripMenuItem1,
            this.aboutToolStripMenuItem1});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Size = new System.Drawing.Size(699, 24);
            this.MainMenu.TabIndex = 4;
            this.MainMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem1
            // 
            this.fileToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem1.Name = "fileToolStripMenuItem1";
            this.fileToolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem1.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
            // 
            // clipToolStripMenuItem
            // 
            this.clipToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pasteENTERToolStripMenuItem,
            this.pasteTextCTRLENTERToolStripMenuItem,
            this.deleteToolStripMenuItem2});
            this.clipToolStripMenuItem.Name = "clipToolStripMenuItem";
            this.clipToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.clipToolStripMenuItem.Text = "Clip";
            // 
            // pasteENTERToolStripMenuItem
            // 
            this.pasteENTERToolStripMenuItem.Name = "pasteENTERToolStripMenuItem";
            this.pasteENTERToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.pasteENTERToolStripMenuItem.Text = "Paste (ENTER)";
            this.pasteENTERToolStripMenuItem.Click += new System.EventHandler(this.pasteOriginalToolStripMenuItem_Click);
            // 
            // pasteTextCTRLENTERToolStripMenuItem
            // 
            this.pasteTextCTRLENTERToolStripMenuItem.Name = "pasteTextCTRLENTERToolStripMenuItem";
            this.pasteTextCTRLENTERToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.pasteTextCTRLENTERToolStripMenuItem.Text = "Paste text (CTRL+ ENTER)";
            this.pasteTextCTRLENTERToolStripMenuItem.Click += new System.EventHandler(this.pasteAsTextToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem2
            // 
            this.deleteToolStripMenuItem2.Name = "deleteToolStripMenuItem2";
            this.deleteToolStripMenuItem2.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.deleteToolStripMenuItem2.Size = new System.Drawing.Size(209, 22);
            this.deleteToolStripMenuItem2.Text = "Delete";
            this.deleteToolStripMenuItem2.Click += new System.EventHandler(this.Delete_Click);
            // 
            // settingsToolStripMenuItem1
            // 
            this.settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
            this.settingsToolStripMenuItem1.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem1.Text = "Settings";
            this.settingsToolStripMenuItem1.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem1.Text = "About";
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(92, 22);
            this.toolStripMenuItem1.Text = "Exit";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sendPasteToolStripMenuItem,
            this.pasteAsTextToolStripMenuItem,
            this.toolStripMenuItem4});
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.deleteToolStripMenuItem.Text = "Clip";
            // 
            // sendPasteToolStripMenuItem
            // 
            this.sendPasteToolStripMenuItem.Name = "sendPasteToolStripMenuItem";
            this.sendPasteToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.sendPasteToolStripMenuItem.Text = "Paste original (ENTER)";
            this.sendPasteToolStripMenuItem.Click += new System.EventHandler(this.pasteOriginalToolStripMenuItem_Click);
            // 
            // pasteAsTextToolStripMenuItem
            // 
            this.pasteAsTextToolStripMenuItem.Name = "pasteAsTextToolStripMenuItem";
            this.pasteAsTextToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.pasteAsTextToolStripMenuItem.Text = "Paste  text (CTRL+ENTER)";
            this.pasteAsTextToolStripMenuItem.Click += new System.EventHandler(this.pasteAsTextToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.toolStripMenuItem4.Size = new System.Drawing.Size(209, 22);
            this.toolStripMenuItem4.Text = "Delete";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.Delete_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStripNotifyIcon;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Clip Angel";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);
            // 
            // contextMenuStripNotifyIcon
            // 
            this.contextMenuStripNotifyIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem1});
            this.contextMenuStripNotifyIcon.Name = "contextMenuStripNotifyIcon";
            this.contextMenuStripNotifyIcon.Size = new System.Drawing.Size(93, 26);
            // 
            // exitToolStripMenuItem1
            // 
            this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            this.exitToolStripMenuItem1.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem1.Text = "Exit";
            this.exitToolStripMenuItem1.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(699, 444);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.MainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.MainMenu;
            this.Name = "Main";
            this.Text = "Clip Angel";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Activated += new System.EventHandler(this.Main_Activated);
            this.Deactivate += new System.EventHandler(this.Main_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Main_FormClosed);
            this.Load += new System.EventHandler(this.Main_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Main_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.contextMenuStripDataGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.clipBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbDataSet)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.contextMenuStripFilter.ResumeLayout(false);
            this.tableLayoutPanelData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ImageControl)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.contextMenuStripNotifyIcon.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox Application;
        private System.Windows.Forms.TextBox Window;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.BindingSource clipBindingSource;
        private System.Windows.Forms.ComboBox Filter;
        private System.Windows.Forms.Button ClearFilter;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label labelClipSource;
        private dbDataSet dbDataSet;
        private dbDataSetTableAdapters.ClipsTableAdapter clipsTableAdapter;
        private dbDataSetTableAdapters.TableAdapterManager tableAdapterManager;
        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sendPasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteAsTextToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripDataGrid;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFilter;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem activateListToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripNotifyIcon;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem1;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelData;
        private System.Windows.Forms.PictureBox ImageControl;
        private System.Windows.Forms.DataGridViewImageColumn TypeImg;
        private System.Windows.Forms.DataGridViewTextBoxColumn titleDataGridViewTextBoxColumn;
        private System.Windows.Forms.ToolStripStatusLabel Position;
        private System.Windows.Forms.ToolStripStatusLabel Chars;
        private System.Windows.Forms.ToolStripStatusLabel Size;
        private System.Windows.Forms.ToolStripStatusLabel Type;
        private System.Windows.Forms.ToolStripStatusLabel Created;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clipToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem pasteENTERToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteTextCTRLENTERToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.RichTextBox textBoxUrl;
    }
}

