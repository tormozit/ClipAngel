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
    public partial class HotkeyEditorForm : Form
    {
        public HotkeyEditorForm()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void GlobalHotkeyShow_KeyDown(object sender, KeyEventArgs e)
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
            e.SuppressKeyPress = true; // Anti beep
        }

        private void HotkeyEditorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ((Main)Owner).RegisterHotKeys();
        }

        private void HotkeyEditorForm_Load(object sender, EventArgs e)
        {
            ((Main)Owner).keyboardHook.UnregisterHotKeys();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }
    }
}
