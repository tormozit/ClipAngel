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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.toolStripBottom = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonClearFilter = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonFindNext = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonFindPrevious = new System.Windows.Forms.ToolStripButton();
            this.toolStripFindSettings = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripButtonAutoSelectMatch = new System.Windows.Forms.ToolStripMenuItem();
            this.caseSensetiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.everyWordIndependentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.meandsAnySequenceOfCharsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MarkFilter = new System.Windows.Forms.ComboBox();
            this.TypeFilter = new System.Windows.Forms.ComboBox();
            this.comboBoxFilter = new ClipAngel.CueComboBox();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.AppImage = new System.Windows.Forms.DataGridViewImageColumn();
            this.TypeImage = new System.Windows.Forms.DataGridViewImageColumn();
            this.TitleSimple = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnTitle = new ClipAngel.DataGridViewRichTextBoxColumn();
            this.ImageSample = new System.Windows.Forms.DataGridViewImageColumn();
            this.VisualWeight = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.imageSampleDataGridViewImageColumn = new System.Windows.Forms.DataGridViewImageColumn();
            this.charsDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SizeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CreatedDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.usedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.typeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.titleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.idDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.favoriteDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.contextMenuStripDataGrid = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.clipBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dbDataSet = new ClipAngel.dbDataSet();
            this.pictureBoxSource = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanelData = new System.Windows.Forms.TableLayoutPanel();
            this.urlTextBox = new System.Windows.Forms.RichTextBox();
            this.contextMenuUrl = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripUrlCopyAll = new System.Windows.Forms.ToolStripMenuItem();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.contextMenuStripRtf = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.rtfMenuItemOpenLink = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.rtfMenuItemSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.htmlTextBox = new System.Windows.Forms.WebBrowser();
            this.contextMenuStripHtml = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.htmlMenuItemOpenLink = new System.Windows.Forms.ToolStripMenuItem();
            this.htmlMenuItemCopyLinkAdress = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem15 = new System.Windows.Forms.ToolStripMenuItem();
            this.htmlMenuItemSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.ImageControl = new System.Windows.Forms.PictureBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.stripLabelPosition = new System.Windows.Forms.ToolStripStatusLabel();
            this.StripLabelVisualSize = new System.Windows.Forms.ToolStripStatusLabel();
            this.StripLabelSize = new System.Windows.Forms.ToolStripStatusLabel();
            this.StripLabelType = new System.Windows.Forms.ToolStripStatusLabel();
            this.StripLabelCreated = new System.Windows.Forms.ToolStripStatusLabel();
            this.textBoxApplication = new System.Windows.Forms.TextBox();
            this.contextMenuStripApplication = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyFullFilenameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripApplicationCopyAll = new System.Windows.Forms.ToolStripMenuItem();
            this.textBoxWindow = new System.Windows.Forms.TextBox();
            this.contextMenuWindowTitle = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripWindowTitleCopyAll = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSwitchFocus = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemSetFocusClipText = new System.Windows.Forms.ToolStripMenuItem();
            this.windowAlwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showInTaskbarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemMonitoringClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.сдуфкClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveCopiedClipToTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveUpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveDownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllNonFavoriteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
            this.showAllMarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOnlyFavoriteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOnlyUsedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripSeparator();
            this.showAllTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOnlyImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOnlyFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOnlyTextsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem17 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItemClearFilters = new System.Windows.Forms.ToolStripMenuItem();
            this.returnToPrevousSelectedClipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemClearFilterAndSelectTop = new System.Windows.Forms.ToolStripMenuItem();
            this.clipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteENTERToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteTextCTRLENTERToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemPasteChars = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.changeClipTitleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nextMatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.previousMatchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textFormattingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wordWrapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.monospacedFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setFavouriteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetFavouriteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editClipTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem16 = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeTextOfClipsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textCompareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.translateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ignoreApplicationInCaptureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uploadImageToWebToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.fToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.historyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.supportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.openFavoritesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemCompareLastClips = new System.Windows.Forms.ToolStripMenuItem();
            this.trayMenuItemMonitoringClipboard = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTipDynamic = new System.Windows.Forms.ToolTip(this.components);
            this.toolStripTop = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonTextFormatting = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonWordWrap = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonMonospacedFont = new System.Windows.Forms.ToolStripButton();
            this.moveCopiedClipToTopToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonReturnToPrevous = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonClearFilterAndSelectTop = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonMarkFavorite = new System.Windows.Forms.ToolStripButton();
            this.toolStripMenuItemEditClipText = new System.Windows.Forms.ToolStripButton();
            this.toolStripMenuItemOpenFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButtonOpenWith = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonTopMostWindow = new System.Windows.Forms.ToolStripButton();
            this.toolStripUpdateToSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.buttonUpdate = new System.Windows.Forms.ToolStripButton();
            this.cultureManager1 = new Infralution.Localization.CultureManager(this.components);
            this.timerDaily = new System.Windows.Forms.Timer(this.components);
            this.timerReconnect = new System.Windows.Forms.Timer(this.components);
            this.timerApplyTextFiler = new System.Windows.Forms.Timer(this.components);
            this.dataGridViewImageColumn1 = new System.Windows.Forms.DataGridViewImageColumn();
            this.dataGridViewImageColumn2 = new System.Windows.Forms.DataGridViewImageColumn();
            this.dataGridViewRichTextBoxColumn1 = new ClipAngel.DataGridViewRichTextBoxColumn();
            this.dataGridViewImageColumn3 = new System.Windows.Forms.DataGridViewImageColumn();
            this.dataGridViewImageColumn4 = new System.Windows.Forms.DataGridViewImageColumn();
            this.dataGridViewRichTextBoxColumn2 = new ClipAngel.DataGridViewRichTextBoxColumn();
            this.dataGridViewImageColumn5 = new System.Windows.Forms.DataGridViewImageColumn();
            this.tooltipTimer = new System.Windows.Forms.Timer(this.components);
            this.clipsTableAdapter = new ClipAngel.dbDataSetTableAdapters.ClipsTableAdapter();
            this.tableAdapterManager = new ClipAngel.dbDataSetTableAdapters.TableAdapterManager();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.toolStripBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.contextMenuStripDataGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.clipBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSource)).BeginInit();
            this.tableLayoutPanelData.SuspendLayout();
            this.contextMenuUrl.SuspendLayout();
            this.contextMenuStripRtf.SuspendLayout();
            this.contextMenuStripHtml.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImageControl)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.contextMenuStripApplication.SuspendLayout();
            this.contextMenuWindowTitle.SuspendLayout();
            this.MainMenu.SuspendLayout();
            this.contextMenuStripNotifyIcon.SuspendLayout();
            this.toolStripTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.toolStripBottom);
            this.splitContainer1.Panel1.Controls.Add(this.MarkFilter);
            this.splitContainer1.Panel1.Controls.Add(this.TypeFilter);
            this.splitContainer1.Panel1.Controls.Add(this.comboBoxFilter);
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView);
            // 
            // splitContainer1.Panel2
            // 
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            this.splitContainer1.Panel2.Controls.Add(this.pictureBoxSource);
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanelData);
            this.splitContainer1.Panel2.Controls.Add(this.statusStrip);
            this.splitContainer1.Panel2.Controls.Add(this.textBoxApplication);
            this.splitContainer1.Panel2.Controls.Add(this.textBoxWindow);
            this.splitContainer1.TabStop = false;
            // 
            // toolStripBottom
            // 
            resources.ApplyResources(this.toolStripBottom, "toolStripBottom");
            this.toolStripBottom.CanOverflow = false;
            this.toolStripBottom.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripBottom.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonClearFilter,
            this.toolStripButtonFindNext,
            this.toolStripButtonFindPrevious,
            this.toolStripFindSettings});
            this.toolStripBottom.Name = "toolStripBottom";
            // 
            // toolStripButtonClearFilter
            // 
            this.toolStripButtonClearFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonClearFilter.Image = global::ClipAngel.Properties.Resources.ClearFilter;
            resources.ApplyResources(this.toolStripButtonClearFilter, "toolStripButtonClearFilter");
            this.toolStripButtonClearFilter.Name = "toolStripButtonClearFilter";
            this.toolStripButtonClearFilter.Click += new System.EventHandler(this.ClearFilter_Click);
            // 
            // toolStripButtonFindNext
            // 
            this.toolStripButtonFindNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonFindNext.Image = global::ClipAngel.Properties.Resources.FindNext;
            resources.ApplyResources(this.toolStripButtonFindNext, "toolStripButtonFindNext");
            this.toolStripButtonFindNext.Name = "toolStripButtonFindNext";
            this.toolStripButtonFindNext.Click += new System.EventHandler(this.buttonFindNext_Click);
            // 
            // toolStripButtonFindPrevious
            // 
            this.toolStripButtonFindPrevious.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonFindPrevious.Image = global::ClipAngel.Properties.Resources.FindPrevious;
            resources.ApplyResources(this.toolStripButtonFindPrevious, "toolStripButtonFindPrevious");
            this.toolStripButtonFindPrevious.Name = "toolStripButtonFindPrevious";
            this.toolStripButtonFindPrevious.Click += new System.EventHandler(this.buttonFindPrevious_Click);
            // 
            // toolStripFindSettings
            // 
            this.toolStripFindSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripFindSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonAutoSelectMatch,
            this.caseSensetiveToolStripMenuItem,
            this.everyWordIndependentToolStripMenuItem,
            this.meandsAnySequenceOfCharsToolStripMenuItem});
            this.toolStripFindSettings.Image = global::ClipAngel.Properties.Resources.Autoselect_first_match;
            resources.ApplyResources(this.toolStripFindSettings, "toolStripFindSettings");
            this.toolStripFindSettings.Name = "toolStripFindSettings";
            // 
            // toolStripButtonAutoSelectMatch
            // 
            this.toolStripButtonAutoSelectMatch.Name = "toolStripButtonAutoSelectMatch";
            resources.ApplyResources(this.toolStripButtonAutoSelectMatch, "toolStripButtonAutoSelectMatch");
            this.toolStripButtonAutoSelectMatch.Click += new System.EventHandler(this.buttonAutoSelectMatch_Click);
            // 
            // caseSensetiveToolStripMenuItem
            // 
            this.caseSensetiveToolStripMenuItem.Name = "caseSensetiveToolStripMenuItem";
            resources.ApplyResources(this.caseSensetiveToolStripMenuItem, "caseSensetiveToolStripMenuItem");
            // 
            // everyWordIndependentToolStripMenuItem
            // 
            this.everyWordIndependentToolStripMenuItem.Name = "everyWordIndependentToolStripMenuItem";
            resources.ApplyResources(this.everyWordIndependentToolStripMenuItem, "everyWordIndependentToolStripMenuItem");
            // 
            // meandsAnySequenceOfCharsToolStripMenuItem
            // 
            this.meandsAnySequenceOfCharsToolStripMenuItem.Name = "meandsAnySequenceOfCharsToolStripMenuItem";
            resources.ApplyResources(this.meandsAnySequenceOfCharsToolStripMenuItem, "meandsAnySequenceOfCharsToolStripMenuItem");
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
            // comboBoxFilter
            // 
            resources.ApplyResources(this.comboBoxFilter, "comboBoxFilter");
            this.comboBoxFilter.Name = "comboBoxFilter";
            this.comboBoxFilter.TabStop = false;
            this.toolTipDynamic.SetToolTip(this.comboBoxFilter, resources.GetString("comboBoxFilter.ToolTip"));
            this.comboBoxFilter.TextChanged += new System.EventHandler(this.Filter_TextChanged);
            this.comboBoxFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Filter_KeyDown);
            this.comboBoxFilter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Filter_KeyPress);
            this.comboBoxFilter.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Filter_KeyUp);
            this.comboBoxFilter.MouseEnter += new System.EventHandler(this.comboBoxFilter_MouseEnter);
            this.comboBoxFilter.MouseHover += new System.EventHandler(this.comboBoxFilter_MouseHover);
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
            this.AppImage,
            this.TypeImage,
            this.TitleSimple,
            this.ColumnTitle,
            this.ImageSample,
            this.VisualWeight,
            this.imageSampleDataGridViewImageColumn,
            this.charsDataGridViewTextBoxColumn,
            this.SizeDataGridViewTextBoxColumn,
            this.CreatedDataGridViewTextBoxColumn,
            this.usedDataGridViewCheckBoxColumn,
            this.typeDataGridViewTextBoxColumn,
            this.titleDataGridViewTextBoxColumn,
            this.idDataGridViewTextBoxColumn,
            this.favoriteDataGridViewCheckBoxColumn});
            this.dataGridView.ContextMenuStrip = this.contextMenuStripDataGrid;
            this.dataGridView.DataSource = this.clipBindingSource;
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.ReadOnly = true;
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.RowTemplate.Height = 19;
            this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView.StandardTab = true;
            this.dataGridView.TabStop = false;
            this.dataGridView.Tag = "";
            this.dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridView_CellFormatting);
            this.dataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView_CellMouseDown);
            this.dataGridView.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellMouseEnter);
            this.dataGridView.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_CellMouseLeave);
            this.dataGridView.RowPrePaint += new System.Windows.Forms.DataGridViewRowPrePaintEventHandler(this.dataGridView_RowPrePaint);
            this.dataGridView.SelectionChanged += new System.EventHandler(this.dataGridView_SelectionChanged);
            this.dataGridView.DoubleClick += new System.EventHandler(this.dataGridView_DoubleClick);
            this.dataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView_KeyDown);
            this.dataGridView.MouseHover += new System.EventHandler(this.dataGridView_MouseHover);
            // 
            // AppImage
            // 
            resources.ApplyResources(this.AppImage, "AppImage");
            this.AppImage.Name = "AppImage";
            this.AppImage.ReadOnly = true;
            this.AppImage.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // TypeImage
            // 
            resources.ApplyResources(this.TypeImage, "TypeImage");
            this.TypeImage.Name = "TypeImage";
            this.TypeImage.ReadOnly = true;
            this.TypeImage.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // TitleSimple
            // 
            this.TitleSimple.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.TitleSimple.DataPropertyName = "Title";
            resources.ApplyResources(this.TitleSimple, "TitleSimple");
            this.TitleSimple.Name = "TitleSimple";
            this.TitleSimple.ReadOnly = true;
            this.TitleSimple.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // ColumnTitle
            // 
            this.ColumnTitle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.ColumnTitle, "ColumnTitle");
            this.ColumnTitle.Name = "ColumnTitle";
            this.ColumnTitle.ReadOnly = true;
            this.ColumnTitle.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // ImageSample
            // 
            resources.ApplyResources(this.ImageSample, "ImageSample");
            this.ImageSample.Name = "ImageSample";
            this.ImageSample.ReadOnly = true;
            // 
            // VisualWeight
            // 
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Green;
            this.VisualWeight.DefaultCellStyle = dataGridViewCellStyle1;
            resources.ApplyResources(this.VisualWeight, "VisualWeight");
            this.VisualWeight.Name = "VisualWeight";
            this.VisualWeight.ReadOnly = true;
            this.VisualWeight.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.VisualWeight.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // imageSampleDataGridViewImageColumn
            // 
            this.imageSampleDataGridViewImageColumn.DataPropertyName = "ImageSample";
            resources.ApplyResources(this.imageSampleDataGridViewImageColumn, "imageSampleDataGridViewImageColumn");
            this.imageSampleDataGridViewImageColumn.Name = "imageSampleDataGridViewImageColumn";
            this.imageSampleDataGridViewImageColumn.ReadOnly = true;
            // 
            // charsDataGridViewTextBoxColumn
            // 
            this.charsDataGridViewTextBoxColumn.DataPropertyName = "Chars";
            resources.ApplyResources(this.charsDataGridViewTextBoxColumn, "charsDataGridViewTextBoxColumn");
            this.charsDataGridViewTextBoxColumn.Name = "charsDataGridViewTextBoxColumn";
            this.charsDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // SizeDataGridViewTextBoxColumn
            // 
            this.SizeDataGridViewTextBoxColumn.DataPropertyName = "Size";
            resources.ApplyResources(this.SizeDataGridViewTextBoxColumn, "SizeDataGridViewTextBoxColumn");
            this.SizeDataGridViewTextBoxColumn.Name = "SizeDataGridViewTextBoxColumn";
            this.SizeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // CreatedDataGridViewTextBoxColumn
            // 
            this.CreatedDataGridViewTextBoxColumn.DataPropertyName = "Created";
            resources.ApplyResources(this.CreatedDataGridViewTextBoxColumn, "CreatedDataGridViewTextBoxColumn");
            this.CreatedDataGridViewTextBoxColumn.Name = "CreatedDataGridViewTextBoxColumn";
            this.CreatedDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // usedDataGridViewCheckBoxColumn
            // 
            this.usedDataGridViewCheckBoxColumn.DataPropertyName = "Used";
            resources.ApplyResources(this.usedDataGridViewCheckBoxColumn, "usedDataGridViewCheckBoxColumn");
            this.usedDataGridViewCheckBoxColumn.Name = "usedDataGridViewCheckBoxColumn";
            this.usedDataGridViewCheckBoxColumn.ReadOnly = true;
            // 
            // typeDataGridViewTextBoxColumn
            // 
            this.typeDataGridViewTextBoxColumn.DataPropertyName = "Type";
            resources.ApplyResources(this.typeDataGridViewTextBoxColumn, "typeDataGridViewTextBoxColumn");
            this.typeDataGridViewTextBoxColumn.Name = "typeDataGridViewTextBoxColumn";
            this.typeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // titleDataGridViewTextBoxColumn
            // 
            this.titleDataGridViewTextBoxColumn.DataPropertyName = "Title";
            resources.ApplyResources(this.titleDataGridViewTextBoxColumn, "titleDataGridViewTextBoxColumn");
            this.titleDataGridViewTextBoxColumn.Name = "titleDataGridViewTextBoxColumn";
            this.titleDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // idDataGridViewTextBoxColumn
            // 
            this.idDataGridViewTextBoxColumn.DataPropertyName = "Id";
            resources.ApplyResources(this.idDataGridViewTextBoxColumn, "idDataGridViewTextBoxColumn");
            this.idDataGridViewTextBoxColumn.Name = "idDataGridViewTextBoxColumn";
            this.idDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // favoriteDataGridViewCheckBoxColumn
            // 
            this.favoriteDataGridViewCheckBoxColumn.DataPropertyName = "Favorite";
            resources.ApplyResources(this.favoriteDataGridViewCheckBoxColumn, "favoriteDataGridViewCheckBoxColumn");
            this.favoriteDataGridViewCheckBoxColumn.Name = "favoriteDataGridViewCheckBoxColumn";
            this.favoriteDataGridViewCheckBoxColumn.ReadOnly = true;
            // 
            // contextMenuStripDataGrid
            // 
            this.contextMenuStripDataGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem3,
            this.toolStripMenuItem7,
            this.copyToClipboardToolStripMenuItem,
            this.toolStripMenuItem12,
            this.toolStripMenuItem14,
            this.toolStripMenuItem11,
            this.toolStripMenuItem10,
            this.deleteToolStripMenuItem1});
            this.contextMenuStripDataGrid.Name = "contextMenuStripDataGrid";
            resources.ApplyResources(this.contextMenuStripDataGrid, "contextMenuStripDataGrid");
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Image = global::ClipAngel.Properties.Resources.Paste;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            resources.ApplyResources(this.toolStripMenuItem2, "toolStripMenuItem2");
            this.toolStripMenuItem2.Click += new System.EventHandler(this.pasteOriginalToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Image = global::ClipAngel.Properties.Resources.Paste;
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            resources.ApplyResources(this.toolStripMenuItem3, "toolStripMenuItem3");
            this.toolStripMenuItem3.Click += new System.EventHandler(this.pasteAsTextToolStripMenuItem_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.Image = global::ClipAngel.Properties.Resources.keyboard;
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            resources.ApplyResources(this.toolStripMenuItem7, "toolStripMenuItem7");
            this.toolStripMenuItem7.Click += new System.EventHandler(this.toolStripMenuItemPasteChars_Click);
            // 
            // copyToClipboardToolStripMenuItem
            // 
            this.copyToClipboardToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.copy;
            this.copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
            resources.ApplyResources(this.copyToClipboardToolStripMenuItem, "copyToClipboardToolStripMenuItem");
            this.copyToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyClipToolStripMenuItem_Click);
            // 
            // toolStripMenuItem12
            // 
            this.toolStripMenuItem12.Name = "toolStripMenuItem12";
            resources.ApplyResources(this.toolStripMenuItem12, "toolStripMenuItem12");
            this.toolStripMenuItem12.Click += new System.EventHandler(this.changeClipTitleToolStripMenuItem_Click);
            // 
            // toolStripMenuItem14
            // 
            this.toolStripMenuItem14.Image = global::ClipAngel.Properties.Resources.MarkFavorite;
            this.toolStripMenuItem14.Name = "toolStripMenuItem14";
            resources.ApplyResources(this.toolStripMenuItem14, "toolStripMenuItem14");
            this.toolStripMenuItem14.Click += new System.EventHandler(this.setFavoriteToolStripMenuItem_Click);
            // 
            // toolStripMenuItem11
            // 
            this.toolStripMenuItem11.Image = global::ClipAngel.Properties.Resources.OpenFile;
            this.toolStripMenuItem11.Name = "toolStripMenuItem11";
            resources.ApplyResources(this.toolStripMenuItem11, "toolStripMenuItem11");
            this.toolStripMenuItem11.Click += new System.EventHandler(this.openInDefaultApplicationToolStripMenuItem_Click);
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.Image = global::ClipAngel.Properties.Resources.Compare;
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            resources.ApplyResources(this.toolStripMenuItem10, "toolStripMenuItem10");
            this.toolStripMenuItem10.Click += new System.EventHandler(this.textCompareToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem1
            // 
            this.deleteToolStripMenuItem1.Image = global::ClipAngel.Properties.Resources.delete;
            resources.ApplyResources(this.deleteToolStripMenuItem1, "deleteToolStripMenuItem1");
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
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
            // pictureBoxSource
            // 
            resources.ApplyResources(this.pictureBoxSource, "pictureBoxSource");
            this.pictureBoxSource.Name = "pictureBoxSource";
            this.pictureBoxSource.TabStop = false;
            this.toolTipDynamic.SetToolTip(this.pictureBoxSource, resources.GetString("pictureBoxSource.ToolTip"));
            // 
            // tableLayoutPanelData
            // 
            resources.ApplyResources(this.tableLayoutPanelData, "tableLayoutPanelData");
            this.tableLayoutPanelData.Controls.Add(this.urlTextBox, 0, 3);
            this.tableLayoutPanelData.Controls.Add(this.richTextBox, 0, 0);
            this.tableLayoutPanelData.Controls.Add(this.htmlTextBox, 0, 1);
            this.tableLayoutPanelData.Controls.Add(this.ImageControl, 0, 2);
            this.tableLayoutPanelData.Name = "tableLayoutPanelData";
            // 
            // urlTextBox
            // 
            resources.ApplyResources(this.urlTextBox, "urlTextBox");
            this.urlTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.urlTextBox.ContextMenuStrip = this.contextMenuUrl;
            this.urlTextBox.DetectUrls = false;
            this.urlTextBox.HideSelection = false;
            this.urlTextBox.Name = "urlTextBox";
            this.urlTextBox.ReadOnly = true;
            this.urlTextBox.TabStop = false;
            this.toolTipDynamic.SetToolTip(this.urlTextBox, resources.GetString("urlTextBox.ToolTip"));
            this.urlTextBox.Click += new System.EventHandler(this.textBoxUrl_Click);
            // 
            // contextMenuUrl
            // 
            this.contextMenuUrl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripUrlCopyAll});
            this.contextMenuUrl.Name = "contextMenuUrl";
            resources.ApplyResources(this.contextMenuUrl, "contextMenuUrl");
            // 
            // toolStripUrlCopyAll
            // 
            this.toolStripUrlCopyAll.Name = "toolStripUrlCopyAll";
            resources.ApplyResources(this.toolStripUrlCopyAll, "toolStripUrlCopyAll");
            this.toolStripUrlCopyAll.Click += new System.EventHandler(this.toolStripUrlCopyAll_Click);
            // 
            // richTextBox
            // 
            this.richTextBox.AcceptsTab = true;
            resources.ApplyResources(this.richTextBox, "richTextBox");
            this.richTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBox.ContextMenuStrip = this.contextMenuStripRtf;
            this.richTextBox.DetectUrls = false;
            this.richTextBox.HideSelection = false;
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.ReadOnly = true;
            this.richTextBox.SelectionChanged += new System.EventHandler(this.richTextBox_SelectionChanged);
            this.richTextBox.Click += new System.EventHandler(this.RichText_Click);
            this.richTextBox.Enter += new System.EventHandler(this.richTextBox_Enter);
            this.richTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.richTextBox_KeyDown);
            // 
            // contextMenuStripRtf
            // 
            this.contextMenuStripRtf.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rtfMenuItemOpenLink,
            this.copyToolStripMenuItem,
            this.toolStripMenuItem6,
            this.rtfMenuItemSelectAll});
            this.contextMenuStripRtf.Name = "contextMenuStripRtf";
            resources.ApplyResources(this.contextMenuStripRtf, "contextMenuStripRtf");
            // 
            // rtfMenuItemOpenLink
            // 
            this.rtfMenuItemOpenLink.Name = "rtfMenuItemOpenLink";
            resources.ApplyResources(this.rtfMenuItemOpenLink, "rtfMenuItemOpenLink");
            this.rtfMenuItemOpenLink.Click += new System.EventHandler(this.rtfMenuItemOpenLink_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            resources.ApplyResources(this.copyToolStripMenuItem, "copyToolStripMenuItem");
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.Image = global::ClipAngel.Properties.Resources.Paste;
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            resources.ApplyResources(this.toolStripMenuItem6, "toolStripMenuItem6");
            this.toolStripMenuItem6.Click += new System.EventHandler(this.pasteAsTextToolStripMenuItem_Click);
            // 
            // rtfMenuItemSelectAll
            // 
            this.rtfMenuItemSelectAll.Name = "rtfMenuItemSelectAll";
            resources.ApplyResources(this.rtfMenuItemSelectAll, "rtfMenuItemSelectAll");
            this.rtfMenuItemSelectAll.Click += new System.EventHandler(this.rtfMenuItemSelectAll_Click);
            // 
            // htmlTextBox
            // 
            this.htmlTextBox.AllowNavigation = false;
            this.htmlTextBox.AllowWebBrowserDrop = false;
            resources.ApplyResources(this.htmlTextBox, "htmlTextBox");
            this.htmlTextBox.ContextMenuStrip = this.contextMenuStripHtml;
            this.htmlTextBox.IsWebBrowserContextMenuEnabled = false;
            this.htmlTextBox.Name = "htmlTextBox";
            this.htmlTextBox.ScriptErrorsSuppressed = true;
            this.htmlTextBox.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.htmlTextBox_DocumentCompleted);
            // 
            // contextMenuStripHtml
            // 
            this.contextMenuStripHtml.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.htmlMenuItemOpenLink,
            this.htmlMenuItemCopyLinkAdress,
            this.toolStripMenuItem15,
            this.htmlMenuItemSelectAll});
            this.contextMenuStripHtml.Name = "contextMenuStripHtml";
            resources.ApplyResources(this.contextMenuStripHtml, "contextMenuStripHtml");
            // 
            // htmlMenuItemOpenLink
            // 
            this.htmlMenuItemOpenLink.Name = "htmlMenuItemOpenLink";
            resources.ApplyResources(this.htmlMenuItemOpenLink, "htmlMenuItemOpenLink");
            this.htmlMenuItemOpenLink.Click += new System.EventHandler(this.openLinkInBrowserToolStripMenuItem_Click);
            // 
            // htmlMenuItemCopyLinkAdress
            // 
            this.htmlMenuItemCopyLinkAdress.Name = "htmlMenuItemCopyLinkAdress";
            resources.ApplyResources(this.htmlMenuItemCopyLinkAdress, "htmlMenuItemCopyLinkAdress");
            this.htmlMenuItemCopyLinkAdress.Click += new System.EventHandler(this.copyLinkAdressToolStripMenuItem_Click);
            // 
            // toolStripMenuItem15
            // 
            this.toolStripMenuItem15.Image = global::ClipAngel.Properties.Resources.Paste;
            this.toolStripMenuItem15.Name = "toolStripMenuItem15";
            resources.ApplyResources(this.toolStripMenuItem15, "toolStripMenuItem15");
            this.toolStripMenuItem15.Click += new System.EventHandler(this.pasteAsTextToolStripMenuItem_Click);
            // 
            // htmlMenuItemSelectAll
            // 
            this.htmlMenuItemSelectAll.Name = "htmlMenuItemSelectAll";
            resources.ApplyResources(this.htmlMenuItemSelectAll, "htmlMenuItemSelectAll");
            this.htmlMenuItemSelectAll.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // ImageControl
            // 
            resources.ApplyResources(this.ImageControl, "ImageControl");
            this.ImageControl.BackColor = System.Drawing.SystemColors.Control;
            this.ImageControl.Name = "ImageControl";
            this.ImageControl.TabStop = false;
            this.ImageControl.DoubleClick += new System.EventHandler(this.ImageControl_DoubleClick);
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
            this.textBoxApplication.ContextMenuStrip = this.contextMenuStripApplication;
            this.textBoxApplication.Name = "textBoxApplication";
            this.textBoxApplication.ReadOnly = true;
            this.textBoxApplication.TabStop = false;
            this.toolTipDynamic.SetToolTip(this.textBoxApplication, resources.GetString("textBoxApplication.ToolTip"));
            // 
            // contextMenuStripApplication
            // 
            this.contextMenuStripApplication.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyFullFilenameToolStripMenuItem,
            this.toolStripApplicationCopyAll});
            this.contextMenuStripApplication.Name = "contextMenuStripApplication";
            resources.ApplyResources(this.contextMenuStripApplication, "contextMenuStripApplication");
            // 
            // copyFullFilenameToolStripMenuItem
            // 
            this.copyFullFilenameToolStripMenuItem.Name = "copyFullFilenameToolStripMenuItem";
            resources.ApplyResources(this.copyFullFilenameToolStripMenuItem, "copyFullFilenameToolStripMenuItem");
            this.copyFullFilenameToolStripMenuItem.Click += new System.EventHandler(this.copyFullFilenameToolStripMenuItem_Click);
            // 
            // toolStripApplicationCopyAll
            // 
            this.toolStripApplicationCopyAll.Name = "toolStripApplicationCopyAll";
            resources.ApplyResources(this.toolStripApplicationCopyAll, "toolStripApplicationCopyAll");
            this.toolStripApplicationCopyAll.Click += new System.EventHandler(this.toolStripApplicationCopyAll_Click);
            // 
            // textBoxWindow
            // 
            resources.ApplyResources(this.textBoxWindow, "textBoxWindow");
            this.textBoxWindow.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxWindow.ContextMenuStrip = this.contextMenuWindowTitle;
            this.textBoxWindow.Name = "textBoxWindow";
            this.textBoxWindow.ReadOnly = true;
            this.textBoxWindow.TabStop = false;
            this.toolTipDynamic.SetToolTip(this.textBoxWindow, resources.GetString("textBoxWindow.ToolTip"));
            // 
            // contextMenuWindowTitle
            // 
            this.contextMenuWindowTitle.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripWindowTitleCopyAll});
            this.contextMenuWindowTitle.Name = "contextMenuWindowTitle";
            resources.ApplyResources(this.contextMenuWindowTitle, "contextMenuWindowTitle");
            // 
            // toolStripWindowTitleCopyAll
            // 
            this.toolStripWindowTitleCopyAll.Name = "toolStripWindowTitleCopyAll";
            resources.ApplyResources(this.toolStripWindowTitleCopyAll, "toolStripWindowTitleCopyAll");
            this.toolStripWindowTitleCopyAll.Click += new System.EventHandler(this.toolStripWindowTitleCopyAll_Click);
            // 
            // dataGridViewTextBoxColumn2
            // 
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
            this.toolStripMenuItemSwitchFocus,
            this.toolStripMenuItemSetFocusClipText,
            this.windowAlwaysOnTopToolStripMenuItem,
            this.showInTaskbarToolStripMenuItem,
            this.toolStripMenuItemMonitoringClipboard,
            this.сдуфкClipboardToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem1.Name = "fileToolStripMenuItem1";
            resources.ApplyResources(this.fileToolStripMenuItem1, "fileToolStripMenuItem1");
            // 
            // toolStripMenuItemSwitchFocus
            // 
            this.toolStripMenuItemSwitchFocus.Name = "toolStripMenuItemSwitchFocus";
            resources.ApplyResources(this.toolStripMenuItemSwitchFocus, "toolStripMenuItemSwitchFocus");
            this.toolStripMenuItemSwitchFocus.Click += new System.EventHandler(this.activateListToolStripMenuItem_Click);
            // 
            // toolStripMenuItemSetFocusClipText
            // 
            this.toolStripMenuItemSetFocusClipText.Name = "toolStripMenuItemSetFocusClipText";
            resources.ApplyResources(this.toolStripMenuItemSetFocusClipText, "toolStripMenuItemSetFocusClipText");
            this.toolStripMenuItemSetFocusClipText.Click += new System.EventHandler(this.toolStripMenuItem16_Click);
            // 
            // windowAlwaysOnTopToolStripMenuItem
            // 
            this.windowAlwaysOnTopToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.TopMostWindow;
            this.windowAlwaysOnTopToolStripMenuItem.Name = "windowAlwaysOnTopToolStripMenuItem";
            resources.ApplyResources(this.windowAlwaysOnTopToolStripMenuItem, "windowAlwaysOnTopToolStripMenuItem");
            this.windowAlwaysOnTopToolStripMenuItem.Click += new System.EventHandler(this.windowAlwaysOnTopToolStripMenuItem_Click);
            // 
            // showInTaskbarToolStripMenuItem
            // 
            this.showInTaskbarToolStripMenuItem.Name = "showInTaskbarToolStripMenuItem";
            resources.ApplyResources(this.showInTaskbarToolStripMenuItem, "showInTaskbarToolStripMenuItem");
            this.showInTaskbarToolStripMenuItem.Click += new System.EventHandler(this.showInTaskbarToolStripMenuItem_Click);
            // 
            // toolStripMenuItemMonitoringClipboard
            // 
            this.toolStripMenuItemMonitoringClipboard.Image = global::ClipAngel.Properties.Resources.eye;
            this.toolStripMenuItemMonitoringClipboard.Name = "toolStripMenuItemMonitoringClipboard";
            resources.ApplyResources(this.toolStripMenuItemMonitoringClipboard, "toolStripMenuItemMonitoringClipboard");
            this.toolStripMenuItemMonitoringClipboard.Click += new System.EventHandler(this.MonitoringClipboardToolStripMenuItem_Click);
            // 
            // сдуфкClipboardToolStripMenuItem
            // 
            this.сдуфкClipboardToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.clear2;
            resources.ApplyResources(this.сдуфкClipboardToolStripMenuItem, "сдуфкClipboardToolStripMenuItem");
            this.сдуфкClipboardToolStripMenuItem.Name = "сдуфкClipboardToolStripMenuItem";
            this.сдуфкClipboardToolStripMenuItem.Click += new System.EventHandler(this.clearClipboardToolStripMenuItem_Click);
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
            this.moveCopiedClipToTopToolStripMenuItem,
            this.moveTopToolStripMenuItem,
            this.moveUpToolStripMenuItem,
            this.moveDownToolStripMenuItem,
            this.deleteAllNonFavoriteToolStripMenuItem,
            this.toolStripMenuItem5,
            this.showAllMarksToolStripMenuItem,
            this.showOnlyFavoriteToolStripMenuItem,
            this.showOnlyUsedToolStripMenuItem,
            this.toolStripMenuItem13,
            this.showAllTypesToolStripMenuItem,
            this.showOnlyImagesToolStripMenuItem,
            this.showOnlyFilesToolStripMenuItem,
            this.showOnlyTextsToolStripMenuItem,
            this.toolStripMenuItem17,
            this.toolStripMenuItemClearFilters,
            this.returnToPrevousSelectedClipToolStripMenuItem,
            this.toolStripMenuItemClearFilterAndSelectTop});
            this.listToolStripMenuItem.Name = "listToolStripMenuItem";
            resources.ApplyResources(this.listToolStripMenuItem, "listToolStripMenuItem");
            // 
            // moveCopiedClipToTopToolStripMenuItem
            // 
            this.moveCopiedClipToTopToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.MoveCopiedClipToTop;
            this.moveCopiedClipToTopToolStripMenuItem.Name = "moveCopiedClipToTopToolStripMenuItem";
            resources.ApplyResources(this.moveCopiedClipToTopToolStripMenuItem, "moveCopiedClipToTopToolStripMenuItem");
            this.moveCopiedClipToTopToolStripMenuItem.Click += new System.EventHandler(this.moveClipToTopToolStripMenuItem_Click);
            // 
            // moveTopToolStripMenuItem
            // 
            this.moveTopToolStripMenuItem.Name = "moveTopToolStripMenuItem";
            resources.ApplyResources(this.moveTopToolStripMenuItem, "moveTopToolStripMenuItem");
            this.moveTopToolStripMenuItem.Click += new System.EventHandler(this.moveTopToolStripMenuItem_Click);
            // 
            // moveUpToolStripMenuItem
            // 
            this.moveUpToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.up;
            this.moveUpToolStripMenuItem.Name = "moveUpToolStripMenuItem";
            resources.ApplyResources(this.moveUpToolStripMenuItem, "moveUpToolStripMenuItem");
            this.moveUpToolStripMenuItem.Click += new System.EventHandler(this.moveUpToolStripMenuItem_Click);
            // 
            // moveDownToolStripMenuItem
            // 
            this.moveDownToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.down;
            this.moveDownToolStripMenuItem.Name = "moveDownToolStripMenuItem";
            resources.ApplyResources(this.moveDownToolStripMenuItem, "moveDownToolStripMenuItem");
            this.moveDownToolStripMenuItem.Click += new System.EventHandler(this.moveDownToolStripMenuItem_Click);
            // 
            // deleteAllNonFavoriteToolStripMenuItem
            // 
            this.deleteAllNonFavoriteToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.clear1;
            this.deleteAllNonFavoriteToolStripMenuItem.Name = "deleteAllNonFavoriteToolStripMenuItem";
            resources.ApplyResources(this.deleteAllNonFavoriteToolStripMenuItem, "deleteAllNonFavoriteToolStripMenuItem");
            this.deleteAllNonFavoriteToolStripMenuItem.Click += new System.EventHandler(this.deleteAllNonFavoriteToolStripMenuItem_Click);
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
            // showOnlyFavoriteToolStripMenuItem
            // 
            this.showOnlyFavoriteToolStripMenuItem.Name = "showOnlyFavoriteToolStripMenuItem";
            resources.ApplyResources(this.showOnlyFavoriteToolStripMenuItem, "showOnlyFavoriteToolStripMenuItem");
            this.showOnlyFavoriteToolStripMenuItem.Click += new System.EventHandler(this.showOnlyFavoriteToolStripMenuItem_Click);
            // 
            // showOnlyUsedToolStripMenuItem
            // 
            this.showOnlyUsedToolStripMenuItem.Name = "showOnlyUsedToolStripMenuItem";
            resources.ApplyResources(this.showOnlyUsedToolStripMenuItem, "showOnlyUsedToolStripMenuItem");
            this.showOnlyUsedToolStripMenuItem.Click += new System.EventHandler(this.showOnlyUsedToolStripMenuItem_Click);
            // 
            // toolStripMenuItem13
            // 
            this.toolStripMenuItem13.Name = "toolStripMenuItem13";
            resources.ApplyResources(this.toolStripMenuItem13, "toolStripMenuItem13");
            // 
            // showAllTypesToolStripMenuItem
            // 
            this.showAllTypesToolStripMenuItem.Name = "showAllTypesToolStripMenuItem";
            resources.ApplyResources(this.showAllTypesToolStripMenuItem, "showAllTypesToolStripMenuItem");
            this.showAllTypesToolStripMenuItem.Click += new System.EventHandler(this.toolStripMenuItemShowAllTypes_Click);
            // 
            // showOnlyImagesToolStripMenuItem
            // 
            this.showOnlyImagesToolStripMenuItem.Name = "showOnlyImagesToolStripMenuItem";
            resources.ApplyResources(this.showOnlyImagesToolStripMenuItem, "showOnlyImagesToolStripMenuItem");
            this.showOnlyImagesToolStripMenuItem.Click += new System.EventHandler(this.showOnlyImagesToolStripMenuItem_Click);
            // 
            // showOnlyFilesToolStripMenuItem
            // 
            this.showOnlyFilesToolStripMenuItem.Name = "showOnlyFilesToolStripMenuItem";
            resources.ApplyResources(this.showOnlyFilesToolStripMenuItem, "showOnlyFilesToolStripMenuItem");
            this.showOnlyFilesToolStripMenuItem.Click += new System.EventHandler(this.showOnlyFilesToolStripMenuItem_Click);
            // 
            // showOnlyTextsToolStripMenuItem
            // 
            this.showOnlyTextsToolStripMenuItem.Name = "showOnlyTextsToolStripMenuItem";
            resources.ApplyResources(this.showOnlyTextsToolStripMenuItem, "showOnlyTextsToolStripMenuItem");
            this.showOnlyTextsToolStripMenuItem.Click += new System.EventHandler(this.showOnlyTextsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem17
            // 
            this.toolStripMenuItem17.Name = "toolStripMenuItem17";
            resources.ApplyResources(this.toolStripMenuItem17, "toolStripMenuItem17");
            // 
            // toolStripMenuItemClearFilters
            // 
            this.toolStripMenuItemClearFilters.Image = global::ClipAngel.Properties.Resources.ClearFilter;
            this.toolStripMenuItemClearFilters.Name = "toolStripMenuItemClearFilters";
            resources.ApplyResources(this.toolStripMenuItemClearFilters, "toolStripMenuItemClearFilters");
            this.toolStripMenuItemClearFilters.Click += new System.EventHandler(this.toolStripMenuItemClearFilters_Click);
            // 
            // returnToPrevousSelectedClipToolStripMenuItem
            // 
            this.returnToPrevousSelectedClipToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.Back;
            this.returnToPrevousSelectedClipToolStripMenuItem.Name = "returnToPrevousSelectedClipToolStripMenuItem";
            resources.ApplyResources(this.returnToPrevousSelectedClipToolStripMenuItem, "returnToPrevousSelectedClipToolStripMenuItem");
            this.returnToPrevousSelectedClipToolStripMenuItem.Click += new System.EventHandler(this.returnToPrevousSelectedClipToolStripMenuItem_Click);
            // 
            // toolStripMenuItemClearFilterAndSelectTop
            // 
            this.toolStripMenuItemClearFilterAndSelectTop.Image = global::ClipAngel.Properties.Resources.Top2;
            this.toolStripMenuItemClearFilterAndSelectTop.Name = "toolStripMenuItemClearFilterAndSelectTop";
            resources.ApplyResources(this.toolStripMenuItemClearFilterAndSelectTop, "toolStripMenuItemClearFilterAndSelectTop");
            this.toolStripMenuItemClearFilterAndSelectTop.Click += new System.EventHandler(this.toolStripMenuItemClearFilterAndSelectTop_Click);
            // 
            // clipToolStripMenuItem
            // 
            this.clipToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pasteENTERToolStripMenuItem,
            this.pasteTextCTRLENTERToolStripMenuItem,
            this.toolStripMenuItemPasteChars,
            this.toolStripMenuItem8,
            this.changeClipTitleToolStripMenuItem,
            this.nextMatchToolStripMenuItem,
            this.previousMatchToolStripMenuItem,
            this.textFormattingToolStripMenuItem,
            this.wordWrapToolStripMenuItem,
            this.monospacedFontToolStripMenuItem,
            this.setFavouriteToolStripMenuItem,
            this.resetFavouriteToolStripMenuItem,
            this.editClipTextToolStripMenuItem,
            this.toolStripMenuItem16,
            this.openWithToolStripMenuItem,
            this.mergeTextOfClipsToolStripMenuItem,
            this.textCompareToolStripMenuItem,
            this.translateToolStripMenuItem,
            this.ignoreApplicationInCaptureToolStripMenuItem,
            this.uploadImageToWebToolStripMenuItem});
            this.clipToolStripMenuItem.Name = "clipToolStripMenuItem";
            resources.ApplyResources(this.clipToolStripMenuItem, "clipToolStripMenuItem");
            // 
            // pasteENTERToolStripMenuItem
            // 
            this.pasteENTERToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.Paste;
            this.pasteENTERToolStripMenuItem.Name = "pasteENTERToolStripMenuItem";
            resources.ApplyResources(this.pasteENTERToolStripMenuItem, "pasteENTERToolStripMenuItem");
            this.pasteENTERToolStripMenuItem.Click += new System.EventHandler(this.pasteOriginalToolStripMenuItem_Click);
            // 
            // pasteTextCTRLENTERToolStripMenuItem
            // 
            this.pasteTextCTRLENTERToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.Paste;
            this.pasteTextCTRLENTERToolStripMenuItem.Name = "pasteTextCTRLENTERToolStripMenuItem";
            resources.ApplyResources(this.pasteTextCTRLENTERToolStripMenuItem, "pasteTextCTRLENTERToolStripMenuItem");
            this.pasteTextCTRLENTERToolStripMenuItem.Click += new System.EventHandler(this.pasteAsTextToolStripMenuItem_Click);
            // 
            // toolStripMenuItemPasteChars
            // 
            this.toolStripMenuItemPasteChars.Image = global::ClipAngel.Properties.Resources.keyboard;
            this.toolStripMenuItemPasteChars.Name = "toolStripMenuItemPasteChars";
            resources.ApplyResources(this.toolStripMenuItemPasteChars, "toolStripMenuItemPasteChars");
            this.toolStripMenuItemPasteChars.Click += new System.EventHandler(this.toolStripMenuItemPasteChars_Click);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.Image = global::ClipAngel.Properties.Resources.delete;
            resources.ApplyResources(this.toolStripMenuItem8, "toolStripMenuItem8");
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            // 
            // changeClipTitleToolStripMenuItem
            // 
            this.changeClipTitleToolStripMenuItem.Name = "changeClipTitleToolStripMenuItem";
            resources.ApplyResources(this.changeClipTitleToolStripMenuItem, "changeClipTitleToolStripMenuItem");
            this.changeClipTitleToolStripMenuItem.Click += new System.EventHandler(this.changeClipTitleToolStripMenuItem_Click);
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
            // textFormattingToolStripMenuItem
            // 
            this.textFormattingToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.TextFormatting;
            this.textFormattingToolStripMenuItem.Name = "textFormattingToolStripMenuItem";
            resources.ApplyResources(this.textFormattingToolStripMenuItem, "textFormattingToolStripMenuItem");
            this.textFormattingToolStripMenuItem.Click += new System.EventHandler(this.toolStripButtonTextFormatting_Click);
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
            // setFavouriteToolStripMenuItem
            // 
            this.setFavouriteToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.MarkFavorite;
            this.setFavouriteToolStripMenuItem.Name = "setFavouriteToolStripMenuItem";
            resources.ApplyResources(this.setFavouriteToolStripMenuItem, "setFavouriteToolStripMenuItem");
            this.setFavouriteToolStripMenuItem.Click += new System.EventHandler(this.setFavoriteToolStripMenuItem_Click);
            // 
            // resetFavouriteToolStripMenuItem
            // 
            this.resetFavouriteToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.UnmarkFavorite;
            this.resetFavouriteToolStripMenuItem.Name = "resetFavouriteToolStripMenuItem";
            resources.ApplyResources(this.resetFavouriteToolStripMenuItem, "resetFavouriteToolStripMenuItem");
            this.resetFavouriteToolStripMenuItem.Click += new System.EventHandler(this.resetFavoriteToolStripMenuItem_Click);
            // 
            // editClipTextToolStripMenuItem
            // 
            this.editClipTextToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.Edit;
            this.editClipTextToolStripMenuItem.Name = "editClipTextToolStripMenuItem";
            resources.ApplyResources(this.editClipTextToolStripMenuItem, "editClipTextToolStripMenuItem");
            this.editClipTextToolStripMenuItem.Click += new System.EventHandler(this.editClipTextToolStripMenuItem_Click);
            // 
            // toolStripMenuItem16
            // 
            this.toolStripMenuItem16.Image = global::ClipAngel.Properties.Resources.OpenFile;
            this.toolStripMenuItem16.Name = "toolStripMenuItem16";
            resources.ApplyResources(this.toolStripMenuItem16, "toolStripMenuItem16");
            this.toolStripMenuItem16.Click += new System.EventHandler(this.openInDefaultApplicationToolStripMenuItem_Click);
            // 
            // openWithToolStripMenuItem
            // 
            this.openWithToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.Open_with;
            this.openWithToolStripMenuItem.Name = "openWithToolStripMenuItem";
            resources.ApplyResources(this.openWithToolStripMenuItem, "openWithToolStripMenuItem");
            this.openWithToolStripMenuItem.Click += new System.EventHandler(this.openWithToolStripMenuItem_Click);
            // 
            // mergeTextOfClipsToolStripMenuItem
            // 
            this.mergeTextOfClipsToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.merge;
            this.mergeTextOfClipsToolStripMenuItem.Name = "mergeTextOfClipsToolStripMenuItem";
            resources.ApplyResources(this.mergeTextOfClipsToolStripMenuItem, "mergeTextOfClipsToolStripMenuItem");
            // 
            // textCompareToolStripMenuItem
            // 
            this.textCompareToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.Compare;
            this.textCompareToolStripMenuItem.Name = "textCompareToolStripMenuItem";
            resources.ApplyResources(this.textCompareToolStripMenuItem, "textCompareToolStripMenuItem");
            this.textCompareToolStripMenuItem.Click += new System.EventHandler(this.textCompareToolStripMenuItem_Click);
            // 
            // translateToolStripMenuItem
            // 
            this.translateToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.translate;
            this.translateToolStripMenuItem.Name = "translateToolStripMenuItem";
            resources.ApplyResources(this.translateToolStripMenuItem, "translateToolStripMenuItem");
            this.translateToolStripMenuItem.Click += new System.EventHandler(this.translateToolStripMenuItem_Click);
            // 
            // ignoreApplicationInCaptureToolStripMenuItem
            // 
            this.ignoreApplicationInCaptureToolStripMenuItem.Name = "ignoreApplicationInCaptureToolStripMenuItem";
            resources.ApplyResources(this.ignoreApplicationInCaptureToolStripMenuItem, "ignoreApplicationInCaptureToolStripMenuItem");
            this.ignoreApplicationInCaptureToolStripMenuItem.Click += new System.EventHandler(this.ignoreApplicationInCaptureToolStripMenuItem_Click);
            // 
            // uploadImageToWebToolStripMenuItem
            // 
            this.uploadImageToWebToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.upload;
            this.uploadImageToWebToolStripMenuItem.Name = "uploadImageToWebToolStripMenuItem";
            resources.ApplyResources(this.uploadImageToWebToolStripMenuItem, "uploadImageToWebToolStripMenuItem");
            this.uploadImageToWebToolStripMenuItem.Click += new System.EventHandler(this.uploadImageToWebToolStripMenuItem_Click);
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
            this.supportToolStripMenuItem,
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
            // supportToolStripMenuItem
            // 
            this.supportToolStripMenuItem.Name = "supportToolStripMenuItem";
            resources.ApplyResources(this.supportToolStripMenuItem, "supportToolStripMenuItem");
            this.supportToolStripMenuItem.Click += new System.EventHandler(this.supportToolStripMenuItem_Click);
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
            this.openFavoritesToolStripMenuItem,
            this.toolStripMenuItemCompareLastClips,
            this.trayMenuItemMonitoringClipboard,
            this.toolStripMenuItem9,
            this.exitToolStripMenuItem1});
            this.contextMenuStripNotifyIcon.Name = "contextMenuStripNotifyIcon";
            resources.ApplyResources(this.contextMenuStripNotifyIcon, "contextMenuStripNotifyIcon");
            // 
            // openFavoritesToolStripMenuItem
            // 
            this.openFavoritesToolStripMenuItem.Image = global::ClipAngel.Properties.Resources.MarkFavorite;
            this.openFavoritesToolStripMenuItem.Name = "openFavoritesToolStripMenuItem";
            resources.ApplyResources(this.openFavoritesToolStripMenuItem, "openFavoritesToolStripMenuItem");
            this.openFavoritesToolStripMenuItem.Click += new System.EventHandler(this.openFavoritesToolStripMenuItem_Click);
            // 
            // toolStripMenuItemCompareLastClips
            // 
            this.toolStripMenuItemCompareLastClips.Image = global::ClipAngel.Properties.Resources.Compare;
            this.toolStripMenuItemCompareLastClips.Name = "toolStripMenuItemCompareLastClips";
            resources.ApplyResources(this.toolStripMenuItemCompareLastClips, "toolStripMenuItemCompareLastClips");
            this.toolStripMenuItemCompareLastClips.Click += new System.EventHandler(this.toolStripMenuItemCompareLastClips_Click);
            // 
            // trayMenuItemMonitoringClipboard
            // 
            this.trayMenuItemMonitoringClipboard.Image = global::ClipAngel.Properties.Resources.eye;
            this.trayMenuItemMonitoringClipboard.Name = "trayMenuItemMonitoringClipboard";
            resources.ApplyResources(this.trayMenuItemMonitoringClipboard, "trayMenuItemMonitoringClipboard");
            this.trayMenuItemMonitoringClipboard.Click += new System.EventHandler(this.MonitoringClipboardToolStripMenuItem_Click);
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Image = global::ClipAngel.Properties.Resources.clear2;
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            resources.ApplyResources(this.toolStripMenuItem9, "toolStripMenuItem9");
            this.toolStripMenuItem9.Click += new System.EventHandler(this.clearClipboardToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem1
            // 
            this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            resources.ApplyResources(this.exitToolStripMenuItem1, "exitToolStripMenuItem1");
            this.exitToolStripMenuItem1.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
            // 
            // toolStripTop
            // 
            resources.ApplyResources(this.toolStripTop, "toolStripTop");
            this.toolStripTop.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonTextFormatting,
            this.toolStripButtonWordWrap,
            this.toolStripButtonMonospacedFont,
            this.moveCopiedClipToTopToolStripButton,
            this.toolStripButtonReturnToPrevous,
            this.toolStripButtonClearFilterAndSelectTop,
            this.toolStripButtonMarkFavorite,
            this.toolStripMenuItemEditClipText,
            this.toolStripMenuItemOpenFile,
            this.toolStripButtonOpenWith,
            this.toolStripButtonTopMostWindow,
            this.toolStripUpdateToSeparator,
            this.buttonUpdate});
            this.toolStripTop.Name = "toolStripTop";
            // 
            // toolStripButtonTextFormatting
            // 
            this.toolStripButtonTextFormatting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonTextFormatting.DoubleClickEnabled = true;
            this.toolStripButtonTextFormatting.Image = global::ClipAngel.Properties.Resources.TextFormatting;
            resources.ApplyResources(this.toolStripButtonTextFormatting, "toolStripButtonTextFormatting");
            this.toolStripButtonTextFormatting.Name = "toolStripButtonTextFormatting";
            this.toolStripButtonTextFormatting.Click += new System.EventHandler(this.toolStripButtonTextFormatting_Click);
            // 
            // toolStripButtonWordWrap
            // 
            this.toolStripButtonWordWrap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonWordWrap.Image = global::ClipAngel.Properties.Resources.WordWrap;
            resources.ApplyResources(this.toolStripButtonWordWrap, "toolStripButtonWordWrap");
            this.toolStripButtonWordWrap.Name = "toolStripButtonWordWrap";
            this.toolStripButtonWordWrap.Click += new System.EventHandler(this.wordWrapToolStripMenuItem_Click);
            // 
            // toolStripButtonMonospacedFont
            // 
            this.toolStripButtonMonospacedFont.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonMonospacedFont.Image = global::ClipAngel.Properties.Resources.FixedWidthFont;
            resources.ApplyResources(this.toolStripButtonMonospacedFont, "toolStripButtonMonospacedFont");
            this.toolStripButtonMonospacedFont.Name = "toolStripButtonMonospacedFont";
            this.toolStripButtonMonospacedFont.Click += new System.EventHandler(this.toolStripButtonFixedWidthFont_Click);
            // 
            // moveCopiedClipToTopToolStripButton
            // 
            this.moveCopiedClipToTopToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.moveCopiedClipToTopToolStripButton.Image = global::ClipAngel.Properties.Resources.MoveCopiedClipToTop;
            resources.ApplyResources(this.moveCopiedClipToTopToolStripButton, "moveCopiedClipToTopToolStripButton");
            this.moveCopiedClipToTopToolStripButton.Name = "moveCopiedClipToTopToolStripButton";
            this.moveCopiedClipToTopToolStripButton.Click += new System.EventHandler(this.moveCopiedClipToTopToolStripButton_Click);
            // 
            // toolStripButtonReturnToPrevous
            // 
            this.toolStripButtonReturnToPrevous.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonReturnToPrevous.Image = global::ClipAngel.Properties.Resources.Back;
            resources.ApplyResources(this.toolStripButtonReturnToPrevous, "toolStripButtonReturnToPrevous");
            this.toolStripButtonReturnToPrevous.Name = "toolStripButtonReturnToPrevous";
            this.toolStripButtonReturnToPrevous.Click += new System.EventHandler(this.returnToPrevousSelectedClipToolStripMenuItem_Click);
            // 
            // toolStripButtonClearFilterAndSelectTop
            // 
            this.toolStripButtonClearFilterAndSelectTop.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripButtonClearFilterAndSelectTop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonClearFilterAndSelectTop.Image = global::ClipAngel.Properties.Resources.Top2;
            resources.ApplyResources(this.toolStripButtonClearFilterAndSelectTop, "toolStripButtonClearFilterAndSelectTop");
            this.toolStripButtonClearFilterAndSelectTop.Name = "toolStripButtonClearFilterAndSelectTop";
            this.toolStripButtonClearFilterAndSelectTop.Click += new System.EventHandler(this.toolStripMenuItemClearFilterAndSelectTop_Click);
            // 
            // toolStripButtonMarkFavorite
            // 
            this.toolStripButtonMarkFavorite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonMarkFavorite.Image = global::ClipAngel.Properties.Resources.MarkFavorite;
            resources.ApplyResources(this.toolStripButtonMarkFavorite, "toolStripButtonMarkFavorite");
            this.toolStripButtonMarkFavorite.Name = "toolStripButtonMarkFavorite";
            this.toolStripButtonMarkFavorite.Click += new System.EventHandler(this.toolStripButtonMarkFavorite_Click);
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
            // toolStripButtonOpenWith
            // 
            this.toolStripButtonOpenWith.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonOpenWith.Image = global::ClipAngel.Properties.Resources.Open_with;
            resources.ApplyResources(this.toolStripButtonOpenWith, "toolStripButtonOpenWith");
            this.toolStripButtonOpenWith.Name = "toolStripButtonOpenWith";
            this.toolStripButtonOpenWith.Click += new System.EventHandler(this.openWithToolStripMenuItem_Click);
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
            // cultureManager1
            // 
            this.cultureManager1.ManagedControl = this;
            // 
            // timerDaily
            // 
            this.timerDaily.Tick += new System.EventHandler(this.timerDaily_Tick);
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
            resources.ApplyResources(this.dataGridViewImageColumn1, "dataGridViewImageColumn1");
            this.dataGridViewImageColumn1.Name = "dataGridViewImageColumn1";
            this.dataGridViewImageColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // dataGridViewImageColumn2
            // 
            resources.ApplyResources(this.dataGridViewImageColumn2, "dataGridViewImageColumn2");
            this.dataGridViewImageColumn2.Name = "dataGridViewImageColumn2";
            this.dataGridViewImageColumn2.ReadOnly = true;
            this.dataGridViewImageColumn2.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // dataGridViewRichTextBoxColumn1
            // 
            this.dataGridViewRichTextBoxColumn1.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.dataGridViewRichTextBoxColumn1, "dataGridViewRichTextBoxColumn1");
            this.dataGridViewRichTextBoxColumn1.Name = "dataGridViewRichTextBoxColumn1";
            this.dataGridViewRichTextBoxColumn1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // dataGridViewImageColumn3
            // 
            resources.ApplyResources(this.dataGridViewImageColumn3, "dataGridViewImageColumn3");
            this.dataGridViewImageColumn3.Name = "dataGridViewImageColumn3";
            this.dataGridViewImageColumn3.ReadOnly = true;
            // 
            // dataGridViewImageColumn4
            // 
            this.dataGridViewImageColumn4.DataPropertyName = "ImageSample";
            resources.ApplyResources(this.dataGridViewImageColumn4, "dataGridViewImageColumn4");
            this.dataGridViewImageColumn4.Name = "dataGridViewImageColumn4";
            this.dataGridViewImageColumn4.ReadOnly = true;
            // 
            // dataGridViewRichTextBoxColumn2
            // 
            resources.ApplyResources(this.dataGridViewRichTextBoxColumn2, "dataGridViewRichTextBoxColumn2");
            this.dataGridViewRichTextBoxColumn2.Name = "dataGridViewRichTextBoxColumn2";
            this.dataGridViewRichTextBoxColumn2.ReadOnly = true;
            // 
            // dataGridViewImageColumn5
            // 
            resources.ApplyResources(this.dataGridViewImageColumn5, "dataGridViewImageColumn5");
            this.dataGridViewImageColumn5.Name = "dataGridViewImageColumn5";
            this.dataGridViewImageColumn5.ReadOnly = true;
            // 
            // tooltipTimer
            // 
            this.tooltipTimer.Interval = 50;
            this.tooltipTimer.Tick += new System.EventHandler(this.tooltipTimer_Tick);
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
            // Main
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStripTop);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.MainMenu);
            this.KeyPreview = true;
            this.MainMenuStrip = this.MainMenu;
            this.Name = "Main";
            this.Activated += new System.EventHandler(this.Main_Activated);
            this.Deactivate += new System.EventHandler(this.Main_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Main_FormClosed);
            this.Load += new System.EventHandler(this.Main_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Main_KeyDown);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.toolStripBottom.ResumeLayout(false);
            this.toolStripBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.contextMenuStripDataGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.clipBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSource)).EndInit();
            this.tableLayoutPanelData.ResumeLayout(false);
            this.contextMenuUrl.ResumeLayout(false);
            this.contextMenuStripRtf.ResumeLayout(false);
            this.contextMenuStripHtml.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ImageControl)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.contextMenuStripApplication.ResumeLayout(false);
            this.contextMenuWindowTitle.ResumeLayout(false);
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.contextMenuStripNotifyIcon.ResumeLayout(false);
            this.toolStripTop.ResumeLayout(false);
            this.toolStripTop.PerformLayout();
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
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
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
        private System.Windows.Forms.ToolTip toolTipDynamic;
        private System.Windows.Forms.ComboBox TypeFilter;
        private System.Windows.Forms.ToolStripMenuItem nextMatchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem previousMatchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wordWrapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStripTop;
        private System.Windows.Forms.ToolStripButton toolStripButtonWordWrap;
        private System.Windows.Forms.ToolStripMenuItem fToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.ToolStripButton toolStripButtonClearFilterAndSelectTop;
        private System.Windows.Forms.ToolStripMenuItem changeClipTitleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem listToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemClearFilterAndSelectTop;
        private System.Windows.Forms.ToolStripMenuItem setFavouriteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetFavouriteToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonMarkFavorite;
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
        private System.Windows.Forms.Timer timerDaily;
        private System.Windows.Forms.ToolStripSeparator toolStripUpdateToSeparator;
        private System.Windows.Forms.ToolStripMenuItem historyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemPasteChars;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem7;
        private System.Windows.Forms.ToolStripMenuItem openWithToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowAlwaysOnTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonTopMostWindow;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemOpenFile;
        private System.Windows.Forms.ToolStripMenuItem editClipTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripMenuItemEditClipText;
        private System.Windows.Forms.Timer timerReconnect;
        private System.Windows.Forms.Timer timerApplyTextFiler;
        private System.Windows.Forms.ToolStripButton toolStripButtonMonospacedFont;
        private System.Windows.Forms.ToolStripMenuItem monospacedFontToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trayMenuItemMonitoringClipboard;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMonitoringClipboard;
        private System.Windows.Forms.ToolStripMenuItem translateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textCompareToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButtonTextFormatting;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem8;
        private System.Windows.Forms.ToolStripMenuItem textFormattingToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBoxSource;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelData;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.PictureBox ImageControl;
        private System.Windows.Forms.WebBrowser htmlTextBox;
        private System.Windows.Forms.RichTextBox urlTextBox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripHtml;
        private System.Windows.Forms.ToolStripMenuItem htmlMenuItemCopyLinkAdress;
        private System.Windows.Forms.ToolStripMenuItem htmlMenuItemSelectAll;
        private System.Windows.Forms.ToolStripMenuItem htmlMenuItemOpenLink;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn2;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn3;
        private DataGridViewRichTextBoxColumn dataGridViewRichTextBoxColumn2;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn4;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn5;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRtf;
        private System.Windows.Forms.ToolStripMenuItem rtfMenuItemOpenLink;
        private System.Windows.Forms.ToolStripMenuItem rtfMenuItemSelectAll;
        private System.Windows.Forms.DataGridViewImageColumn dataGridViewImageColumn1;
        private DataGridViewRichTextBoxColumn dataGridViewRichTextBoxColumn1;
        private System.Windows.Forms.ToolStripMenuItem showInTaskbarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem сдуфкClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem9;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem12;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem11;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem10;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFavoritesToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStripBottom;
        private System.Windows.Forms.ToolStripButton toolStripButtonFindPrevious;
        private System.Windows.Forms.ToolStripButton toolStripButtonFindNext;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem13;
        private System.Windows.Forms.ToolStripMenuItem showOnlyImagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showOnlyFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showOnlyTextsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showAllTypesToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton moveCopiedClipToTopToolStripButton;
        private System.Windows.Forms.ToolStripMenuItem moveCopiedClipToTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem14;
        private System.Windows.Forms.ToolStripMenuItem ignoreApplicationInCaptureToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripApplication;
        private System.Windows.Forms.ToolStripMenuItem copyFullFilenameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSwitchFocus;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSetFocusClipText;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem6;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem15;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemCompareLastClips;
        private System.Windows.Forms.ToolStripMenuItem deleteAllNonFavoriteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem16;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpenWith;
        private System.Windows.Forms.ToolStripMenuItem uploadImageToWebToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuUrl;
        private System.Windows.Forms.ToolStripMenuItem toolStripUrlCopyAll;
        private System.Windows.Forms.ContextMenuStrip contextMenuWindowTitle;
        private System.Windows.Forms.ToolStripMenuItem toolStripWindowTitleCopyAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripApplicationCopyAll;
        private System.Windows.Forms.ToolStripMenuItem supportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem returnToPrevousSelectedClipToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem17;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemClearFilters;
        private System.Windows.Forms.ToolStripButton toolStripButtonClearFilter;
        private System.Windows.Forms.ToolStripMenuItem mergeTextOfClipsToolStripMenuItem;
        private CueComboBox comboBoxFilter;
        private System.Windows.Forms.ToolStripButton toolStripButtonReturnToPrevous;
        private System.Windows.Forms.DataGridViewImageColumn AppImage;
        private System.Windows.Forms.DataGridViewImageColumn TypeImage;
        private System.Windows.Forms.DataGridViewTextBoxColumn TitleSimple;
        private DataGridViewRichTextBoxColumn ColumnTitle;
        private System.Windows.Forms.DataGridViewImageColumn ImageSample;
        private System.Windows.Forms.DataGridViewTextBoxColumn VisualWeight;
        private System.Windows.Forms.DataGridViewImageColumn imageSampleDataGridViewImageColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn charsDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SizeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn CreatedDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn usedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn typeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn titleDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn idDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn favoriteDataGridViewCheckBoxColumn;
        private System.Windows.Forms.Timer tooltipTimer;
        private System.Windows.Forms.ToolStripDropDownButton toolStripFindSettings;
        private System.Windows.Forms.ToolStripMenuItem toolStripButtonAutoSelectMatch;
        private System.Windows.Forms.ToolStripMenuItem caseSensetiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem everyWordIndependentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem meandsAnySequenceOfCharsToolStripMenuItem;
    }
}

