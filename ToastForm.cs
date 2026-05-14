using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClipAngel
{
    /// <summary>
    /// Small toast notification that appears in the tray area for 1 second
    /// </summary>
    public partial class ToastForm : Form
    {
        private Timer closeTimer;
        private Timer fadeInTimer;
        private bool isDisposing = false;

        public string Title
        {
            get { return titleLabel.Text; }
            set { titleLabel.Text = value; }
        }

        public string Message
        {
            get { return messageLabel.Text; }
            set { messageLabel.Text = value; }
        }

        public ToastForm()
        {
            InitializeComponent();
            SetupTimers();
        }
        
        public ToastForm(string title, string message) : this()
        {
            this.Title = title;
            this.Message = message;
            
            PositionForm();
        }

        private void PositionForm()
        {
            // Position in tray area (bottom right corner)
            Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(
                workingArea.Right - this.Width - 10,
                workingArea.Bottom - this.Height - 10
            );
        }

        private void SetupTimers()
        {
            // Set up close timer for 2 seconds
            closeTimer = new Timer { Interval = 2000 };
            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                if (!isDisposing)
                {
                    this.Close();
                }
            };
            
            // Enable fade effect
            this.Opacity = 0;
            fadeInTimer = new Timer { Interval = 50 };
            fadeInTimer.Tick += (s, e) =>
            {
                if (!isDisposing && this.Opacity < 1)
                {
                    this.Opacity += 0.1;
                }
                else
                {
                    fadeInTimer.Stop();
                    if (!isDisposing)
                    {
                        closeTimer.Start();
                    }
                }
            };
            
            if (!DesignMode)
            {
                fadeInTimer.Start();
            }
        }
        
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80; // WS_EX_TOOLWINDOW - prevents showing in taskbar
                return cp;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                PositionForm();
            }
        }
    }
}
