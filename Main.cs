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
using System.Linq;
using System.Linq.Expressions;
using System.Media;
using System.Security.Cryptography;
using System.Resources;
using System.Net;
using System.Net.Http;
using AngleSharp.Parser.Html;
using AngleSharp.Dom;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.UI.Design;
using System.Xml;
using System.Xml.Linq;
using WindowsInput.Native;
using mshtml;
//using Word = Microsoft.Office.Interop.Word;
using static IconTools;
using Timer = System.Windows.Forms.Timer;

namespace ClipAngel
{
    enum PasteMethod
    {
        Standart,
        PasteText,
        SendChars,
        File,
        Null
    };

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
        int LastId = 0;
        MatchCollection TextLinkMatches;
        MatchCollection UrlLinkMatches;
        MatchCollection FilterMatches;
        string DataFormat_ClipboardViewerIgnore = "Clipboard Viewer Ignore";
        string DataFormat_XMLSpreadSheet = "XML SpreadSheet";
        string ActualVersion;
        //DateTime lastAutorunUpdateCheck;
        int MaxTextViewSize = 5000;
        bool TextWasCut;
        KeyboardHook keyboardHook;
        WinEventDelegate dele = null;
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;
        static private IntPtr lastActiveParentWindow;
        static private IntPtr lastChildWindow;
        static private RECT lastChildWindowRect;
        //static private string lastWindowSelectedText;
        static Point lastCaretPoint;
        private IntPtr HookChangeActiveWindow;
        private bool AllowFilterProcessing = true;
        private static Color favoriteColor = Color.FromArgb(255, 230, 220);
        private static Color _usedColor = Color.FromArgb(210, 255, 255);
        Bitmap imageText;
        Bitmap imageHtml;
        Bitmap imageRtf;
        Bitmap imageFile;
        Bitmap imageImg;
        string filterText = ""; // TODO optimize speed
        bool periodFilterOn = false;
        int tabLength = 4;
        readonly RichTextBox _richTextBox = new RichTextBox();
        static Dictionary<string, Bitmap> originalIconCache = new Dictionary<string, Bitmap>();
        static Dictionary<string, Bitmap> brightIconCache = new Dictionary<string, Bitmap>();
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
        private StringCollection ignoreModulesInClipCapture;
        static Dictionary<string, object> clipboardContents = new Dictionary<string, object>();
        private bool UsualClipboardMode = false;
        private List<int> selectedClipsBeforeFilterApply = new List<int>();
        private Point lastMousePoint;
        private int maxWindowCoordForHiddenState = -10000;
        private Color[] _wordColors = new Color[] { Color.Red, Color.DeepPink, Color.DarkOrange };
        private DateTime lastCaptureMoment = DateTime.Now;
        private DateTime lastPasteMoment = DateTime.Now;
        private Dictionary <int, DateTime> lastPastedClips = new Dictionary<int, DateTime>();
        private bool lastClipWasMultiCaptured = false;
        private Point LastMousePoint;
        private Timer captureTimer = new Timer();
        private Thread updateDBThread;
        private bool stopUpdateDBThread = false;
        private int selectedRangeStart = -1;
        private bool allowProcessDataGridSelectionChanged = true;
        private Point PreviousCursor;
        private bool titleToolTipShown = false;
        private ToolTip titleToolTip = new ToolTip();
        private Timer titleToolTipBeforeTimer = new Timer();
        string sortField = "Id";
        private static string timePattern = "\\b[012]?\\d:[0-5]?\\d(?::[0-5]?\\d)?\\b";
        private static string datePattern = "\\b(?:19|20)?[0-9]{2}[\\-/.][0-9]{2}[\\-/.](?:19|20)?[0-9]{2}\\b";
        static private Dictionary<string, string> TextPatterns = new Dictionary<string, string>
        {
            {"time", "((" + datePattern + "\\s"+ timePattern + ")|(?:(" + timePattern + "\\s)?"+ datePattern + ")|(?:"+ timePattern + "))"},
            {"email", "(\\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,6}\\b)"},
            //{"number", "(?:[\\s\n\r\\(<>\\[]|^)([-+]?[0-9]+\\.?[0-9]+)(?:[;\\s\\)<>\\]%]|,\\B|$)"},
            {"number", "((?:(?:\\s|^)[-])?\\b[0-9]+\\.?[0-9]+)\\b"},
            {"phone", "(?:[\\s\\(]|^)(\\+?\\b\\d?(\\d[ \\-\\(\\)]{0,2}){7,19}\\b)"},
            {"url", "(\\b(?:https?|ftp|file)://[-A-Z0-9+&@#\\\\/%?=~_|!:,.;]*[A-Z0-9+&@#/%=~_|])"}
        };
        static string LinkPattern = TextPatterns["url"];

        //[DllImport("dwmapi", PreserveSig = true)]
        //static extern int DwmSetWindowAttribute(IntPtr hWnd, int attr, ref int value, int attrLen);

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

            // Title tooltip
            titleToolTip.AutoPopDelay = 3000;
            titleToolTipBeforeTimer.Interval = 500;
            titleToolTipBeforeTimer.Tick += delegate (object sender, EventArgs e)
            {
                titleToolTipBeforeTimer.Stop();
                string text = Application.ProductName + String.Format(" <{0}> >> <{1}> [<{2}>]", CurrentLangResourceManager.GetString("Version"), 
                    CurrentLangResourceManager.GetString("TargetWindow"), CurrentLangResourceManager.GetString("TargetApp"));
                titleToolTip.Show(text, this, this.PointToClient(Cursor.Position), titleToolTip.AutoPopDelay);
                titleToolTipShown = true;
            };
            GlobalMouseHandler gmh = new GlobalMouseHandler();
            gmh.TheMouseMoved += new MouseMovedEvent(HideTitleTooltip);
            Application.AddMessageFilter(gmh);

