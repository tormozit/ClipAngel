using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Resources;
using System.Net;
using AngleSharp.Parser.Html;
using AngleSharp.Dom;
using System.Reflection;
using System.Runtime.CompilerServices;
using WindowsInput.Native;
using mshtml;
//using Word = Microsoft.Office.Interop.Word;
using static IconTools;

namespace ClipAngel
{
    enum PasteMethod {Standart, PasteText, SendChars };
    public partial class Main : Form
    {
        public const string IsMainPropName = "IsMain";
        public ResourceManager CurrentLangResourceManager;
        public string Locale = "";
        public bool PortableMode = false;
        public int ClipsNumber = 0;
        public string UserSettingsPath;
        public string DbFileName;
        SQLiteConnection m_dbConnection;
        public string ConnectionString;
        SQLiteDataAdapter dataAdapter;
        //bool CaptureClipboard = true;
        bool allowRowLoad = true;
        //bool AutoGotoLastRow = true;
        bool AllowHotkeyProcess = true;
        bool EditMode = false;
        SQLiteDataReader RowReader;
        static string LinkPattern = "\\b(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[A-Z0-9+&@#/%=~_|]";
        int LastId = 0;
        MatchCollection TextLinkMatches;
        MatchCollection UrlLinkMatches;
        MatchCollection FilterMatches;
        string DataFormat_ClipboardViewerIgnore = "Clipboard Viewer Ignore";
        string ActualVersion;
        //DateTime lastAutorunUpdateCheck;
        int MaxTextViewSize = 5000;
        bool TextWasCut;
        KeyboardHook keyboardHook;
        WinEventDelegate dele = null;
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;
        private IntPtr lastActiveWindow;
        private IntPtr HookChangeActiveWindow;
        private bool AllowFilterProcessing = true;
        private static Color favoriteColor = Color.FromArgb(255, 220, 220);
        private static Color _usedColor = Color.FromArgb(200, 255, 255);
        Bitmap imageText;
        Bitmap imageHtml;
        Bitmap imageRtf;
        Bitmap imageFile;
        Bitmap imageImg;
        string filterText = ""; // To optimize speed
        int tabLength = 4;
        readonly RichTextBox _richTextBox = new RichTextBox();
        Dictionary<string, Bitmap> origibalIconCache = new Dictionary<string, Bitmap>();
        Dictionary<string, Bitmap> brightIconCache = new Dictionary<string, Bitmap>();
        private bool allowTextPositionChangeUpdate = false;
        private bool MonitoringClipboard = true;
        private int _lastSelectedForCompareId;
        const int ClipTitleLength = 70;
        int factualTop = 0;
        bool htmlMode = false;
        int selectionLength = 0;
        int selectionStart = 0;
        bool htmlInitialized = false;
        private int clipRichTextLength = 0; // To always know where ends real text (location of end marker)
        bool filterOn = false;
        private HtmlElement lastClickedHtmlElement;
        private bool allowVisible = false;

        [DllImport("dwmapi", PreserveSig = true)]
        static extern int DwmSetWindowAttribute(IntPtr hWnd, int attr, ref int value, int attrLen);

