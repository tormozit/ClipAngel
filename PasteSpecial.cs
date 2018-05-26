using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace ClipAngel
{
    public partial class PasteSpecial : Form
    {
        public string OriginalText;
        public string ResultText;
        public bool PasteIntoNewClip;
        public PasteSpecial()
        {
            InitializeComponent();
        }

        private void UpdatePreview()
        {
            string text = OriginalText;
            if (AllLowerCase.Checked)
            {
                text = text.ToLower();
            }
            if (AllUpper.Checked)
            {
                text = text.ToUpper();
            }
            if (SentenceCase.Checked)
            {
                text = text.ToLower();
                var r = new Regex(@"(^[a-zа-яё])|\.\s+(.)", RegexOptions.ExplicitCapture); // Only english and russian letters supported
                text = r.Replace(text, s => s.Value.ToUpper());
            }
            if (ReplaceEOL.Checked)
            {
                text = Regex.Replace(text, @"\r|\n", " ");
            }
            if (checkBoxReplaceTabWithSpaces.Checked)
            {
                int NumberOfSpacesInTab = -1;
                try
                {
                    NumberOfSpacesInTab = Convert.ToInt32(textBoxNumberOfSpacesInTab.Text);
                }
                catch (Exception e)
                {
                }
                if (NumberOfSpacesInTab > 0)
                {
                    text = Regex.Replace(text, @"\t", new String(" "[0], NumberOfSpacesInTab));
                }
            }
            if (NormalizeFreeSpace.Checked)
            {
                text = text.Trim();
                text = Regex.Replace(text, @"[ \f\t\v]+", " ");
            }
            richTextBox1.Text = text;
            ResultText = text;
        }

        private void SpecialPaste_Load(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void NormalizeFreeSpace_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void checkBoxReplaceEOL_CheckedChanged(object sender, EventArgs e)
        {
           UpdatePreview();
        }

        private void CaseNoChange_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void AllUpper_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void AllLowerCase_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void SentenceCase_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            PasteIntoNewClip = checkBoxPasteIntoNewClip.Checked;
        }

        private void checkBoxReplaceTabWithSpaces_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void textBoxNumberOfSpacesInTab_TextChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }
    }
}