            toolStripSearchOptions.DropDownDirection = ToolStripDropDownDirection.AboveRight;
            dele = new WinEventDelegate(WinEventProc);
            HookChangeActiveWindow = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele,
                0, 0, WINEVENT_OUTOFCONTEXT);
            // register the event that is fired after the key press.
            keyboardHook = new KeyboardHook(CurrentLangResourceManager);
            keyboardHook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            ImageControl.MouseWheel += new MouseEventHandler(ImageControl_MouseWheel);
            captureTimer.Tick += delegate { CaptureClipboardData(); };
            RegisterHotKeys();
            toolStripTop.Renderer = new MyToolStripRenderer();
            toolStripBottom.Renderer = new MyToolStripRenderer();

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
                .GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            aProp.SetValue(dataGridView, true, null);
            //aProp.SetValue(richTextBox, true, null); // No effect
            //aProp.SetValue(htmlTextBox, true, null); // No effect

            BindingList<ListItemNameText> _comboItemsTypes = new BindingList<ListItemNameText>
            {
                new ListItemNameText {Name = "allTypes"},
                new ListItemNameText {Name = "img"},
                new ListItemNameText {Name = "file"},
                new ListItemNameText {Name = "text"},
                new ListItemNameText {Name = "rtf"},
                new ListItemNameText {Name = "html"},
            };
            foreach (KeyValuePair<string, string> pair in TextPatterns)
            {
                _comboItemsTypes.Add(new ListItemNameText { Name = "text_" + pair.Key});
            }
            TypeFilter.DataSource = _comboItemsTypes;
            TypeFilter.DisplayMember = "Text";
            TypeFilter.ValueMember = "Name";
            //MarkFilter.SelectedValue = "allTypes";
            MarkFilter.SelectedIndex = 0;

            BindingList<ListItemNameText> _comboItemsMarks = new BindingList<ListItemNameText>();
            _comboItemsMarks.Add(new ListItemNameText { Name = "allMarks" });
            _comboItemsMarks.Add(new ListItemNameText { Name = "favorite" });
            _comboItemsMarks.Add(new ListItemNameText { Name = "used" });
            MarkFilter.DataSource = _comboItemsMarks;
            MarkFilter.DisplayMember = "Text";
            MarkFilter.ValueMember = "Name";
            //MarkFilter.SelectedValue = "allMarks";
            TypeFilter.SelectedIndex = 0;

            (dataGridView.Columns["AppImage"] as DataGridViewImageColumn).DefaultCellStyle.NullValue = null;
            richTextBox.AutoWordSelection = false;
            urlTextBox.AutoWordSelection = false;

            // Initialize StringCollection settings to prevent error saving settings 
            if (Properties.Settings.Default.LastFilterValues == null)
            {
                Properties.Settings.Default.LastFilterValues = new StringCollection();
            }
            if (Properties.Settings.Default.IgnoreApplicationsClipCapture == null)
            {
                Properties.Settings.Default.IgnoreApplicationsClipCapture = new StringCollection();
            }

            FillFilterItems();

            if (!Directory.Exists(UserSettingsPath))
            {
                Directory.CreateDirectory(UserSettingsPath);
            }
            OpenDatabase();
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
            if (Properties.Settings.Default.MainWindowSize.Width > 0)
                this.Size = Properties.Settings.Default.MainWindowSize;
            timerDaily.Interval = 1;
            timerDaily.Start();
            timerReconnect.Interval = (1000 * 5); // 5 seconds
            timerReconnect.Start();
            this.ActiveControl = dataGridView;
            ResetIsMainProperty();

            LoadSettings();
        }

        // Mouse moved in window working area
        void HideTitleTooltip() 
        {
            titleToolTipBeforeTimer.Stop();
            titleToolTip.Hide(this);
            titleToolTipShown = false;
        }

        public delegate void MouseMovedEvent();

        public class GlobalMouseHandler : IMessageFilter
        {
            private const int WM_MOUSEMOVE = 0x0200;

            public event MouseMovedEvent TheMouseMoved;

            #region IMessageFilter Members

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_MOUSEMOVE)
                {
                    if (TheMouseMoved != null)
                    {
                        TheMouseMoved();
                    }
                }
                // Always allow message to continue to the next filter control
                return false;
            }

            #endregion
        }

        private void OpenDatabase()
        {
            if (!String.IsNullOrWhiteSpace(Properties.Settings.Default.DatabaseFile))
                DbFileName = Properties.Settings.Default.DatabaseFile;
            else
                DbFileName = UserSettingsPath + "\\" + Properties.Resources.DBShortFilename;
            ConnectionString = "data source=" + DbFileName + ";journal_mode=OFF;"; // journal_mode=OFF - Disabled transactions
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
            {
            }
            command = new SQLiteCommand("ALTER TABLE Clips" + " ADD COLUMN Favorite BOOLEAN", m_dbConnection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch
            {
            }
            command = new SQLiteCommand("ALTER TABLE Clips" + " ADD COLUMN ImageSample BINARY", m_dbConnection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch
            {
            }
            command = new SQLiteCommand("ALTER TABLE Clips" + " ADD COLUMN AppPath CHAR(256)", m_dbConnection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch
            {
            }
            command = new SQLiteCommand("CREATE unique index unique_hash on Clips(hash)", m_dbConnection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            string fieldsNeedUpdateText = "", fieldsNeedSelectText = "";
            StringCollection patternNamesNeedUpdate = new StringCollection();
            SQLiteCommand commandUpdate = new SQLiteCommand("", m_dbConnection);
            foreach (KeyValuePair<string, string> pair in TextPatterns)
            {
                string fieldName = "Contain_" + pair.Key;
                command = new SQLiteCommand(String.Format("ALTER TABLE Clips ADD COLUMN {0} BOOLEAN", fieldName), m_dbConnection);
                commandUpdate.Parameters.AddWithValue(pair.Key, false);
                try
                {
                    command.ExecuteNonQuery();
                }
                catch
                {
                    continue; // Comment to debug updateDB
                }
                if (fieldsNeedUpdateText.Length > 0)
                    fieldsNeedUpdateText += ", ";
                fieldsNeedUpdateText += fieldName + " = @" + pair.Key;
                fieldsNeedSelectText += ", " + fieldName;
                patternNamesNeedUpdate.Add(pair.Key);
            }
            if (patternNamesNeedUpdate.Count > 0)
            {
                ThreadStart work = delegate {UpdateNewDBFieldsBackground(commandUpdate, fieldsNeedUpdateText, fieldsNeedSelectText, patternNamesNeedUpdate);};
                updateDBThread = new Thread(work);
                updateDBThread.Start();
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
        }

        private void UpdateNewDBFieldsBackground(SQLiteCommand commandUpdate, string fieldsNeedUpdateText, string fieldsNeedSelectText, StringCollection patternNamesNeedUpdate)
        {
            commandUpdate.CommandText = "UPDATE Clips SET " + fieldsNeedUpdateText + " WHERE Id = @Id";
            commandUpdate.Parameters.AddWithValue("Id", 0);
            SQLiteCommand commandSelect = new SQLiteCommand("", m_dbConnection);
            commandSelect.CommandText = "SELECT Id, Text " + fieldsNeedSelectText + " FROM Clips";
            using (SQLiteDataReader reader = commandSelect.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (stopUpdateDBThread)
                        return;
                    int ClipId = reader.GetInt32(reader.GetOrdinal("Id"));
                    string plainText = reader.GetString(reader.GetOrdinal("Text"));
                    commandUpdate.Parameters["id"].Value = ClipId;
                    bool needWrite = false;
                    foreach (string patternName in patternNamesNeedUpdate)
                    {
                        needWrite = Regex.IsMatch(plainText, TextPatterns[patternName], RegexOptions.IgnoreCase);
                        commandUpdate.Parameters[patternName].Value = needWrite;
                    }
                    if (needWrite)
                        commandUpdate.ExecuteNonQuery();
                }
            }
        }

        private void ResetIsMainProperty()
        {
            SetProp(this.Handle, IsMainPropName, new IntPtr(1));
        }

        delegate void WinEventDelegate(
            IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread,
            uint dwmsEventTime);

        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild,
            uint dwEventThread, uint dwmsEventTime)
        {
            // http://stackoverflow.com/a/10280800/4085971
            int targetProcessId = UpdateLastActiveParentWindow(hwnd);
        }

        private int UpdateLastActiveParentWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                hwnd = GetForegroundWindow();
            int targetProcessId;
            uint remoteThreadId = GetWindowThreadProcessId(hwnd, out targetProcessId);
            if (targetProcessId != Process.GetCurrentProcess().Id)
            {
                lastActiveParentWindow = hwnd;
                lastChildWindow = IntPtr.Zero;
                lastChildWindowRect = new RECT();
                lastCaretPoint = new Point();
                //lastWindowSelectedText = null;
                UpdateWindowTitle();
            }
            return targetProcessId;
        }

        private class MyToolStripRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
            {
                var btn = e.Item as ToolStripButton;
                if (btn != null && btn.Checked && !btn.Selected)
                {
                    Rectangle bounds1 = new Rectangle(Point.Empty, e.Item.Size);
                    bounds1 = new Rectangle(bounds1.Left, bounds1.Top, e.Item.Size.Width - 1, e.Item.Size.Height - 1);
                    Rectangle bounds2 = new Rectangle(bounds1.Left + 1, bounds1.Top + 1, bounds1.Right - 1,
                        bounds1.Bottom - 1);
                    e.Graphics.DrawRectangle(new Pen(Color.DeepSkyBlue), bounds1);
                    Brush brush = new SolidBrush(Color.FromArgb(255, 180, 240, 240));
                    e.Graphics.FillRectangle(brush, bounds2);
                }
                else base.OnRenderButtonBackground(e);
            }
        }

        private void UpdateWindowTitle(bool forced = false)
        {
            if ((this.Top <= maxWindowCoordForHiddenState || !this.Visible) && !forced)
                return;
            string targetTitle = "<" + CurrentLangResourceManager.GetString("NoActiveWindow") + ">";
            if (lastActiveParentWindow != null)
            {
                targetTitle = GetWindowTitle(lastActiveParentWindow);
                int pid;
                GetWindowThreadProcessId(lastActiveParentWindow, out pid);
                Process proc = Process.GetProcessById(pid);
                if (proc != null)
                {
                    targetTitle += " [" + proc.ProcessName + "]";
                }
            }
            Debug.WriteLine("Active window " + lastActiveParentWindow + " " + targetTitle);
            this.Text = Application.ProductName + " " + Properties.Resources.VersionValue + " >> " + targetTitle;
        }

        public static T ParseEnum<T>(string value)
        {
            return (T) Enum.Parse(typeof(T), value, true);
        }

        private void RegisterHotKeys()
        {
            EnumModifierKeys Modifiers;
            Keys Key;
            if (ReadHotkeyFromText(Properties.Settings.Default.GlobalHotkeyOpenLast, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(Properties.Settings.Default.GlobalHotkeyOpenCurrent, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(Properties.Settings.Default.GlobalHotkeyOpenFavorites, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(Properties.Settings.Default.GlobalHotkeyIncrementalPaste, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(Properties.Settings.Default.GlobalHotkeyCompareLastClips, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(Properties.Settings.Default.GlobalHotkeyPasteText, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (Properties.Settings.Default.CopyTextInAnyWindowOnCTRLF3 && ReadHotkeyFromText("Control + F3", out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
        }

        private static bool ReadHotkeyFromText(string HotkeyText, out EnumModifierKeys Modifiers, out Keys Key)
        {
            Modifiers = 0;
            Key = 0;
            if (HotkeyText == "" || HotkeyText == "No")
                return false;
            string[] Frags = HotkeyText.Split(new[] {"+"}, StringSplitOptions.None);
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
            if (hotkeyTitle == Properties.Settings.Default.GlobalHotkeyOpenLast)
            {
                if (this.Visible && this.ContainsFocus && this.Top > maxWindowCoordForHiddenState && MarkFilter.SelectedValue.ToString() != "favorite") // Sometimes it can cotain focus but be not visible!
                    this.Close();
                else
                {
                    ShowForPaste(false, true);
                    dataGridView.Focus();
                }
            }
            else if (hotkeyTitle == Properties.Settings.Default.GlobalHotkeyOpenCurrent)
            {
                if (this.Visible && this.ContainsFocus && this.Top > maxWindowCoordForHiddenState && MarkFilter.SelectedValue.ToString() != "favorite")
                    this.Close();
                else
                {
                    ShowForPaste();
                    //dataGridView.Focus();
                }
            }
            else if (hotkeyTitle == Properties.Settings.Default.GlobalHotkeyOpenFavorites)
            {
                if (this.Visible && this.ContainsFocus && this.Top > maxWindowCoordForHiddenState && MarkFilter.SelectedValue.ToString() == "favorite")
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
                SendPasteClipExpress(null, PasteMethod.Standart, false, true);
                if ((e.Modifier & EnumModifierKeys.Alt) != 0)
                    keybd_event((byte) VirtualKeyCode.MENU, 0x38, 0, 0); // LEFT
                if ((e.Modifier & EnumModifierKeys.Control) != 0)
                    keybd_event((byte) VirtualKeyCode.CONTROL, 0x1D, 0, 0);
                if ((e.Modifier & EnumModifierKeys.Shift) != 0)
                    keybd_event((byte) VirtualKeyCode.SHIFT, 0x2A, 0, 0);
                DataRow oldCurrentDataRow = ((DataRowView) clipBindingSource.Current).Row;
                clipBindingSource.MovePrevious();
                DataRow CurrentDataRow = ((DataRowView) clipBindingSource.Current).Row;
                notifyIcon.Visible = true;
                string messageText;
                if (oldCurrentDataRow == CurrentDataRow)
                    messageText = CurrentLangResourceManager.GetString("PastedLastClip");
                else
                    messageText = CurrentDataRow["Title"].ToString();
                notifyIcon.ShowBalloonTip(3000, CurrentLangResourceManager.GetString("NextClip"), messageText, ToolTipIcon.Info);
                AllowHotkeyProcess = true;
            }
            else if (hotkeyTitle == Properties.Settings.Default.GlobalHotkeyCompareLastClips)
            {
                toolStripMenuItemCompareLastClips_Click();
            }
            else if (hotkeyTitle == Properties.Settings.Default.GlobalHotkeyPasteText)
            {
                SendPasteClipExpress(dataGridView.Rows[0], PasteMethod.PasteText);
            }
            else if (hotkeyTitle == "Control + F3")
            {
                keyboardHook.UnregisterHotKeys();
                BackupClipboard();
                //Clipboard.Clear();
                Paster.SendCopy(false);
                SendKeys.SendWait("^{F3}");
                RegisterHotKeys();
                RestoreClipboard();
            }
            else
            {
                //int a = 0;
            }
        }

        public void BackupClipboard()
        {
            clipboardContents.Clear();
            IDataObject o = Clipboard.GetDataObject();
            foreach (string format in o.GetFormats())
            {
                object data;
                try
                {
                    data = o.GetData(format);
                }
                catch
                {
                    Debug.WriteLine(
                        String.Format(CurrentLangResourceManager.GetString("FailedToReadFormatFromClipboard"), format));
                    continue;
                }
                clipboardContents.Add(format, data);
            }
        }

        public void RestoreClipboard()
        {
            DataObject o = new DataObject();
            foreach (string format in clipboardContents.Keys)
            {
                o.SetData(format, clipboardContents[format]);
            }
            SetClipboardDataObject(o, false);
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
            switch ((Msgs) m.Msg)
            {
                case Msgs.WM_CLIPBOARDUPDATE:
                    Debug.WriteLine("WM_CLIPBOARDUPDATE", "WndProc");
                    if (UsualClipboardMode)
                        UsualClipboardMode = false;
                    else if (!captureTimer.Enabled)
                    {
                        captureTimer.Interval = 50;
                        captureTimer.Start();
                    }
                    break;
                case Msgs.WM_NCMOUSEMOVE:
                    if (PreviousCursor != Cursor.Position)
                    {
                        titleToolTipBeforeTimer.Stop();
                        titleToolTip.Hide(this);
                    }
                    if (m.WParam == new IntPtr(0x0002)) // HT_CAPTION
                    {
                        if (!titleToolTipShown)
                            titleToolTipBeforeTimer.Start();
                    }
                    PreviousCursor = Cursor.Position;
                    return;
                case Msgs.WM_MOVE:
                    titleToolTipShown = true;
                    titleToolTipBeforeTimer.Stop();
                    titleToolTip.Hide(this);
                    return;
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
                object[] attributes = Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute) attributes[0];
                    if (!String.IsNullOrEmpty(titleAttribute.Title))
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute) attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute) attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute) attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes =
                    Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute) attributes[0]).Company;
            }
        }

        #endregion


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
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
                if (ErrorCode != ERROR_INVALID_PARAMETER)
                    Debug.WriteLine("Failed to connect clipboard: " + Marshal.GetLastWin32Error());
                else
                {
                    //already connected
                }
            }
        }

        private void AfterRowLoad(bool FullTextLoad = false, int CurrentRowIndex = -1, int NewSelectionStart = -1,
            int NewSelectionLength = -1)
        {
            DataRowView CurrentRowView;
            mshtml.IHTMLDocument2 htmlDoc;
            string clipType;
            string textPattern = RegexpPattern();
            bool autoSelectMatch = (textPattern.Length > 0 && Properties.Settings.Default.AutoSelectMatch);
            FullTextLoad = FullTextLoad || EditMode;
            richTextBox.ReadOnly = !EditMode;
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
            allowTextPositionChangeUpdate = false;
            clipRichTextLength = 0;
            clipType = "";
            pictureBoxSource.Image = null;
            ImageControl.Image = null;

            //htmlTextBox.Parent = new Control();
            htmlTextBox.Parent.Enabled = false; // Prevent stealing focus
            //htmlTextBox.Document.OpenNew(false);
            //htmlTextBox.Document.Write("");
            htmlDoc = htmlTextBox.Document.DomDocument as mshtml.IHTMLDocument2;
            htmlDoc.write("");
            htmlDoc.close(); // Steals focus!!!

            richTextBox.Enabled = false;
            richTextBox.Text = "";
            textBoxApplication.Text = "";
            textBoxWindow.Text = "";
            StripLabelCreated.Text = "";
            StripLabelSize.Text = "";
            StripLabelVisualSize.Text = "";
            StripLabelType.Text = "";
            stripLabelPosition.Text = "";
            LoadRowReader();
            if (RowReader != null)
            {
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
                StripLabelCreated.Text = ((DateTime) RowReader["Created"]).ToString();
                if (!(RowReader["Size"] is DBNull))
                    StripLabelSize.Text = FormattedClipNumericPropery((int)RowReader["Size"], MultiLangByteUnit());
                if (!(RowReader["Chars"] is DBNull))
                    StripLabelVisualSize.Text = FormattedClipNumericPropery((int)RowReader["Chars"], MultiLangCharUnit());
                string TypeEng = RowReader["Type"].ToString();
                StripLabelType.Text = LocalTypeName(TypeEng);
                stripLabelPosition.Text = "1";
                richTextBox.Clear();
                // to prevent autoscrolling during marking
                richTextBox.HideSelection = true;
                int fontsize = (int) richTextBox.Font.Size; // Size should be without digits after comma
                richTextBox.SelectionTabs = new int[] {fontsize * 4, fontsize * 8, fontsize * 12, fontsize * 16};
                // Set tab size ~ 4
                string shortText;
                string endMarker;
                Font markerFont = richTextBox.Font;
                Color markerColor;
                if (!FullTextLoad && MaxTextViewSize < fullText.Length)
                {
                    //if (useNativeTextFormatting)
                    //    shortText = fullRTF.Substring(0, MaxTextViewSize); // TODO find way correct cutting RTF
                    //else
                    shortText = fullText.Substring(0, MaxTextViewSize);
                    richTextBox.Text = shortText;
                    endMarker = MultiLangCutMarker();
                    markerFont = new Font(markerFont, FontStyle.Underline);
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
                            // fix for 1C formatted document source, fragment has very small width of textbox
                            string marker = "<DIV class=\"fullSize fdFieldMainContainer";
                            string replacement = "<DIV style=\"width: 100%\" class=\"fullSize fdFieldMainContainer";
                            htmlText = htmlText.Replace(marker, replacement);

                            //while (this.htmlTextBox.ReadyState != WebBrowserReadyState.Complete)
                            //{
                            //    Application.DoEvents();
                            //    Thread.Sleep(5);
                            //}
                            //htmlTextBox.Document.OpenNew(false);
                            //htmlTextBox.Document.Write(htmlText);
                            htmlDoc.write(htmlText);
                            htmlDoc.close(); // Steals focus!!!
                            mshtml.IHTMLBodyElement body = htmlDoc.body as mshtml.IHTMLBodyElement;
                            //htmlDoc.selection.empty();
                            htmlTextBox.Document.Body.Drag += new HtmlElementEventHandler(htmlTextBoxDrag);
                            htmlTextBox.Document.Body.KeyDown += new HtmlElementEventHandler(htmlTextBoxDocumentKeyDown);

                            // Need to be called every time, else handler will be lost
                            htmlTextBox.Document.AttachEventHandler("onselectionchange",
                                htmlTextBoxDocumentSelectionChange); // No multi call to handler, but why?
                            if (!htmlInitialized)
                            {
                                mshtml.HTMLDocumentEvents2_Event iEvent = (mshtml.HTMLDocumentEvents2_Event) htmlDoc;
                                iEvent.onclick +=
                                    new mshtml.HTMLDocumentEvents2_onclickEventHandler(htmlTextBoxDocumentClick); //
                                iEvent.onmousedown +=
                                    new mshtml.HTMLDocumentEvents2_onmousedownEventHandler(htmlTextBoxMouseDown); //
                                //iEvent.onselectionchange += new mshtml.HTMLDocumentEvents2_onselectionchangeEventHandler(htmlTextBoxDocumentSelectionChange);
                                htmlInitialized = true;
                            }
                        }
                        else
                        {
                            richTextBox.Rtf = fullRTF;
                        }
                    }
                    else
                        richTextBox.Text = fullText;
                    endMarker = MultiLangEndMarker();
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
                        if (textPattern.Length > 0)
                        {
                            MarkRegExpMatchesInWebBrowser(htmlTextBox, textPattern, !String.IsNullOrEmpty(filterText));
                        }
                    }
                    else
                    {
                        richTextBox.SelectionStart = richTextBox.TextLength;
                        richTextBox.SelectionColor = markerColor;
                        richTextBox.SelectionFont = markerFont;
                        if (TextWasCut)
                            endMarker = Environment.NewLine + endMarker;
                        richTextBox.AppendText(endMarker);
                        // Do it first, else ending hyperlink will connect underline to it

                        MarkLinksInRichTextBox(richTextBox, out TextLinkMatches);
                        if (textPattern.Length > 0)
                        {
                            MarkRegExpMatchesInRichTextBox(richTextBox, textPattern, Color.Red, false, !String.IsNullOrEmpty(filterText), out FilterMatches);
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
                    Image image = GetImageFromBinary((byte[]) RowReader["Binary"]);
                    ImageControl.Image = image;
                    ImageControl.ZoomFitInside();
                }
                if (!autoSelectMatch)
                    RestoreTextSelection(NewSelectionStart, NewSelectionLength);
                allowTextPositionChangeUpdate = true;
                UpdateSelectionPosition();
            }
            tableLayoutPanelData.SuspendLayout();
            UpdateClipButtons();
            htmlTextBox.Parent.Enabled = true;
            if (comboBoxFilter.Focused)
            {
                // Antibug webBrowser steals focus. We set it back
                int filterSelectionLength = comboBoxFilter.SelectionLength;
                int filterSelectionStart = comboBoxFilter.SelectionStart;
                comboBoxFilter.Focus();
                comboBoxFilter.SelectionStart = filterSelectionStart;
                comboBoxFilter.SelectionLength = filterSelectionLength;
            }
            if (clipType == "img")
            {
                tableLayoutPanelData.RowStyles[0].Height = 25;
                tableLayoutPanelData.RowStyles[0].SizeType = SizeType.Absolute;
                tableLayoutPanelData.RowStyles[1].Height = 0;
                tableLayoutPanelData.RowStyles[1].SizeType = SizeType.Absolute;
                tableLayoutPanelData.RowStyles[2].Height = 100;
                tableLayoutPanelData.RowStyles[2].SizeType = SizeType.Percent;
                if (elementPanelHasFocus)
                    ImageControl.Focus();
                htmlTextBox.Visible = false; // Without it htmlTextBox will be visible but why?
                richTextBox.Enabled = true;
            }
            else if (htmlMode)
            {
                tableLayoutPanelData.RowStyles[0].Height = 0;
                tableLayoutPanelData.RowStyles[0].SizeType = SizeType.Absolute;
                tableLayoutPanelData.RowStyles[1].Height = 100;
                tableLayoutPanelData.RowStyles[1].SizeType = SizeType.Percent;
                tableLayoutPanelData.RowStyles[2].Height = 0;
                tableLayoutPanelData.RowStyles[2].SizeType = SizeType.Absolute;
                //htmlTextBox.Enabled = true;
                if (elementPanelHasFocus)
                    htmlTextBox.Document.Focus();
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
                richTextBox.Enabled = true;
                if (elementPanelHasFocus)
                    richTextBox.Focus();
            }
            if (urlTextBox.Text == "")
                tableLayoutPanelData.RowStyles[3].Height = 0;
            else
                tableLayoutPanelData.RowStyles[3].Height = 25;
            if (EditMode && this.Visible && this.ContainsFocus)
                richTextBox.Focus(); // Can activate this window, so we check that window has focus
            tableLayoutPanelData.ResumeLayout();
            if (autoSelectMatch)
                SelectNextMatchInClipText();
        }

        protected virtual void LoadRowReader(int CurrentRowIndex = -1)
        {
            DataRowView CurrentRowView;
            RowReader = null;
            if (CurrentRowIndex == -1)
            {
                CurrentRowView = clipBindingSource.Current as DataRowView;
            }
            else
            {
                CurrentRowView = clipBindingSource[CurrentRowIndex] as DataRowView;
            }
            if (CurrentRowView == null)
                return;
            DataRow CurrentRow = CurrentRowView.Row;
            RowReader = getRowReader((int) CurrentRow["Id"]);
        }

        private string LocalTypeName(string TypeEng)
        {
            string localTypeName;
            if (CurrentLangResourceManager.GetString(TypeEng) == null)
                localTypeName = TypeEng;
            else
                localTypeName = CurrentLangResourceManager.GetString(TypeEng);
            return localTypeName;
        }

        private string FormattedClipNumericPropery(int number, string unit)
        {
            NumberFormatInfo numberFormat = new CultureInfo(Locale).NumberFormat;
            numberFormat.NumberDecimalDigits = 0;
            numberFormat.NumberGroupSeparator = " ";
            return number.ToString("N", numberFormat) + " " + unit;
        }

        private string GetHtmlFromHtmlClipText()
        {
            string htmlClipText = RowReader["htmlText"].ToString();
            if (String.IsNullOrEmpty(htmlClipText))
                return "";
            int indexOfHtlTag = htmlClipText.IndexOf("<html", StringComparison.OrdinalIgnoreCase);
            if (indexOfHtlTag < 0)
                return "";
            return htmlClipText.Substring(indexOfHtlTag);
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
                SetRichTextboxSelection(NewSelectionStart, NewSelectionLength);
            }
        }

        private void SetRichTextboxSelection(int NewSelectionStart, int NewSelectionLength)
        {
            richTextBox.SelectionStart = NewSelectionStart;
            richTextBox.SelectionLength = NewSelectionLength;
            if (richTextBox.SelectionStart > 0 || richTextBox.SelectionLength > 0)
                richTextBox.HideSelection = false; // slow
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
            string[] sizes = {MultiLangByteUnit(), MultiLangKiloByteUnit(), MultiLangMegaByteUnit()};
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
            MarkRegExpMatchesInRichTextBox(control, LinkPattern, Color.Blue, true, false, out matches);
        }

        private void MarkRegExpMatchesInRichTextBox(RichTextBox control, string pattern, Color color, bool underline,
            bool bold, out MatchCollection matches)
        {
            RegexOptions options = RegexOptions.Singleline;
            if (!Properties.Settings.Default.SearchCaseSensitive)
                options = options | RegexOptions.IgnoreCase;
            matches = Regex.Matches(control.Text, pattern, options);
            control.DeselectAll();
            int maxMarked = 50; // prevent slow down
            foreach (Match match in matches)
            {
                control.SelectionStart = match.Groups[1].Index;
                control.SelectionLength = match.Groups[1].Length;
                if (match.Groups.Count > 3)
                {
                    int startGroup = 2;
                    for (int counter = startGroup; counter < match.Groups.Count; counter++)
                    {
                        if (match.Groups[counter].Success)
                        {
                            color = _wordColors[(counter - startGroup) % _wordColors.Length];
                            break;
                        }
                    }
                }
                control.SelectionColor = color;
                FontStyle newstyle = control.SelectionFont.Style;
                if (bold)
                    newstyle = newstyle | FontStyle.Bold;
                if (underline)
                    newstyle = newstyle | FontStyle.Underline;
                if (newstyle != control.SelectionFont.Style)
                    control.SelectionFont = new Font(control.SelectionFont, newstyle);
                maxMarked--;
                if (maxMarked < 0)
                    break;
            }
            control.DeselectAll();
            control.SelectionColor = new Color();
            control.SelectionFont = new Font(control.SelectionFont, FontStyle.Regular);
        }

        private void MarkRegExpMatchesInWebBrowser(WebBrowser control, string pattern, bool bold = false)
        {
            mshtml.IHTMLDocument2 htmlDoc = (mshtml.IHTMLDocument2) htmlTextBox.Document.DomDocument;
            mshtml.IHTMLBodyElement body = htmlDoc.body as mshtml.IHTMLBodyElement;
            int boundingTop = 0;
            int colorIndex = 0;
            int maxMarked = 50; // prevent slow down
            int searchFlags = 0;
            if (Properties.Settings.Default.SearchCaseSensitive)
                searchFlags = 4;
            string[] array;
            if (Properties.Settings.Default.SearchWordsIndependently)
                array = filterText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            else
                array = new string[1] { filterText };
            foreach (var word in array)
            {
                mshtml.IHTMLTxtRange range = body.createTextRange();
                if (colorIndex >= array.Length)
                    colorIndex = 0;
                Color wordColor = _wordColors[colorIndex];
                colorIndex++;
                while (range.findText(word, 1, searchFlags))
                {
                    range.execCommand("ForeColor", false, ColorTranslator.ToHtml(wordColor));
                    if (bold)
                        range.execCommand("Bold", true);
                    mshtml.IHTMLTextRangeMetrics metrics = (mshtml.IHTMLTextRangeMetrics) range;
                    if (boundingTop == 0 || boundingTop > metrics.boundingTop)
                    {
                        boundingTop = metrics.boundingTop;
                        range.scrollIntoView();
                    }
                    range.collapse(false);
                    maxMarked--;
                    if (maxMarked < 0)
                        break;
                }
            }

        }

        private void RichText_Click(object sender, EventArgs e)
        {
            OpenLinkIfAltPressed(sender as RichTextBox, e, TextLinkMatches);
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
            UpdateClipBindingSource(true);
        }

        private void UpdateClipBindingSource(bool forceRowLoad = false, int currentClipId = 0, bool keepTextSelectionIfIDChanged = false, List<int> selectedClipIDs = null)
        {
            if (!(this.Visible && this.ContainsFocus))
                sortField = "Id";
            if (dataAdapter == null)
                return;
            if (EditMode)
                SaveClipText();
            if (currentClipId == 0 && clipBindingSource.Current != null)
            {
                currentClipId = (int) (clipBindingSource.Current as DataRowView).Row["Id"];
                if (dataGridView.SelectedRows.Count > 1 && selectedClipIDs == null)
                {
                    selectedClipIDs = new List<int>();
                    foreach (DataGridViewRow selectedRow in dataGridView.SelectedRows)
                    {
                        if (selectedRow == null)
                            continue;
                        DataRowView dataRow = (DataRowView) selectedRow.DataBoundItem;
                        selectedClipIDs.Insert(0, (int) dataRow["Id"]);
                    }
                }
            }
            allowRowLoad = false;
            bool oldFilterOn = filterOn;
            filterOn = false;
            string sqlFilter = "1 = 1";
            string filterValue = "";
            if (!String.IsNullOrEmpty(filterText))
            {
                string[] array;
                string filterTextTemp = filterText;
                filterTextTemp = filterTextTemp.Replace("_", "\\_");
                filterTextTemp = filterTextTemp.Replace("\\", "\\\\");
                filterTextTemp = filterTextTemp.Replace("'", "''");
                if (!Properties.Settings.Default.SearchWildcards)
                    filterTextTemp = filterTextTemp.Replace("%", "\\%");
                if (Properties.Settings.Default.SearchWordsIndependently)
                    array = filterTextTemp.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                else
                    array = new string[1] { filterTextTemp };
                foreach (var word in array)
                {
                    if (Properties.Settings.Default.SearchCaseSensitive)
                        sqlFilter += " AND Text Like '%" + word + "%' ESCAPE '\\'";
                    else
                        sqlFilter += " AND UPPER(Text) Like UPPER('%" + word + "%') ESCAPE '\\'";
                }
                filterOn = true;
                if (Properties.Settings.Default.SearchIgnoreBigTexts)
                    sqlFilter += " AND Chars < 100000";
            }
            if (TypeFilter.SelectedValue as string != "allTypes")
            {
                filterValue = TypeFilter.SelectedValue as string;
                bool isText = filterValue.Contains("text");
                string filterValueText;
                if (isText)
                    filterValueText = "'html','rtf','text'";
                else
                    filterValueText = "'" + filterValue + "'";
                sqlFilter += " AND type IN (" + filterValueText + ")";
                if (isText && filterValue != "text")
                {
                    sqlFilter += String.Format(" AND Contain_{0}", filterValue.Substring("text_".Length));
                }
                filterOn = true;
            }
            if (MarkFilter.SelectedValue as string != "allMarks")
            {
                filterValue = MarkFilter.SelectedValue as string;
                sqlFilter += " AND " + filterValue;
                filterOn = true;
            }
            if (periodFilterOn)
            {
                sqlFilter += " AND Created BETWEEN @startDate AND @endDate ";
                dataAdapter.SelectCommand.Parameters.AddWithValue("startDate", monthCalendar1.SelectionStart);
                dataAdapter.SelectCommand.Parameters.AddWithValue("endDate", monthCalendar1.SelectionEnd);
                filterOn = true;
            }
            if (!oldFilterOn && filterOn)
                selectedClipsBeforeFilterApply.Clear();
            // Dublicated code 8gfd8843
            //string selectCommandText = "Select Id, Used, Title, Chars, Type, Favorite, ImageSample, AppPath, Size, Created From Clips";
            string selectCommandText = "Select Id, NULL AS Used, NULL AS Title, NULL AS Chars, NULL AS Type, NULL AS Favorite, NULL AS ImageSample, NULL AS AppPath, NULL AS Size, NULL AS Created From Clips";
            selectCommandText += " WHERE " + sqlFilter;
            selectCommandText += " ORDER BY " + sortField + " desc";
            if (Properties.Settings.Default.SearchCaseSensitive)
                selectCommandText = "PRAGMA case_sensitive_like = 1; " + selectCommandText;
            else
                selectCommandText = "PRAGMA case_sensitive_like = 0; " + selectCommandText;
            dataAdapter.SelectCommand.CommandText = selectCommandText;

            DataTable table = new DataTable();
            table.Locale = CultureInfo.InvariantCulture;
            dataAdapter.Fill(table);
            clipBindingSource.DataSource = table;

            PrepareTableGrid(); // Long
            if (filterOn)
            {
                toolStripButtonClearFilter.Enabled = true;
                //toolStripButtonClearFilter.Checked = true; // Back color wil not change
                toolStripButtonClearFilter.BackColor = Color.GreenYellow;
            }
            else
            {
                toolStripButtonClearFilter.Enabled = false;
                toolStripButtonClearFilter.BackColor = DefaultBackColor;
                //toolStripButtonClearFilter.Checked = false;
            }
            if (LastId == 0)
            {
                GotoLastRow();
                ClipsNumber = clipBindingSource.Count;
                DataRowView lastRow = (DataRowView) clipBindingSource.Current;
                if (lastRow == null)
                {
                    LastId = 0;
                }
                else
                {
                    LastId = (int) lastRow["Id"];
                }
            }
            else
            {
                allowRowLoad = false;
                dataGridView.ClearSelection();
                allowRowLoad = true;
                RestoreSelectedCurrentClip(forceRowLoad, currentClipId, false, keepTextSelectionIfIDChanged);
                if (selectedClipIDs != null)
                {
                    allowProcessDataGridSelectionChanged = false;
                    foreach (int selectedID in selectedClipIDs)
                    {
                        int newIndex = clipBindingSource.Find("Id", selectedID);
                        if (newIndex >= 0)
                        {
                            dataGridView.Rows[newIndex].Selected = false;
                            dataGridView.Rows[newIndex].Selected = true;
                        }
                    }
                    allowProcessDataGridSelectionChanged = true;
                }
            }
            allowRowLoad = true;
            //AutoGotoLastRow = false;
        }

        private void RestoreSelectedCurrentClip(bool forceRowLoad = false, int currentClipId = -1,
            bool clearSelection = true, bool keepTextSelectionIfIDChanged = false)
        {
            if (false
                //|| AutoGotoLastRow 
                || currentClipId <= 0)
            {
                UpdateSelectedClipsHistory();
                GotoLastRow();
            }
            else if (currentClipId > 0)
            {
                int newPosition = clipBindingSource.Find("Id", currentClipId);
                allowRowLoad = false;
                if (newPosition == -1)
                    UpdateSelectedClipsHistory();
                else
                {
                    // Calls SelectionChanged in DataGridView. Resets selectedRows!
                    clipBindingSource.Position = newPosition; 
                    //dataGridView.CurrentCell = dataGridView.Rows[newPosition].Cells[0];
                }
                allowRowLoad = true;
                SelectCurrentRow(forceRowLoad || newPosition == -1, true, clearSelection, keepTextSelectionIfIDChanged);
            }
        }

        private void ClearFilter_Click(object sender = null, EventArgs e = null)
        {
            ClearFilter();
            dataGridView.Focus();
        }

        private void ClearFilter(int CurrentClipID = 0)
        {
            if (filterOn)
            {
                AllowFilterProcessing = false;
                comboBoxFilter.Text = "";
                periodFilterOn = false;
                ReadFilterText();
                TypeFilter.SelectedIndex = 0;
                MarkFilter.SelectedIndex = 0;
                AllowFilterProcessing = true;
                //UpdateClipBindingSource(false, CurrentClipID);
                UpdateClipBindingSource(true, CurrentClipID); // To repaint text
            }
            else if (CurrentClipID != 0)
                RestoreSelectedCurrentClip(false, CurrentClipID);
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
                    if (lastActiveParentWindow != null)
                        lastActSet = SetForegroundWindow(lastActiveParentWindow);
                    if (!lastActSet)
                        //SetForegroundWindow(IntPtr.Zero); // This way focus was not lost!
                        SetActiveWindow(IntPtr.Zero);
                    this.Top = maxWindowCoordForHiddenState;
                }
                else
                {
                    this.SuspendLayout();
                    this.FormBorderStyle = FormBorderStyle.FixedToolWindow; // To disable animation
                    this.Hide();
                    this.ResumeLayout();
                }
                e.Cancel = true;
                //if (Properties.Settings.Default.ClearFiltersOnClose)
                //    ClearFilter();
                //this.ResumeLayout();
            }
            else
            {
                if (WindowState == FormWindowState.Normal)
                    Properties.Settings.Default.MainWindowSize = Size;
                else
                    Properties.Settings.Default.MainWindowSize = RestoreBounds.Size;
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetClipboardOwner();

        private void CaptureClipboardData()
        {
            captureTimer.Stop();
            //
            // Data on the clipboard uses the 
            // IDataObject interface
            //
            //if (!CaptureClipboard)
            //{ return; }
            IDataObject iData = new DataObject();
            string clipType = "";
            string clipText = "";
            string richText = "";
            string htmlText = "";
            string clipUrl = "";
            int clipChars = 0;
            string appPath = "";
            string clipWindow = "";
            string clipApplication = "";
            GetClipboardOwnerLockerInfo(false, out clipWindow, out clipApplication, out appPath);
            if (ignoreModulesInClipCapture.Contains(clipApplication.ToLower()))
                return;
            try
            {
                iData = Clipboard.GetDataObject();
            }
            catch (Exception ex)
            {
                // Copying a field definition in Access 2002 causes this sometimes or Excel big clips
                Debug.WriteLine(String.Format("Clipboard.GetDataObject(): InteropServices.ExternalException: {0}", ex.Message));
                string Message = CurrentLangResourceManager.GetString("ErrorReadingClipboard") + ": " + ex.Message;
                notifyIcon.ShowBalloonTip(2000, Application.ProductName, Message, ToolTipIcon.Info);
                return;
            }
            if (iData.GetDataPresent(DataFormat_ClipboardViewerIgnore) && Properties.Settings.Default.IgnoreExclusiveFormatClipCapture)
                return;
            bool textFormatPresent = false;
            byte[] binaryBuffer = new byte[0];
            byte[] imageSampleBuffer = new byte[0];
            int NumberOfFilledCells = 0;
            int NumberOfImageCells = 0;
            Bitmap bitmap = null;
            try
            {
                if (iData.GetDataPresent(DataFormats.UnicodeText))
                {
                    clipText = (string) iData.GetData(DataFormats.UnicodeText);
                    if (!String.IsNullOrEmpty(clipText))
                    {
                        clipType = "text";
                        textFormatPresent = true;
                    }
                }
                if (!textFormatPresent && iData.GetDataPresent(DataFormats.Text))
                {
                    clipText = (string) iData.GetData(DataFormats.Text);
                    if (!String.IsNullOrEmpty(clipText))
                    {
                        clipType = "text";
                        textFormatPresent = true;
                    }
                }
                if (iData.GetDataPresent(DataFormat_XMLSpreadSheet))
                {
                    object data = iData.GetData(DataFormat_XMLSpreadSheet);
                    if (data.GetType() == typeof(MemoryStream))
                    {
                        // Excel
                        using (MemoryStream ms = (MemoryStream)data)
                        {
                            if (ms!=null && ms.Length > 0)
                            {
                                byte[] buffer = new byte[ms.Length];
                                ms.Read(buffer, 0, (int) ms.Length);
                                string xmlSheet = Encoding.Default.GetString(buffer);
                                Match match = Regex.Match(xmlSheet, "ExpandedColumnCount=\"(\\d+)\"");
                                int NumberOfColumns = 1;
                                if (match.Success)
                                    NumberOfColumns = Convert.ToInt32(match.Groups[1].Value);
                                match = Regex.Match(xmlSheet, "ExpandedRowCount=\"(\\d+)\"");
                                int NumberOfRows = 1;
                                if (match.Success)
                                    NumberOfRows = Convert.ToInt32(match.Groups[1].Value);
                                NumberOfImageCells = NumberOfRows * NumberOfColumns;
                                NumberOfFilledCells = Regex.Matches(xmlSheet, "<Row").Count;
                            }
                        }
                    }
                }

                if (true
                    && iData.GetDataPresent(DataFormats.Html)
                    && (false
                        || NumberOfFilledCells == 0
                        || Properties.Settings.Default.MaxCellsToCaptureFormattedText > NumberOfFilledCells))
                {
                    htmlText = (string) iData.GetData(DataFormats.Html);
                    if (String.IsNullOrEmpty(htmlText))
                    {
                        htmlText = "";
                    }
                    else
                    {
                        clipType = "html";
                        Match match = Regex.Match(htmlText, "SourceURL:(" + LinkPattern + ")", RegexOptions.IgnoreCase);
                        if (match.Captures.Count > 0)
                            clipUrl = match.Groups[1].ToString();
                        if (String.IsNullOrWhiteSpace(clipText))
                        {
                            // It may take much time to parse big html
                            var htmlParser = new HtmlParser();
                            var documentHtml = htmlParser.Parse(htmlText);
                            if (documentHtml.Images.Length > 0)
                            {
                                string ImageUrl = documentHtml.Images[0].Source;
                                using (WebClient webClient = new WebClient())
                                {
                                    webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
                                    try
                                    {
                                        byte[] tempBuffer = webClient.DownloadData(ImageUrl);
                                        using (var ms = new MemoryStream(tempBuffer))
                                        {
                                            bitmap = new Bitmap(ms);
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }

                if (true
                    && iData.GetDataPresent(DataFormats.Rtf)
                    && clipType != "html"
                    && (false
                        || NumberOfFilledCells == 0
                        || Properties.Settings.Default.MaxCellsToCaptureFormattedText > NumberOfFilledCells))
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

                string clipTextImage = "";
                int clipCharsImage = 0;
                // http://www.cyberforum.ru/ado-net/thread832314.html
                // html text check to prevent crush from too big generated Excel image
                if (true
                    && iData.GetDataPresent(DataFormats.Bitmap)
                    && (false
                        || NumberOfImageCells == 0
                        || Properties.Settings.Default.MaxCellsToCaptureImage > NumberOfImageCells))
                {
                    //clipType = "img";
                    bitmap = iData.GetData(DataFormats.Bitmap) as Bitmap;
                    if (bitmap != null) // NUll happens while copying image in standart image viewer Windows 10
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            bitmap.Save(memoryStream, ImageFormat.Png);
                            binaryBuffer = memoryStream.ToArray();
                        }
                        //clipTextImage = CurrentLangResourceManager.GetString("Size") + ": " + image.Width + "x" + image.Height + "\n"
                        //     + CurrentLangResourceManager.GetString("PixelFormat") + ": " + image.PixelFormat + "\n";
                        clipTextImage = bitmap.Width + " x " + bitmap.Height;
                        if (!String.IsNullOrEmpty(clipWindow))
                            clipTextImage += ", " + clipWindow;
                        clipTextImage += ", " + CurrentLangResourceManager.GetString("PixelFormat") + ": " +
                                         Image.GetPixelFormatSize(bitmap.PixelFormat);
                        clipCharsImage = bitmap.Width * bitmap.Height;
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
                }
                if (bitmap != null)
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        int fragmentWidth = 100;
                        int fragmentHeight = 20;
                        var bestPoint = FindBestImageFragment(bitmap, fragmentWidth, fragmentHeight);
                        using (Image ImageSample = CopyRectImage(bitmap, new Rectangle(bestPoint.X, bestPoint.Y, fragmentWidth, fragmentHeight)))
                        {
                            ImageSample.Save(memoryStream, ImageFormat.Png);
                            imageSampleBuffer = memoryStream.ToArray();
                        }
                    }

                // Split Image+Html clip into 2: Image and Html 
                if (clipTextImage != "")
                {
                    // Image clip
                    AddClip(binaryBuffer, imageSampleBuffer, "", "", "img", clipTextImage, clipApplication,
                        clipWindow, clipUrl, clipCharsImage, appPath, false, false, htmlText == "");
                    if (!String.IsNullOrWhiteSpace(clipText))
                        imageSampleBuffer = new byte[0];
                }
                if (clipType != "")
                {
                    // Non image clip
                    AddClip(new byte[0], imageSampleBuffer, htmlText, richText, clipType, clipText, clipApplication,
                        clipWindow, clipUrl, clipChars, appPath, false, false, true);
                }
            }
            finally
            {
                if (bitmap != null)
                    bitmap.Dispose();
            }
        }

        private Point FindBestImageFragment(Bitmap bitmap, int fragmentWidth, int fragmentHeight, int diffTreshold = 20)
        {
            int bestX = -1, bestY = -1;
            int goodX = -1, goodY = -1;
            //int badX = -1, badY = -1;
            //int maxDelta = 0;
            for (int x = 0; x < Math.Min(bitmap.Width, 500) - 2; x+=2) // step 1->2 for speed up
            {
                for (int y = 0; y < Math.Min(bitmap.Height, 500) - 2; y+=2) // step 1->2 for speed up
                {
                    //// Too slow
                    //int minColor = 100000;
                    //int maxColor = 0;
                    //for (int i = 0; i < 3; i++)
                    //    for (int j = 0; j < 3; j++)
                    //    {
                    //        Color curColor = bitmap.GetPixel(x + i, y + j);
                    //        int colorValue = 30*curColor.R + 59*curColor.G + 11*curColor.B;
                    //        if (colorValue < minColor)
                    //            minColor = colorValue;
                    //        if (colorValue > maxColor)
                    //            maxColor = colorValue;
                    //    }
                    //int curDelta = maxColor - minColor;
                    //if (curDelta > maxDelta)
                    //{
                    //    maxDelta = curDelta;
                    //    badX = x;
                    //    badY = y;
                    //}
                    Color basePixel = bitmap.GetPixel(x, y);
                    if (true
                        && ColorDifference(basePixel, bitmap.GetPixel(x + 2, y + 1)) > diffTreshold
                        && ColorDifference(basePixel, bitmap.GetPixel(x + 1, y + 2)) > diffTreshold)
                    {
                        if (goodX < 0)
                        {
                            goodX = x;
                            goodY = y;
                        }
                        if (false
                            || (true
                                && ColorDifference(bitmap.GetPixel(x + 2, y + 1), bitmap.GetPixel(x + 2, y + 2)) > diffTreshold
                                && ColorDifference(bitmap.GetPixel(x + 2, y + 1), bitmap.GetPixel(x + 2, y)) > diffTreshold
                                && ColorDifference(bitmap.GetPixel(x + 2, y + 2), bitmap.GetPixel(x, y + 2)) > diffTreshold)
                            || (true
                                && ColorDifference(bitmap.GetPixel(x + 1, y + 2), bitmap.GetPixel(x, y + 2)) > diffTreshold
                                && ColorDifference(bitmap.GetPixel(x + 1, y + 2), bitmap.GetPixel(x + 2, y + 2)) > diffTreshold
                                && ColorDifference(bitmap.GetPixel(x + 2, y + 2), bitmap.GetPixel(x + 2, y)) > diffTreshold)
                            || (true
                                && ColorDifference(bitmap.GetPixel(x + 2, y + 1), bitmap.GetPixel(x + 2, y)) > diffTreshold
                                && ColorDifference(bitmap.GetPixel(x + 1, y + 2), bitmap.GetPixel(x, y + 2)) > diffTreshold))
                        {
                            bestX = x;
                            bestY = y;
                            break;
                        }
                    }
                }
                if (bestX >= 0)
                    break;
            }
            if (bestX < 0)
            {
                bestX = goodX;
                bestY = goodY;
            }
            //if (bestX < 0)
            //{
            //    bestX = badX;
            //    bestY = badY;
            //}
            bestX = Math.Max(0, Math.Min(bestX, bitmap.Width - fragmentWidth - 1));
            bestY = Math.Max(0, Math.Min(bestY, bitmap.Height - fragmentHeight - 1));
            Point bestPoint = new Point(bestX, bestY);
            return bestPoint;
        }

        int ColorDifference(Color Color1, Color Color2)
        {
            int result = 0;
            result += 30 * Math.Abs(Color1.R - Color2.R);
            result += 59 * Math.Abs(Color1.G - Color2.G);
            result += 11 * Math.Abs(Color1.B - Color2.B);
            result = result / 100;
            return result;
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
            string applicationText = "", string windowText = "", string url = "", int chars = 0, string appPath = "", bool used = false, bool favorite = false, bool updateList = true)
        {
            DateTime dtNow = DateTime.Now;
            int msFromLastCapture = DateDiffMilliseconds(lastCaptureMoment);
            if (plainText == null)
                plainText = "";
            if (richText == null)
                richText = "";
            if (htmlText == null)
                htmlText = "";
            int byteSize = plainText.Length * 2; // dirty
            if (chars == 0)
                chars = plainText.Length;
            if (binaryBuffer != null)
                byteSize += binaryBuffer.Length;
            byteSize += htmlText.Length * 2; // dirty
            byteSize += richText.Length * 2; // dirty
            if (byteSize > Properties.Settings.Default.MaxClipSizeKB * 1000)
            {
                string message = String.Format(CurrentLangResourceManager.GetString("ClipWasNotCaptured"), (int)(byteSize / 1024), Properties.Settings.Default.MaxClipSizeKB,
                    LocalTypeName(typeText));
                notifyIcon.ShowBalloonTip(2000, Application.ProductName, message, ToolTipIcon.Info);
                return;
            }
            if (Properties.Settings.Default.PlaySoundOnClipCapture)
                SystemSounds.Beep.Play();
            DateTime created = DateTime.Now;
            string clipTitle = TextClipTitle(plainText);
            MD5 md5 = new MD5CryptoServiceProvider();
            if (binaryBuffer != null)
                md5.TransformBlock(binaryBuffer, 0, binaryBuffer.Length, binaryBuffer, 0);
            byte[] binaryText = Encoding.Unicode.GetBytes(plainText);
            md5.TransformBlock(binaryText, 0, binaryText.Length, binaryText, 0);
            if (Properties.Settings.Default.UseFormattingInDublicateDetection || String.IsNullOrEmpty(plainText))
            {
                byte[] binaryRichText = Encoding.Unicode.GetBytes(richText);
                md5.TransformBlock(binaryRichText, 0, binaryRichText.Length, binaryRichText, 0);
                byte[] binaryHtml = Encoding.Unicode.GetBytes(htmlText);
                md5.TransformFinalBlock(binaryHtml, 0, binaryHtml.Length);
            }
            else
            {
                byte[] binaryType = Encoding.Unicode.GetBytes(typeText);
                md5.TransformFinalBlock(binaryType, 0, binaryType.Length);
            }
            string hash = Convert.ToBase64String(md5.Hash);
            string sql = "SELECT Id, Title, Used, Favorite, Created FROM Clips Where Hash = @Hash";
            SQLiteCommand commandSelect = new SQLiteCommand(sql, m_dbConnection);
            commandSelect.Parameters.AddWithValue("@Hash", hash);

            int oldCurrentClipId = 0;
            lastClipWasMultiCaptured = false;
            using (SQLiteDataReader reader = commandSelect.ExecuteReader())
            {
                if (reader.Read())
                {
                    oldCurrentClipId = reader.GetInt32(reader.GetOrdinal("Id"));
                    if (true
                        && lastPastedClips.ContainsKey(oldCurrentClipId)
                        && DateDiffMilliseconds(lastPastedClips[oldCurrentClipId], dtNow) < 1000) // Protection from automated return copy after we send paste. For example Word does so for html paste.
                    {
                        return;
                    }
                    used = GetNullableBoolFromSqlReader(reader, "Used");
                    favorite = GetNullableBoolFromSqlReader(reader, "Favorite");
                    clipTitle = reader.GetString(reader.GetOrdinal("Title"));
                    sql = "DELETE FROM Clips Where Id = @Id";
                    SQLiteCommand commandDelete = new SQLiteCommand(sql, m_dbConnection);
                    if (true
                        && oldCurrentClipId == LastId 
                        && msFromLastCapture > 100) // Protection from automated repeated copy. For example PuntoSwitcher does so.
                    {
                        lastClipWasMultiCaptured = true;
                    }
                    commandDelete.Parameters.AddWithValue("@Id", oldCurrentClipId);
                    commandDelete.ExecuteNonQuery();
                    RegisterClipIdChange(oldCurrentClipId, LastId + 1);
                }
            }
            LastId = LastId + 1;

            SQLiteCommand commandInsert = new SQLiteCommand("", m_dbConnection);
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
            string intoFieldsText = "", intoParamsText = "";
            foreach (KeyValuePair<string, string> pair in TextPatterns)
            {
                commandInsert.Parameters.AddWithValue("@Contain_" + pair.Key, Regex.IsMatch(plainText, pair.Value, RegexOptions.IgnoreCase));
                intoFieldsText += ", Contain_" + pair.Key;
                intoParamsText += ", @Contain_" + pair.Key;
            }
            sql = "insert into Clips (Id, Title, Text, Application, Window, Created, Type, Binary, ImageSample, Size, Chars, RichText, HtmlText, Used, Favorite, Url, Hash, appPath"
                  + intoFieldsText + ") "
                  + "values (@Id, @Title, @Text, @Application, @Window, @Created, @Type, @Binary, @ImageSample, @Size, @Chars, @RichText, @HtmlText, @Used, @Favorite, @Url, @Hash, @appPath"
                  + intoParamsText + ")";
            commandInsert.CommandText = sql;
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
            //if (this.Visible)
            //{
            if (updateList)
                UpdateClipBindingSource(false, 0, oldCurrentClipId > 0);
            if (true
                && applicationText == "ScreenshotReader"
                && IsTextType(typeText)
                //&& !Visible 
                //&& Properties.Settings.Default.SelectTopClipOnOpen 
            )
                ShowForPaste(false, true);
            //}
            lastCaptureMoment = DateTime.Now;
        }

        // dt2 - null or DateTime
        private int DateDiffMilliseconds(DateTime dt1, object dt2 = null)
        {
            if (dt2 == null)
                dt2 = DateTime.Now;
            TimeSpan span = (DateTime) dt2 - dt1;
            int ms = (int)span.TotalMilliseconds;
            return ms;
        }

        private void DeleteOldClips()
        {
            if (Properties.Settings.Default.HistoryDepthDays == 0)
                return;
            SQLiteCommand command = new SQLiteCommand(m_dbConnection);
            command.CommandText = "Delete From Clips where (NOT Favorite OR Favorite IS NULL) AND Created < date('now','-" + Properties.Settings.Default.HistoryDepthDays + " day')";
            //commandInsert.Parameters.AddWithValue("Number", Properties.Settings.Default.HistoryDepthDays);
            //command.Parameters.AddWithValue("CurDate", DateTime.Now);
            command.ExecuteNonQuery();
        }

        private void DeleteExcessClips()
        {
            if (Properties.Settings.Default.HistoryDepthNumber == 0)
                return;
            SQLiteCommand command = new SQLiteCommand(m_dbConnection);
            int numberOfClipsToDelete = ClipsNumber - Properties.Settings.Default.HistoryDepthNumber;
            if (numberOfClipsToDelete > 0)
            {
                command.CommandText = "Delete From Clips where (NOT Favorite OR Favorite IS NULL) AND Id IN (Select ID From Clips ORDER BY ID Limit @Number)";
                command.Parameters.AddWithValue("Number", numberOfClipsToDelete);
                command.ExecuteNonQuery();
                //ClipsNumber -= numberOfClipsToDelete;
            }
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
            // Removing repeats (series) of empty space and leave only 1 space
            title = Regex.Replace(title, @"\s+", " ");
            // Removing repeats (series of one char) of non digits and leave only 4 chars
            title = Regex.Replace(title, "([^\\d])(?<=\\1\\1\\1\\1\\1)", String.Empty,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            // Removing repeats (series of one char) of digits and leave only 20 chars
            title = Regex.Replace(title, "(\\d)(?<=\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1)", String.Empty,
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
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
                DataRowView dataRow = (DataRowView) selectedRow.DataBoundItem;
                if (dataRow["Favorite"] != DBNull.Value && (bool)dataRow["Favorite"])
                    continue;
                string parameterName = "@Id" + counter;
                sql += "," + parameterName;
                command.Parameters.Add(parameterName, DbType.Int32).Value = dataRow["Id"];
                counter++;
                dataGridView.Rows.Remove(selectedRow);
                ClipsNumber--;
                if (counter == 999) // SQLITE_MAX_VARIABLE_NUMBER, which defaults to 999, but can be lowered at runtime
                    break;
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
        [return: MarshalAs(UnmanagedType.Bool)]
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
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("User32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnableWindow(IntPtr hwnd, bool bEnable);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
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
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
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
                this.hkl = LoadKeyboardLayout(pwszKlid,
                    KeyboardLayoutFlags.KLF_ACTIVATE | KeyboardLayoutFlags.KLF_SUBSTITUTE_OK);
            }

            private KeyboardLayout(uint hkl)
            {
                this.hkl = hkl;
            }

            public uint Handle
            {
                get { return this.hkl; }
            }

            public static KeyboardLayout GetCurrent()
            {
                uint hkl = GetKeyboardLayout((uint) Thread.CurrentThread.ManagedThreadId);
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

        private string CopyClipToClipboard(SQLiteDataReader rowReader = null, bool onlySelectedPlainText = false, bool allowSelfCapture = true)
        {
            SaveFilterInLastUsedList();
            string clipText;
            var dto = ClipDataObject(rowReader, onlySelectedPlainText, out clipText);
            //if (!Properties.Settings.Default.MoveCopiedClipToTop)
            //    CaptureClipboard = false;
            SetClipboardDataObject(dto, allowSelfCapture);
            return clipText;
        }

        private DataObject ClipDataObject(SQLiteDataReader rowReader, bool onlySelectedPlainText, out string clipText)
        {
            if (rowReader == null)
                rowReader = RowReader;

            //DataRow CurrentDataRow = ((DataRowView)clipBindingSource.Current).Row;
            string type = (string) rowReader["type"];
            object richText = rowReader["RichText"];
            object htmlText = rowReader["HtmlText"];
            byte[] binary = rowReader["Binary"] as byte[];
            clipText = (string) rowReader["Text"];
            if (type == "img" && onlySelectedPlainText)
            {
                string fileEditor = "";
                clipText = GetClipTempFile(out fileEditor, rowReader);
            }
            if (rowReader == RowReader)
            {
                string selectedText = GetSelectedText(onlySelectedPlainText);
                if (!String.IsNullOrEmpty(selectedText))
                    clipText = selectedText;
            }
            DataObject dto = new DataObject();
            if (IsTextType(type) || type == "file" || onlySelectedPlainText)
            {
                SetTextInClipboardDataObject(dto, clipText);
            }
            if (true
                && (type == "rtf" || type == "html")
                && !(richText is DBNull)
                && !String.IsNullOrEmpty(richText.ToString())
                && !onlySelectedPlainText)
            {
                dto.SetText((string) richText, TextDataFormat.Rtf);
            }
            if (true
                && type == "html"
                && !(htmlText is DBNull)
                && !onlySelectedPlainText)
            {
                dto.SetText((string) htmlText, TextDataFormat.Html);
            }
            if (type == "file" && !onlySelectedPlainText)
            {
                string[] fileNameList = clipText.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
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
            return dto;
        }

        private string GetSelectedText(bool onlySelectedPlainText = true)
        {
            string selectedText = "";
            mshtml.IHTMLTxtRange htmlSelection = GetHtmlCurrentTextRangeOrAllDocument(true);
            bool selectedPlainTextMode = true
                                         && onlySelectedPlainText
                                         && (false
                                             || !String.IsNullOrEmpty(richTextBox.SelectedText)
                                             || htmlSelection != null && !String.IsNullOrEmpty(htmlSelection.text));
            if (selectedPlainTextMode && !String.IsNullOrEmpty(richTextBox.SelectedText))
            {
                selectedText = richTextBox.SelectedText;
            }
            else if (selectedPlainTextMode && !String.IsNullOrEmpty(htmlSelection.text))
            {
                selectedText = htmlSelection.text;
            }
            else if (EditMode)
                selectedText = richTextBox.Text;
            return selectedText;
        }

        private void SaveFilterInLastUsedList()
        {
            StringCollection lastFilterValues = Properties.Settings.Default.LastFilterValues;
            if (!String.IsNullOrEmpty(filterText) && !lastFilterValues.Contains(filterText))
            {
                lastFilterValues.Insert(0, filterText);
                while (lastFilterValues.Count > 20)
                {
                    lastFilterValues.RemoveAt(lastFilterValues.Count - 1);
                }
                FillFilterItems();
            }
        }

        private bool IsTextType(string type = "")
        {
            if (type == "")
                type = (string) RowReader["type"];
            return type == "rtf" || type == "text" || type == "html";
        }

        // Does not respect MoveCopiedClipToTop
        private string SendPasteClipExpress(DataGridViewRow currentViewRow = null, PasteMethod pasteMethod = PasteMethod.Standart, bool pasteDelimiter = false, bool updateDB = false)
        {
            if (currentViewRow == null)
                currentViewRow = dataGridView.CurrentRow;
            if (currentViewRow == null)
                return "";
            var dataRow = (DataRowView) currentViewRow.DataBoundItem;
            var rowReader = getRowReader((int) dataRow["id"]);
            string type = (string) rowReader["type"];
            if (pasteMethod == PasteMethod.Null)
            {
                string textToPaste = (string) rowReader["Text"];
                if (type == "img")
                {
                    string fileEditor = "";
                    textToPaste = GetClipTempFile(out fileEditor, rowReader);
                }
                if (pasteDelimiter)
                    textToPaste = Environment.NewLine + textToPaste;
                return textToPaste;
            }
            if (pasteDelimiter && pasteMethod == PasteMethod.Standart)
            {
                int multipasteDelay = 50;
                Thread.Sleep(multipasteDelay);
                //if (IsTextType(type))
                //{
                    SetTextInClipboard(Environment.NewLine + Environment.NewLine, false);
                    SendPaste(pasteMethod);
                    Thread.Sleep(multipasteDelay);
                //}
            }
            CopyClipToClipboard(rowReader, pasteMethod != PasteMethod.Standart && pasteMethod != PasteMethod.File, false);
            if (SendPaste(pasteMethod))
                return "";

            if (updateDB)
                SetRowMark("Used", true, false, true);
            //if (false
            //    || Properties.Settings.Default.MoveCopiedClipToTop 
            //    || (true 
            //        && pasteMethod == PasteMethod.PasteText 
            //        && !String.IsNullOrEmpty(richTextBox.SelectedText)))
            //{
            //    CaptureClipboardData();
            //}
            //else
            //{
            if (currentViewRow.DataBoundItem != null)
            {
                ((DataRowView) currentViewRow.DataBoundItem).Row["Used"] = true;
                //PrepareTableGrid();
                UpdateTableGridRowBackColor(currentViewRow);
            }
            //}
            return "";
        }

        private bool SendPaste(PasteMethod pasteMethod = PasteMethod.Standart)
        {
            int targetProcessId;
            //string oldWindowSelectedText = lastWindowSelectedText;
            IntPtr oldChildWindow = lastChildWindow;
            RECT oldChildWindowRect = lastChildWindowRect;
            Point oldCaretPoint = lastCaretPoint;
            uint remoteThreadId = GetWindowThreadProcessId(lastActiveParentWindow, out targetProcessId);
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
                SetForegroundWindow(lastActiveParentWindow);
                Debug.WriteLine("Set foreground window " + lastActiveParentWindow + " " +
                                GetWindowTitle(lastActiveParentWindow));
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

            if (oldChildWindow != IntPtr.Zero && Properties.Settings.Default.RestoreCaretPositionOnFocusReturn)
            {
                Point point;
                RECT newRect;
                GUITHREADINFO guiInfo = new GUITHREADINFO();
                for (int i = 0; i < 500; i += waitStep)
                {
                    guiInfo = GetGuiInfo(lastActiveParentWindow, out point);
                    if (guiInfo.hwndFocus == oldChildWindow)
                        break;
                    Thread.Sleep(waitStep);
                }
                GetWindowRect(oldChildWindow, out newRect);
                if (newRect.Equals(oldChildWindowRect))
                {
                    //if (guiInfo.hwndFocus != oldChildWindow)
                    //{
                    //    // Adress text box of IE11
                    //    Paster.ClickOnPoint(oldChildWindow, oldCaretPoint);
                    //}
                    //else
                    //{
                    //    //string newActiveWindowSelectedText = getActiveWindowSelectedText();
                    //    //if (newActiveWindowSelectedText != oldWindowSelectedText && oldWindowSelectedText == "")
                    //    //{
                    //        Paster.ClickOnPoint(oldChildWindow, oldCaretPoint);
                    //    //}
                    //}

                    //AttachThreadInput(GetCurrentThreadId(), remoteThreadId, true);
                    //Point PosBeforeChange;
                    //GetCaretPos(out PosBeforeChange);
                    //Point currentPos;
                    //int result = SetCaretPos(lastCaretPoint.X, lastCaretPoint.Y);
                    //int ErrorCode = Marshal.GetLastWin32Error(); // Always return 5 - Access denied
                    //for (int i = 0; i < 500; i += waitStep)
                    //{
                    //    GetCaretPos(out currentPos);
                    //    if (PosBeforeChange != currentPos)
                    //        break;
                    //    Thread.Sleep(waitStep);
                    //}
                    //AttachThreadInput(GetCurrentThreadId(), remoteThreadId, false);
                }
            }

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
                        return true;
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
                    return true;
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
                lastPasteMoment = DateTime.Now;
            }
            else
            {
                if (!IsTextType())
                    return true;
                if (!needElevation)
                    Paster.SendChars();
                else
                {
                    EventWaitHandle sendCharsEvent = Paster.GetSendCharsEventWaiter();
                    sendCharsEvent.Set();
                }
            }
            return false;
        }

        private string getActiveWindowSelectedText()
        {
            string newActiveWindowSelectedText = "";
            //RemoveClipboardFormatListener(this.Handle);
            BackupClipboard();
            //Clipboard.Clear(); // Если не делать, то ошибка записи в буфер обмена потом возникает с большей вероятностью
            UsualClipboardMode = true;
            Paster.SendCopy(true);
            int waitStep = 10;
            for (int i = 0; i < 100; i += waitStep)
            {
                Application.DoEvents();
                if (!UsualClipboardMode)
                {
                    newActiveWindowSelectedText = Clipboard.GetText(TextDataFormat.UnicodeText);
                    break;
                }
                Thread.Sleep(waitStep);
            }
            Debug.WriteLine("UsualClipboardMode = " + UsualClipboardMode);
            UsualClipboardMode = false;
            Debug.WriteLine("selected text = " + newActiveWindowSelectedText);
            RestoreClipboard();
            //ConnectClipboard();
            return newActiveWindowSelectedText;
        }

        private void ShowElevationFail()
        {
            MessageBox.Show(this, CurrentLangResourceManager.GetString("CantPasteInElevatedWindow"),
                Application.ProductName);
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

        private void SetRowMark(string fieldName, bool newValue = true, bool allSelected = false, bool addToLastPasted = false)
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
            List<int> selectedIDs = new List<int>();
            foreach (DataGridViewRow selectedRow in selectedRows)
            {
                if (selectedRow == null)
                    continue;
                DataRowView dataRow = (DataRowView) selectedRow.DataBoundItem;
                string parameterName = "@Id" + counter;
                sql += "," + parameterName;
                command.Parameters.Add(parameterName, DbType.Int32).Value = dataRow["Id"];
                selectedIDs.Insert(0, (int) dataRow["Id"]);
                counter++;
                dataRow[fieldName] = newValue;
                UpdateTableGridRowBackColor(selectedRow);
                if (addToLastPasted)
                {
                    lastPastedClips[(int)dataRow["Id"]] = DateTime.Now;
                    if (lastPastedClips.Count > 100)
                        lastPastedClips.Remove(lastPastedClips.Aggregate((l, r) => l.Value < r.Value ? l : r).Key);
                }
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
            {
                LoadRowReader();
                UpdateClipButtons();
            }
        }

        private void UpdateClipButtons()
        {
            toolStripButtonMarkFavorite.Checked = BoolFieldValue("Favorite");
            // dataGridView.CurrentRow could be null here!
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
            int filterSelectionLength = comboBoxFilter.SelectionLength;
            int filterSelectionStart = comboBoxFilter.SelectionStart;

            StringCollection lastFilterValues = Properties.Settings.Default.LastFilterValues;
            comboBoxFilter.Items.Clear();
            foreach (string String in lastFilterValues)
            {
                comboBoxFilter.Items.Add(String);
            }

            // For some reason selection is reset. So we restore it
            comboBoxFilter.SelectionStart = filterSelectionStart;
            comboBoxFilter.SelectionLength = filterSelectionLength;
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

        [DllImport("psapi.dll", CharSet = CharSet.Unicode)]
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
            const int lengthSb = 1000; // Dirty
            var sb = new StringBuilder(lengthSb);
            string result = null;
            // Possibly there is no such fuction in Windows 7 https://stackoverflow.com/a/321343/4085971
            if (GetModuleFileNameEx(processHandle, IntPtr.Zero, sb, lengthSb) > 0)
            {
                result = sb.ToString();
            }
            CloseHandle(processHandle);
            return result;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetOpenClipboardWindow();

        static public void GetClipboardOwnerLockerInfo(bool Locker, out string window, out string application,
            out string appPath, bool replaceNullWithLastActive = true)
        {
            IntPtr hwnd;
            if (Locker)
                hwnd = GetOpenClipboardWindow();
            else
                hwnd = GetClipboardOwner();
            if (hwnd == IntPtr.Zero)
            {
                if (replaceNullWithLastActive)
                    hwnd = lastActiveParentWindow;
                else
                {
                    window = null;
                    application = null;
                    appPath = null;
                    return;
                }
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
            uint scanCode = MapVirtualKey((uint) keyCode, 0);
            uint lParam = 0x00000001 | (scanCode << 16);
            if (extended)
            {
                lParam |= 0x01000000;
            }
            if (down)
            {
                PostMessage(hwnd, WM_KEYDOWN, (int) keyCode, (int) lParam);
            }
            lParam |= 0xC0000000; // set previous key and transition states (bits 30 and 31)
            if (up)
            {
                PostMessage(hwnd, WM_KEYUP, (int) keyCode, (int) lParam);
            }
        }

        private static string GetWindowTitle(IntPtr hwnd)
        {
            int nChars = (GetWindowTextLength(hwnd) + 1) * 2; // Multiply 2 is made for possible fix of crash https://sourceforge.net/p/clip-angel/tickets/20/
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
            SendPasteOfSelectedTextOrSelectedClips();
        }

        private void pasteOriginalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendPasteOfSelectedTextOrSelectedClips();
        }

        private void pasteAsTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendPasteOfSelectedTextOrSelectedClips(PasteMethod.PasteText);
        }

        private void SendPasteOfSelectedTextOrSelectedClips(PasteMethod pasteMethod = PasteMethod.Standart)
        {
            string agregateTextToPaste = "";
            string selectedText = "";
            PasteMethod itemPasteMethod;
            int count = 0;
            if (pasteMethod == PasteMethod.File)
            {
                DataObject dto = new DataObject();
                SetClipFilesInDataObject(dto);
                SetClipboardDataObject(dto, false);
                SendPaste();
            }
            else
            {
                if (pasteMethod == PasteMethod.Standart)
                    itemPasteMethod = pasteMethod;
                else
                {
                    itemPasteMethod = PasteMethod.Null;
                    selectedText = GetSelectedText();
                    if (!String.IsNullOrEmpty(selectedText))
                        agregateTextToPaste = selectedText;
                }
                if (String.IsNullOrEmpty(agregateTextToPaste))
                {
                    agregateTextToPaste = JoinOrPasteTextOfClips(itemPasteMethod, out count);
                }
                if (itemPasteMethod == PasteMethod.Null && !String.IsNullOrEmpty(agregateTextToPaste))
                {
                    SetTextInClipboard(agregateTextToPaste, false);
                    SendPaste(pasteMethod);
                }
            }

            if (String.IsNullOrEmpty(selectedText))
            {
                SetRowMark("Used", true, true, true);
            }
            if (true
                && Properties.Settings.Default.MoveCopiedClipToTop
                && String.IsNullOrEmpty(selectedText)
                //&& count == 1
                )
            {
                MoveSelectedRows(0);
                //CaptureClipboardData();
            }
            else if (true
                     && pasteMethod == PasteMethod.PasteText
                     && !String.IsNullOrEmpty(selectedText))
            {
                // With multipaste works incorrect
                CaptureClipboardData();
                if (Properties.Settings.Default.MoveCopiedClipToTop)
                    MoveSelectedRows(0);
            }
        }

        private string JoinOrPasteTextOfClips(PasteMethod itemPasteMethod, out int count)
        {
            string agregateTextToPaste = "";
            bool pasteDelimiter = false;
            count = dataGridView.SelectedRows.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                DataGridViewRow selectedRow = dataGridView.SelectedRows[i];
                agregateTextToPaste += SendPasteClipExpress(selectedRow, itemPasteMethod, pasteDelimiter);
                pasteDelimiter = true;
            }
            return agregateTextToPaste;
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (!allowProcessDataGridSelectionChanged)
                return;
            if (allowRowLoad)
            {
                if (EditMode)
                    editClipTextToolStripMenuItem_Click();
                else
                    LoadClipIfChangedID();
                SaveFilterInLastUsedList();
            }
            if (dataGridView.Focused || comboBoxFilter.Focused)
            {
                allowProcessDataGridSelectionChanged = false;
                dataGridView.SuspendLayout();
                if (true
                    && selectedRangeStart >= 0
                    && dataGridView.CurrentRow != null
                    && (ModifierKeys & Keys.Shift) != 0)
                {
                    // Make natural (2 directions) order of range selected rows
                    int lastIndex = dataGridView.CurrentRow.Index;
                    int firstIndex = selectedRangeStart;
                    int step;
                    if (firstIndex > lastIndex)
                        step = -1;
                    else
                        step = +1;
                    for (int i = firstIndex; i != lastIndex + step; i += step)
                    {
                        dataGridView.Rows[i].Selected = false;
                        dataGridView.Rows[i].Selected = true;
                    }
                }
                dataGridView.ResumeLayout();
                if ((ModifierKeys & Keys.Shift) == 0)
                {
                    if (dataGridView.CurrentRow == null)
                        selectedRangeStart = -1;
                    else
                        selectedRangeStart = dataGridView.CurrentRow.Index;
                }
                allowProcessDataGridSelectionChanged = true;
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

        private void SaveClipUrl(string Url)
        {
            string sql = "Update Clips set Url = @Url where Id = @Id";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.Parameters.AddWithValue("@Id", RowReader["Id"]);
            command.Parameters.AddWithValue("@Url", Url);
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
                if (this.Top > maxWindowCoordForHiddenState)
                    factualTop = this.Top;
            }
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowForPaste();
            }
        }

        [DllImport("user32.dll", EntryPoint = "GetGUIThreadInfo")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetGUIThreadInfo(uint tId, out GUITHREADINFO threadInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ClientToScreen(IntPtr hWnd, out System.Drawing.Point position);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr handle, out RECT lpRect);

        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool GetClientRect(IntPtr hWnd, ref RECT rect);

        [DllImport("user32", SetLastError = true)]
        private extern static int GetCaretPos(out System.Drawing.Point p);
        [DllImport("user32", SetLastError = true)]
        private extern static int SetCaretPos(int x, int y);

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
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        private void ShowForPaste(bool onlyFavorites = false, bool clearFiltersAndGoToTop = false)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form.Modal && form.Visible)
                {
                    form.Activate();
                    return;
                }
            }

            //if (this.CanFocus) // With FastOpenWindow=False and TopMost=True is always false
            //{
            if (onlyFavorites)
                showOnlyFavoriteToolStripMenuItem_Click();
            else if (MarkFilter.SelectedValue.ToString() == "favorite")
                showAllMarksToolStripMenuItem_Click();
            //if (Properties.Settings.Default.SelectTopClipOnOpen)
            //    GotoLastRow();
            //else 
            if (clearFiltersAndGoToTop)
                ClearFilter(-1);
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //this.SuspendLayout();
            if (this.Visible && this.ContainsFocus)
            {
                RestoreWindowIfMinimized();
                return;
            }
            // https://www.codeproject.com/Articles/34520/Getting-Caret-Position-Inside-Any-Application
            // http://stackoverflow.com/questions/31055249/is-it-possible-to-get-caret-position-in-word-to-update-faster
            UpdateLastActiveParentWindow(IntPtr.Zero); // sometimes lastActiveParentWindow is not equal GetForegroundWindow()
            IntPtr hWindow = lastActiveParentWindow;
            if (hWindow != IntPtr.Zero)
            {
                Point caretPoint;
                GUITHREADINFO guiInfo = GetGuiInfo(hWindow, out caretPoint);
                if (caretPoint.Y > 0 && Properties.Settings.Default.RestoreCaretPositionOnFocusReturn)
                {
                    lastChildWindow = guiInfo.hwndFocus;
                    //lastWindowSelectedText = getActiveWindowSelectedText();
                    GetWindowRect(lastChildWindow, out lastChildWindowRect);
                    lastCaretPoint = new Point(guiInfo.rcCaret.left, guiInfo.rcCaret.top);

                    //int pid;
                    //uint remoteThreadId = GetWindowThreadProcessId(hWindow, out pid);
                    //AttachThreadInput(GetCurrentThreadId(), remoteThreadId, true);
                    //int Result = GetCaretPos(out lastCaretPoint);
                    //AttachThreadInput(GetCurrentThreadId(), remoteThreadId, false);
                }
                if (Properties.Settings.Default.WindowAutoPosition)
                {
                    int newX = -1;
                    int newY = -1;
                    RECT activeRect;
                    if (caretPoint.Y > 0)
                    {
                        activeRect = guiInfo.rcCaret;
                        Screen screen = Screen.FromPoint(caretPoint);
                        newX = Math.Max(screen.WorkingArea.Left, Math.Min(activeRect.right + caretPoint.X, screen.WorkingArea.Width + screen.WorkingArea.Left - this.Width));
                        newY = Math.Max(screen.WorkingArea.Top, Math.Min(activeRect.bottom + caretPoint.Y + 1, screen.WorkingArea.Height + screen.WorkingArea.Top - this.Height));
                    }
                    else
                    {
                        IntPtr baseWindow;
                        if (guiInfo.hwndFocus != IntPtr.Zero)
                            baseWindow = guiInfo.hwndFocus;
                        else
                            baseWindow = hWindow;
                        ClientToScreen(baseWindow, out caretPoint);
                        GetWindowRect(baseWindow, out activeRect);
                        Screen screen = Screen.FromPoint(caretPoint);
                        newX = Math.Max(screen.WorkingArea.Left,
                            Math.Min((activeRect.right - activeRect.left - this.Width) / 2 + caretPoint.X, screen.WorkingArea.Width + screen.WorkingArea.Left - this.Width));
                        newY = Math.Max(screen.WorkingArea.Top,
                            Math.Min((activeRect.bottom - activeRect.top - this.Height) / 2 + caretPoint.Y, screen.WorkingArea.Height + screen.WorkingArea.Top - this.Height));
                    }
                    RestoreWindowIfMinimized(newX, newY);
                }
            }
            //sw.Stop();
            //Debug.WriteLine("autoposition duration" + sw.ElapsedMilliseconds.ToString());
            if (!Properties.Settings.Default.FastWindowOpen)
            {
                this.Activate();
                this.Show();
            }
            SetForegroundWindow(this.Handle);
            //this.ResumeLayout();
        }

        private static GUITHREADINFO GetGuiInfo(IntPtr hWindow, out Point point)
        {
            int pid;
            uint remoteThreadId = GetWindowThreadProcessId(hWindow, out pid);
            var guiInfo = new GUITHREADINFO();
            guiInfo.cbSize = (uint) Marshal.SizeOf(guiInfo);
            GetGUIThreadInfo(remoteThreadId, out guiInfo);
            point = new Point(0, 0);
            ClientToScreen(guiInfo.hwndCaret, out point);
            //AttachThreadInput(GetCurrentThreadId(), remoteThreadId, true);
            //int Result = GetCaretPos(out point);
            //AttachThreadInput(GetCurrentThreadId(), remoteThreadId, false);
            // Screen.FromHandle(hwnd)
            return guiInfo;
        }

        private void RestoreWindowIfMinimized(int newX = -12345, int newY = -12345)
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            if (!allowVisible)
            {
                // executed only in first call of process life
                allowVisible = true;
                Show();
            }
            UpdateWindowTitle(true);
            if (newX == -12345)
            {
                if (this.Left > maxWindowCoordForHiddenState)
                    newX = this.Left;
                else
                    newX = this.RestoreBounds.X;
            }
            if (newY == -12345)
            {
                if (this.Top > maxWindowCoordForHiddenState)
                    newY = this.Top;
                else
                    newY = this.RestoreBounds.Y;
            }
            if (Properties.Settings.Default.FastWindowOpen)
            {
                if (newY <= maxWindowCoordForHiddenState)
                    newY = factualTop;
                if (newX > maxWindowCoordForHiddenState)
                    MoveWindow(this.Handle, newX, newY, this.Width, this.Height, true);
            }
            else
            {
                this.Left = newX;
                this.Top = newY;
            }
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal; // Window can be minimized by "Minimize All" command
            this.Activate(); // Without it window can be shown and be not focused
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
                if (ProcessEnterKeyDown(e.Control))
                    return;
                e.Handled = true;
            }
            if (e.KeyCode == Keys.Tab)
            {
                e.Handled = true;
                FocusClipText();
            }
        }

        private bool ProcessEnterKeyDown(bool isControlPressed)
        {
            PasteMethod pasteMethod;
            if (isControlPressed)
                pasteMethod = PasteMethod.PasteText;
            else
            {
                //if (!pasteENTERToolStripMenuItem.Enabled)
                if (richTextBox.Focused && EditMode)
                    return true;
                pasteMethod = PasteMethod.Standart;
            }
            SendPasteOfSelectedTextOrSelectedClips(pasteMethod);
            return false;
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

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearFilter_Click();
        }

        private void gotoLastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GotoLastRow();
        }

        void GotoLastRow(bool keepTextSelectionIfIDChanged = false)
        {
            if (dataGridView.Rows.Count > 0)
            {
                allowRowLoad = false;
                clipBindingSource.MoveFirst(); // It changes selected row
                allowRowLoad = true;
                if (dataGridView.CurrentRow != null)
                    SelectCurrentRow(false, true, true, keepTextSelectionIfIDChanged);
            }
            LoadClipIfChangedID(false, true, keepTextSelectionIfIDChanged);
        }

        private void UpdateSelectedClipsHistory()
        {
            if (RowReader != null)
            {
                int oldID = (int) RowReader["id"];
                if (!selectedClipsBeforeFilterApply.Contains(oldID))
                    selectedClipsBeforeFilterApply.Add(oldID);
            }
        }

        private bool CurrentIDChanged()
        {
            return false
                   || (RowReader == null && dataGridView.CurrentRow != null)
                   || (RowReader != null && dataGridView.CurrentRow == null)
                   || !(true
                        && RowReader != null
                        && dataGridView.CurrentRow != null
                        && (int) (dataGridView.CurrentRow.DataBoundItem as DataRowView)["ID"] == (int) RowReader["ID"]);
        }

        void SelectCurrentRow(bool forceRowLoad = false, bool keepTextSelection = true, bool clearSelection = true,
            bool keepTextSelectionIfIDChanged = false)
        {
            if (clearSelection)
            {
                allowRowLoad = false;
                dataGridView.ClearSelection();
                allowRowLoad = true;
            }
            if (dataGridView.CurrentRow == null)
            {
                GotoLastRow();
                return;
            }
            allowRowLoad = false;
            dataGridView.Rows[dataGridView.CurrentRow.Index].Selected = true;
            allowRowLoad = true;
            LoadClipIfChangedID(forceRowLoad, keepTextSelection, keepTextSelectionIfIDChanged);
        }

        private void LoadClipIfChangedID(bool forceRowLoad = false, bool keepTextSelection = true,
            bool keepTextSelectionIfIDChanged = false)
        {
            bool currentIDChanged = CurrentIDChanged();
            if (forceRowLoad || currentIDChanged)
            {
                if (currentIDChanged)
                {
                    if (!keepTextSelectionIfIDChanged)
                        keepTextSelection = false;
                    EditMode = false;
                    UpdateControlsStates();
                }
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


        private void activateListToolStripMenuItem_Click(object sender = null, EventArgs e = null)
        {
            if (dataGridView.Focused)
                FocusClipText();
            else
                dataGridView.Focus();
        }

        private void FocusClipText()
        {
            if (htmlMode)
                htmlTextBox.Document.Focus();
            else if (richTextBox.Enabled)
                richTextBox.Focus();
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

        private void LoadVisibleRows()
        {
            int visibleRowsCount = dataGridView.DisplayedRowCount(true);
            int firstDisplayedRowIndex = dataGridView.FirstDisplayedCell.RowIndex;
            int lastDisplayedRowIndex = Math.Min(firstDisplayedRowIndex + visibleRowsCount, dataGridView.RowCount - 1);
            bool needLoad = false;
            for (int rowIndex = firstDisplayedRowIndex; rowIndex <= lastDisplayedRowIndex; rowIndex++)
            {
                DataRowView drv = (dataGridView.Rows[rowIndex].DataBoundItem as DataRowView);
                if (String.IsNullOrEmpty(drv["type"].ToString()))
                {
                    needLoad = true;
                    break;
                }
            }
            if (!needLoad)
                return;
            LoadDataboundItems(firstDisplayedRowIndex, lastDisplayedRowIndex);
        }

        private void LoadDataboundItems(int firstDisplayedRowIndex, int lastDisplayedRowIndex)
        {
            int bufferSize = 50;
            int firstLoadedRowIndex = Math.Max(firstDisplayedRowIndex - bufferSize, 0);
            int lastLoadedRowIndex = Math.Min(lastDisplayedRowIndex + bufferSize, dataGridView.RowCount - 1);
            SQLiteCommand command = new SQLiteCommand(m_dbConnection);
            string queryText = "";
            for (int rowIndex = firstLoadedRowIndex; rowIndex <= lastLoadedRowIndex; rowIndex++)
            {
                DataRowView drv = (dataGridView.Rows[rowIndex].DataBoundItem as DataRowView);
                if (!String.IsNullOrEmpty(drv["type"].ToString()))
                    continue;
                if (!String.IsNullOrEmpty(queryText))
                {
                    queryText += "\n\rUNION ALL\n\r";
                }
                int rowId = (int) drv["id"];
                // Dublicated code 8gfd8843
                queryText += "Select " + rowIndex + " AS _Index, Id, Used, Title, Chars, Type, Favorite, ImageSample, AppPath, Size, Created From Clips WHERE Id = " + rowId;
            }
            if (String.IsNullOrEmpty(queryText))
                return;
            command.CommandText += queryText;
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                DataTable table = (DataTable) clipBindingSource.DataSource;
                while (reader.Read())
                {
                    int rowIndex = reader.GetInt32(reader.GetOrdinal("_Index"));
                    DataRowView row = (dataGridView.Rows[rowIndex].DataBoundItem as DataRowView);
                    //foreach (var VARIABLE in table.Columns)
                    //{
                    //}
                    row["Used"] = reader.GetBoolean(reader.GetOrdinal("used"));
                    row["Title"] = reader.GetString(reader.GetOrdinal("Title"));
                    row["Chars"] = reader.GetInt32(reader.GetOrdinal("Chars"));
                    row["Type"] = reader.GetString(reader.GetOrdinal("Type"));
                    row["Favorite"] = reader.IsDBNull(reader.GetOrdinal("Favorite")) ? false : reader.GetBoolean(reader.GetOrdinal("Favorite"));
                    row["ImageSample"] = reader.GetValue(reader.GetOrdinal("ImageSample"));
                    row["AppPath"] = reader.GetValue(reader.GetOrdinal("AppPath"));
                    row["Size"] = reader.GetInt32(reader.GetOrdinal("Size"));
                    row["Created"] = reader.IsDBNull(reader.GetOrdinal("Created")) ? new DateTime() : reader.GetDateTime(reader.GetOrdinal("Created"));
                }
            }
        }

        private void PrepareRow(DataGridViewRow row = null)
        {
            LoadVisibleRows();
            if (row == null)
                row = dataGridView.CurrentRow;
            DataRowView dataRowView = (DataRowView) row.DataBoundItem;
            int shortSize = dataRowView.Row["Chars"].ToString().Length;
            if (shortSize > 2)
                row.Cells["VisualWeight"].Value = shortSize;
            string clipType = dataRowView.Row["Type"].ToString();
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
                row.Cells["TypeImage"].Value = image;
            }
            row.Cells["ColumnTitle"].Value = dataRowView.Row["Title"].ToString();

            string textPattern = RegexpPatternFromTextFilter();
            if (!String.IsNullOrEmpty(textPattern))
            {
                _richTextBox.Clear();
                _richTextBox.Font = dataGridView.RowsDefaultCellStyle.Font;
                _richTextBox.Text = row.Cells["ColumnTitle"].Value.ToString();
                MatchCollection tempMatches;
                MarkRegExpMatchesInRichTextBox(_richTextBox, textPattern, Color.Red, false, true, out tempMatches);
                row.Cells["ColumnTitle"].Value = _richTextBox.Rtf;
            }
            var imageSampleBuffer = dataRowView["ImageSample"];
            if (imageSampleBuffer != DBNull.Value)
                if ((imageSampleBuffer as byte[]).Length > 0)
                {
                    Image imageSample = GetImageFromBinary((byte[]) imageSampleBuffer);
                    row.Cells["imageSample"].Value = ChangeImageOpacity(imageSample, 0.8f);
                    ////string str = BitConverter.ToString((byte[])imageSampleBuffer, 0).Replace("-", string.Empty);
                    ////string imgString = @"{\pict\pngblip\picw" + imageSample.Width + @"\pich" + imageSample.Height + @"\picwgoal" + imageSample.Width + @"\pichgoal" + imageSample.Height + @"\bin " + str + "}";
                    //string imgString = GetEmbedImageString((Bitmap)imageSample, 0, 18);
                    //_richTextBox.SelectionStart = _richTextBox.TextLength;
                    //_richTextBox.SelectedRtf = imgString;
                }
            if (dataGridView.Columns["AppImage"].Visible)
            {
                var bitmap = ApplicationIcon(dataRowView["appPath"].ToString(), false);
                if (bitmap != null)
                    row.Cells["AppImage"].Value = bitmap;
            }
            UpdateTableGridRowBackColor(row);
        }

        private string RegexpPattern()
        {
            string textPattern = "";
            if (!String.IsNullOrEmpty(filterText))
                textPattern = RegexpPatternFromTextFilter();
            else
            {
                string filterValue = TypeFilter.SelectedValue as string;
                if (filterValue.Contains("text") && filterValue != "text")
                    textPattern = TextPatterns[filterValue.Substring("text_".Length)];
            }
            return textPattern;
        }

        private string RegexpPatternFromTextFilter()
        {
            string result = filterText;
            string[] array;
            if (Properties.Settings.Default.SearchWordsIndependently)
                array = result.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            else
                array = new string[1] {result};
            result = "";
            foreach (var word in array)
            {
                if (result != "")
                    result += "|";
                result += "(" + Regex.Escape(word) + ")";
            }
            if (Properties.Settings.Default.SearchWildcards)
                result = result.Replace("%", ".*?");
            return "(" + result + ")";
        }

        static public Bitmap ApplicationIcon(string filePath, bool original = true)
        {
            Bitmap originalImage = null;
            Bitmap brightImage = null;
            if (originalIconCache.ContainsKey(filePath))
            {
                originalImage = originalIconCache[filePath];
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
                originalIconCache[filePath] = originalImage;
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

            Bitmap bmp = (Bitmap) originalImage.Clone();

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

                argbValues[counter + pos] = (byte) (argbValues[counter + pos] * opacity);
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
            string mpic = @"{\pict\wbitmapN\picw" + img.Width + @"\pich" + img.Height + @"\picwgoal" + width +
                          @"\pichgoal" + height + @"\bin " + str + "}";
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
        private static extern uint GdipEmfToWmfBits(IntPtr _hEmf, uint _bufferSize, byte[] _buffer, int _mappingMode, EmfToWmfBitsFlags _flags);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SetMetaFileBitsEx(uint _bufferSize, byte[] _buffer);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CopyMetaFile(IntPtr hWmf, string filename);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteMetaFile(IntPtr hWmf);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteEnhMetaFile(IntPtr hEmf);

        public static string GetEmbedImageString(Bitmap image, int width = 0, int height = 0)
        {
            Metafile metafile = null;
            float dpiX;
            float dpiY;
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

            string proto = @"{\rtf1{\pict\wmetafile8\picw" + (int) (((float) width / dpiX) * 2540)
                           + @"\pich" + (int) (((float) height / dpiY) * 2540)
                           + @"\picwgoal" + (int) (((float) width / dpiX) * 1440)
                           + @"\pichgoal" + (int) (((float) height / dpiY) * 1440)
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
            g.DrawString(recValue, dataGridView1.DefaultCellStyle.Font,
                new SolidBrush(dataGridView1.DefaultCellStyle.ForeColor), newCell.X + 3, newCell.Y + 3);
        }

        public static Bitmap CopyRectImage(Bitmap bitmap, Rectangle selection)
        {
            int newBottom = selection.Bottom;
            if (selection.Bottom > bitmap.Height)
                newBottom = bitmap.Height;
            int newRight = selection.Right;
            if (selection.Right > bitmap.Width)
                newRight = bitmap.Width;
            // TODO check other borders
            // Sometimes Clone() raises strange OutOfMemory exception http://www.codingdefined.com/2015/04/solved-bitmapclone-out-of-memory.html
            //Bitmap RectImage = bitmap.Clone(
            //    new Rectangle(selection.Left, selection.Top, newRight, newBottom), bitmap.PixelFormat);
            Bitmap RectImage = new Bitmap(selection.Width, selection.Height);
            using (Graphics gph = Graphics.FromImage(RectImage))
            {
                gph.DrawImage(bitmap, new Rectangle(0, 0, RectImage.Width, RectImage.Height), selection, GraphicsUnit.Pixel);
            }
            return RectImage;
        }

        private void UpdateTableGridRowBackColor(DataGridViewRow row = null)
        {
            if (row == null)
                row = dataGridView.CurrentRow;
            DataRowView dataRowView = (DataRowView) (row.DataBoundItem);
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
            {
                //dataRowView = (DataRowView)(dataGridView.CurrentRow.DataBoundItem);
                dataRowView = (DataRowView) (clipBindingSource.Current);
                if (dataRowView != null && RowReader != null)
                    dataRowView[fieldName] = RowReader[fieldName]; // DataBoundItem can be not read yet
            }
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
            string oldDatabaseFile = Properties.Settings.Default.DatabaseFile;
            settingsFormForm.ShowDialog(this);
            if (settingsFormForm.DialogResult == DialogResult.OK)
            {
                if (oldDatabaseFile != Properties.Settings.Default.DatabaseFile)
                {
                    CloseDatabase();
                    OpenDatabase();
                }
                LoadSettings();
                keyboardHook.UnregisterHotKeys();
                RegisterHotKeys();
                DeleteOldClips();
                DeleteExcessClips();
                UpdateClipBindingSource(); // Needed to update ClipsNumber
                Properties.Settings.Default.Save(); // Not all properties are saved here. For example ShowInTaskbar are not saved
            }
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

            UpdateIgnoreModulesInClipCapture();
            BindingList<ListItemNameText> comboItemsTypes = (BindingList<ListItemNameText>) TypeFilter.DataSource;
            foreach (ListItemNameText item in comboItemsTypes)
            {
                item.Text = CurrentLangResourceManager.GetString(item.Name);
            }
            // To refresh text in list
            TypeFilter.DisplayMember = "";
            TypeFilter.DisplayMember = "Text";
            VisibleUserSettings allSettings = new VisibleUserSettings(this);
            toolStripButtonAutoSelectMatch.ToolTipText = allSettings.GetProperties().Find("AutoSelectMatch", true).Description;
            toolStripMenuItemSearchCaseSensitive.ToolTipText = allSettings.GetProperties().Find("SearchCaseSensitive", true).Description;
            toolStripMenuItemSearchWordsIndependently.ToolTipText = allSettings.GetProperties().Find("SearchWordsIndependently", true).Description;
            toolStripMenuItemSearchWildcards.ToolTipText = allSettings.GetProperties().Find("SearchWildcards", true).Description;
            moveCopiedClipToTopToolStripButton.ToolTipText = allSettings.GetProperties().Find("MoveCopiedClipToTop", true).Description;
            moveCopiedClipToTopToolStripMenuItem.ToolTipText = allSettings.GetProperties().Find("MoveCopiedClipToTop", true).Description;
            textFormattingToolStripMenuItem.ToolTipText = allSettings.GetProperties().Find("ShowNativeTextFormatting", true).Description;
            toolStripButtonTextFormatting.ToolTipText = allSettings.GetProperties().Find("ShowNativeTextFormatting", true).Description;
            toolStripButtonMonospacedFont.ToolTipText = allSettings.GetProperties().Find("MonospacedFont", true).Description;
            monospacedFontToolStripMenuItem.ToolTipText = allSettings.GetProperties().Find("MonospacedFont", true).Description;
            wordWrapToolStripMenuItem.ToolTipText = allSettings.GetProperties().Find("WordWrap", true).Description;
            toolStripButtonWordWrap.ToolTipText = allSettings.GetProperties().Find("WordWrap", true).Description;
            toolStripButtonSecondaryColumns.ToolTipText = allSettings.GetProperties().Find("ShowSecondaryColumns", true).Description;
            toolStripMenuItemSecondaryColumns.ToolTipText = allSettings.GetProperties().Find("ShowSecondaryColumns", true).Description;

            BindingList<ListItemNameText> comboItemsMarks = (BindingList<ListItemNameText>) MarkFilter.DataSource;
            foreach (ListItemNameText item in comboItemsMarks)
            {
                item.Text = CurrentLangResourceManager.GetString(item.Name);
            }
            // To refresh text in list
            MarkFilter.DisplayMember = "";
            MarkFilter.DisplayMember = "Text";
            Properties.Settings.Default.RestoreCaretPositionOnFocusReturn = false; // disabled
            dataGridView.RowsDefaultCellStyle.Font = Properties.Settings.Default.Font;
            dataGridView.Columns["ColumnCreated"].DefaultCellStyle.Format = "HH:mmm:ss dd.MM";
            UpdateColumnsSet();
            AfterRowLoad();
            this.ResumeLayout();
        }

        private void UpdateColumnsSet()
        {
            dataGridView.Columns["appImage"].Visible = Properties.Settings.Default.ShowApplicationIconColumn;
            //dataGridView.Columns["VisualWeight"].Visible = Properties.Settings.Default.ShowVisualWeightColumn;
            dataGridView.Columns["ColumnCreated"].Visible = Properties.Settings.Default.ShowSecondaryColumns;
        }

        private void UpdateIgnoreModulesInClipCapture()
        {
            ignoreModulesInClipCapture = new StringCollection();
            if (Properties.Settings.Default.IgnoreApplicationsClipCapture != null)
                foreach (var fullFilename in Properties.Settings.Default.IgnoreApplicationsClipCapture)
                {
                    ignoreModulesInClipCapture.Add(Path.GetFileNameWithoutExtension(fullFilename).ToLower());
                }
        }

        public async void CheckUpdate(bool UserRequest = false)
        {
            if (!UserRequest && !Properties.Settings.Default.AutoCheckForUpdate)
                return;
            buttonUpdate.Visible = false;
            toolStripUpdateToSeparator.Visible = false;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    string HtmlSource = await webClient.DownloadStringTaskAsync(Properties.Resources.Website);

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

                    Match match = Regex.Match(lastVersion, @"Clip\s*Angel (.*).zip");
                    if (match == null)
                        return;
                    ActualVersion = match.Groups[1].Value;
                    if (ActualVersion != Properties.Resources.VersionValue)
                    {
                        buttonUpdate.Visible = true;
                        toolStripUpdateToSeparator.Visible = true;
                        buttonUpdate.ForeColor = Color.Blue;
                        buttonUpdate.Text = CurrentLangResourceManager.GetString("UpdateTo") + " " + ActualVersion;
                        if (UserRequest)
                        {
                            MessageBox.Show(this, CurrentLangResourceManager.GetString("NewVersionAvailable"),
                                Application.ProductName);
                        }
                    }
                    else if (UserRequest)
                    {
                        MessageBox.Show(this, CurrentLangResourceManager.GetString("YouLatestVersion"),
                            Application.ProductName);
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
            using (WebClient webClient = new WebClient())
            {
                webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
                string tempFolder = Path.GetTempPath() + Guid.NewGuid();
                Directory.CreateDirectory(tempFolder);
                string tempFilenameZip = tempFolder + "\\NewVersion" + ".zip";
                bool success = true;
                //try
                //{
                webClient.DownloadFile(Properties.Resources.DownloadPage, tempFilenameZip);
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
                string exePath = Path.GetDirectoryName(Application.ExecutablePath);
                File.Copy(exePath + "\\" + UpdaterName, tempFolder + "\\" + UpdaterName);
                File.Copy(exePath + "\\" + "DotNetZip.dll", tempFolder + "\\DotNetZip.dll");
                if (success)
                {
                    string ExeParam = "";
                    if (PortableMode)
                        ExeParam = "/p";
                    Process.Start(tempFolder + "\\" + UpdaterName, "\"" + tempFilenameZip + "\" \"" + Application.StartupPath + "\" \""
                        + Application.ExecutablePath + "\" \"" + ExeParam + "\" " + Process.GetCurrentProcess().Id);
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
            //if (String.Compare(Locale, "ru", true) == 0)
            //    LangResourceManager = Properties.Resource_RU.ResourceManager;
            //else
                LangResourceManager = Properties.Resources.ResourceManager;
            return LangResourceManager;
        }

        static public string getCurrentLocale()
        {
            // Bad luck trying to use Multilang tool http://stackoverflow.com/questions/38117969/convert-resx-to-xliff-format-failed
            string locale;
            if (Properties.Settings.Default.Language == "Default")
                locale = Application.CurrentCulture.TwoLetterISOLanguageName;
            else if (Properties.Settings.Default.Language == "Chinese")
                locale = "zh-CN";
            else if (Properties.Settings.Default.Language == "German")
                locale = "de";
            else if (Properties.Settings.Default.Language == "Italian")
                locale = "it";
            else if (Properties.Settings.Default.Language == "Polish")
                locale = "pl";
            else if (Properties.Settings.Default.Language == "PortugueseBrazil")
                locale = "pt-BR";
            else if (Properties.Settings.Default.Language == "Spain")
                locale = "es";
            else if (Properties.Settings.Default.Language == "Hindi")
                locale = "hi";
            else if (Properties.Settings.Default.Language == "French")
                locale = "fr";
            else if (Properties.Settings.Default.Language == "Greek")
                locale = "el";
            else if (Properties.Settings.Default.Language == "Russian")
                locale = "ru";
            else
                locale = "en";
            return locale;
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Visible = false;
            //Properties.Settings.Default.Save(); // Not all properties were saved here. For example ShowInTaskbar was not saved
            RemoveClipboardFormatListener(this.Handle);
            UnhookWinEvent(HookChangeActiveWindow);
            CloseDatabase();
        }

        private void CloseDatabase()
        {
            if (updateDBThread != null)
            {
                stopUpdateDBThread = true;
                updateDBThread.Join();
            }
            if (RowReader != null)
                RowReader = null;
            m_dbConnection.Close();

            // Shrink database to really delete deleted clips
            m_dbConnection.Open();
            SQLiteCommand command = new SQLiteCommand("vacuum", m_dbConnection);
            command.ExecuteNonQuery();
            m_dbConnection.Close();
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
            if (Properties.Settings.Default.FastWindowOpen)
            {
                RestoreWindowIfMinimized();
            }
            SetForegroundWindow(this.Handle);
        }

        private void Filter_KeyPress(object sender, KeyPressEventArgs e)
        {
            // http://csharpcoding.org/tag/keypress/ Workaroud strange beeping 
            if (e.KeyChar == (char) Keys.Enter || e.KeyChar == (char) Keys.Escape)
                e.Handled = true;
        }

        private static void OpenLinkIfAltPressed(RichTextBox sender, EventArgs e, MatchCollection matches)
        {
            Keys mod = Control.ModifierKeys & Keys.Modifiers;
            bool altOnly = mod == Keys.Alt;
            if (altOnly)
                OpenLinkInRichTextBox(sender, matches);
        }

        private static void OpenLinkInRichTextBox(RichTextBox sender, MatchCollection matches)
        {
            foreach (Match match in matches)
            {
                if (match.Index <= sender.SelectionStart && (match.Index + match.Length) >= sender.SelectionStart)
                    Process.Start(match.Value);
            }
        }

        private void textBoxUrl_Click(object sender, EventArgs e)
        {
            OpenLinkIfAltPressed(sender as RichTextBox, e, UrlLinkMatches);
        }

        private void ImageControl_DoubleClick(object sender, EventArgs e)
        {
            OpenClipFile();
        }

        private void TypeFilter_SelectedValueChanged(object sender, EventArgs e)
        {
            if (AllowFilterProcessing)
            {
                UpdateClipBindingSource(true);
            }
        }

        private void buttonFindNext_Click(object sender = null, EventArgs e = null)
        {
            if (EditMode)
                return;
            SaveFilterInLastUsedList();
            if (TextWasCut)
                AfterRowLoad(true);
            SelectNextMatchInClipText();
        }

        private void SelectNextMatchInClipText()
        {
            if (htmlMode)
                SelectNextMatchInWebBrowser(1);
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
                        allowTextPositionChangeUpdate = false;
                        control.SelectionStart = match.Groups[1].Index;
                        control.SelectionLength = match.Groups[1].Length;
                        control.HideSelection = false;
                        allowTextPositionChangeUpdate = true;
                        UpdateSelectionPosition();
                        break;
                    }
                }
            }
        }

        private void SelectNextMatchInWebBrowser(int direction)
        {
            mshtml.IHTMLDocument2 htmlDoc = (mshtml.IHTMLDocument2) htmlTextBox.Document.DomDocument;
            mshtml.IHTMLBodyElement body = htmlDoc.body as mshtml.IHTMLBodyElement;
            mshtml.IHTMLTxtRange currentRange = GetHtmlCurrentTextRangeOrAllDocument();
            mshtml.IHTMLTxtRange nearestMatch = null;
            int searchFlags = 0;
            if (Properties.Settings.Default.SearchCaseSensitive)
                searchFlags = 4;
            string[] array;
            if (Properties.Settings.Default.SearchWordsIndependently)
                array = filterText.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            else
                array = new string[1] {filterText};
            foreach (var word in array)
            {
                mshtml.IHTMLTxtRange range = body.createTextRange();
                if (direction > 0)
                    range.setEndPoint("StartToEnd", currentRange);
                else
                    range.setEndPoint("EndToStart", currentRange);
                if (range.findText(word, direction, searchFlags))
                {
                    if (false
                        || nearestMatch == null
                        || (true
                            && direction > 0
                            && (false
                                ||
                                (nearestMatch as IHTMLTextRangeMetrics).boundingTop > (range as IHTMLTextRangeMetrics).boundingTop
                                || (true
                                    && (nearestMatch as IHTMLTextRangeMetrics).boundingTop == (range as IHTMLTextRangeMetrics).boundingTop
                                    && (nearestMatch as IHTMLTextRangeMetrics).boundingLeft > (range as IHTMLTextRangeMetrics).boundingLeft)))
                        || (true
                            && direction < 0
                            && (false
                                || (nearestMatch as IHTMLTextRangeMetrics).boundingTop < (range as IHTMLTextRangeMetrics).boundingTop
                                || (true
                                    && (nearestMatch as IHTMLTextRangeMetrics).boundingTop == (range as IHTMLTextRangeMetrics).boundingTop
                                    && (nearestMatch as IHTMLTextRangeMetrics).boundingLeft < (range as IHTMLTextRangeMetrics).boundingLeft))))
                    {
                        nearestMatch = range;
                    }
                }
            }
            if (nearestMatch != null)
            {
                nearestMatch.scrollIntoView();
                nearestMatch.@select();
            }
        }

        private mshtml.IHTMLTxtRange GetHtmlCurrentTextRangeOrAllDocument(bool onlySelection = false)
        {
            mshtml.IHTMLDocument2 htmlDoc = (mshtml.IHTMLDocument2) htmlTextBox.Document.DomDocument;
            mshtml.IHTMLBodyElement body = htmlDoc.body as mshtml.IHTMLBodyElement;
            mshtml.IHTMLSelectionObject sel = htmlDoc.selection;
            //sel.empty(); // get an empty selection, so we start from the beginning
            mshtml.IHTMLTxtRange range = (mshtml.IHTMLTxtRange) sel.createRange();
            if (range == null && !onlySelection)
                range = body.createTextRange();
            return range;
        }

        private void buttonFindPrevious_Click(object sender, EventArgs e)
        {
            if (EditMode)
                return;
            SaveFilterInLastUsedList();
            if (TextWasCut)
                AfterRowLoad(true);
            if (htmlMode)
                SelectNextMatchInWebBrowser(-1);
            else
            {
                RichTextBox control = richTextBox;
                if (FilterMatches == null)
                    return;
                Match prevMatch = null;
                foreach (Match match in FilterMatches)
                {
                    if (false
                        || control.SelectionStart > match.Groups[1].Index
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
                    control.SelectionStart = prevMatch.Groups[1].Index;
                    control.SelectionLength = prevMatch.Groups[1].Length;
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
            CopyClipToClipboard(null, false, Properties.Settings.Default.MoveCopiedClipToTop);
        }

        private void UpdateControlsStates()
        {
            toolStripMenuItemSecondaryColumns.Checked = Properties.Settings.Default.ShowSecondaryColumns;
            toolStripButtonSecondaryColumns.Checked = Properties.Settings.Default.ShowSecondaryColumns;
            toolStripMenuItemSearchCaseSensitive.Checked = Properties.Settings.Default.SearchCaseSensitive;
            toolStripMenuItemSearchWordsIndependently.Checked = Properties.Settings.Default.SearchWordsIndependently;
            toolStripMenuItemSearchWildcards.Checked = Properties.Settings.Default.SearchWildcards;
            ignoreBigTextsToolStripMenuItem.Checked = Properties.Settings.Default.SearchIgnoreBigTexts;
            moveCopiedClipToTopToolStripButton.Checked = Properties.Settings.Default.MoveCopiedClipToTop;
            moveCopiedClipToTopToolStripMenuItem.Checked = Properties.Settings.Default.MoveCopiedClipToTop;
            toolStripButtonAutoSelectMatch.Checked = Properties.Settings.Default.AutoSelectMatch;
            trayMenuItemMonitoringClipboard.Checked = MonitoringClipboard;
            toolStripMenuItemMonitoringClipboard.Checked = MonitoringClipboard;
            toolStripButtonTextFormatting.Checked = Properties.Settings.Default.ShowNativeTextFormatting;
            textFormattingToolStripMenuItem.Checked = Properties.Settings.Default.ShowNativeTextFormatting;
            toolStripButtonMonospacedFont.Checked = Properties.Settings.Default.MonospacedFont;
            monospacedFontToolStripMenuItem.Checked = Properties.Settings.Default.MonospacedFont;
            wordWrapToolStripMenuItem.Checked = Properties.Settings.Default.WordWrap;
            toolStripButtonWordWrap.Checked = Properties.Settings.Default.WordWrap;
            richTextBox.WordWrap = wordWrapToolStripMenuItem.Checked;
            //showInTaskbarToolStripMenuItem.Enabled = Properties.Settings.Default.FastWindowOpen;
            showInTaskbarToolStripMenuItem.Checked = Properties.Settings.Default.ShowInTaskBar;
            //if (Properties.Settings.Default.FastWindowOpen)
            //{
                this.ShowInTaskbar = Properties.Settings.Default.ShowInTaskBar;
                // After ShowInTaskbar change true->false all window properties are deleted. So we need to reset it.
                ResetIsMainProperty();
            //}
            editClipTextToolStripMenuItem.Checked = EditMode;
            toolStripMenuItemEditClipText.Checked = EditMode;
            //pasteENTERToolStripMenuItem.Enabled = !EditMode;
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
                InputBoxResult inputResult = InputBox.Show(CurrentLangResourceManager.GetString("HowUseAutoTitle"),
                    CurrentLangResourceManager.GetString("EditClipTitle"), oldTitle, this);
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
                    UpdateClipBindingSource(true);
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
                && e.KeyCode != Keys.Apps
                && e.KeyCode != Keys.F10
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

        private void timerDaily_Tick(object sender, EventArgs e)
        {
            CheckUpdate();
            DeleteOldClips();
            DeleteExcessClips();
            timerDaily.Interval = (1000 * 60 * 60 * 24); // 1 day
            timerDaily.Start();
        }

        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoveSelectedRows(-1);
        }

        // shiftType - 0 - TOP
        //            -1 - UP
        //             1 - DOWN
        private void MoveSelectedRows(int shiftType)
        {
            if (dataGridView.CurrentRow == null)
                return;
            int newID;
            int newCurrentID = 0;
            int currentRowIndex = dataGridView.CurrentRow.Index;
            List<int> seletedRowIndexes = new List<int>();
            int counter = 0;
            Dictionary<int, int> order = new Dictionary<int, int>();
            foreach (DataGridViewRow selectedRow in dataGridView.SelectedRows)
            {
                seletedRowIndexes.Add(selectedRow.Index);
                order.Add(counter, selectedRow.Index);
                counter++;
            }
            if (false
                || shiftType <= 0 && seletedRowIndexes.Contains(0)
                || shiftType > 0 && seletedRowIndexes.Contains(dataGridView.RowCount - 1))
            {
                return;
            }
            IOrderedEnumerable<int> SortedRowIndexes;
            if (shiftType < 0)
                SortedRowIndexes = seletedRowIndexes.OrderBy(i=>i);
            else
                SortedRowIndexes = seletedRowIndexes.OrderByDescending(i => i);
            foreach (int selectedRowIndex in SortedRowIndexes.ToList())
            {
                DataRow selectedDataRow = ((DataRowView)clipBindingSource[selectedRowIndex]).Row;
                int oldID = (int)selectedDataRow["ID"];
                if (shiftType != 0)
                {
                    DataRow exchangeDataRow = ((DataRowView)clipBindingSource[selectedRowIndex + shiftType]).Row;
                    newID = (int)exchangeDataRow["ID"];
                }
                else
                {
                    LastId++;
                    newID = LastId;
                }
                if (currentRowIndex == selectedRowIndex)
                    newCurrentID = newID;
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
                order[order.FirstOrDefault(x => x.Value == selectedRowIndex).Key] = newID;
                RegisterClipIdChange(oldID, newID);
            }
            List <int> ids = order.Select(d => d.Value).ToList();
            ids.Reverse();
            UpdateClipBindingSource(false, newCurrentID, true, ids);
        }

        private void RegisterClipIdChange(int oldID, int newID)
        {
            if (lastPastedClips.ContainsKey(oldID))
            {
                var value = lastPastedClips[oldID];
                lastPastedClips.Remove(oldID);
                lastPastedClips[newID] = value;
            }
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoveSelectedRows(1);
        }

        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(CurrentLangResourceManager.GetString("HistoryOfChanges")); // Returns 0. Why?
        }

        private void toolStripMenuItemPasteChars_Click(object sender, EventArgs e)
        {
            SendPasteOfSelectedTextOrSelectedClips(PasteMethod.SendChars);
        }

        private void openInDefaultApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenClipFile();
        }

        private void OpenClipFile(bool defaultAppMode = true)
        {
            string fileEditor = "";
            string tempFile = GetClipTempFile(out fileEditor);
            if (String.IsNullOrEmpty(tempFile))
                return;
            try
            {
                if (defaultAppMode)
                {
                    string command;
                    string argument = "";
                    if (!String.IsNullOrEmpty(fileEditor))
                    {
                        command = fileEditor;
                        argument = "\"" + tempFile + "\"";
                    }
                    else
                    {
                        command = tempFile;
                    }
                    Process.Start(command, argument);
                }
                else
                {
                    // http://stackoverflow.com/questions/4726441/how-can-i-show-the-open-with-file-dialog

                    // Not reliable
                    var args = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
                    args += ",OpenAs_RunDLL \"" + tempFile + "\"";
                    Process.Start("rundll32.exe", args);

                    //// Not reliable
                    //var processInfo = new ProcessStartInfo(tempFile);
                    //processInfo.Verb = "openas";
                    //processInfo.ErrorDialog = true;
                    //Process.Start(processInfo);
                }
                //if (deleteAfterOpen)
                //{
                //    //Thread.Sleep(1000); // to be almost sure that file has been opened
                //    //File.Delete(tempFile);
                //}
            }
            catch
            {
                // ignored
            }
        }

        [Serializable]
        public struct ShellExecuteInfo
        {
            public int Size;
            public uint Mask;
            public IntPtr hwnd;
            public string Verb;
            public string File;
            public string Parameters;
            public string Directory;
            public uint Show;
            public IntPtr InstApp;
            public IntPtr IDList;
            public string Class;
            public IntPtr hkeyClass;
            public uint HotKey;
            public IntPtr Icon;
            public IntPtr Monitor;
        }

        // Code For OpenWithDialog Box
        [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShellExecuteEx(ref ShellExecuteInfo lpExecInfo);

        private string GetClipTempFile(out string fileEditor, SQLiteDataReader rowReader = null)
        {
            fileEditor = "";
            if (rowReader == null)
                rowReader = RowReader;
            if (rowReader == null)
                return "";
            string type = RowReader["type"].ToString();
            //string TempFile = Path.GetTempFileName();
            string tempFile = clipTempFile(rowReader, "copy");
            if (tempFile == "")
            {
                MessageBox.Show(this, CurrentLangResourceManager.GetString("ClipFileAlreadyOpened"));
                return "";
            }
            if (type == "text" /*|| type == "file"*/)
            {
                File.WriteAllText(tempFile, rowReader["text"].ToString(), Encoding.UTF8);
                fileEditor = Properties.Settings.Default.TextEditor;
            }
            else if (type == "rtf")
            {
                RichTextBox rtb = new RichTextBox();
                rtb.Rtf = RowReader["richText"].ToString();
                rtb.SaveFile(tempFile);
                fileEditor = Properties.Settings.Default.RtfEditor;
            }
            else if (type == "html")
            {
                File.WriteAllText(tempFile, GetHtmlFromHtmlClipText(), Encoding.UTF8);
                fileEditor = Properties.Settings.Default.HtmlEditor;
            }
            else if (type == "img")
            {
                ImageControl.Image.Save(tempFile);
                fileEditor = Properties.Settings.Default.ImageEditor;
            }
            else if (type == "file")
            {
                string[] tokens = Regex.Split(RowReader["text"].ToString(), @"\r?\n|\r");
                tempFile = tokens[0];
                if (!File.Exists(tempFile))
                    tempFile = "";
            }
            return tempFile;
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
                extension = "png";
            else
                extension = "dat";
            string tempFolder = Properties.Settings.Default.ClipTempFileFolder;
            if (!Directory.Exists(tempFolder))
                tempFolder = Path.GetTempPath();
            if (!tempFolder.EndsWith("\\"))
                tempFolder += "\\";
            string tempFile = tempFolder + "Clip " + rowReader["id"] + " " + suffix + "." + extension;
            try
            {
                using (new StreamWriter(tempFile))
                {
                }
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
            bool newEditMode = !EditMode;
            string clipType = RowReader["type"].ToString();
            if (!IsTextType())
                return;
            //selectionStart = richTextBox.SelectionStart;
            //selectionLength = richTextBox.SelectionLength;
            allowRowLoad = false;
            if (!newEditMode)
            {
                UpdateClipBindingSource();
            }
            else
            {
                if (clipType != "text")
                {
                    AddClip(null, null, "", "", "text", RowReader["text"].ToString(), "", "", "", 0, "",
                        (bool) RowReader["used"], (bool) RowReader["favorite"]);
                    GotoLastRow(true);
                }
                //else
                //    UpdateClipBindingSource();
            }
            allowRowLoad = true;
            EditMode = newEditMode;
            AfterRowLoad(true, -1);
            UpdateControlsStates();
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
                    hoverCell.ToolTipText = CurrentLangResourceManager.GetString("VisualWeightTooltip");
            }
            //if (e.ColumnIndex == dataGridView.Columns["TypeImage"].Index)
            //{
            //    DataGridViewCell hoverCell = row.Cells[dataGridView.Columns["ColumnTitle"].Index];
            //    DataRowView dataRowView = row.DataBoundItem as DataRowView;
            //    string tooltipText = "";
            //    if (!(dataRowView["Chars"] is DBNull))
            //        tooltipText += FormattedClipNumericPropery((int)dataRowView["Chars"], MultiLangCharUnit()) + ", ";
            //    if (!(dataRowView["Size"] is DBNull))
            //        tooltipText += FormattedClipNumericPropery((int)dataRowView["Size"], MultiLangByteUnit()) + ", ";
            //    tooltipText += dataRowView["Created"].ToString();
            //    hoverCell.ToolTipText = tooltipText;
            //}
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
                if (!dataGridView.Rows[e.RowIndex].Selected)
                {
                    //dataGridView.Rows[e.RowIndex].Selected = true;
                    dataGridView.CurrentCell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    //dataGridView.Focus();
                }
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

        private void UpdateTextPositionIndicator(int line = 1, int column = 1)
        {
            string newText;
            if (RowReader!= null && RowReader["type"].ToString() == "img")
            {
                //double zoomFactor = Math.Min((double) ImageControl.ClientSize.Width / ImageControl.Image.Width, (double) ImageControl.ClientSize.Height / ImageControl.Image.Height);
                double zoomFactor = ImageControl.ZoomFactor();
                newText = CurrentLangResourceManager.GetString("Zoom") + ": " + zoomFactor.ToString("0.00");
            }
            else
            {
                newText = "" + selectionStart;
                newText += "(" + line + ":" + column + ")";
                if (selectionLength > 0)
                {
                    newText += "+" + selectionLength;
                }
            }
            stripLabelPosition.Text = newText;
            //StripLabelPositionXY.Text = NewText;
        }

        private void ImageControl_MouseWheel(object sender, MouseEventArgs e)
        {
            UpdateTextPositionIndicator();
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
                    _lastSelectedForCompareId = (int) RowReader["id"];
                    //MessageBox.Show("Now execute this command on the second clip to compare", Application.ProductName);
                    return;
                }
                else
                {
                    id1 = (int) RowReader["id"];
                    id2 = _lastSelectedForCompareId;
                    _lastSelectedForCompareId = 0;
                }
            }
            else //if (dataGridView.SelectedRows.Count > 1)
            {
                DataRowView row1 = (DataRowView) dataGridView.SelectedRows[0].DataBoundItem;
                id1 = (int) row1["id"];
                DataRowView row2 = (DataRowView) dataGridView.SelectedRows[1].DataBoundItem;
                id2 = (int) row2["id"];
            }
            CompareClipsByID(id1, id2);
        }

        private void CompareClipsByID(int id1, int id2)
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
            File.WriteAllText(filename1, rowReader1["text"].ToString(), Encoding.UTF8);
            string filename2 = clipTempFile(rowReader2, "comp");
            File.WriteAllText(filename2, rowReader2["text"].ToString(), Encoding.UTF8);
            Process.Start(comparatorName, String.Format("\"{0}\" \"{1}\"", filename1, filename2));
        }

        string comparatorExeFileName()
        {
            // TODO read paths from registry and let use custom application
            string path;
            path = Properties.Settings.Default.TextCompareApplication;
            if (!String.IsNullOrEmpty(path) && File.Exists(path))
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

            path = "C:\\Program Files (x86)\\KDiff3\\kdiff3.exe";
            if (File.Exists(path))
            {
                return path;
            }

            path = "C:\\Program Files\\KDiff3\\KDiff3.exe";
            if (File.Exists(path))
            {
                return path;
            }

            MessageBox.Show(this, CurrentLangResourceManager.GetString("NoSupportedTextCompareApplication"),
                Application.ProductName);
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
                            || e.KeyPressedCode == (int) Keys.Down
                            || e.KeyPressedCode == (int) Keys.Up
                            || e.KeyPressedCode == (int) Keys.Left
                            || e.KeyPressedCode == (int) Keys.Right
                            || e.KeyPressedCode == (int) Keys.PageDown
                            || e.KeyPressedCode == (int) Keys.PageUp
                            || e.KeyPressedCode == (int) Keys.Home
                            || e.KeyPressedCode == (int) Keys.End
                ;
            if (e.KeyPressedCode == (int) Keys.Escape)
                Close();
            else if (e.KeyPressedCode == (int) Keys.Enter)
            {
                ProcessEnterKeyDown(e.CtrlKeyPressed);
            }
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
            mshtml.IHTMLTxtRange range = null;
            try
            {
                range = (IHTMLTxtRange) htmlDoc.selection.createRange();
            }
            catch
            {
            }
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
            SetTextInClipboard(href);
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
            MoveSelectedRows(0);
        }

        private void clearClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataObject dto = new DataObject();
            string text = richTextBox.SelectedText;
            SetTextInClipboardDataObject(dto, text);
            if (Properties.Settings.Default.ShowNativeTextFormatting)
            {
                dto.SetText(richTextBox.SelectedRtf, TextDataFormat.Rtf);
            }
            SetClipboardDataObject(dto);
        }

        private static void SetTextInClipboardDataObject(DataObject dto, string text)
        {
            if (String.IsNullOrEmpty(text))
                return;
            dto.SetText(text, TextDataFormat.Text);
            dto.SetText(text, TextDataFormat.UnicodeText);
        }

        private void openFavoritesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowForPaste(true);
        }

        private void rtfMenuItemOpenLink_Click(object sender, EventArgs e)
        {
            OpenLinkInRichTextBox(richTextBox, TextLinkMatches);
        }

        private void toolStripMenuItemShowAllTypes_Click(object sender, EventArgs e)
        {
            TypeFilter.SelectedValue = "allTypes";
        }

        private void showOnlyTextsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TypeFilter.SelectedValue = "text";
        }

        private void showOnlyFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TypeFilter.SelectedValue = "file";
        }

        private void showOnlyImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TypeFilter.SelectedValue = "img";
        }

        private void moveCopiedClipToTopToolStripButton_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.MoveCopiedClipToTop = !Properties.Settings.Default.MoveCopiedClipToTop;
            UpdateControlsStates();
        }

        private void moveClipToTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.MoveCopiedClipToTop = !Properties.Settings.Default.MoveCopiedClipToTop;
            UpdateControlsStates();
        }

        private void richTextBox_Enter(object sender, EventArgs e)
        {
            if (true
                && RowReader != null
                && RowReader["type"].ToString() == "file"
                && richTextBox.SelectionLength == 0
                && richTextBox.SelectionStart == 0)
            {
                var match = Regex.Match(richTextBox.Text, @"([^\\/:*?""<>|\r\n]+)[$<\r\n]", RegexOptions.Singleline);
                if (match != null)
                {
                    SetRichTextboxSelection(match.Groups[1].Index, match.Groups[1].Length);
                }
            }
        }

        private void ignoreApplicationInCaptureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RowReader == null)
                return;
            StringCollection IgnoreApplicationsClipCapture = Properties.Settings.Default.IgnoreApplicationsClipCapture;
            string fullFilename = RowReader["AppPath"].ToString();
            if (String.IsNullOrEmpty(fullFilename))
                return;
            if (IgnoreApplicationsClipCapture.Contains(fullFilename))
                return;
            IgnoreApplicationsClipCapture.Add(fullFilename);
            UpdateIgnoreModulesInClipCapture();
            string moduleName = Path.GetFileName(fullFilename);
            MessageBox.Show(this,
                String.Format(CurrentLangResourceManager.GetString("ApplicationAddedToIgnoreList"), moduleName),
                AssemblyProduct);
        }

        private void copyFullFilenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (RowReader == null)
                return;
            string fullFilename = RowReader["AppPath"].ToString();
            SetTextInClipboard(fullFilename);
        }

        void SetTextInClipboard(string text, bool allowSelfCapture = true)
        {
            DataObject dto = new DataObject();
            SetTextInClipboardDataObject(dto, text);
            SetClipboardDataObject(dto, allowSelfCapture);
        }

        void SetClipboardDataObject(IDataObject dto, bool allowSelfCapture = true)
        {
            // If not doing this, WM_CLIPBOARDUPDATE event will be raised 2 times (why?) if "copy"=true
            RemoveClipboardFormatListener(this.Handle);
            bool success = false;
            try
            {
                Clipboard.SetDataObject(dto, true, 10, 10);
                    // Very important to set second parameter to true to give immidiate access to buffer to other processes!
                success = true;
            }
            catch
            {
            }
            if (!success)
                try
                {
                    Clipboard.SetDataObject(dto, false, 10, 10);
                }
                catch (Exception ex)
                {
                    string appPath = "";
                    string clipWindow = "";
                    string clipApplication = "";
                    GetClipboardOwnerLockerInfo(true, out clipWindow, out clipApplication, out appPath);
                    Debug.WriteLine(String.Format(CurrentLangResourceManager.GetString("FailedToWriteClipboard"),
                        clipWindow, clipApplication));
                }
            ConnectClipboard();
            if (allowSelfCapture)
                CaptureClipboardData();
        }

        private void toolStripMenuItem16_Click(object sender, EventArgs e)
        {
            FocusClipText();
        }

        private void toolStripMenuItemCompareLastClips_Click(object sender = null, EventArgs e = null)
        {
            if (lastClipWasMultiCaptured)
                notifyIcon.ShowBalloonTip(2000, Application.ProductName, CurrentLangResourceManager.GetString("LastClipWasMultiCaptured"), ToolTipIcon.Info);
            string sql = "SELECT Id FROM Clips ORDER BY Id Desc Limit 2";
            SQLiteCommand commandSelect = new SQLiteCommand(sql, m_dbConnection);
            using (SQLiteDataReader reader = commandSelect.ExecuteReader())
            {
                int id1 = 0, id2 = 0;
                if (reader.Read())
                    id1 = (int)reader["Id"];
                if (reader.Read())
                    id2 = (int)reader["Id"];
                if (id2 > 0 && id1 > 0)
                    CompareClipsByID(id1, id2);
            }
        }

        private void deleteAllNonFavoriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this,
                CurrentLangResourceManager.GetString("СonfirmDeleteAllNonFavorite"),
                CurrentLangResourceManager.GetString("Confirmation"), MessageBoxButtons.OKCancel);
            if (result != DialogResult.OK)
                return;
            allowRowLoad = false;
            string sql = "Delete from Clips where NOT Favorite OR Favorite IS NULL";
            SQLiteCommand command = new SQLiteCommand("", m_dbConnection);
            command.CommandText = sql;
            command.ExecuteNonQuery();
            UpdateClipBindingSource();
        }

        private void openWithToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenClipFile(false);
        }

        private static string GetMimeType(String filename)
        {
            var extension = System.IO.Path.GetExtension(filename).ToLower();
            var regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(extension);

            string result =
                ((regKey != null) && (regKey.GetValue("Content Type") != null))
                    ? regKey.GetValue("Content Type").ToString()
                    : "image/unknown";
            return result;
        }

        private void uploadImageToWebToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string clipType = (string) RowReader["type"];
            if (clipType != "img")
                return;
            string junkVar;
            string tempFile = GetClipTempFile(out junkVar);
            if (String.IsNullOrEmpty(tempFile))
                return;
            // base on http://cropperplugins.codeplex.com/SourceControl/latest#Cropper.Plugins/ImageShack/Plugin.cs
            string _baseUri = "http://www.imageshack.us/";
            string _developerKey = "T39OZMFC7b60153bbc4341b959be614bc37f3278";
            try
            {
                string relativeUrl = "upload_api.php";
                var http = new HttpClient();
                http.BaseAddress = new Uri(_baseUri);
                var form = new MultipartFormDataContent();
                string mimetype = GetMimeType(tempFile);
                HttpContent fileContent = new ByteArrayContent(File.ReadAllBytes(tempFile));
                form.Add(fileContent, "fileupload", tempFile);
                HttpContent keyContent = new StringContent(_developerKey);
                form.Add(keyContent, "key");
                HttpContent rembarContent = new StringContent("1");
                form.Add(rembarContent, "rembar");
                var response = http.PostAsync(relativeUrl, form);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response.Result.Content.ReadAsStringAsync().Result);
                XmlNamespaceManager namespaces = new XmlNamespaceManager(doc.NameTable);
                namespaces.AddNamespace("ns", doc.DocumentElement.NamespaceURI);
                XmlNode node = doc.DocumentElement.SelectSingleNode("/ns:imginfo/ns:links/ns:image_link", namespaces);
                string ImageUrl = node.InnerText;
                SaveClipUrl(ImageUrl);
                AfterRowLoad();
                SetTextInClipboard(ImageUrl);
            }
            catch (Exception ex)
            {
                WriteExcetionToLog(ex);
                MessageBox.Show(this, ex.Message, "Error uploading image", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void WriteExcetionToLog(Exception ex)
        {
            Debug.WriteLine("--- " + DateTime.Now);
            Debug.WriteLine(ex.Message);
            Debug.WriteLine(ex.StackTrace);
        }

        private void toolStripWindowTitleCopyAll_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBoxWindow.Text))
                SetTextInClipboard(textBoxWindow.Text);
        }

        private void toolStripUrlCopyAll_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(urlTextBox.Text))
                SetTextInClipboard(urlTextBox.Text);
        }

        private void toolStripApplicationCopyAll_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBoxApplication.Text))
                SetTextInClipboard(textBoxApplication.Text);
        }

        private void richTextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void supportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Properties.Resources.Support);
        }

        private void returnToPrevousSelectedClipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int newSelectedID = 0;
            if (selectedClipsBeforeFilterApply.Count > 0)
            {
                newSelectedID = selectedClipsBeforeFilterApply[selectedClipsBeforeFilterApply.Count - 1];
                selectedClipsBeforeFilterApply.RemoveAt(selectedClipsBeforeFilterApply.Count - 1);
            }
            ClearFilter(newSelectedID);
            dataGridView.Focus();
        }

        private void toolStripMenuItemClearFilters_Click(object sender, EventArgs e)
        {
            ClearFilter();
            dataGridView.Focus();
        }

        private void mergeTextOfClipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int count = 0;
            string agregateTextToPaste = JoinOrPasteTextOfClips(PasteMethod.Null, out count);
            AddClip(null, null, "", "", "text", agregateTextToPaste);
            GotoLastRow(true);
        }

        private void comboBoxFilter_MouseHover(object sender, EventArgs e)
        {
            //// Antibug. Tooltip not shown
            //ComponentResourceManager resources = new ComponentResourceManager(typeof(Main));
            ////toolTipDynamic.Show(resources.GetString("comboBoxFilter.ToolTip"), comboBoxFilter);
            //toolTipDynamic.Show(resources.GetString("comboBoxFilter.ToolTip"), TypeFilter);
            Debug.WriteLine("tooltip");
        }

        private void comboBoxFilter_MouseEnter(object sender, EventArgs e)
        {

        }

        private void dataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (false
                || e.ColumnIndex == dataGridView.Columns["AppImage"].Index
                || e.ColumnIndex == dataGridView.Columns["TypeImage"].Index
                || e.ColumnIndex == dataGridView.Columns["ColumnTitle"].Index
                )
            {
                lastMousePoint = new Point(MousePosition.X, MousePosition.Y);
                tooltipTimer.Start();
            }
        }

        private void dataGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            Point pt = dataGridView.PointToClient(Cursor.Position);
            int ColumnIndex = dataGridView.HitTest(pt.X, pt.Y).ColumnIndex;
            if (true
                && ColumnIndex != dataGridView.Columns["AppImage"].Index
                && ColumnIndex != dataGridView.Columns["TypeImage"].Index
                && ColumnIndex != dataGridView.Columns["ColumnTitle"].Index
            )
            {
                tooltipTimer.Stop();
                toolTipDynamic.Hide(this);
            }
        }

        private void tooltipTimer_Tick(object sender, EventArgs e)
        {
            tooltipTimer.Stop();
            Point pt = dataGridView.PointToClient(Cursor.Position);
            int RowIndex = dataGridView.HitTest(pt.X, pt.Y).RowIndex;
            if (RowIndex == -1)
                return;
            DataGridViewRow row = dataGridView.Rows[RowIndex];
            DataRowView dataRowView = row.DataBoundItem as DataRowView;
            string tooltipText = "";
            if (!(dataRowView["Created"] is DBNull))
                tooltipText += String.Format("{0:" + dataGridView.Columns["ColumnCreated"].DefaultCellStyle.Format + "}", (DateTime)dataRowView["Created"]);
            if (!(dataRowView["Chars"] is DBNull))
                tooltipText += ", " + FormattedClipNumericPropery((int)dataRowView["Chars"], MultiLangCharUnit());
            if (!(dataRowView["Size"] is DBNull))
                tooltipText += ", " + FormattedClipNumericPropery((int)dataRowView["Size"], MultiLangByteUnit());
            //Rectangle cellRect = dataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
            toolTipDynamic.Show(tooltipText, this,
                          MousePosition.X - this.Left,
                          //dataGridView.Location.Y + cellRect.Y + cellRect.Size.Height * 4,
                          MousePosition.Y - this.Top + 25,
                          3000);
        }

        private void toolStripButtonAutoSelectMatch_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoSelectMatch = !Properties.Settings.Default.AutoSelectMatch;
            UpdateControlsStates();
        }

        private void caseSensetiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.SearchCaseSensitive = !Properties.Settings.Default.SearchCaseSensitive;
            UpdateControlsStates();
            TextFilterApply();
        }

        private void everyWordIndependentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.SearchWordsIndependently = !Properties.Settings.Default.SearchWordsIndependently;
            UpdateControlsStates();
            TextFilterApply();
        }

        private void meandsAnySequenceOfCharsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.SearchWildcards = !Properties.Settings.Default.SearchWildcards;
            UpdateControlsStates();
            TextFilterApply();
        }

        private void dataGridView_MouseMove(object sender, MouseEventArgs e)
        {
            if (LastMousePoint != e.Location)
            {
                LastMousePoint = e.Location;
                if (e.Button == MouseButtons.Left)
                {
                    DataGridView.HitTestInfo info = dataGridView.HitTest(e.X, e.Y);
                    if (info.RowIndex >= 0)
                    {
                        if (info.RowIndex >= 0 && info.ColumnIndex >= 0)
                        {
                            DataObject dto = new DataObject();
                            int count = 0;
                            string agregateTextToPaste = JoinOrPasteTextOfClips(PasteMethod.Null, out count);
                            SetTextInClipboardDataObject(dto, agregateTextToPaste);
                            SetClipFilesInDataObject(dto);
                            dataGridView.DoDragDrop(dto, DragDropEffects.Copy);
                        }
                    }
                }
            }
        }

        private void SetClipFilesInDataObject(DataObject dto, int maxRowsDrag = 100)
        {
            StringCollection fileNameCollection = new StringCollection();
            foreach (DataGridViewRow selectedRow in dataGridView.SelectedRows)
            {
                DataRowView dataRowView = (DataRowView) selectedRow.DataBoundItem;
                SQLiteDataReader RowReader = getRowReader((int) dataRowView["id"]);
                string clipText;
                DataObject clipDto = ClipDataObject(RowReader, false, out clipText);
                if (RowReader["type"].ToString() != "file")
                {
                    string junkVar;
                    string filename = GetClipTempFile(out junkVar, RowReader);
                    fileNameCollection.Add(filename);
                }
                else
                {
                    foreach (string filename in clipDto.GetFileDropList())
                    {
                        fileNameCollection.Add(filename);
                    }
                }
                maxRowsDrag--;
                if (maxRowsDrag == 0)
                    break;
            }
            dto.SetFileDropList(fileNameCollection);
        }

        private void contextMenuUrlOpenLink_Click(object sender, EventArgs e)
        {
            Process.Start(urlTextBox.Text);
        }

        private void pasteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendPasteOfSelectedTextOrSelectedClips(PasteMethod.File);
        }

        private void filterByDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            monthCalendar1.Show();
            monthCalendar1.Focus();
        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            monthCalendar1.Hide();
            periodFilterOn = true;
            UpdateClipBindingSource();
        }

        private void monthCalendar1_Leave(object sender, EventArgs e)
        {
            monthCalendar1.Hide();
        }

        private void ImageControl_Resize(object sender, EventArgs e)
        {
            UpdateTextPositionIndicator();
        }

        private void ImageControl_ZoomChanged(object sender, EventArgs e)
        {
            UpdateTextPositionIndicator();
        }

        private void fitFromInsideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageControl.ZoomFitInside();
        }

        private void originalSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImageControl.ZoomOriginalSize();
        }

        private void dataGridView_MouseClick(object sender, MouseEventArgs e)
        {
        }

        private void dataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
        }

        private void Main_Resize(object sender, EventArgs e)
        {
        }

        private void sortByDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sortField = "Clips.Id";
            UpdateClipBindingSource();
        }

        private void sortByVisualSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //sortField = "Chars"; // not working
            sortField = "Clips.Chars";
            UpdateClipBindingSource();
        }

        private void sortByCreationDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sortField = "Clips.Created";
            UpdateClipBindingSource();
        }

        private void toolStripMenuItem20_Click(object sender, EventArgs e)
        {
            sortField = "Clips.Size";
            UpdateClipBindingSource();
        }

        private void toolStripButtonSecondaryColumns_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowSecondaryColumns = !Properties.Settings.Default.ShowSecondaryColumns;
            UpdateControlsStates();
            UpdateColumnsSet();
        }

        private void toolStripMenuItemSecondaryColumns_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowSecondaryColumns = !Properties.Settings.Default.ShowSecondaryColumns;
            UpdateControlsStates();
            UpdateColumnsSet();
        }

        private void ignoreBigTextClipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.SearchIgnoreBigTexts = !Properties.Settings.Default.SearchIgnoreBigTexts;
            UpdateControlsStates();
            TextFilterApply();
        }

        private void addSelectedTextInFilterToolStripMenu_Click(object sender, EventArgs e)
        {
            AllowFilterProcessing = false;
            if (!String.IsNullOrWhiteSpace(comboBoxFilter.Text))
                comboBoxFilter.Text += " ";
            comboBoxFilter.Text += GetSelectedText();
            AllowFilterProcessing = true;
            TextFilterApply();
        }

        private void setSelectedTextInFilterToolStripMenu_Click(object sender, EventArgs e)
        {
            AllowFilterProcessing = false;
            comboBoxFilter.Text = richTextBox.SelectedText;
            AllowFilterProcessing = true;
            TextFilterApply();
        }
    }
}


public sealed class KeyboardHook : IDisposable
{
    // http://stackoverflow.com/questions/2450373/set-global-hotkeys-using-c-sharp

    // Registers a hot key with Windows.
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    // Unregisters the hot key with Windows.
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
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
            //int ErrorCode = Marshal.GetLastWin32Error(); // 0 always
            //string errorText = resourceManager.GetString("CouldNotRegisterHotkey") + " \"" + hotkeyTitle + "\": " + ErrorCode;
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
