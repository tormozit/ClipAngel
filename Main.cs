using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Drawing.Imaging;
using System.Security.Cryptography;

namespace ClipAngel
{
    public partial class Main : Form
    {
        SQLiteConnection m_dbConnection;
        String connectionString;
        SQLiteDataAdapter dataAdapter;
        IntPtr _ClipboardViewerNext;
        bool CaptureClipboard = true;
        bool allowRowLoad = true;
        bool AutoGotoLastRow = true;
        bool AllowFormClose = false;
        static string LinkPattern = "\\b(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[A-Z0-9+&@#/%=~_|]";
        int LastId = 0;
        string LastText;
        public string DBFileName;
        public int ClipsNumber = 0;
        public bool StartMinimized = false;
        MatchCollection TextLinkMatches;
        MatchCollection UrlLinkMatches;
        KeyboardHook hook = new KeyboardHook();

        #region Clipboard Formats

        string[] formatsAll = new string[]
        {
            DataFormats.Bitmap,
            DataFormats.CommaSeparatedValue,
            DataFormats.Dib,
            DataFormats.Dif,
            DataFormats.EnhancedMetafile,
            DataFormats.FileDrop,
            DataFormats.Html,
            DataFormats.Locale,
            DataFormats.MetafilePict,
            DataFormats.OemText,
            DataFormats.Palette,
            DataFormats.PenData,
            DataFormats.Riff,
            DataFormats.Rtf,
            DataFormats.Serializable,
            DataFormats.StringFormat,
            DataFormats.SymbolicLink,
            DataFormats.Text,
            DataFormats.Tiff,
            DataFormats.UnicodeText,
            DataFormats.WaveAudio
        };

        string[] formatsAllDesc = new String[]
        {
            "Bitmap",
            "CommaSeparatedValue",
            "Dib",
            "Dif",
            "EnhancedMetafile",
            "FileDrop",
            "Html",
            "Locale",
            "MetafilePict",
            "OemText",
            "Palette",
            "PenData",
            "Riff",
            "Rtf",
            "Serializable",
            "StringFormat",
            "SymbolicLink",
            "Text",
            "Tiff",
            "UnicodeText",
            "WaveAudio"
        };

        #endregion

        public Main()
        {
            InitializeComponent();
            // register the event that is fired after the key press.
            hook.KeyPressed +=
                new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            // register the control + alt + F12 combination as hot key.
            hook.RegisterHotKey(EnumModifierKeys.Alt, Keys.V);
        }

        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            ShowWindow();
            Filter.Focus();
        }

        protected override void WndProc(ref Message m)
        {
            switch ((Msgs)m.Msg)
            {
                //
                // The WM_DRAWCLIPBOARD message is sent to the first window 
                // in the clipboard viewer chain when the content of the 
                // clipboard changes. This enables a clipboard viewer 
                // window to display the new content of the clipboard. 
                //
                case Msgs.WM_DRAWCLIPBOARD:

                    Debug.WriteLine("WindowProc DRAWCLIPBOARD: " + m.Msg, "WndProc");

                    GetClipboardData();

                    //
                    // Each window that receives the WM_DRAWCLIPBOARD message 
                    // must call the SendMessage function to pass the message 
                    // on to the next window in the clipboard viewer chain.
                    //
                    User32.SendMessage(_ClipboardViewerNext, m.Msg, m.WParam, m.LParam);
                    break;


                //
                // The WM_CHANGECBCHAIN message is sent to the first window 
                // in the clipboard viewer chain when a window is being 
                // removed from the chain. 
                //
                case Msgs.WM_CHANGECBCHAIN:
                    Debug.WriteLine("WM_CHANGECBCHAIN: lParam: " + m.LParam, "WndProc");

                    // When a clipboard viewer window receives the WM_CHANGECBCHAIN message, 
                    // it should call the SendMessage function to pass the message to the 
                    // next window in the chain, unless the next window is the window 
                    // being removed. In this case, the clipboard viewer should save 
                    // the handle specified by the lParam parameter as the next window in the chain. 

                    //
                    // wParam is the Handle to the window being removed from 
                    // the clipboard viewer chain 
                    // lParam is the Handle to the next window in the chain 
                    // following the window being removed. 
                    if (m.WParam == _ClipboardViewerNext)
                    {
                        //
                        // If wParam is the next clipboard viewer then it
                        // is being removed so update pointer to the next
                        // window in the clipboard chain
                        //
                        _ClipboardViewerNext = m.LParam;
                    }
                    else
                    {
                        User32.SendMessage(_ClipboardViewerNext, m.Msg, m.WParam, m.LParam);
                    }
                    break;

                default:
                    //
                    // Let the form process the messages that we are
                    // not interested in
                    //
                    base.WndProc(ref m);
                    break;

            }

        }

