using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Resources;
using Microsoft.VisualBasic;
using static ClipAngel.dbDataSet;
using System.Net;
using AngleSharp.Parser.Html;
using AngleSharp.Dom;
using System.Timers;
using System.Reflection;

namespace ClipAngel
{
    public partial class Main : Form
    {
        public static ResourceManager CurrentResourceManager;
        public static string Locale = "";
        public bool PortableMode = false;
        public int ClipsNumber = 0;
        public string UserSettingsPath;
        public string DBFileName;
        public bool StartMinimized = false;
        SQLiteConnection m_dbConnection;
        string connectionString;
        SQLiteDataAdapter dataAdapter;
        bool CaptureClipboard = true;
        bool allowRowLoad = true;
        bool AutoGotoLastRow = true;
        bool AllowFormClose = false;
        bool AllowHotkeyProcess = true;
        SQLiteDataReader RowReader;
        static string LinkPattern = "\\b(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[A-Z0-9+&@#/%=~_|]";
        int LastId = 0;
        MatchCollection TextLinkMatches;
        MatchCollection UrlLinkMatches;
        MatchCollection FilterMatches;
        string DataFormat_ClipboardViewerIgnore = "Clipboard Viewer Ignore";
        string ActualVersion;
        //DateTime lastAutorunUpdateCheck;
        int MaxTextViewSize = 10000;
        bool TextWasCut;
        KeyboardHook hook = new KeyboardHook();

        public Main()
        {
            UpdateCurrentCulture(); // Antibug. Before bug it was not required
            InitializeComponent();
            // register the event that is fired after the key press.
            hook.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            RegisterHotKeys();
            timer1.Interval = (1000 * 60 * 60 * 24); // 1 day
            timer1.Start();
        }
        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
        private void RegisterHotKeys()
        {
            EnumModifierKeys Modifiers;
            Keys Key;
            if (ReadHotkeyFromText(Properties.Settings.Default.HotkeyShow, out Modifiers, out Key))
                hook.RegisterHotKey(Modifiers, Key);
            if (ReadHotkeyFromText(Properties.Settings.Default.HotkeyIncrementalPaste, out Modifiers, out Key))
                hook.RegisterHotKey(Modifiers, Key);
        }

