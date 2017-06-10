using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace ClipAngel
{
    partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
        }

        private void AboutBox_Load(object sender, EventArgs e)
        {
            cultureManager1.UICulture = new CultureInfo((Owner as Main).Locale);
            this.labelProductName.Text = (Owner as Main).AssemblyProduct;
            this.labelVersion.Text = Properties.Resources.VersionValue;
            this.labelCopyright.Text = (Owner as Main).AssemblyCopyright;
        }

        private void linkLabelEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:" + Properties.Resources.Email);
        }

        private void linkLabelWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(Properties.Resources.Website);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start((Owner as Main).CurrentLangResourceManager.GetString("Donate"));
        }
    }
}
