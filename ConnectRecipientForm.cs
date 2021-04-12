using Newtonsoft.Json;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipAngel
{
    public partial class ConnectRecipientForm : Form
    {
        public ConnectRecipientForm()
        {
            InitializeComponent();
        }

        private async void buttonReset_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show(this, Properties.Resources.ConfirmResetChannel, Properties.Resources.Confirmation, MessageBoxButtons.YesNo);
            if (confirmResult != DialogResult.Yes)
                return;
            Main owner = (Main)Owner;
            await owner.CreateSendChannel();
            RefreshRecipients();
            checkBoxShow_CheckedChanged();
        }

        private async void buttonRefresh_Click(object sender, EventArgs e)
        {
            await RefreshRecipients();
        }

        private async Task RefreshRecipients()
        {
            Main owner = (Main)Owner;
            var array = await owner.GetChannelRecipients();
            string recipients = "";
            int Quantity = 0;
            if (array != null)
            {
                foreach (var item in array)
                {
                    if (!String.IsNullOrEmpty(recipients))
                        recipients += ", ";
                    recipients = recipients + item.Name;
                    Quantity++;
                }
            }
            textBoxRecipients.Text = recipients;
            QuantityOfRecipients.Text = Quantity.ToString();
        }

        private void ConnectRecipientForm_Load(object sender, EventArgs e)
        {
            Main owner = (Main)Owner;
            textBoxChannelName.Text = owner.CurrentSendChannel();
            textBoxSenderName.Text = Environment.MachineName;
            RefreshRecipients();
        }

        private void checkBoxShow_CheckedChanged(object sender = null, EventArgs e = null)
        {
            if (checkBoxShow.Checked)
            {
                Main owner = (Main)Owner;
                textBoxChannelName.Text = owner.CurrentSendChannel();
                AESKey key = owner.channelEncryptionKey();
                var data = new
                {
                    channel = textBoxChannelName.Text,
                    senderName = Environment.MachineName,
                    key = key.key
                };
                string dataString = JsonConvert.SerializeObject(data);
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(dataString, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);
                pictureBox.Image = qrCodeImage;
                pictureBox.Visible = true;
            }
            else
            {
                pictureBox.Visible = false;
            }
        }

        private void ConnectRecipientForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void linkClipoid_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://play.google.com/store/apps/details?id=com.clipangel.app");
        }
    }
}