        private static bool ReadHotkeyFromText(string HotkeyText, out EnumModifierKeys Modifiers, out Keys Key)
        {
            Modifiers = 0;
            Key = 0;
            if (HotkeyText == "" || HotkeyText == "No")
                return false;
            string[] Frags = HotkeyText.Split(new[] { " + " }, StringSplitOptions.None);
            for (int i = 0; i < Frags.Length - 1; i++)
            {
                EnumModifierKeys Modifier = 0;
                Enum.TryParse(Frags[i], out Modifier);
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
            if (hotkeyTitle == Properties.Settings.Default.HotkeyShow)
            {
                ShowForPaste();
                dataGridView.Focus();
            }
            else if (hotkeyTitle == Properties.Settings.Default.HotkeyIncrementalPaste)
            {
                AllowHotkeyProcess = false;
                SendPaste();
                clipBindingSource.MoveNext();
                DataRow CurrentDataRow = ((DataRowView)clipBindingSource.Current).Row;
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(2000, CurrentResourceManager.GetString("NextClip"), CurrentDataRow["Title"] as string, ToolTipIcon.Info);
                AllowHotkeyProcess = true;
            }
            else
            {
                //int a = 0;
            }
        }

        protected override void WndProc(ref Message m)
        {
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


        [DllImport("user32.dll")]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        private void Main_Load(object sender, EventArgs e)
        {
            TypeFilter.SelectedIndex = 0;
            MarkFilter.SelectedIndex = 0;
            CheckUpdate();
            LoadSettings();
            if (Properties.Settings.Default.LastFilterValues == null)
            {
                Properties.Settings.Default.LastFilterValues = new StringCollection();
            }
            FillFilterItems();
            if (!Directory.Exists(UserSettingsPath))
            {
                Directory.CreateDirectory(UserSettingsPath);
            }
            DBFileName = UserSettingsPath + "\\" + Properties.Resources.DBShortFilename;
            connectionString = "data source=" + DBFileName + ";";
            string Reputation = "Magic67234784";
            if (!File.Exists(DBFileName))
            {
                File.WriteAllBytes(DBFileName, Properties.Resources.dbTemplate);
                m_dbConnection = new SQLiteConnection(connectionString);
                m_dbConnection.Open();
                // Encryption http://stackoverflow.com/questions/12190672/can-i-password-encrypt-sqlite-database
                m_dbConnection.ChangePassword(Reputation);
                m_dbConnection.Close();
            }
            connectionString += "Password = " + Reputation + ";";
            m_dbConnection = new SQLiteConnection(connectionString);
            m_dbConnection.Open();
            SQLiteCommand command;

            command = new SQLiteCommand("ALTER TABLE Clips" + " ADD COLUMN Hash CHAR(32)", m_dbConnection);
            try {
                command.ExecuteNonQuery();
            }
            catch {
            }
            command = new SQLiteCommand("ALTER TABLE Clips" + " ADD COLUMN Favorite BOOLEAN", m_dbConnection);
            try {
                command.ExecuteNonQuery();
            }
            catch {
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
            dataAdapter = new SQLiteDataAdapter("", connectionString);
            //dataGridView.DataSource = clipBindingSource;

            UpdateClipBindingSource();
            AddClipboardFormatListener(this.Handle);
            if (StartMinimized)
            {
                StartMinimized = false;
                this.ShowInTaskbar = false;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void AfterRowLoad(int CurrentRowIndex = -1, bool FullTextLoad = false)
        {
            DataRowView CurrentRowView;
            string ClipType;
            if (CurrentRowIndex == -1)
            { CurrentRowView = clipBindingSource.Current as DataRowView; }
            else
            { CurrentRowView = clipBindingSource[CurrentRowIndex] as DataRowView; }
            if (CurrentRowView == null)
            {
                ClipType = "";
                richTextBox.Text = "";
                textBoxApplication.Text = "";
                textBoxWindow.Text = "";
                StripLabelCreated.Text = "";
                StripLabelSize.Text = "";
                StripLabelVisibleSize.Text = "";
                StripLabelType.Text = "";
                StripLabelPosition.Text = "";
            }
            else
            {
                DataRow CurrentRow = CurrentRowView.Row;
                string sql = "SELECT * FROM Clips Where Id = @Id";
                SQLiteCommand commandSelect = new SQLiteCommand(sql, m_dbConnection);
                commandSelect.Parameters.Add("@Id", DbType.Int32).Value = CurrentRow["Id"];
                RowReader = commandSelect.ExecuteReader();
                RowReader.Read();
                StripLabelCreated.Text = RowReader["Created"].ToString();
                StripLabelSize.Text = RowReader["Size"] + MultiLangByteUnit();
                StripLabelVisibleSize.Text = RowReader["Chars"]+ MultiLangCharUnit();
                string TypeEng = RowReader["Type"].ToString();
                if (CurrentResourceManager.GetString(TypeEng) == null)
                    StripLabelType.Text = TypeEng;
                else
                    StripLabelType.Text = CurrentResourceManager.GetString(TypeEng);
                StripLabelPosition.Text = "1";

                // to prevent autoscrolling during marking
                richTextBox.HideSelection = true;
                richTextBox.Clear();
                string FullText = RowReader["Text"].ToString();
                string ShortText;
                string EndMarker;
                Font MarkerFont;
                Color MarkerColor;
                if (!FullTextLoad && MaxTextViewSize < FullText.Length)
                {
                    ShortText = FullText.Substring(1, MaxTextViewSize);
                    EndMarker = MultiLangCutMarker();
                    MarkerFont = new Font(richTextBox.SelectionFont, FontStyle.Underline);
                    TextWasCut = true;
                    MarkerColor = Color.Blue;
                }
                else
                {
                    ShortText = FullText;
                    EndMarker = MultiLangEndMarker();
                    MarkerFont = richTextBox.SelectionFont;
                    TextWasCut = false;
                    MarkerColor = Color.Green;
                }
                richTextBox.Text = ShortText;
                richTextBox.SelectionStart = richTextBox.TextLength;
                richTextBox.SelectionColor = MarkerColor;
                richTextBox.SelectionFont = MarkerFont;
                richTextBox.AppendText(EndMarker); // Do it first, else ending hyperlink will connect underline to it
                MarkLinksInRichTextBox(richTextBox, out TextLinkMatches);
                if (comboBoxFilter.Text.Length > 0)
                {
                    MarkRegExpMatchesInRichTextBox(richTextBox, Regex.Escape(comboBoxFilter.Text).Replace("%", ".*?"), Color.Red, false, out FilterMatches);
                }
                else
                    FilterMatches = null;
                richTextBox.SelectionColor = new Color();
                richTextBox.SelectionStart = 0;
                richTextBox.HideSelection = false;

                textBoxUrl.Clear();
                textBoxUrl.Text = RowReader["Url"].ToString();
                MarkLinksInRichTextBox(textBoxUrl, out UrlLinkMatches);

                ClipType = RowReader["type"].ToString();
                if (ClipType == "img")
                {
                    Image image = GetImageFromBinary((byte[])RowReader["Binary"]);
                    ImageControl.Image = image;
                }
            }
            if (ClipType == "img")
            {
                tableLayoutPanelData.RowStyles[0].Height = 20;
                tableLayoutPanelData.RowStyles[1].Height = 80;
            }
            else
            {
                tableLayoutPanelData.RowStyles[0].Height = 100;
                tableLayoutPanelData.RowStyles[1].Height = 0;
            }
            if (textBoxUrl.Text == "")
                tableLayoutPanelData.RowStyles[2].Height = 0;
            else
                tableLayoutPanelData.RowStyles[2].Height = 25;
        }

        private static string MultiLangEndMarker()
        {
            return "<" + CurrentResourceManager.GetString("EndMarker") + ">";
        }

        private static string MultiLangCutMarker()
        {
            return "<" + CurrentResourceManager.GetString("CutMarker") + ">";
        }

        private static string MultiLangCharUnit()
        {
            return CurrentResourceManager.GetString("CharUnit");
        }

        private static string MultiLangByteUnit()
        {
            return CurrentResourceManager.GetString("ByteUnit");
        }

        private void MarkLinksInRichTextBox(RichTextBox Control, out MatchCollection Matches)
        {
            MarkRegExpMatchesInRichTextBox(Control, LinkPattern, Color.Blue, true, out Matches);
        }

        private void MarkRegExpMatchesInRichTextBox(RichTextBox Control, string Pattern, Color Color, bool Underline, out MatchCollection Matches)
        {
            Matches = Regex.Matches(Control.Text, Pattern, RegexOptions.IgnoreCase);
            Control.DeselectAll();
            int MaxMarked = 100; // prevent slow down
            foreach (Match Match in Matches)
            {
                Control.SelectionStart = Match.Index;
                Control.SelectionLength = Match.Length;
                Control.SelectionColor = Color;
                if (Underline)
                    Control.SelectionFont = new Font(Control.SelectionFont, FontStyle.Underline);
                MaxMarked--;
                if (MaxMarked < 0)
                    break;
            }
            Control.DeselectAll();
            Control.SelectionColor = new Color();
            Control.SelectionFont = new Font(Control.SelectionFont, FontStyle.Regular);
        }

        private void RichText_Click(object sender, EventArgs e)
        {
            string NewText;
            NewText = "" + richTextBox.SelectionStart;
            if (richTextBox.SelectionLength > 0)
            {
                //NewText += "+" + Text.SelectionLength + "=" + (Text.SelectionStart + Text.SelectionLength);
                NewText += "+" + richTextBox.SelectionLength;
            }
            StripLabelPosition.Text = NewText;

            //NewText = "" + Text.Cursor;
            //StripLabelPositionXY.Text = NewText;
            OpenLinkIfCtrlPressed(sender as RichTextBox, e, TextLinkMatches);
            if (MaxTextViewSize >= (sender as RichTextBox).SelectionStart && TextWasCut)
                AfterRowLoad(-1, true);
        }

        private void Filter_TextChanged(object sender, EventArgs e)
        {
            ChooseTitleColumnDraw();
            UpdateClipBindingSource(true);
        }

        private void UpdateClipBindingSource(bool forceRowLoad = false)
        {
            if (dataAdapter == null)
                return;
            int CurrentClipID = 0;
            if (clipBindingSource.Current != null)
                CurrentClipID = (int)(clipBindingSource.Current as DataRowView).Row["Id"];
            allowRowLoad = false;
            string SqlFilter = "1 = 1";
            if (comboBoxFilter.Text != "")
            {
                SqlFilter += " AND UPPER(Text) Like UPPER('%" + comboBoxFilter.Text + "%')";
            }
            if (TypeFilter.SelectedValue as string != "allTypes")
            {
                string FilterValue = TypeFilter.SelectedValue as string;
                if (FilterValue == "text")
                    FilterValue = "'html','rtf','text'";
                else
                    FilterValue = "'" + FilterValue + "'";
                SqlFilter += " AND type IN (" + FilterValue + ")";
            }
            if (MarkFilter.SelectedValue as string != "allMarks")
            {
                string FilterValue = MarkFilter.SelectedValue as string;
                SqlFilter += " AND " + FilterValue;
            }
            string SelectCommandText = "Select Id, Used, Title, Chars, Type, Favorite From Clips";
            SelectCommandText += " WHERE " + SqlFilter;
            SelectCommandText += " ORDER BY Id desc";
            dataAdapter.SelectCommand.CommandText = SelectCommandText;

            DataTable table = new DataTable();
            table.Locale = CultureInfo.InvariantCulture;
            //
            //DataTable table = dbDataSet.Clips;
            //table.Clear();

            dataAdapter.Fill(table); // Long
            clipBindingSource.DataSource = table;

            PrepareTableGrid(); // Long

            if (LastId == 0)
            {
                GotoLastRow();
                ClipsNumber = clipBindingSource.Count;
                DataRowView LastRow = (DataRowView)clipBindingSource.Current;
                if (LastRow == null)
                {
                    LastId = 0;
                }
                else
                {
                    LastId = (int)LastRow["Id"];
                }
            }
            else if (AutoGotoLastRow || CurrentClipID == 0)
                GotoLastRow();
            else if (CurrentClipID != 0)
            {
                clipBindingSource.Position = clipBindingSource.Find("id", CurrentClipID);
                //if (dataGridView.CurrentRow != null)
                //    dataGridView.CurrentCell = dataGridView.CurrentRow.Cells[0];
                SelectCurrentRow(forceRowLoad);
            }
            allowRowLoad = true;
            AutoGotoLastRow = false;
        }

        private void ClearFilter_Click(object sender = null, EventArgs e = null)
        {
            comboBoxFilter.Text = "";
            TypeFilter.SelectedIndex = 0;
            MarkFilter.SelectedIndex = 0;
            dataGridView.Focus();
            UpdateClipBindingSource(true);
        }

        private void dataGridView_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            //UpdateStatusStrip(e.RowIndex);
        }

        private void Text_CursorChanged(object sender, EventArgs e)
        {
            // This event not working. Why? Decided to use Click instead.
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!AllowFormClose)
            {
                Hide();
                e.Cancel = true;
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
            if (!CaptureClipboard)
            { return; }
            IDataObject iData = new DataObject();
            string Type = "";
            string Text = "";
            string Window = "";
            string Application = "";
            string RichText = "";
            string HtmlText = "";
            string Url = "";
            int Chars = 0;
            GetClipboardOwnerInfo(out Window, out Application);
            try
            {
                iData = Clipboard.GetDataObject();
            }
            catch (ExternalException externEx)
            {
                // Copying a field definition in Access 2002 causes this sometimes?
                Debug.WriteLine("InteropServices.ExternalException: {0}", externEx.Message);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            if (iData.GetDataPresent(DataFormat_ClipboardViewerIgnore))
                return;
            if (iData.GetDataPresent(DataFormats.UnicodeText))
            {
                Text = (string)iData.GetData(DataFormats.UnicodeText);
                Type = "text";
                //Debug.WriteLine(Text);
            }
            else if (iData.GetDataPresent(DataFormats.Text))
            {
                Text = (string)iData.GetData(DataFormats.Text);
                Type = "text";
                //Debug.WriteLine(Text);
            }

            if (iData.GetDataPresent(DataFormats.Rtf))
            {
                RichText = (string)iData.GetData(DataFormats.Rtf);
                if (iData.GetDataPresent(DataFormats.Text))
                {
                    Type = "rtf";
                }
            }
            if (iData.GetDataPresent(DataFormats.Html))
            {
                HtmlText = (string)iData.GetData(DataFormats.Html);
                if (iData.GetDataPresent(DataFormats.Text))
                {
                    Type = "html";
                    Match Match = Regex.Match(HtmlText, "SourceURL:(" + LinkPattern + ")", RegexOptions.IgnoreCase);
                    if (Match.Captures.Count > 0)
                        Url = Match.Groups[1].ToString();
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
                string[] FileNameList = iData.GetData(DataFormats.FileDrop) as string[];
                if (FileNameList != null)
                {
                    Text = String.Join("\n", FileNameList);
                    if (iData.GetDataPresent(DataFormats.FileDrop))
                    {
                        Type = "file";
                    }
                }
                else
                {
                    // Coping Outlook task
                }
            }

            byte[] BinaryBuffer = new byte[0];
            // http://www.cyberforum.ru/ado-net/thread832314.html
            if (iData.GetDataPresent(DataFormats.Bitmap))
            {
                Type = "img";
                Bitmap Img = iData.GetData(DataFormats.Bitmap) as Bitmap;
                MemoryStream MemoryStream = new MemoryStream();
                Img.Save(MemoryStream, System.Drawing.Imaging.ImageFormat.Png);
                BinaryBuffer = MemoryStream.ToArray();
                if (Text == "")
                {
                    Text = CurrentResourceManager.GetString("Size") + ": " + Img.Width + "x" + Img.Height + "\n"
                         + CurrentResourceManager.GetString("ColorDepth") + ": " + Img.PixelFormat + "\n";
                }
                Chars = Img.Width * Img.Height;
            }

            if (Type != "")
            {
                AddClip(BinaryBuffer, HtmlText, RichText, Type, Text, Application, Window, Url, Chars);
            }

        }
        void AddClip(byte[] BinaryBuffer, string HtmlText, string RichText, string Type, string Text, string Application, string Window, string Url, int Chars = 0)
        {
            int Size = Text.Length * 2; // dirty
            if (Chars == 0)
                Chars = Text.Length;
            LastId = LastId + 1;
            Size += BinaryBuffer.Length;
            if (Size > Properties.Settings.Default.MaxClipSizeKB * 1000)
                return;
            DateTime Created = DateTime.Now;
            String Title = Text.TrimStart();
            Title = Regex.Replace(Title, @"\s+", " ");
            if (Title.Length > 50)
            {
                Title = Title.Substring(0, 50 - 1 - 3) + "...";
            }
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] BinaryText = Encoding.Unicode.GetBytes(Text);
            md5.TransformBlock(BinaryText, 0, BinaryText.Length, BinaryText, 0);
            byte[] BinaryRichText = Encoding.Unicode.GetBytes(RichText);
            md5.TransformBlock(BinaryRichText, 0, BinaryRichText.Length, BinaryRichText, 0);
            byte[] BinaryHtml = Encoding.Unicode.GetBytes(HtmlText);
            md5.TransformBlock(BinaryHtml, 0, BinaryHtml.Length, BinaryHtml, 0);
            md5.TransformFinalBlock(BinaryBuffer, 0, BinaryBuffer.Length);
            string Hash = Convert.ToBase64String(md5.Hash);
            bool Used = false;

            string sql = "SELECT Id, Used FROM Clips Where Hash = @Hash";
            SQLiteCommand commandSelect = new SQLiteCommand(sql, m_dbConnection);
            commandSelect.Parameters.Add("@Hash", DbType.String).Value = Hash;
            using (SQLiteDataReader reader = commandSelect.ExecuteReader())
            {
                if (reader.Read())
                {
                    Used = reader.GetBoolean(reader.GetOrdinal("Used"));
                    sql = "DELETE FROM Clips Where Id = @Id";
                    SQLiteCommand commandDelete = new SQLiteCommand(sql, m_dbConnection);
                    commandDelete.Parameters.Add("@Id", DbType.String).Value = reader.GetInt32(reader.GetOrdinal("Id"));
                    commandDelete.ExecuteNonQuery();
                }
            }

            sql = "insert into Clips (Id, Title, Text, Application, Window, Created, Type, Binary, Size, Chars, RichText, HtmlText, Used, Url, Hash) "
               + "values (@Id, @Title, @Text, @Application, @Window, @Created, @Type, @Binary, @Size, @Chars, @RichText, @HtmlText, @Used, @Url, @Hash)";

            SQLiteCommand commandInsert = new SQLiteCommand(sql, m_dbConnection);
            commandInsert.Parameters.Add("@Id", System.Data.DbType.Int32).Value = LastId;
            commandInsert.Parameters.Add("@Title", System.Data.DbType.String).Value = Title;
            commandInsert.Parameters.Add("@Text", System.Data.DbType.String).Value = Text;
            commandInsert.Parameters.Add("@RichText", System.Data.DbType.String).Value = RichText;
            commandInsert.Parameters.Add("@HtmlText", System.Data.DbType.String).Value = HtmlText;
            commandInsert.Parameters.Add("@Application", System.Data.DbType.String).Value = Application;
            commandInsert.Parameters.Add("@Window", System.Data.DbType.String).Value = Window;
            commandInsert.Parameters.Add("@Created", System.Data.DbType.DateTime).Value = Created;
            commandInsert.Parameters.Add("@Type", System.Data.DbType.String).Value = Type;
            commandInsert.Parameters.Add("@Binary", System.Data.DbType.Binary).Value = BinaryBuffer;
            commandInsert.Parameters.Add("@Size", System.Data.DbType.Int32).Value = Size;
            commandInsert.Parameters.Add("@Chars", System.Data.DbType.Int32).Value = Chars;
            commandInsert.Parameters.Add("@Used", System.Data.DbType.Boolean).Value = Used;
            commandInsert.Parameters.Add("@Url", System.Data.DbType.String).Value = Url;
            commandInsert.Parameters.Add("@Hash", System.Data.DbType.String).Value = Hash;
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
            int NumberOfClipsToDelete = ClipsNumber - Properties.Settings.Default.HistoryDepthNumber;
            if (NumberOfClipsToDelete > 0)
            {
                commandInsert.CommandText = "Delete From Clips where (NOT Favorite OR Favorite IS NULL) AND Id IN (Select ID From Clips ORDER BY ID Limit @Number)";
                commandInsert.Parameters.Add("Number", System.Data.DbType.Int32).Value = NumberOfClipsToDelete;
                commandInsert.ExecuteNonQuery();
                ClipsNumber -= NumberOfClipsToDelete;
            }
            if (this.Visible)
                UpdateClipBindingSource();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            allowRowLoad = false;
            //int i = dataGridView.CurrentRow.Index;
            string sql = "Delete from Clips where Id IN(null";
            SQLiteCommand command = new SQLiteCommand("", m_dbConnection);
            int Counter = 0;
            foreach (DataGridViewRow SelectedRow in dataGridView.SelectedRows)
            {
                DataRowView DataRow = (DataRowView)SelectedRow.DataBoundItem;
                string ParameterName = "@Id" + Counter;
                sql += "," + ParameterName;
                command.Parameters.Add(ParameterName, System.Data.DbType.Int32).Value = DataRow["Id"];
                Counter++;
                dataGridView.Rows.Remove(SelectedRow);
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
            AfterRowLoad();
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();
        [DllImport("User32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("User32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
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
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int ProcessId);
        [DllImport("user32.dll")]
        static extern IntPtr GetFocus();
        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        // http://stackoverflow.com/questions/37291533/change-keyboard-layout-from-c-sharp-code-with-net-4-5-2
        internal sealed class KeyboardLayout
        {
            [DllImport("user32.dll")]
            static extern uint LoadKeyboardLayout(StringBuilder pwszKLID, uint flags);
            [DllImport("user32.dll")]
            static extern uint GetKeyboardLayout(uint idThread);
            [DllImport("user32.dll")]
            static extern uint ActivateKeyboardLayout(uint hkl, uint Flags);

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

        class KeyboardLayoutScope : IDisposable
        {
            private readonly KeyboardLayout currentLayout;

            public KeyboardLayoutScope(CultureInfo culture)
            {
                this.currentLayout = KeyboardLayout.GetCurrent();
                var layout = KeyboardLayout.Load(culture);
                layout.Activate();
            }

            public void Dispose()
            {
                this.currentLayout.Activate();
            }
        }

        private void CopyClipToClipboard(bool OnlyText = false)
        {
            StringCollection LastFilterValues = Properties.Settings.Default.LastFilterValues;
            if (comboBoxFilter.Text != "" && !LastFilterValues.Contains(comboBoxFilter.Text))
            {
                LastFilterValues.Insert(0, comboBoxFilter.Text);
                while (LastFilterValues.Count > 20)
                {
                    LastFilterValues.RemoveAt(LastFilterValues.Count - 1);
                }
                FillFilterItems();
            }

            DataRow CurrentDataRow = ((DataRowView)clipBindingSource.Current).Row;
            string Type = (string)RowReader["type"];
            object RichText = RowReader["RichText"];
            object HtmlText = RowReader["HtmlText"];
            byte[] Binary = RowReader["Binary"] as byte[];
            string Text = (string)RowReader["Text"];
            DataObject dto = new DataObject();
            if (Type == "rtf" || Type == "text" || Type == "html")
            {
                dto.SetText(Text, TextDataFormat.UnicodeText);
            }
            if (Type == "rtf" && !(RichText is DBNull) && !OnlyText)
            {
                dto.SetText((string)RichText, TextDataFormat.Rtf);
            };
            if (Type == "html" && !(HtmlText is DBNull) && !OnlyText)
            {
                dto.SetText((string)HtmlText, TextDataFormat.Html);
            };
            if (Type == "file" && !OnlyText)
            {
                string[] FileNameList = Text.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                StringCollection FileNameCollection = new StringCollection();
                foreach (string FileName in FileNameList)
                {
                    FileNameCollection.Add(FileName);
                }
                dto.SetFileDropList(FileNameCollection as StringCollection);
            };
            if (Type == "img" && !OnlyText)
            {
                Image image = GetImageFromBinary(Binary);
                dto.SetImage(image);
                //MemoryStream ms = new MemoryStream();
                //MemoryStream ms2 = new MemoryStream();
                //image.Save(ms, ImageFormat.Bmp);
                //byte[] b = ms.GetBuffer();
                //ms2.Write(b, 14, (int)ms.Length - 14);
                //ms.Position = 0;
                //dto.SetData("DeviceIndependentBitmap", ms2);
            };
            if (!Properties.Settings.Default.MoveCopiedClipToTop)
                CaptureClipboard = false;
            Clipboard.Clear();
            Clipboard.SetDataObject(dto);
            Application.DoEvents(); // To process UpdateClipBoardMessage
            CaptureClipboard = true;
        }

        private void SendPaste(bool OnlyText = false)
        {
            SetRowMark("Used");
            CopyClipToClipboard(OnlyText);

            CultureInfo EnglishCultureInfo = null;
            foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
            {
                if (String.Compare(lang.Culture.TwoLetterISOLanguageName, "en", true) == 0)
                {
                    EnglishCultureInfo = lang.Culture;
                    break;
                }
            }
            if (EnglishCultureInfo == null)
            {
                MessageBox.Show("Unable to find English input language", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.Hide();
            IntPtr hForegroundWindow = GetForegroundWindow();
            EnableWindow(hForegroundWindow, true);
            //IntPtr hFocusWindow = FocusWindow();
            //string WindowTitle = GetWindowTitle(hFocusWindow);
            //Debug.WriteLine("Window = " + hFocusWindow + " \"" + WindowTitle + "\"");
            //Thread.Sleep(50);

            // Option 1
            // http://stackoverflow.com/questions/37291533/change-keyboard-layout-from-c-sharp-code-with-net-4-5-2

            using (new KeyboardLayoutScope(EnglishCultureInfo))
            {
                SendKeys.SendWait("^{v}"); // Работает только при включенной английской раскладке клавиатуры
            }

            ((DataRowView)dataGridView.CurrentRow.DataBoundItem).Row["Used"] = true;
            PrepareTableGrid();

            //// Option 2
            //const int WM_SYSCOMMAND = 0x0112;
            //const int SC_CLOSE = 0xF060;
            //const int WM_COPY = 0x0301;
            //const int WM_CHAR = 0x0102;
            //const int WM_PASTE = 0x0302;
            //const int WM_GETTEXT = 0x000D;
            //sendKey(hwnd, Keys.ControlKey, false, true, false);
            //sendKey(hwnd, Keys.V, false, true, false);
            ////PostMessage(hwnd, WM_PASTE, 0, 0);
            //if (GetAsyncKeyState(Keys.V) == 0)
            //{
            //    sendKey(hwnd, Keys.V, false, false, true);
            //}
            //if (GetAsyncKeyState(Keys.ControlKey) == 0)
            //{
            //    sendKey(hwnd, Keys.ControlKey, false, false, true);
            //}

            //// CMD window https://blogs.msdn.microsoft.com/bill/2012/06/09/programmatically-paste-clipboard-text-to-a-cmd-window-c-or-c/
            //int WM_COMMAND = 0x0111;
            //SendMessage(hwnd, WM_COMMAND, 0xfff1, 0);

            //this.Show();
        }

        private void SetRowMark(string FieldName, Boolean NewValue = true, Boolean AllSelected = false)
        {
            string sql = "Update Clips set " + FieldName + "=@Value where Id IN(null";
            SQLiteCommand command = new SQLiteCommand("", m_dbConnection);
            List<DataGridViewRow> SelectedRows = new List<DataGridViewRow>();
            if (AllSelected)
                foreach (DataGridViewRow SelectedRow in dataGridView.SelectedRows)
                    SelectedRows.Add(SelectedRow);
            else
                SelectedRows.Add((DataGridViewRow)dataGridView.CurrentRow);
            int Counter = 0;
            foreach (DataGridViewRow SelectedRow in SelectedRows)
            {
                if (SelectedRow == null)
                    continue;
                DataRowView DataRow = (DataRowView)SelectedRow.DataBoundItem;
                string ParameterName = "@Id" + Counter;
                sql += "," + ParameterName;
                command.Parameters.Add(ParameterName, System.Data.DbType.Int32).Value = DataRow["Id"];
                Counter++;
            }
            sql += ")";
            command.CommandText = sql;
            command.Parameters.Add("@Value", DbType.Boolean).Value = NewValue;
            command.ExecuteNonQuery();
            //dbDataSet.ClipsRow Row = (dbDataSet.ClipsRow)dbDataSet.Clips.Rows[dataGridView.CurrentRow.Index];
            //Row.Used = true;
            //dataAdapter.Update(dbDataSet);
            UpdateClipBindingSource();
        }

        private static Image GetImageFromBinary(byte[] Binary)
        {
            MemoryStream MemoryStream = new MemoryStream(Binary, 0, Binary.Length);
            MemoryStream.Write(Binary, 0, Binary.Length);
            Image image = new Bitmap(MemoryStream);
            return image;
        }

        private StringCollection FillFilterItems()
        {
            StringCollection LastFilterValues = Properties.Settings.Default.LastFilterValues;
            comboBoxFilter.Items.Clear();
            foreach (string String in LastFilterValues)
            {
                comboBoxFilter.Items.Add(String);
            }

            return LastFilterValues;
        }

        IntPtr FocusWindow()
        {
            IntPtr hwnd = GetForegroundWindow();
            int PID;
            uint remoteThreadId = GetWindowThreadProcessId(hwnd, out PID);
            uint currentThreadId = GetCurrentThreadId();
            //AttachTrheadInput is needed so we can get the handle of a focused window in another app
            AttachThreadInput(remoteThreadId, currentThreadId, true);
            hwnd = GetFocus();
            AttachThreadInput(remoteThreadId, currentThreadId, false);
            return hwnd;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        void GetClipboardOwnerInfo(out string Window, out string Application)
        {
            IntPtr hwnd = GetClipboardOwner();
            int ProcessId = 0;
            GetWindowThreadProcessId(hwnd, out ProcessId);
            Window = "";
            Process Process1 = Process.GetProcessById((int)ProcessId);
            Application = Process1.ProcessName;
            hwnd = Process1.MainWindowHandle;
            Window = GetWindowTitle(hwnd);
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

        void sendKey(IntPtr hwnd, Keys keyCode, bool extended = false, bool Down = true, bool Up = true)
        {
            // http://stackoverflow.com/questions/10280000/how-to-create-lparam-of-sendmessage-wm-keydown
            const int WM_KEYDOWN = 0x0100;
            const int WM_KEYUP = 0x0101;
            uint scanCode = MapVirtualKey((uint)keyCode, 0);
            uint lParam;
            lParam = (0x00000001 | (scanCode << 16));
            if (extended)
            {
                lParam |= 0x01000000;
            }
            if (Down)
            {
                PostMessage(hwnd, WM_KEYDOWN, (int)keyCode, (int)lParam);
            }
            lParam |= 0xC0000000;  // set previous key and transition states (bits 30 and 31)
            if (Up)
            {
                PostMessage(hwnd, WM_KEYUP, (int)keyCode, (int)lParam);
            }
        }

        private static string GetWindowTitle(IntPtr hwnd)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            string WindowTitle = "";
            if (GetWindowText(hwnd, Buff, nChars) > 0)
            {
                WindowTitle = Buff.ToString();
            }
            return WindowTitle;
        }

        private void dataGridView_DoubleClick(object sender, EventArgs e)
        {
            SendPaste();
            this.Hide();
        }
        private void pasteOriginalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendPaste();
        }
        private void pasteAsTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendPaste(true);
        }
        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (allowRowLoad)
                AfterRowLoad();
        }

        private void Main_Deactivate(object sender, EventArgs e)
        {
            //if (this.WindowState == FormWindowState.Minimized)
            //{
            //    this.ShowInTaskbar = false;
            //    //notifyIcon.Visible = true;
            //}
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

        private void ShowForPaste()
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            AutoGotoLastRow = Properties.Settings.Default.SelectTopClipOnShow;
            if (Properties.Settings.Default.WindowAutoPosition)
            {
                // https://www.codeproject.com/Articles/34520/Getting-Caret-Position-Inside-Any-Application
                // http://stackoverflow.com/questions/31055249/is-it-possible-to-get-caret-position-in-word-to-update-faster
                IntPtr hWindow = GetForegroundWindow();
                if (hWindow != this.Handle)
                { 
	                int PID;
	                uint remoteThreadId = GetWindowThreadProcessId(hWindow, out PID);
	                var guiInfo = new GUITHREADINFO();
	                guiInfo.cbSize = (uint)Marshal.SizeOf(guiInfo);
	                GetGUIThreadInfo(remoteThreadId, out guiInfo);
	                Point point = new Point(0, 0);
                    ClientToScreen(guiInfo.hwndCaret, out point);
                    //AttachThreadInput(GetCurrentThreadId(), remoteThreadId, true);
                    //int Result = GetCaretPos(out point);
                    //AttachThreadInput(GetCurrentThreadId(), remoteThreadId, false);
                    // Screen.FromHandle(hwnd)
                    if (point.Y > 0)
	                {
                        RECT ActiveRect = guiInfo.rcCaret;
                        this.Left = Math.Min(ActiveRect.right + point.X, SystemInformation.VirtualScreen.Width - this.Width);
                        this.Top = Math.Min(ActiveRect.bottom + point.Y + 1, SystemInformation.VirtualScreen.Height - this.Height - 30);
                    }
                    else
                    {
                        ClientToScreen(guiInfo.hwndFocus, out point);
                        RECT ActiveRect;
                        GetWindowRect(guiInfo.hwndFocus, out ActiveRect);
                        this.Left = Math.Max(0, Math.Min((ActiveRect.right - ActiveRect.left - this.Width) / 2 + point.X, SystemInformation.VirtualScreen.Width - this.Width));
                        this.Top = Math.Max(0, Math.Min((ActiveRect.bottom - ActiveRect.top - this.Height) / 2 + point.Y, SystemInformation.VirtualScreen.Height - this.Height - 30));
                    }
                }
            }
            this.Activate();
            this.Show();
            //notifyIcon.Visible = false;
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Hide();
                e.Handled = true;
            }
            if (e.KeyCode == Keys.Enter)
            {
                
                SendPaste(e.Control);
                e.Handled = true;
            }
        }

        private void exitToolStripMenuItem_Click(object sender = null, EventArgs e = null)
        {
            AllowFormClose = true;
            this.Close();
        }

        private void Filter_KeyDown(object sender, KeyEventArgs e)
        {
            PassKeyToGrid(true, e);
        }

        private void Filter_KeyUp(object sender, KeyEventArgs e)
        {
            PassKeyToGrid(false, e);
        }

        private void PassKeyToGrid(bool DownOrUp, KeyEventArgs e)
        {
            if (IsKeyPassedFromFilterToGrid(e.KeyCode, e.Control))
            {
                sendKey(dataGridView.Handle, e.KeyCode, false, DownOrUp, !DownOrUp);
                e.Handled = true;
            }
        }

        private static bool IsKeyPassedFromFilterToGrid(Keys Key, bool IsCtrlDown = false)
        {
            return false
                || Key == Keys.Down
                || Key == Keys.Up
                || Key == Keys.PageUp
                || Key == Keys.PageDown
                || Key == Keys.ControlKey
                || Key == Keys.Home && IsCtrlDown
                || Key == Keys.End && IsCtrlDown;
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
                SelectCurrentRow();
            }
            AfterRowLoad();
        }

        void SelectCurrentRow(bool forceRowLoad = false)
        {
            dataGridView.ClearSelection();
            if (dataGridView.CurrentRow == null)
            {
                GotoLastRow();
                return;
            }
            dataGridView.Rows[dataGridView.CurrentRow.Index].Selected = true;
            if (forceRowLoad)
                AfterRowLoad();
        }

        private void activateListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView.Focus();
        }

        private void PrepareTableGrid()
        {
            System.Resources.ResourceManager ResourceManager = Properties.Resources.ResourceManager;
            Bitmap ImageText = ResourceManager.GetObject("TypeText") as Bitmap;
            Bitmap ImageHtml = ResourceManager.GetObject("TypeHtml") as Bitmap;
            Bitmap ImageRtf = ResourceManager.GetObject("TypeRtf") as Bitmap;
            Bitmap ImageFile = ResourceManager.GetObject("TypeFile") as Bitmap;
            Bitmap ImageImg = ResourceManager.GetObject("TypeImg") as Bitmap;
            RichTextBox _richTextBox = new RichTextBox();
            foreach (DataGridViewRow Row in dataGridView.Rows)
            {
                DataRowView DataRowView = (DataRowView)(dataGridView.Rows[Row.Index].DataBoundItem);
                int ShortSize = DataRowView.Row["Chars"].ToString().Length;
                if (ShortSize > 2)
                    Row.Cells["ShortVisibleSize"].Value = ShortSize;
                string ClipType = (string)DataRowView.Row["Type"];
                Bitmap Image = null;
                switch (ClipType)
                {
                    case "text":
                        Image = ImageText;
                        break;
                    case "html":
                        Image = ImageHtml;
                        break;
                    case "rtf":
                        Image = ImageRtf;
                        break;
                    case "file":
                        Image = ImageFile;
                        break;
                    case "img":
                        Image = ImageImg;
                        break;
                    default:
                        break;
                }
                if (Image != null)
                {
                    Row.Cells["TypeImg"].Value = Image;
                }
                if (DataRowView.Row["Favorite"] != DBNull.Value && (bool)DataRowView.Row["Favorite"])
                {
                    foreach (DataGridViewCell Cell in Row.Cells)
                    {
                        Cell.Style.BackColor = Color.FromArgb(255, 220, 220);
                    }
                }
                else if ((bool)DataRowView.Row["Used"])
                {
                    foreach (DataGridViewCell Cell in Row.Cells)
                    {
                        Cell.Style.BackColor = Color.FromArgb(200, 255, 255);
                    }
                }
                Row.Cells["Title"].Value = DataRowView.Row["Title"].ToString();
                if (comboBoxFilter.Text!="" && dataGridView.Columns["Title"].Visible)
                {
                    _richTextBox.Clear();
                    _richTextBox.Text = Row.Cells["Title"].Value as string;
                    MatchCollection tempMatches;
                    MarkRegExpMatchesInRichTextBox(_richTextBox, Regex.Escape(comboBoxFilter.Text).Replace("%", ".*?"), Color.Red, false, out tempMatches);
                    Row.Cells["Title"].Value = _richTextBox.Rtf;
                }
            }
            dataGridView.Update();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings SettingsForm = new Settings();
            SettingsForm.Owner = this;
            hook.UnregisterHotKeys();
            SettingsForm.ShowDialog();
            if (SettingsForm.DialogResult == DialogResult.OK)
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
            UpdateCurrentCulture();
            cultureManager1.UICulture = Thread.CurrentThread.CurrentUICulture;

            this.Text = "Clip Angel " + Properties.Resources.Version;

            BindingList<ListItemNameText> _comboItemsTypes = new BindingList<ListItemNameText>();
            _comboItemsTypes.Add(new ListItemNameText { Name = "allTypes", Text = CurrentResourceManager.GetString("allTypes") });
            _comboItemsTypes.Add(new ListItemNameText { Name = "text", Text = CurrentResourceManager.GetString("text") });
            _comboItemsTypes.Add(new ListItemNameText { Name = "file", Text = CurrentResourceManager.GetString("file") });
            _comboItemsTypes.Add(new ListItemNameText { Name = "img", Text = CurrentResourceManager.GetString("img") });
            TypeFilter.DataSource = _comboItemsTypes;
            TypeFilter.DisplayMember = "Text";
            TypeFilter.ValueMember = "Name";

            BindingList<ListItemNameText> _comboItemsMarks = new BindingList<ListItemNameText>();
            _comboItemsMarks.Add(new ListItemNameText { Name = "allMarks", Text = CurrentResourceManager.GetString("allMarks") });
            _comboItemsMarks.Add(new ListItemNameText { Name = "used", Text = CurrentResourceManager.GetString("used") });
            _comboItemsMarks.Add(new ListItemNameText { Name = "favorite", Text = CurrentResourceManager.GetString("favorite") });
            MarkFilter.DataSource = _comboItemsMarks;
            MarkFilter.DisplayMember = "Text";
            MarkFilter.ValueMember = "Name";

            ChooseTitleColumnDraw();
            AfterRowLoad();
        }

        private void ChooseTitleColumnDraw()
        {
            bool ResultSimpleDraw = Properties.Settings.Default.ClipListSimpleDraw || comboBoxFilter.Text == "";
            dataGridView.Columns["TitleSimple"].Visible = ResultSimpleDraw;
            dataGridView.Columns["Title"].Visible = !ResultSimpleDraw;
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
                    var htmlParser = new HtmlParser();
                    var documentHtml = htmlParser.Parse(HtmlSource);
                    IHtmlCollection<IElement> Refs = documentHtml.GetElementsByClassName("sfdl");
                    Match match = Regex.Match(Refs[0].TextContent, @"Clip Angel (.*).zip");
                    if (match == null)
                        return;
                    ActualVersion = match.Groups[1].Value;
                    if (ActualVersion != Properties.Resources.Version)
                    {
                        buttonUpdate.Visible = true;
                        toolStripUpdateToSeparator.Visible = true;
                        buttonUpdate.ForeColor = Color.Blue;
                        buttonUpdate.Text = CurrentResourceManager.GetString("UpdateTo") + " " + ActualVersion;
                        if (UserRequest)
                        {
                            MessageBox.Show(CurrentResourceManager.GetString("NewVersionAvailable"), "Clip Angel");
                        }
                    }
                    else if (UserRequest)
                    {
                        MessageBox.Show(CurrentResourceManager.GetString("YouLatestVersion"), "Clip Angel");
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
                string TempFolder = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString();
                Directory.CreateDirectory(TempFolder);
                string TempFilenameZip = TempFolder + "\\NewVersion" + ".zip";
                bool Success = true;
                //try
                //{
                    wc.DownloadFile(Properties.Resources.DownloadPage, TempFilenameZip);
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
                File.Copy(UpdaterName, TempFolder + "\\" + UpdaterName);
                File.Copy("DotNetZip.dll", TempFolder + "\\DotNetZip.dll");
                if (Success)
                {
                    Process.Start(TempFolder + "\\" + UpdaterName, "\"" + TempFilenameZip + "\" \"" + Application.StartupPath + "\" \"" + Application.ExecutablePath
                        + "\" " + Process.GetCurrentProcess().Id);
                    exitToolStripMenuItem_Click();
                }
            }
        }

        private void UpdateCurrentCulture()
        {
            if (Properties.Settings.Default.Language == "Default")
                Locale = Application.CurrentCulture.TwoLetterISOLanguageName;
            else if (Properties.Settings.Default.Language == "Russian")
                Locale = "ru";
            else
                Locale = "en";
            if (true
                && CurrentResourceManager != null
                && String.Compare(Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName, Locale, true) != 0
            )
                MessageBox.Show(CurrentResourceManager.GetString("LangRestart"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (String.Compare(Locale, "ru", true) == 0)
                CurrentResourceManager = Properties.Resource_RU.ResourceManager;
            else
                CurrentResourceManager = Properties.Resources.ResourceManager;
            // https://www.codeproject.com/Articles/23694/Changing-Your-Application-User-Interface-Culture-O
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Locale);
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
            RemoveClipboardFormatListener(this.Handle);
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            exitToolStripMenuItem_Click();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form AboutBox = new AboutBox();
            AboutBox.ShowDialog(this);
        }

        private void Main_Activated(object sender, EventArgs e)
        {
            //#if DEBUG
            //    return;
            //#endif
            //PrepareTableGrid(); // иначе оформление не появлялось при свернутом запуске
            UpdateClipBindingSource();
        }

        private void Filter_KeyPress(object sender, KeyPressEventArgs e)
        {
            // http://csharpcoding.org/tag/keypress/ Workaroud strange beeping 
            if (e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Escape)
                e.Handled = true;
        }

        private static void OpenLinkIfCtrlPressed(RichTextBox sender, EventArgs e, MatchCollection Matches)
        {
            Keys mod = Control.ModifierKeys & Keys.Modifiers;
            bool ctrlOnly = mod == Keys.Control;
            if (ctrlOnly)
                foreach (Match Match in Matches)
                {
                    if (Match.Index <= sender.SelectionStart && (Match.Index + Match.Length) >= sender.SelectionStart)
                        Process.Start(Match.Value);
                }
                    
        }

        private void textBoxUrl_Click(object sender, EventArgs e)
        {
            OpenLinkIfCtrlPressed(sender as RichTextBox, e, UrlLinkMatches);
        }

        private void ImageControl_DoubleClick(object sender, EventArgs e)
        {
            string TempFile = Path.GetTempFileName() + Guid.NewGuid().ToString() + ".bmp";
            ImageControl.Image.Save(TempFile);
            Process.Start(TempFile);

        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            UpdateClipBindingSource();
        }

        private void TypeFilter_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateClipBindingSource();
        }

        private void buttonFindNext_Click(object sender, EventArgs e)
        {
            RichTextBox Control = richTextBox;
            MatchCollection Matches = FilterMatches;
            if (FilterMatches == null)
                return;
            if (TextWasCut)
                AfterRowLoad(-1, true);
            foreach (Match Match in Matches)
            {
                if (Control.SelectionStart < Match.Index)
                {
                    Control.SelectionStart = Match.Index;
                    Control.SelectionLength = Match.Length;
                    break;
                }
            }
        }

        private void buttonFindPrevious_Click(object sender, EventArgs e)
        {
            RichTextBox Control = richTextBox;
            MatchCollection Matches = FilterMatches;
            if (FilterMatches == null)
                return;
            Match PrevMatch = null;
            foreach (Match Match in Matches)
            {
                if (Control.SelectionStart > Match.Index)
                {
                    PrevMatch = Match;
                }
            }
            if (PrevMatch != null)
            {
                Control.SelectionStart = PrevMatch.Index;
                Control.SelectionLength = PrevMatch.Length;
            }
        }

        private void wordWrapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.WordWrap = !Properties.Settings.Default.WordWrap;
            UpdateControlsStates();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(CurrentResourceManager.GetString("HelpPage"));
        }

        private void copyClipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyClipToClipboard();
        }

        private void toolStripButtonSelectTopClipOnShow_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.SelectTopClipOnShow = !Properties.Settings.Default.SelectTopClipOnShow;
            UpdateControlsStates();
        }
        private void UpdateControlsStates()
        {
            selectTopClipOnShowToolStripMenuItem.Checked = Properties.Settings.Default.SelectTopClipOnShow;
            toolStripButtonSelectTopClipOnShow.Checked = Properties.Settings.Default.SelectTopClipOnShow;
            wordWrapToolStripMenuItem.Checked = Properties.Settings.Default.WordWrap;
            toolStripButtonWordWrap.Checked = Properties.Settings.Default.WordWrap;
            dataGridView.Columns["ShortVisibleSize"].Visible = Properties.Settings.Default.ShowVisibleSizeColumn;
            richTextBox.WordWrap = wordWrapToolStripMenuItem.Checked;
        }

        private void toolStripMenuItemClearFilterAndSelectTop_Click(object sender, EventArgs e)
        {
            ClearFilter_Click();
            GotoLastRow();
        }

        private void changeClipTitleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView.CurrentRow != null)
            {
                DataRow CurrentDataRow = ((DataRowView)clipBindingSource.Current).Row;
                string oldTitle = CurrentDataRow["Title"] as string;
                string newTitle = Interaction.InputBox("", "Edit clip title", oldTitle);
                if (newTitle != "")
                {
                    string sql = "Update Clips set Title=@Title where Id=@Id";
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    command.Parameters.Add("@Id", DbType.Int32).Value = CurrentDataRow["Id"];
                    command.Parameters.Add("@Title", DbType.String).Value = newTitle;
                    command.ExecuteNonQuery();
                    UpdateClipBindingSource();
                }
            }
        }

        private void setFavouriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetRowMark("Favorite", true, true);
        }