        private void Main_Load(object sender, EventArgs e)
        {
            notifyIcon.Visible = true;
            this.Text += " " + Properties.Settings.Default.Version;
            if (Properties.Settings.Default.LastFilterValues == null)
            {
                Properties.Settings.Default.LastFilterValues = new StringCollection();
            }
            FillFilterItems();
            string SettingsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\ClipAngel";
            if (!Directory.Exists(SettingsPath))
            {
                Directory.CreateDirectory(SettingsPath);
            }
            DBFileName = SettingsPath + "\\db.sqlite";
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

            SQLiteCommand command = new SQLiteCommand("ALTER TABLE Clips" + " ADD COLUMN Hash CHAR(32)", m_dbConnection);
            try
            {
                command.ExecuteNonQuery();
            }
            catch
            {
                bool ColumnHashExisted = true;
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
            dataGridView.DataSource = clipBindingSource;

            UpdateClipBindingSource();
            RegisterClipboardViewer();
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

        private void AfterRowLoad(int CurrentRowIndex = -1)
        {
            DataRowView CurrentRowView;
            if (CurrentRowIndex == -1)
            { CurrentRowView = clipBindingSource.Current as DataRowView; }
            else
            { CurrentRowView = clipBindingSource[CurrentRowIndex] as DataRowView; }
            if (CurrentRowView == null)
            {
                richTextBox.Text = "";
                Application.Text = "";
                Window.Text = "";
                statusStrip.Items.Find("Created", false)[0].Text = "";
                statusStrip.Items.Find("Size", false)[0].Text = "";
                statusStrip.Items.Find("Chars", false)[0].Text = "";
                statusStrip.Items.Find("Type", false)[0].Text = "";
                statusStrip.Items.Find("Position", false)[0].Text = "";
            }
            else
            {
                DataRow CurrentRow = CurrentRowView.Row;
                statusStrip.Items.Find("Created", false)[0].Text = CurrentRow["Created"].ToString();
                statusStrip.Items.Find("Size", false)[0].Text = CurrentRow["Size"].ToString() + "b";
                statusStrip.Items.Find("Chars", false)[0].Text = CurrentRow["Chars"].ToString() + "ch";
                statusStrip.Items.Find("Type", false)[0].Text = CurrentRow["Type"].ToString();
                statusStrip.Items.Find("Position", false)[0].Text = "1";

                // to prevent autoscrolling during marking
                richTextBox.HideSelection = true;
                richTextBox.Clear();
                richTextBox.Text = CurrentRow["Text"].ToString();
                if (Filter.Text.Length > 0)
                {
                    MatchCollection Junk;
                    MarkRegExpMatchesInRichTextBox(richTextBox, Filter.Text, Color.Red, false, out Junk);
                }
                MarkLinksInRichTextBox(richTextBox, out TextLinkMatches);
                richTextBox.SelectionStart = richTextBox.TextLength;
                richTextBox.SelectionColor = Color.Green;
                richTextBox.AppendText("<END>");
                richTextBox.SelectionColor = new Color();
                richTextBox.SelectionStart = 0;
                richTextBox.HideSelection = false;

                textBoxUrl.Clear();
                textBoxUrl.Text = CurrentRow["Url"].ToString();
                MarkLinksInRichTextBox(textBoxUrl, out UrlLinkMatches);

                if (Type.Text == "img")
                {
                    Image image = GetImageFromBinary(CurrentRow["Binary"] as byte[]);
                    ImageControl.Image = image;
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
            statusStrip.Items.Find("Position", false)[0].Text = NewText;

            //NewText = "" + Text.Cursor;
            //statusStrip.Items.Find("PositionXY", false)[0].Text = NewText;
            OpenLinkIfCtrlPressed(sender as RichTextBox, e, TextLinkMatches);
        }

        private void Filter_TextChanged(object sender, EventArgs e)
        {
            UpdateClipBindingSource(true);
        }

        private void UpdateClipBindingSource(bool forceRowLoad = false)
        {
            if (dataAdapter == null)
                return;
            int CurrentClipID = 0;
            if (clipBindingSource.Current != null)
                CurrentClipID = (int)(clipBindingSource.Current as DataRowView).Row["Id"];
            string SelectCommandText = "Select * From Clips";
            if (Filter.Text != "")
            {
                SelectCommandText += " Where UPPER(Text) Like '%" + Filter.Text.ToUpper() + "%'";
            }
            SelectCommandText += " ORDER BY Id desc";
            dataAdapter.SelectCommand.CommandText = SelectCommandText;
            DataTable table = new DataTable();
            table.Locale = CultureInfo.InvariantCulture;
            dataAdapter.Fill(table);
            allowRowLoad = false;
            clipBindingSource.DataSource = table;
            PrepareTableGrid();

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
                    LastText = (string)LastRow["Text"];
                }
            }
            else if (AutoGotoLastRow)
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
            Filter.Text = "";
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

        /// Register this form as a Clipboard Viewer application
        private void RegisterClipboardViewer()
        {
            _ClipboardViewerNext = User32.SetClipboardViewer(this.Handle);
        }

        /// Remove this form from the Clipboard Viewer list
        private void UnregisterClipboardViewer()
        {
            User32.ChangeClipboardChain(this.Handle, _ClipboardViewerNext);
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
            object Data = null;
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
                Text = (string)iData.GetData(DataFormats.Text);
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
                Text = String.Join("\n", FileNameList);
                if (iData.GetDataPresent(DataFormats.FileDrop))
                {
                    Type = "file";
                }
            }

            byte[] BinaryBuffer = new byte[0];
            // http://www.cyberforum.ru/ado-net/thread832314.html
            if (Type == "img")
            {
            }
            if (iData.GetDataPresent(DataFormats.Bitmap))
            {
                Type = "img";
                Bitmap Img = iData.GetData(DataFormats.Bitmap) as Bitmap;
                MemoryStream MemoryStream = new MemoryStream();
                Img.Save(MemoryStream, System.Drawing.Imaging.ImageFormat.Png);
                BinaryBuffer = MemoryStream.ToArray();
                if (Text == "")
                {
                    Text = "Size: " + Img.Width + "x" + Img.Height + "\n"
                         + "Color depth: " + Img.PixelFormat + "\n";
                }
            }

            if (Type != "")
            {
                AddClip(BinaryBuffer, HtmlText, RichText, Type, Text, Application, Window, Url);
            }

        }
        void AddClip(byte[] BinaryBuffer, string HtmlText, string RichText, string Type, string Text, string Application, string Window, string Url)
        {
            if (LastText == Text)
            {
                return;
            }
            int Size = Text.Length * 2; // dirty
            int Chars = Text.Length;
            DateTime Created = DateTime.Now;
            String Title = Text.TrimStart();
            Title = Regex.Replace(Title, @"\s+", " ");
            if (Title.Length > 50)
            {
                Title = Title.Substring(0, 50 - 1 - 3) + "...";
            }
            LastId = LastId + 1;
            LastText = Text;
            Size += BinaryBuffer.Length;

            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] BinaryText = Encoding.Unicode.GetBytes(Text);
            md5.TransformBlock(BinaryText, 0, BinaryText.Length, BinaryText, 0);
            byte[] BinaryRichText = Encoding.Unicode.GetBytes(RichText);
            md5.TransformBlock(BinaryRichText, 0, BinaryRichText.Length, BinaryRichText, 0);
            byte[] BinaryHtml = Encoding.Unicode.GetBytes(HtmlText);
            md5.TransformBlock(BinaryHtml, 0, BinaryHtml.Length, BinaryHtml, 0);
            md5.TransformFinalBlock(BinaryBuffer, 0, BinaryBuffer.Length);
            string Hash = Convert.ToBase64String(md5.Hash);

            string sql = "DELETE FROM Clips Where Hash = @Hash";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.Parameters.Add("@Hash", System.Data.DbType.String).Value = Hash;
            command.ExecuteNonQuery();

            sql = "insert into Clips (Id, Title, Text, Application, Window, Created, Type, Binary, Size, Chars, RichText, HtmlText, Used, Url, Hash) "
               + "values (@Id, @Title, @Text, @Application, @Window, @Created, @Type, @Binary, @Size, @Chars, @RichText, @HtmlText, @Used, @Url, @Hash)";

            command = new SQLiteCommand(sql, m_dbConnection);
            command.Parameters.Add("@Id", System.Data.DbType.Int32).Value = LastId;
            command.Parameters.Add("@Title", System.Data.DbType.String).Value = Title;
            command.Parameters.Add("@Text", System.Data.DbType.String).Value = Text;
            command.Parameters.Add("@RichText", System.Data.DbType.String).Value = RichText;
            command.Parameters.Add("@HtmlText", System.Data.DbType.String).Value = HtmlText;
            command.Parameters.Add("@Application", System.Data.DbType.String).Value = Application;
            command.Parameters.Add("@Window", System.Data.DbType.String).Value = Window;
            command.Parameters.Add("@Created", System.Data.DbType.DateTime).Value = Created;
            command.Parameters.Add("@Type", System.Data.DbType.String).Value = Type;
            command.Parameters.Add("@Binary", System.Data.DbType.Binary).Value = BinaryBuffer;
            command.Parameters.Add("@Size", System.Data.DbType.Int32).Value = Size;
            command.Parameters.Add("@Chars", System.Data.DbType.Int32).Value = Chars;
            command.Parameters.Add("@Used", System.Data.DbType.Boolean).Value = false;
            command.Parameters.Add("@Url", System.Data.DbType.String).Value = Url;
            command.Parameters.Add("@Hash", System.Data.DbType.String).Value = Hash;
            command.ExecuteNonQuery();

            //dbDataSet.ClipsRow NewRow = (dbDataSet.ClipsRow)dbDataSet.Clips.NewRow();
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
            //foreach (DataColumn Column in dbDataSet.Clips.Columns)
            //{
            //    if (Column.DataType == System.Type.GetType("System.String") && Column.MaxLength > 0)
            //    {
            //        string NewValue = NewRow[Column.ColumnName] as string;
            //        NewRow[Column.ColumnName] = NewValue.Substring(0, Math.Min(NewValue.Length, Column.MaxLength));
            //    }
            //}
            //dbDataSet.Clips.Rows.Add(NewRow);
            //clipsTableAdapter.Insert(NewRow.Type, NewRow.Text, NewRow.Title, NewRow.Application, NewRow.Window, NewRow.Size, NewRow.Chars, NewRow.Created, NewRow.Binary, NewRow.RichText, NewRow.Id, NewRow.HtmlText, NewRow.Used);
            ClipsNumber++;
            if(ClipsNumber > Properties.Settings.Default.HistoryDepthNumber)
            {
                command.CommandText = "Delete From Clips where Id IN (Select Min(Id) From Clips)";
                command.ExecuteNonQuery();
                ClipsNumber--;
            }
            if (this.Visible)
                UpdateClipBindingSource();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            int i = dataGridView.CurrentRow.Index;
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
                this.hkl = LoadKeyboardLayout(pwszKlid, KeyboardLayoutFlags.KLF_ACTIVATE| KeyboardLayoutFlags.KLF_SUBSTITUTE_OK);
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

        private void SendPaste(bool OnlyText = false)
        {
            CaptureClipboard = false;
            StringCollection LastFilterValues = Properties.Settings.Default.LastFilterValues;
            if (Filter.Text != "" && !LastFilterValues.Contains(Filter.Text))
            {
                LastFilterValues.Insert(0, Filter.Text);
                while (LastFilterValues.Count > 20)
                {
                    LastFilterValues.RemoveAt(LastFilterValues.Count - 1);
                }
                FillFilterItems();
            }

            DataRow CurrentDataRow = ((DataRowView)clipBindingSource.Current).Row;
            string Type = (string)CurrentDataRow["type"];
            object RichText = CurrentDataRow["RichText"];
            object HtmlText = CurrentDataRow["HtmlText"];
            byte[] Binary = CurrentDataRow["Binary"] as byte[];
            DataObject dto = new DataObject();
            if (Type == "rtf" || Type == "text" || Type == "html")
            {
                string Text = (string)CurrentDataRow["Text"];
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
                string[] FileNameList = Text.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
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
            Clipboard.Clear();
            Clipboard.SetDataObject(dto);
            CaptureClipboard = true;

            CultureInfo EnglishCultureInfo = null;
            foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
            {
                if (String.Compare(lang.Culture.TwoLetterISOLanguageName, "en", true)==0)
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

            //dbDataSet.ClipsRow Row = (dbDataSet.ClipsRow)dbDataSet.Clips.Rows[dataGridView.CurrentRow.Index];
            //Row.Used = true;
            //dataAdapter.Update(dbDataSet);

            string sql = "Update Clips set Used =@Used where Id=@Id";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.Parameters.Add("@Id", System.Data.DbType.Int32).Value = CurrentDataRow["Id"];
            command.Parameters.Add("@Used", System.Data.DbType.Boolean).Value = true;
            command.ExecuteNonQuery();

            ((System.Data.DataRowView)dataGridView.CurrentRow.DataBoundItem).Row["Used"] = true;
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
            Filter.Items.Clear();
            foreach (string String in LastFilterValues)
            {
                Filter.Items.Add(String);
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
            { ShowWindow(); }
        }

        private void ShowWindow()
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            AutoGotoLastRow = true;
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.F9))
            {
                ClearFilter_Click();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void dataGridView_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (!IsKeyPassedFromFilterToGrid(e.KeyCode, e.Control) && e.KeyCode != Keys.Delete && e.KeyCode != Keys.Home && e.KeyCode != Keys.End && e.KeyCode != Keys.Enter)
            {
                Filter.Focus();
                sendKey(Filter.Handle, e.KeyData, false, true);
            }
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
            foreach (DataGridViewRow Row in dataGridView.Rows)
            {
                DataRowView DataRowView = (DataRowView)(dataGridView.Rows[Row.Index].DataBoundItem);
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
                if ((bool)DataRowView.Row["Used"])
                {
                    foreach (DataGridViewCell Cell in Row.Cells)
                    {
                        Cell.Style.BackColor = Color.FromArgb(200, 255, 255);
                    }
                }
            }
            dataGridView.Update();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings SettingsForm = new Settings();
            SettingsForm.Owner = this;
            SettingsForm.ShowDialog();
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.Save();
            UnregisterClipboardViewer();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            exitToolStripMenuItem_Click();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form AboutBox = new AboutBox();
            AboutBox.ShowDialog();
        }

        private void Main_Activated(object sender, EventArgs e)
        {
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

    }
}

public sealed class KeyboardHook : IDisposable
{
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
            string HotkeyPresentation = key.ToString();
            if (modifier.ToString() != "")
                HotkeyPresentation = modifier.ToString() + " + " + HotkeyPresentation;
            string ErrorText = "Couldn’t register the hot key " + HotkeyPresentation;
            //throw new InvalidOperationException(ErrorText);
            MessageBox.Show(ErrorText, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// A hot key has been pressed.
    /// </summary>
    public event EventHandler<KeyPressedEventArgs> KeyPressed;

    #region IDisposable Members

    public void Dispose()
    {
        // unregister all the registered hot keys.
        for (int i = _currentId; i > 0; i--)
        {
            UnregisterHotKey(_window.Handle, i);
        }

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