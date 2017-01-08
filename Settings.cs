using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            Autostart.Checked = Properties.Settings.Default.Autostart;
            GlobalHotkeyShowWindow.Text = Properties.Settings.Default.HotkeyShowWindow.ToString();
            NumberOfClips.Text = (Owner as Main).ClipsNumber.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int numValue;

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
                if (Autostart.Checked)
                    reg.SetValue(Keyname, Application.ExecutablePath + " /m");
                else
                    reg.DeleteValue(Keyname);

                reg.Close();
                Properties.Settings.Default.Autostart = Autostart.Checked;
            }
            catch
            {
            }

            Properties.Settings.Default.Save();
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