        private void resetFavouriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetRowMark("Favorite", false, true);
        }

        private void MarkFilter_SelectedValueChanged(object sender, EventArgs e)
        {
            UpdateClipBindingSource();
        }

        private void showAllMarksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MarkFilter.SelectedValue = "allMarks";
        }

        private void showOnlyUsedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MarkFilter.SelectedValue = "used";
        }

        private void showOnlyFavoriteToolStripMenuItem_Click(object sender, EventArgs e)
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

        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            RunUpdate();
        }

        private void checkUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckUpdate(true);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CheckUpdate();
        }

        private void moveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RowShift(-1);
        }

        private void RowShift(int IndexShift =-1)
        {
            if (dataGridView.CurrentRow == null)
                return;
            int CurrentRowIndex = dataGridView.CurrentRow.Index;
            if (false
                || IndexShift < 0 && CurrentRowIndex == 0
                || IndexShift > 0 && CurrentRowIndex == dataGridView.RowCount
               )
                return;
            DataRow CurrentDataRow = ((DataRowView)clipBindingSource[CurrentRowIndex]).Row;
            DataRow NearDataRow = ((DataRowView)clipBindingSource[CurrentRowIndex + IndexShift]).Row;
            int oldID = (int)CurrentDataRow["ID"];
            int newID = (int)NearDataRow["ID"];
            int tempID = LastId + 1;
            string sql = "Update Clips set Id=@NewId where Id=@Id";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.Parameters.Add("@Id", DbType.Int32).Value = newID;
            command.Parameters.Add("@NewID", DbType.Int32).Value = tempID;
            command.ExecuteNonQuery();
            command.Parameters.Add("@Id", DbType.Int32).Value = oldID;
            command.Parameters.Add("@NewID", DbType.Int32).Value = newID;
            command.ExecuteNonQuery();
            command.Parameters.Add("@Id", DbType.Int32).Value = tempID;
            command.Parameters.Add("@NewID", DbType.Int32).Value = oldID;
            command.ExecuteNonQuery();
            clipBindingSource.Position = CurrentRowIndex + IndexShift;
            //SelectCurrentRow();
            UpdateClipBindingSource();
        }

        private void moveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RowShift(1);
        }

        private void historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Process.Start(CurrentResourceManager.GetString("HistoryOfChanges")); // Returns 0. Why?
            Process.Start("https://sourceforge.net/p/clip-angel/blog");
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

    /// <summary>
    /// Represents the window that is used internally to get the messages.
    /// </summary>
    private class Window : NativeWindow, IDisposable
    {
        private static int WM_HOTKEY = 0x0312;

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

    public KeyboardHook()
    {
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
            string ErrorText = "Couldn’t register the hot key " + hotkeyTitle;
            //throw new InvalidOperationException(ErrorText);
            MessageBox.Show(ErrorText, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public static string HotkeyTitle(Keys key, EnumModifierKeys modifier)
    {
        string HotkeyTitle = "";
        if ((modifier & EnumModifierKeys.Win) != 0)
            HotkeyTitle += Keys.Control.ToString() + " + ";
        if ((modifier & EnumModifierKeys.Control) != 0)
            HotkeyTitle += Keys.Control.ToString() + " + ";
        if ((modifier & EnumModifierKeys.Alt) != 0)
            HotkeyTitle += Keys.Alt.ToString() + " + ";
        if ((modifier & EnumModifierKeys.Shift) != 0)
            HotkeyTitle += Keys.Shift.ToString() + " + ";
        HotkeyTitle += key.ToString();
        return HotkeyTitle;
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

// Решение проблемы регистрозависимости UNICODE символов SQLite http://www.cyberforum.ru/ado-net/thread1708878.html
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
