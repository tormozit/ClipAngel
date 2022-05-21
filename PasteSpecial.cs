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
using System.Globalization;

namespace ClipAngel
{
    public partial class PasteSpecial : Form
    {
        string OriginalText;
        public string ResultText;
        public bool PasteIntoNewClip;
        public PasteSpecial(Main Owner)
        {
            this.Owner = Owner;
            InitializeComponent();
            DelimiterForTextJoin.Text = Properties.Settings.Default.DelimiterForTextJoin.ToString();
        }

        private void UpdatePreview()
        {
            string Dummy = "";
            OriginalText = (Owner as Main).GetSelectedTextOfClips(ref Dummy, PasteMethod.Null, DelimiterForTextJoin.Text);
            string text = OriginalText;
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            if (AllLowerCase.Checked)
            {
                text = text.ToLower();
            }
            if (AllUpperCase.Checked)
            {
                text = text.ToUpper();
            }
            if (SentenceCase.Checked)
            {
                text = text.ToLower();
                var r = new Regex(@"(^[a-zа-яё])|\.\s+(.)", RegexOptions.ExplicitCapture); // Only english and russian letters supported
                text = r.Replace(text, s => s.Value.ToUpper());
            }
             if (UpperCamelCase.Checked)
            {
                text = textInfo.ToTitleCase(text);
                text = Regex.Replace(text, @"\s", @"");
            }
             if (LowerCamelCase.Checked)
            {
                text = textInfo.ToTitleCase(text);
                text = Char.ToLowerInvariant(text[0]) + text.Substring(1);
                text = Regex.Replace(text, @"\s", @"");
            }
             if (FromCamelCase.Checked)
            {
                text = SplitCamelCase(text);
                text = text.ToLower();
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
            if (NormalizeEndOfLines.Checked)
            {
                text = text.Trim();
                text = Regex.Replace(text, @"^\s*[\n\r]+", "");
                text = Regex.Replace(text, @"[\n\r]+\s*[\n\r]+", Environment.NewLine);
            }
            richTextBox1.Text = text;
            ResultText = text;
        }

        private void SpecialPaste_Load(object sender, EventArgs e)
        {
            CaseNoChange.Checked = Properties.Settings.Default.CaseConversionMode == 0;
            AllUpperCase.Checked = Properties.Settings.Default.CaseConversionMode == 1;
            AllLowerCase.Checked = Properties.Settings.Default.CaseConversionMode == 2;
            SentenceCase.Checked = Properties.Settings.Default.CaseConversionMode == 3;
            UpperCamelCase.Checked = Properties.Settings.Default.CaseConversionMode == 4;
            LowerCamelCase.Checked = Properties.Settings.Default.CaseConversionMode == 5;
            FromCamelCase.Checked = Properties.Settings.Default.CaseConversionMode == 6;
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
            Properties.Settings.Default.DelimiterForTextJoin = DelimiterForTextJoin.Text;
            if (CaseNoChange.Checked)
                Properties.Settings.Default.CaseConversionMode = 0;
            if (AllUpperCase.Checked)
                Properties.Settings.Default.CaseConversionMode = 1;
            if (AllLowerCase.Checked)
                Properties.Settings.Default.CaseConversionMode = 2;
            if (SentenceCase.Checked)
                Properties.Settings.Default.CaseConversionMode = 3;
            if (UpperCamelCase.Checked)
                Properties.Settings.Default.CaseConversionMode = 4;
            if (LowerCamelCase.Checked)
                Properties.Settings.Default.CaseConversionMode = 5;
            if (FromCamelCase.Checked)
                Properties.Settings.Default.CaseConversionMode = 6;
        }

        private void checkBoxReplaceTabWithSpaces_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void textBoxNumberOfSpacesInTab_TextChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void UpperCamelCase_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void LowerCamelCase_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void NormalizeEndOfLines_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        public static string SplitCamelCase(string input)
        {
            int i = 0;
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(string.Join(string.Empty, input
                    .Select(c => new
                    {
                        Character = c.ToString(),
                        Start = i++ == 0
                                || (Char.IsUpper(input[i - 1])
                                    && (!Char.IsUpper(input[i - 2])
                                        || (i < input.Length && !Char.IsUpper(input[i]))))
                    })
                    .Select(x => x.Start ? " " + x.Character : x.Character)
                    .ToArray()))
                .Trim(); ;
            ;
        }

        private void DelimiterForTextJoin_TextChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }
    }
}

