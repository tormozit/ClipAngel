using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private void Settings_Load(object sender, EventArgs e)
        {
            HistoryDepthNumber.Text = Properties.Settings.Default.HistoryDepthNumber.ToString();
            MaxClipSizeKB.Text = Properties.Settings.Default.MaxClipSizeKB.ToString();

            GlobalHotkeyShow.Text = Properties.Settings.Default.HotkeyShow.ToString();
            GlobalHotkeyIncrementalPaste.Text = Properties.Settings.Default.HotkeyIncrementalPaste.ToString();
            Language.Text = Properties.Settings.Default.Language.ToString();
            NumberOfClips.Text = (Owner as Main).ClipsNumber.ToString();
            cultureManager1.UICulture = new CultureInfo(Main.Locale);
            checkBoxAutostart.Checked = Properties.Settings.Default.Autostart;
            checkBoxMoveCopiedClipToTop.Checked = Properties.Settings.Default.MoveCopiedClipToTop;
            checkBoxShowSizeColumn.Checked = Properties.Settings.Default.ShowVisibleSizeColumn;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int numValue;
            Properties.Settings.Default.MoveCopiedClipToTop = checkBoxMoveCopiedClipToTop.Checked;
            Properties.Settings.Default.ShowVisibleSizeColumn = checkBoxShowSizeColumn.Checked;
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
            string Keyname = "Clip Angel";
            try
            {
                if (checkBoxAutostart.Checked)
                    reg.SetValue(Keyname, Application.ExecutablePath + " /m");
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
            if (e.KeyCode == Keys.Delete)
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
        }

    }
}
