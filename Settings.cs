using Microsoft.Win32;
using System;
using System.Configuration;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace ClipAngel
{
    public partial class Settings : Form
    {
        class WrongNumber : System.Exception {
            public WrongNumber() { }
            public WrongNumber(string message) : base(message) { }
        }
        

        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void Settings_Load(object sender = null, EventArgs e = null)
        {
            HistoryDepthNumber.Text = Properties.Settings.Default.HistoryDepthNumber.ToString();
            MaxClipSizeKB.Text = Properties.Settings.Default.MaxClipSizeKB.ToString();

            textBoxTextCompareApplication.Text = Properties.Settings.Default.TextCompareApplication;
            textBoxDefaultFont.Font = Properties.Settings.Default.Font;
            textBoxDefaultFont.Text = GetFontPresentation(textBoxDefaultFont.Font);
            GlobalHotkeyShow.Text = Properties.Settings.Default.HotkeyShow;
            GlobalHotkeyShowFavorites.Text = Properties.Settings.Default.HotKeyShowFavorites;
            GlobalHotkeyIncrementalPaste.Text = Properties.Settings.Default.HotkeyIncrementalPaste;
            Language.Text = Properties.Settings.Default.Language;
            cultureManager1.UICulture = new CultureInfo((Owner as Main).Locale);
            checkBoxAutostart.Checked = Properties.Settings.Default.Autostart;
            checkBoxWindowAutoPosition.Checked = Properties.Settings.Default.WindowAutoPosition;
            checkBoxMoveCopiedClipToTop.Checked = Properties.Settings.Default.MoveCopiedClipToTop;
            checkBoxShowSizeColumn.Checked = Properties.Settings.Default.ShowVisualWeightColumn;
            checkBoxClipListSimpleDraw.Checked = Properties.Settings.Default.ClipListSimpleDraw;
            checkBoxAutoCheckUpdate.Checked = Properties.Settings.Default.AutoCheckForUpdate;
            checkBoxClearFiltersOnClose.Checked = Properties.Settings.Default.ClearFiltersOnClose;
            checkBoxShowApplicationIconColumn.Checked = Properties.Settings.Default.ShowApplicationIconColumn;

            NumberOfClips.Text = (Owner as Main).ClipsNumber.ToString();
            textBoxUserSettingsPath.Text = (Owner as Main).UserSettingsPath;
            textBoxDatabaseSize.Text = ((new FileInfo((Owner as Main).DbFileName)).Length / (1024 * 1024)).ToString();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            int numValue;
            Properties.Settings.Default.TextCompareApplication = textBoxTextCompareApplication.Text;
            Properties.Settings.Default.Font = textBoxDefaultFont.Font;
            Properties.Settings.Default.ShowApplicationIconColumn = checkBoxShowApplicationIconColumn.Checked;
            Properties.Settings.Default.ClearFiltersOnClose = checkBoxClearFiltersOnClose.Checked;
            Properties.Settings.Default.AutoCheckForUpdate = checkBoxAutoCheckUpdate.Checked;
            Properties.Settings.Default.ClipListSimpleDraw = checkBoxClipListSimpleDraw.Checked;
            Properties.Settings.Default.MoveCopiedClipToTop = checkBoxMoveCopiedClipToTop.Checked;
            Properties.Settings.Default.ShowVisualWeightColumn = checkBoxShowSizeColumn.Checked;
            Properties.Settings.Default.WindowAutoPosition = checkBoxWindowAutoPosition.Checked;
            Properties.Settings.Default.HotkeyShow = GlobalHotkeyShow.Text;
            Properties.Settings.Default.HotKeyShowFavorites = GlobalHotkeyShowFavorites.Text;
            Properties.Settings.Default.HotkeyIncrementalPaste = GlobalHotkeyIncrementalPaste.Text;
            Properties.Settings.Default.Language = Language.Text;
            if (Int32.TryParse(HistoryDepthNumber.Text, out numValue))
            { Properties.Settings.Default.HistoryDepthNumber = numValue; }
            else
            { throw new WrongNumber("Wrong number \"" + HistoryDepthNumber.Text + "\""); }

            if (Int32.TryParse(MaxClipSizeKB.Text, out numValue))
            { Properties.Settings.Default.MaxClipSizeKB = numValue; }
            else
            { throw new WrongNumber("Wrong number \"" + MaxClipSizeKB.Text + "\""); }

            string ExePath = System.Windows.Forms.Application.ExecutablePath;
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            string Keyname = Application.ProductName;
            try
            {
                if (checkBoxAutostart.Checked)
                {
                    string CommandLine = Application.ExecutablePath + " /m";
                    if ((Owner as Main).PortableMode)
                        CommandLine += " /p";
                    reg.SetValue(Keyname, CommandLine);
                }
                else
                    reg.DeleteValue(Keyname);
                reg.Close();
                Properties.Settings.Default.Autostart = checkBoxAutostart.Checked;
            }
            catch
            {
            }

            Properties.Settings.Default.Save();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void HotkeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (false
                || e.KeyCode == Keys.ControlKey
                || e.KeyCode == Keys.ShiftKey
                || e.KeyCode == Keys.Menu)
            {
                e.Handled = true;
                return;
            }
            string HotkeyTitle = "";
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                HotkeyTitle = "No";
            else
            {
                if (e.Control)
                    HotkeyTitle += Keys.Control.ToString() + " + ";
                if (e.Alt)
                    HotkeyTitle += Keys.Alt.ToString() + " + ";
                if (e.Shift)
                    HotkeyTitle += Keys.Shift.ToString() + " + ";
                HotkeyTitle += e.KeyCode.ToString();
            }
            (sender as TextBox).Text = HotkeyTitle;
            e.Handled = true;
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {

        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            string question = (Owner as Main).CurrentLangResourceManager.GetString("QuestionResetSettings");
            if (DialogResult.Yes == MessageBox.Show(question, Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation))
            {
                //Properties.Settings.Default.Reset(); // Not working, it seem reason is type System.Collections.Specialized.StringCollection
                foreach (SettingsProperty settingsKey in Properties.Settings.Default.Properties)
                {
                    var value = Properties.Settings.Default[settingsKey.Name];
                    object newValue;
                    try
                    {
                        newValue = Convert.ChangeType(settingsKey.DefaultValue, value.GetType());
                    }
                    catch
                    {
                        // All object types can not be deserialized. why?
                        continue;
                    }
                    if (newValue != null)
                        Properties.Settings.Default[settingsKey.Name] = newValue;
                }
                Settings_Load();
            }
        }

        private void DefaultFont_DoubleClick(object sender, EventArgs e)
        {
            fontDialog1.Font = textBoxDefaultFont.Font;
            if (fontDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxDefaultFont.Font = fontDialog1.Font;
                textBoxDefaultFont.Text = GetFontPresentation(fontDialog1.Font);
            }
        }

        string GetFontPresentation(Font font)
        {
            string result;
            result = font.Name + " " + font.Size;
            return result;
        }

        private void textBoxTextCompareApplication_DoubleClick(object sender, EventArgs e)
        {
            openFileDialog1.FileName = textBoxTextCompareApplication.Text;
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.Filter = "|*.exe";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                textBoxTextCompareApplication.Text = openFileDialog1.FileName;
        }
    }
}
