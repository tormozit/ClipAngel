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
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Resources;
using System.Net;
using System.Net.Http;
using AngleSharp.Parser.Html;
using AngleSharp.Dom;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using WindowsInput.Native;
using mshtml;
//using Word = Microsoft.Office.Interop.Word;
using static IconTools;
using Timer = System.Windows.Forms.Timer;
using UIAutomationClient;
using TreeScope = UIAutomationClient.TreeScope;
using System.Security.Principal;
using System.Xml.Serialization;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Input;
using WindowsInput;
using ClipAngel.Properties;
using Microsoft.Win32;
using Newtonsoft.Json;
using Cursor = System.Windows.Forms.Cursor;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using MouseEventHandler = System.Windows.Forms.MouseEventHandler;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using Google.Apis.Auth.OAuth2;
using System.Net.Http.Headers;

namespace ClipAngel
{
    public enum PasteMethod
    {
        Standard,
        Text,
        Line,
        SendCharsFast,
        SendCharsSlow,
        File,
        Null
    };
    public struct AESKey
    {
        public string key;
        public string IV;
    }
    public partial class Main : Form
    {
        public const string IsMainPropName = "IsMain";
        public ResourceManager CurrentLangResourceManager;
        public string Locale = "";
        public string DbFileName;

        public bool PortableMode = false;

        //public int ClipsNumber = 0;
        public string UserSettingsPath;

        SQLiteConnection m_dbConnection;
        SQLiteDataAdapter backgroundDataAdapter;
        public string ConnectionString;

        SQLiteDataAdapter globalDataAdapter;

        private struct ReloadTask
        {
            Task task;
            private SQLiteConnection DBConnection;
        };
        ReloadTask[] tasks;
        public struct removeClipsFilter
        {
            public int PID;
            public DateTime TimeStart;
            public DateTime TimeEnd;
        }
        public struct LastClip
        {
            public int ID;
            public DateTime Created;
            public int ProcessID;
        }
        static List<LastClip> lastClips = new List<LastClip>();

        //bool CaptureClipboard = true;
        bool allowRowLoad = true;

        //bool AutoGotoLastRow = true;
        bool AllowHotkeyProcess = true;
 
        private Task<DataTable> lastReloadListTask;
        private DateTime lastReloadListTime;
        bool EditMode = false;
        SQLiteDataReader LoadedClipRowReader;
        int LastId = 0;
        MatchCollection TextLinkMatches;
        MatchCollection UrlLinkMatches;
        MatchCollection FilterMatches;
        string DataFormat_ClipboardViewerIgnore = "Clipboard Viewer Ignore";
        string DataFormat_XMLSpreadSheet = "XML SpreadSheet";
        string DataFormat_RemoveTempClipsFromHistory = "RemoveTempClipsFromHistory"; // Turboconf

        string ActualVersion;

        //DateTime lastAutorunUpdateCheck;
        bool TextWasCut;

        public KeyboardHook keyboardHook;
        WinEventDelegate dele = null;
        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;
        private static IntPtr lastActiveParentWindow;
        private static IntPtr lastChildWindow;

        private static RECT lastChildWindowRect;

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
        const int ChannelDataLifeTime = 60;
        string searchString = ""; // TODO optimize speed
        bool periodFilterOn = false;
        const int MaxTextViewSize = 10000;
        const int tabLength = 4;
        const int maxClipsToSelect = 300; // SQLite default limit for quantity of variables
        const int ClipTitleLength = 70;
        static Dictionary<string, Bitmap> originalIconCache = new Dictionary<string, Bitmap>();
        static Dictionary<string, Bitmap> brightIconCache = new Dictionary<string, Bitmap>();
        private bool allowTextPositionChangeUpdate = false;
        private int _lastSelectedForCompareId;
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
        private Color[] _wordColors = new Color[] {Color.Red, Color.DeepPink, Color.DarkOrange};
        private DateTime lastCaptureMoment = DateTime.Now;
        private DateTime lastPasteMoment = DateTime.Now;
        private Dictionary<int, DateTime> lastPastedClips = new Dictionary<int, DateTime>();
        private bool lastClipWasMultiCaptured = false;
        private Point LastMousePoint;
        private Timer captureTimer = new Timer();
        private Timer channelTimer = new Timer();
        private Timer fix1CTimer = new Timer();
        private Timer tempCaptureTimer = new Timer();
        private DateTime TimeFromWindowOpen;
        private Thread updateDBThread;
        private bool stopUpdateDBThread = false;
        private int selectedRangeStart = -1;
        private bool allowProcessDataGridSelectionChanged = true;
        private Point PreviousCursor;
        private bool titleToolTipShown = false;
        private ToolTip titleToolTip = new ToolTip();
        private Timer titleToolTipBeforeTimer = new Timer();
        string sortField = "Id";
        string link1Cprefix = "1Clink:";
        List<int> searchMatchedIDs = new List<int>();
        static string timePattern = "\\b[012]?\\d:[0-5]?\\d(?::[0-5]?\\d)?\\b";
        static string datePattern = "\\b(?:19|20)?[0-9]{2}[\\-/.][0-9]{2}[\\-/.](?:19|20)?[0-9]{2}\\b";
        private bool areDeletedClips = false;
        readonly RichTextBox _richTextBox = new RichTextBox();
        readonly RichTextBox richTextInternal = new RichTextBox();
        static string LetDig = "a-zа-яё0-9_";
        static private Dictionary<string, string> TextPatterns = new Dictionary<string, string>
        {
            {"time", $@"(({datePattern}\s{timePattern})|(?:({timePattern}\s)?{datePattern})|(?:{timePattern}))"},
            {"email", @"(\b[A-Z0-9._%+-]+@[-A-Z0-9.]+\.[A-Z]{2,6}\b)"},
            {"number", @"((?:(?:\s|^)[-])?\b[0-9]+\.?[0-9]+)\b"},
            {"phone", @"(?:[\s\(]|^)(\+?\b\d?(\d[ \-\()]{0,2}){7,19}\b)"},
            {"url", $@"(\b(?:https?|ftp|file|e1c)://[{LetDig}\-+&@#\\/%?=~|!:,.;()]+[{LetDig}])"},
            {"url_image", @"(https?:\/\/.*\.(?:png|jpg|gif|jpeg|svg))"},
            {"url_video", @"(?:https?://)?(?:www\.)?youtu(?:\.be|be\.com)/(?:(?:.*)v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)"},
            {"filename", $@"((?:\b[a-z]:|\\\\[{LetDig} %.\-]+\\[{LetDig} $%.\-]+)\\(?:[^\\/:*?""<>|\r\n]+\\)*[^\\/:*?""<>|\r\n]*)"},
            {"1CLine", $@"(\{{([{LetDig}]+ )?(([{LetDig}]+::.+?::)?(?:[{LetDig}\.])*(?:Форма|Form|Модуль[{LetDig}]*|[{LetDig}]*Module|))\((\d+)(?:,(\d+))?(?:\:?([{LetDig}<>]*)(?:,(-?\d+))?)?\)\}})"}
        };

        static string LinkPattern = TextPatterns["url"];
        static string fileOrFolderPattern = TextPatterns["filename"];
        static string imagePattern = TextPatterns["url_image"];
        static string videoPattern = TextPatterns["url_video"];

        static private Dictionary<string, string> typeMap1C = new Dictionary<string, string>
        {
            {"HTTPСервис", "HTTPService"},
            {"WebСервис", "WebService"},
            {"ВнешнийИсточникДанных", "ExternalDataSource"},
            {"Документ", "Document"},
            {"Задача", "Task"},
            {"Команда", "Command"},
            {"Константа", "Constant"},
            {"МодульВнешнегоСоединения", "ExternalConnectionModule"},
            {"МодульКоманды", "CommandModule"},
            {"МодульОбычногоПриложения", "OrdinaryApplicationModule"},
            {"МодульОбъекта", "ObjectModule"},
            {"МодульНабораЗаписей", "RecordsetModule"},
            {"МодульМенеджера", "ManagerModule"},
            {"МодульМенеджераЗначения", "ValueManagerModule"},
            {"МодульСеанса", "SessionModule"},
            {"МодульУправляемогоПриложения", "ManagedApplicationModule"},
            {"Модуль", "Module"},
            {"ОбщаяФорма", "CommonForm"},
            {"ОбщийМодуль", "CommonModule"},
            {"ОбщаяКоманда", "CommonCommand"},
            {"Обработка", "DataProcessor"},
            {"Перечисление", "Enum"},
            {"ПланВидовРасчета", "ChartOfCalculationTypes"},
            {"ПланВидовХарактеристик", "ChartOfCharacteristicTypes"},
            {"ПланОбмена", "ExchangePlan"},
            {"Отчет", "Report"},
            {"РегистрБухгалтерии", "AccountingRegister"},
            {"РегистрНакопления", "AccumulationRegister"},
            {"РегистрРасчета", "CalculationRegister"},
            {"РегистрСведений", "InformationRegister"},
            {"Форма", "Form"},
            {"Справочник", "Catalog"},
      };

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
            if (true
                && (ClipAngel.Properties.Settings.Default.WindowPositionX != 0 || ClipAngel.Properties.Settings.Default.WindowPositionY != 0)
                && (ClipAngel.Properties.Settings.Default.WindowPositionX != -32000 && ClipAngel.Properties.Settings.Default.WindowPositionY != -32000) // old version could save minimized state coords
                && ClipAngel.Properties.Settings.Default.WindowPositionY != maxWindowCoordForHiddenState
                )
            {
                this.Left = ClipAngel.Properties.Settings.Default.WindowPositionX;
                this.Top = ClipAngel.Properties.Settings.Default.WindowPositionY;
            }

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
            titleToolTipBeforeTimer.Tick += delegate(object sender, EventArgs e)
            {
                titleToolTipBeforeTimer.Stop();
                string text = Application.ProductName + String.Format(" <{0}> >> <{1}> [<{2}>]", Properties.Resources.Version, Properties.Resources.TargetWindow,
                                  Properties.Resources.TargetApp);
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
            fix1CTimer.Interval = 2000;
            fix1CTimer.Tick += delegate { fix1CFormat(); };
            fix1CTimer.Start();
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
            //ClipAngel.Properties.Settings.Default.FastWindowOpen = false; // for debug

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
                _comboItemsTypes.Add(new ListItemNameText {Name = "text_" + pair.Key});
            }
            TypeFilter.DataSource = _comboItemsTypes;
            TypeFilter.DisplayMember = "Text";
            TypeFilter.ValueMember = "Name";
            //MarkFilter.SelectedValue = "allTypes";
            MarkFilter.SelectedIndex = 0;

            BindingList<ListItemNameText> _comboItemsMarks = new BindingList<ListItemNameText>();
            _comboItemsMarks.Add(new ListItemNameText {Name = "allMarks"});
            _comboItemsMarks.Add(new ListItemNameText {Name = "favorite"});
            _comboItemsMarks.Add(new ListItemNameText {Name = "used"});
            MarkFilter.DataSource = _comboItemsMarks;
            MarkFilter.DisplayMember = "Text";
            MarkFilter.ValueMember = "Name";
            //MarkFilter.SelectedValue = "allMarks";
            TypeFilter.SelectedIndex = 0;

            (dataGridView.Columns["AppImage"] as DataGridViewImageColumn).DefaultCellStyle.NullValue = null;
            richTextBox.AutoWordSelection = false;
            urlTextBox.AutoWordSelection = false;

            // Initialize StringCollection settings to prevent error saving settings 
            if (ClipAngel.Properties.Settings.Default.LastFilterValues == null)
            {
                ClipAngel.Properties.Settings.Default.LastFilterValues = new StringCollection();
            }
            if (ClipAngel.Properties.Settings.Default.IgnoreApplicationsClipCapture == null)
            {
                ClipAngel.Properties.Settings.Default.IgnoreApplicationsClipCapture = new StringCollection();
            }

            FillFilterItems();

            if (!Directory.Exists(UserSettingsPath))
            {
                Directory.CreateDirectory(UserSettingsPath);
            }
            OpenDatabase(true);
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
            if (ClipAngel.Properties.Settings.Default.MainWindowSize.Width > 0)
                this.Size = ClipAngel.Properties.Settings.Default.MainWindowSize;
            timerDaily.Interval = 1;
            timerDaily.Start();
            timerReconnect.Interval = (1000 * 5); // 5 seconds
            timerReconnect.Start();
            CheckClearChannelData();
            this.ActiveControl = dataGridView;
            ResetIsMainProperty();

            LoadSettings();
            if (ClipAngel.Properties.Settings.Default.dataGridViewWidth != 0)
            {
                splitContainer1.SplitterDistance = ClipAngel.Properties.Settings.Default.dataGridViewWidth;
            }
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