        const int WS_EX_NOACTIVATE = 0x08000000;
        //const int WS_EX_TOPMOST = 0x00000008;
        const int WS_EX_COMPOSITED = 0x02000000;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams param = base.CreateParams;
                //param.ExStyle |= WS_EX_TOPMOST; // make the form topmost
                param.ExStyle |= WS_EX_NOACTIVATE; // prevent the form from being activated
                //param.ExStyle |= WS_EX_COMPOSITED;  // Turn on WS_EX_COMPOSITED
                return param;
            }
        }

        public Main(string UserSettingsPath, bool PortableMode, bool StartMinimized)
        {
            this.UserSettingsPath = UserSettingsPath;
            this.PortableMode = PortableMode;
            //// Disable window animation on minimize and restore. Failed
            //const int DWMWA_TRANSITIONS_FORCEDISABLED = 3;
            //int value = 1;  // TRUE to disable
            //DwmSetWindowAttribute(this.Handle, DWMWA_TRANSITIONS_FORCEDISABLED, ref value, 4);
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            UpdateCurrentCulture(); // Antibug. Before bug it was not required
            InitializeComponent();
            dele = new WinEventDelegate(WinEventProc);
            HookChangeActiveWindow = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);
            // register the event that is fired after the key press.
            keyboardHook = new KeyboardHook(CurrentLangResourceManager);
            keyboardHook.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            RegisterHotKeys();

            ResourceManager resourceManager = Properties.Resources.ResourceManager;
            imageText = resourceManager.GetObject("TypeText") as Bitmap;
            imageHtml = resourceManager.GetObject("TypeHtml") as Bitmap;
            imageRtf = resourceManager.GetObject("TypeRtf") as Bitmap;
            imageFile = resourceManager.GetObject("TypeFile") as Bitmap;
            imageImg = resourceManager.GetObject("TypeImg") as Bitmap;

            htmlTextBox.Navigate("about:blank");
            htmlTextBox.Document.ExecCommand("EditMode", false, null);
            //Properties.Settings.Default.FastWindowOpen = false; // for debug

            // Antiflicker double buffering
            // http://stackoverflow.com/questions/76993/how-to-double-buffer-net-controls-on-a-form
            System.Reflection.PropertyInfo aProp = typeof(System.Windows.Forms.Control)
            .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aProp.SetValue(dataGridView, true, null);
            //aProp.SetValue(richTextBox, true, null); // No effect
            //aProp.SetValue(htmlTextBox, true, null); // No effect

            BindingList<ListItemNameText> _comboItemsTypes = new BindingList<ListItemNameText>
            {
                new ListItemNameText {Name = "allTypes"},
                new ListItemNameText {Name = "text"},
                new ListItemNameText {Name = "file"},
                new ListItemNameText {Name = "img"}
            };
            TypeFilter.DataSource = _comboItemsTypes;
            TypeFilter.DisplayMember = "Text";
            TypeFilter.ValueMember = "Name";
            //MarkFilter.SelectedValue = "allTypes";
            MarkFilter.SelectedIndex = 0;

            BindingList<ListItemNameText> _comboItemsMarks = new BindingList<ListItemNameText>();
            _comboItemsMarks.Add(new ListItemNameText { Name = "allMarks" });
            _comboItemsMarks.Add(new ListItemNameText { Name = "used" });
            _comboItemsMarks.Add(new ListItemNameText { Name = "favorite" });
            MarkFilter.DataSource = _comboItemsMarks;
            MarkFilter.DisplayMember = "Text";
            MarkFilter.ValueMember = "Name";
            //MarkFilter.SelectedValue = "allMarks";
            TypeFilter.SelectedIndex = 0;

            (dataGridView.Columns["AppImage"] as DataGridViewImageColumn).DefaultCellStyle.NullValue = null;
            richTextBox.AutoWordSelection = false;
            urlTextBox.AutoWordSelection = false;
            if (Properties.Settings.Default.LastFilterValues == null)
            {
                Properties.Settings.Default.LastFilterValues = new StringCollection();
            }
            FillFilterItems();

            if (!Directory.Exists(UserSettingsPath))
            {
                Directory.CreateDirectory(UserSettingsPath);
            }
            DbFileName = UserSettingsPath + "\\" + Properties.Resources.DBShortFilename;
            ConnectionString = "data source=" + DbFileName + ";";
            string Reputation = "Magic67234784";
            if (!File.Exists(DbFileName))
            {
                File.WriteAllBytes(DbFileName, Properties.Resources.dbTemplate);
                m_dbConnection = new SQLiteConnection(ConnectionString);
                m_dbConnection.Open();
                // Encryption http://stackoverflow.com/questions/12190672/can-i-password-encrypt-sqlite-database
                m_dbConnection.ChangePassword(Reputation);
                m_dbConnection.Close();
            }
            ConnectionString += "Password = " + Reputation + ";";
            m_dbConnection = new SQLiteConnection(ConnectionString);
            m_dbConnection.Open();
            SQLiteCommand command;

            command = new SQLiteCommand("ALTER TABLE Clips" + " ADD COLUMN Hash CHAR(32)", m_dbConnection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch
            { }
            command = new SQLiteCommand("ALTER TABLE Clips" + " ADD COLUMN Favorite BOOLEAN", m_dbConnection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch
            { }
            command = new SQLiteCommand("ALTER TABLE Clips" + " ADD COLUMN ImageSample BINARY", m_dbConnection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch
            { }
            command = new SQLiteCommand("ALTER TABLE Clips" + " ADD COLUMN AppPath CHAR(256)", m_dbConnection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch
            { }
            command = new SQLiteCommand("CREATE unique index unique_hash on Clips(hash)", m_dbConnection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            // http://blog.tigrangasparian.com/2012/02/09/getting-started-with-sqlite-in-c-part-one/
            //sql = "CREATE TABLE Clips (Title VARCHAR(50), Text VARCHAR(0), Data BLOB, Size INT, Type VARCHAR(10), Created DATETIME, Application VARCHAR(50), Window VARCHAR(100))";
            //command = new SQLiteCommand(sql, m_dbConnection);
            //try
            //{
            //    command.ExecuteNonQuery();
            //}
            //catch { };

            // https://msdn.microsoft.com/ru-ru/library/fbk67b6z(v=vs.110).aspx
            dataAdapter = new SQLiteDataAdapter("", ConnectionString);
            //dataGridView.DataSource = clipBindingSource;
            UpdateClipBindingSource();
            ConnectClipboard();
            if (StartMinimized)
            {
                //StartMinimized = false;
                //Close();
            }
            else
            {
                //UpdateControlsStates(); //
                //RestoreWindowIfMinimized();
                allowVisible = true;
            }
            if (Properties.Settings.Default.WindowSize.Width > 0)
                this.Size = Properties.Settings.Default.WindowSize;
            timerCheckUpdate.Interval = 1;
            timerCheckUpdate.Start();
            timerReconnect.Interval = (1000 * 5); // 5 seconds
            timerReconnect.Start();
            this.ActiveControl = dataGridView;
            ResetIsMainProperty();

            LoadSettings();
        }

        private void ResetIsMainProperty()
        {
            SetProp(this.Handle, IsMainPropName, new IntPtr(1));
        }

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            int targetProcessId;
            uint remoteThreadId = GetWindowThreadProcessId(hwnd, out targetProcessId);
            if (targetProcessId != Process.GetCurrentProcess().Id)
            {
                lastActiveWindow = hwnd;
                UpdateWindowTitle();
            }
        }

        private void UpdateWindowTitle(bool forced = false)
        {
            if (this.Top < 0 && !forced)
                return;
            string windowText = "";
            if (lastActiveWindow != null)
                windowText = GetWindowTitle(lastActiveWindow);
            Debug.WriteLine("Active window " + lastActiveWindow + " " + windowText);
            this.Text = Application.ProductName + " " + Properties.Resources.Version + " >> " + windowText;
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
        private void RegisterHotKeys()
        {
            EnumModifierKeys Modifiers;
            Keys Key;
            if (ReadHotkeyFromText(Properties.Settings.Default.GlobalHotkeyOpen, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(Properties.Settings.Default.GlobalHotkeyOpenFavorites, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(Properties.Settings.Default.GlobalHotkeyIncrementalPaste, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(Properties.Settings.Default.GlobalHotkeyCompareLastClips, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText("Control + F3", out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
        }

        private static bool ReadHotkeyFromText(string HotkeyText, out EnumModifierKeys Modifiers, out Keys Key)
        {
            Modifiers = 0;
            Key = 0;
            if (HotkeyText == "" || HotkeyText == "No")
                return false;
            string[] Frags = HotkeyText.Split(new[] { "+" }, StringSplitOptions.None);
            for (int i = 0; i < Frags.Length - 1; i++)
            {
                EnumModifierKeys Modifier = 0;
                Enum.TryParse(Frags[i].Trim(), true, out Modifier);
                Modifiers |= Modifier;
            }
            Enum.TryParse(Frags[Frags.Length - 1], out Key);
            return true;
        }

        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (!AllowHotkeyProcess)
                return;
            string hotkeyTitle = KeyboardHook.HotkeyTitle(e.Key, e.Modifier);
            if (hotkeyTitle == Properties.Settings.Default.GlobalHotkeyOpen)
            {
                if (this.ContainsFocus && this.Top >= 0 && MarkFilter.SelectedValue.ToString() != "favorite")
                    this.Close();
                else
                {
                    ShowForPaste();
                    dataGridView.Focus();
                }
            }
            else if (hotkeyTitle == Properties.Settings.Default.GlobalHotkeyOpenFavorites)
            {
                if (this.ContainsFocus && this.Top >= 0 && MarkFilter.SelectedValue.ToString() == "favorite")
                    this.Close();
                else
                {
                    ShowForPaste(true);
                    dataGridView.Focus();
                }
            }
            else if (hotkeyTitle == Properties.Settings.Default.GlobalHotkeyIncrementalPaste)
            {
                AllowHotkeyProcess = false;
                SendPasteClip();
                if ((e.Modifier & EnumModifierKeys.Alt) != 0)
                    keybd_event((byte)VirtualKeyCode.MENU, 0x38, 0, 0); // LEFT
                if ((e.Modifier & EnumModifierKeys.Control) != 0)
                    keybd_event((byte)VirtualKeyCode.CONTROL, 0x1D, 0, 0);
                if ((e.Modifier & EnumModifierKeys.Shift) != 0)
                    keybd_event((byte)VirtualKeyCode.SHIFT, 0x2A, 0, 0);
                clipBindingSource.MoveNext();
                DataRow CurrentDataRow = ((DataRowView)clipBindingSource.Current).Row;
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(2000, CurrentLangResourceManager.GetString("NextClip"), CurrentDataRow["Title"].ToString(), ToolTipIcon.Info);
                AllowHotkeyProcess = true;
            }
            else if (hotkeyTitle == Properties.Settings.Default.GlobalHotkeyCompareLastClips)
            {
                if (dataGridView.Rows.Count > 1)
                {
                    DataRowView row1 = (DataRowView)dataGridView.Rows[0].DataBoundItem;
                    int id1 = (int)row1["id"];
                    DataRowView row2 = (DataRowView)dataGridView.Rows[1].DataBoundItem;
                    int id2 = (int)row2["id"];
                    CompareClipsbyID(id1, id2);

                }
            }
            else if (hotkeyTitle == "Control + F3")
            {
                keyboardHook.UnregisterHotKeys();
                //var data = Clipboard.GetDataObject();
                Paster.SendCopy();
                SendKeys.SendWait("^{F3}");
                RegisterHotKeys();
                //Clipboard.SetDataObject(data);
            }
            else
            {
                //int a = 0;
            }
        }

        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MINIMIZE = 0xf020;
        protected override void WndProc(ref Message m)
        {
            if (Properties.Settings.Default.FastWindowOpen)
            {
                if (m.Msg == WM_SYSCOMMAND)
                {
                    if (m.WParam.ToInt32() == SC_MINIMIZE) // TODO catch MinimizeAll command if it is possible
                    {
                        m.Result = IntPtr.Zero;
                        Close();
                        return;
                    }
                }

            }
            switch ((Msgs)m.Msg)
            {
                case Msgs.WM_CLIPBOARDUPDATE:
                    Debug.WriteLine("WindowProc WM_CLIPBOARDUPDATE: " + m.Msg, "WndProc");
                    GetClipboardData();
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion


        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetProp(IntPtr hWnd, string lpString, IntPtr hData);

        private void Main_Load(object sender, EventArgs e)
        {
            // Due to the hidden start window can be shown and this event raised not on the start
            // So we do not use it and make everything in constructor
            UpdateControlsStates(); //
            RestoreWindowIfMinimized();
        }

        // To hide on start
        protected override void SetVisibleCore(bool value)
        {
            if (!allowVisible)
            {
                value = false;
                if (!this.IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        private void ConnectClipboard()
        {
            if (!MonitoringClipboard)
                return;
            if (!AddClipboardFormatListener(this.Handle))
            {
                int ErrorCode = Marshal.GetLastWin32Error();
                int ERROR_INVALID_PARAMETER = 87;
                if (ErrorCode!= ERROR_INVALID_PARAMETER)
                    Debug.WriteLine("Failed to connect clipboard: " + Marshal.GetLastWin32Error());
                else
                {
                    //already connected
                }
            }
        }

        private void AfterRowLoad(bool FullTextLoad = false, int CurrentRowIndex = -1, int NewSelectionStart = -1, int NewSelectionLength = -1)
        {
            DataRowView CurrentRowView;
            mshtml.IHTMLDocument2 htmlDoc;
            string clipType;
            FullTextLoad = FullTextLoad || EditMode;
            richTextBox.ReadOnly = !EditMode;
            if (CurrentRowIndex == -1)
            { CurrentRowView = clipBindingSource.Current as DataRowView; }
            else
            { CurrentRowView = clipBindingSource[CurrentRowIndex] as DataRowView; }
            FilterMatches = null;
            if (Properties.Settings.Default.MonospacedFont)
                richTextBox.Font = new Font(FontFamily.GenericMonospace, Properties.Settings.Default.Font.Size);
            else
                richTextBox.Font = Properties.Settings.Default.Font;
            bool useNativeTextFormatting = false;
            htmlMode = false;
            bool elementPanelHasFocus = false
                                        || ImageControl.Focused
                                        || richTextBox.Focused
                                        || htmlTextBox.Focused
                                        || urlTextBox.Focused;
            clipRichTextLength = 0;
            clipType = "";
            pictureBoxSource.Image = null;
            ImageControl.Image = null;
            richTextBox.Text = "";
            textBoxApplication.Text = "";
            textBoxWindow.Text = "";
            StripLabelCreated.Text = "";
            StripLabelSize.Text = "";
            StripLabelVisualSize.Text = "";
            StripLabelType.Text = "";
            stripLabelPosition.Text = "";
            if (CurrentRowView != null)
            {
                allowTextPositionChangeUpdate = false;
                DataRow CurrentRow = CurrentRowView.Row;
                RowReader = getRowReader((int)CurrentRow["Id"]);
                clipType = RowReader["type"].ToString();
                string fullText = RowReader["Text"].ToString();
                string fullRTF = RowReader["richText"].ToString();
                string htmlText = GetHtmlFromHtmlClipText();
                useNativeTextFormatting = true
                                               && Properties.Settings.Default.ShowNativeTextFormatting
                                               && (clipType == "html" || clipType == "rtf");
                Bitmap appIcon = ApplicationIcon(RowReader["appPath"].ToString());
                if (appIcon != null)
                {
                    pictureBoxSource.Image = appIcon;
                }
                textBoxApplication.Text = RowReader["Application"].ToString();
                textBoxWindow.Text = RowReader["Window"].ToString();
                StripLabelCreated.Text = RowReader["Created"].ToString();
                NumberFormatInfo numberFormat= new CultureInfo(Locale).NumberFormat;
                numberFormat.NumberDecimalDigits = 0;
                numberFormat.NumberGroupSeparator = " ";
                StripLabelSize.Text = ((int)RowReader["Size"]).ToString("N", numberFormat) + " " + MultiLangByteUnit();
                StripLabelVisualSize.Text = ((int)RowReader["Chars"]).ToString("N", numberFormat) + " " + MultiLangCharUnit();
                string TypeEng = RowReader["Type"].ToString();
                if (CurrentLangResourceManager.GetString(TypeEng) == null)
                    StripLabelType.Text = TypeEng;
                else
                    StripLabelType.Text = CurrentLangResourceManager.GetString(TypeEng);
                stripLabelPosition.Text = "1";
                // to prevent autoscrolling during marking
                richTextBox.HideSelection = true;
                richTextBox.Clear();
                int fontsize = (int)richTextBox.Font.Size; // Size should be without digits after comma
                richTextBox.SelectionTabs = new int[] { fontsize * 4, fontsize * 8, fontsize * 12, fontsize * 16 }; // Set tab size ~ 4
                string shortText;
                string endMarker;
                Font markerFont;
                Color markerColor;
                if (!FullTextLoad && MaxTextViewSize < fullText.Length)
                {
                    //if (useNativeTextFormatting)
                    //    shortText = fullRTF.Substring(1, MaxTextViewSize); // TODO find way correct cutting RTF
                    //else
                    shortText = fullText.Substring(1, MaxTextViewSize);
                    richTextBox.Text = shortText;
                    endMarker = MultiLangCutMarker();
                    markerFont = new Font(richTextBox.SelectionFont, FontStyle.Underline);
                    TextWasCut = true;
                    markerColor = Color.Blue;
                }
                else
                {
                    if (useNativeTextFormatting)
                    {
                        htmlMode = true
                            && TypeEng == "html"
                            && !String.IsNullOrEmpty(htmlText);
                        if (htmlMode)
                        {
                            //while (this.htmlTextBox.ReadyState != WebBrowserReadyState.Complete)
                            //{
                            //    Application.DoEvents();
                            //    Thread.Sleep(5);
                            //}
                            htmlTextBox.Parent.Enabled = false; // Prevent stealing focus 
                            htmlDoc = htmlTextBox.Document.DomDocument as mshtml.IHTMLDocument2;
                            htmlDoc.write(htmlText);
                            htmlDoc.close();

                            mshtml.IHTMLBodyElement body = htmlDoc.body as mshtml.IHTMLBodyElement;
                            htmlTextBox.Document.Body.Drag += new HtmlElementEventHandler(htmlTextBoxDrag);
                            htmlTextBox.Document.Body.KeyDown += new HtmlElementEventHandler(htmlTextBoxDocumentKeyDown);

                            // Need to be called every time, else handler will be lost
                            htmlTextBox.Document.AttachEventHandler("onselectionchange", htmlTextBoxDocumentSelectionChange); // No multi call to handler, but why?
                            if (!htmlInitialized)
                            {
                                mshtml.HTMLDocumentEvents2_Event iEvent = (mshtml.HTMLDocumentEvents2_Event)htmlDoc;
                                iEvent.onclick += new mshtml.HTMLDocumentEvents2_onclickEventHandler(htmlTextBoxDocumentClick); //
                                iEvent.onmousedown += new mshtml.HTMLDocumentEvents2_onmousedownEventHandler(htmlTextBoxMouseDown); //
                                //iEvent.onselectionchange += new mshtml.HTMLDocumentEvents2_onselectionchangeEventHandler(htmlTextBoxDocumentSelectionChange);
                                htmlInitialized = true;
                            }
                            htmlTextBox.Parent.Enabled = true;
                        }
                        else
                        {
                            richTextBox.Rtf = fullRTF;
                        }
                    }
                    else
                        richTextBox.Text = fullText;
                    endMarker = MultiLangEndMarker();
                    markerFont = richTextBox.SelectionFont;
                    TextWasCut = false;
                    markerColor = Color.Green;
                }
                clipRichTextLength = richTextBox.TextLength;
                if (!EditMode)
                {
                    if (htmlMode)
                    {
                        //HtmlElement paragraph = htmlTextBox.Document.CreateElement("p");
                        //paragraph.Style = "color: blue;";
                        //paragraph.InnerText = endMarker;
                        //paragraph.Style = markerColor.ToString();
                        //paragraph.Style = markerFont.ToString();
                        //htmlTextBox.Document.Body.AppendChild(paragraph);
                        if (filterText.Length > 0)
                        {
                            MarkRegExpMatchesInWebBrowser(htmlTextBox, Regex.Escape(filterText).Replace("%", ".*?"), Color.Red);
                        }
                    }
                    else
                    {
                        richTextBox.SelectionStart = richTextBox.TextLength;
                        richTextBox.SelectionColor = markerColor;
                        richTextBox.SelectionFont = markerFont;
                        if (TextWasCut)
                            endMarker = "\r\n" + endMarker;
                        richTextBox.AppendText(endMarker); // Do it first, else ending hyperlink will connect underline to it

                        MarkLinksInRichTextBox(richTextBox, out TextLinkMatches);
                        if (filterText.Length > 0)
                        {
                            MarkRegExpMatchesInRichTextBox(richTextBox, Regex.Escape(filterText).Replace("%", ".*?"), Color.Red, false, out FilterMatches);
                        }
                    }
                }
                richTextBox.SelectionColor = new Color();
                richTextBox.SelectionStart = 0;
                //richTextBox.HideSelection = false; // slow

                urlTextBox.HideSelection = true;
                urlTextBox.Clear();
                urlTextBox.Text = RowReader["Url"].ToString();
                MarkLinksInRichTextBox(urlTextBox, out UrlLinkMatches);

                if (clipType == "img")
                {
                    Image image = GetImageFromBinary((byte[])RowReader["Binary"]);
                    ImageControl.Image = image;
                }
                RestoreTextSelection(NewSelectionStart, NewSelectionLength);
                allowTextPositionChangeUpdate = true;
                UpdateSelectionPosition();
            }
            tableLayoutPanelData.SuspendLayout();
            UpdateClipButtons();
            if (clipType == "img")
            {
                tableLayoutPanelData.RowStyles[0].Height = 70;
                tableLayoutPanelData.RowStyles[0].SizeType = SizeType.Absolute;
                tableLayoutPanelData.RowStyles[1].Height = 0;
                tableLayoutPanelData.RowStyles[1].SizeType = SizeType.Absolute;
                tableLayoutPanelData.RowStyles[2].Height = 100;
                tableLayoutPanelData.RowStyles[2].SizeType = SizeType.Percent;
                if (elementPanelHasFocus)
                    ImageControl.Focus();
                htmlTextBox.Visible = false; // Without it htmlTextBox will be visible but why?
            }
            else if (htmlMode)
            {
                tableLayoutPanelData.RowStyles[0].Height = 0;
                tableLayoutPanelData.RowStyles[0].SizeType = SizeType.Absolute;
                tableLayoutPanelData.RowStyles[1].Height = 100;
                tableLayoutPanelData.RowStyles[1].SizeType = SizeType.Percent;
                tableLayoutPanelData.RowStyles[2].Height = 0;
                tableLayoutPanelData.RowStyles[2].SizeType = SizeType.Absolute;
                if (elementPanelHasFocus)
                    htmlTextBox.Focus();
                htmlTextBox.Visible = true;
            }
            else
            {
                tableLayoutPanelData.RowStyles[0].Height = 100;
                tableLayoutPanelData.RowStyles[0].SizeType = SizeType.Percent;
                tableLayoutPanelData.RowStyles[1].Height = 0;
                tableLayoutPanelData.RowStyles[1].SizeType = SizeType.Percent;
                tableLayoutPanelData.RowStyles[2].Height = 0;
                tableLayoutPanelData.RowStyles[2].SizeType = SizeType.Absolute;
                if (elementPanelHasFocus)
                    richTextBox.Focus();
            }
            if (urlTextBox.Text == "")
                tableLayoutPanelData.RowStyles[3].Height = 0;
            else
                tableLayoutPanelData.RowStyles[3].Height = 25;
            if (EditMode)
                richTextBox.Focus();
            tableLayoutPanelData.ResumeLayout();
        }

        private string GetHtmlFromHtmlClipText()
        {
            string htmlClipText = RowReader["htmlText"].ToString();
            if (String.IsNullOrEmpty(htmlClipText))
                return "";
            return htmlClipText.Substring(htmlClipText.IndexOf("<html", StringComparison.OrdinalIgnoreCase));
        }

        private void RestoreTextSelection(int NewSelectionStart = -1, int NewSelectionLength = -1)
        {
            if (NewSelectionStart == -1)
                NewSelectionStart = selectionStart;
            if (NewSelectionLength == -1)
                NewSelectionLength = selectionLength;
            if (htmlMode)
            {
                mshtml.IHTMLDocument2 htmlDoc = htmlTextBox.Document.DomDocument as mshtml.IHTMLDocument2;
                mshtml.IHTMLBodyElement body = htmlDoc.body as mshtml.IHTMLBodyElement;
                mshtml.IHTMLTxtRange range = body.createTextRange();
                range.moveStart("character", NewSelectionStart);
                range.collapse();
                range.moveEnd("character", NewSelectionLength);
                range.@select();
                range.scrollIntoView();
            }
            else
            {
                richTextBox.SelectionStart = NewSelectionStart;
                richTextBox.SelectionLength = NewSelectionLength;
                richTextBox.HideSelection = false;
            }
        }

        private void UpdateSelectionPosition()
        {
            if (htmlMode)
                htmlTextBoxDocumentSelectionChange();
            else
                richTextBox_SelectionChanged();
        }

        private SQLiteDataReader getRowReader(int id)
        {
            string sql = "SELECT * FROM Clips Where Id = @Id";
            SQLiteCommand commandSelect = new SQLiteCommand(sql, m_dbConnection);
            commandSelect.Parameters.AddWithValue("@Id", id);
            SQLiteDataReader rowReader = commandSelect.ExecuteReader();
            rowReader.Read();
            return rowReader;
        }

        private string FormatByteSize(int byteSize)
        {
            string[] sizes = { MultiLangByteUnit(), MultiLangKiloByteUnit(), MultiLangMegaByteUnit() };
            double len = byteSize;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = String.Format("{0:0.#} {1}", len, sizes[order]);
            return result;
        }

        private string MultiLangEndMarker()
        {
            return "<" + CurrentLangResourceManager.GetString("EndMarker") + ">";
        }

        private string MultiLangCutMarker()
        {
            return "<" + CurrentLangResourceManager.GetString("CutMarker") + ">";
        }

        private string MultiLangCharUnit()
        {
            return CurrentLangResourceManager.GetString("CharUnit");
        }

        private string MultiLangByteUnit()
        {
            return CurrentLangResourceManager.GetString("ByteUnit");
        }
        private string MultiLangKiloByteUnit()
        {
            return CurrentLangResourceManager.GetString("KiloByteUnit");
        }
        private string MultiLangMegaByteUnit()
        {
            return CurrentLangResourceManager.GetString("MegaByteUnit");
        }

        private void MarkLinksInRichTextBox(RichTextBox control, out MatchCollection matches)
        {
            MarkRegExpMatchesInRichTextBox(control, LinkPattern, Color.Blue, true, out matches);
        }

        private void MarkRegExpMatchesInRichTextBox(RichTextBox control, string pattern, Color color, bool underline, out MatchCollection matches)
        {
            matches = Regex.Matches(control.Text, pattern, RegexOptions.IgnoreCase);
            control.DeselectAll();
            int maxMarked = 100; // prevent slow down
            foreach (Match match in matches)
            {
                control.SelectionStart = match.Index;
                control.SelectionLength = match.Length;
                control.SelectionColor = color;
                if (underline)
                    control.SelectionFont = new Font(control.SelectionFont, FontStyle.Underline);
                maxMarked--;
                if (maxMarked < 0)
                    break;
            }
            control.DeselectAll();
            control.SelectionColor = new Color();
            control.SelectionFont = new Font(control.SelectionFont, FontStyle.Regular);
        }

        private void MarkRegExpMatchesInWebBrowser(WebBrowser control, string pattern, Color color)
        {
            mshtml.IHTMLDocument2 htmlDoc = (mshtml.IHTMLDocument2)htmlTextBox.Document.DomDocument;
            int boundingTop = 0;
            mshtml.IHTMLBodyElement body = htmlDoc.body as mshtml.IHTMLBodyElement;
            mshtml.IHTMLTxtRange range = body.createTextRange();
            while (range.findText(filterText))
            {
                range.execCommand("ForeColor", false, ColorTranslator.ToHtml(color));
                mshtml.IHTMLTextRangeMetrics metrics = (mshtml.IHTMLTextRangeMetrics)range;
                if (boundingTop == 0 || boundingTop > metrics.boundingTop)
                {
                    boundingTop = metrics.boundingTop;
                    range.scrollIntoView();
                }
                range.collapse(false);
            }
        }

        private void RichText_Click(object sender, EventArgs e)
        {
            OpenLinkIfCtrlPressed(sender as RichTextBox, e, TextLinkMatches);
            if (MaxTextViewSize >= (sender as RichTextBox).SelectionStart && TextWasCut)
                AfterRowLoad(true);
        }

        private void Filter_TextChanged(object sender, EventArgs e)
        {
            if (AllowFilterProcessing)
                timerApplyTextFiler.Start();
        }

        private void TextFilterApply()
        {
            ReadFilterText();
            ChooseTitleColumnDraw();
            UpdateClipBindingSource(true);
        }

        private void UpdateClipBindingSource(bool forceRowLoad = false, int currentClipId = 0)
        {
            if (dataAdapter == null)
                return;
            if (EditMode)
                SaveClipText();
            if (currentClipId == 0 && clipBindingSource.Current != null)
            {
                currentClipId = (int)(clipBindingSource.Current as DataRowView).Row["Id"];

            }
            allowRowLoad = false;
            filterOn = false;
            string sqlFilter = "1 = 1";
            string filterValue = "";
            if (filterText != "")
            {
                sqlFilter += " AND UPPER(Text) Like UPPER('%" + filterText + "%')";
                filterOn = true;
            }
            if (TypeFilter.SelectedValue as string != "allTypes")
            {
                filterValue = TypeFilter.SelectedValue as string;
                if (filterValue == "text")
                    filterValue = "'html','rtf','text'";
                else
                    filterValue = "'" + filterValue + "'";
                sqlFilter += " AND type IN (" + filterValue + ")";
                filterOn = true;
            }
            if (MarkFilter.SelectedValue as string != "allMarks")
            {
                filterValue = MarkFilter.SelectedValue as string;
                sqlFilter += " AND " + filterValue;
                filterOn = true;
            }
            string selectCommandText = "Select Id, Used, Title, Chars, Type, Favorite, ImageSample, AppPath From Clips";
            selectCommandText += " WHERE " + sqlFilter;
            selectCommandText += " ORDER BY Id desc";
            dataAdapter.SelectCommand.CommandText = selectCommandText;

            DataTable table = new DataTable();
            table.Locale = CultureInfo.InvariantCulture;
            dataAdapter.Fill(table);
            clipBindingSource.DataSource = table;

            PrepareTableGrid(); // Long
            if (filterOn)
                buttonClearFilter.BackColor = Color.GreenYellow;
            else
                buttonClearFilter.BackColor = DefaultBackColor;
            if (LastId == 0)
            {
                GotoLastRow();
                ClipsNumber = clipBindingSource.Count;
                DataRowView lastRow = (DataRowView)clipBindingSource.Current;
                if (lastRow == null)
                {
                    LastId = 0;
                }
                else
                {
                    LastId = (int)lastRow["Id"];
                }
            }
            else if (false
                //|| AutoGotoLastRow 
                || currentClipId <= 0)

                GotoLastRow();
            else if (currentClipId > 0)
            {
                int newPosition = clipBindingSource.Find("Id", currentClipId);
                clipBindingSource.Position = newPosition;
                ////if (dataGridView.CurrentRow != null)
                ////    dataGridView.CurrentCell = dataGridView.CurrentRow.Cells[0];
                SelectCurrentRow(forceRowLoad || newPosition == -1);
            }
            allowRowLoad = true;
            //AutoGotoLastRow = false;
        }

        private void ClearFilter_Click(object sender = null, EventArgs e = null)
        {
            ClearFilter();
            dataGridView.Focus();
        }

        private void ClearFilter(int CurrentClipID = 0)
        {
            if (!filterOn && CurrentClipID == 0) 
                return;
            AllowFilterProcessing = false;
            comboBoxFilter.Text = "";
            ReadFilterText();
            TypeFilter.SelectedIndex = 0;
            MarkFilter.SelectedIndex = 0;
            AllowFilterProcessing = true;
            ChooseTitleColumnDraw();
            //UpdateClipBindingSource(true, CurrentClipID);
            UpdateClipBindingSource(false, CurrentClipID);
        }

        private void Text_CursorChanged(object sender, EventArgs e)
        {
            // This event not working. Why? Decided to use Click instead.
        }

        //[DllImport("user32.dll")]
        //static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        //static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        //static readonly IntPtr HWND_TOP = new IntPtr(0);
        //const UInt32 SWP_NOSIZE = 0x0001;
        //const UInt32 SWP_NOMOVE = 0x0002;
        //const UInt32 SWP_NOACTIVATE = 0x0010;

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                //this.SuspendLayout();
                if (Properties.Settings.Default.FastWindowOpen)
                {
                    bool lastActSet = false;
                    if (lastActiveWindow != null)
                        lastActSet = SetForegroundWindow(lastActiveWindow);
                    if (!lastActSet)
                        //SetForegroundWindow(IntPtr.Zero); // This way focus was not lost!
                        SetActiveWindow(IntPtr.Zero);
                    this.Top = -10000;
                }
                else
                {
                    this.FormBorderStyle = FormBorderStyle.FixedToolWindow; // To disable animation
                    this.Hide();
                }
                e.Cancel = true;
                if (Properties.Settings.Default.ClearFiltersOnClose)
                    ClearFilter();
                //this.ResumeLayout();
            }
            else
            {
                if (WindowState == FormWindowState.Normal)
                    Properties.Settings.Default.WindowSize = Size;
                else
                    Properties.Settings.Default.WindowSize = RestoreBounds.Size;
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetClipboardOwner();

        private void GetClipboardData()
        {
            //
            // Data on the clipboard uses the 
            // IDataObject interface
            //
            //if (!CaptureClipboard)
            //{ return; }
            IDataObject iData = new DataObject();
            string clipType = "";
            string clipText = "";
            string clipWindow = "";
            string clipApplication = "";
            string richText = "";
            string htmlText = "";
            string clipUrl = "";
            int clipChars = 0;
            string appPath = "";
            GetClipboardOwnerInfo(out clipWindow, out clipApplication, out appPath);
            try
            {
                iData = Clipboard.GetDataObject();
            }
            catch (ExternalException externEx)
            {
                // Copying a field definition in Access 2002 causes this sometimes?
                Debug.WriteLine("Clipboard.GetDataObject(): InteropServices.ExternalException: {0}", externEx.Message);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), Application.ProductName);
                return;
            }
            if (iData.GetDataPresent(DataFormat_ClipboardViewerIgnore))
                return;
            bool textFormatPresent = false;
            if (iData.GetDataPresent(DataFormats.UnicodeText))
            {
                clipText = (string)iData.GetData(DataFormats.UnicodeText);
                clipType = "text";
                textFormatPresent = true;
            }
            else if (iData.GetDataPresent(DataFormats.Text))
            {
                clipText = (string)iData.GetData(DataFormats.Text);
                clipType = "text";
                textFormatPresent = true;
            }

            if (iData.GetDataPresent(DataFormats.Rtf))
            {
                richText = (string)iData.GetData(DataFormats.Rtf);
                clipType = "rtf";
                if (!textFormatPresent)
                {
                    var rtfBox = new RichTextBox();
                    rtfBox.Rtf = richText;
                    clipText = rtfBox.Text;
                    textFormatPresent = true;
                }
            }

            if (iData.GetDataPresent(DataFormats.Html))
            {
                htmlText = (string)iData.GetData(DataFormats.Html);
                if (!String.IsNullOrEmpty(htmlText))
                {
                    clipType = "html";
                    Match match = Regex.Match(htmlText, "SourceURL:(" + LinkPattern + ")", RegexOptions.IgnoreCase);
                    if (match.Captures.Count > 0)
                        clipUrl = match.Groups[1].ToString();
                    if (!textFormatPresent)
                    {
                        // It make take much time to parse big html, so we sacrifice text content
                    }
                }
            }

            //StringCollection UrlFormatNames = new StringCollection();
            //UrlFormatNames.Add("text/x-moz-url-priv");
            //UrlFormatNames.Add("msSourceUrl");
            //foreach (string UrlFormatName in UrlFormatNames)
            //    if (iData.GetDataPresent(UrlFormatName))
            //    {
            //        var ms = (MemoryStream)iData.GetData(UrlFormatName);
            //        var sr = new StreamReader(ms, Encoding.Unicode, true);
            //        Url = sr.ReadToEnd();
            //        break;
            //    }

            if (iData.GetDataPresent(DataFormats.FileDrop))
            {
                string[] fileNameList = iData.GetData(DataFormats.FileDrop) as string[];
                if (fileNameList != null)
                {
                    clipText = String.Join("\n", fileNameList);
                    if (iData.GetDataPresent(DataFormats.FileDrop))
                    {
                        clipType = "file";
                    }
                }
                else
                {
                    // Coping Outlook task
                }
            }

            byte[] binaryBuffer = new byte[0];
            byte[] imageSampleBuffer = new byte[0];
            // http://www.cyberforum.ru/ado-net/thread832314.html
            if (iData.GetDataPresent(DataFormats.Bitmap) && htmlText == "") // html text check to prevent crush from Excel clip
            {
                clipType = "img";
                Bitmap image = iData.GetData(DataFormats.Bitmap) as Bitmap;
                if (image == null)
                    // Happans while copying image in standart image viewer Windows 10
                    return;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    image.Save(memoryStream, ImageFormat.Png);
                    binaryBuffer = memoryStream.ToArray();
                }
                if (clipText == "")
                {
                    //clipText = CurrentLangResourceManager.GetString("Size") + ": " + image.Width + "x" + image.Height + "\n"
                    //     + CurrentLangResourceManager.GetString("PixelFormat") + ": " + image.PixelFormat + "\n";

                    clipText = image.Width + " x " + image.Height;
                    if (clipWindow != "")
                        clipText += ",\n" + clipWindow;
                    clipText += ",\n" + CurrentLangResourceManager.GetString("PixelFormat") + ": " +
                                Image.GetPixelFormatSize(image.PixelFormat) + "\n";
                }
                clipChars = image.Width * image.Height;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    Image ImageSample = CopyRectImage(image, new Rectangle(0, 0, 100, 20));
                    ImageSample.Save(memoryStream, ImageFormat.Png);
                    imageSampleBuffer = memoryStream.ToArray();
                }
                // OCR
                //try
                //{
                //    TesseractEngine ocr = new TesseractEngine("./tessdata", "eng", EngineMode.TesseractAndCube);
                //    string fileName = Path.GetTempFileName();
                //    image.Save(fileName);
                //    Pix pix = Pix.LoadFromFile(fileName);
                //    var result = ocr.Process(pix, PageSegMode.Auto);
                //    clipText = result.GetText();
                //}
                //catch (Exception e)
                //{
                //}
            }

            if (clipType != "")
            {
                AddClip(binaryBuffer, imageSampleBuffer, htmlText, richText, clipType, clipText, clipApplication, clipWindow, clipUrl, clipChars, appPath);
                //UpdateClipBindingSource();
            }

        }

        int CountLines(string str, int position = 0)
        {
            if (str == null)
                throw new ArgumentNullException("str");
            if (str == string.Empty)
                return 0;
            int index = -1;
            int count = 0;
            if (position == 0)
                position = str.Length;
            //while (-1 != (index = str.IndexOf(Environment.NewLine, index + 1)) && position > index)
            while (-1 != (index = str.IndexOf("\n", index + 1)) && position > index)
            {
                count++;
            }
            return count + 1;
        }

        void AddClip(byte[] binaryBuffer = null, byte[] imageSampleBuffer = null, string htmlText = "", string richText = "", string typeText = "text", string plainText = "",
            string applicationText = "", string windowText = "", string url = "", int chars = 0, string appPath = "")
        {
            if (plainText == null)
                plainText = "";
            if (richText == null)
                richText = "";
            if (htmlText == null)
                htmlText = "";
            int byteSize = plainText.Length * 2; // dirty
            if (chars == 0)
                chars = plainText.Length;
            LastId = LastId + 1;
            if (binaryBuffer != null)
                byteSize += binaryBuffer.Length;
            byteSize += htmlText.Length * 2; // dirty
            byteSize += richText.Length * 2; // dirty
            if (byteSize > Properties.Settings.Default.MaxClipSizeKB * 1000)
                return;
            DateTime created = DateTime.Now;
            string clipTitle = TextClipTitle(plainText);
            MD5 md5 = new MD5CryptoServiceProvider();
            if (binaryBuffer != null)
                md5.TransformBlock(binaryBuffer, 0, binaryBuffer.Length, binaryBuffer, 0);
            byte[] binaryText = Encoding.Unicode.GetBytes(plainText);
            md5.TransformBlock(binaryText, 0, binaryText.Length, binaryText, 0);
            byte[] binaryRichText = Encoding.Unicode.GetBytes(richText);
            md5.TransformBlock(binaryRichText, 0, binaryRichText.Length, binaryRichText, 0);
            byte[] binaryHtml = Encoding.Unicode.GetBytes(htmlText);
            md5.TransformFinalBlock(binaryHtml, 0, binaryHtml.Length);
            string hash = Convert.ToBase64String(md5.Hash);
            bool used = false;
            bool favorite = false;

            string sql = "SELECT Id, Title, Used, Favorite FROM Clips Where Hash = @Hash";
            SQLiteCommand commandSelect = new SQLiteCommand(sql, m_dbConnection);
            commandSelect.Parameters.AddWithValue("@Hash", hash);
            using (SQLiteDataReader reader = commandSelect.ExecuteReader())
            {
                if (reader.Read())
                {
                    used = GetNullableBoolFromSqlReader(reader, "Used");
                    favorite = GetNullableBoolFromSqlReader(reader, "Favorite");
                    clipTitle = reader.GetString(reader.GetOrdinal("Title"));
                    sql = "DELETE FROM Clips Where Id = @Id";
                    SQLiteCommand commandDelete = new SQLiteCommand(sql, m_dbConnection);
                    commandDelete.Parameters.AddWithValue("@Id", reader.GetInt32(reader.GetOrdinal("Id")));
                    commandDelete.ExecuteNonQuery();
                }
            }

            sql = "insert into Clips (Id, Title, Text, Application, Window, Created, Type, Binary, ImageSample, Size, Chars, RichText, HtmlText, Used, Favorite, Url, Hash, appPath) "
               + "values (@Id, @Title, @Text, @Application, @Window, @Created, @Type, @Binary, @ImageSample, @Size, @Chars, @RichText, @HtmlText, @Used, @Favorite, @Url, @Hash, @appPath)";

            SQLiteCommand commandInsert = new SQLiteCommand(sql, m_dbConnection);
            commandInsert.Parameters.AddWithValue("@Id", LastId);
            commandInsert.Parameters.AddWithValue("@Title", clipTitle);
            commandInsert.Parameters.AddWithValue("@Text", plainText);
            commandInsert.Parameters.AddWithValue("@RichText", richText);
            commandInsert.Parameters.AddWithValue("@HtmlText", htmlText);
            commandInsert.Parameters.AddWithValue("@Application", applicationText);
            commandInsert.Parameters.AddWithValue("@Window", windowText);
            commandInsert.Parameters.AddWithValue("@Created", created);
            commandInsert.Parameters.AddWithValue("@Type", typeText);
            commandInsert.Parameters.AddWithValue("@Binary", binaryBuffer);
            commandInsert.Parameters.AddWithValue("@ImageSample", imageSampleBuffer);
            commandInsert.Parameters.AddWithValue("@Size", byteSize);
            commandInsert.Parameters.AddWithValue("@Chars", chars);
            commandInsert.Parameters.AddWithValue("@Used", used);
            commandInsert.Parameters.AddWithValue("@Url", url);
            commandInsert.Parameters.AddWithValue("@Favorite", favorite);
            commandInsert.Parameters.AddWithValue("@Hash", hash);
            commandInsert.Parameters.AddWithValue("@appPath", appPath);
            commandInsert.ExecuteNonQuery();

            //dbDataSet.ClipsDataTable ClipsTable = (dbDataSet.ClipsDataTable)clipBindingSource.DataSource;
            //dbDataSet.ClipsRow NewRow = (dbDataSet.ClipsRow) ClipsTable.NewRow();
            //NewRow.Id = LastId;
            //NewRow.Title = Title;
            //NewRow.Text = Text;
            //NewRow.RichText = RichText;
            //NewRow.HtmlText = HtmlText;
            //NewRow.Application = Application;
            //NewRow.Window = Window;
            //NewRow.Created = Created;
            //NewRow.Type = Type;
            //NewRow.Binary = BinaryBuffer;
            //NewRow.Size = Size;
            //NewRow.Chars = Chars;
            //NewRow.Used = false;
            //NewRow.Url = Url;
            //NewRow.Hash = Hash;
            //foreach (DataColumn Column in dbDataSet.Clips.Columns)
            //{
            //    if (Column.DataType == System.Type.GetType("System.String") && Column.MaxLength > 0)
            //    {
            //        string NewValue = NewRow[Column.ColumnName] as string;
            //        NewRow[Column.ColumnName] = NewValue.Substring(0, Math.Min(NewValue.Length, Column.MaxLength));
            //    }
            //}
            ////dbDataSet.Clips.Rows.Add(NewRow);
            ////clipsTableAdapter.Insert(NewRow.Type, NewRow.Text, NewRow.Title, NewRow.Application, NewRow.Window, NewRow.Size, NewRow.Chars, NewRow.Created, NewRow.Binary, NewRow.RichText, NewRow.Id, NewRow.HtmlText, NewRow.Used);
            //ClipsTable.Rows.InsertAt(NewRow, 0);
            //PrepareTableGrid();

            ClipsNumber++;
            int numberOfClipsToDelete = ClipsNumber - Properties.Settings.Default.HistoryDepthNumber;
            if (numberOfClipsToDelete > 0)
            {
                commandInsert.CommandText = "Delete From Clips where (NOT Favorite OR Favorite IS NULL) AND Id IN (Select ID From Clips ORDER BY ID Limit @Number)";
                commandInsert.Parameters.Add("Number", DbType.Int32).Value = numberOfClipsToDelete;
                commandInsert.ExecuteNonQuery();
                ClipsNumber -= numberOfClipsToDelete;
            }
            //if (this.Visible)
            //{
                UpdateClipBindingSource();
                if (true
                    && applicationText == "ScreenshotReader"
                    && IsTextType(typeText)
                    //&& !Visible 
                    //&& Properties.Settings.Default.SelectTopClipOnShow 
                )
                    ShowForPaste();
            //}
        }

        private static bool GetNullableBoolFromSqlReader(SQLiteDataReader reader, string columnName)
        {
            int columnIndex = reader.GetOrdinal(columnName);
            bool result;
            bool DefaultValue = false;
            if (!reader.IsDBNull(columnIndex))
                result = reader.GetBoolean(columnIndex);
            else
                result = DefaultValue;
            return result;
        }

        private static string TextClipTitle(string text)
        {
            string title = text.TrimStart();
            title = Regex.Replace(title, @"\s+", " ");
            if (title.Length > ClipTitleLength)
            {
                title = title.Substring(0, ClipTitleLength - 1 - 3) + "...";
            }

            return title;
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            allowRowLoad = false;
            //int i = dataGridView.CurrentRow.Index;
            string sql = "Delete from Clips where Id IN(null";
            SQLiteCommand command = new SQLiteCommand("", m_dbConnection);
            int counter = 0;
            foreach (DataGridViewRow selectedRow in dataGridView.SelectedRows)
            {
                DataRowView dataRow = (DataRowView)selectedRow.DataBoundItem;
                string parameterName = "@Id" + counter;
                sql += "," + parameterName;
                command.Parameters.Add(parameterName, DbType.Int32).Value = dataRow["Id"];
                counter++;
                dataGridView.Rows.Remove(selectedRow);
                ClipsNumber--;
            }
            sql += ")";
            command.CommandText = sql;
            command.ExecuteNonQuery();
            //dataGridView.ClearSelection();
            //if (i+1 < dataGridView.Rows.Count)
            //    dataGridView.CurrentCell = dataGridView.Rows[i+1].Cells[0];
            //else if (i-1 >= 0)
            //    dataGridView.CurrentCell = dataGridView.Rows[i-1].Cells[0];
            //UpdateClipBindingSource();
            allowRowLoad = true;
            //AfterRowLoad();
            SelectCurrentRow();
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("User32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("User32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);
        [DllImport("user32.dll")]
        static extern int UnhookWinEvent(IntPtr hWinEventHook);
        [DllImport("User32.dll")]
        static extern bool PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("User32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);
        [DllImport("user32.dll")]
        static extern bool EnableWindow(IntPtr hwnd, bool bEnable);
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")]
        static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);
        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);
        [DllImport("user32.dll")]
        static extern IntPtr GetFocus();
        [DllImport("user32.dll")]
        static extern IntPtr SetFocus(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern IntPtr SetActiveWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);
        enum GetWindowCmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        // http://stackoverflow.com/questions/37291533/change-keyboard-layout-from-c-sharp-code-with-net-4-5-2
        internal sealed class KeyboardLayout
        {
            [DllImport("user32.dll")]
            static extern uint LoadKeyboardLayout(StringBuilder pwszKLID, uint flags);
            [DllImport("user32.dll")]
            static extern uint GetKeyboardLayout(uint idThread);
            [DllImport("user32.dll")]
            static extern uint ActivateKeyboardLayout(uint hkl, uint flags);

            private readonly uint hkl;

            static class KeyboardLayoutFlags
            {
                //https://msdn.microsoft.com/ru-ru/library/windows/desktop/ms646305(v=vs.85).aspx
                public const uint KLF_ACTIVATE = 0x00000001;
                public const uint KLF_SUBSTITUTE_OK = 0x00000002;
                public const uint KLF_SETFORPROCESS = 0x00000100;
            }

            private KeyboardLayout(CultureInfo cultureInfo)
            {
                string layoutName = cultureInfo.LCID.ToString("x8");

                var pwszKlid = new StringBuilder(layoutName);
                this.hkl = LoadKeyboardLayout(pwszKlid, KeyboardLayoutFlags.KLF_ACTIVATE | KeyboardLayoutFlags.KLF_SUBSTITUTE_OK);
            }

            private KeyboardLayout(uint hkl)
            {
                this.hkl = hkl;
            }

            public uint Handle
            {
                get
                {
                    return this.hkl;
                }
            }

            public static KeyboardLayout GetCurrent()
            {
                uint hkl = GetKeyboardLayout((uint)Thread.CurrentThread.ManagedThreadId);
                return new KeyboardLayout(hkl);
            }

            public static KeyboardLayout Load(CultureInfo culture)
            {
                return new KeyboardLayout(culture);
            }

            public void Activate()
            {
                ActivateKeyboardLayout(this.hkl, KeyboardLayoutFlags.KLF_SETFORPROCESS);
            }
        }

        //class KeyboardLayoutScope : IDisposable
        //{
        //    private readonly KeyboardLayout currentLayout;

        //    public KeyboardLayoutScope(CultureInfo culture)
        //    {
        //        this.currentLayout = KeyboardLayout.GetCurrent();
        //        var layout = KeyboardLayout.Load(culture);
        //        layout.Activate();
        //    }

        //    public void Dispose()
        //    {
        //        this.currentLayout.Activate();
        //    }
        //}

        private string CopyClipToClipboard(/*out DataObject oldDataObject,*/ bool onlySelectedPlainText = false)
        {
            //oldDataObject = null;
            StringCollection lastFilterValues = Properties.Settings.Default.LastFilterValues;
            if (filterText != "" && !lastFilterValues.Contains(filterText))
            {
                lastFilterValues.Insert(0, filterText);
                while (lastFilterValues.Count > 20)
                {
                    lastFilterValues.RemoveAt(lastFilterValues.Count - 1);
                }
                FillFilterItems();
            }

            //DataRow CurrentDataRow = ((DataRowView)clipBindingSource.Current).Row;
            string type = (string)RowReader["type"];
            object richText = RowReader["RichText"];
            object htmlText = RowReader["HtmlText"];
            byte[] binary = RowReader["Binary"] as byte[];
            string clipText = (string)RowReader["Text"];
            bool selectedPlainTextMode = onlySelectedPlainText && richTextBox.SelectedText != "";
            if (selectedPlainTextMode)
            {
                clipText = richTextBox.SelectedText; // Если тут не копировать, а передавать SelectedText, то возникает долгое ожидание потом
            }
            else if (EditMode)
                clipText = richTextBox.Text;
            DataObject dto = new DataObject();
            if (IsTextType() || type == "file")
            {
                dto.SetText(clipText, TextDataFormat.UnicodeText);
            }
            if (true
                && (type == "rtf" || type == "html")
                && !(richText is DBNull) 
                && richText.ToString() != ""
                && !onlySelectedPlainText)
            {
                dto.SetText((string)richText, TextDataFormat.Rtf);
            }
            if (true
                && type == "html" 
                && !(htmlText is DBNull) 
                && !onlySelectedPlainText)
            {
                dto.SetText((string)htmlText, TextDataFormat.Html);
            }
            if (type == "file" && !onlySelectedPlainText)
            {
                string[] fileNameList = clipText.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                StringCollection fileNameCollection = new StringCollection();
                foreach (string fileName in fileNameList)
                {
                    fileNameCollection.Add(fileName);
                }
                dto.SetFileDropList(fileNameCollection);
            }
            if (type == "img" && !onlySelectedPlainText)
            {
                Image image = GetImageFromBinary(binary);
                dto.SetImage(image);
                //MemoryStream ms = new MemoryStream();
                //MemoryStream ms2 = new MemoryStream();
                //image.Save(ms, ImageFormat.Bmp);
                //byte[] b = ms.GetBuffer();
                //ms2.Write(b, 14, (int)ms.Length - 14);
                //ms.Position = 0;
                //dto.SetData("DeviceIndependentBitmap", ms2);
            }
            //if (!Properties.Settings.Default.MoveCopiedClipToTop)
            //    CaptureClipboard = false;
            ////oldDataObject = (DataObject) Clipboard.GetDataObject();
            RemoveClipboardFormatListener(this.Handle);
            Clipboard.Clear();
            Clipboard.SetDataObject(dto, true); // Very important to set second parameter to true to give immidiate access to buffer to other processes!
            ConnectClipboard();
            return clipText;
        }

        private bool IsTextType(string type = "")
        {
            if (type == "")
                type = (string) RowReader["type"];
            return type == "rtf" || type == "text" || type == "html";
        }

        private void SendPasteClip(PasteMethod pasteMethod = PasteMethod.Standart)
        {
            if (dataGridView.CurrentRow == null)
                return;
            string textToPaste = "";
            //DataObject oldDataObject;
            //if (pasteMethod != PasteMethod.SendChars)
            textToPaste = CopyClipToClipboard(/*out oldDataObject,*/ pasteMethod != PasteMethod.Standart);
            //CultureInfo EnglishCultureInfo = null;
            //foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
            //{
            //    if (String.Compare(lang.Culture.TwoLetterISOLanguageName, "en", true) == 0)
            //    {
            //        EnglishCultureInfo = lang.Culture;
            //        break;
            //    }
            //}
            //if (EnglishCultureInfo == null)
            //{
            //    MessageBox.Show(this, "Unable to find English input language", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}
            int targetProcessId;
            uint remoteThreadId = GetWindowThreadProcessId(lastActiveWindow, out targetProcessId);
            bool needElevation = targetProcessId != 0 && !UacHelper.IsProcessAccessible(targetProcessId);
            //if (needElevation && pasteMethod == PasteMethod.SendChars)
            //{
            //    ShowElevationFail();
            //    return;
            //}

            // not reliable method
            // Previous active window by z-order https://www.whitebyte.info/programming/how-to-get-main-window-handle-of-the-last-active-window

            if (!this.TopMost)
            {
                this.Close();
            }
            else
            {
                SetForegroundWindow(lastActiveWindow);
                Debug.WriteLine("Set foreground window " + lastActiveWindow + " " + GetWindowTitle(lastActiveWindow));
            }
            int waitStep = 5;
            IntPtr hForegroundWindow = IntPtr.Zero;
            for (int i = 0; i < 500; i += waitStep)
            {
                hForegroundWindow = GetForegroundWindow();
                if (hForegroundWindow != IntPtr.Zero)
                    break;
                Thread.Sleep(waitStep);
            }
            Debug.WriteLine("Get foreground window " + hForegroundWindow + " " + GetWindowTitle(hForegroundWindow));
            //IntPtr hFocusWindow = FocusWindow();
            //string WindowTitle = GetWindowTitle(hFocusWindow);
            //Debug.WriteLine("Window = " + hFocusWindow + " \"" + WindowTitle + "\"");
            //Thread.Sleep(50);

            //AttachThreadInput(GetCurrentThreadId(), remoteThreadId, true);
            //IntPtr hFocusWindow = IntPtr.Zero;
            //for (int i = 0; i < 500; i+= waitStep)
            //{
            //    hFocusWindow = GetFocus();
            //    //var guiInfo = new GUITHREADINFO();
            //    //guiInfo.cbSize = (uint)Marshal.SizeOf(guiInfo);
            //    //GetGUIThreadInfo(remoteThreadId, out guiInfo);
            //    if (hFocusWindow != IntPtr.Zero)
            //    {
            //        //Debug.WriteLine("Active " + guiInfo.hwndActive);
            //        //Debug.WriteLine("Caret " + guiInfo.hwndCaret);
            //        SetActiveWindow(hFocusWindow);
            //        break;
            //    }
            //    Thread.Sleep(waitStep);
            //}
            //Debug.WriteLine("Got focus window " + hFocusWindow + " " + GetWindowTitle(hFocusWindow));

            var curproc = Process.GetCurrentProcess();
            if (needElevation)
            {
                string ElevatedMutexName = "ClipAngelElevatedMutex" + curproc.Id;
                Mutex ElevatedMutex = null;
                try
                {
                    ElevatedMutex = Mutex.OpenExisting(ElevatedMutexName);
                }
                catch
                {
                    string exePath = curproc.MainModule.FileName;
                    //exePath = "D:\\VC\\ClipAngel\\bin\\Debug\\ClipAngel.exe";
                    ProcessStartInfo startInfo = new ProcessStartInfo(exePath, "/elevated " + curproc.Id);
                    startInfo.Verb = "runas";
                    try
                    {
                        Process.Start(startInfo);
                    }
                    catch
                    {
                        ShowElevationFail();
                        return;
                    }
                }
                int maxWait = 2000;
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                while (stopWatch.ElapsedMilliseconds < maxWait)
                {
                    try
                    {
                        ElevatedMutex = Mutex.OpenExisting(ElevatedMutexName);
                        break;
                    }
                    catch
                    {
                    }
                    Thread.Sleep(5);
                }
                if (ElevatedMutex == null)
                {
                    ShowElevationFail();
                    return;
                }
            }
            if (pasteMethod != PasteMethod.SendChars)
            {
                if (!needElevation)
                    Paster.SendPaste();
                else
                {
                    EventWaitHandle pasteEvent = Paster.GetPasteEventWaiter();
                    pasteEvent.Set();
                }
            }
            else
            {
                if (!IsTextType())
                    return;
                if (!needElevation)
                    Paster.SendChars();
                else
                {
                    EventWaitHandle sendCharsEvent = Paster.GetSendCharsEventWaiter();
                    sendCharsEvent.Set();
                }
            }
            //AttachThreadInput(GetCurrentThreadId(), remoteThreadId, false);
            SetRowMark("Used");
            if (false
                || Properties.Settings.Default.MoveCopiedClipToTop 
                || (true 
                    && pasteMethod == PasteMethod.PasteText 
                    && richTextBox.SelectedText != ""))
            {
                GetClipboardData();
            }
            else
            {
                ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row["Used"] = true;
                //PrepareTableGrid();
                UpdateTableGridRowBackColor(dataGridView.CurrentRow);
            }

            // We need delay about 100ms before restore clipboard object
            //Clipboard.SetDataObject(oldDataObject);
        }

        private void ShowElevationFail()
        {
            MessageBox.Show(this, CurrentLangResourceManager.GetString("CantPasteInElevatedWindow"), Application.ProductName);
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private static IntPtr GetTopParentWindow(IntPtr hForegroundWindow)
        {
            while (true)
            {
                IntPtr temp = GetParent(hForegroundWindow);
                if (temp.Equals(IntPtr.Zero)) break;
                hForegroundWindow = temp;
            }

            return hForegroundWindow;
        }

        private void SetRowMark(string fieldName, bool newValue = true, bool allSelected = false)
        {
            string sql = "Update Clips set " + fieldName + "=@Value where Id IN(null";
            SQLiteCommand command = new SQLiteCommand("", m_dbConnection);
            List<DataGridViewRow> selectedRows = new List<DataGridViewRow>();
            if (allSelected)
                foreach (DataGridViewRow selectedRow in dataGridView.SelectedRows)
                    selectedRows.Add(selectedRow);
            else
                selectedRows.Add(dataGridView.CurrentRow);
            int counter = 0;
            ReadFilterText();
            foreach (DataGridViewRow selectedRow in selectedRows)
            {
                if (selectedRow == null)
                    continue;
                DataRowView dataRow = (DataRowView)selectedRow.DataBoundItem;
                string parameterName = "@Id" + counter;
                sql += "," + parameterName;
                command.Parameters.Add(parameterName, DbType.Int32).Value = dataRow["Id"];
                counter++;
                dataRow[fieldName] = newValue;
                UpdateTableGridRowBackColor(selectedRow);
            }
            sql += ")";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Value", newValue);
            command.ExecuteNonQuery();

            ////dbDataSet.ClipsRow Row = (dbDataSet.ClipsRow)dbDataSet.Clips.Rows[dataGridView.CurrentRow.Index];
            ////Row[fieldName] = newValue;
            ////dataAdapter.Update(dbDataSet);
            if (allSelected)
                UpdateClipBindingSource(true);
            else
                UpdateClipButtons();
        }

        private void UpdateClipButtons()
        {
            toolStripButtonMarkFavorite.Checked = BoolFieldValue("Favorite"); // dataGridView.CurrentRow could be null here!
        }

        private void ReadFilterText()
        {
            filterText = comboBoxFilter.Text;
        }

        private static Image GetImageFromBinary(byte[] binary)
        {
            MemoryStream memoryStream = new MemoryStream(binary, 0, binary.Length);
            memoryStream.Write(binary, 0, binary.Length);
            Image image = new Bitmap(memoryStream);
            return image;
        }

        private void FillFilterItems()
        {
            StringCollection lastFilterValues = Properties.Settings.Default.LastFilterValues;
            comboBoxFilter.Items.Clear();
            foreach (string String in lastFilterValues)
            {
                comboBoxFilter.Items.Add(String);
            }
        }

        IntPtr GetFocusWindow(int maxWait = 100)
        {
            IntPtr hwnd = GetForegroundWindow();
            int pid;
            uint remoteThreadId = GetWindowThreadProcessId(hwnd, out pid);
            uint currentThreadId = GetCurrentThreadId();
            //AttachTrheadInput is needed so we can get the handle of a focused window in another app
            AttachThreadInput(remoteThreadId, currentThreadId, true);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            while (stopWatch.ElapsedMilliseconds < maxWait)
            {
                hwnd = GetFocus();
                if (hwnd != IntPtr.Zero)
                    break;
                Thread.Sleep(5);
            }
            AttachThreadInput(remoteThreadId, currentThreadId, false);
            return hwnd;
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("psapi.dll")]
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        // http://stackoverflow.com/questions/9501771/how-to-avoid-a-win32-exception-when-accessing-process-mainmodule-filename-in-c
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetProcessMainModuleFullName(int pid)
        {
            var processHandle = OpenProcess(0x0400 | 0x0010, false, pid);
            if (processHandle == IntPtr.Zero)
            {
                // Not enough priviledges. Need to call it elevated
                return null;
            }
            const int lengthSb = 4000;
            var sb = new StringBuilder(lengthSb);
            string result = null;
            if (GetModuleFileNameEx(processHandle, IntPtr.Zero, sb, lengthSb) > 0)
            {
                result = sb.ToString();
            }
            CloseHandle(processHandle);
            return result;
        }

        void GetClipboardOwnerInfo(out string window, out string application, out string appPath)
        {
            IntPtr hwnd = GetClipboardOwner();
            if (hwnd == IntPtr.Zero)
            {
                hwnd = lastActiveWindow;
            }
            int processId;
            GetWindowThreadProcessId(hwnd, out processId);
            Process process1 = Process.GetProcessById(processId);
            application = process1.ProcessName;
            appPath = GetProcessMainModuleFullName(processId);
            hwnd = process1.MainWindowHandle;
            window = GetWindowTitle(hwnd);
            //// We need top level window
            ////const uint GW_OWNER = 4;
            //while ((int)hwnd != 0)
            //{
            //    Window = GetWindowTitle(hwnd);
            //    //IntPtr hOwner = GetWindow(hwnd, GW_OWNER);
            //    hwnd = GetParent(hwnd);
            //    //if ((int) hwnd == 0)
            //    //{
            //    //    hwnd = hOwner;
            //    //}
            //}
        }

        void sendKey(IntPtr hwnd, Keys keyCode, bool extended = false, bool down = true, bool up = true)
        {
            // http://stackoverflow.com/questions/10280000/how-to-create-lparam-of-sendmessage-wm-keydown
            const int WM_KEYDOWN = 0x0100;
            const int WM_KEYUP = 0x0101;
            uint scanCode = MapVirtualKey((uint)keyCode, 0);
            uint lParam = 0x00000001 | (scanCode << 16);
            if (extended)
            {
                lParam |= 0x01000000;
            }
            if (down)
            {
                PostMessage(hwnd, WM_KEYDOWN, (int)keyCode, (int)lParam);
            }
            lParam |= 0xC0000000;  // set previous key and transition states (bits 30 and 31)
            if (up)
            {
                PostMessage(hwnd, WM_KEYUP, (int)keyCode, (int)lParam);
            }
        }

        private static string GetWindowTitle(IntPtr hwnd)
        {
            const int nChars = 256;
            StringBuilder buff = new StringBuilder(nChars);
            string windowTitle = "";
            if (GetWindowText(hwnd, buff, nChars) > 0)
            {
                windowTitle = buff.ToString();
            }
            return windowTitle;
        }

        private void dataGridView_DoubleClick(object sender, EventArgs e)
        {
            SendPasteClip();
        }
        private void pasteOriginalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendPasteClip();
        }
        private void pasteAsTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendPasteClip(PasteMethod.PasteText);
        }
        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (allowRowLoad)
            {
                if (EditMode)
                    editClipTextToolStripMenuItem_Click();
                else 
                    LoadClipIfChangedID();
            }
        }

        private void SaveClipText()
        {
            string sql = "Update Clips set Title = @Title, Text = @Text where Id = @Id";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.Parameters.AddWithValue("@Id", RowReader["Id"]);
            command.Parameters.AddWithValue("@Text", richTextBox.Text);
            string newTitle = "";
            if (RowReader["Title"].ToString() == TextClipTitle(RowReader["Text"].ToString()))
                newTitle = TextClipTitle(richTextBox.Text);
            else
                newTitle = RowReader["Title"].ToString();
            command.Parameters.AddWithValue("@Title", newTitle);
            command.ExecuteNonQuery();
        }

        private void Main_Deactivate(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.FastWindowOpen)
            {
                //if (this.WindowState == FormWindowState.Minimized)
                //{
                //    this.ShowInTaskbar = false;
                //    //notifyIcon.Visible = true;
                //}
                if (this.Top >= 0)
                    factualTop = this.Top;
            }
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            { ShowForPaste(); }
        }

        [DllImport("user32.dll", EntryPoint = "GetGUIThreadInfo")]
        public static extern bool GetGUIThreadInfo(uint tId, out GUITHREADINFO threadInfo);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, out Point position);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr handle, out RECT lpRect);

        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool GetClientRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32")]
        private extern static int GetCaretPos(out Point p);

        [StructLayout(LayoutKind.Sequential)]
        public struct GUITHREADINFO
        {
            public uint cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
            //public System.Drawing.Rectangle rcCaret;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        private void ShowForPaste(bool onlyFavorites = false)
        {
            if (onlyFavorites)
                showOnlyFavoriteToolStripMenuItem_Click();
            else if (MarkFilter.SelectedValue.ToString() == "favorite")
                showAllMarksToolStripMenuItem_Click();
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            this.SuspendLayout();

            int newX = -1;
            int newY = -1;
            //AutoGotoLastRow = Properties.Settings.Default.SelectTopClipOnShow;
            if (Properties.Settings.Default.WindowAutoPosition)
            {
                // https://www.codeproject.com/Articles/34520/Getting-Caret-Position-Inside-Any-Application
                // http://stackoverflow.com/questions/31055249/is-it-possible-to-get-caret-position-in-word-to-update-faster
                //IntPtr hWindow = GetForegroundWindow();
                IntPtr hWindow = lastActiveWindow;
                if (hWindow != this.Handle && hWindow != IntPtr.Zero)
                { 
	                int pid;
	                uint remoteThreadId = GetWindowThreadProcessId(hWindow, out pid);
	                var guiInfo = new GUITHREADINFO();
	                guiInfo.cbSize = (uint)Marshal.SizeOf(guiInfo);
	                GetGUIThreadInfo(remoteThreadId, out guiInfo);
	                Point point = new Point(0, 0);
                    ClientToScreen(guiInfo.hwndCaret, out point);
                    //AttachThreadInput(GetCurrentThreadId(), remoteThreadId, true);
                    //int Result = GetCaretPos(out point);
                    //AttachThreadInput(GetCurrentThreadId(), remoteThreadId, false);
                    // Screen.FromHandle(hwnd)
                    RECT activeRect;
                    if (point.Y > 0)
	                {
                        activeRect = guiInfo.rcCaret;
                        newX = Math.Min(activeRect.right + point.X, SystemInformation.VirtualScreen.Width - this.Width);
                        newY = Math.Min(activeRect.bottom + point.Y + 1, SystemInformation.VirtualScreen.Height - this.Height - 30);
                    }
                    else
                    {
                        IntPtr baseWindow;
                        if (guiInfo.hwndFocus != IntPtr.Zero)
                            baseWindow = guiInfo.hwndFocus;
                        else
                            baseWindow = hWindow;
                        ClientToScreen(baseWindow, out point);
                        GetWindowRect(baseWindow, out activeRect);
                        newX = Math.Max(0, Math.Min((activeRect.right - activeRect.left - this.Width) / 2 + point.X, SystemInformation.VirtualScreen.Width - this.Width));
                        newY = Math.Max(0, Math.Min((activeRect.bottom - activeRect.top - this.Height) / 2 + point.Y, SystemInformation.VirtualScreen.Height - this.Height - 30));
                    }
                }
            }
            RestoreWindowIfMinimized(newX, newY);
            //sw.Stop();
            //Debug.WriteLine("autoposition duration" + sw.ElapsedMilliseconds.ToString());
            if (!Properties.Settings.Default.FastWindowOpen)
            {
                this.Activate();
                this.Show();
            }
            SetForegroundWindow(this.Handle);
            if (Properties.Settings.Default.SelectTopClipOnShow)
                GotoLastRow();
            this.ResumeLayout();
        }

        private void RestoreWindowIfMinimized(int newX = -1, int newY = -1)
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            if (!allowVisible)
            {
                allowVisible = true;
                this.Activate();
                Show();
            }
            if (Properties.Settings.Default.FastWindowOpen)
            { 
                if (newX == -1)
                {
                    if (this.Left >= 0)
                        newX = this.Left;
                    else
                        newX = this.RestoreBounds.X;
                }
                if (newY == -1)
                {
                    if (this.Top >= 0)
                        newY = this.Top;
                    else
                        newY = this.RestoreBounds.Y;
                }
                if (Properties.Settings.Default.FastWindowOpen)
                    if (newY < 0)
                        newY = factualTop;
                if (newX > 0)
                    MoveWindow(this.Handle, newX, newY, this.Width, this.Height, true);
            }
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal; // Window can be minimized by "Minimize All" command
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                this.Close();
            }
            if (e.KeyCode == Keys.Enter)
            {
                PasteMethod pasteMethod;
                if (e.Control)
                    pasteMethod = PasteMethod.PasteText;
                else
                {
                    if (!pasteENTERToolStripMenuItem.Enabled)
                        return;
                    pasteMethod = PasteMethod.Standart;
                }
                SendPasteClip(pasteMethod);
                e.Handled = true;
            }
        }

        private void exitToolStripMenuItem_Click(object sender = null, EventArgs e = null)
        {
            Application.Exit();
        }

        private void Filter_KeyDown(object sender, KeyEventArgs e)
        {
            PassKeyToGrid(true, e);
        }

        private void Filter_KeyUp(object sender, KeyEventArgs e)
        {
            PassKeyToGrid(false, e);
        }

        private void PassKeyToGrid(bool downOrUp, KeyEventArgs e)
        {
            if (IsKeyPassedFromFilterToGrid(e.KeyCode, e.Control))
            {
                sendKey(dataGridView.Handle, e.KeyCode, false, downOrUp, !downOrUp);
                e.Handled = true;
            }
         }

        private static bool IsKeyPassedFromFilterToGrid(Keys key, bool isCtrlDown = false)
        {
            return false
                || key == Keys.Down
                || key == Keys.Up
                || key == Keys.PageUp
                || key == Keys.PageDown
                || key == Keys.ControlKey
                || key == Keys.ControlKey
                || key == Keys.Home && isCtrlDown
                || key == Keys.End && isCtrlDown;
        }

        //protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        //{
        //    if (keyData == (Keys.Control | Keys.F9))
        //    {
        //        ClearFilter_Click();
        //        return true;
        //    }
        //    return base.ProcessCmdKey(ref msg, keyData);
        //}

        private void dataGridView_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearFilter_Click();
        }

        private void gotoLastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GotoLastRow();
        }

        void GotoLastRow()
        {
            if (dataGridView.Rows.Count > 0)
            {
                clipBindingSource.MoveFirst();
                if (dataGridView.CurrentRow != null)
                SelectCurrentRow();
            }
            LoadClipIfChangedID();
        }

        private bool CurrentIDChanged()
        {
            return !(true
                && RowReader != null
                && dataGridView.CurrentRow != null
                && (int)(dataGridView.CurrentRow.DataBoundItem as DataRowView)["ID"] == (int)RowReader["ID"]);
        }

        void SelectCurrentRow(bool forceRowLoad = false, bool keepTextSelection = true)
        {
            dataGridView.ClearSelection();
            if (dataGridView.CurrentRow == null)
            {
                GotoLastRow();
                return;
            }
            dataGridView.Rows[dataGridView.CurrentRow.Index].Selected = true;
            LoadClipIfChangedID(forceRowLoad, keepTextSelection);
        }

        private void LoadClipIfChangedID(bool forceRowLoad = false, bool keepTextSelection = true)
        {
            bool currentIDChanged = CurrentIDChanged();
            if (forceRowLoad || currentIDChanged)
            {
                if (currentIDChanged)
                    keepTextSelection = false;
                int NewSelectionStart, NewSelectionLength;
                if (keepTextSelection)
                {
                    NewSelectionStart = -1;
                    NewSelectionLength = -1;
                }
                else
                {
                    NewSelectionStart = 0;
                    NewSelectionLength = 0;
                }
                AfterRowLoad(false, -1, NewSelectionStart, NewSelectionLength);
            }
        }

        private void activateListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView.Focus();
        }

        private void PrepareTableGrid()
        {
            //ReadFilterText();
            //foreach (DataGridViewRow row in dataGridView.Rows)
            //{
            //    PrepareRow(row);
            //}
            //dataGridView.Update();
        }

        private void PrepareRow(DataGridViewRow row = null)
        {
            if (row == null)
                row = dataGridView.CurrentRow;
            DataRowView dataRowView = (DataRowView) row.DataBoundItem;
            int shortSize = dataRowView.Row["Chars"].ToString().Length;
            if (shortSize > 2)
                row.Cells["VisualWeight"].Value = shortSize;
            string clipType = (string) dataRowView.Row["Type"];
            Bitmap image = null;
            switch (clipType)
            {
                case "text":
                    image = imageText;
                    break;
                case "html":
                    image = imageHtml;
                    break;
                case "rtf":
                    image = imageRtf;
                    break;
                case "file":
                    image = imageFile;
                    break;
                case "img":
                    image = imageImg;
                    break;
                default:
                    break;
            }
            if (image != null)
            {
                row.Cells["TypeImg"].Value = image;
            }
            row.Cells["ColumnTitle"].Value = dataRowView.Row["Title"].ToString();
            if (filterText != "" && dataGridView.Columns["ColumnTitle"].Visible)
            {
                _richTextBox.Clear();
                _richTextBox.Text = row.Cells["ColumnTitle"].Value.ToString();
                MatchCollection tempMatches;
                MarkRegExpMatchesInRichTextBox(_richTextBox, Regex.Escape(filterText).Replace("%", ".*?"), Color.Red,
                    false, out tempMatches);
                row.Cells["ColumnTitle"].Value = _richTextBox.Rtf;
            }
            if (dataGridView.Columns["ColumnTitle"].Visible)
            {
                var imageSampleBuffer = dataRowView["ImageSample"];
                if (imageSampleBuffer != DBNull.Value)
                    if ((imageSampleBuffer as byte[]).Length > 0)
                    {
                        Image imageSample = GetImageFromBinary((byte[])imageSampleBuffer);
                        row.Cells["imageSample"].Value = ChangeImageOpacity(imageSample, 0.8f);
                        ////string str = BitConverter.ToString((byte[])imageSampleBuffer, 0).Replace("-", string.Empty);
                        ////string imgString = @"{\pict\pngblip\picw" + imageSample.Width + @"\pich" + imageSample.Height + @"\picwgoal" + imageSample.Width + @"\pichgoal" + imageSample.Height + @"\bin " + str + "}";
                        //string imgString = GetEmbedImageString((Bitmap)imageSample, 0, 18);
                        //_richTextBox.SelectionStart = _richTextBox.TextLength;
                        //_richTextBox.SelectedRtf = imgString;
                    }

            }
            if (dataGridView.Columns["AppImage"].Visible)
            {
                var bitmap = ApplicationIcon(dataRowView["appPath"].ToString(), false);
                if (bitmap != null)
                    row.Cells["AppImage"].Value = bitmap;
            }
            UpdateTableGridRowBackColor(row);
        }

        private Bitmap ApplicationIcon(string filePath, bool original = true)
        {
            Bitmap originalImage = null;
            Bitmap brightImage = null;
            if (origibalIconCache.ContainsKey(filePath))
            {
                originalImage = origibalIconCache[filePath];
                brightImage = brightIconCache[filePath];
            }
            else
            {
                if (File.Exists(filePath))
                {
                    Icon smallIcon = null;
                    smallIcon = IconTools.GetIconForFile(filePath, ShellIconSize.SmallIcon);
                    originalImage = smallIcon.ToBitmap();
                    brightImage = (Bitmap) ChangeImageOpacity(originalImage, 0.6f);
                }
                origibalIconCache[filePath] = originalImage;
                brightIconCache[filePath] = brightImage;
            }
            if (original)
                return originalImage;
            else
                return brightImage;
        }

        /// <param name="opacity">Opacity, where 1.0 is no opacity, 0.0 is full transparency</param>
        public static Image ChangeImageOpacity(Image originalImage, double opacity)
        {
            const int bytesPerPixel = 4;
            if ((originalImage.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
            {
                // Cannot modify an image with indexed colors
                return originalImage;
            }

            Bitmap bmp = (Bitmap)originalImage.Clone();

            // Specify a pixel format.
            PixelFormat pxf = PixelFormat.Format32bppArgb;

            // Lock the bitmap's bits.
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            // This code is specific to a bitmap with 32 bits per pixels 
            // (32 bits = 4 bytes, 3 for RGB and 1 byte for alpha).
            int numBytes = bmp.Width * bmp.Height * bytesPerPixel;
            byte[] argbValues = new byte[numBytes];

            // Copy the ARGB values into the array.
            Marshal.Copy(ptr, argbValues, 0, numBytes);

            // Manipulate the bitmap, such as changing the
            // RGB values for all pixels in the the bitmap.
            for (int counter = 0; counter < argbValues.Length; counter += bytesPerPixel)
            {
                // argbValues is in format BGRA (Blue, Green, Red, Alpha)

                // If 100% transparent, skip pixel
                if (argbValues[counter + bytesPerPixel - 1] == 0)
                    continue;

                int pos = 0;
                pos++; // B value
                pos++; // G value
                pos++; // R value

                argbValues[counter + pos] = (byte)(argbValues[counter + pos] * opacity);
            }

            // Copy the ARGB values back to the bitmap
            Marshal.Copy(argbValues, 0, ptr, numBytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        public string GetImageForRTF(Image img, int width = 0, int height = 0)
        {
            //string newPath = Path.Combine(Environment.CurrentDirectory, path);
            //Image img = Image.FromFile(newPath);
            if (width == 0)
                width = img.Width;
            if (height == 0)
                height = img.Width;
            MemoryStream stream = new MemoryStream();
            img.Save(stream, ImageFormat.Bmp);
            byte[] bytes = stream.ToArray();
            string str = BitConverter.ToString(bytes, 0).Replace("-", string.Empty);
            //string str = System.Text.Encoding.UTF8.GetString(bytes);
            string mpic = @"{\pict\wbitmapN\picw" + img.Width + @"\pich" + img.Height + @"\picwgoal" + width + @"\pichgoal" + height + @"\bin " + str + "}";
            return mpic;
        }

        // RTF Image Format
        // {\pict\wmetafile8\picw[A]\pich[B]\picwgoal[C]\pichgoal[D]
        //  
        // A    = (Image Width in Pixels / Graphics.DpiX) * 2540 
        //  
        // B    = (Image Height in Pixels / Graphics.DpiX) * 2540 
        //  
        // C    = (Image Width in Pixels / Graphics.DpiX) * 1440 
        //  
        // D    = (Image Height in Pixels / Graphics.DpiX) * 1440 

        [Flags]
        enum EmfToWmfBitsFlags
        {
            EmfToWmfBitsFlagsDefault = 0x00000000,
            EmfToWmfBitsFlagsEmbedEmf = 0x00000001,
            EmfToWmfBitsFlagsIncludePlaceable = 0x00000002,
            EmfToWmfBitsFlagsNoXORClip = 0x00000004
        }

        const int MM_ISOTROPIC = 7;
        const int MM_ANISOTROPIC = 8;

        [DllImport("gdiplus.dll")]
        private static extern uint GdipEmfToWmfBits(IntPtr _hEmf, uint _bufferSize,
            byte[] _buffer, int _mappingMode, EmfToWmfBitsFlags _flags);
        [DllImport("gdi32.dll")]
        private static extern IntPtr SetMetaFileBitsEx(uint _bufferSize,
            byte[] _buffer);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CopyMetaFile(IntPtr hWmf,
            string filename);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteMetaFile(IntPtr hWmf);
        [DllImport("gdi32.dll")]
        private static extern bool DeleteEnhMetaFile(IntPtr hEmf);

        public static string GetEmbedImageString(Bitmap image, int width = 0, int height = 0)
        {
            Metafile metafile = null;
            float dpiX; float dpiY;
            if (height == 0 || height > image.Height)
                height = image.Height;
            if (width == 0 || width > image.Width)
                width = image.Width;
            using (Graphics g = Graphics.FromImage(image))
            {
                IntPtr hDC = g.GetHdc();
                metafile = new Metafile(hDC, EmfType.EmfOnly);
                g.ReleaseHdc(hDC);
            }

            using (Graphics g = Graphics.FromImage(metafile))
            {
                g.DrawImage(image, 0, 0);
                dpiX = g.DpiX;
                dpiY = g.DpiY;
            }

            IntPtr _hEmf = metafile.GetHenhmetafile();
            uint _bufferSize = GdipEmfToWmfBits(_hEmf, 0, null, MM_ANISOTROPIC,
                EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);
            byte[] _buffer = new byte[_bufferSize];
            GdipEmfToWmfBits(_hEmf, _bufferSize, _buffer, MM_ANISOTROPIC,
                                        EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault);
            IntPtr hmf = SetMetaFileBitsEx(_bufferSize, _buffer);
            string tempfile = Path.GetTempFileName();
            CopyMetaFile(hmf, tempfile);
            DeleteMetaFile(hmf);
            DeleteEnhMetaFile(_hEmf);

            var stream = new MemoryStream();
            byte[] data = File.ReadAllBytes(tempfile);
            //File.Delete (tempfile);
            int count = data.Length;
            stream.Write(data, 0, count);

            string proto = @"{\rtf1{\pict\wmetafile8\picw" + (int)(((float)width / dpiX) * 2540)
                              + @"\pich" + (int)(((float)height / dpiY) * 2540)
                              + @"\picwgoal" + (int)(((float)width / dpiX) * 1440)
                              + @"\pichgoal" + (int)(((float) height / dpiY) * 1440)
                              + " "
                  + BitConverter.ToString(stream.ToArray()).Replace("-", "")
                              + "}}";
            return proto;
        }

        private void MergeCellsInRow(DataGridView dataGridView1, DataGridViewRow row, int col1, int col2)
        {
            Graphics g = dataGridView1.CreateGraphics();
            Pen p = new Pen(dataGridView1.GridColor);
            Rectangle r1 = dataGridView1.GetCellDisplayRectangle(col1, row.Index, true);
            //Rectangle r2 = dataGridView1.GetCellDisplayRectangle(col2, row.Index, true);

            int recWidth = 0;
            string recValue = string.Empty;
            for (int i = col1; i <= col2; i++)
            {
                if (!row.Cells[i].Visible)
                    continue;
                recWidth += dataGridView1.GetCellDisplayRectangle(i, row.Index, true).Width;
                if (row.Cells[i].Value != null)
                    recValue += row.Cells[i].Value.ToString() + " ";
            }
            Rectangle newCell = new Rectangle(r1.X, r1.Y, recWidth, r1.Height);
            g.FillRectangle(new SolidBrush(dataGridView1.DefaultCellStyle.BackColor), newCell);
            g.DrawRectangle(p, newCell);
            g.DrawString(recValue, dataGridView1.DefaultCellStyle.Font, new SolidBrush(dataGridView1.DefaultCellStyle.ForeColor), newCell.X + 3, newCell.Y + 3);
        }

        public static Image CopyRectImage(Image image, Rectangle selection)
        {
            int newBottom = selection.Bottom;
            if (selection.Bottom > image.Height)
                newBottom = image.Height;
            int newRight = selection.Right;
            if (selection.Right > image.Width)
                newRight = image.Width;
            // TODO check other borders
            Bitmap RectImage = (image as Bitmap).Clone(new Rectangle(selection.Left, selection.Top, newRight, newBottom), image.PixelFormat);
            return RectImage;
        }

        private void UpdateTableGridRowBackColor(DataGridViewRow row = null)
        {
            if (row == null)
                row = dataGridView.CurrentRow;
            DataRowView dataRowView = (DataRowView)(row.DataBoundItem);
            bool fav = BoolFieldValue("Favorite", dataRowView);
            bool used = (bool) dataRowView.Row["Used"];
            foreach (DataGridViewCell cell in row.Cells)
            {
                if (fav)
                    cell.Style.BackColor = favoriteColor;
                else if (used)
                    cell.Style.BackColor = _usedColor;
                else
                    cell.Style.BackColor = default(Color);
            }
        }

        // for nullable bool fields
        private bool BoolFieldValue(string fieldName, DataRowView dataRowView = null)
        {
            if (dataRowView == null)
                //dataRowView = (DataRowView)(dataGridView.CurrentRow.DataBoundItem);
                dataRowView = (DataRowView)(clipBindingSource.Current);
            bool favVal = false;
            if (dataRowView != null)
            {
                var favVal1 = dataRowView.Row[fieldName];
                favVal = favVal1 != DBNull.Value && (bool) favVal1;
            }
            return favVal;
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm settingsFormForm = new SettingsForm(this);
            keyboardHook.UnregisterHotKeys();
            settingsFormForm.ShowDialog();
            if (settingsFormForm.DialogResult == DialogResult.OK)
                LoadSettings();
            RegisterHotKeys();
        }

        private class ListItemNameText
        {
            public string Name { get; set; }
            public string Text { get; set; }
        }

        private void LoadSettings()
        {
            UpdateControlsStates();
            this.SuspendLayout();
            UpdateCurrentCulture();
            cultureManager1.UICulture = Thread.CurrentThread.CurrentUICulture;

            UpdateWindowTitle(true);

            BindingList<ListItemNameText> comboItemsTypes = (BindingList < ListItemNameText >) TypeFilter.DataSource;
            foreach (ListItemNameText item in comboItemsTypes)
            {
                item.Text = CurrentLangResourceManager.GetString(item.Name);
            }
            // To refresh text in list
            TypeFilter.DisplayMember = "";
            TypeFilter.DisplayMember = "Text";

            BindingList<ListItemNameText> comboItemsMarks = (BindingList<ListItemNameText>) MarkFilter.DataSource;
            foreach (ListItemNameText item in comboItemsMarks)
            {
                item.Text = CurrentLangResourceManager.GetString(item.Name);
            }
            // To refresh text in list
            MarkFilter.DisplayMember = "";
            MarkFilter.DisplayMember = "Text";

            dataGridView.RowsDefaultCellStyle.Font = Properties.Settings.Default.Font;
            ChooseTitleColumnDraw();
            dataGridView.Columns["appImage"].Visible = Properties.Settings.Default.ShowApplicationIconColumn;
            AfterRowLoad();
            this.ResumeLayout();
        }

        private void ChooseTitleColumnDraw()
        {
            bool ResultSimpleDraw = Properties.Settings.Default.ClipListSimpleDraw;
            dataGridView.Columns["TitleSimple"].Visible = ResultSimpleDraw;
            dataGridView.Columns["ColumnTitle"].Visible = !ResultSimpleDraw;
        }

        public async void CheckUpdate(bool UserRequest = false)
        {
            if (!UserRequest && !Properties.Settings.Default.AutoCheckForUpdate)
                return;
            buttonUpdate.Visible = false;
            toolStripUpdateToSeparator.Visible = false;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    string HtmlSource = await wc.DownloadStringTaskAsync(Properties.Resources.Website);

                    // AngileSharp
                    var htmlParser = new HtmlParser();
                    var documentHtml = htmlParser.Parse(HtmlSource);
                    IHtmlCollection<IElement> Refs = documentHtml.GetElementsByClassName("sfdl");
                    string lastVersion = Refs[0].TextContent;

                    // unsuccess to make it by MSHTML
                    //// MsHTML
                    //HTMLDocumentClass htmlDoc = new mshtml.HTMLDocumentClass();
                    //IHTMLDocument2 documentHtml = (IHTMLDocument2)htmlDoc;
                    //documentHtml.write(new object[] { HtmlSource });
                    //documentHtml.close();
                    //string lastVersion = getElementsByTagAndClassName(htmlDoc, "a", "sfdl ")[0].innerText;

                    Match match = Regex.Match(lastVersion, @"Clip Angel (.*).zip");
                    if (match == null)
                        return;
                    ActualVersion = match.Groups[1].Value;
                    if (ActualVersion != Properties.Resources.Version)
                    {
                        buttonUpdate.Visible = true;
                        toolStripUpdateToSeparator.Visible = true;
                        buttonUpdate.ForeColor = Color.Blue;
                        buttonUpdate.Text = CurrentLangResourceManager.GetString("UpdateTo") + " " + ActualVersion;
                        if (UserRequest)
                        {
                            MessageBox.Show(this, CurrentLangResourceManager.GetString("NewVersionAvailable"), Application.ProductName);
                        }
                    }
                    else if (UserRequest)
                    {
                        MessageBox.Show(this, CurrentLangResourceManager.GetString("YouLatestVersion"), Application.ProductName);
                    }
                }
            }
            catch
            {
                if (UserRequest)
                    throw;
            }
        }

        private void RunUpdate()
        {
            using (WebClient wc = new WebClient())
            {
                string tempFolder = Path.GetTempPath() + Guid.NewGuid();
                Directory.CreateDirectory(tempFolder);
                string tempFilenameZip = tempFolder + "\\NewVersion" + ".zip";
                bool success = true;
                //try
                //{
                    wc.DownloadFile(Properties.Resources.DownloadPage, tempFilenameZip);
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show(ex.ToString());
                //    Success = false;
                //}

                //string HtmlSource = wc.DownloadString(Properties.Resources.DownloadPage);
                //var htmlParser = new HtmlParser();
                //var documentHtml = htmlParser.Parse(HtmlSource);
                //IHtmlCollection<IElement> Refs = documentHtml.GetElementsByClassName("direct-download");
                //string DirectLink = Refs[1].GetAttribute("href");
                //wc.DownloadFile(DirectLink, TempFilename);
                string UpdaterName = "ExternalUpdater.exe";
                File.Copy(UpdaterName, tempFolder + "\\" + UpdaterName);
                File.Copy("DotNetZip.dll", tempFolder + "\\DotNetZip.dll");
                if (success)
                {
                    Process.Start(tempFolder + "\\" + UpdaterName, "\"" + tempFilenameZip + "\" \"" + Application.StartupPath + "\" \"" + Application.ExecutablePath
                        + "\" " + Process.GetCurrentProcess().Id);
                    exitToolStripMenuItem_Click();
                }
            }
        }

        private void UpdateCurrentCulture()
        {
            CurrentLangResourceManager = getResourceManager(out Locale);
            // https://www.codeproject.com/Articles/23694/Changing-Your-Application-User-Interface-Culture-O
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Locale);
        }

        static public ResourceManager getResourceManager(out string Locale)
        {
            Locale = getCurrentLocale();
            //if (true
            //    && CurrentLangResourceManager != null
            //    && String.Compare(Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName, Locale, true) != 0)
            //{
            //    MessageBox.Show(this, CurrentLangResourceManager.GetString("LangRestart"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
            ResourceManager LangResourceManager;
            if (String.Compare(Locale, "ru", true) == 0)
                LangResourceManager = Properties.Resource_RU.ResourceManager;
            else
                LangResourceManager = Properties.Resources.ResourceManager;
            return LangResourceManager;
        }

        static public string getCurrentLocale()
        {
            string locale;
            if (Properties.Settings.Default.Language == "Default")
                locale = Application.CurrentCulture.TwoLetterISOLanguageName;
            else if (Properties.Settings.Default.Language == "Russian")
                locale = "ru";
            else
                locale = "en";
            return locale;
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Properties.Settings.Default.Save(); // Not all properties were saved here. For example ShowInTaskbar was not saved
            RemoveClipboardFormatListener(this.Handle);
            UnhookWinEvent(HookChangeActiveWindow);
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            exitToolStripMenuItem_Click();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form aboutBox = new AboutBox();
            aboutBox.ShowDialog(this);
        }

        private void Main_Activated(object sender, EventArgs e)
        {
            //Debug.WriteLine("Activated");
            if (Properties.Settings.Default.FastWindowOpen)
            {
                RestoreWindowIfMinimized();
            }
            SetForegroundWindow(this.Handle);
        }

        private void Filter_KeyPress(object sender, KeyPressEventArgs e)
        {
            // http://csharpcoding.org/tag/keypress/ Workaroud strange beeping 
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
                e.Handled = true;
        }

        private static void OpenLinkIfCtrlPressed(RichTextBox sender, EventArgs e, MatchCollection matches)
        {
            Keys mod = Control.ModifierKeys & Keys.Modifiers;
            bool altOnly = mod == Keys.Alt;
            if (altOnly)
                foreach (Match match in matches)
                {
                    if (match.Index <= sender.SelectionStart && (match.Index + match.Length) >= sender.SelectionStart)
                        Process.Start(match.Value);
                }
        }

        private void textBoxUrl_Click(object sender, EventArgs e)
        {
            OpenLinkIfCtrlPressed(sender as RichTextBox, e, UrlLinkMatches);
        }

        private void ImageControl_DoubleClick(object sender, EventArgs e)
        {
            OpenInDefaultApplication(); 
        }

        private void TypeFilter_SelectedValueChanged(object sender, EventArgs e)
        {
            if (AllowFilterProcessing)
            {
                UpdateClipBindingSource();
            }
        }

        private void buttonFindNext_Click(object sender, EventArgs e)
        {
            if (EditMode)
                return;
            if (TextWasCut)
                AfterRowLoad(true);
            if (htmlMode)
            {
                mshtml.IHTMLTxtRange range = GetHtmlCurrentTextRangeOrAllDocument();
                range.collapse(false);
                if (range.findText(filterText, 1000000000, 0))
                {
                    range.select();
                }
            }
            else
            {
                RichTextBox control = richTextBox;
                if (FilterMatches == null)
                    return;
                foreach (Match match in FilterMatches)
                {
                    if (false
                        || control.SelectionStart < match.Index
                        || (true
                            && control.SelectionLength == 0
                            && match.Index == 0
                            ))
                    {
                        control.SelectionStart = match.Index;
                        control.SelectionLength = match.Length;
                        control.HideSelection = false;
                        break;
                    }
                }
            }
        }

        private mshtml.IHTMLTxtRange GetHtmlCurrentTextRangeOrAllDocument()
        {
            mshtml.IHTMLDocument2 htmlDoc = (mshtml.IHTMLDocument2) htmlTextBox.Document.DomDocument;
            mshtml.IHTMLBodyElement body = htmlDoc.body as mshtml.IHTMLBodyElement;
            mshtml.IHTMLSelectionObject sel = htmlDoc.selection;
            //sel.empty(); // get an empty selection, so we start from the beginning
            mshtml.IHTMLTxtRange range = (mshtml.IHTMLTxtRange) sel.createRange();
            if (range == null)
                range = body.createTextRange();
            return range;
        }

        private void buttonFindPrevious_Click(object sender, EventArgs e)
        {
            if (EditMode)
                return;
            if (TextWasCut)
                AfterRowLoad(true);
            if (htmlMode)
            {
                mshtml.IHTMLTxtRange range = GetHtmlCurrentTextRangeOrAllDocument();
                range.collapse(true);
                if (range.findText(filterText, -1, 0))
                {
                    range.select();
                }
            }
            else
            {
                RichTextBox control = richTextBox;
                if (FilterMatches == null)
                    return;
                Match prevMatch = null;
                foreach (Match match in FilterMatches)
                {
                    if (false
                        || control.SelectionStart > match.Index
                        || (true
                            && control.SelectionLength == 0
                            && match.Index == 0
                        ))
                    {
                        prevMatch = match;
                    }
                }
                if (prevMatch != null)
                {
                    control.SelectionStart = prevMatch.Index;
                    control.SelectionLength = prevMatch.Length;
                    control.HideSelection = false;
                }
            }
        }

        private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.WordWrap = !Properties.Settings.Default.WordWrap;
            allowTextPositionChangeUpdate = false;
            UpdateControlsStates();
            allowTextPositionChangeUpdate = true;
            UpdateSelectionPosition();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(CurrentLangResourceManager.GetString("HelpPage"));
        }

        private void copyClipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //DataObject oldDataObject;
            CopyClipToClipboard(/*out oldDataObject*/);
            if (Properties.Settings.Default.MoveCopiedClipToTop)
                GetClipboardData();
        }

        private void toolStripButtonSelectTopClipOnShow_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.SelectTopClipOnShow = !Properties.Settings.Default.SelectTopClipOnShow;
            UpdateControlsStates();
        }
        private void UpdateControlsStates()
        {
            trayMenuItemMonitoringClipboard.Checked = MonitoringClipboard;
            toolStripMenuItemMonitoringClipboard.Checked = MonitoringClipboard;
            toolStripButtonTextFormatting.Checked = Properties.Settings.Default.ShowNativeTextFormatting;
            textFormattingToolStripMenuItem.Checked = Properties.Settings.Default.ShowNativeTextFormatting;
            toolStripButtonMonospacedFont.Checked = Properties.Settings.Default.MonospacedFont;
            monospacedFontToolStripMenuItem.Checked = Properties.Settings.Default.MonospacedFont;
            selectTopClipOnShowToolStripMenuItem.Checked = Properties.Settings.Default.SelectTopClipOnShow;
            toolStripButtonSelectTopClipOnShow.Checked = Properties.Settings.Default.SelectTopClipOnShow;
            wordWrapToolStripMenuItem.Checked = Properties.Settings.Default.WordWrap;
            toolStripButtonWordWrap.Checked = Properties.Settings.Default.WordWrap;
            dataGridView.Columns["VisualWeight"].Visible = Properties.Settings.Default.ShowVisualWeightColumn;
            richTextBox.WordWrap = wordWrapToolStripMenuItem.Checked;
            showInTaskbarToolStripMenuItem.Enabled = Properties.Settings.Default.FastWindowOpen;
            showInTaskbarToolStripMenuItem.Checked = Properties.Settings.Default.ShowInTaskBar;
            if (Properties.Settings.Default.FastWindowOpen)
            {
                this.ShowInTaskbar = Properties.Settings.Default.ShowInTaskBar;
                // After ShowInTaskbar change true->false all window properties are deleted. So we need to reset it.
                ResetIsMainProperty();
            }
        }

        private void toolStripMenuItemClearFilterAndSelectTop_Click(object sender, EventArgs e)
        {
            ClearFilter(-1);
            dataGridView.Focus();
        }

        private void changeClipTitleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null)
            {
                string oldTitle = RowReader["Title"] as string;
                InputBoxResult inputResult = InputBox.Show(CurrentLangResourceManager.GetString("HowUseAutoTitle"), CurrentLangResourceManager.GetString("EditClipTitle"), oldTitle, this);
                if (inputResult.ReturnCode == DialogResult.OK)
                {
                    string sql = "Update Clips set Title=@Title where Id=@Id";
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    command.Parameters.AddWithValue("@Id", RowReader["Id"]);
                    string newTitle;
                    if (inputResult.Text == "")
                        newTitle = TextClipTitle(RowReader["text"].ToString());
                    else
                        newTitle = inputResult.Text;
                    command.Parameters.AddWithValue("@Title", newTitle);
                    command.ExecuteNonQuery();
                    UpdateClipBindingSource();
                }
            }
        }

        private void setFavoriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetRowMark("Favorite", true, true);
        }

        private void resetFavoriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetRowMark("Favorite", false, true);
        }

        private void MarkFilter_SelectedValueChanged(object sender, EventArgs e)
        {
            if (AllowFilterProcessing)
            {
                UpdateClipBindingSource(); 
            }
        }

        private void showAllMarksToolStripMenuItem_Click(object sender = null, EventArgs e = null)
        {
            MarkFilter.SelectedValue = "allMarks";
        }

        private void showOnlyUsedToolStripMenuItem_Click(object sender = null, EventArgs e = null)
        {
            MarkFilter.SelectedValue = "used";
        }

        private void showOnlyFavoriteToolStripMenuItem_Click(object sender = null, EventArgs e = null)
        {
            MarkFilter.SelectedValue = "favorite";
        }

        private void dataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            if (true
                && !IsKeyPassedFromFilterToGrid(e.KeyCode, e.Control)
                && e.KeyCode != Keys.Delete
                && e.KeyCode != Keys.Home
                && e.KeyCode != Keys.End
                && e.KeyCode != Keys.Enter
                && e.KeyCode != Keys.ShiftKey
                && e.KeyCode != Keys.Alt
                && e.KeyCode != Keys.Menu
                && e.KeyCode != Keys.Tab
                && !e.Control
                //&& e.KeyCode != Keys.F1
                //&& e.KeyCode != Keys.F2
                //&& e.KeyCode != Keys.F3
                //&& e.KeyCode != Keys.F4
                //&& e.KeyCode != Keys.F5
                //&& e.KeyCode != Keys.F6
                //&& e.KeyCode != Keys.F7
                //&& e.KeyCode != Keys.F8
                //&& e.KeyCode != Keys.F9
                //&& e.KeyCode != Keys.F10
                //&& e.KeyCode != Keys.F11
                //&& e.KeyCode != Keys.F12
                //&& !e.Alt
                )
            {
                comboBoxFilter.Focus();
                sendKey(comboBoxFilter.Handle, e.KeyData, false, true);
                e.Handled = true;
            }
            //else if (e.KeyCode == Keys.Tab)
            //{
            //    // Tired of trying to make it with TAB order
            //    richTextBox.Focus();
            //    e.Handled = true;
            //}
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            RunUpdate();
        }

        private void checkUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckUpdate(true);
        }

        private void timerCheckUpdate_Tick(object sender, EventArgs e)
        {
            CheckUpdate();
            timerCheckUpdate.Interval = (1000 * 60 * 60 * 24); // 1 day
            timerCheckUpdate.Start();
        }

        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RowShift(-1);
        }

        // indexShift - 0 - TOP
        //              -1 - UP
        //              1 - DWON
        private void RowShift(int indexShift)
        {
            if (dataGridView.CurrentRow == null)
                return;
            int currentRowIndex = dataGridView.CurrentRow.Index;
            if (false
                || indexShift <= 0 && currentRowIndex == 0
                || indexShift > 0 && currentRowIndex == dataGridView.RowCount
               )
                return;
            int newID;
            if (indexShift != 0)
            {
                DataRow nearDataRow = ((DataRowView) clipBindingSource[currentRowIndex + indexShift]).Row;
                newID = (int) nearDataRow["ID"];
            }
            else
            {
                LastId++;
                newID = LastId;
                //indexShift = -currentRowIndex - 1;
            }
            DataRow currentDataRow = ((DataRowView)clipBindingSource[currentRowIndex]).Row;
            int oldID = (int)currentDataRow["ID"];
            int tempID = LastId + 1;
            string sql = "Update Clips set Id=@NewId where Id=@Id";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            if (newID != tempID)
            {
                command.Parameters.AddWithValue("@Id", newID);
                command.Parameters.AddWithValue("@NewID", tempID);
                command.ExecuteNonQuery();
            }
            command.Parameters.AddWithValue("@Id", oldID);
            command.Parameters.AddWithValue("@NewID", newID);
            command.ExecuteNonQuery();
            if (newID != tempID)
            {
                command.Parameters.AddWithValue("@Id", tempID);
                command.Parameters.AddWithValue("@NewID", oldID);
                command.ExecuteNonQuery();
            }
            //clipBindingSource.Position = currentRowIndex + indexShift;
            UpdateClipBindingSource(false, newID);
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RowShift(1);
        }

        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(CurrentLangResourceManager.GetString("HistoryOfChanges")); // Returns 0. Why?
        }

        private void toolStripMenuItemPasteChars_Click(object sender, EventArgs e)
        {
            SendPasteClip(PasteMethod.SendChars);
         }

        private void openInDefaultApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenInDefaultApplication();
        }

        private void OpenInDefaultApplication()
        {
            if (RowReader == null)
                return;
            string type = RowReader["type"].ToString();
            //string TempFile = Path.GetTempFileName();
            SQLiteDataReader rowReader = RowReader;
            var tempFile = clipTempFile(rowReader, "copy");
            if (tempFile == "")
            {
                MessageBox.Show(this, CurrentLangResourceManager.GetString("ClipFileAlreadyOpened"));
                return;
            }
            bool deleteAfterOpen = false;
            if (type == "text" || type == "file")
            {
                File.WriteAllText(tempFile, RowReader["text"].ToString(), Encoding.Default);
                deleteAfterOpen = true;
            }
            else if (type == "rtf")
            {
                RichTextBox rtb = new RichTextBox();
                rtb.Rtf = RowReader["richText"].ToString();
                rtb.SaveFile(tempFile);
                deleteAfterOpen = true;
            }
            else if (type == "html")
            {
                File.WriteAllText(tempFile, GetHtmlFromHtmlClipText(), Encoding.Default);
                deleteAfterOpen = true;
            }
            else if (type == "img")
            {
                ImageControl.Image.Save(tempFile);
                deleteAfterOpen = true;
            }
            else if (type == "file")
            {
                string[] tokens = Regex.Split(RowReader["text"].ToString(), @"\r?\n|\r");
                tempFile = tokens[0];
                if (!File.Exists(tempFile))
                    tempFile = "";
            }
            if (tempFile != "")
            {
                try
                {
                    Process.Start(tempFile);
                    if (deleteAfterOpen)
                    {
                        //Thread.Sleep(1000); // to be almost sure that file has been opened
                        //File.Delete(tempFile);
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        private string clipTempFile(SQLiteDataReader rowReader, string suffix = "")
        {
            string extension;
            string type = rowReader["type"].ToString();
            if (type == "text" || type == "file")
                extension = "txt";
            else if (type == "rtf" || type == "html")
                extension = type;
            else if (type == "img")
                extension = "bmp";
            else
                extension = "dat";
            string tempFile = Path.GetTempPath() + "Clip " + rowReader["id"] + " " + suffix + "." + extension;
            try
            {
                using (new StreamWriter(tempFile))
                { }
            }
            catch
            {
                //tempFile = Path.GetTempPath() + new Guid() + "." + extension;
                tempFile = "";
            }
            return tempFile;
        }

        private void windowAlwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.TopMost = !this.TopMost;
            windowAlwaysOnTopToolStripMenuItem.Checked = this.TopMost;
            toolStripButtonTopMostWindow.Checked = this.TopMost;
        }

        private void editClipTextToolStripMenuItem_Click(object sender = null, EventArgs e = null)
        {
            if (RowReader == null)
                return;
            string clipType = RowReader["type"].ToString();
            if (!IsTextType())
                return;
            //selectionStart = richTextBox.SelectionStart;
            //selectionLength = richTextBox.SelectionLength;
            bool newEditMode = !EditMode;
            allowRowLoad = false;
            if (!newEditMode)
                SaveClipText();
            else
            {
                if (clipType != "text")
                {
                    AddClip(null, null, "", "", "text", RowReader["text"].ToString());
                    GotoLastRow();
                }
            }
            UpdateClipBindingSource();
            allowRowLoad = true;
            EditMode = newEditMode;
            AfterRowLoad(true, -1);
            editClipTextToolStripMenuItem.Checked = EditMode;
            toolStripMenuItemEditClipText.Checked = EditMode;
            pasteENTERToolStripMenuItem.Enabled = !EditMode;
        }

        private void timerReconnect_Tick(object sender, EventArgs e)
        {
            ConnectClipboard();
        }

        private void dataGridView_MouseHover(object sender, EventArgs e)
        {
            //Point clientPos = dataGridView.PointToClient(Control.MousePosition);
            //DataGridView.HitTestInfo hitInfo = dataGridView.HitTest(clientPos.X, clientPos.Y);
            //if (hitInfo.Type == (DataGridViewHitTestType) DataGrid.HitTestType.Cell)
            //{
            //    if (hitInfo.ColumnIndex == dataGridView.Columns["VisualWeight"].Index)
            //    {
            //        DataGridViewCell hoverCell = dataGridView.Rows[hitInfo.RowIndex].Cells[hitInfo.ColumnIndex];
            //        hoverCell.ToolTipText = CurrentLangResourceManager.GetString("VisualWeightTooltip"); // No effect
            //    }
            //}
        }

        private void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var row = dataGridView.Rows[e.RowIndex];
            if (e.ColumnIndex == dataGridView.Columns["VisualWeight"].Index)
            {
                DataGridViewCell hoverCell = row.Cells[e.ColumnIndex];
                if (hoverCell.Value != null)
                    hoverCell.ToolTipText = CurrentLangResourceManager.GetString("VisualWeightTooltip"); // No effect
            }
        }

        private void timerApplyTextFiler_Tick(object sender, EventArgs e)
        {
            TextFilterApply();
            timerApplyTextFiler.Stop();
        }

        private void dataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                dataGridView.CurrentCell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                //dataGridView.Rows[e.RowIndex].Selected = true;
                //dataGridView.Focus();
            }
        }

        private void dataGridView_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = dataGridView.Rows[e.RowIndex];
            if (row.Cells["ColumnTitle"].Value == null)
            {
                PrepareRow(row);
                e.PaintCells(e.ClipBounds, DataGridViewPaintParts.All);
                e.Handled = true;
            }
        }

        private void richTextBox_SelectionChanged(object sender = null, EventArgs e = null)
        {
            if (!allowTextPositionChangeUpdate)
                return;
            selectionStart = richTextBox.SelectionStart;
            if (selectionStart > richTextBox.Text.Length)
                selectionStart = richTextBox.Text.Length;
            int line = richTextBox.GetLineFromCharIndex(selectionStart);
            int lineStart = richTextBox.GetFirstCharIndexFromLine(line);
            string strLine = richTextBox.Text.Substring(lineStart, selectionStart - lineStart);
            char tab = '\u0009'; // TAB
            var TabSpace = new String(' ', tabLength);
            strLine = strLine.Replace(tab.ToString(), TabSpace);
            int column = strLine.Length + 1;
            line++;
            //if (richTextBox.Text.Length - MultiLangEndMarker().Length < (int)RowReader["chars"])
            //    selectionStart += line - 1; // to take into account /r/n vs /n
            selectionLength = richTextBox.SelectionLength;
            UpdateTextPositionIndicator(line, column);
        }

        private void UpdateTextPositionIndicator(int line, int column)
        {
            string newText;
            newText = "" + selectionStart;
            newText += "(" + line + ":" + column + ")";
            if (selectionLength > 0)
            {
                newText += "+" + selectionLength;
            }
            stripLabelPosition.Text = newText;
            //StripLabelPositionXY.Text = NewText;
        }

        private void toolStripButtonFixedWidthFont_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.MonospacedFont = !Properties.Settings.Default.MonospacedFont;
            UpdateControlsStates();
            AfterRowLoad();
        }

        private void MonitoringClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwitchMonitoringClipboard();
        }

        private void SwitchMonitoringClipboard()
        {
            MonitoringClipboard = !MonitoringClipboard;
            if (MonitoringClipboard)
                ConnectClipboard();
            else
                RemoveClipboardFormatListener(this.Handle);
            UpdateControlsStates();
        }

        private void translateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedText = getSelectedOrAllText();
            if (IsTextType())
                Process.Start("https://translate.google.com/?tl=en#auto/en/" + selectedText);
        }

        private string getSelectedOrAllText()
        {
            string selectedText = richTextBox.SelectedText;
            if (selectedText == "")
                selectedText = RowReader["text"].ToString();
            return selectedText;
        }

        private void textCompareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string type1;
            int id1, id2;
            if (dataGridView.SelectedRows.Count == 0)
                return;
            if (dataGridView.SelectedRows.Count == 1)
            {
                type1 = RowReader["type"].ToString();
                if (!IsTextType() && type1 != "file")
                    return;
                if (_lastSelectedForCompareId == 0)
                {
                    _lastSelectedForCompareId = (int)RowReader["id"];
                    //MessageBox.Show("Now execute this command on the second clip to compare", Application.ProductName);
                    return;
                }
                else
                {
                    id1 = (int)RowReader["id"];
                    id2 = _lastSelectedForCompareId;
                    _lastSelectedForCompareId = 0;
                }
            }
            else //if (dataGridView.SelectedRows.Count > 1)
            {
                DataRowView row1 = (DataRowView) dataGridView.SelectedRows[0].DataBoundItem;
                id1 = (int)row1["id"];
                DataRowView row2 = (DataRowView)dataGridView.SelectedRows[1].DataBoundItem;
                id2 = (int)row2["id"];
            }
            CompareClipsbyID(id1, id2);
        }

        private void CompareClipsbyID(int id1, int id2)
        {
            string comparatorName = comparatorExeFileName();
            if (comparatorName == "")
            {
                return;
            }
            string type1;
            string type2;
            var rowReader1 = getRowReader(id1);
            var rowReader2 = getRowReader(id2);
            type1 = rowReader1["type"].ToString();
            type2 = rowReader2["type"].ToString();
            if (!IsTextType(type1) && type1 != "file")
                return;
            if (!IsTextType(type2) && type2 != "file")
                return;
            string filename1 = clipTempFile(rowReader1, "comp");
            File.WriteAllText(filename1, rowReader1["text"].ToString(), Encoding.Default);
            string filename2 = clipTempFile(rowReader2, "comp");
            File.WriteAllText(filename2, rowReader2["text"].ToString(), Encoding.Default);
            Process.Start(comparatorName, String.Format("\"{0}\" \"{1}\"", filename1, filename2));
        }

        string comparatorExeFileName()
        {
            // TODO read paths from registry and let use custom application
            string path;
            path = Properties.Settings.Default.TextCompareApplication;
            if (path != "" && File.Exists(path))
            {
                return path;
            }

            path = "C:\\Program Files (x86)\\Beyond Compare 3\\BCompare.exe";
            if (File.Exists(path))
            {
                return path;
            }

            path = "C:\\Program Files\\Beyond Compare 3\\BCompare.exe";
            if (File.Exists(path))
            {
                return path;
            }

            path = "C:\\Program Files (x86)\\ExamDiff Pro\\ExamDiff.exe";
            if (File.Exists(path))
            {
                return path;
            }

            path = "C:\\Program Files\\ExamDiff Pro\\ExamDiff.exe";
            if (File.Exists(path))
            {
                return path;
            }

            path = "C:\\Program Files (x86)\\WinMerge\\WinMergeU.exe";
            if (File.Exists(path))
            {
                return path;
            }

            path = "C:\\Program Files (x86)\\Araxis\\Araxis Merge\\compare.exe";
            if (File.Exists(path))
            {
                return path;
            }

            path = "C:\\Program Files\\Araxis\\Araxis Merge\\compare.exe";
            if (File.Exists(path))
            {
                return path;
            }

            path = "C:\\Program Files (x86)\\SourceGear\\Common\\DiffMerge\\sgdm.exe";
            if (File.Exists(path))
            {
                return path;
            }

            path = "C:\\Program Files\\SourceGear\\Common\\DiffMerge\\sgdm.exe";
            if (File.Exists(path))
            {
                return path;
            }

            MessageBox.Show("No supported text compare application found. Will open website to get free one.", Application.ProductName);
            Process.Start("http://winmerge.org/");
            return "";
        }

        private void toolStripButtonTextFormatting_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowNativeTextFormatting = !Properties.Settings.Default.ShowNativeTextFormatting;
            UpdateControlsStates();
            string clipType = RowReader["type"].ToString();
            if (clipType == "html" || clipType == "rtf")
                AfterRowLoad();
        }

        private void toolStripButtonMarkFavorite_Click(object sender, EventArgs e)
        {
            if (RowReader == null)
                return;
            SetRowMark("Favorite", !BoolFieldValue("Favorite"));
        }

        private void htmlTextBox_DocumentCompleted(object sender = null, WebBrowserDocumentCompletedEventArgs e = null)
        {
            //mshtml.IHTMLDocument2 htmlDoc = htmlTextBox.Document.DomDocument as mshtml.IHTMLDocument2;
            //mshtml.HTMLDocumentEvents2_Event iEvent = (mshtml.HTMLDocumentEvents2_Event)htmlDoc;
            ////htmlDoc.body.setAttribute("contentEditable", true); // Something changes inside htmlDoc in first switch contentEditable = true
            ////htmlDoc.body.setAttribute("contentEditable", false);
            //iEvent.onclick += new mshtml.HTMLDocumentEvents2_onclickEventHandler(htmlTextBoxDocumentClick);
            //iEvent.onselectionchange += new mshtml.HTMLDocumentEvents2_onselectionchangeEventHandler(htmlTextBoxDocumentSelectionChange);
            //htmlTextBox.Document.AttachEventHandler("onSelectionChange", htmlTextBoxDocumentSelectionChange);
            //htmlTextBox.Document.MouseDown += new HtmlElementEventHandler(htmlTextBoxMouseDown);
            //htmlTextBox.Document.MouseUp += new HtmlElementEventHandler(htmlTextBoxMouseUp);
            //htmlTextBox.Document.MouseMove += new HtmlElementEventHandler(htmlTextBoxMouseMove);
            //htmlTextBox.Document.MouseOver += new HtmlElementEventHandler(htmlTextBoxMouseOver);
            //htmlTextBox.Document.Click += new HtmlElementEventHandler(htmlTextBoxClick);
        }

        private void htmlTextBoxDocumentKeyDown(Object sender, HtmlElementEventArgs e)
        {
            e.ReturnValue = false
                || e.CtrlKeyPressed
                || e.AltKeyPressed
                || e.KeyPressedCode == (int)Keys.Down
                || e.KeyPressedCode == (int)Keys.Up
                || e.KeyPressedCode == (int)Keys.PageDown
                || e.KeyPressedCode == (int)Keys.PageUp
                || e.KeyPressedCode == (int)Keys.Home
                || e.KeyPressedCode == (int)Keys.End
                ;
        }

        private bool htmlTextBoxDocumentClick(mshtml.IHTMLEventObj e)
        {
            if (e.altKey)
            {
                openLinkInBrowserToolStripMenuItem_Click();
            }
            return false;
        }

        private void htmlTextBoxDocumentSelectionChange(Object sender = null, EventArgs e = null)
        //private void htmlTextBoxDocumentSelectionChange(mshtml.IHTMLEventObj e = null)
        {
            if (!allowTextPositionChangeUpdate)
                return;
            mshtml.IHTMLDocument2 htmlDoc = htmlTextBox.Document.DomDocument as mshtml.IHTMLDocument2;
            mshtml.IHTMLTxtRange range = (IHTMLTxtRange) htmlDoc.selection.createRange();
            if (range == null)
                return;
            selectionLength = 0;
            if (!String.IsNullOrEmpty(range.text))
                selectionLength = range.text.Length;
            range.collapse();
            range.moveStart("character", -100000);
            selectionStart = 0;
            if (!String.IsNullOrEmpty(range.text))
                selectionStart = range.text.Length;
            string innerText = htmlDoc.body.innerText;
            if (!String.IsNullOrEmpty(innerText))
                if (selectionStart > innerText.Length)
                    selectionStart = innerText.Length;
            UpdateTextPositionIndicator(0, 0);
        }

        private void htmlTextBoxMouseMove(Object sender, HtmlElementEventArgs e)
        {
            //e.ReturnValue = false;
        }
        private void htmlTextBoxMouseDown(IHTMLEventObj pEvtObj)
        {
            lastClickedHtmlElement = htmlTextBox.Document.GetElementFromPoint(htmlTextBox.PointToClient(MousePosition));
            bool isLink = (String.Compare(lastClickedHtmlElement.TagName, "A", true) == 0);
            htmlMenuItemCopyLinkAdress.Enabled = isLink;
            htmlMenuItemOpenLink.Enabled = isLink;
        }
        private void htmlTextBoxDrag(Object sender, HtmlElementEventArgs e)
        {
            e.ReturnValue = false;
        }

        private void copyLinkAdressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string href = lastClickedHtmlElement.GetAttribute("href");
            Clipboard.Clear();
            Clipboard.SetText(href, TextDataFormat.UnicodeText);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            htmlTextBox.Document.ExecCommand("SelectAll", false, null);
        }

        private void openLinkInBrowserToolStripMenuItem_Click(object sender = null, EventArgs e = null)
        {
            string href = lastClickedHtmlElement.GetAttribute("href");
            Process.Start(href);
        }

        private void rtfMenuItemSelectAll_Click(object sender, EventArgs e)
        {
            if (EditMode)
                richTextBox.SelectAll();
            else
                richTextBox.Select(0, clipRichTextLength);
        }

        private void showInTaskbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowInTaskBar = !Properties.Settings.Default.ShowInTaskBar;
            UpdateControlsStates();
        }

        private void moveTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RowShift(0);
        }

        private void clearClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.ShowNativeTextFormatting)
                Clipboard.SetText(richTextBox.SelectedRtf, TextDataFormat.Rtf);
            else
                Clipboard.SetText(richTextBox.SelectedText, TextDataFormat.UnicodeText);
        }
    }
}


