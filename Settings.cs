using Microsoft.Win32;
using System;
using System.Configuration;
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

            GlobalHotkeyShow.Text = Properties.Settings.Default.HotkeyShow.ToString();
            GlobalHotkeyIncrementalPaste.Text = Properties.Settings.Default.HotkeyIncrementalPaste.ToString();
            Language.Text = Properties.Settings.Default.Language.ToString();
            cultureManager1.UICulture = new CultureInfo((Owner as Main).Locale);
            checkBoxAutostart.Checked = Properties.Settings.Default.Autostart;
            checkBoxWindowAutoPosition.Checked = Properties.Settings.Default.WindowAutoPosition;
            checkBoxMoveCopiedClipToTop.Checked = Properties.Settings.Default.MoveCopiedClipToTop;
            checkBoxShowSizeColumn.Checked = Properties.Settings.Default.ShowVisualWeightColumn;
            checkBoxClipListSimpleDraw.Checked = Properties.Settings.Default.ClipListSimpleDraw;
            checkBoxAutoCheckUpdate.Checked = Properties.Settings.Default.AutoCheckForUpdate;
            checkBoxClearFiltersOnClose.Checked = Properties.Settings.Default.ClearFiltersOnClose;

            NumberOfClips.Text = (Owner as Main).ClipsNumber.ToString();
            textBoxUserSettingsPath.Text = (Owner as Main).UserSettingsPath;
            textBoxDatabaseSize.Text = ((new FileInfo((Owner as Main).DbFileName)).Length / (1024 * 1024)).ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int numValue;
            Properties.Settings.Default.ClearFiltersOnClose = checkBoxClearFiltersOnClose.Checked;
            Properties.Settings.Default.AutoCheckForUpdate = checkBoxAutoCheckUpdate.Checked;
            Properties.Settings.Default.ClipListSimpleDraw = checkBoxClipListSimpleDraw.Checked;
            Properties.Settings.Default.MoveCopiedClipToTop = checkBoxMoveCopiedClipToTop.Checked;
            Properties.Settings.Default.ShowVisualWeightColumn = checkBoxShowSizeColumn.Checked;
            Properties.Settings.Default.WindowAutoPosition = checkBoxWindowAutoPosition.Checked;
            Properties.Settings.Default.HotkeyShow = GlobalHotkeyShow.Text;
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void HotkeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (false
                || e.KeyCode == Keys.ControlKey
                || e.KeyCode == Keys.ShiftKey
                || e.KeyCode == Keys.Menu)
            { return; }
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
                    var newValue = Convert.ChangeType(settingsKey.DefaultValue, value.GetType());
                    if (newValue != null)
                        Properties.Settings.Default[settingsKey.Name] = newValue;
                }
                Settings_Load();
            }
        }
    }
}