        private void OpenDatabase(bool updateEncryption = false)
        {
            if (!String.IsNullOrWhiteSpace(ClipAngel.Properties.Settings.Default.DatabaseFile))
                DbFileName = ClipAngel.Properties.Settings.Default.DatabaseFile;
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
            if (updateEncryption)
            {
                string encryptException = "";
                if (ClipAngel.Properties.Settings.Default.EncryptDatabaseForCurrentUser)
                {
                    try
                    {
                        //File.Encrypt("dhjjsfjsgfjsfjgsfj"); // for test
                        File.Encrypt(DbFileName);
                    }
                    catch (Exception exception)
                    {
                        // https://sourceforge.net/p/clip-angel/tickets/60/
                        encryptException = exception.Message;
                        ClipAngel.Properties.Settings.Default.EncryptDatabaseForCurrentUser = false;
                    }
                }
                else
                {
                    try
                    {
                        //File.Encrypt("dhjjsfjsgfjsfjgsfj"); // for test
                        File.Decrypt(DbFileName);
                    }
                    catch (Exception exception)
                    {
                        // https://sourceforge.net/p/clip-angel/tickets/60/
                        encryptException = exception.Message;
                        ClipAngel.Properties.Settings.Default.EncryptDatabaseForCurrentUser = true;
                    }
                }
                if (!String.IsNullOrEmpty(encryptException))
                {
                    MessageBox.Show(this, Properties.Resources.FailedChangeDatabaseFieEncryption + ": \n" + encryptException, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
                ThreadStart work = delegate { UpdateNewDBFieldsBackground(commandUpdate, fieldsNeedUpdateText, fieldsNeedSelectText, patternNamesNeedUpdate); };
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
            globalDataAdapter = new SQLiteDataAdapter("", m_dbConnection);
            //dataGridView.DataSource = clipBindingSource;
            ReloadList();
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
            UpdateLastActiveParentWindow(hwnd);
        }

        private void UpdateLastActiveParentWindow(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                hwnd = GetForegroundWindow();
            bool targetIsCurrentProcess = DoActiveWindowBelongsToCurrentProcess(hwnd);
            if (!targetIsCurrentProcess && lastActiveParentWindow != hwnd)
            {
                //preLastActiveParentWindow = lastActiveParentWindow; // In case with Explorer tray click it will not help
                lastActiveParentWindow = hwnd;
                lastChildWindow = IntPtr.Zero;
                lastChildWindowRect = new RECT();
                lastCaretPoint = new Point();
                //lastWindowSelectedText = null;
                UpdateWindowTitle();
            }
        }

        private static bool DoActiveWindowBelongsToCurrentProcess(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
                hwnd = GetForegroundWindow();
            int targetProcessId;
            uint remoteThreadId = GetWindowThreadProcessId(hwnd, out targetProcessId);
            bool targetIsCurrentProcess = targetProcessId == Process.GetCurrentProcess().Id;
            return targetIsCurrentProcess;
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
            string targetTitle = "<" + Properties.Resources.NoActiveWindow + ">";
            if (lastActiveParentWindow != null)
            {
                targetTitle = GetWindowTitle(lastActiveParentWindow);
                int pid;
                GetWindowThreadProcessId(lastActiveParentWindow, out pid);
                try
                {
                    Process proc = Process.GetProcessById(pid);
                    if (proc != null)
                        targetTitle += " [" + proc.ProcessName + "]";
                }
                catch (Exception) { }
            }
            Debug.WriteLine("Active window " + lastActiveParentWindow + " " + targetTitle);
            string newTitle = Application.ProductName + " " + Properties.Resources.VersionValue;
            if (!ClipAngel.Properties.Settings.Default.MonitoringClipboard)
            {
                newTitle += " [" + Properties.Resources.NoCapture + "]";
            }
            this.Text = newTitle + " >> " + targetTitle;
            notifyIcon.Text = newTitle;
        }

        public static T ParseEnum<T>(string value)
        {
            return (T) Enum.Parse(typeof(T), value, true);
        }

        public void RegisterHotKeys()
        {
            EnumModifierKeys Modifiers;
            Keys Key;
            if (ReadHotkeyFromText(ClipAngel.Properties.Settings.Default.GlobalHotkeyOpenLast, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(ClipAngel.Properties.Settings.Default.GlobalHotkeyOpenCurrent, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(ClipAngel.Properties.Settings.Default.GlobalHotkeyOpenFavorites, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(ClipAngel.Properties.Settings.Default.GlobalHotkeyIncrementalPaste, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(ClipAngel.Properties.Settings.Default.GlobalHotkeyDecrementalPaste, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(ClipAngel.Properties.Settings.Default.GlobalHotkeyCompareLastClips, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(ClipAngel.Properties.Settings.Default.GlobalHotkeyPasteText, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(ClipAngel.Properties.Settings.Default.GlobalHotkeySimulateInput, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(ClipAngel.Properties.Settings.Default.GlobalHotkeySwitchMonitoring, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(ClipAngel.Properties.Settings.Default.GlobalHotkeyForcedCapture, out Modifiers, out Key))
                keyboardHook.RegisterHotKey(Modifiers, Key);
            //if (ClipAngel.Properties.Settings.Default.CopyTextInAnyWindowOnCTRLF3 && ReadHotkeyFromText("Control + F3", out Modifiers, out Key))
            //    keyboardHook.RegisterHotKey(Modifiers, Key);
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
            if (hotkeyTitle == ClipAngel.Properties.Settings.Default.GlobalHotkeyOpenLast)
            {
                if (IsVisible() && this.ContainsFocus && MarkFilter.SelectedValue.ToString() != "favorite") // Sometimes it can cotain focus but be not visible!
                    this.Close();
                else
                {
                    ShowForPaste(false, true);
                    dataGridView.Focus();
                }
            }
            else if (hotkeyTitle == ClipAngel.Properties.Settings.Default.GlobalHotkeyOpenCurrent)
            {
                if (IsVisible() && this.ContainsFocus)
                    this.Close();
                else
                {
                    ShowForPaste();
                    //dataGridView.Focus();
                }
            }
            else if (hotkeyTitle == ClipAngel.Properties.Settings.Default.GlobalHotkeyOpenFavorites)
            {
                if (IsVisible() && this.ContainsFocus && MarkFilter.SelectedValue.ToString() == "favorite")
                    this.Close();
                else
                {
                    ShowForPaste(true, true);
                    dataGridView.Focus();
                }
            }
            else if (hotkeyTitle == ClipAngel.Properties.Settings.Default.GlobalHotkeyIncrementalPaste)
            {
                PasteAndSelectNext(1);
            }
            else if (hotkeyTitle == ClipAngel.Properties.Settings.Default.GlobalHotkeyDecrementalPaste)
            {
                PasteAndSelectNext(-1);
            }
            else if (hotkeyTitle == ClipAngel.Properties.Settings.Default.GlobalHotkeyCompareLastClips)
            {
                if (filterOn)
                    ClearFilter(-1, false, true);
                toolStripMenuItemCompareLastClips_Click();
            }
            else if (hotkeyTitle == ClipAngel.Properties.Settings.Default.GlobalHotkeyPasteText)
            {
                if (filterOn)
                    ClearFilter(-1, false, true);
                SendPasteClipExpress(dataGridView.Rows[0], PasteMethod.Text);
            }
            else if (hotkeyTitle == ClipAngel.Properties.Settings.Default.GlobalHotkeySimulateInput)
            {
                if (filterOn)
                    ClearFilter(-1, false, true);
                SendPasteClipExpress(dataGridView.Rows[0], PasteMethod.SendCharsFast);
            }
            else if (hotkeyTitle == ClipAngel.Properties.Settings.Default.GlobalHotkeySwitchMonitoring)
            {
                SwitchMonitoringClipboard(true);
            }
            else if (hotkeyTitle == ClipAngel.Properties.Settings.Default.GlobalHotkeyForcedCapture)
            {
                if (!ClipAngel.Properties.Settings.Default.MonitoringClipboard)
                {
                    ClipAngel.Properties.Settings.Default.MonitoringClipboard = true;
                    ConnectClipboard();
                    tempCaptureTimer.Interval = 2000;
                    tempCaptureTimer.Start();
                }
                Paster.SendCopy(false);
            }
            //else if (hotkeyTitle == "Control + F3")
            //{
            //    keyboardHook.UnregisterHotKeys();
            //    BackupClipboard();
            //    //Clipboard.Clear();
            //    Paster.SendCopy(false);
            //    SendKeys.SendWait("^{F3}");
            //    RegisterHotKeys();
            //    RestoreClipboard();
            //}
            else
            {
                //int a = 0;
            }

        }

        private void PasteAndSelectNext(int direction)
        {
            AllowHotkeyProcess = false;
            try
            {
                SendPasteClipExpress(null, PasteMethod.Standard, false, true);
                // https://www.hostedredmine.com/issues/925182
                //if ((e.Modifier & EnumModifierKeys.Alt) != 0)
                //    keybd_event((byte) VirtualKeyCode.MENU, 0x38, 0, 0); // LEFT
                //if ((e.Modifier & EnumModifierKeys.Control) != 0)
                //    keybd_event((byte) VirtualKeyCode.CONTROL, 0x1D, 0, 0);
                //if ((e.Modifier & EnumModifierKeys.Shift) != 0)
                //    keybd_event((byte) VirtualKeyCode.SHIFT, 0x2A, 0, 0);
                DataRow oldCurrentDataRow = ((DataRowView) clipBindingSource.Current).Row;
                if (direction > 0)
                    clipBindingSource.MovePrevious();
                else
                    clipBindingSource.MoveNext();
                DataRow CurrentDataRow = ((DataRowView) clipBindingSource.Current).Row;
                notifyIcon.Visible = true;
                string messageText;
                if (oldCurrentDataRow == CurrentDataRow)
                    messageText = Properties.Resources.PastedLastClip;
                else
                    messageText = CurrentDataRow["Title"].ToString();
                notifyIcon.ShowBalloonTip(3000, Properties.Resources.NextClip, messageText, ToolTipIcon.Info);
            }
            catch (Exception)
            {
                int dummy = 1;
            }
            finally
            {
                AllowHotkeyProcess = true;
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
                    Debug.WriteLine(String.Format(Properties.Resources.FailedToReadFormatFromClipboard, format));
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
            if (ClipAngel.Properties.Settings.Default.FastWindowOpen)
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
                        captureTimer.Interval = 100;
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
            if (this.Visible && ClipAngel.Properties.Settings.Default.ShowTipsOnStart)
            {
                Tips form = new Tips();
                form.ShowDialog(this);
                Settings.Default.Save();
            }
            dataGridView.Focus();
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
            if (!ClipAngel.Properties.Settings.Default.MonitoringClipboard)
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
            //DataRowView CurrentRowView;
            IHTMLDocument2 htmlDoc;
            string clipType;
            string textPattern = RegexpPattern();
            bool autoSelectMatch = (textPattern.Length > 0 && ClipAngel.Properties.Settings.Default.AutoSelectMatch);
            FullTextLoad = FullTextLoad || EditMode;
            richTextBox.ReadOnly = !EditMode;
            FilterMatches = null;
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
            if (ClipAngel.Properties.Settings.Default.MonospacedFont)
                richTextBox.Font = new Font(FontFamily.GenericMonospace, ClipAngel.Properties.Settings.Default.Font.Size);
            else
                richTextBox.Font = ClipAngel.Properties.Settings.Default.Font;
            richTextInternal.Font = richTextBox.Font;
            int fontsize = (int)richTextBox.Font.Size; // Size should be without digits after comma
            richTextBox.SelectionTabs = new int[] { fontsize * 4, fontsize * 8, fontsize * 12, fontsize * 16 }; // Set tab size ~ 4
            richTextInternal.SelectionTabs = richTextBox.SelectionTabs;

            richTextInternal.Clear();
            urlTextBox.Text = "";
            textBoxApplication.Text = "";
            textBoxWindow.Text = "";
            StripLabelCreated.Text = "";
            StripLabelSize.Text = "";
            StripLabelVisualSize.Text = "";
            StripLabelType.Text = "";
            stripLabelPosition.Text = "";
            LoadRowReader();
            if (true 
                && LoadedClipRowReader != null 
                && !(LoadedClipRowReader["Created"] is DBNull) // protection from reading deleted clip
                )
            {
                clipType = LoadedClipRowReader["type"].ToString();
                string fullText = LoadedClipRowReader["Text"].ToString();
                string fullRTF = LoadedClipRowReader["richText"].ToString();
                string htmlText = GetHtmlFromHtmlClipText();
                useNativeTextFormatting = true
                                          && ClipAngel.Properties.Settings.Default.ShowNativeTextFormatting
                                          && (clipType == "html" || clipType == "rtf");
                Bitmap appIcon = ApplicationIcon(LoadedClipRowReader["appPath"].ToString());
                htmlDoc = htmlTextBox.Document.DomDocument as mshtml.IHTMLDocument2;
                if (clipType == "html")
                {
                    //htmlTextBox.Parent = new Control();
                    htmlTextBox.Parent.Enabled = false; // Prevent stealing focus
                    //htmlTextBox.Document.OpenNew(false);
                    //htmlTextBox.Document.Write("");
                    htmlDoc.write("");
                    htmlDoc.close(); // Steals focus!!!
                }
                if (appIcon != null)
                {
                    pictureBoxSource.Image = appIcon;
                }
                textBoxApplication.Text = LoadedClipRowReader["Application"].ToString();
                textBoxWindow.Text = LoadedClipRowReader["Window"].ToString();
                StripLabelCreated.Text = ((DateTime) LoadedClipRowReader["Created"]).ToString();
                if (!(LoadedClipRowReader["Size"] is DBNull))
                    StripLabelSize.Text = FormattedClipNumericPropery((int) LoadedClipRowReader["Size"], MultiLangByteUnit());
                if (!(LoadedClipRowReader["Chars"] is DBNull))
                    StripLabelVisualSize.Text = FormattedClipNumericPropery((int) LoadedClipRowReader["Chars"], MultiLangCharUnit());
                StripLabelType.Text = LocalTypeName(clipType);
                stripLabelPosition.Text = "1";
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
                    richTextInternal.Text = shortText;
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
                                   && clipType == "html"
                                   && !String.IsNullOrEmpty(htmlText);
                        if (htmlMode)
                        {
                            // fix for 1C formatted document source, fragment has very small width of textbox
                            string marker = "<DIV class=\"fullSize fdFieldMainContainer";
                            string replacement = "<DIV style=\"width: 100%\" class=\"fullSize fdFieldMainContainer";
                            htmlText = htmlText.Replace(marker, replacement);

                            string newStyle = " margin: 0;";
                            if (ClipAngel.Properties.Settings.Default.WordWrap)
                            {
                                newStyle += " word-wrap: break-word;";
                            }
                            htmlText = Regex.Replace(htmlText, "<body", "<body style=\"" + newStyle + "\" ", RegexOptions.IgnoreCase);

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
                            htmlTextBox.Document.Body.Drag += new HtmlElementEventHandler(htmlTextBoxDrag); // to prevent internal drag&drop
                            htmlTextBox.Document.Body.KeyDown += new HtmlElementEventHandler(htmlTextBoxDocumentKeyDown);

                            // Need to be called every time, else handler will be lost
                            htmlTextBox.Document.AttachEventHandler("onselectionchange", htmlTextBoxDocumentSelectionChange); // No multi call to handler, but why?
                            if (!htmlInitialized)
                            {
                                mshtml.HTMLDocumentEvents2_Event iEvent = (mshtml.HTMLDocumentEvents2_Event) htmlDoc;
                                iEvent.onclick += new mshtml.HTMLDocumentEvents2_onclickEventHandler(htmlTextBoxDocumentClick); //
                                iEvent.onmousedown += new mshtml.HTMLDocumentEvents2_onmousedownEventHandler(htmlTextBoxMouseDown); //
                                //iEvent.onselectionchange += new mshtml.HTMLDocumentEvents2_onselectionchangeEventHandler(htmlTextBoxDocumentSelectionChange);
                                htmlInitialized = true;
                            }
                        }
                        else
                        {
                            richTextInternal.Rtf = fullRTF;
                        }
                    }
                    else
                        richTextInternal.Text = fullText;
                    endMarker = MultiLangEndMarker();
                    TextWasCut = false;
                    markerColor = Color.Green;
                }
                clipRichTextLength = richTextInternal.TextLength;
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
                        MarkLinksInWebBrowser(htmlTextBox, out TextLinkMatches);
                        if (textPattern.Length > 0)
                        {
                            MarkMatchesInWebBrowser(htmlTextBox, textPattern, !String.IsNullOrEmpty(searchString));
                        }
                    }
                    else
                    {
                        richTextInternal.SelectionStart = richTextInternal.TextLength;
                        richTextInternal.SelectionColor = markerColor;
                        richTextInternal.SelectionFont = markerFont;
                        if (TextWasCut)
                            endMarker = Environment.NewLine + endMarker;
                        richTextInternal.AppendText(endMarker);
                        // Do it first, else ending hyperlink will connect underline to it

                        MarkLinksInRichTextBox(richTextInternal, out TextLinkMatches);
                        if (textPattern.Length > 0)
                        {
                            MarkRegExpMatchesInRichTextBox(richTextInternal, textPattern, Color.Red, true, false, !String.IsNullOrEmpty(searchString), out FilterMatches);
                        }
                        richTextInternal.AppendText(Environment.NewLine); // adding new line to prevent horizontal scroll to end of extra long last line
                    }
                }
                richTextInternal.SelectionColor = new Color();
                richTextInternal.SelectionStart = 0;
                richTextBox.Rtf = richTextInternal.Rtf;

                urlTextBox.HideSelection = true;
                urlTextBox.Clear();
                urlTextBox.Text = LoadedClipRowReader["Url"].ToString();
                MarkLinksInRichTextBox(urlTextBox, out UrlLinkMatches);
                if (clipType == "html")
                {
                    htmlTextBox.Parent.Enabled = true;
                }
            }
            else
            {
                richTextBox.Clear();
            }
            tableLayoutPanelData.SuspendLayout();
            UpdateClipButtons();
            if (comboBoxSearchString.Focused)
            {
                // Antibug webBrowser steals focus. We set it back
                int filterSelectionLength = comboBoxSearchString.SelectionLength;
                int filterSelectionStart = comboBoxSearchString.SelectionStart;
                comboBoxSearchString.Focus();
                comboBoxSearchString.SelectionStart = filterSelectionStart;
                comboBoxSearchString.SelectionLength = filterSelectionLength;
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
                if (elementPanelHasFocus)
                    richTextBox.Focus();
            }
            if (urlTextBox.Text == "")
                tableLayoutPanelData.RowStyles[3].Height = 0;
            else
                tableLayoutPanelData.RowStyles[3].Height = 25;
            if (EditMode && this.Visible && this.ContainsFocus)
                richTextBox.Focus(); // Can activate this window, so we check that window has focus
            if (clipType == "img")
            {
                Image image = GetImageFromBinary((byte[]) LoadedClipRowReader["Binary"]);
                ImageControl.Image = image;
            }
            tableLayoutPanelData.ResumeLayout();
            if (clipType == "img")
            {
                ImageControl.ZoomFitInside();
            }
            else
            {
                if (autoSelectMatch)
                    SelectNextMatchInClipText(false);
                else
                {
                    if (NewSelectionStart == -1)
                        NewSelectionStart = selectionStart;
                    if (NewSelectionLength == -1)
                        NewSelectionLength = selectionLength;
                    if (NewSelectionStart > 0 || NewSelectionLength > 0)
                    {
                        if (htmlMode)
                        {
                            IHTMLTxtRange range = SelectTextRangeInWebBrowser(NewSelectionStart, NewSelectionLength);
                            range.scrollIntoView();
                        }
                        else
                        {
                            SetRichTextboxSelection(NewSelectionStart, NewSelectionLength, true);
                        }
                    }
                }
                allowTextPositionChangeUpdate = true;
                OnClipContentSelectionChange();
            }
            tableLayoutPanelData.Refresh(); // to let user see clip in autoSelectNextClip state
        }

        protected virtual void LoadRowReader(int CurrentRowIndex = -1)
        {
            DataRowView CurrentRowView;
            LoadedClipRowReader = null;
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
            LoadedClipRowReader = getRowReader((int) CurrentRow["Id"]);
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

        private string GetHtmlFromHtmlClipText(bool AddSourceUrlComment = false)
        {
            string htmlClipText = LoadedClipRowReader["htmlText"].ToString();
            if (String.IsNullOrEmpty(htmlClipText))
                return "";
            int indexOfHtlTag = htmlClipText.IndexOf("<html", StringComparison.OrdinalIgnoreCase);
            if (indexOfHtlTag < 0)
                return "";
            string result = htmlClipText.Substring(indexOfHtlTag);
            if (AddSourceUrlComment)
                result = result + "\n<!-- Original URL - " + LoadedClipRowReader["url"].ToString() + " -->";
            return result;
        }

        private void RestoreTextSelection(int NewSelectionStart = -1, int NewSelectionLength = -1)
        {
        }

        private IHTMLTxtRange SelectTextRangeInWebBrowser(int NewSelectionStart, int NewSelectionLength)
        {
            IHTMLDocument2 htmlDoc = htmlTextBox.Document.DomDocument as IHTMLDocument2;
            IHTMLBodyElement body = htmlDoc.body as IHTMLBodyElement;
            IHTMLTxtRange range = body.createTextRange();
            range.moveStart("character", NewSelectionStart
                //+ GetNormalizedTextDeltaSize(RowReader["Text"].ToString().Substring(0, Math.Max(NewSelectionStart, 0)))
            );
            range.collapse();
            range.moveEnd("character", NewSelectionLength
                //+ GetNormalizedTextDeltaSize(RowReader["Text"].ToString().Substring(Math.Max(NewSelectionStart, 0), NewSelectionLength))
            );
            range.@select();
            return range;
        }

        private void SetRichTextboxSelection(int NewSelectionStart, int NewSelectionLength, bool preventHardScroll = false)
        {
            richTextBox.SelectionStart = NewSelectionStart;
            richTextBox.SelectionLength = NewSelectionLength;
            if (preventHardScroll)
                richTextBox.HideSelection = true; // slow // Exeption in ScrollToCaret can be thrown without this 
            try
            {
                richTextBox.ScrollToCaret();
            }
            catch
            {
                // Happens when click in not full loaded richTextBox 
            }
            if (preventHardScroll)
                richTextBox.HideSelection = false; // slow
        }

        private void OnClipContentSelectionChange()
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
            return "<" + Properties.Resources.EndMarker + ">";
        }

        private string MultiLangCutMarker()
        {
            return "<" + Properties.Resources.CutMarker + ">";
        }

        private string MultiLangCharUnit()
        {
            return Properties.Resources.CharUnit;
        }

        private string MultiLangByteUnit()
        {
            return Properties.Resources.ByteUnit;
        }

        private string MultiLangKiloByteUnit()
        {
            return Properties.Resources.KiloByteUnit;
        }

        private string MultiLangMegaByteUnit()
        {
            return Properties.Resources.MegaByteUnit;
        }

        private void MarkLinksInRichTextBox(RichTextBox control, out MatchCollection matches)
        {
            MarkRegExpMatchesInRichTextBox(control, "(" + fileOrFolderPattern + "|" + LinkPattern + "|" + TextPatterns["1CLine"] + ")", Color.Blue, false, true, false, out matches);
        }

        private void MarkRegExpMatchesInRichTextBox(RichTextBox control, string pattern, Color color, bool allowDymanicColor, bool underline,
            bool bold, out MatchCollection matches)
        {
            control.DetectUrls = false; // Antibug Framework 4.8 - explicit set value "false" in editor is ignored and changed to "true" in runtime
            RegexOptions options = RegexOptions.Singleline;
            if (!ClipAngel.Properties.Settings.Default.SearchCaseSensitive)
                options = options | RegexOptions.IgnoreCase;
            matches = Regex.Matches(control.Text, pattern, options);
            control.DeselectAll();
            int maxMarked = 50; // prevent slow down
            foreach (Match match in matches)
            {
                int startGroup = 2;
                if (match.Groups.Count < 2)
                    throw new ArgumentNullException("Wrong regexp pattern");
                control.SelectionStart = match.Groups[1].Index;
                control.SelectionLength = match.Groups[1].Length;
                if (allowDymanicColor && match.Groups.Count > 3)
                {
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
                if (control.SelectionFont != null)
                {
                    FontStyle newstyle = control.SelectionFont.Style;
                    if (bold)
                        newstyle = newstyle | FontStyle.Bold;
                    if (underline)
                        newstyle = newstyle | FontStyle.Underline;
                    if (newstyle != control.SelectionFont.Style)
                        control.SelectionFont = new Font(control.SelectionFont, newstyle);
                }
                maxMarked--;
                if (maxMarked < 0)
                    break;
            }
            control.DeselectAll();
            control.SelectionColor = new Color();
            control.SelectionFont = new Font(control.SelectionFont, FontStyle.Regular);
        }

        private void MarkLinksInWebBrowser(WebBrowser control, out MatchCollection matches)
        {
            MarkRegExpMatchesInWebBrowser(control, "((ъъъ)|(ъъъ)|" + TextPatterns["1CLine"] + ")", true, out matches);
        }

        private void MarkRegExpMatchesInWebBrowser(WebBrowser control, string pattern, bool is1Clink, out MatchCollection matches)
        {
            mshtml.IHTMLDocument2 htmlDoc = (mshtml.IHTMLDocument2)control.Document.DomDocument;
            RegexOptions options = RegexOptions.Singleline;
            if (!ClipAngel.Properties.Settings.Default.SearchCaseSensitive)
                options = options | RegexOptions.IgnoreCase;
            matches = Regex.Matches(LoadedClipRowReader["text"].ToString(), pattern, options);
            int maxMarked = 50; // prevent slow down
            string href = "";
            mshtml.IHTMLTxtRange range = null;
            foreach (Match match in matches)
            {
                range = SelectTextRangeInWebBrowser(match.Groups[1].Index, match.Groups[1].Length);
                if (is1Clink)
                    href = link1Cprefix + match.Index;
                range.execCommand("CreateLink", false, href);
                maxMarked--;
                if (maxMarked < 0)
                    break;
            }
            if (range != null)
            {
                SelectTextRangeInWebBrowser(1, 0);
            }
        }

        private void MarkMatchesInWebBrowser(WebBrowser control, string pattern, bool bold = false)
        {
            mshtml.IHTMLDocument2 htmlDoc = (mshtml.IHTMLDocument2) control.Document.DomDocument;
            mshtml.IHTMLBodyElement body = htmlDoc.body as mshtml.IHTMLBodyElement;
            int boundingTop = 0;
            int colorIndex = 0;
            int maxMarked = 50; // prevent slow down
            int searchFlags = 0;
            if (ClipAngel.Properties.Settings.Default.SearchCaseSensitive)
                searchFlags = 4;
            string[] array;
            if (ClipAngel.Properties.Settings.Default.SearchWordsIndependently)
                array = searchString.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            else
                array = new string[1] { searchString };
            foreach (var word in array)
            {
                mshtml.IHTMLTxtRange range = body.createTextRange();
                if (colorIndex >= _wordColors.Length)
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
            OpenLinkIfAltPressed(richTextBox, e, TextLinkMatches);
            if (MaxTextViewSize >= richTextBox.SelectionStart && TextWasCut)
                AfterRowLoad(true);
        }

        private void SearchString_TextChanged(object sender, EventArgs e)
        {
            if (AllowFilterProcessing || !ClipAngel.Properties.Settings.Default.FilterListBySearchString)
            {
                timerApplySearchString.Stop();
                timerApplySearchString.Start();
            }
        }

        async private void SearchStringApply()
        {
            ReadSearchString();
            searchMatchedIDs.Clear();
            if (ClipAngel.Properties.Settings.Default.FilterListBySearchString)
            {
                ReloadList(true);
            }
            else

            {
                UpdateSearchMatchedIDs();
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    row.Cells["ColumnTitle"].Value = null;
                }
                if (ClipAngel.Properties.Settings.Default.AutoSelectMatchedClip)
                {
                    GotoSearchMatchInList(true, true);
                }
                else
                {
                    SelectCurrentRow(true);
                }
            }
        }

        private void UpdateSearchMatchedIDs()
        {
            searchMatchedIDs.Clear();
            if (!String.IsNullOrEmpty(searchString))
            {
                SQLiteCommand command = new SQLiteCommand(m_dbConnection);
                command.CommandText = "Select Id From Clips";
                command.CommandText += " WHERE 1=1 " + SqlSearchFilter();
                command.CommandText += " ORDER BY " + sortField + " desc";
                if (ClipAngel.Properties.Settings.Default.SearchCaseSensitive)
                    command.CommandText = "PRAGMA case_sensitive_like = 1; " + command.CommandText;
                else
                    command.CommandText = "PRAGMA case_sensitive_like = 0; " + command.CommandText;
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    searchMatchedIDs.Add((int) reader["id"]);
                }
            }
        }

        private void GotoSearchMatchInList(bool Forward = true, bool resetPosition = false)
        {
            if (searchMatchedIDs.Count == 0)
                return;
            int currentListMatchIndex;
            if (Forward)
            {
                if (resetPosition)
                    currentListMatchIndex = 0;
                else
                {
                    currentListMatchIndex = searchMatchedIDs.Count - 1;
                    if (dataGridView.CurrentRow != null)
                    {
                        int ListMatchIndex;
                        for (int index = dataGridView.CurrentRow.Index + 1; index < dataGridView.RowCount; index++)
                        {
                            DataRowView dataRow = (DataRowView)dataGridView.Rows[index].DataBoundItem;
                            ListMatchIndex = searchMatchedIDs.IndexOf((int)dataRow["Id"]);
                            if (ListMatchIndex > -1)
                            {
                                currentListMatchIndex = ListMatchIndex;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                currentListMatchIndex = 0;
                if (dataGridView.CurrentRow != null)
                {
                    int ListMatchIndex;
                    for (int index = dataGridView.CurrentRow.Index - 1; index >= 0; index--)
                    {
                        DataRowView dataRow = (DataRowView)dataGridView.Rows[index].DataBoundItem;
                        ListMatchIndex = searchMatchedIDs.IndexOf((int)dataRow["Id"]);
                        if (ListMatchIndex > -1)
                        {
                            currentListMatchIndex = ListMatchIndex;
                            break;
                        }
                    }
                }
            }
            RestoreSelectedCurrentClip(true, searchMatchedIDs[currentListMatchIndex]);
        }

        private void SelectRowByID(int IDToSelect)
        {
            int newIndex = clipBindingSource.Find("Id", IDToSelect);
            if (newIndex >= 0)
            {
                dataGridView.Rows[newIndex].Selected = false;
                dataGridView.Rows[newIndex].Selected = true;
            }
        }

        async private void ReloadList(bool forceRowLoad = false, int currentClipId = 0, bool keepTextSelectionIfIDChanged = false, List<int> selectedClipIDs = null, bool waitFinish = false)
        {
            if (globalDataAdapter == null)
                return;
            if (!(this.Visible && this.ContainsFocus))
                sortField = "Id";
            if (EditMode)
                SaveClipText();
            string TypeFilterSelectedValue = TypeFilter.SelectedValue as string;
            if (currentClipId == 0 && clipBindingSource.Current != null)
            {
                currentClipId = (int)(clipBindingSource.Current as DataRowView).Row["Id"];
                if (dataGridView.SelectedRows.Count > 1 && selectedClipIDs == null)
                {
                    selectedClipIDs = new List<int>();
                    foreach (DataGridViewRow selectedRow in dataGridView.SelectedRows)
                    {
                        if (selectedRow == null)
                            continue;
                        DataRowView dataRow = (DataRowView)selectedRow.DataBoundItem;
                        selectedClipIDs.Insert(0, (int)dataRow["Id"]);
                    }
                }
            }
            string MarkFilterSelectedValue = MarkFilter.SelectedValue as string;
            DateTime monthCalendar1SelectionStart = monthCalendar1.SelectionStart;
            DateTime monthCalendar1SelectionEnd = monthCalendar1.SelectionEnd;
            Task<DataTable> reloadListTask = Task.Run(() => ReloadListAsync(TypeFilterSelectedValue, MarkFilterSelectedValue, monthCalendar1SelectionStart, monthCalendar1SelectionEnd));
            lastReloadListTask = reloadListTask;
            if (waitFinish)
                reloadListTask.Wait();
            DataTable table = await reloadListTask;
            if (true
                && lastReloadListTask != null
                && lastReloadListTask != reloadListTask
                //&& (false
                //    || lastReloadListTask.IsCompleted 
                //    || (true 
                //        && lastReloadListTime != null
                //        && DateDiffMilliseconds(lastReloadListTime, DateTime.Now) < 5000))
               )
            {
                return;
            }
            lastReloadListTime = DateTime.Now;
            clipBindingSource.DataSource = table;
            stripLabelPosition.Spring = false;
            stripLabelPosition.Width = 50;
            stripLabelFiltered.Visible = filterOn;
            if (filterOn)
                stripLabelFiltered.Text = String.Format(Properties.Resources.FilteredStatusText, table.Rows.Count);
            stripLabelPosition.Spring = true;
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
            //ClipsNumber = clipBindingSource.Count;
            if (LastId == 0)
            {
                GotoLastRow();
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
                        SelectRowByID(selectedID);
                    }
                    allowProcessDataGridSelectionChanged = true;
                }
            }
            allowRowLoad = true;
            //AutoGotoLastRow = false;
        }

        private async Task<DataTable> ReloadListAsync(string TypeFilterSelectedValue, string MarkFilterSelectedValue, DateTime monthCalendar1SelectionStart, DateTime monthCalendar1SelectionEnd)
        {
            if (backgroundDataAdapter != null)
            {
                backgroundDataAdapter.SelectCommand.Cancel();
            }
            backgroundDataAdapter = new SQLiteDataAdapter("", m_dbConnection);
            allowRowLoad = false;
            bool oldFilterOn = filterOn;
            filterOn = false;
            string sqlFilter = "1 = 1";
            string filterValue = "";
            if (!String.IsNullOrEmpty(searchString) && ClipAngel.Properties.Settings.Default.FilterListBySearchString)
            {
                sqlFilter += SqlSearchFilter();
                filterOn = true;
                if (ClipAngel.Properties.Settings.Default.SearchIgnoreBigTexts)
                    sqlFilter += " AND (Chars < 100000 OR type = 'img')";
            }
            if (TypeFilterSelectedValue as string != "allTypes")
            {
                filterValue = TypeFilterSelectedValue as string;
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
            if (MarkFilterSelectedValue as string != "allMarks")
            {
                filterValue = MarkFilterSelectedValue as string;
                sqlFilter += " AND " + filterValue;
                filterOn = true;
            }
            if (periodFilterOn)
            {
                sqlFilter += " AND Created BETWEEN @startDate AND @endDate ";
                backgroundDataAdapter.SelectCommand.Parameters.AddWithValue("startDate", monthCalendar1SelectionStart);
                backgroundDataAdapter.SelectCommand.Parameters.AddWithValue("endDate", monthCalendar1SelectionEnd);
                filterOn = true;
            }
            if (!oldFilterOn && filterOn)
                selectedClipsBeforeFilterApply.Clear();
            // Dublicated code 8gfd8843
            //string selectCommandText = "Select Id, Used, Title, Chars, Type, Favorite, ImageSample, AppPath, Size, Created From Clips";
            string selectCommandText = "Select Id, NULL AS Used, NULL AS Title, NULL AS Chars, NULL AS Type, NULL AS Favorite, NULL AS ImageSample, NULL AS AppPath, NULL AS Size, NULL AS Created From Clips";
            selectCommandText += " WHERE " + sqlFilter;
            selectCommandText += " ORDER BY " + sortField + " desc";
            if (ClipAngel.Properties.Settings.Default.SearchCaseSensitive)
                selectCommandText = "PRAGMA case_sensitive_like = 1; " + selectCommandText;
            else
                selectCommandText = "PRAGMA case_sensitive_like = 0; " + selectCommandText;
            backgroundDataAdapter.SelectCommand.CommandText = selectCommandText;

            DataTable table = new DataTable();
            table.Locale = CultureInfo.InvariantCulture;
            try
            {
                backgroundDataAdapter.Fill(table);
            }
            catch (Exception e)
            {
                int dummy = 0;
            }
            return table;
        }

        private string SqlSearchFilter()
        {
            string[] array;
            string filterTextTemp = searchString;
            filterTextTemp = filterTextTemp.Replace("\\", "\\\\");
            filterTextTemp = filterTextTemp.Replace("_", "\\_");
            filterTextTemp = filterTextTemp.Replace("'", "''");
            if (!ClipAngel.Properties.Settings.Default.SearchWildcards)
                filterTextTemp = filterTextTemp.Replace("%", "\\%");
            if (ClipAngel.Properties.Settings.Default.SearchWordsIndependently)
                array = filterTextTemp.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            else
                array = new string[1] {filterTextTemp};
            List<string> fields = new List<string>{"Text", "Title"};
            if (ClipAngel.Properties.Settings.Default.SearchAllFields)
            {
                fields.Add("Window");
                fields.Add("Url");
            }
            string sqlSearchFilter = "";
            foreach (string field in fields)
            {
                sqlSearchFilter += " OR (1=1";
                foreach (var word in array)
                {
                    if (ClipAngel.Properties.Settings.Default.SearchCaseSensitive)
                        sqlSearchFilter += "\n AND " + field + " Like '%" + word + "%' ESCAPE '\\'";
                    else
                        sqlSearchFilter += "\n AND UPPER(" + field + ") Like UPPER('%" + word + "%') ESCAPE '\\'";
                }
                sqlSearchFilter += ")";
            }
            if (!String.IsNullOrEmpty(sqlSearchFilter))
                sqlSearchFilter = " AND (1=0" + sqlSearchFilter + ")";
            return sqlSearchFilter;
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
                {
                    UpdateSelectedClipsHistory();
                    GotoLastRow();
                }
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

        private void ClearFilter(int CurrentClipID = 0, bool keepMarkFilterFilter = false, bool waitFinish = false)
        {
            if (filterOn)
            {
                AllowFilterProcessing = false;
                comboBoxSearchString.Text = "";
                periodFilterOn = false;
                ReadSearchString();
                TypeFilter.SelectedIndex = 0;
                if (!keepMarkFilterFilter)
                    MarkFilter.SelectedIndex = 0;
                AllowFilterProcessing = true;
                //UpdateClipBindingSource(false, CurrentClipID);
                ReloadList(true, CurrentClipID, false, null, waitFinish); // To repaint text
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
                bool lastActSet = false;
                if (lastActiveParentWindow != null)
                    lastActSet = SetForegroundWindow(lastActiveParentWindow);
                if (!lastActSet)
                    //SetForegroundWindow(IntPtr.Zero); // This way focus was not lost!
                    SetActiveWindow(IntPtr.Zero);
                if (ClipAngel.Properties.Settings.Default.FastWindowOpen)
                {
                    //bool lastActSet = false;
                    //if (lastActiveParentWindow != null)
                    //    lastActSet = SetForegroundWindow(lastActiveParentWindow);
                    //if (!lastActSet)
                    //    //SetForegroundWindow(IntPtr.Zero); // This way focus was not lost!
                    //    SetActiveWindow(IntPtr.Zero);
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
                //if (ClipAngel.Properties.Settings.Default.ClearFiltersOnClose)
                //    ClearFilter();
                //this.ResumeLayout();
            }
            else
            {
                if (WindowState == FormWindowState.Normal)
                    ClipAngel.Properties.Settings.Default.MainWindowSize = Size;
                else
                    ClipAngel.Properties.Settings.Default.MainWindowSize = RestoreBounds.Size;
            }
        }

        public static ImageFormat GetImageFormat(Image img)
        {
            if (img.RawFormat.Equals(ImageFormat.Jpeg))
                return ImageFormat.Jpeg;
            if (img.RawFormat.Equals(ImageFormat.Bmp))
                return ImageFormat.Bmp;
            if (img.RawFormat.Equals(ImageFormat.Png))
                return ImageFormat.Png;
            if (img.RawFormat.Equals(ImageFormat.Emf))
                return ImageFormat.Emf;
            if (img.RawFormat.Equals(ImageFormat.Exif))
                return ImageFormat.Exif;
            if (img.RawFormat.Equals(ImageFormat.Gif))
                return ImageFormat.Gif;
            if (img.RawFormat.Equals(ImageFormat.Icon))
                return ImageFormat.Icon;
            if (img.RawFormat.Equals(ImageFormat.MemoryBmp))
                return ImageFormat.MemoryBmp;
            if (img.RawFormat.Equals(ImageFormat.Tiff))
                return ImageFormat.Tiff;
            else
                return ImageFormat.Wmf;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetClipboardOwner();
        [DllImport("user32.dll")]
        private static extern bool IsHungAppWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("user32.dll")]
        private static extern uint RegisterClipboardFormat(string lpszFormat);

        public struct ClipFormat
        {
            public string Name;
            public uint Id;
            public ClipFormat(string name, uint id)
            {
                Name = name;
                Id = id;
            }
        }

        private void fix1CFormat()
        {
            if (!ClipAngel.Properties.Settings.Default.MonitoringClipboard)
                return;
            //IDataObject iData = Clipboard.GetDataObject(); // Зависает
            ClipFormat[] formats = new ClipFormat[]
            {
                new ClipFormat("1C:MD8", 0),
                new ClipFormat("1C:MD8 Info", 0),
                new ClipFormat("1C:MD8 External Data", 0)
            };
            for (int i = 0; i < formats.Length; i++)
            {
                formats[i].Id = RegisterClipboardFormat(formats[i].Name);
            }
            bool formatFound = false;
            foreach (ClipFormat clipFormat in formats)
            {
                if (IsClipboardFormatAvailable(clipFormat.Id))
                {
                    formatFound = true;
                    break;
                }
            }
            if (formatFound)
            {
                ClipboardOwner clipboardOwner = GetClipboardOwnerLockerInfo(false);
                if (true
                    && clipboardOwner.application.StartsWith("1cv8")
                    && IsHungAppWindow(clipboardOwner.windowHandle))
                {
                    GotoLastRow();
                    CopyClipToClipboard();
                }
            }
        }

        private void CaptureClipboardData()
        {
            DateTime now = DateTime.Now;
            captureTimer.Stop();
            if (tempCaptureTimer.Enabled)
            {
                tempCaptureTimer.Stop();
                ClipAngel.Properties.Settings.Default.MonitoringClipboard = false;
                RemoveClipboardFormatListener(this.Handle);
            }
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
            string imageUrl = "";
            int clipChars = 0;
            bool needUpdateList = false;
            ClipboardOwner clipboardOwner = GetClipboardOwnerLockerInfo(false);
            if (ignoreModulesInClipCapture.Contains(clipboardOwner.application.ToLower()))
                return;
            try
            {
                iData = Clipboard.GetDataObject();
            }
            catch (Exception ex)
            {
                // Copying a field definition in Access 2002 causes this sometimes or Excel big clips
                Debug.WriteLine(String.Format("Clipboard.GetDataObject(): InteropServices.ExternalException: {0}", ex.Message));
                string Message = Properties.Resources.ErrorReadingClipboard + ": " + ex.Message;
                notifyIcon.ShowBalloonTip(2000, Application.ProductName, Message, ToolTipIcon.Info);
                return;
            }
            if (iData.GetDataPresent(DataFormat_RemoveTempClipsFromHistory))
            {
                removeClipsFilter removeClipsFilter;
                var dataString = GetStringFromClipboardData(iData, DataFormat_RemoveTempClipsFromHistory);
                if (!String.IsNullOrWhiteSpace(dataString))  // Rarely we got NULL 
                {
                    // Test
                    //removeClipsFilter removeClipsFilter = new removeClipsFilter
                    //{
                    //    PID = 0,
                    //    TimeStart = DateTime.Now.AddMilliseconds(-1000),
                    //    TimeEnd = DateTime.Now
                    //};
                    //dataString = JsonConvert.SerializeObject(removeClipsFilter);

                    removeClipsFilter = JsonConvert.DeserializeObject<removeClipsFilter>(dataString);
                    double maxRemovedClipAge = removeClipsFilter.TimeEnd.Subtract(removeClipsFilter.TimeStart).TotalMilliseconds; // Докинем немного времени на путь от источника до приемника
                    double timeDelta = now.Subtract(removeClipsFilter.TimeEnd).TotalMilliseconds;
                    for (int i = lastClips.Count - 1; i >= 0; i--)
                    {
                        LastClip lastClip = lastClips[i];
                        double clipAge = now.Subtract(lastClip.Created).TotalMilliseconds;
                        if (true
                            // Will not work correctly during capturing clip from remote desktop session
                            //&& (false
                            //    || removeClipsFilter.PID == 0
                            //    || removeClipsFilter.PID == lastClip.ProcessID)
                            && clipAge < maxRemovedClipAge)
                        {
                            SQLiteCommand command = new SQLiteCommand("Delete from Clips where Id = @Id and not Favorite", m_dbConnection);
                            command.Parameters.Add("Id", DbType.Int32).Value = lastClip.ID;
                            command.ExecuteNonQuery();
                            lastClips.RemoveAt(i);
                            needUpdateList = true;
                        }
                    }
                }
            }
            if (iData.GetDataPresent(DataFormat_ClipboardViewerIgnore) && ClipAngel.Properties.Settings.Default.IgnoreExclusiveFormatClipCapture)
            {
                if (needUpdateList)
                    ReloadList();
                return;
            }
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
                    clipText = GetStringFromClipboardData(iData, DataFormats.UnicodeText);
                    if (!String.IsNullOrEmpty(clipText))
                    {
                        clipType = "text";
                        textFormatPresent = true;
                    }
                }
                if (!textFormatPresent && iData.GetDataPresent(DataFormats.Text))
                {
                    clipText = GetStringFromClipboardData(iData, DataFormats.Text);
                    if (!String.IsNullOrEmpty(clipText))
                    {
                        clipType = "text";
                        textFormatPresent = true;
                    }
                }
                string Moxel1C8 = "1C:Moxcel8 Document";
                if (iData.GetDataPresent(Moxel1C8))
                {
                    object data = iData.GetData(Moxel1C8);
                    if (data.GetType() == typeof(MemoryStream))
                    {
                        // Excel
                        using (MemoryStream ms = (MemoryStream)data)
                        {
                            if (ms != null && ms.Length > 0)
                            {
                                byte[] buffer = new byte[ms.Length];
                                ms.Read(buffer, 0, (int)ms.Length);
                                string xmlSheet = Encoding.UTF8.GetString(buffer);
                                Match match = Regex.Match(xmlSheet, "\\{0,0\\},\\d+,\\d+,(\\d+),\\d+,\\d+,(\\d+),\\d+");
                                int NumberOfColumns = 1;
                                int NumberOfRows = 1;
                                if (match.Success)
                                {
                                    NumberOfColumns = Convert.ToInt32(match.Groups[1].Value);
                                    NumberOfRows = Convert.ToInt32(match.Groups[2].Value);
                                }
                                NumberOfImageCells = NumberOfRows * NumberOfColumns;
                                NumberOfFilledCells = NumberOfImageCells;
                            }
                        }
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
                            if (ms != null && ms.Length > 0)
                            {
                                byte[] buffer = new byte[ms.Length];
                                ms.Read(buffer, 0, (int)ms.Length);
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
                        || ClipAngel.Properties.Settings.Default.MaxCellsToCaptureFormattedText > NumberOfFilledCells))
                {
                    htmlText = GetStringFromClipboardData(iData, DataFormats.Html);
                    if (String.IsNullOrEmpty(htmlText))
                    {
                        htmlText = "";
                    }
                    else
                    {
                        clipType = "html";
                        // Example of htmlText
                        //Version: 1.0
                        //StartHTML: 000000174
                        //EndHTML: 000000603
                        //StartFragment: 000000285
                        //EndFragment: 000000571
                        //StartSelection: 000000285
                        //EndSelection: 000000571
                        //SourceURL: about: blank
                        //    <!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN">
                        //    <HTML><HEAD></HEAD>
                        //    <BODY><!--StartFragment--><PRE><SPAN class=k><FONT color =#ff0000>Для</FONT></SPAN> бизнес<SPAN class=k><FONT color=#ff0000>-</FONT></SPAN>процесса <SPAN class=s>"Согласование изменений маршрута</SPAN>" добавлена команда <SPAN class=s>"</SPAN>
                        //    </PRE><!--EndFragment--></BODY></HTML>
                        Match match = Regex.Match(htmlText, @"SourceURL:(file:///?)?(.*?)(?:\n|\r|$)", RegexOptions.IgnoreCase);
                        if (match.Captures.Count > 0)
                        {
                            clipUrl = match.Groups[2].ToString();
                            if (!String.IsNullOrEmpty(match.Groups[1].ToString()))
                            {
                                clipUrl = System.Web.HttpUtility.UrlDecode(clipUrl);
                                clipUrl = clipUrl.Replace(@"/", @"\");
                            }
                            string[] IgnoreTemplates = Settings.Default.IgnoreUrlsClipCapture.Trim().ToLower().Split(new string[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
                            if (IgnoreTemplates.Length > 0)
                            {
                                foreach (var template in IgnoreTemplates)
                                {
                                    if (clipUrl.ToLower().Contains(template.Trim()))
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                        if (ClipAngel.Properties.Settings.Default.CaptureImages && String.IsNullOrWhiteSpace(clipText))
                        {
                            // It may take much time to parse big html
                            var htmlParser = new HtmlParser();
                            var documentHtml = htmlParser.Parse(htmlText);
                            if (documentHtml.Images.Length > 0)
                            {
                                imageUrl = documentHtml.Images[0].Source;
                                if (iData.GetDataPresent(DataFormats.Bitmap) && documentHtml.TextContent == null && documentHtml.Images.Length == 1)
                                {
                                    // Command "Copy image" executed in browser
                                    htmlText = "";
                                    //clipType = "";
                                }
                                else// if (!imageUrl.StartsWith("data:image"))
                                {
                                    if (bitmap == null)
                                        bitmap = getBitmapFromUrl(imageUrl);
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
                        || ClipAngel.Properties.Settings.Default.MaxCellsToCaptureFormattedText > NumberOfFilledCells))
                {
                    richText = GetStringFromClipboardData(iData, DataFormats.Rtf);
                    clipType = "rtf";
                    if (!textFormatPresent)
                    {
                        var rtfBox = new RichTextBox();
                        rtfBox.Rtf = richText;
                        clipText = rtfBox.Text;
                        textFormatPresent = true;
                    }
                }
                if (ClipAngel.Properties.Settings.Default.CaptureImages && textFormatPresent && bitmap == null)
                {
                    Match match;
                    match = Regex.Match(clipText, "^\\s*" + videoPattern + "\\s*$", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        imageUrl = "http://img.youtube.com/vi/";
                        int youtubeIndex = (match.Groups.Count - 1);
                        var youtubeId = match.Groups[youtubeIndex].ToString();
                        imageUrl = imageUrl + youtubeId + "/default.jpg";
                        bitmap = getBitmapFromUrl(imageUrl);
                    }
                    match = Regex.Match(clipText, "^\\s*" + imagePattern + "\\s*$", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        imageUrl = match.Value;
                        bitmap = getBitmapFromUrl(imageUrl);
                    }
                }
                if (!String.IsNullOrEmpty(clipText) && String.IsNullOrEmpty(richText) && String.IsNullOrEmpty(htmlText) && ClipAngel.Properties.Settings.Default.Max1CCodeSizeToColorize > clipText.Length)
                {
                    string[] textLines = TextToLines(clipText.ToLower());
                    int maxLinesToProcess = 100;
                    maxLinesToProcess = Math.Min(textLines.Length, maxLinesToProcess);
                    string line;
                    int positiveScore = 0;
                    int negativeScore = 0;
                    SyntaxHighlighter syntax1C = new SyntaxHighlighter();
                    for (int i = 0; i < maxLinesToProcess; i++)
                    {
                        line = textLines[i];
                        int lineScore = syntax1C.isLineLike1C(line);
                        if (lineScore > 0)
                            positiveScore++;
                        else if (lineScore < 0)
                            negativeScore++;
                    }
                    if (false
                        || negativeScore == 0 && clipboardOwner.is1CCode && textLines.Length > 1
                        || negativeScore == 0 && positiveScore > 1
                        || negativeScore > 0 && positiveScore > negativeScore)
                    {
                        htmlText = syntax1C.ProcessCode(clipText);
                        htmlText = ClipboardHelper.GetHtmlDataString(htmlText);
                        clipType = "html";
                    }
                }

                if (iData.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] fileNameList = iData.GetData(DataFormats.FileDrop) as string[];
                    if (fileNameList != null)
                    {
                        if (ClipAngel.Properties.Settings.Default.CaptureImages && fileNameList.Length == 1 && iData.GetDataPresent(DataFormats.Bitmap))
                        {
                            // Command "Copy image" executed in browser IE
                            clipType = "";
                        }
                        else
                        {
                            clipText = String.Join("\n", fileNameList);
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
                    && ClipAngel.Properties.Settings.Default.CaptureImages
                    && iData.GetDataPresent(DataFormats.Bitmap)
                    && (false
                        || NumberOfImageCells == 0 && string.IsNullOrWhiteSpace(clipText)
                        || NumberOfImageCells != 0 && ClipAngel.Properties.Settings.Default.MaxCellsToCaptureImage > NumberOfImageCells))
                {
                    //clipType = "img";
                    if (iData.GetDataPresent(DataFormats.Dib))
                    {
                        var dibData = Clipboard.GetData(DataFormats.Dib);
                        if (dibData != null)
                        {
                            var dib = ((MemoryStream)dibData).ToArray();
                            bitmap = ImageFromClipboardDib(dib);
                        }
                    }
                    if (bitmap == null)
                        // Second - get 24b bitmap
                        bitmap = iData.GetData(DataFormats.Bitmap, false) as Bitmap;

                    if (bitmap != null) // NUll happens while copying image in standard image viewer Windows 10
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            bitmap.Save(memoryStream, ImageFormat.Png);
                            binaryBuffer = memoryStream.ToArray();
                        }
                        //clipTextImage = Properties.Resources.Size + ": " + image.Width + "x" + image.Height + "\n"
                        //     + Properties.Resources.PixelFormat + ": " + image.PixelFormat + "\n";
                        clipTextImage = bitmap.Width + " x " + bitmap.Height;
                        if (!String.IsNullOrEmpty(clipboardOwner.windowTitle))
                            clipTextImage += ", " + clipboardOwner.windowTitle;
                        clipTextImage += ", " + Properties.Resources.PixelFormat + ": " + Image.GetPixelFormatSize(bitmap.PixelFormat);
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

                if (clipType == "html" && clipText == "")
                    clipType = "";
                // Split Image+Html clip into 2: Image and Html 
                if (clipTextImage != "")
                {
                    // Image clip
                    if (!String.IsNullOrEmpty(clipUrl))
                        imageUrl = clipUrl;
                    if (imageUrl.StartsWith("data:image"))
                        imageUrl = "";
                    bool clipAdded = AddClip(binaryBuffer, imageSampleBuffer, "", "", "img", clipTextImage, clipboardOwner.application,
                        clipboardOwner.windowTitle, imageUrl, clipCharsImage, clipboardOwner.appPath, false, false, false, "");
                    needUpdateList = needUpdateList || clipAdded;

                    if (!String.IsNullOrWhiteSpace(clipText))
                        imageSampleBuffer = new byte[0];
                }
                if (clipType != "")
                {
                    // Non image clip
                    bool clipAdded = AddClip(new byte[0], imageSampleBuffer, htmlText, richText, clipType, clipText, clipboardOwner.application,
                        clipboardOwner.windowTitle, clipUrl, clipChars, clipboardOwner.appPath, false, false, false, "");
                    needUpdateList = needUpdateList || clipAdded;
                }
            }
            finally
            {
                bitmap?.Dispose();
            }
            if (needUpdateList)
                ReloadList();
        }

        private string GetStringFromClipboardData(IDataObject iData, string formatName)
        {
            try
            {
                var data = iData.GetData(formatName);
                if (data == null) return "";
                if (data is string)
                {
                    return (string) data;
                }
                if (!(data is MemoryStream))
                {
                    throw new Exception("Stream is Null: " + formatName);
                }
                Encoding encoding;
                if (formatName == DataFormats.UnicodeText || formatName == DataFormat_RemoveTempClipsFromHistory)
                {
                    encoding = Encoding.Unicode;
                }
                else if (formatName == DataFormats.OemText)
                {
                    encoding = Encoding.ASCII;
                }
                else
                {
                    encoding = Encoding.UTF8;
                }
                MemoryStream stream = (MemoryStream)data;
                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, (int)stream.Length);
                return encoding.GetString(buffer);
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public static Bitmap ImageFromClipboardDib(Byte[] dibBytes)
        {
            Bitmap bitmap = null;
            var width = 0;
            var height = 0;
            var planes = 0;
            var bitCount = 0;
            var compression = 0;
            var headerSize = 0;
            if (dibBytes == null || dibBytes.Length < 4)
                return null;
            headerSize = BitConverter.ToInt32(dibBytes, 0);
            // Only supporting 40-byte DIB from clipboard
            if (headerSize != 40)
                return null;

            // First - try get 24b bitmap + 8b alpfa (transparency)
            // https://www.csharpcodi.com/vs2/1561/noterium/src/Noterium.Core/Helpers/ClipboardHelper.cs/
            // https://www.hostedredmine.com/issues/929403
            width = BitConverter.ToInt32(dibBytes, 4);
            height = BitConverter.ToInt32(dibBytes, 8);
            planes = BitConverter.ToInt16(dibBytes, 12);
            bitCount = BitConverter.ToInt16(dibBytes, 14);
            compression = BitConverter.ToInt32(dibBytes, 16);
            if (bitCount == 32 && planes == 1 && (compression == 0))
            {
                var gch = GCHandle.Alloc(dibBytes, GCHandleType.Pinned);
                try
                {
                    var ptr = new IntPtr((long)gch.AddrOfPinnedObject() + headerSize);
                    bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppArgb, ptr);
                    bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
                }
                finally
                {
                    gch.Free();
                }
            }
            return bitmap;

            //// https://www.folkstalk.com/tech/copying-from-and-to-clipboard-loses-image-transparency-with-code-solution/
            //try
            //{
            //    Byte[] header = new Byte[headerSize];
            //    Array.Copy(dibBytes, header, headerSize);
            //    Int32 imageIndex = headerSize;
            //    width = BitConverter.ToInt32(header, 0x04);
            //    height = BitConverter.ToInt32(header, 0x08);
            //    planes = BitConverter.ToInt16(header, 0x0C);
            //    bitCount = BitConverter.ToInt16(header, 0x0E);
            //    //Compression: 0 = RGB; 3 = BITFIELDS.
            //    compression = BitConverter.ToInt32(header, 0x10);
            //    // Not dealing with non-standard formats.
            //    if (planes != 1 || (compression != 0 && compression != 3))
            //        return null;
            //    PixelFormat format;
            //    switch (bitCount)
            //    {
            //        case 32:
            //            format = PixelFormat.Format32bppArgb; //PixelFormat.Format32bppRgb;
            //            break;
            //        case 24:
            //            format = PixelFormat.Format24bppRgb;
            //            break;
            //        case 16:
            //            format = PixelFormat.Format16bppRgb555;
            //            break;
            //        default:
            //            return null;
            //    }
            //    if (compression == 3)
            //        imageIndex += 12;
            //    if (dibBytes.Length < imageIndex)
            //        return null;
            //    Byte[] image = new Byte[dibBytes.Length - imageIndex];
            //    Array.Copy(dibBytes, imageIndex, image, 0, image.Length);
            //    // Classic stride: fit within blocks of 4 bytes.
            //    Int32 stride = (((((bitCount * width) + 7) / 8) + 3) / 4) * 4;
            //    if (compression == 3)
            //    {
            //        if (format == PixelFormat.Format32bppArgb)
            //            format = PixelFormat.Format32bppRgb; // For Excel
            //        UInt32 redMask = BitConverter.ToUInt32(dibBytes, headerSize + 0);
            //        UInt32 greenMask = BitConverter.ToUInt32(dibBytes, headerSize + 4);
            //        UInt32 blueMask = BitConverter.ToUInt32(dibBytes, headerSize + 8);
            //        // Fix for the undocumented use of 32bppARGB disguised as BITFIELDS. Despite lacking an alpha bit field,
            //        // the alpha bytes are still filled in, without any header indication of alpha usage.
            //        // Pure 32-bit RGB: check if a switch to ARGB can be made by checking for non-zero alpha.
            //        // Admitted, this may give a mess if the alpha bits simply aren't cleared, but why the hell wouldn't it use 24bpp then?
            //        if (bitCount == 32 && redMask == 0xFF0000 && greenMask == 0x00FF00 && blueMask == 0x0000FF)
            //        {
            //            // Stride is always a multiple of 4; no need to take it into account for 32bpp.
            //            for (Int32 pix = 3; pix < image.Length; pix += 4)
            //            {
            //                // 0 can mean transparent, but can also mean the alpha isn't filled in, so only check for non-zero alpha,
            //                // which would indicate there is actual data in the alpha bytes.
            //                if (image[pix] == 0)
            //                    continue;
            //                format = PixelFormat.Format32bppPArgb;
            //                break;
            //            }
            //        }
            //        else
            //            // Could be supported with a system that parses the colour masks,
            //            // but I don't think the clipboard ever uses these anyway.
            //            return null;
            //    }
            //    var gch = GCHandle.Alloc(image, GCHandleType.Pinned);
            //    try
            //    {
            //        var ptr = new IntPtr((long)gch.AddrOfPinnedObject());
            //        bitmap = new Bitmap(width, height, stride, format, ptr);
            //    }
            //    finally
            //    {
            //        gch.Free();
            //    }
            //    if (bitmap != null)
            //        // This is bmp; reverse image lines.
            //        bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
            //    return bitmap;
            //}
            //catch
            //{
            //    return null;
            //}
        }
        private Bitmap getBitmapFromUrl(string imageUrl)
        {
            Bitmap bitmap = null;
            if (ClipAngel.Properties.Settings.Default.AllowDownloadThumbnail||imageUrl.StartsWith("data:image")||File.Exists(imageUrl))
                using (WebClient webClient = new WebClient())
                {
                    webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    try
                    {
                        byte[] tempBuffer = webClient.DownloadData(imageUrl);
                        using (var ms = new MemoryStream(tempBuffer))
                        {
                            bitmap = new Bitmap(ms);
                        }
                    }
                    catch
                    {
                    }
                }
            return bitmap;
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

        bool AddClip(byte[] binaryBuffer = null, byte[] imageSampleBuffer = null, string htmlText = "", string richText = "", string typeText = "text", string plainText = "",
            string applicationText = "", string windowText = "", string url = "", int chars = 0, string appPath = "", bool used = false, bool favorite = false, bool updateList = true,
            string clipTitle = "", DateTime created = new DateTime())
        {
            DateTime dtNow = DateTime.Now;
            int msFromLastCapture = DateDiffMilliseconds(lastCaptureMoment);
            if (plainText == null)
                plainText = "";
            if (richText == null)
                richText = "";
            if (htmlText == null)
                htmlText = "";
            int byteSize = 0;
            CalculateByteAndCharSizeOfClip(htmlText, richText, plainText, ref chars, ref byteSize);
            if (binaryBuffer != null)
                byteSize += binaryBuffer.Length;
            if (byteSize > ClipAngel.Properties.Settings.Default.MaxClipSizeKB * 1000)
            {
                string message = String.Format(Properties.Resources.ClipWasNotCaptured, (int)(byteSize / 1024), ClipAngel.Properties.Settings.Default.MaxClipSizeKB,
                    LocalTypeName(typeText));
                notifyIcon.ShowBalloonTip(2000, Application.ProductName, message, ToolTipIcon.Info);
                return false;
            }
            if (!String.IsNullOrEmpty(ClipAngel.Properties.Settings.Default.PlaySoundOnClipCapture))
            {
                string soundFileName = ClipAngel.Properties.Settings.Default.PlaySoundOnClipCapture;
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(soundFileName);
                try
                {
                    player.Play();
                }
                catch
                {
                }
            }
            int oldCurrentClipId = 0;
            lastClipWasMultiCaptured = false;
            if (DateTime.MinValue == created)
                created = DateTime.Now;
            if (String.IsNullOrEmpty(clipTitle))
                clipTitle = TextClipTitle(plainText);
            string hash;
            string sql = "SELECT Id, Title, Used, Favorite, Created FROM Clips Where Hash = @Hash";
            if (ClipAngel.Properties.Settings.Default.ReplaceDuplicates)
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                if (binaryBuffer != null)
                    md5.TransformBlock(binaryBuffer, 0, binaryBuffer.Length, binaryBuffer, 0);
                byte[] binaryText = Encoding.Unicode.GetBytes(plainText);
                md5.TransformBlock(binaryText, 0, binaryText.Length, binaryText, 0);
                if (ClipAngel.Properties.Settings.Default.UseFormattingInDuplicateDetection || String.IsNullOrEmpty(plainText))
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
                hash = Convert.ToBase64String(md5.Hash);
                SQLiteCommand commandSelect = new SQLiteCommand(sql, m_dbConnection);
                commandSelect.Parameters.AddWithValue("@Hash", hash);
                using (SQLiteDataReader reader = commandSelect.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        oldCurrentClipId = reader.GetInt32(reader.GetOrdinal("Id"));
                        if (true
                            && lastPastedClips.ContainsKey(oldCurrentClipId)
                            && DateDiffMilliseconds(lastPastedClips[oldCurrentClipId], dtNow) < 1000) // Protection from automated return copy after we send paste. For example Word does so for html paste.
                        {
                            return false;
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
            }
            else
            {
                Guid g = Guid.NewGuid();
                hash = Convert.ToBase64String(g.ToByteArray());
            }
            LastId = LastId + 1;
            lastClips.Add(new LastClip {Created = created, ID = LastId, ProcessID = 0});
            int lastClipsMaxSize = 5;
            while (lastClips.Count > lastClipsMaxSize)
            {
                lastClips.RemoveRange(0, lastClips.Count - lastClipsMaxSize);
            }

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

            //ClipsNumber++;
            //if (this.Visible)
            //{
            if (updateList)
                ReloadList(false, 0, oldCurrentClipId > 0, null, true);
            if (true
                && applicationText == "ScreenshotReader"
                && IsTextType(typeText)
            )
                ShowForPaste(false, true);
            //}
            lastCaptureMoment = DateTime.Now;
            return true;
        }

        private static void CalculateByteAndCharSizeOfClip(string htmlText, string richText, string plainText, ref int chars, ref int byteSize)
        {
            if (chars == 0)
                chars = plainText.Length;
            byteSize += plainText.Length * 2; // dirty
            byteSize += htmlText.Length * 2; // dirty
            byteSize += richText.Length * 2; // dirty
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
            if (ClipAngel.Properties.Settings.Default.HistoryDepthDays == 0)
                return;
            SQLiteCommand command = new SQLiteCommand(m_dbConnection);
            command.CommandText = "Delete From Clips where (NOT Favorite OR Favorite IS NULL) AND Created < date('now','-" + ClipAngel.Properties.Settings.Default.HistoryDepthDays + " day')";
            //commandInsert.Parameters.AddWithValue("Number", ClipAngel.Properties.Settings.Default.HistoryDepthDays);
            //command.Parameters.AddWithValue("CurDate", DateTime.Now);
            command.ExecuteNonQuery();
        }

        private void DeleteExcessClips()
        {
            if (ClipAngel.Properties.Settings.Default.HistoryDepthNumber == 0)
                return;
            SQLiteCommand command = new SQLiteCommand(m_dbConnection);
            int clipsCount = ClipsCount();
            int numberOfClipsToDelete = clipsCount - ClipAngel.Properties.Settings.Default.HistoryDepthNumber;
            if (numberOfClipsToDelete > 0)
            {
                command.CommandText = "Delete From Clips where (NOT Favorite OR Favorite IS NULL) AND Id IN (Select ID From Clips ORDER BY ID Limit @Number)";
                command.Parameters.AddWithValue("Number", numberOfClipsToDelete);
                command.ExecuteNonQuery();
            }
        }

        public int ClipsCount()
        {
            SQLiteCommand command = new SQLiteCommand(m_dbConnection);
            command.CommandText = "Select Count(*) From Clips";
            int clipsCount = 0;
            using (SQLiteDataReader reader = command.ExecuteReader())
            {
                reader.Read();
                clipsCount = unchecked((int)(long)reader[0]);
            }
            return clipsCount;
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
            if (title.Length > ClipTitleLength)
            {
                // Removing repeats (series of one char) of non digits and leave only 8 chars
                title = Regex.Replace(title, "([^\\d])(?<=\\1\\1\\1\\1\\1\\1\\1\\1\\1)", String.Empty,
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
                // Removing repeats (series of one char) of digits and leave only 20 chars
                title = Regex.Replace(title, "(\\d)(?<=\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1\\1)", String.Empty,
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            }
            if (title.Length > ClipTitleLength)
            {
                title = title.Substring(0, ClipTitleLength - 1 - 3) + "...";
            }
            return title;
        }

        private void Delete_Click(object sender = null, EventArgs e = null)
        {
            if (Properties.Settings.Default.ConfirmationBeforeDelete)
            {
                var confirmResult = MessageBox.Show(this, Properties.Resources.ConfirmDeleteSelectedClips, Properties.Resources.Confirmation, MessageBoxButtons.YesNo);
                if (confirmResult != DialogResult.Yes)
                    return;
            }
            allowRowLoad = false;
            //int i = dataGridView.CurrentRow.Index;
            string sql = "Delete from Clips where NOT Favorite AND Id IN(null";
            SQLiteCommand command = new SQLiteCommand("", m_dbConnection);
            int counter = 0;
            bool doFullReload = false;
            foreach (DataGridViewRow selectedRow in dataGridView.SelectedRows)
            {
                DataRowView dataRow = (DataRowView) selectedRow.DataBoundItem;
                if (dataRow["Favorite"] == DBNull.Value)
                {
                    doFullReload = true;
                    continue;
                }
                if ((bool)dataRow["Favorite"])
                    continue;
                string parameterName = "@Id" + counter;
                sql += "," + parameterName;
                command.Parameters.Add(parameterName, DbType.Int32).Value = dataRow["Id"];
                counter++;
                dataGridView.Rows.Remove(selectedRow);
                //ClipsNumber--;
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
            areDeletedClips = true;
            if (doFullReload)
                ReloadList(); // ViewWindow will move -bad
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
        static extern int GetWindowText(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder text, int count);

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
            if (dto == null)
                return "";
            //if (!ClipAngel.Properties.Settings.Default.MoveCopiedClipToTop)
            //    CaptureClipboard = false;
            SetClipboardDataObject(dto, allowSelfCapture);
            return clipText;
        }

        private DataObject ClipDataObject(SQLiteDataReader rowReader, bool onlySelectedPlainText, out string clipText)
        {
            clipText = "";
            if (rowReader == null)
                rowReader = LoadedClipRowReader;
            if (rowReader == null)
                return null;

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
            if (rowReader == LoadedClipRowReader)
            {
                string selectedText = GetSelectedTextOfClip(onlySelectedPlainText);
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
                //// This way many applications will prefer file instead of image format
                //StringCollection fileNameCollection = new StringCollection();
                //string fileEditor = "";
                //string fileName = GetClipTempFile(out fileEditor, rowReader);
                //fileNameCollection.Add(fileName);
                //dto.SetFileDropList(fileNameCollection);

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

        private string GetSelectedTextOfClip(bool onlySelectedPlainText = true)
        {
            string selectedText = "";
            mshtml.IHTMLTxtRange htmlSelection = null;
            if (LoadedClipRowReader["type"].ToString() == "html")
                htmlSelection = GetHtmlCurrentTextRangeOrAllDocument(true);
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
            StringCollection lastFilterValues = ClipAngel.Properties.Settings.Default.LastFilterValues;
            if (!String.IsNullOrEmpty(searchString) && !lastFilterValues.Contains(searchString))
            {
                lastFilterValues.Insert(0, searchString);
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
                type = (string) LoadedClipRowReader["type"];
            return type == "rtf" || type == "text" || type == "html";
        }

        // Does not respect MoveCopiedClipToTop
        private string SendPasteClipExpress(DataGridViewRow currentViewRow = null, PasteMethod pasteMethod = PasteMethod.Standard, bool pasteDelimiter = false, bool updateDB = false, string DelimiterForTextJoin = null)
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
                {
                    if (DelimiterForTextJoin == null)
                        DelimiterForTextJoin = ClipAngel.Properties.Settings.Default.DelimiterForTextJoin;
                    textToPaste = DelimiterForTextJoin.Replace("\\n", Environment.NewLine) + textToPaste;
                }
                return textToPaste;
            }
            if (pasteDelimiter && pasteMethod == PasteMethod.Standard)
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
            CopyClipToClipboard(rowReader, pasteMethod != PasteMethod.Standard && pasteMethod != PasteMethod.File, false);
            if (SendPaste(pasteMethod))
                return "";

            if (updateDB)
                SetRowMark("Used", true, false, true);
            //if (false
            //    || ClipAngel.Properties.Settings.Default.MoveCopiedClipToTop 
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

        // Return - bool - true if failed
        private bool SendPaste(PasteMethod pasteMethod = PasteMethod.Standard)
        {
            int targetProcessId;
            GetWindowThreadProcessId(lastActiveParentWindow, out targetProcessId);
            bool needElevation = targetProcessId != 0 && !UacHelper.IsProcessAccessible(targetProcessId);
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
            ActivateAndCheckTargetWindow();
            bool targetIsCurrentProcess = DoActiveWindowBelongsToCurrentProcess(IntPtr.Zero);
            if (targetIsCurrentProcess)
                return true;
            if (pasteMethod == PasteMethod.SendCharsFast || pasteMethod == PasteMethod.SendCharsSlow)
            {
                if (!IsTextType())
                    return true;
                if (!needElevation)
                    Paster.SendChars(this, pasteMethod == PasteMethod.SendCharsSlow);
                else
                {
                    EventWaitHandle sendCharsEvent = Paster.GetSendCharsEventWaiter(0, pasteMethod == PasteMethod.SendCharsSlow);
                    sendCharsEvent.Set();
                }
            }
            else
            {
                if (Properties.Settings.Default.DontSendPaste)
                    return false;
                if (!needElevation)
                {
                     Paster.SendPaste(this);
                }
                else
                {
                    EventWaitHandle pasteEvent = Paster.GetPasteEventWaiter();
                    pasteEvent.Set();
                }
                lastPasteMoment = DateTime.Now;
            }
            return false;
        }

        public bool ActivateAndCheckTargetWindow(bool activate = true)
        {
            bool isTargetActive = false;

            // not reliable method
            // Previous active window by z-order https://www.whitebyte.info/programming/how-to-get-main-window-handle-of-the-last-active-window

            if (activate)
            {
                if (!this.TopMost)
                {
                    this.Close();
                }
                else
                {
                    SetForegroundWindow(lastActiveParentWindow);
                    Debug.WriteLine("Set foreground window " + lastActiveParentWindow + " " + GetWindowTitle(lastActiveParentWindow));
                }

            }
            int waitStep = 5;
            IntPtr hForegroundWindow = IntPtr.Zero;
            for (int i = 0; i < 200; i += waitStep)
            {
                hForegroundWindow = GetForegroundWindow();
                if (hForegroundWindow != IntPtr.Zero)
                    break;
                Thread.Sleep(waitStep);
            }
            Debug.WriteLine("Get foreground window " + hForegroundWindow + " " + GetWindowTitle(hForegroundWindow));
            isTargetActive = hForegroundWindow == lastActiveParentWindow;

            //if (oldChildWindow != IntPtr.Zero && ClipAngel.Properties.Settings.Default.RestoreCaretPositionOnFocusReturn)
            //{
            //    Point point;
            //    RECT newRect;
            //    GUITHREADINFO guiInfo = new GUITHREADINFO();
            //    for (int i = 0; i < 500; i += waitStep)
            //    {
            //        guiInfo = GetGuiInfo(lastActiveParentWindow, out point);
            //        if (guiInfo.hwndFocus == oldChildWindow)
            //            break;
            //        Thread.Sleep(waitStep);
            //    }
            //    GetWindowRect(oldChildWindow, out newRect);
            //    if (newRect.Equals(oldChildWindowRect))
            //    {
            //        if (guiInfo.hwndFocus != oldChildWindow)
            //        {
            //            // Adress text box of IE11
            //            Paster.ClickOnPoint(oldChildWindow, oldCaretPoint);
            //        }
            //        else
            //        {
            //            //string newActiveWindowSelectedText = getActiveWindowSelectedText();
            //            //if (newActiveWindowSelectedText != oldWindowSelectedText && oldWindowSelectedText == "")
            //            //{
            //            Paster.ClickOnPoint(oldChildWindow, oldCaretPoint);
            //            //}
            //        }

            //        AttachThreadInput(GetCurrentThreadId(), remoteThreadId, true);
            //        Point PosBeforeChange;
            //        GetCaretPos(out PosBeforeChange);
            //        Point currentPos;
            //        int result = SetCaretPos(lastCaretPoint.X, lastCaretPoint.Y);
            //        int ErrorCode = Marshal.GetLastWin32Error(); // Always return 5 - Access denied
            //        for (int i = 0; i < 500; i += waitStep)
            //        {
            //            GetCaretPos(out currentPos);
            //            if (PosBeforeChange != currentPos)
            //                break;
            //            Thread.Sleep(waitStep);
            //        }
            //        AttachThreadInput(GetCurrentThreadId(), remoteThreadId, false);
            //    }
            //}
            return isTargetActive;
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
            MessageBox.Show(this, Properties.Resources.CantPasteInElevatedWindow, Application.ProductName);
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
            ReadSearchString();
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
                ReloadList(true);
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

        private void ReadSearchString()
        {
            searchString = comboBoxSearchString.Text;
        }

        private static Image GetImageFromBinary(byte[] binary)
        {
            MemoryStream memoryStream = new MemoryStream(binary, 0, binary.Length);
            memoryStream.Write(binary, 0, binary.Length);
            Bitmap image = new Bitmap(memoryStream);
            //image.MakeTransparent(); // It just makes all black color pixels become transparent
            return image;
        }

        private void FillFilterItems()
        {
            int filterSelectionLength = comboBoxSearchString.SelectionLength;
            int filterSelectionStart = comboBoxSearchString.SelectionStart;

            StringCollection lastFilterValues = ClipAngel.Properties.Settings.Default.LastFilterValues;
            comboBoxSearchString.Items.Clear();
            foreach (string String in lastFilterValues)
            {
                comboBoxSearchString.Items.Add(String);
            }

            // For some reason selection is reset. So we restore it
            comboBoxSearchString.SelectionStart = filterSelectionStart;
            comboBoxSearchString.SelectionLength = filterSelectionLength;
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
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpBaseName, int nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        // http://stackoverflow.com/questions/9501771/how-to-avoid-a-win32-exception-when-accessing-process-mainmodule-filename-in-c
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetProcessMainModuleFullName(int pid)
        {
            string result = null;
            //return result; // anti crash

            //Process p = Process.GetProcessById((int)pid);
            //string result = "";
            //try
            //{
            //    result = p.MainModule.FileName;
            //}
            //catch (Exception e)
            //{
            //}
            var processHandle = OpenProcess(0x0400 | 0x0010, false, pid);
            if (processHandle == IntPtr.Zero)
            {
                // Not enough priviledges. Need to call it elevated
                return null;
            }
            const int lengthSb = 1000; // Dirty
            var sb = new StringBuilder(lengthSb);
            // Possibly there is no such fuction in Windows 7 https://stackoverflow.com/a/321343/4085971
            if (GetModuleFileNameEx(processHandle, IntPtr.Zero, sb, sb.Capacity) > 0)
            {
                result = sb.ToString();
            }
            CloseHandle(processHandle);

            return result;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetOpenClipboardWindow();

        public class ClipboardOwner
        {
            public string windowTitle = "";
            public IntPtr windowHandle = IntPtr.Zero;
            public string application = "";
            public string appPath = "";
            public bool is1CCode = false;
            public IUIAutomationElement mainWindowAutomation = null;
            public int processId = 0;
            public bool isRemoteDesktop = false;
        }

        public ClipboardOwner GetClipboardOwnerLockerInfo(bool Locker, bool replaceNullWithLastActive = true, bool forceReadWindowTitles = false)
        {
            IntPtr hwnd;
            ClipboardOwner result = new ClipboardOwner();
            if (!ClipAngel.Properties.Settings.Default.ReadWindowTitles && !forceReadWindowTitles)
                return result;
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
                    return result;
                }
            }
            uint activeWindowThread = GetWindowThreadProcessId(hwnd, out result.processId);
            Process process1 = Process.GetProcessById(result.processId);
            try
            {
                result.application = process1.ProcessName;
                hwnd = process1.MainWindowHandle;
            }
            catch (Exception e)
            {
                return result;
            }
            result.appPath = GetProcessMainModuleFullName(result.processId);
            result.windowHandle = hwnd;
            result.windowTitle = GetWindowTitle(hwnd);
            if (true
                && hwnd != IntPtr.Zero 
                && (false
                    || String.Compare(result.application, "1cv8", true) == 0
                    || String.Compare(result.application, "1CV7", true) == 0
                    || String.Compare(result.application, "1CV7L", true) == 0
                    || String.Compare(result.application, "1CV7S", true) == 0))
            {
                //// This way 1C configurator can crash later
                //try
                //{
                //    var _automation = new CUIAutomation();
                //    IUIAutomationElement focusedControl = null;
                //    focusedControl = _automation.GetFocusedElement();
                //    is1CCode = true
                //               && focusedControl != null
                //               && focusedControl.CurrentLocalizedControlType == "документ";
                //}
                //catch
                //{ };
                result.is1CCode = true;
            }
            if (true
                && hwnd != IntPtr.Zero
                && (false
                    || String.Compare(result.application, "RDCMan", true) == 0
                    || String.Compare(result.application, "RDP", true) == 0))
            {
                result.isRemoteDesktop = true;
            }
            return result;
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
            string windowTitle = "";
            //return windowTitle; // anti crash

            if (ClipAngel.Properties.Settings.Default.ReadWindowTitles)
            {
                int nChars = Math.Max(1024, GetWindowTextLength(hwnd) + 1); // crash https://sourceforge.net/p/clip-angel/tickets/20/
                StringBuilder buff = new StringBuilder(nChars);
                if (GetWindowText(hwnd, buff, buff.Capacity) > 0)
                {
                    windowTitle = buff.ToString();
                }
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
            SendPasteOfSelectedTextOrSelectedClips(PasteMethod.Text);
        }

        private void SendPasteOfSelectedTextOrSelectedClips(PasteMethod pasteMethod = PasteMethod.Standard)
        {
            string agregateTextToPaste = "";
            string selectedText = "";
            PasteMethod itemPasteMethod;
            if (pasteMethod == PasteMethod.File)
            {
                DataObject dto = new DataObject();
                string clipText = SetClipFilesInDataObject(dto);
                SetTextInClipboardDataObject(dto, clipText);
                SetClipboardDataObject(dto, false);
                SendPaste();
            }
            else
            {
                if (pasteMethod == PasteMethod.Standard)
                    itemPasteMethod = pasteMethod;
                else
                    itemPasteMethod = PasteMethod.Null;
                agregateTextToPaste = GetSelectedTextOfClips(ref selectedText, itemPasteMethod);
                if (itemPasteMethod == PasteMethod.Null && !String.IsNullOrEmpty(agregateTextToPaste))
                {
                    if (pasteMethod == PasteMethod.Line)
                    {
                        agregateTextToPaste = ConvertTextToLine(agregateTextToPaste);
                    }
                    SetTextInClipboard(agregateTextToPaste, false);
                    SendPaste(pasteMethod);
                }
            }

            if (String.IsNullOrEmpty(selectedText))
            {
                SetRowMark("Used", true, true, true);
            }
            if (true
                && ClipAngel.Properties.Settings.Default.MoveCopiedClipToTop
                && String.IsNullOrEmpty(selectedText)
                )
            {
                MoveSelectedRows(0);
                //CaptureClipboardData();
            }
            else if (true
                     && pasteMethod == PasteMethod.Text
                     && !String.IsNullOrEmpty(selectedText))
            {
                // With multipaste works incorrect
                CaptureClipboardData();
                if (ClipAngel.Properties.Settings.Default.MoveCopiedClipToTop)
                    MoveSelectedRows(0);
            }
        }

        private static string ConvertTextToLine(string agregateTextToPaste)
        {
            agregateTextToPaste = agregateTextToPaste.Trim();
            agregateTextToPaste = Regex.Replace(agregateTextToPaste, "\\s+", " ");
            return agregateTextToPaste;
        }

        public string GetSelectedTextOfClips(ref string selectedText, PasteMethod itemPasteMethod = PasteMethod.Null, string DelimiterForTextJoin = null)
        {
            string agregateTextToPaste = "";
            int count;
            if (itemPasteMethod == PasteMethod.Null)
            {
                selectedText = GetSelectedTextOfClip();
                if (!String.IsNullOrEmpty(selectedText))
                    agregateTextToPaste = selectedText;
            }
            if (String.IsNullOrEmpty(agregateTextToPaste))
            {
                agregateTextToPaste = JoinOrPasteTextOfClips(itemPasteMethod, out count, DelimiterForTextJoin);
            }
            return agregateTextToPaste;
        }

        private string JoinOrPasteTextOfClips(PasteMethod itemPasteMethod, out int count, string DelimiterForTextJoin = null)
        {
            string agregateTextToPaste = "";
            bool pasteDelimiter = false;
            count = dataGridView.SelectedRows.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                DataGridViewRow selectedRow = dataGridView.SelectedRows[i];
                agregateTextToPaste += SendPasteClipExpress(selectedRow, itemPasteMethod, pasteDelimiter, false, DelimiterForTextJoin);
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
            if (dataGridView.Focused || comboBoxSearchString.Focused)
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
                    int firstIndex = Math.Min(selectedRangeStart, dataGridView.RowCount - 1);
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
            int byteSize = 0;
            int chars = 0;
            string newText = richTextBox.Text;
            CalculateByteAndCharSizeOfClip("", "", newText, ref chars, ref byteSize);
            string sql = "Update Clips set Title = @Title, Text = @Text, Size = @Size, Chars = @Chars where Id = @Id";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.Parameters.AddWithValue("@Id", LoadedClipRowReader["Id"]);
            command.Parameters.AddWithValue("@Text", newText);
            command.Parameters.AddWithValue("@Size", byteSize);
            command.Parameters.AddWithValue("@chars", chars);
            string newTitle = "";
            if (LoadedClipRowReader["Title"].ToString() == TextClipTitle(LoadedClipRowReader["Text"].ToString()))
                newTitle = TextClipTitle(newText);
            else
                newTitle = LoadedClipRowReader["Title"].ToString();
            command.Parameters.AddWithValue("@Title", newTitle);
            command.ExecuteNonQuery();

        }

        private void SaveClipUrl(string Url)
        {
            string sql = "Update Clips set Url = @Url where Id = @Id";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.Parameters.AddWithValue("@Id", LoadedClipRowReader["Id"]);
            command.Parameters.AddWithValue("@Url", Url);
            command.ExecuteNonQuery();
        }

        private void Main_Deactivate(object sender, EventArgs e)
        {
            if (ClipAngel.Properties.Settings.Default.FastWindowOpen)
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
                if (IsVisible())
                    this.Close();
                else
                    ShowForPaste(false, false, true);
            }
        }

        private bool IsVisible()
        {
            return this.Visible && this.Top > maxWindowCoordForHiddenState;
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

        private void ShowForPaste(bool onlyFavorites = false, bool clearFiltersAndGoToTop = false, bool safeOpen = false)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form.Modal && form.Visible && form.CanFocus)
                {
                    form.Activate();
                    return;
                }
            }

            //if (this.CanFocus) // With FastOpenWindow=False and TopMost=True is always false
            //{
            if (onlyFavorites)
                showOnlyFavoriteToolStripMenuItem_Click();
            //else if (MarkFilter.SelectedValue.ToString() == "favorite")
            //    showAllMarksToolStripMenuItem_Click();
            if (clearFiltersAndGoToTop)
                ClearFilter(-1, onlyFavorites);
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //this.SuspendLayout();
            int newX = -12345;
            int newY = -12345;
            if (this.Visible && this.ContainsFocus)
            {
                RestoreWindowIfMinimized(newX, newY, safeOpen);
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
                if (caretPoint.Y > 0 && ClipAngel.Properties.Settings.Default.RestoreCaretPositionOnFocusReturn)
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
                if (ClipAngel.Properties.Settings.Default.WindowAutoPosition)
                {
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
                }
            }
            //sw.Stop();
            //Debug.WriteLine("autoposition duration" + sw.ElapsedMilliseconds.ToString());
            RestoreWindowIfMinimized(newX, newY, safeOpen);
            if (!ClipAngel.Properties.Settings.Default.FastWindowOpen || safeOpen)
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

        private void RestoreWindowIfMinimized(int newX = -12345, int newY = -12345, bool safeOpen = false)
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
            if (!safeOpen && ClipAngel.Properties.Settings.Default.FastWindowOpen)
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
                if (ProcessEnterKeyDown(e.Control, e.Shift))
                    return;
                e.Handled = true;
            }
            if (e.KeyCode == Keys.Tab)
            {
                e.Handled = true;
                FocusClipText();
            }
            if (true
                && (DateTime.Now - TimeFromWindowOpen).TotalMilliseconds < 1000 // Temporary block main menu activation to avoid unwanted action while opening main window with ALT+* hotkey
                && (e.Modifiers == Keys.Alt) 
                && (e.KeyCode == Keys.Menu))
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private bool ProcessEnterKeyDown(bool isControlPressed, bool isShiftPressed)
        {
            PasteMethod pasteMethod;
            if (isControlPressed && !isShiftPressed)
                pasteMethod = PasteMethod.Text;
            else if (isControlPressed && isShiftPressed)
                pasteMethod = PasteMethod.Line;
            else
            {
                //if (!pasteENTERToolStripMenuItem.Enabled)
                if (richTextBox.Focused && EditMode)
                    return true;
                pasteMethod = PasteMethod.Standard;
            }
            SendPasteOfSelectedTextOrSelectedClips(pasteMethod);
            return false;
        }

        private void exitToolStripMenuItem_Click(object sender = null, EventArgs e = null)
        {
            Application.Exit();
        }

        private void SearchString_KeyDown(object sender, KeyEventArgs e)
        {
            PassKeyToGrid(true, e);
        }

        private void SearchString_KeyUp(object sender, KeyEventArgs e)
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
            if (LoadedClipRowReader != null)
            {
                int oldID = (int) LoadedClipRowReader["id"];
                if (!selectedClipsBeforeFilterApply.Contains(oldID))
                    selectedClipsBeforeFilterApply.Add(oldID);
            }
        }

        private bool CurrentIDChanged()
        {
            try
            {
                return false
                       || (LoadedClipRowReader == null && dataGridView.CurrentRow != null)
                       || (LoadedClipRowReader != null && dataGridView.CurrentRow == null)
                       || !(true
                            && LoadedClipRowReader != null
                            && dataGridView.CurrentRow != null
                            && dataGridView.CurrentRow.DataBoundItem != null
                            && (int)(dataGridView.CurrentRow.DataBoundItem as DataRowView)["ID"] == (int)LoadedClipRowReader["ID"]);
            }
            catch (Exception e)
            {
                // Cast exception
                return true;
            }
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
            else //if (richTextBox.Enabled)
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
            _richTextBox.Clear();
            _richTextBox.Font = dataGridView.RowsDefaultCellStyle.Font;
            _richTextBox.Text = row.Cells["ColumnTitle"].Value.ToString();
            if (!String.IsNullOrEmpty(textPattern))
            {
                MatchCollection tempMatches;
                MarkRegExpMatchesInRichTextBox(_richTextBox, textPattern, Color.Red, true, false, true, out tempMatches);
            }
            row.Cells["ColumnTitle"].Value = _richTextBox.Rtf;

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
            if (!String.IsNullOrEmpty(searchString))
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
            string result = searchString;
            string[] array;
            if (ClipAngel.Properties.Settings.Default.SearchWordsIndependently)
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
            if (ClipAngel.Properties.Settings.Default.SearchWildcards)
                result = result.Replace("%", ".*?");
            if (!String.IsNullOrWhiteSpace(result))
                result = "(" + result + ")";
            return result;
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
                    Icon smallIcon = IconTools.GetIconForFile(filePath, ShellIconSize.SmallIcon);
                    if (smallIcon != null)
                    {
                        originalImage = smallIcon.ToBitmap();
                        brightImage = (Bitmap) ChangeImageOpacity(originalImage, 0.6f);
                    }
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
            bool used;
            try
            {
                used = (bool)dataRowView.Row["Used"];
            }
            catch (Exception)
            {
                // Not yet fully read
                return;
            }
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
                if (dataRowView != null && LoadedClipRowReader != null)
                    dataRowView[fieldName] = LoadedClipRowReader[fieldName]; // DataBoundItem can be not read yet
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
            string oldDatabaseFile = ClipAngel.Properties.Settings.Default.DatabaseFile;
            bool oldEncryptDatabaseForCurrentUser = ClipAngel.Properties.Settings.Default.EncryptDatabaseForCurrentUser;
            settingsFormForm.ShowDialog(this);
            if (settingsFormForm.DialogResult == DialogResult.OK)
            {
                if (false
                    || oldDatabaseFile != ClipAngel.Properties.Settings.Default.DatabaseFile
                    || oldEncryptDatabaseForCurrentUser != ClipAngel.Properties.Settings.Default.EncryptDatabaseForCurrentUser)
                {
                    CloseDatabase();
                    OpenDatabase(true);
                }
                LoadSettings();
                keyboardHook.UnregisterHotKeys();
                RegisterHotKeys();
                AutodeleteClips();
                ClipAngel.Properties.Settings.Default.Save(); // Not all properties are saved here. For example ShowInTaskbar are not saved
            }
        }

        private void AutodeleteClips()
        {
            DeleteOldClips();
            DeleteExcessClips();
            ReloadList();
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
            UpdateNotifyIcon();

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
            searchAllFieldsMenuItem.ToolTipText = allSettings.GetProperties().Find("SearchAllFields", true).Description;
            toolStripButtonAutoSelectMatch.ToolTipText = allSettings.GetProperties().Find("AutoSelectMatch", true).Description;
            autoselectMatchedClipMenuItem.ToolTipText = allSettings.GetProperties().Find("AutoSelectMatchedClip", true).Description;
            filterListBySearchStringMenuItem.ToolTipText = allSettings.GetProperties().Find("FilterListBySearchString", true).Description;
            toolStripMenuItemSearchCaseSensitive.ToolTipText = allSettings.GetProperties().Find("SearchCaseSensitive", true).Description;
            ignoreBigTextsToolStripMenuItem.ToolTipText = allSettings.GetProperties().Find("SearchIgnoreBigTexts", true).Description;
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
            ClipAngel.Properties.Settings.Default.RestoreCaretPositionOnFocusReturn = false; // disabled
            dataGridView.RowsDefaultCellStyle.Font = ClipAngel.Properties.Settings.Default.Font;
            dataGridView.Columns["ColumnCreated"].DefaultCellStyle.Format = "HH:mmm:ss dd.MM";
            dataGridView.Columns["VisualWeight"].Width = (int)dataGridView.RowsDefaultCellStyle.Font.Size;
            dataGridView.RowTemplate.Height = (int)(dataGridView.RowsDefaultCellStyle.Font.Size + 11);

            UpdateColumnsSet();
            AfterRowLoad();
            this.ResumeLayout();
        }

        private void UpdateColumnsSet()
        {
            dataGridView.Columns["appImage"].Visible = ClipAngel.Properties.Settings.Default.ShowApplicationIconColumn;
            //dataGridView.Columns["VisualWeight"].Visible = ClipAngel.Properties.Settings.Default.ShowVisualWeightColumn;
            dataGridView.Columns["ColumnCreated"].Visible = ClipAngel.Properties.Settings.Default.ShowSecondaryColumns;
        }

        private void UpdateIgnoreModulesInClipCapture()
        {
            ignoreModulesInClipCapture = new StringCollection();
            if (ClipAngel.Properties.Settings.Default.IgnoreApplicationsClipCapture != null)
                foreach (var fullFilename in ClipAngel.Properties.Settings.Default.IgnoreApplicationsClipCapture)
                {
                    ignoreModulesInClipCapture.Add(Path.GetFileNameWithoutExtension(fullFilename).ToLower());
                }
        }

        public async void CheckUpdate(bool UserRequest = false)
        {
            if (!UserRequest && !ClipAngel.Properties.Settings.Default.AutoCheckForUpdate)
                return;
            buttonUpdate.Visible = false;
            toolStripUpdateToSeparator.Visible = false;
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                using (WebClient webClient = new WebClient())
                {
                    webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    string HtmlSource = await webClient.DownloadStringTaskAsync(Properties.Resources.Website);

                    // AngileSharp
                    var htmlParser = new HtmlParser();
                    var documentHtml = htmlParser.Parse(HtmlSource);
                    IHtmlCollection<IElement> Refs = documentHtml.GetElementsByClassName("button download big-text green ");
                    string lastVersion = ((AngleSharp.Dom.Html.IHtmlElement)(Refs[0])).Title;

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
                        buttonUpdate.Text = Properties.Resources.UpdateTo + " " + ActualVersion;
                        if (UserRequest)
                        {
                            MessageBox.Show(this, Properties.Resources.NewVersionAvailable, Application.ProductName);
                        }
                    }
                    else if (UserRequest)
                    {
                        MessageBox.Show(this, Properties.Resources.YouLatestVersion, Application.ProductName);
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
            //    MessageBox.Show(this, Properties.Resources.LangRestart, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (ClipAngel.Properties.Settings.Default.Language == "Default")
                locale = Application.CurrentCulture.TwoLetterISOLanguageName;
            else if (ClipAngel.Properties.Settings.Default.Language == "Chinese")
                locale = "zh-CN";
            else if (ClipAngel.Properties.Settings.Default.Language == "German")
                locale = "de";
            else if (ClipAngel.Properties.Settings.Default.Language == "Italian")
                locale = "it";
            else if (ClipAngel.Properties.Settings.Default.Language == "Polish")
                locale = "pl";
            else if (ClipAngel.Properties.Settings.Default.Language == "PortugueseBrazil")
                locale = "pt-BR";
            else if (ClipAngel.Properties.Settings.Default.Language == "Spain")
                locale = "es";
            else if (ClipAngel.Properties.Settings.Default.Language == "Hindi")
                locale = "hi";
            else if (ClipAngel.Properties.Settings.Default.Language == "French")
                locale = "fr";
            else if (ClipAngel.Properties.Settings.Default.Language == "Greek")
                locale = "el";
            else if (ClipAngel.Properties.Settings.Default.Language == "Russian")
                locale = "ru";
            else
                locale = "en";
            return locale;
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Visible = false;
            if (this.Left == -32000)
                ClipAngel.Properties.Settings.Default.WindowPositionX = this.RestoreBounds.Left;
            else
                ClipAngel.Properties.Settings.Default.WindowPositionX = this.Left;
            if (this.Top == -32000)
                ClipAngel.Properties.Settings.Default.WindowPositionY = this.RestoreBounds.Top;
            else if (this.Top == maxWindowCoordForHiddenState)
                ClipAngel.Properties.Settings.Default.WindowPositionY = factualTop;
            else
                ClipAngel.Properties.Settings.Default.WindowPositionY = this.Top;
            ClipAngel.Properties.Settings.Default.dataGridViewWidth = splitContainer1.SplitterDistance;

            //ClipAngel.Properties.Settings.Default.Save(); // Not all properties were saved here. For example ShowInTaskbar was not saved
            RemoveClipboardFormatListener(this.Handle);
            UnhookWinEvent(HookChangeActiveWindow);
            CloseDatabase();
        }

        private void CloseDatabase()
        {
            if (ClipAngel.Properties.Settings.Default.DeleteNonFavoriteClipsOnExit)
                deleteAllNonFavoriteClips();
            if (updateDBThread != null)
            {
                stopUpdateDBThread = true;
                updateDBThread.Join();
            }
            if (LoadedClipRowReader != null)
                LoadedClipRowReader = null;
            m_dbConnection.Close();

            if (areDeletedClips)
            {
                // Shrink database to really delete deleted clips. It can take up to several seconds. 
                m_dbConnection.Open();
                SQLiteCommand command = new SQLiteCommand("vacuum", m_dbConnection);
                command.ExecuteNonQuery();
                m_dbConnection.Close();
            }
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
            UpdateColumnsSet(); // 
            if (ClipAngel.Properties.Settings.Default.FastWindowOpen)
            {
                RestoreWindowIfMinimized();
            }
            SetForegroundWindow(this.Handle);
            TimeFromWindowOpen = DateTime.Now;
        }

        private void SearchString_KeyPress(object sender, KeyPressEventArgs e)
        {
            // http://csharpcoding.org/tag/keypress/ Workaroud strange beeping 
            if (e.KeyChar == (char) Keys.Enter || e.KeyChar == (char) Keys.Escape)
                e.Handled = true;
        }

        private void OpenLinkIfAltPressed(RichTextBox sender, EventArgs e, MatchCollection matches, bool checkAlt = true)
        {
            Keys mod = Control.ModifierKeys & Keys.Modifiers;
            bool altOnly = mod == Keys.Alt;
            if (!checkAlt || altOnly)
                OpenLinkFromTextBox(matches, sender.SelectionStart);
        }

        // Result - true if known link found
        private bool OpenLinkFromTextBox(MatchCollection matches, int SelectionStart, bool allowOpenUnknown = true)
        {
            foreach (Match match in matches)
            {
                if (match.Index <= SelectionStart && (match.Index + match.Length) >= SelectionStart)
                {
                    int startIndex1C = 4;
                    if (match.Groups[2].Success) // File link
                    {
                        string filePath = match.Value;
                        if (!File.Exists(filePath) && !Directory.Exists(filePath))
                        {
                            return true;
                        }
                        string argument = "/select, \"" + filePath + "\"";
                        System.Diagnostics.Process.Start("explorer.exe", argument);
                    }
                    else if (match.Groups[startIndex1C].Success) // 1C code link
                    {
                        ClipboardOwner clipboardOwner = GetClipboardOwnerLockerInfo(true, true, true);
                        if (String.Compare(clipboardOwner.application, "1cv8", true) == 0)
                        {
                            string extensionName = match.Groups[startIndex1C + 1].ToString();
                            string moduleName = match.Groups[startIndex1C + 2].ToString();
                            string[] fragments = moduleName.Split("."[0]);
                            if (fragments[0] == "ВнешняяОбработка" || fragments[0] == "ExternalDataProcessor")
                                return true;
                            string MDObjectName = "";
                            if (fragments.Length > 1)
                                MDObjectName = fragments[1];
                            string MDFormName = "";
                            if (fragments.Length > 3)
                                MDFormName = fragments[3];
                            int lineNumber = Convert.ToInt32(match.Groups[startIndex1C + 4].ToString());
                            ActivateAndCheckTargetWindow();
                            SendKeys.Send("%{F9}");
                            //SendKeys.Flush();
                            //Paster.ModifiersState mod = new Paster.ModifiersState();
                            //mod.ReleaseAll(true);
                            bool success = false;
                            object valuePattern = null;
                            int UIA_ValuePatternId = 10002;
                            string breakpointMarker = "OpenLink";
                            string tempFilename = Path.ChangeExtension(Path.GetTempFileName(), "xml");
                            IUIAutomationElement tableElement = null;
                            IUIAutomationElement breakPointsWindow;
                            IUIAutomationElement tempElement;
                            CUIAutomation _automation = new CUIAutomation();
                            IUIAutomationTreeWalker treeWalker = _automation.CreateTreeWalker(_automation.CreateTrueCondition());
                            bool Later8_3_24 = false;
                            success = WaitWindowFocus(null, "Точки останова", out breakPointsWindow, "V8NewLocalFrameBaseWnd", treeWalker, 1000, out Later8_3_24);
                            if (!success && Later8_3_24)
                            {
                                Paster.SendRestore();
                                success = WaitWindowFocus(null, "Точки останова", out breakPointsWindow, "V8NewLocalFrameBaseWnd", treeWalker, 1000, out Later8_3_24);
                            }
                            int maxWait = 2000;
                            Stopwatch stopWatch = new Stopwatch();
                            if (success)
                            {
                                success = false;
                                tableElement = FindTable1C(breakPointsWindow, treeWalker);
                                if (tableElement != null)
                                {
                                    SetFocusByClick(tableElement);
                                    Thread.Sleep(200);
                                    Paster.SendSave();
                                    success = WaitWindowFocus(breakPointsWindow, "Сохранить точки останова в файл", out tempElement, "#32770", treeWalker, 1000, out Later8_3_24);
                                }
                            }
                            else
                                success = success;
                            if (success)
                            {
                                success = false;
                                File.Delete(tempFilename);
                                valuePattern = _automation.GetFocusedElement().GetCurrentPattern(UIA_ValuePatternId);
                                ((IUIAutomationValuePattern) valuePattern).SetValue(tempFilename);
                                Paster.SendKeyPress(VirtualKeyCode.RETURN);
                                stopWatch.Restart();
                                while (stopWatch.ElapsedMilliseconds < maxWait)
                                {
                                    if (File.Exists(tempFilename))
                                    {
                                        success = true;
                                        break;
                                    }
                                    Thread.Sleep(50);
                                }
                            }
                            else
                                success = success;
                            //Logger.WriteLine("Automation-1");
                            if (success)
                            {
                                success = false;
                                // DataProcessor.StandardTotalsManagement.Form.MainForm
                                string xml = "";
                                string MDObject = "";
                                if (fragments.Length == 1)
                                {
                                    MDObject = "Configuration._";
                                }
                                else
                                {
                                    for (int counter = 0; counter < fragments.Length / 2; counter++)
                                    {
                                        string rusName = fragments[counter * 2];
                                        if (true
                                            && !typeMap1C.ContainsKey(rusName)
                                            && !typeMap1C.ContainsValue(rusName))
                                        {
                                            Activate();
                                            MessageBox.Show(this, "Unknown 1C object type " + rusName, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            return true;
                                        }
                                        string engName;
                                        if (typeMap1C.ContainsKey(rusName))
                                            engName = typeMap1C[rusName];
                                        else
                                            engName = rusName;
                                        if (!String.IsNullOrEmpty(MDObject))
                                            MDObject += ".";
                                        MDObject += engName;
                                        MDObject += "." + fragments[counter * 2 + 1];
                                    }
                                }
                                //if (!String.IsNullOrEmpty(extensionName))
                                //    MDObject = extensionName + MDObject;
                                string MDProperty = fragments[fragments.Length - 1];
                                if (!typeMap1C.ContainsValue(MDProperty))
                                    MDProperty = typeMap1C[MDProperty];
                                stopWatch.Start();
                                while (stopWatch.ElapsedMilliseconds < maxWait)
                                {
                                    try
                                    {
                                        xml = File.ReadAllText(tempFilename);
                                        break;
                                    }
                                    catch
                                    {
                                    }
                                    ;
                                    Thread.Sleep(50);
                                }
                                if (!String.IsNullOrEmpty(xml))
                                {

                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(xml);
                                    XmlElement moduleBPInfo = null;
                                    XmlElement root = (XmlElement) doc.ChildNodes[1];
                                    foreach (XmlElement moduleBPInfoCycle in root.ChildNodes)
                                    {
                                        XmlElement id = (XmlElement)moduleBPInfoCycle.GetElementsByTagName("id")[0];
                                        if (true
                                            && id.GetElementsByTagName("debugBaseData:MDObject")[0].InnerText == MDObject
                                            && id.GetElementsByTagName("debugBaseData:MDProperty")[0].InnerText == MDProperty)
                                        {
                                            moduleBPInfo = moduleBPInfoCycle;
                                            break;
                                        }
                                    }
                                    if (moduleBPInfo == null)
                                    {
                                        moduleBPInfo = doc.CreateElement("moduleBPInfo", root.NamespaceURI);
                                        doc.ChildNodes[1].AppendChild(moduleBPInfo);
                                        XmlElement id = doc.CreateElement("id", root.NamespaceURI);
                                        moduleBPInfo.AppendChild(id);
                                        string namespace2 = "http://v8.1c.ru/8.3/debugger/debugBaseData";
                                        XmlElement idType = doc.CreateElement("debugBaseData:type", namespace2);
                                        id.AppendChild(idType);
                                        if (!String.IsNullOrEmpty(extensionName))
                                        {
                                            idType.InnerText = "ExtensionModule";
                                            XmlElement idExtensionName = doc.CreateElement("debugBaseData:extensionName", namespace2);
                                            id.AppendChild(idExtensionName);
                                            idExtensionName.InnerText = extensionName.TrimEnd();
                                        }
                                        else
                                        {
                                            idType.InnerText = "ConfigModule";
                                        }
                                        XmlElement idMDObject = doc.CreateElement("debugBaseData:MDObject", namespace2);
                                        id.AppendChild(idMDObject);
                                        idMDObject.InnerText = MDObject;
                                        XmlElement idMDProperty = doc.CreateElement("debugBaseData:MDProperty", namespace2);
                                        id.AppendChild(idMDProperty);
                                        idMDProperty.InnerText = MDProperty;
                                    }
                                    XmlElement bpInfo = doc.CreateElement("bpInfo", root.NamespaceURI);
                                    moduleBPInfo.AppendChild(bpInfo);
                                    XmlElement line = doc.CreateElement("line", root.NamespaceURI);
                                    line.InnerText = lineNumber.ToString();
                                    bpInfo.AppendChild(line);
                                    breakpointMarker = breakpointMarker + line.InnerText;
                                    XmlElement condition = doc.CreateElement("condition", root.NamespaceURI);
                                    condition.InnerText = breakpointMarker;
                                    bpInfo.AppendChild(condition);
                                    if (Later8_3_24)
                                    {
                                        XmlElement active = doc.CreateElement("breakOnCondition", root.NamespaceURI);
                                        active.InnerText = "true";
                                        bpInfo.AppendChild(active);
                                    }

                                    doc.Save(tempFilename);
                                    success = true;
                                }
                            }
                            if (success)
                            {
                                success = false;
                                Paster.SendOpen();
                                success = WaitWindowFocus(breakPointsWindow, "Загрузить точки останова из файла", out tempElement, "#32770", treeWalker, 1000, out Later8_3_24);
                            }
                            else
                                success = success;
                            if (success)
                            {
                                success = false;
                                valuePattern = _automation.GetFocusedElement().GetCurrentPattern(UIA_ValuePatternId);
                                ((IUIAutomationValuePattern) valuePattern).SetValue(tempFilename);
                                Paster.SendKeyPress(VirtualKeyCode.RETURN);
                                success = WaitWindowFocus(null, "Точки останова", out breakPointsWindow, "V8NewLocalFrameBaseWnd", treeWalker, 1000, out Later8_3_24);
                            }
                            else
                                success = success;
                            if (success)
                            {
                                success = false;
                                //tableElement = FindTable1C(UIWindow, treeWalker);
                                stopWatch.Restart();
                                IUIAutomationElement cell, cell1;
                                string fullModuleName = moduleName;
                                if (!String.IsNullOrEmpty(extensionName))
                                    fullModuleName = extensionName + fullModuleName;
                                Paster.SendKeyPress(VirtualKeyCode.NEXT); // PageDOwn
                                while (stopWatch.ElapsedMilliseconds < maxWait)
                                {
                                    try
                                    {
                                        cell = treeWalker.GetFirstChildElement(tableElement);
                                    }
                                    catch
                                    {
                                        cell = null;
                                    }
                                    while (cell != null)
                                    {
                                        string cellText = cell.CurrentName;
                                        if (true
                                            && cellText.Contains(extensionName)
                                            && cellText.Contains("." + MDObjectName + ".")
                                            && (MDFormName.Length == 0 || cellText.Contains("." + MDFormName + "."))
                                            )
                                        {
                                            cell = treeWalker.GetNextSiblingElement(cell);
                                            if (cell == null)
                                            {
                                                break;
                                            }
                                            cell1 = treeWalker.GetNextSiblingElement(cell);
                                            if (cell1 == null)
                                            {
                                                break;
                                            }
                                            cellText = cell1.CurrentName;
                                            if (cellText.Contains(breakpointMarker))
                                            {
                                                success = true;
                                                break;
                                            }
                                        }
                                        cell = treeWalker.GetNextSiblingElement(cell);
                                    }
                                    if (success)
                                    {
                                        SetFocusByClick(cell);
                                        SetFocusByClick(cell);
                                        //Paster.SendKeyPress(VirtualKeyCode.RETURN);
                                        Paster.SendKeyPress(VirtualKeyCode.F9);
                                        break;
                                    }
                                    else
                                    {
                                        Paster.SendKeyPress(VirtualKeyCode.NEXT); // PageDown
                                    }
                                    Thread.Sleep(50);
                                }
                                if (!success)
                                    // Например в списке точек останова присутствует скроллбар и потому нужная ячейка была видимой области
                                    Paster.SendKeyPress(VirtualKeyCode.ESCAPE);
                                else if (Later8_3_24)
                                {
                                    SendKeys.Send("%{F9}");
                                    Paster.SendCloseChild();
                                }
                                    
                            }
                            File.Delete(tempFilename);
                        }
                    }
                    else // url
                    if (allowOpenUnknown)
                    {
                        try
                        {
                            Process.Start(match.Value);
                        }
                        catch
                        {
                            // for example file://C:/Users/Donny/AppData/Local/Temp/Clip.html
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        internal static class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true)]
            internal static extern uint SendInput(uint nInputs, ref NativeStructs.Input pInputs, int cbSize);
        }

        internal static class NativeStructs
        {
            [StructLayout(LayoutKind.Sequential)]
            internal struct Input
            {
                public NativeEnums.SendInputEventType type;
                public MouseInput mouseInput;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct MouseInput
            {
                public int dx;
                public int dy;
                public uint mouseData;
                public NativeEnums.MouseEventFlags dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }
        }

        internal static class NativeEnums
        {
            internal enum SendInputEventType : int
            {
                Mouse = 0,
                Keyboard = 1,
                Hardware = 2,
            }

            [Flags]
            internal enum MouseEventFlags : uint
            {
                Move = 0x0001,
                LeftDown = 0x0002,
                LeftUp = 0x0004,
                RightDown = 0x0008,
                RightUp = 0x0010,
                MiddleDown = 0x0020,
                MiddleUp = 0x0040,
                XDown = 0x0080,
                XUp = 0x0100,
                Wheel = 0x0800,
                Absolute = 0x8000,
            }
        }

        private static void SetFocusByClick(IUIAutomationElement tableElement)
        {
            //tableElement.SetFocus(); // exception - not implemented
            //IUIAutomationInvokePattern invokePattern = tableElement.GetCurrentPattern(InvokePattern.Pattern));
            //invokePattern.Invoke();
            UIAutomationClient.tagPOINT tagPoint;
            try
            {
                tableElement.GetClickablePoint(out tagPoint);
            }
            catch
            {
                return;
            }
            int x = tagPoint.x;
            int y = tagPoint.y;

            // This way not worked if user moves mouse
            //Mouse.MoveTo(new Point(tagPoint.x, tagPoint.y));
            //Mouse.Click(MouseButton.Left);

            // This way click is made always at current cursor position
            //const int MOUSEEVENTF_LEFTDOWN = 0x02;
            //const int MOUSEEVENTF_LEFTUP = 0x04;
            //const int MOUSEEVENTF_RIGHTDOWN = 0x08;
            //const int MOUSEEVENTF_RIGHTUP = 0x10;
            //mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint) tagPoint.x, (uint) tagPoint.y, 0, 0);

            // https://stackoverflow.com/questions/6554494/how-can-i-send-a-right-click-event-to-an-automationelement-using-wpfs-ui-automa
            NativeStructs.Input input = new NativeStructs.Input
            {
                type = NativeEnums.SendInputEventType.Mouse,
                mouseInput = new NativeStructs.MouseInput
                {
                    dx = 0,
                    dy = 0,
                    mouseData = 0,
                    dwFlags = NativeEnums.MouseEventFlags.Absolute | NativeEnums.MouseEventFlags.LeftDown | NativeEnums.MouseEventFlags.Move,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero,
                },
            };
            var primaryScreen = Screen.PrimaryScreen;
            input.mouseInput.dx = Convert.ToInt32((x - primaryScreen.Bounds.Left) * 65536 / primaryScreen.Bounds.Width);
            input.mouseInput.dy = Convert.ToInt32((y - primaryScreen.Bounds.Top) * 65536 / primaryScreen.Bounds.Height);
            NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
            input.mouseInput.dwFlags = NativeEnums.MouseEventFlags.Absolute | NativeEnums.MouseEventFlags.LeftUp | NativeEnums.MouseEventFlags.Move;
            NativeMethods.SendInput(1, ref input, Marshal.SizeOf(input));
            Thread.Sleep(50);
        }

        private static IUIAutomationElement FindTable1C(IUIAutomationElement child, IUIAutomationTreeWalker treeWalker)
        {
            IUIAutomationElement result = null;
            if (child != null)
            {
                child = treeWalker.GetFirstChildElement(child);
                while (child != null)
                {
                    if (child.CurrentLocalizedControlType == "таблицу")
                        result = child;
                    else
                        result = FindTable1C(child, treeWalker);
                    if (result != null)
                        break;
                    child = treeWalker.GetNextSiblingElement(child);
                }
            }
            return result;
        }

        private static bool IsRussianInputLanguage()
        {
            return String.Compare(InputLanguage.CurrentInputLanguage.Culture.TwoLetterISOLanguageName, "ru", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        private bool WaitWindowFocus(IUIAutomationElement mainWindow, string title, out IUIAutomationElement resultWindow, string className, IUIAutomationTreeWalker treeWalker,
            int wait, out bool Later8_3_24)
        {
            Later8_3_24 = false;
            resultWindow = null;
            bool success = false;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            //while (stopWatch.ElapsedMilliseconds < maxWait)
            //{
            //    IntPtr hwnd = GetFocusWindow();
            //    if (hwnd == IntPtr.Zero)
            //        continue;
            //    Debug.WriteLine(title + " VS " + GetWindowTitle(hwnd));
            //    if (GetWindowTitle(hwnd) == title)
            //    {
            //        success = true;
            //        break;
            //    }
            //    Thread.Sleep(50);
            //}

            CUIAutomation _automation = new CUIAutomation();
            //IUIAutomationCacheRequest request = _automation.CreateCacheRequest();
            int UIA_NamePropertyId = 30005;
            int UIA_ClassNamePropertyId = 30012;
            if (mainWindow == null)
            {
                bool rootResult;
                IUIAutomationElement rootWindow = _automation.GetRootElement();
                rootResult = WaitWindowFocus(rootWindow, title, out resultWindow, className, treeWalker, wait, out Later8_3_24);
                if (rootResult)
                    return true;
                IUIAutomationCondition cond = _automation.CreatePropertyCondition(UIA_ClassNamePropertyId, "V8TopLevelFrame");
                IUIAutomationElement parentWindow = _automation.GetFocusedElement();
                IUIAutomationElement topWindow = null;
                //topWindow = topWindow.FindFirst(TreeScope.TreeScope_Parent, cond); // not works
                while (parentWindow != null)
                {
                    if (parentWindow.GetCurrentPropertyValue(UIA_ClassNamePropertyId) == "V8TopLevelFrame")
                    {
                        topWindow = parentWindow;
                        break;
                    }
                    parentWindow = treeWalker.GetParentElement(parentWindow);
                }
                if (topWindow!=null)
                {
                    rootResult = WaitWindowFocus(topWindow, title, out resultWindow, className, treeWalker, wait, out Later8_3_24);
                    Later8_3_24 = true;
                    if (rootResult)
                        return true;
                }
                return false;
            }
            else
            {
                IUIAutomationElement parentWindow = treeWalker.GetParentElement(mainWindow);
                if (parentWindow!=null && parentWindow.GetCurrentPropertyValue(UIA_ClassNamePropertyId) == "V8TopLevelFrame")
                {
                    Later8_3_24 = true;
                    mainWindow = parentWindow;
                }
            }
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ff625922(v=vs.85).aspx#WalkAncestors
            if (treeWalker == null)
                treeWalker = _automation.CreateTreeWalker(_automation.CreateTrueCondition());
            while (stopWatch.ElapsedMilliseconds < wait)
            {
                try
                {
                    IUIAutomationCondition cond1 = _automation.CreatePropertyCondition(UIA_NamePropertyId, title);
                    IUIAutomationCondition cond2 = _automation.CreatePropertyCondition(UIA_ClassNamePropertyId, className);
                    IUIAutomationCondition cond = _automation.CreateAndCondition(cond1, cond2);
                    resultWindow = mainWindow.FindFirst(TreeScope.TreeScope_Children, cond);
                    if (resultWindow != null)
                    {
                        resultWindow.SetFocus();
                        //Stopwatch stopWatch2 = new Stopwatch();
                        //stopWatch2.Start();
                        //while (stopWatch2.ElapsedMilliseconds < wait)
                        //{
                        //    if (resultWindow.CurrentHasKeyboardFocus != 0)
                        //    {
                                success = true;
                                break;
                        //    }
                        //}
                        //break;
                    }
                }
                catch (Exception e)
                {
                    continue;
                }
                Thread.Sleep(50);
            }
            return success;
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
                ReloadList(true);
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

        private void SelectNextMatchInClipText(bool fromCurrentSelection = true)
        {
            if (htmlMode)
                SelectNextMatchInWebBrowser(1, fromCurrentSelection);
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
                        OnClipContentSelectionChange();
                        break;
                    }
                }
            }
        }

        private void SelectNextMatchInWebBrowser(int direction, bool fromCurrentSelection = true)
        {
            IHTMLDocument2 htmlDoc = (IHTMLDocument2) htmlTextBox.Document.DomDocument;
            IHTMLBodyElement body = htmlDoc.body as IHTMLBodyElement;
            IHTMLTxtRange currentRange = null;
            if (fromCurrentSelection)
                currentRange = GetHtmlCurrentTextRangeOrAllDocument();
            IHTMLTxtRange nearestMatch = null;
            int searchFlags = 0;
            if (ClipAngel.Properties.Settings.Default.SearchCaseSensitive)
                searchFlags = 4;
            string[] array;
            if (ClipAngel.Properties.Settings.Default.SearchWordsIndependently)
                array = searchString.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
            else
                array = new string[1] {searchString};
            foreach (var word in array)
            {
                IHTMLTxtRange range = body.createTextRange();
                if (currentRange != null)
                {
                    if (direction > 0)
                        range.setEndPoint("StartToEnd", currentRange);
                    else
                        range.setEndPoint("EndToStart", currentRange);
                }
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
            mshtml.IHTMLTxtRange range = null;
            try
            {
                range = (mshtml.IHTMLTxtRange)sel.createRange();
            }
            catch{}
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
            ClipAngel.Properties.Settings.Default.WordWrap = !ClipAngel.Properties.Settings.Default.WordWrap;
            allowTextPositionChangeUpdate = false;
            UpdateControlsStates();
            allowTextPositionChangeUpdate = true;
            if (LoadedClipRowReader!=null && LoadedClipRowReader["type"].ToString() == "html")
                AfterRowLoad();
            OnClipContentSelectionChange();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(Properties.Resources.HelpPage);
        }

        private void copyClipToolStripMenuItem_Click(object sender = null, EventArgs e = null)
        {
            CopyClipToClipboard(null, false, ClipAngel.Properties.Settings.Default.MoveCopiedClipToTop);
        }

        private void UpdateControlsStates()
        {
            searchAllFieldsMenuItem.Checked = ClipAngel.Properties.Settings.Default.SearchAllFields;
            filterListBySearchStringMenuItem.Checked = ClipAngel.Properties.Settings.Default.FilterListBySearchString;
            autoselectMatchedClipMenuItem.Checked = ClipAngel.Properties.Settings.Default.AutoSelectMatchedClip;
            toolStripMenuItemSecondaryColumns.Checked = ClipAngel.Properties.Settings.Default.ShowSecondaryColumns;
            toolStripButtonSecondaryColumns.Checked = ClipAngel.Properties.Settings.Default.ShowSecondaryColumns;
            toolStripMenuItemSearchCaseSensitive.Checked = ClipAngel.Properties.Settings.Default.SearchCaseSensitive;
            toolStripMenuItemSearchWordsIndependently.Checked = ClipAngel.Properties.Settings.Default.SearchWordsIndependently;
            toolStripMenuItemSearchWildcards.Checked = ClipAngel.Properties.Settings.Default.SearchWildcards;
            ignoreBigTextsToolStripMenuItem.Checked = ClipAngel.Properties.Settings.Default.SearchIgnoreBigTexts;
            moveCopiedClipToTopToolStripButton.Checked = ClipAngel.Properties.Settings.Default.MoveCopiedClipToTop;
            moveCopiedClipToTopToolStripMenuItem.Checked = ClipAngel.Properties.Settings.Default.MoveCopiedClipToTop;
            toolStripButtonAutoSelectMatch.Checked = ClipAngel.Properties.Settings.Default.AutoSelectMatch;
            trayMenuItemMonitoringClipboard.Checked = ClipAngel.Properties.Settings.Default.MonitoringClipboard;
            toolStripMenuItemMonitoringClipboard.Checked = ClipAngel.Properties.Settings.Default.MonitoringClipboard;
            toolStripButtonTextFormatting.Checked = ClipAngel.Properties.Settings.Default.ShowNativeTextFormatting;
            textFormattingToolStripMenuItem.Checked = ClipAngel.Properties.Settings.Default.ShowNativeTextFormatting;
            toolStripButtonMonospacedFont.Checked = ClipAngel.Properties.Settings.Default.MonospacedFont;
            monospacedFontToolStripMenuItem.Checked = ClipAngel.Properties.Settings.Default.MonospacedFont;
            wordWrapToolStripMenuItem.Checked = ClipAngel.Properties.Settings.Default.WordWrap;
            toolStripButtonWordWrap.Checked = ClipAngel.Properties.Settings.Default.WordWrap;
            richTextBox.WordWrap = wordWrapToolStripMenuItem.Checked;
            //showInTaskbarToolStripMenuItem.Enabled = ClipAngel.Properties.Settings.Default.FastWindowOpen;
            showInTaskbarToolStripMenuItem.Checked = ClipAngel.Properties.Settings.Default.ShowInTaskBar;
            //if (ClipAngel.Properties.Settings.Default.FastWindowOpen)
            //{
                this.ShowInTaskbar = ClipAngel.Properties.Settings.Default.ShowInTaskBar;
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
                string oldTitle = LoadedClipRowReader["Title"] as string;
                InputBoxResult inputResult = InputBox.Show(Properties.Resources.HowUseAutoTitle, Properties.Resources.EditClipTitle, oldTitle, this);
                if (inputResult.ReturnCode == DialogResult.OK)
                {
                    string sql = "Update Clips set Title=@Title where Id=@Id";
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    command.Parameters.AddWithValue("@Id", LoadedClipRowReader["Id"]);
                    string newTitle;
                    if (inputResult.Text == "")
                        newTitle = TextClipTitle(LoadedClipRowReader["text"].ToString());
                    else
                        newTitle = inputResult.Text;
                    command.Parameters.AddWithValue("@Title", newTitle);
                    command.ExecuteNonQuery();
                    ReloadList(true);
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
                ReloadList();
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
                comboBoxSearchString.Focus();
                sendKey(comboBoxSearchString.Handle, e.KeyData, false, true);
                e.Handled = true;
            }
            //else if (e.KeyCode == Keys.Tab)
            //{
            //    // Tired of trying to make it with TAB order
            //    richTextBox.Focus();
            //    e.Handled = true;
            //}
            else if (e.KeyCode == Keys.A && e.Control)
            {
                // Prevent CTRL+A from selection all clips
                e.Handled = true;
                for (int i = 0; i < Math.Min(dataGridView.RowCount, maxClipsToSelect); i++)
                {
                    dataGridView.Rows[i].Selected = true;
                }
            }
            else if (e.KeyCode == Keys.Insert && e.Control)
            {
                e.Handled = true;
                copyClipToolStripMenuItem_Click();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
                Delete_Click();
            }
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
            AutodeleteClips();
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
            IOrderedEnumerable<int> SortedRowIndexes;
            if (shiftType < 0)
                SortedRowIndexes = seletedRowIndexes.OrderBy(i=>i);
            else
                SortedRowIndexes = seletedRowIndexes.OrderByDescending(i=>i);
            foreach (int selectedRowIndex in SortedRowIndexes.ToList())
            {
                DataRow selectedDataRow = ((DataRowView)clipBindingSource[selectedRowIndex]).Row;
                int oldID = (int)selectedDataRow["ID"];
                if (shiftType != 0)
                {
                    if (selectedRowIndex + shiftType < 0 || selectedRowIndex + shiftType > clipBindingSource.Count - 1)
                        continue;
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
            ReloadList(false, newCurrentID, true, ids, true);
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
            Process.Start(Properties.Resources.HistoryOfChanges); // Returns 0. Why?
        }

        private void toolStripMenuItemPasteCharsFast_Click(object sender, EventArgs e)
        {
            SendPasteOfSelectedTextOrSelectedClips(PasteMethod.SendCharsFast);
        }

        private void toolStripMenuItemPasteCharsSlow_Click(object sender, EventArgs e)
        {
            SendPasteOfSelectedTextOrSelectedClips(PasteMethod.SendCharsSlow);
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
                rowReader = LoadedClipRowReader;
            if (rowReader == null)
                return "";
            string type = rowReader["type"].ToString();
            //string TempFile = Path.GetTempFileName();
            string tempFile = clipTempFile(rowReader);
            if (tempFile == "")
            {
                MessageBox.Show(this, Properties.Resources.ClipFileAlreadyOpened);
                return "";
            }
            if (type == "text" /*|| type == "file"*/)
            {
                File.WriteAllText(tempFile, rowReader["text"].ToString(), Encoding.UTF8);
                fileEditor = ClipAngel.Properties.Settings.Default.TextEditor;
            }
            else if (type == "rtf")
            {
                RichTextBox rtb = new RichTextBox();
                rtb.Rtf = rowReader["richText"].ToString();
                rtb.SaveFile(tempFile);
                fileEditor = ClipAngel.Properties.Settings.Default.RtfEditor;
            }
            else if (type == "html")
            {
                File.WriteAllText(tempFile, GetHtmlFromHtmlClipText(true), Encoding.UTF8);
                fileEditor = ClipAngel.Properties.Settings.Default.HtmlEditor;
            }
            else if (type == "img")
            {
                Image image = GetImageFromBinary((byte[]) rowReader["Binary"]);
                image.Save(tempFile);
                fileEditor = ClipAngel.Properties.Settings.Default.ImageEditor;
            }
            else if (type == "file")
            {
                var tokens = TextToLines(rowReader["text"].ToString());
                tempFile = tokens[0];
                if (!File.Exists(tempFile))
                    tempFile = "";
            }
            return tempFile;
        }

        private static string[] TextToLines(string Text)
        {
            string[] tokens = Regex.Split(Text, @"\r?\n|\r");
            return tokens;
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
            string tempFolder = ClipAngel.Properties.Settings.Default.ClipTempFileFolder;
            if (!Directory.Exists(tempFolder))
                tempFolder = Path.GetTempPath();
            if (!tempFolder.EndsWith("\\"))
                tempFolder += "\\";
            string tempFile = tempFolder + "Clip_" + rowReader["id"];
            if (!String.IsNullOrEmpty(suffix))
                tempFile += "_" + suffix;
            tempFile += "." + extension;
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
            if (LoadedClipRowReader == null)
                return;
            bool newEditMode = !EditMode;
            string clipType = LoadedClipRowReader["type"].ToString();
            if (!IsTextType())
                return;
            //selectionStart = richTextBox.SelectionStart;
            //selectionLength = richTextBox.SelectionLength;
            allowRowLoad = false;
            if (!newEditMode)
            {
                ReloadList();
            }
            else
            {
                if (clipType != "text")
                {
                    AddClip(null, null, "", "", "text", LoadedClipRowReader["text"].ToString(), "", "", "", 0, "",
                        (bool) LoadedClipRowReader["used"], (bool) LoadedClipRowReader["favorite"]);
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
            //        hoverCell.ToolTipText = Properties.Resources.VisualWeightTooltip; // No effect
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
                    hoverCell.ToolTipText = Properties.Resources.VisualWeightTooltip;
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

        private void timerApplySearchString_Tick(object sender = null, EventArgs e = null)
        {
            timerApplySearchString.Stop();
            SearchStringApply();
        }

        private void dataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (!dataGridView.Rows[e.RowIndex].Selected)
                {
                    //dataGridView.CurrentCell = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    //dataGridView.Rows[e.RowIndex].Selected = true;
                    DataRowView row1 = (DataRowView)dataGridView.Rows[e.RowIndex].DataBoundItem;
                    int newPosition = clipBindingSource.Find("Id", (int)row1["id"]);
                    clipBindingSource.Position = newPosition;
                    SelectCurrentRow();
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
            if (!allowTextPositionChangeUpdate||htmlMode)
                return;
            if (!EditMode && richTextBox.SelectionStart + richTextBox.SelectionLength > clipRichTextLength)
            {
                richTextBox.Select(richTextBox.SelectionStart, clipRichTextLength - richTextBox.SelectionStart);
                return;
            }
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
            UpdateClipContentPositionIndicator(line, column);
        }

        private void UpdateClipContentPositionIndicator(int line = 1, int column = 1)
        {
            string newText;
            if (LoadedClipRowReader!= null && LoadedClipRowReader["type"].ToString() == "img")
            {
                //double zoomFactor = Math.Min((double) ImageControl.ClientSize.Width / ImageControl.Image.Width, (double) ImageControl.ClientSize.Height / ImageControl.Image.Height);
                double zoomFactor = ImageControl.ZoomFactor();
                newText = Properties.Resources.Zoom + ": " + zoomFactor.ToString("0.00");
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
            UpdateClipContentPositionIndicator();
        }

        private void toolStripButtonFixedWidthFont_Click(object sender, EventArgs e)
        {
            ClipAngel.Properties.Settings.Default.MonospacedFont = !ClipAngel.Properties.Settings.Default.MonospacedFont;
            UpdateControlsStates();
            AfterRowLoad();
        }

        private void MonitoringClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SwitchMonitoringClipboard();
        }

        private void SwitchMonitoringClipboard(bool showToolTip = false)
        {
            ClipAngel.Properties.Settings.Default.MonitoringClipboard = !ClipAngel.Properties.Settings.Default.MonitoringClipboard;
            if (ClipAngel.Properties.Settings.Default.MonitoringClipboard)
                ConnectClipboard();
            else
                RemoveClipboardFormatListener(this.Handle);
            UpdateControlsStates();
            UpdateWindowTitle();
            UpdateNotifyIcon();
            if (showToolTip)
            {
                string text;
                if (ClipAngel.Properties.Settings.Default.MonitoringClipboard)
                    text = Properties.Resources.MonitoringON;
                else
                    text = Properties.Resources.MonitoringOFF;
                notifyIcon.ShowBalloonTip(2000, Application.ProductName, text, ToolTipIcon.Info);
            }
        }

        private void UpdateNotifyIcon()
        {
            Icon icon;
            if (!ClipAngel.Properties.Settings.Default.MonitoringClipboard)
            {
                icon = Properties.Resources.alienNoCapture;
            }
            else
            {
                icon = Properties.Resources.alien;
            }
            notifyIcon.Icon = icon;
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
                selectedText = LoadedClipRowReader["text"].ToString();
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
                type1 = LoadedClipRowReader["type"].ToString();
                if (!IsTextType() && type1 != "file")
                    return;
                if (_lastSelectedForCompareId == 0)
                {
                    _lastSelectedForCompareId = (int) LoadedClipRowReader["id"];
                    //MessageBox.Show("Now execute this command on the second clip to compare", Application.ProductName);
                    return;
                }
                else
                {
                    id1 = (int) LoadedClipRowReader["id"];
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
            CompareClipsByID(id2, id1);
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
            string preParam = "";
            if (comparatorName.EndsWith("code.exe", true, CultureInfo.CurrentCulture))
                preParam = "-d ";
            Process.Start(comparatorName, preParam + String.Format("\"{0}\" \"{1}\"", filename1, filename2));
        }

        string comparatorExeFileName()
        {
            // TODO read paths from registry and let use custom application
            string path;
            path = ClipAngel.Properties.Settings.Default.TextCompareApplication;
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

            MessageBox.Show(this, Properties.Resources.NoSupportedTextCompareApplication, Application.ProductName);
            Process.Start("http://winmerge.org/");
            return "";
        }

        private void toolStripButtonTextFormatting_Click(object sender, EventArgs e)
        {
            ClipAngel.Properties.Settings.Default.ShowNativeTextFormatting = !ClipAngel.Properties.Settings.Default.ShowNativeTextFormatting;
            UpdateControlsStates();
            if (LoadedClipRowReader != null)
            {
                string clipType = LoadedClipRowReader["type"].ToString();
                if (clipType == "html" || clipType == "rtf")
                    AfterRowLoad(true);
            }
        }

        private void toolStripButtonMarkFavorite_Click(object sender, EventArgs e)
        {
            if (LoadedClipRowReader == null)
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
                ProcessEnterKeyDown(e.CtrlKeyPressed, e.ShiftKeyPressed);
            }
        }

        private bool htmlTextBoxDocumentClick(mshtml.IHTMLEventObj e)
        {
            if (e.altKey)
            {
                IHTMLElement hlink = e.srcElement;
                openLinkInBrowserToolStripMenuItem_Click();
            }
            return false;
        }

        private void htmlTextBoxDocumentSelectionChange(Object sender = null, EventArgs e = null)
            //private void htmlTextBoxDocumentSelectionChange(mshtml.IHTMLEventObj e = null)
        {
            if (!allowTextPositionChangeUpdate)
                return;
            selectionStart = GetHtmlPosition(out selectionLength);
            UpdateClipContentPositionIndicator(0, 0);
        }

        private int GetHtmlPosition(out int length)
        {
            length = 0;
            int start = 0;
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
                return start;
            if (!String.IsNullOrEmpty(range.text))
                length = range.text.Length
                    //- GetNormalizedTextDeltaSize(range.text)
                    ;
            string innerText = htmlDoc.body.innerText;
            if (String.IsNullOrEmpty(innerText) || innerText.Length > 3000)
                // Long html will make moveStart slow
                return start;
            range.collapse();
            range.moveStart("character", -100000);
            if (!String.IsNullOrEmpty(range.text))
                start = range.text.Length 
                    //- GetNormalizedTextDeltaSize(range.text)
                    ;
            if (!String.IsNullOrEmpty(innerText))
            {
                int maxStart = innerText.Length 
                    //- GetNormalizedTextDeltaSize(innerText)
                    ;
                if (start > maxStart)
                    start = maxStart;
            }
            return start;
        }

        int GetNormalizedTextDeltaSize(string text)
        {
            // Found no reliable way to map html position <-> text position
            return 0;
            int result = (text.Length - text.Replace("\n", "").Length);
            return result;
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
            int textOffset = -1;
            if (href.StartsWith(link1Cprefix))
                try
                {
                    textOffset = Convert.ToInt32(href.Replace(link1Cprefix, ""));
                }
                catch
                {}
            if (textOffset >= 0 && OpenLinkFromTextBox(TextLinkMatches, textOffset, false))
            {
            }
            else
            {
                if (!String.IsNullOrEmpty(href))
                    Process.Start(href);
            }
        }

        private void showInTaskbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClipAngel.Properties.Settings.Default.ShowInTaskBar = !ClipAngel.Properties.Settings.Default.ShowInTaskBar;
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
            if (ClipAngel.Properties.Settings.Default.ShowNativeTextFormatting && richTextBox.SelectedRtf.Length > 0)
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
            OpenLinkFromTextBox(TextLinkMatches, richTextBox.SelectionStart);
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
            ClipAngel.Properties.Settings.Default.MoveCopiedClipToTop = !ClipAngel.Properties.Settings.Default.MoveCopiedClipToTop;
            UpdateControlsStates();
        }

        private void moveClipToTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClipAngel.Properties.Settings.Default.MoveCopiedClipToTop = !ClipAngel.Properties.Settings.Default.MoveCopiedClipToTop;
            UpdateControlsStates();
        }

        private void richTextBox_Enter(object sender, EventArgs e)
        {
            // It will corrupt protected memory on entering text field with cutted text.
            //Необработанное исключение типа "System.AccessViolationException" произошел в System.Windows.Forms.dll
            //    Дополнительная информация: Попытка чтения или записи защищенной памяти. Это часто свидетельствует о том, что другая память повреждена.
            //if (TextWasCut)
            //    AfterRowLoad(true);

            if (true
                && LoadedClipRowReader != null
                && LoadedClipRowReader["type"].ToString() == "file"
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
            if (LoadedClipRowReader == null)
                return;
            StringCollection IgnoreApplicationsClipCapture = ClipAngel.Properties.Settings.Default.IgnoreApplicationsClipCapture;
            string fullFilename = LoadedClipRowReader["AppPath"].ToString();
            if (String.IsNullOrEmpty(fullFilename))
                return;
            if (IgnoreApplicationsClipCapture.Contains(fullFilename))
                return;
            IgnoreApplicationsClipCapture.Add(fullFilename);
            UpdateIgnoreModulesInClipCapture();
            string moduleName = Path.GetFileName(fullFilename);
            MessageBox.Show(this,
                String.Format(Properties.Resources.ApplicationAddedToIgnoreList, moduleName), AssemblyProduct);
        }

        private void copyFullFilenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (LoadedClipRowReader == null)
                return;
            string fullFilename = LoadedClipRowReader["AppPath"].ToString();
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
            //bool success = false;
            try
            {
                Clipboard.SetDataObject(dto, true, 10, 20);
                    // Very important to set second parameter to TRUE to give immidiate access to buffer to other processes!
                //success = true;
            }
            catch
            {
                ClipboardOwner clipboardOwner = GetClipboardOwnerLockerInfo(true);
                Debug.WriteLine(String.Format(Properties.Resources.FailedToWriteClipboard, clipboardOwner.windowTitle, clipboardOwner.application));
            }
            //if (!success)
            //    try
            //    {
            //        Clipboard.SetDataObject(dto, false, 10, 20);
            //    }
            //    catch (Exception ex)
            //    {
            //        ClipboardOwner clipboardOwner = GetClipboardOwnerLockerInfo(true);
            //        Debug.WriteLine(String.Format(Properties.Resources.FailedToWriteClipboard, clipboardOwner.windowTitle, clipboardOwner.application));
            //    }
            ConnectClipboard();
            if (allowSelfCapture)
                CaptureClipboardData();
        }

        private void toolStripMenuItemCompareLastClips_Click(object sender = null, EventArgs e = null)
        {
            if (lastClipWasMultiCaptured)
                notifyIcon.ShowBalloonTip(2000, Application.ProductName, Properties.Resources.LastClipWasMultiCaptured, ToolTipIcon.Info);
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
                    CompareClipsByID(id2, id1);
            }
        }

        private void deleteAllNonFavoriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(this, Properties.Resources.ConfirmDeleteAllNonFavorite, Properties.Resources.Confirmation, MessageBoxButtons.OKCancel);
            if (result != DialogResult.OK)
                return;
            deleteAllNonFavoriteClips();
            ReloadList();
        }

        private void deleteAllNonFavoriteClips()
        {
            allowRowLoad = false;
            string sql = "Delete from Clips where NOT Favorite OR Favorite IS NULL";
            SQLiteCommand command = new SQLiteCommand("", m_dbConnection);
            command.CommandText = sql;
            command.ExecuteNonQuery();
            areDeletedClips = true;
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

        async private void uploadImageToWebToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string clipType = (string) LoadedClipRowReader["type"];
            if (clipType != "img")
                return;
            string junkVar;
            string tempFile = GetClipTempFile(out junkVar);
            if (String.IsNullOrEmpty(tempFile))
                return;
            string ImageUrl = "";
            try
            {
                //// Key expired
                //// base on http://cropperplugins.codeplex.com/SourceControl/latest#Cropper.Plugins/ImageShack/Plugin.cs
                //string _baseUri = "http://www.imageshack.us/";
                //string _developerKey = "234DNUWX2e44a0a56a245678963bcb127a1061ca"; //T39OZMFC7b60153bbc4341b959be614bc37f3278
                //string relativeUrl = "upload_api.php";
                //var http = new HttpClient();
                //http.BaseAddress = new Uri(_baseUri);
                //var form = new MultipartFormDataContent();
                //string mimetype = GetMimeType(tempFile);
                //HttpContent fileContent = new ByteArrayContent(File.ReadAllBytes(tempFile));
                //form.Add(fileContent, "fileupload", tempFile);
                //HttpContent keyContent = new StringContent(_developerKey);
                //form.Add(keyContent, "key");
                //HttpContent rembarContent = new StringContent("1");
                //form.Add(rembarContent, "rembar");
                //var response = http.PostAsync(relativeUrl, form);
                //XmlDocument doc = new XmlDocument();
                //string result = response.Result.Content.ReadAsStringAsync().Result;
                //doc.LoadXml(result);
                //XmlNamespaceManager namespaces = new XmlNamespaceManager(doc.NameTable);
                //namespaces.AddNamespace("ns", doc.DocumentElement.NamespaceURI);
                //XmlNode node = doc.DocumentElement.SelectSingleNode("/ns:imginfo/ns:links/ns:image_link", namespaces);
                //string ImageUrl = node.InnerText;

                //// based on // https://www.codeproject.com/Questions/1091120/How-to-upload-big-size-image-file-using-imgur-api
                //// http://imgur.com/
                //// ClientID - ac075d738068f2f - get it here https://apidocs.imgur.com/#intro search "Register your application"
                //string api_url_image = "https://api.imgur.com/3/image";
                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(api_url_image);
                ////request.Headers.Add("Authorization", "Client-ID ac075d738068f2f");    //authorized id - old API
                //request.Headers.Add("Authorization", string.Format("Bearer {0}", await getImgurAccessToken()));
                //request.Method = "POST";
                //FileStream file = new FileStream(tempFile, FileMode.Open);
                //byte[] image = new byte[file.Length];
                //file.Read(image, 0, (int)file.Length);
                //ASCIIEncoding enc = new ASCIIEncoding();
                //string postData = Convert.ToBase64String(image);
                //byte[] bytes = enc.GetBytes(postData);
                //request.ContentType = "application/x-www-form-urlencoded";
                //request.ContentLength = bytes.Length;
                //request.Timeout = 5000;
                //Stream writer = request.GetRequestStream();
                //writer.Write(bytes, 0, bytes.Length);
                //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                //response.GetResponseStream();
                //if (response.StatusCode == HttpStatusCode.OK)
                //{
                //    Stream responseStream = response.GetResponseStream();
                //    string responseStr = new StreamReader(responseStream).ReadToEnd();
                //    ImageUrl = responseStr.Remove(0, responseStr.IndexOf("link") + 7);
                //    ImageUrl = ImageUrl.Substring(0, ImageUrl.IndexOf("success") - 4);
                //    ImageUrl = ImageUrl.Replace("\\", "");
                //    responseStream.Close();
                //}
                //else
                //{
                //    throw new Exception("WebServer returned error " + response.StatusCode);
                //}
                //response.Close();

                string apiKey = SecretData.ImgBBKey(); // Получите API-ключ на сайте ImgBB
                string apiUrl = $"https://api.imgbb.com/1/upload?key={apiKey}";
                byte[] imageBytes = File.ReadAllBytes(tempFile);
                using (HttpClient client = new HttpClient())
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        var imageContent = new ByteArrayContent(imageBytes);
                        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                        formData.Add(imageContent, "image", "_");
                        HttpResponseMessage response = await client.PostAsync(apiUrl, formData);
                        string responseData = await response.Content.ReadAsStringAsync();
                        dynamic responseObject = JsonConvert.DeserializeObject(responseData);
                        ImageUrl = responseObject.data.url;
                    }
                }

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
            UpdateSelectedClipsHistory();
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
            ClipAngel.Properties.Settings.Default.AutoSelectMatch = !ClipAngel.Properties.Settings.Default.AutoSelectMatch;
            UpdateControlsStates();
        }

        private void caseSensetiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClipAngel.Properties.Settings.Default.SearchCaseSensitive = !ClipAngel.Properties.Settings.Default.SearchCaseSensitive;
            UpdateControlsStates();
            SearchStringApply();
        }

        private void everyWordIndependentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClipAngel.Properties.Settings.Default.SearchWordsIndependently = !ClipAngel.Properties.Settings.Default.SearchWordsIndependently;
            UpdateControlsStates();
            SearchStringApply();
        }

        private void meandsAnySequenceOfCharsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClipAngel.Properties.Settings.Default.SearchWildcards = !ClipAngel.Properties.Settings.Default.SearchWildcards;
            UpdateControlsStates();
            SearchStringApply();
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
                            if (agregateTextToPaste.Length < 1000) // MS Teams bug workaround https://github.com/tormozit/ClipAngel/issues/16
                            {
                                SetTextInClipboardDataObject(dto, agregateTextToPaste);
                            }
                            SetClipFilesInDataObject(dto);
                            dataGridView.DoDragDrop(dto, DragDropEffects.Copy);
                        }
                    }
                }
            }
        }

        private string SetClipFilesInDataObject(DataObject dto, int maxRowsDrag = 100)
        {
            string textList = "";
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
                    textList += filename;
                }
                else
                {
                    foreach (string filename in clipDto.GetFileDropList())
                    {
                        fileNameCollection.Add(filename);
                        textList += filename;
                    }
                }
                maxRowsDrag--;
                if (maxRowsDrag == 0)
                    break;
            }
            dto.SetFileDropList(fileNameCollection);
            return textList;
        }

        private void contextMenuUrlOpenLink_Click(object sender, EventArgs e)
        {
            OpenLinkIfAltPressed(urlTextBox, e, UrlLinkMatches, false);
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
            ReloadList();

            // Turn on secodnary columns
            if (!ClipAngel.Properties.Settings.Default.ShowSecondaryColumns)
                toolStripMenuItemSecondaryColumns_Click();
        }

        private void monthCalendar1_Leave(object sender, EventArgs e)
        {
            monthCalendar1.Hide();
        }

        private void ImageControl_Resize(object sender, EventArgs e)
        {
            UpdateClipContentPositionIndicator();
        }

        private void ImageControl_ZoomChanged(object sender, EventArgs e)
        {
            UpdateClipContentPositionIndicator();
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

        private void Main_Resize(object sender, EventArgs e)
        {
        }

        private void sortByDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sortField = "Clips.Id";
            ReloadList();
        }

        private void sortByVisualSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //sortField = "Chars"; // not working
            sortField = "Clips.Chars";
            ReloadList();
        }

        private void sortByCreationDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sortField = "Clips.Created";
            ReloadList();
        }

        private void toolStripMenuItem20_Click(object sender, EventArgs e)
        {
            sortField = "Clips.Size";
            ReloadList();
        }

        private void toolStripButtonSecondaryColumns_Click(object sender, EventArgs e)
        {
            ClipAngel.Properties.Settings.Default.ShowSecondaryColumns = !ClipAngel.Properties.Settings.Default.ShowSecondaryColumns;
            UpdateControlsStates();
            UpdateColumnsSet();
        }

        private void toolStripMenuItemSecondaryColumns_Click(object sender = null, EventArgs e = null)
        {
            ClipAngel.Properties.Settings.Default.ShowSecondaryColumns = !ClipAngel.Properties.Settings.Default.ShowSecondaryColumns;
            UpdateControlsStates();
            UpdateColumnsSet();
        }

        private void ignoreBigTextClipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClipAngel.Properties.Settings.Default.SearchIgnoreBigTexts = !ClipAngel.Properties.Settings.Default.SearchIgnoreBigTexts;
            UpdateControlsStates();
            SearchStringApply();
        }

        private void addSelectedTextInFilterToolStripMenu_Click(object sender, EventArgs e)
        {
            AllowFilterProcessing = false;
            if (!String.IsNullOrWhiteSpace(comboBoxSearchString.Text))
                comboBoxSearchString.Text += " ";
            comboBoxSearchString.Text += GetSelectedTextOfClip();
            AllowFilterProcessing = true;
            SearchStringApply();
        }

        private void setSelectedTextInFilterToolStripMenu_Click(object sender, EventArgs e)
        {
            AllowFilterProcessing = false;
            comboBoxSearchString.Text = richTextBox.SelectedText;
            AllowFilterProcessing = true;
            SearchStringApply();
        }

        private void openLastClipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowForPaste(false, true);
        }

        private void openWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowForPaste(false, false, true);
        }

        private void pasteLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendPasteOfSelectedTextOrSelectedClips(PasteMethod.Line);
        }

        private void pasteSpecialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteSpecial SpecialPasteForm = new PasteSpecial(this);
            DialogResult result = SpecialPasteForm.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                bool PasteIntoNewClip = SpecialPasteForm.PasteIntoNewClip;
                SetTextInClipboard(SpecialPasteForm.ResultText, PasteIntoNewClip);
                if (!PasteIntoNewClip)
                    SendPaste(PasteMethod.Text);
            }
        }

        private void htmlMenuItemCopy_Click(object sender, EventArgs e)
        {
            htmlTextBox.Document.ExecCommand("COPY", false, 0);
        }

        private void gotoTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GotoLastRow();
        }

        private void filterListBySearchStringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClipAngel.Properties.Settings.Default.FilterListBySearchString = !ClipAngel.Properties.Settings.Default.FilterListBySearchString;
            UpdateControlsStates();
            if (!ClipAngel.Properties.Settings.Default.FilterListBySearchString)
            {
                ReloadList();
                UpdateSearchMatchedIDs();
            }
            else
            {
                SearchStringApply();
            }
        }

        private void autoselectFirstMatchedClipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClipAngel.Properties.Settings.Default.AutoSelectMatchedClip = !ClipAngel.Properties.Settings.Default.AutoSelectMatchedClip;
            UpdateControlsStates();
        }

        private void NextMatchListMenuItem_Click(object sender, EventArgs e)
        {
            GotoSearchMatchInList();
        }

        private void PreviousMatchListMenuItem_Click(object sender, EventArgs e)
        {
            GotoSearchMatchInList(false);
        }

        private void pasteIntoSearchFieldMenuItem_Click_1(object sender, EventArgs e)
        {
            string selectedText = "";
            string agregateTextToPaste = GetSelectedTextOfClips(ref selectedText);
            agregateTextToPaste = ConvertTextToLine(agregateTextToPaste);
            comboBoxSearchString.Text = agregateTextToPaste.Substring(0, Math.Min(50, agregateTextToPaste.Length));
        }

        private void menuItemSetFocusClipText_Click(object sender, EventArgs e)
        {
            FocusClipText();
        }

        private void searchAllFieldsMenuItem_Click(object sender, EventArgs e)
        {
            ClipAngel.Properties.Settings.Default.SearchAllFields = !ClipAngel.Properties.Settings.Default.SearchAllFields;
            UpdateControlsStates();
            SearchStringApply();
        }

        private void exportMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.CheckFileExists = false;
            saveFileDialog.Filter = "ClipAngel clips|*.cac|All|*.*";
            if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
                return;
            string sql = "Select * from Clips where Id IN(null";
            int counter = 0;
            foreach (DataGridViewRow selectedRow in dataGridView.SelectedRows)
            {
                DataRowView dataRow = (DataRowView)selectedRow.DataBoundItem;
                string parameterName = "@Id" + counter;
                sql += "," + parameterName;
                globalDataAdapter.SelectCommand.Parameters.Add(parameterName, DbType.Int32).Value = dataRow["Id"];
                counter++;
                if (counter == 999) // SQLITE_MAX_VARIABLE_NUMBER, which defaults to 999, but can be lowered at runtime
                    break;
            }
            sql += ")";
            globalDataAdapter.SelectCommand.CommandText = sql;
            DataTable dataTable = new DataTable();
            dataTable.TableName = "ClipAngelClips";
            dataTable.Locale = CultureInfo.InvariantCulture;
            globalDataAdapter.Fill(dataTable);
            dataTable.WriteXml(saveFileDialog.FileName, XmlWriteMode.WriteSchema);
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = false;
            openFileDialog.Filter = "ClipAngel clips|*.cac|All|*.*";
            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                return;
            DataTable dataTable = new DataTable();
            dataTable.Locale = CultureInfo.InvariantCulture;
            try
            {
                dataTable.ReadXml(openFileDialog.FileName);
            }
            catch
            {
                MessageBox.Show(this, Properties.Resources.WrongFileFormat, Application.ProductName);
                return;
            }
            foreach (DataRow importedRow in dataTable.Rows)
            {
                var Binary = importedRow["Binary"];
                if (Binary == DBNull.Value)
                    Binary = new byte[0];
                var ImageSample = importedRow["ImageSample"];
                if (ImageSample == DBNull.Value)
                    ImageSample = new byte[0];
                AddClip((byte[])Binary, (byte[])ImageSample, importedRow["HtmlText"].ToString(), importedRow["RichText"].ToString(), importedRow["Type"].ToString(), importedRow["Text"].ToString(),
                    importedRow["application"].ToString(), importedRow["window"].ToString(), importedRow["url"].ToString(), Convert.ToInt32(importedRow["chars"].ToString()),
                    importedRow["AppPath"].ToString(), false, Convert.ToBoolean(importedRow["Favorite"].ToString()), false, importedRow["title"].ToString(), DateTime.Parse(importedRow["Created"].ToString()));
            }
            ReloadList(false, 0, false, null, true);
        }
        public static string TruncateLongString(string inputString, int maxChars, string postfix = "...")
        {
            if (maxChars <= 0)
                throw new ArgumentOutOfRangeException("maxChars");
            if (inputString == null || inputString.Length < maxChars)
                return inputString;
            var truncatedString = inputString.Substring(0, maxChars) + postfix;
            return truncatedString;
        }
        private void saveAsFileMenuItem_Click(object sender, EventArgs e)
        {
            string fileEditor = "";
            string tempFile = GetClipTempFile(out fileEditor);
            if (String.IsNullOrEmpty(tempFile))
                return;
            string extension = Path.GetExtension(tempFile);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            string fileName = RemoveInvalidCharsFromFileName(TruncateLongString(LoadedClipRowReader["title"].ToString(), 50, "") + " " + LoadedClipRowReader["created"]);
            saveFileDialog.FileName = fileName;
            saveFileDialog.CheckFileExists = false;
            saveFileDialog.Filter = extension  + "| *" + extension + "|All|*.*";
            if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
                return;
            File.Copy(tempFile, saveFileDialog.FileName);
        }

        private static string RemoveInvalidCharsFromFileName(string fileName, string replacement = "_")
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            fileName = r.Replace(fileName, replacement);
            return fileName;
        }

        private void tableLayoutPanelData_Paint(object sender, PaintEventArgs e)
        {

        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void decodeTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateSelectedClipsHistory();
            string selectedText = getSelectedOrAllText();
            string newSelectedText = HttpUtility.UrlDecode(selectedText);
            if (newSelectedText.Equals(selectedText))
                return;
            AddClip(null, null, "", "", "text", newSelectedText);
            GotoLastRow(true);
        }

        public async Task CreateSendChannel()
        {
            using (Aes myAes = Aes.Create())
            {
                myAes.KeySize = 256;
                myAes.GenerateKey();
                //myAes.GenerateIV();
                AESKey data = new AESKey
                {
                    key = Convert.ToBase64String(myAes.Key),
                    //IV = Convert.ToBase64String(myAes.IV)
                    IV = ""
                };
                string dataString = JsonConvert.SerializeObject(data);
                string filename = ChannelEncryptionKeyFileName();
                File.WriteAllText(filename, dataString);
                if (Properties.Settings.Default.EncryptDatabaseForCurrentUser)
                    File.Encrypt(filename);
            }
            HttpClient client = new HttpClient();
            await client.DeleteAsync(ChannelKeyUrl("recipients"));
            await client.DeleteAsync(ChannelKeyUrl("data"));
            await client.DeleteAsync(ChannelKeyUrl("dataDate"));
            await client.DeleteAsync(ChannelKeyUrl("dataTimestamp"));
            //await client.DeleteAsync(ChannelKeyUrl("senderName"));
            await setChannelKeyValue("senderName", Environment.MachineName);
        }

        private async Task<string> setChannelKeyValue(string key, string value)
        {
            string channelHost = SendChannelHost();
            HttpClient client = new HttpClient();
            HttpContent content = new StringContent("\"" + value + "\"");
            HttpResponseMessage response = await client.PutAsync(ChannelKeyUrl(key), content);
            response.EnsureSuccessStatusCode();
            return channelHost;
        }

       private async Task<string> delChannelKey(string key)
        {
            string channelHost = SendChannelHost();
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.DeleteAsync(ChannelKeyUrl(key));
            response.EnsureSuccessStatusCode();
            return channelHost;
        }

        private string ChannelKeyUrl(string key)
        {
            string URL = SendChannelHost() + CurrentSendChannel() + $"/{key}.json" + "?auth=" + SecretData.FirebaseDatabase();
            return URL;
        }

        private string ChannelEncryptionKeyFileName()
        {
            string result = UserSettingsPath + "\\Backup.db";
            return result;
        }

        public AESKey channelEncryptionKey()
        {
            string KeyFileName = ChannelEncryptionKeyFileName();
            if(!File.Exists(KeyFileName))
                CreateSendChannel();
            string json = File.ReadAllText(ChannelEncryptionKeyFileName());
            AESKey keyData = JsonConvert.DeserializeObject<AESKey>(json);
            keyData.IV = SecretData.CryptoIV();
            return keyData;
        }

        private void connectRecipientToolStripMenuItem_Click(object sender = null, EventArgs e = null)
        {
            ConnectRecipientForm form = new ConnectRecipientForm();
            form.ShowDialog(this);
        }

        public string CurrentSendChannel()
        {
            string sid = WindowsIdentity.GetCurrent().User.Value;
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] binaryText;
            binaryText = Encoding.Unicode.GetBytes(Environment.MachineName);
            md5.TransformBlock(binaryText, 0, binaryText.Length, binaryText, 0);
            binaryText = Encoding.Unicode.GetBytes(sid);
            md5.TransformFinalBlock(binaryText, 0, binaryText.Length);
            string currentSendChannel = Convert.ToBase64String(md5.Hash);
            if (currentSendChannel != Properties.Settings.Default.SendChannel)
            {
                Properties.Settings.Default.SendChannel = currentSendChannel;
                Properties.Settings.Default.Save();
                CreateSendChannel();
            }
            //currentSendChannel = "testChannel1"; // for testing
            return currentSendChannel.Replace("/","!");
        }

        //private static string FindMAC()
        //{
        //    string MAC = "0";
        //    NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        //    if (adapters != null && adapters.Length >= 0)
        //    {
        //        for (int stage = 1; stage <= 2; stage++)
        //        {
        //            foreach (NetworkInterface adapter in adapters)
        //            {
        //                if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet)
        //                    continue;
        //                MAC = stringMACFromAdapter(adapter);
        //                if (false
        //                    || stage == 2
        //                    || (true
        //                        && stage == 1
        //                        && MAC == Properties.Settings.Default.ChannelMAC))
        //                {
        //                    break;
        //                }
        //            }
        //            if (MAC == Properties.Settings.Default.ChannelMAC)
        //                break;
        //            else
        //            {    string dummy = ""; }
        //        }
        //    }
        //    return MAC;
        //}

        private static string stringMACFromAdapter(NetworkInterface adapter)
        {
            PhysicalAddress address = adapter.GetPhysicalAddress();
            byte[] bytes = address.GetAddressBytes();
            string result = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                result += bytes[i].ToString("X2");
            }
            return result;
        }

        public static string SendChannelHost()
        {
            string channelHost = "https://clipangel-495b4-default-rtdb.firebaseio.com/";
            return channelHost;
        }

        async private void sendClipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await SendClipToChannel();
        }

        public static long ToUnixTimestamp(DateTime d)
        {
            var epoch = d - new DateTime(1970, 1, 1, 0, 0, 0);

            return (long)epoch.TotalSeconds;
        }

        private async Task SendClipToChannel()
        {
            var recipients = await GetChannelRecipients();
            if (recipients == null)
            {
                connectRecipientToolStripMenuItem_Click();
                return;
            }
            int Dummy;
            string text = JoinOrPasteTextOfClips(PasteMethod.Null, out Dummy);
            if (text.Length > 10000)
            {
                MessageBox.Show(this, Properties.Resources.TooBigClipForChannel);
                return;
            }
            await setChannelKeyValue("dataDate", DateTime.Now.ToString());
            await setChannelKeyValue("dataTimestamp", ToUnixTimestamp(DateTime.Now).ToString());
            var key = channelEncryptionKey();
            Crypter crypter = new Crypter(key.key, key.IV);
            string value = crypter.EncryptAES(text);
            //string roundTrip = crypter.DecryptAES(value); // test
            await setChannelKeyValue("data", value);
            string urlFCM = "https://fcm.googleapis.com/v1/projects/clipangel-495b4/messages:send";
            foreach (var keyValue in recipients)
            {
                WebRequest tRequest = WebRequest.Create(urlFCM);
                string pushID = keyValue.Value;
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";
                tRequest.Headers.Add("Authorization", string.Format("Bearer {0}", await getServiceAccountAccessToken()));  
                var payload = new
                {
                    message = new
                    {
                        token = pushID,
                        data = new
                        {
                            channel = CurrentSendChannel()
                        }
                    }
                };
                string postbody = JsonConvert.SerializeObject(payload);
                Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
                tRequest.ContentLength = byteArray.Length;
                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    Properties.Settings.Default.ClipSendDate = DateTime.Now;
                    Properties.Settings.Default.Save();
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            if (dataStreamResponse != null)
                                using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    String sResponseFromServer = tReader.ReadToEnd();
                                    //result.Response = sResponseFromServer;
                                    var dumb = 0;
                                }
                        }
                    }
                }
            }
            CheckClearChannelData();
        }
        public static async Task<string> getServiceAccountAccessToken()
        {
            try
            {
                var credentials = GoogleCredential.FromJson(SecretData.FirebaseMessaging());
                var scopes = "https://www.googleapis.com/auth/firebase.messaging";
                credentials = credentials.CreateScoped(scopes);
                var accessToken = await credentials.UnderlyingCredential.GetAccessTokenForRequestAsync();
                return accessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return "";
            }
        }

        private void CheckClearChannelData()
        {
            if (Properties.Settings.Default.ClipSendDate != DateTime.MinValue)
            {
                bool restartTimer = false;
                if ((DateTime.Now - Properties.Settings.Default.ClipSendDate).TotalMinutes >= ChannelDataLifeTime)
                {
                    try
                    {
                        delChannelKey("data");
                        delChannelKey("dataDate");
                        delChannelKey("dataTimestamp");
                        Properties.Settings.Default.ClipSendDate = DateTime.MinValue;
                    }
                    catch (Exception e)
                    {
                        restartTimer = true;
                    }
                }
                else
                {
                    restartTimer = true;
                }
                if (restartTimer)
                {
                    channelTimer.Tick += delegate { CheckClearChannelData(); };
                    channelTimer.Interval = 60 * 60 * 1000;
                    channelTimer.Start();
                }
            }
        }

        public async Task<dynamic> GetChannelRecipients()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(ChannelKeyUrl("recipients"));
            response.EnsureSuccessStatusCode();
            String responseStream = await response.Content.ReadAsStringAsync();
            dynamic array = JsonConvert.DeserializeObject(responseStream);
            return array;
        }

        async private void sendClipMenuItem_Click(object sender, EventArgs e)
        {
            await SendClipToChannel();
        }

        private void fileToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView_DragOver(object sender, DragEventArgs e)
        {

        }

        private void duplicateTextClipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddClip(null, null , "", "", "text", "" + CurrentLangResourceManager.GetString("Duplicate") + " " + LoadedClipRowReader["Text"]);
            GotoLastRow();
        }
    }
    public static class Logger
    {
        private static string LogFile = Path.GetTempPath() + "\\ClipAngelLog.txt";
        public static void WriteLine(string txt)
        {
            File.AppendAllText(LogFile, DateTime.Now.ToString() + ": " + txt + "\n");
        }
        public static void DeleteLog()
        {
            File.Delete(LogFile);
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