public sealed class KeyboardHook : IDisposable
{
    // http://stackoverflow.com/questions/2450373/set-global-hotkeys-using-c-sharp

    // Registers a hot key with Windows.
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    // Unregisters the hot key with Windows.
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private ResourceManager resourceManager;

    /// <summary>
    /// Represents the window that is used internally to get the messages.
    /// </summary>
    private sealed class Window : NativeWindow, IDisposable
    {
        private const int WM_HOTKEY = 0x0312;

        public Window()
        {
            // create the handle for the window.
            this.CreateHandle(new CreateParams());
        }

        /// <summary>
        /// Overridden to get the notifications.
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // check if we got a hot key pressed.
            if (m.Msg == WM_HOTKEY)
            {
                // get the keys.
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                EnumModifierKeys modifier = (EnumModifierKeys)((int)m.LParam & 0xFFFF);

                // invoke the event to notify the parent.
                if (KeyPressed != null)
                    KeyPressed(this, new KeyPressedEventArgs(modifier, key));
            }
        }

        public event EventHandler<KeyPressedEventArgs> KeyPressed;

        #region IDisposable Members

        public void Dispose()
        {
            this.DestroyHandle();
        }

        #endregion
    }

    private Window _window = new Window();
    private int _currentId;

    public KeyboardHook(ResourceManager resourceManager)
    {
        this.resourceManager = resourceManager;
        // register the event of the inner native window.
        _window.KeyPressed += delegate (object sender, KeyPressedEventArgs args)
        {
            if (KeyPressed != null)
                KeyPressed(this, args);
        };
    }

    public void UnregisterHotKeys()
    {
        // unregister all the registered hot keys.
        for (int i = _currentId; i > 0; i--)
        {
            UnregisterHotKey(_window.Handle, i);
        }
    }

    /// <summary>
    /// Registers a hot key in the system.
    /// </summary>
    /// <param name="modifier">The modifiers that are associated with the hot key.</param>
    /// <param name="key">The key itself that is associated with the hot key.</param>
    public void RegisterHotKey(EnumModifierKeys modifier, Keys key)
    {
        // increment the counter.
        _currentId = _currentId + 1;

        // register the hot key.
        if (!RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key))
        {
            string hotkeyTitle = HotkeyTitle(key, modifier);
            string errorText = resourceManager.GetString("CouldNotRegisterHotkey") + " " + hotkeyTitle;
            //throw new InvalidOperationException(ErrorText);
            MessageBox.Show(errorText, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static string HotkeyTitle(Keys key, EnumModifierKeys modifier)
    {
        string hotkeyTitle = "";
        if ((modifier & EnumModifierKeys.Win) != 0)
            hotkeyTitle += Keys.Control.ToString() + " + ";
        if ((modifier & EnumModifierKeys.Control) != 0)
            hotkeyTitle += Keys.Control.ToString() + " + ";
        if ((modifier & EnumModifierKeys.Alt) != 0)
            hotkeyTitle += Keys.Alt.ToString() + " + ";
        if ((modifier & EnumModifierKeys.Shift) != 0)
            hotkeyTitle += Keys.Shift.ToString() + " + ";
        hotkeyTitle += key.ToString();
        return hotkeyTitle;
    }

    /// <summary>
    /// A hot key has been pressed.
    /// </summary>
    public event EventHandler<KeyPressedEventArgs> KeyPressed;

    #region IDisposable Members

    public void Dispose()
    {
        UnregisterHotKeys();
        // dispose the inner native window.
        _window.Dispose();
    }

    #endregion
}

/// <summary>
/// Event Args for the event that is fired after the hot key has been pressed.
/// </summary>
public class KeyPressedEventArgs : EventArgs
{
    private EnumModifierKeys _modifier;
    private Keys _key;

    internal KeyPressedEventArgs(EnumModifierKeys modifier, Keys key)
    {
        _modifier = modifier;
        _key = key;
    }

    public EnumModifierKeys Modifier
    {
        get { return _modifier; }
    }

    public Keys Key
    {
        get { return _key; }
    }
}

/// <summary>
/// The enumeration of possible modifiers.
/// </summary>
[Flags]
public enum EnumModifierKeys : uint
{
    Alt = 1,
    Control = 2,
    Shift = 4,
    Win = 8
}

// Solution for casesensitivity in SQLite http://www.cyberforum.ru/ado-net/thread1708878.html
namespace ASC.Data.SQLite
{

    /// <summary>
    /// Класс переопределяет функцию Lower() в SQLite, т.к. встроенная функция некорректно работает с символами > 128
    /// </summary>
    [SQLiteFunction(Name = "lower", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class LowerFunction : SQLiteFunction
    {

        /// <summary>
        /// Вызов скалярной функции Lower().
        /// </summary>
        /// <param name="args">Параметры функции</param>
        /// <returns>Строка в нижнем регистре</returns>
        public override object Invoke(object[] args)
        {
            if (args.Length == 0 || args[0] == null) return null;
            return ((string)args[0]).ToLower();
        }
    }

    /// <summary>
    /// Класс переопределяет функцию Upper() в SQLite, т.к. встроенная функция некорректно работает с символами > 128
    /// </summary>
    [SQLiteFunction(Name = "upper", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class UpperFunction : SQLiteFunction
    {

        /// <summary>
        /// Вызов скалярной функции Upper().
        /// </summary>
        /// <param name="args">Параметры функции</param>
        /// <returns>Строка в верхнем регистре</returns>
        public override object Invoke(object[] args)
        {
            if (args.Length == 0 || args[0] == null) return null;
            return ((string)args[0]).ToUpper();
        }
    }
}
