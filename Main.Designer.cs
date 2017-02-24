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

        [DllImport("user32.dll")]
        public static extern IntPtr GetClipboardViewer();

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
        WM_CLIPBOARDUPDATE = 0x031D,
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.MarkFilter = new System.Windows.Forms.ComboBox();
            this.buttonFindNext = new System.Windows.Forms.Button();
            this.buttonFindPrevious = new System.Windows.Forms.Button();
            this.TypeFilter = new System.Windows.Forms.ComboBox();
            this.buttonClearFilter = new System.Windows.Forms.Button();
            this.comboBoxFilter = new System.Windows.Forms.ComboBox();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.TypeImg = new System.Windows.Forms.DataGridViewImageColumn();
            this.TitleSimple = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Title = new ClipAngel.DataGridViewRichTextBoxColumn();
            this.ImageSample = new System.Windows.Forms.DataGridViewImageColumn();
            this.VisualWeight = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStripDataGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.copyClipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clipBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dbDataSet = new ClipAngel.dbDataSet();
            this.tableLayoutPanelData = new System.Windows.Forms.TableLayoutPanel();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.ImageControl = new System.Windows.Forms.PictureBox();
            this.textBoxUrl = new System.Windows.Forms.RichTextBox();
            this.labelClipSource = new System.Windows.Forms.Label();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.stripLabelPosition = new System.Windows.Forms.ToolStripStatusLabel();
            this.StripLabelVisualSize = new System.Windows.Forms.ToolStripStatusLabel();
            this.StripLabelSize = new System.Windows.Forms.ToolStripStatusLabel();
            this.StripLabelType = new System.Windows.Forms.ToolStripStatusLabel();
            this.StripLabelCreated = new System.Windows.Forms.ToolStripStatusLabel();
            this.textBoxApplication = new System.Windows.Forms.TextBox();
            this.textBoxWindow = new System.Windows.Forms.TextBox();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.windowAlwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemMonitoringClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemClearFilterAndSelectTop = new System.Windows.Forms.ToolStripMenuItem();
            this.selectTopClipOnShowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.showAllMarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOnlyUsedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOnlyFavoriteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteENTERToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteTextCTRLENTERToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemPasteChars = new System.Windows.Forms.ToolStripMenuItem();
            this.changeClipTitleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editClipTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openInDefaultApplicationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setFavouriteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFavouriteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nextMatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.previousMatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wordWrapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.monospacedFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.fToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.historyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.trayMenuItemMonitoringClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTipDynamic = new System.Windows.Forms.ToolTip(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonWordWrap = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonFixedWidthFont = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSelectTopClipOnShow = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonClearFilterAndSelectTop = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonDelete = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonMarkFavourite = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonUnmarkFavourite = new System.Windows.Forms.ToolStripButton();
            this.toolStripMenuItemEditClipText = new System.Windows.Forms.ToolStripButton();
            this.toolStripMenuItemOpenFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButtonTopMostWindow = new System.Windows.Forms.ToolStripButton();
            this.toolStripUpdateToSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.buttonUpdate = new System.Windows.Forms.ToolStripButton();
            this.clipsTableAdapter = new ClipAngel.dbDataSetTableAdapters.ClipsTableAdapter();
            this.tableAdapterManager = new ClipAngel.dbDataSetTableAdapters.TableAdapterManager();
            this.cultureManager1 = new Infralution.Localization.CultureManager(this.components);
            this.timerCheckUpdate = new System.Windows.Forms.Timer(this.components);
            this.timerReconnect = new System.Windows.Forms.Timer(this.components);
            this.timerApplyTextFiler = new System.Windows.Forms.Timer(this.components);
            this.dataGridViewImageColumn1 = new System.Windows.Forms.DataGridViewImageColumn();
            this.dataGridViewRichTextBoxColumn1 = new ClipAngel.DataGridViewRichTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.contextMenuStripDataGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.clipBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbDataSet)).BeginInit();
            this.tableLayoutPanelData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImageControl)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.MainMenu.SuspendLayout();
            this.contextMenuStripNotifyIcon.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.MarkFilter);
            this.splitContainer1.Panel1.Controls.Add(this.buttonFindNext);
            this.splitContainer1.Panel1.Controls.Add(this.buttonFindPrevious);
            this.splitContainer1.Panel1.Controls.Add(this.TypeFilter);
            this.splitContainer1.Panel1.Controls.Add(this.buttonClearFilter);
            this.splitContainer1.Panel1.Controls.Add(this.comboBoxFilter);
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView);
            // 
            // splitContainer1.Panel2
            // 
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanelData);
            this.splitContainer1.Panel2.Controls.Add(this.labelClipSource);
            this.splitContainer1.Panel2.Controls.Add(this.statusStrip);
            this.splitContainer1.Panel2.Controls.Add(this.textBoxApplication);
            this.splitContainer1.Panel2.Controls.Add(this.textBoxWindow);
            this.splitContainer1.TabStop = false;
            // 
            // MarkFilter
            // 
            resources.ApplyResources(this.MarkFilter, "MarkFilter");
            this.MarkFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.MarkFilter.FormattingEnabled = true;
            this.MarkFilter.Items.AddRange(new object[] {
            resources.GetString("MarkFilter.Items"),
            resources.GetString("MarkFilter.Items1"),
            resources.GetString("MarkFilter.Items2"),
            resources.GetString("MarkFilter.Items3")});
            this.MarkFilter.Name = "MarkFilter";
            this.MarkFilter.TabStop = false;
            this.toolTipDynamic.SetToolTip(this.MarkFilter, resources.GetString("MarkFilter.ToolTip"));
            this.MarkFilter.SelectedValueChanged += new System.EventHandler(this.MarkFilter_SelectedValueChanged);
            // 
            // buttonFindNext
            // 
            resources.ApplyResources(this.buttonFindNext, "buttonFindNext");
            this.buttonFindNext.Image = global::ClipAngel.Properties.Resources.FindNext;
            this.buttonFindNext.Name = "buttonFindNext";
            this.buttonFindNext.TabStop = false;
            this.toolTipDynamic.SetToolTip(this.buttonFindNext, resources.GetString("buttonFindNext.ToolTip"));
            this.buttonFindNext.UseVisualStyleBackColor = true;
            this.buttonFindNext.Click += new System.EventHandler(this.buttonFindNext_Click);
            // 
            // buttonFindPrevious
            // 
            resources.ApplyResources(this.buttonFindPrevious, "buttonFindPrevious");
            this.buttonFindPrevious.Image = global::ClipAngel.Properties.Resources.FindPrevious;
            this.buttonFindPrevious.Name = "buttonFindPrevious";
            this.buttonFindPrevious.TabStop = false;
            this.toolTipDynamic.SetToolTip(this.buttonFindPrevious, resources.GetString("buttonFindPrevious.ToolTip"));
            this.buttonFindPrevious.UseVisualStyleBackColor = true;
            this.buttonFindPrevious.Click += new System.EventHandler(this.buttonFindPrevious_Click);
            // 
            // TypeFilter
            // 
            resources.ApplyResources(this.TypeFilter, "TypeFilter");
            this.TypeFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TypeFilter.FormattingEnabled = true;
            this.TypeFilter.Items.AddRange(new object[] {
            resources.GetString("TypeFilter.Items"),
            resources.GetString("TypeFilter.Items1"),
            resources.GetString("TypeFilter.Items2"),
            resources.GetString("TypeFilter.Items3")});
            this.TypeFilter.Name = "TypeFilter";
            this.TypeFilter.TabStop = false;
            this.toolTipDynamic.SetToolTip(this.TypeFilter, resources.GetString("TypeFilter.ToolTip"));
            this.TypeFilter.SelectedValueChanged += new System.EventHandler(this.TypeFilter_SelectedValueChanged);
            // 
            // buttonClearFilter
            // 
            resources.ApplyResources(this.buttonClearFilter, "buttonClearFilter");
            this.buttonClearFilter.Name = "buttonClearFilter";
            this.buttonClearFilter.TabStop = false;
            this.toolTipDynamic.SetToolTip(this.buttonClearFilter, resources.GetString("buttonClearFilter.ToolTip"));
            this.buttonClearFilter.UseVisualStyleBackColor = true;
            this.buttonClearFilter.Click += new System.EventHandler(this.ClearFilter_Click);
            // 
            // comboBoxFilter
            // 
            resources.ApplyResources(this.comboBoxFilter, "comboBoxFilter");
            this.comboBoxFilter.FormattingEnabled = true;
            this.comboBoxFilter.Name = "comboBoxFilter";
            this.comboBoxFilter.TabStop = false;
            this.toolTipDynamic.SetToolTip(this.comboBoxFilter, resources.GetString("comboBoxFilter.ToolTip"));
            this.comboBoxFilter.TextChanged += new System.EventHandler(this.Filter_TextChanged);
            this.comboBoxFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Filter_KeyDown);
            this.comboBoxFilter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Filter_KeyPress);
            this.comboBoxFilter.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Filter_KeyUp);
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AllowUserToResizeColumns = false;
            this.dataGridView.AllowUserToResizeRows = false;
            resources.ApplyResources(this.dataGridView, "dataGridView");
            this.dataGridView.AutoGenerateColumns = false;
            this.dataGridView.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dataGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.ColumnHeadersVisible = false;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TypeImg,
            this.TitleSimple,
            this.Title,
            this.ImageSample,
            this.VisualWeight});
            this.dataGridView.ContextMenuStrip = this.contextMenuStripDataGrid;
            this.dataGridView.DataSource = this.clipBindingSource;
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.ReadOnly = true;
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.RowTemplate.Height = 19;
            this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView.StandardTab = true;
            this.dataGridView.TabStop = false;
            this.dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridView_CellFormatting);
            this.dataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView_CellMouseDown);
            this.dataGridView.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.dataGridView_RowPrePaint);
            this.dataGridView.SelectionChanged += new System.EventHandler(this.dataGridView_SelectionChanged);
            this.dataGridView.DoubleClick += new System.EventHandler(this.dataGridView_DoubleClick);
            this.dataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView_KeyDown);
            this.dataGridView.MouseHover += new System.EventHandler(this.dataGridView_MouseHover);
            this.dataGridView.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.dataGridView_PreviewKeyDown);
            // 
            // TypeImg
            // 
            this.TypeImg.Frozen = true;
            resources.ApplyResources(this.TypeImg, "TypeImg");
            this.TypeImg.Name = "TypeImg";
            this.TypeImg.ReadOnly = true;
            this.TypeImg.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.TypeImg.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // TitleSimple
            // 
            this.TitleSimple.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.TitleSimple.DataPropertyName = "Title";
            resources.ApplyResources(this.TitleSimple, "TitleSimple");
            this.TitleSimple.Name = "TitleSimple";
            this.TitleSimple.ReadOnly = true;
            // 
            // Title
            // 
            this.Title.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.BottomLeft;
            this.Title.DefaultCellStyle = dataGridViewCellStyle1;
            resources.ApplyResources(this.Title, "Title");
            this.Title.Name = "Title";
            this.Title.ReadOnly = true;
            this.Title.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Title.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // ImageSample
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.NullValue = ((object)(resources.GetObject("dataGridViewCellStyle2.NullValue")));
            dataGridViewCellStyle2.Padding = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.ImageSample.DefaultCellStyle = dataGridViewCellStyle2;
            this.ImageSample.Description = "LinkedToTitle";
            resources.ApplyResources(this.ImageSample, "ImageSample");
            this.ImageSample.Name = "ImageSample";
            this.ImageSample.ReadOnly = true;
            // 
            // VisualWeight
            // 
            dataGridViewCellStyle3.Format = "N0";
            dataGridViewCellStyle3.NullValue = null;
            this.VisualWeight.DefaultCellStyle = dataGridViewCellStyle3;
            this.VisualWeight.FillWeight = 1F;
            resources.ApplyResources(this.VisualWeight, "VisualWeight");
            this.VisualWeight.Name = "VisualWeight";
            this.VisualWeight.ReadOnly = true;
            this.VisualWeight.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // contextMenuStripDataGrid
            // 
            this.contextMenuStripDataGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.toolStripMenuItem7,
            this.deleteToolStripMenuItem1,
            this.copyClipToolStripMenuItem});
            this.contextMenuStripDataGrid.Name = "contextMenuStripDataGrid";
            resources.ApplyResources(this.contextMenuStripDataGrid, "contextMenuStripDataGrid");
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            this.toolStripMenuItem2.Click += new System.EventHandler(this.pasteOriginalToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
            this.toolStripMenuItem3.Click += new System.EventHandler(this.pasteAsTextToolStripMenuItem_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            resources.ApplyResources(this.toolStripMenuItem7, "toolStripMenuItem7");
            this.toolStripMenuItem7.Click += new System.EventHandler(this.toolStripMenuItemPasteChars_Click);
            // 
            // deleteToolStripMenuItem1
            // 
            this.deleteToolStripMenuItem1.Image = global::ClipAngel.Properties.Resources.delete;
            resources.ApplyResources(this.deleteToolStripMenuItem1, "deleteToolStripMenuItem1");
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            this.deleteToolStripMenuItem1.Click += new System.EventHandler(this.Delete_Click);
            // 
            // copyClipToolStripMenuItem
            // 
            this.copyClipToolStripMenuItem.Name = "copyClipToolStripMenuItem";
            resources.ApplyResources(this.copyClipToolStripMenuItem, "copyClipToolStripMenuItem");
            this.copyClipToolStripMenuItem.Click += new System.EventHandler(this.copyClipToolStripMenuItem_Click);
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
            // tableLayoutPanelData
            // 
            resources.ApplyResources(this.tableLayoutPanelData, "tableLayoutPanelData");
            this.tableLayoutPanelData.Controls.Add(this.richTextBox, 0, 0);
            this.tableLayoutPanelData.Controls.Add(this.ImageControl, 0, 1);
            this.tableLayoutPanelData.Controls.Add(this.textBoxUrl, 0, 2);
            this.tableLayoutPanelData.Name = "tableLayoutPanelData";
            // 
            // richTextBox
            // 
            this.richTextBox.AcceptsTab = true;
            this.richTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBox.DetectUrls = false;
            resources.ApplyResources(this.richTextBox, "richTextBox");
            this.richTextBox.HideSelection = false;
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.ReadOnly = true;
            this.richTextBox.SelectionChanged += new System.EventHandler(this.richTextBox_SelectionChanged);
            this.richTextBox.Click += new System.EventHandler(this.RichText_Click);
            // 
            // ImageControl
            // 
            this.ImageControl.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.ImageControl, "ImageControl");
            this.ImageControl.Name = "ImageControl";
            this.ImageControl.TabStop = false;
            this.ImageControl.DoubleClick += new System.EventHandler(this.ImageControl_DoubleClick);
            // 
            // textBoxUrl
            // 
            this.textBoxUrl.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxUrl.DetectUrls = false;
            resources.ApplyResources(this.textBoxUrl, "textBoxUrl");
            this.textBoxUrl.HideSelection = false;
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.ReadOnly = true;
            this.textBoxUrl.TabStop = false;
            this.toolTipDynamic.SetToolTip(this.textBoxUrl, resources.GetString("textBoxUrl.ToolTip"));
            this.textBoxUrl.Click += new System.EventHandler(this.textBoxUrl_Click);
            // 
            // labelClipSource
            // 
            resources.ApplyResources(this.labelClipSource, "labelClipSource");
            this.labelClipSource.Name = "labelClipSource";
            // 
            // statusStrip
            // 
            resources.ApplyResources(this.statusStrip, "statusStrip");
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stripLabelPosition,
            this.StripLabelVisualSize,
            this.StripLabelSize,
            this.StripLabelType,
            this.StripLabelCreated});
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.ShowItemToolTips = true;
            // 
            // stripLabelPosition
            // 
            resources.ApplyResources(this.stripLabelPosition, "stripLabelPosition");
            this.stripLabelPosition.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.stripLabelPosition.Name = "stripLabelPosition";
            this.stripLabelPosition.Spring = true;
            // 
            // StripLabelVisualSize
            // 
            resources.ApplyResources(this.StripLabelVisualSize, "StripLabelVisualSize");
            this.StripLabelVisualSize.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.StripLabelVisualSize.Name = "StripLabelVisualSize";
            // 
            // StripLabelSize
            // 
            resources.ApplyResources(this.StripLabelSize, "StripLabelSize");
            this.StripLabelSize.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.StripLabelSize.Name = "StripLabelSize";
            // 
            // StripLabelType
            // 
            resources.ApplyResources(this.StripLabelType, "StripLabelType");
            this.StripLabelType.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.StripLabelType.Name = "StripLabelType";
            // 
            // StripLabelCreated
            // 
            resources.ApplyResources(this.StripLabelCreated, "StripLabelCreated");
            this.StripLabelCreated.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.StripLabelCreated.Name = "StripLabelCreated";
            // 
            // textBoxApplication
            // 
            resources.ApplyResources(this.textBoxApplication, "textBoxApplication");
            this.textBoxApplication.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxApplication.Name = "textBoxApplication";
            this.textBoxApplication.ReadOnly = true;
            this.textBoxApplication.TabStop = false;
            this.toolTipDynamic.SetToolTip(this.textBoxApplication, resources.GetString("textBoxApplication.ToolTip"));
            // 
            // textBoxWindow
            // 
            resources.ApplyResources(this.textBoxWindow, "textBoxWindow");
            this.textBoxWindow.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxWindow.Name = "textBoxWindow";
            this.textBoxWindow.ReadOnly = true;
            this.textBoxWindow.TabStop = false;
            this.toolTipDynamic.SetToolTip(this.textBoxWindow, resources.GetString("textBoxWindow.ToolTip"));
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridViewTextBoxColumn2.DataPropertyName = "Title";
            resources.ApplyResources(this.dataGridViewTextBoxColumn2, "dataGridViewTextBoxColumn2");
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            // 
            // MainMenu
            // 
            this.MainMenu.BackColor = System.Drawing.SystemColors.Menu;
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem1,
            this.listToolStripMenuItem,
            this.clipToolStripMenuItem,
            this.settingsToolStripMenuItem1,
            this.fToolStripMenuItem});
            resources.ApplyResources(this.MainMenu, "MainMenu");
            this.MainMenu.Name = "MainMenu";
            // 
            // fileToolStripMenuItem1
            // 
            this.fileToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.windowAlwaysOnTopToolStripMenuItem,
            this.toolStripMenuItemMonitoringClipboard,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem1.Name = "fileToolStripMenuItem1";
            resources.ApplyResources(this.fileToolStripMenuItem1, "fileToolStripMenuItem1");
            // 
            // windowAlwaysOnTopToolStripMenuItem
            // 
            this.windowAlwaysOnTopToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.TopMostWindow;
            this.windowAlwaysOnTopToolStripMenuItem.Name = "windowAlwaysOnTopToolStripMenuItem";
            resources.ApplyResources(this.windowAlwaysOnTopToolStripMenuItem, "windowAlwaysOnTopToolStripMenuItem");
            this.windowAlwaysOnTopToolStripMenuItem.Click += new System.EventHandler(this.windowAlwaysOnTopToolStripMenuItem_Click);
            // 
            // toolStripMenuItemMonitoringClipboard
            // 
            this.toolStripMenuItemMonitoringClipboard.Name = "toolStripMenuItemMonitoringClipboard";
            resources.ApplyResources(this.toolStripMenuItemMonitoringClipboard, "toolStripMenuItemMonitoringClipboard");
            this.toolStripMenuItemMonitoringClipboard.Click += new System.EventHandler(this.MonitoringClipboardToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            resources.ApplyResources(this.exitToolStripMenuItem, "exitToolStripMenuItem");
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
            // 
            // listToolStripMenuItem
            // 
            this.listToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem6,
            this.toolStripMenuItemClearFilterAndSelectTop,
            this.selectTopClipOnShowToolStripMenuItem,
            this.moveUpToolStripMenuItem,
            this.moveDownToolStripMenuItem,
            this.toolStripMenuItem5,
            this.showAllMarksToolStripMenuItem,
            this.showOnlyUsedToolStripMenuItem,
            this.showOnlyFavoriteToolStripMenuItem});
            this.listToolStripMenuItem.Name = "listToolStripMenuItem";
            resources.ApplyResources(this.listToolStripMenuItem, "listToolStripMenuItem");
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            resources.ApplyResources(this.toolStripMenuItem6, "toolStripMenuItem6");
            this.toolStripMenuItem6.Click += new System.EventHandler(this.activateListToolStripMenuItem_Click);
            // 
            // toolStripMenuItemClearFilterAndSelectTop
            // 
            this.toolStripMenuItemClearFilterAndSelectTop.Image = global::ClipAngel.Properties.Resources.Top2;
            this.toolStripMenuItemClearFilterAndSelectTop.Name = "toolStripMenuItemClearFilterAndSelectTop";
            resources.ApplyResources(this.toolStripMenuItemClearFilterAndSelectTop, "toolStripMenuItemClearFilterAndSelectTop");
            this.toolStripMenuItemClearFilterAndSelectTop.Click += new System.EventHandler(this.toolStripMenuItemClearFilterAndSelectTop_Click);
            // 
            // selectTopClipOnShowToolStripMenuItem
            // 
            this.selectTopClipOnShowToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.Top1;
            this.selectTopClipOnShowToolStripMenuItem.Name = "selectTopClipOnShowToolStripMenuItem";
            resources.ApplyResources(this.selectTopClipOnShowToolStripMenuItem, "selectTopClipOnShowToolStripMenuItem");
            this.selectTopClipOnShowToolStripMenuItem.Click += new System.EventHandler(this.toolStripButtonSelectTopClipOnShow_Click);
            // 
            // moveUpToolStripMenuItem
            // 
            this.moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            resources.ApplyResources(this.moveUpToolStripMenuItem, "moveUpToolStripMenuItem");
            this.moveUpToolStripMenuItem.Click += new System.EventHandler(this.moveUpToolStripMenuItem_Click);
            // 
            // moveDownToolStripMenuItem
            // 
            this.moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            resources.ApplyResources(this.moveDownToolStripMenuItem, "moveDownToolStripMenuItem");
            this.moveDownToolStripMenuItem.Click += new System.EventHandler(this.moveDownToolStripMenuItem_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            resources.ApplyResources(this.toolStripMenuItem5, "toolStripMenuItem5");
            // 
            // showAllMarksToolStripMenuItem
            // 
            this.showAllMarksToolStripMenuItem.Name = "showAllMarksToolStripMenuItem";
            resources.ApplyResources(this.showAllMarksToolStripMenuItem, "showAllMarksToolStripMenuItem");
            this.showAllMarksToolStripMenuItem.Click += new System.EventHandler(this.showAllMarksToolStripMenuItem_Click);
            // 
            // showOnlyUsedToolStripMenuItem
            // 
            this.showOnlyUsedToolStripMenuItem.Name = "showOnlyUsedToolStripMenuItem";
            resources.ApplyResources(this.showOnlyUsedToolStripMenuItem, "showOnlyUsedToolStripMenuItem");
            this.showOnlyUsedToolStripMenuItem.Click += new System.EventHandler(this.showOnlyUsedToolStripMenuItem_Click);
            // 
            // showOnlyFavoriteToolStripMenuItem
            // 
            this.showOnlyFavoriteToolStripMenuItem.Name = "showOnlyFavoriteToolStripMenuItem";
            resources.ApplyResources(this.showOnlyFavoriteToolStripMenuItem, "showOnlyFavoriteToolStripMenuItem");
            this.showOnlyFavoriteToolStripMenuItem.Click += new System.EventHandler(this.showOnlyFavoriteToolStripMenuItem_Click);
            // 
            // clipToolStripMenuItem
            // 
            this.clipToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pasteENTERToolStripMenuItem,
            this.pasteTextCTRLENTERToolStripMenuItem,
            this.toolStripMenuItemPasteChars,
            this.changeClipTitleToolStripMenuItem,
            this.editClipTextToolStripMenuItem,
            this.openInDefaultApplicationToolStripMenuItem,
            this.setFavouriteToolStripMenuItem,
            this.resetFavouriteToolStripMenuItem,
            this.nextMatchToolStripMenuItem,
            this.previousMatchToolStripMenuItem,
            this.wordWrapToolStripMenuItem,
            this.monospacedFontToolStripMenuItem});
            this.clipToolStripMenuItem.Name = "clipToolStripMenuItem";
            resources.ApplyResources(this.clipToolStripMenuItem, "clipToolStripMenuItem");
            // 
            // pasteENTERToolStripMenuItem
            // 
            this.pasteENTERToolStripMenuItem.Name = "pasteENTERToolStripMenuItem";
            resources.ApplyResources(this.pasteENTERToolStripMenuItem, "pasteENTERToolStripMenuItem");
            this.pasteENTERToolStripMenuItem.Click += new System.EventHandler(this.pasteOriginalToolStripMenuItem_Click);
            // 
            // pasteTextCTRLENTERToolStripMenuItem
            // 
            this.pasteTextCTRLENTERToolStripMenuItem.Name = "pasteTextCTRLENTERToolStripMenuItem";
            resources.ApplyResources(this.pasteTextCTRLENTERToolStripMenuItem, "pasteTextCTRLENTERToolStripMenuItem");
            this.pasteTextCTRLENTERToolStripMenuItem.Click += new System.EventHandler(this.pasteAsTextToolStripMenuItem_Click);
            // 
            // toolStripMenuItemPasteChars
            // 
            this.toolStripMenuItemPasteChars.Name = "toolStripMenuItemPasteChars";
            resources.ApplyResources(this.toolStripMenuItemPasteChars, "toolStripMenuItemPasteChars");
            this.toolStripMenuItemPasteChars.Click += new System.EventHandler(this.toolStripMenuItemPasteChars_Click);
            // 
            // changeClipTitleToolStripMenuItem
            // 
            this.changeClipTitleToolStripMenuItem.Name = "changeClipTitleToolStripMenuItem";
            resources.ApplyResources(this.changeClipTitleToolStripMenuItem, "changeClipTitleToolStripMenuItem");
            this.changeClipTitleToolStripMenuItem.Click += new System.EventHandler(this.changeClipTitleToolStripMenuItem_Click);
            // 
            // editClipTextToolStripMenuItem
            // 
            this.editClipTextToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.Edit;
            this.editClipTextToolStripMenuItem.Name = "editClipTextToolStripMenuItem";
            resources.ApplyResources(this.editClipTextToolStripMenuItem, "editClipTextToolStripMenuItem");
            this.editClipTextToolStripMenuItem.Click += new System.EventHandler(this.editClipTextToolStripMenuItem_Click);
            // 
            // openInDefaultApplicationToolStripMenuItem
            // 
            this.openInDefaultApplicationToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.OpenFile;
            this.openInDefaultApplicationToolStripMenuItem.Name = "openInDefaultApplicationToolStripMenuItem";
            resources.ApplyResources(this.openInDefaultApplicationToolStripMenuItem, "openInDefaultApplicationToolStripMenuItem");
            this.openInDefaultApplicationToolStripMenuItem.Click += new System.EventHandler(this.openInDefaultApplicationToolStripMenuItem_Click);
            // 
            // setFavouriteToolStripMenuItem
            // 
            this.setFavouriteToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.MarkFavorite;
            this.setFavouriteToolStripMenuItem.Name = "setFavouriteToolStripMenuItem";
            resources.ApplyResources(this.setFavouriteToolStripMenuItem, "setFavouriteToolStripMenuItem");
            this.setFavouriteToolStripMenuItem.Click += new System.EventHandler(this.setFavouriteToolStripMenuItem_Click);
            // 
            // resetFavouriteToolStripMenuItem
            // 
            this.resetFavouriteToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.UnmarkFavorite;
            this.resetFavouriteToolStripMenuItem.Name = "resetFavouriteToolStripMenuItem";
            resources.ApplyResources(this.resetFavouriteToolStripMenuItem, "resetFavouriteToolStripMenuItem");
            this.resetFavouriteToolStripMenuItem.Click += new System.EventHandler(this.resetFavouriteToolStripMenuItem_Click);
            // 
            // nextMatchToolStripMenuItem
            // 
            this.nextMatchToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.FindNext;
            this.nextMatchToolStripMenuItem.Name = "nextMatchToolStripMenuItem";
            resources.ApplyResources(this.nextMatchToolStripMenuItem, "nextMatchToolStripMenuItem");
            this.nextMatchToolStripMenuItem.Click += new System.EventHandler(this.buttonFindNext_Click);
            // 
            // previousMatchToolStripMenuItem
            // 
            this.previousMatchToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.FindPrevious;
            this.previousMatchToolStripMenuItem.Name = "previousMatchToolStripMenuItem";
            resources.ApplyResources(this.previousMatchToolStripMenuItem, "previousMatchToolStripMenuItem");
            this.previousMatchToolStripMenuItem.Click += new System.EventHandler(this.buttonFindPrevious_Click);
            // 
            // wordWrapToolStripMenuItem
            // 
            this.wordWrapToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.WordWrap;
            this.wordWrapToolStripMenuItem.Name = "wordWrapToolStripMenuItem";
            resources.ApplyResources(this.wordWrapToolStripMenuItem, "wordWrapToolStripMenuItem");
            this.wordWrapToolStripMenuItem.Click += new System.EventHandler(this.wordWrapToolStripMenuItem_Click);
            // 
            // monospacedFontToolStripMenuItem
            // 
            this.monospacedFontToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.FixedWidthFont;
            this.monospacedFontToolStripMenuItem.Name = "monospacedFontToolStripMenuItem";
            resources.ApplyResources(this.monospacedFontToolStripMenuItem, "monospacedFontToolStripMenuItem");
            this.monospacedFontToolStripMenuItem.Click += new System.EventHandler(this.toolStripButtonFixedWidthFont_Click);
            // 
            // settingsToolStripMenuItem1
            // 
            this.settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
            resources.ApplyResources(this.settingsToolStripMenuItem1, "settingsToolStripMenuItem1");
            this.settingsToolStripMenuItem1.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // fToolStripMenuItem
            // 
            this.fToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkUpdateToolStripMenuItem,
            this.historyToolStripMenuItem,
            this.helpToolStripMenuItem,
            this.aboutToolStripMenuItem1});
            this.fToolStripMenuItem.Name = "fToolStripMenuItem";
            resources.ApplyResources(this.fToolStripMenuItem, "fToolStripMenuItem");
            // 
            // checkUpdateToolStripMenuItem
            // 
            this.checkUpdateToolStripMenuItem.Name = "checkUpdateToolStripMenuItem";
            resources.ApplyResources(this.checkUpdateToolStripMenuItem, "checkUpdateToolStripMenuItem");
            this.checkUpdateToolStripMenuItem.Click += new System.EventHandler(this.checkUpdateToolStripMenuItem_Click);
            // 
            // historyToolStripMenuItem
            // 
            this.historyToolStripMenuItem.Name = "historyToolStripMenuItem";
            resources.ApplyResources(this.historyToolStripMenuItem, "historyToolStripMenuItem");
            this.historyToolStripMenuItem.Click += new System.EventHandler(this.historyToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            resources.ApplyResources(this.aboutToolStripMenuItem1, "aboutToolStripMenuItem1");
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            resources.ApplyResources(this.toolStripMenuItem1, "toolStripMenuItem1");
            this.toolStripMenuItem1.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sendPasteToolStripMenuItem,
            this.pasteAsTextToolStripMenuItem,
            this.toolStripMenuItem4});
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            resources.ApplyResources(this.deleteToolStripMenuItem, "deleteToolStripMenuItem");
            // 
            // sendPasteToolStripMenuItem
            // 
            this.sendPasteToolStripMenuItem.Name = "sendPasteToolStripMenuItem";
            resources.ApplyResources(this.sendPasteToolStripMenuItem, "sendPasteToolStripMenuItem");
            this.sendPasteToolStripMenuItem.Click += new System.EventHandler(this.pasteOriginalToolStripMenuItem_Click);
            // 
            // pasteAsTextToolStripMenuItem
            // 
            this.pasteAsTextToolStripMenuItem.Name = "pasteAsTextToolStripMenuItem";
            resources.ApplyResources(this.pasteAsTextToolStripMenuItem, "pasteAsTextToolStripMenuItem");
            this.pasteAsTextToolStripMenuItem.Click += new System.EventHandler(this.pasteAsTextToolStripMenuItem_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            resources.ApplyResources(this.toolStripMenuItem4, "toolStripMenuItem4");
            this.toolStripMenuItem4.Click += new System.EventHandler(this.Delete_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            resources.ApplyResources(this.settingsToolStripMenuItem, "settingsToolStripMenuItem");
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStripNotifyIcon;
            resources.ApplyResources(this.notifyIcon, "notifyIcon");
            this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);
            // 
            // contextMenuStripNotifyIcon
            // 
            this.contextMenuStripNotifyIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.trayMenuItemMonitoringClipboard,
            this.exitToolStripMenuItem1});
            this.contextMenuStripNotifyIcon.Name = "contextMenuStripNotifyIcon";
            resources.ApplyResources(this.contextMenuStripNotifyIcon, "contextMenuStripNotifyIcon");
            // 
            // trayMenuItemMonitoringClipboard
            // 
            this.trayMenuItemMonitoringClipboard.Name = "trayMenuItemMonitoringClipboard";
            resources.ApplyResources(this.trayMenuItemMonitoringClipboard, "trayMenuItemMonitoringClipboard");
            this.trayMenuItemMonitoringClipboard.Click += new System.EventHandler(this.MonitoringClipboardToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem1
            // 
            this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            resources.ApplyResources(this.exitToolStripMenuItem1, "exitToolStripMenuItem1");
            this.exitToolStripMenuItem1.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
            // 
            // toolStrip1
            // 
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonWordWrap,
            this.toolStripButtonFixedWidthFont,
            this.toolStripButtonSelectTopClipOnShow,
            this.toolStripButtonClearFilterAndSelectTop,
            this.toolStripButtonDelete,
            this.toolStripButtonMarkFavourite,
            this.toolStripButtonUnmarkFavourite,
            this.toolStripMenuItemEditClipText,
            this.toolStripMenuItemOpenFile,
            this.toolStripButtonTopMostWindow,
            this.toolStripUpdateToSeparator,
            this.buttonUpdate});
            this.toolStrip1.Name = "toolStrip1";
            // 
            // toolStripButtonWordWrap
            // 
            this.toolStripButtonWordWrap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonWordWrap.Image = global::ClipAngel.Properties.Resources.WordWrap;
            resources.ApplyResources(this.toolStripButtonWordWrap, "toolStripButtonWordWrap");
            this.toolStripButtonWordWrap.Name = "toolStripButtonWordWrap";
            this.toolStripButtonWordWrap.Click += new System.EventHandler(this.wordWrapToolStripMenuItem_Click);
            // 
            // toolStripButtonFixedWidthFont
            // 
            this.toolStripButtonFixedWidthFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonFixedWidthFont.Image = global::ClipAngel.Properties.Resources.FixedWidthFont;
            resources.ApplyResources(this.toolStripButtonFixedWidthFont, "toolStripButtonFixedWidthFont");
            this.toolStripButtonFixedWidthFont.Name = "toolStripButtonFixedWidthFont";
            this.toolStripButtonFixedWidthFont.Click += new System.EventHandler(this.toolStripButtonFixedWidthFont_Click);
            // 
            // toolStripButtonSelectTopClipOnShow
            // 
            this.toolStripButtonSelectTopClipOnShow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSelectTopClipOnShow.Image = global::ClipAngel.Properties.Resources.Top1;
            resources.ApplyResources(this.toolStripButtonSelectTopClipOnShow, "toolStripButtonSelectTopClipOnShow");
            this.toolStripButtonSelectTopClipOnShow.Name = "toolStripButtonSelectTopClipOnShow";
            this.toolStripButtonSelectTopClipOnShow.Click += new System.EventHandler(this.toolStripButtonSelectTopClipOnShow_Click);
            // 
            // toolStripButtonClearFilterAndSelectTop
            // 
            this.toolStripButtonClearFilterAndSelectTop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonClearFilterAndSelectTop.Image = global::ClipAngel.Properties.Resources.Top2;
            resources.ApplyResources(this.toolStripButtonClearFilterAndSelectTop, "toolStripButtonClearFilterAndSelectTop");
            this.toolStripButtonClearFilterAndSelectTop.Name = "toolStripButtonClearFilterAndSelectTop";
            this.toolStripButtonClearFilterAndSelectTop.Click += new System.EventHandler(this.toolStripMenuItemClearFilterAndSelectTop_Click);
            // 
            // toolStripButtonDelete
            // 
            this.toolStripButtonDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonDelete.Image = global::ClipAngel.Properties.Resources.delete;
            resources.ApplyResources(this.toolStripButtonDelete, "toolStripButtonDelete");
            this.toolStripButtonDelete.Name = "toolStripButtonDelete";
            this.toolStripButtonDelete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // toolStripButtonMarkFavourite
            // 
            this.toolStripButtonMarkFavourite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonMarkFavourite.Image = global::ClipAngel.Properties.Resources.MarkFavorite;
            resources.ApplyResources(this.toolStripButtonMarkFavourite, "toolStripButtonMarkFavourite");
            this.toolStripButtonMarkFavourite.Name = "toolStripButtonMarkFavourite";
            this.toolStripButtonMarkFavourite.Click += new System.EventHandler(this.setFavouriteToolStripMenuItem_Click);
            // 
            // toolStripButtonUnmarkFavourite
            // 
            this.toolStripButtonUnmarkFavourite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonUnmarkFavourite.Image = global::ClipAngel.Properties.Resources.UnmarkFavorite;
            resources.ApplyResources(this.toolStripButtonUnmarkFavourite, "toolStripButtonUnmarkFavourite");
            this.toolStripButtonUnmarkFavourite.Name = "toolStripButtonUnmarkFavourite";
            this.toolStripButtonUnmarkFavourite.Click += new System.EventHandler(this.resetFavouriteToolStripMenuItem_Click);
            // 
            // toolStripMenuItemEditClipText
            // 
            this.toolStripMenuItemEditClipText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripMenuItemEditClipText.Image = global::ClipAngel.Properties.Resources.Edit;
            resources.ApplyResources(this.toolStripMenuItemEditClipText, "toolStripMenuItemEditClipText");
            this.toolStripMenuItemEditClipText.Name = "toolStripMenuItemEditClipText";
            this.toolStripMenuItemEditClipText.Click += new System.EventHandler(this.editClipTextToolStripMenuItem_Click);
            // 
            // toolStripMenuItemOpenFile
            // 
            this.toolStripMenuItemOpenFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripMenuItemOpenFile.Image = global::ClipAngel.Properties.Resources.OpenFile;
            this.toolStripMenuItemOpenFile.Name = "toolStripMenuItemOpenFile";
            resources.ApplyResources(this.toolStripMenuItemOpenFile, "toolStripMenuItemOpenFile");
            this.toolStripMenuItemOpenFile.Click += new System.EventHandler(this.openInDefaultApplicationToolStripMenuItem_Click);
            // 
            // toolStripButtonTopMostWindow
            // 
            this.toolStripButtonTopMostWindow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonTopMostWindow.Image = global::ClipAngel.Properties.Resources.TopMostWindow;
            resources.ApplyResources(this.toolStripButtonTopMostWindow, "toolStripButtonTopMostWindow");
            this.toolStripButtonTopMostWindow.Name = "toolStripButtonTopMostWindow";
            this.toolStripButtonTopMostWindow.Click += new System.EventHandler(this.windowAlwaysOnTopToolStripMenuItem_Click);
            // 
            // toolStripUpdateToSeparator
            // 
            this.toolStripUpdateToSeparator.Name = "toolStripUpdateToSeparator";
            resources.ApplyResources(this.toolStripUpdateToSeparator, "toolStripUpdateToSeparator");
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.buttonUpdate, "buttonUpdate");
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
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
            // cultureManager1
            // 
            this.cultureManager1.ManagedControl = this;
            // 
            // timerCheckUpdate
            // 
            this.timerCheckUpdate.Tick += new System.EventHandler(this.timerCheckUpdate_Tick);
            // 
            // timerReconnect
            // 
            this.timerReconnect.Tick += new System.EventHandler(this.timerReconnect_Tick);
            // 
            // timerApplyTextFiler
            // 
            this.timerApplyTextFiler.Interval = 150;
            this.timerApplyTextFiler.Tick += new System.EventHandler(this.timerApplyTextFiler_Tick);
            // 
            // dataGridViewImageColumn1
            // 
            this.dataGridViewImageColumn1.Frozen = true;
            resources.ApplyResources(this.dataGridViewImageColumn1, "dataGridViewImageColumn1");
            this.dataGridViewImageColumn1.Name = "dataGridViewImageColumn1";
            this.dataGridViewImageColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewImageColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // dataGridViewRichTextBoxColumn1
            // 
            this.dataGridViewRichTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.dataGridViewRichTextBoxColumn1, "dataGridViewRichTextBoxColumn1");
            this.dataGridViewRichTextBoxColumn1.Name = "dataGridViewRichTextBoxColumn1";
            this.dataGridViewRichTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewRichTextBoxColumn1.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // Main
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.MainMenu);
            this.KeyPreview = true;
            this.MainMenuStrip = this.MainMenu;
            this.Name = "Main";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Activated += new System.EventHandler(this.Main_Activated);
            this.Deactivate += new System.EventHandler(this.Main_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Main_FormClosed);
            this.Load += new System.EventHandler(this.Main_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Main_KeyDown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.contextMenuStripDataGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.clipBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbDataSet)).EndInit();
            this.tableLayoutPanelData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ImageControl)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.contextMenuStripNotifyIcon.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox textBoxApplication;
        private System.Windows.Forms.TextBox textBoxWindow;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.BindingSource clipBindingSource;
        private System.Windows.Forms.ComboBox comboBoxFilter;
        private System.Windows.Forms.Button buttonClearFilter;
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
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem1;
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
        private System.Windows.Forms.ToolStripStatusLabel stripLabelPosition;
        private System.Windows.Forms.ToolStripStatusLabel StripLabelVisualSize;
        private System.Windows.Forms.ToolStripStatusLabel StripLabelSize;
        private System.Windows.Forms.ToolStripStatusLabel StripLabelType;
        private System.Windows.Forms.ToolStripStatusLabel StripLabelCreated;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clipToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteENTERToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteTextCTRLENTERToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem1;
        private System.Windows.Forms.RichTextBox textBoxUrl;
        private System.Windows.Forms.PictureBox ImageControl;
        private System.Windows.Forms.ToolTip toolTipDynamic;
        private System.Windows.Forms.ComboBox TypeFilter;
        private System.Windows.Forms.Button buttonFindNext;
        private System.Windows.Forms.Button buttonFindPrevious;
        private System.Windows.Forms.ToolStripMenuItem nextMatchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem previousMatchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wordWrapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyClipToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButtonWordWrap;
        private System.Windows.Forms.ToolStripButton toolStripButtonSelectTopClipOnShow;
        private System.Windows.Forms.ToolStripMenuItem fToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripButton toolStripButtonClearFilterAndSelectTop;
        private System.Windows.Forms.ToolStripMenuItem changeClipTitleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem listToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemClearFilterAndSelectTop;
        private System.Windows.Forms.ToolStripMenuItem selectTopClipOnShowToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonDelete;
        private System.Windows.Forms.ToolStripMenuItem setFavouriteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetFavouriteToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonMarkFavourite;
        private System.Windows.Forms.ToolStripButton toolStripButtonUnmarkFavourite;
        private System.Windows.Forms.ComboBox MarkFilter;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
        private System.Windows.Forms.ToolStripMenuItem showAllMarksToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showOnlyUsedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showOnlyFavoriteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveUpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveDownToolStripMenuItem;
        private Infralution.Localization.CultureManager cultureManager1;
        private System.Windows.Forms.ToolStripButton buttonUpdate;
        private System.Windows.Forms.ToolStripMenuItem checkUpdateToolStripMenuItem;
        private System.Windows.Forms.Timer timerCheckUpdate;
        private System.Windows.Forms.ToolStripSeparator toolStripUpdateToSeparator;
        private System.Windows.Forms.ToolStripMenuItem historyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPasteChars;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem openInDefaultApplicationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowAlwaysOnTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonTopMostWindow;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenFile;
        private System.Windows.Forms.ToolStripMenuItem editClipTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripMenuItemEditClipText;
        private System.Windows.Forms.Timer timerReconnect;
        private System.Windows.Forms.Timer timerApplyTextFiler;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn1;
        private DataGridViewRichTextBoxColumn dataGridViewRichTextBoxColumn1;
        private System.Windows.Forms.DataGridViewImageColumn TypeImg;
        private System.Windows.Forms.DataGridViewTextBoxColumn TitleSimple;
        private DataGridViewRichTextBoxColumn Title;
        private System.Windows.Forms.DataGridViewImageColumn ImageSample;
        private System.Windows.Forms.DataGridViewTextBoxColumn VisualWeight;
        private System.Windows.Forms.ToolStripButton toolStripButtonFixedWidthFont;
        private System.Windows.Forms.ToolStripMenuItem monospacedFontToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trayMenuItemMonitoringClipboard;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMonitoringClipboard;
    }
}

