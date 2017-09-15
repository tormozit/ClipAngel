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
    public partial class Tips : Form
    {
        public Tips()
        {
            InitializeComponent();
            labelTip.Text = labelTip.Text.Replace("<Hotkey>", Properties.Settings.Default.GlobalHotkeyOpenLast);
        }

        private void Tips_Load(object sender, EventArgs e)
        {

        }

        private void Tips_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.ShowTipsOnStart = !DontShowTipsOnStart.Checked;
        }

        private void Tips_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void labelTip_Click(object sender, EventArgs e)
        {

        }
    }
}
