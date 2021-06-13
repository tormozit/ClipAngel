using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipAngel
{
    // http://stackoverflow.com/a/22164138/4085971
    public class ZoomablePictureBox : System.Windows.Forms.UserControl, ISupportInitialize
    {
        private System.Windows.Forms.PictureBox PicBox;
        private Panel OuterPanel;
        private Container components = null;
        private string m_sPicName = "";
        private PictureBoxSizeMode zoomMode;
        public new event EventHandler DoubleClick;
        public event EventHandler ZoomChanged;

        private double ZoomDelta = 1.1;   // = 10% smaller or larger
        private int MINMAX = 5;             // 5 times bigger or smaller than the ctrl


        #region Designer generated code!

        private void InitializeComponent()
        {
            this.PicBox = new System.Windows.Forms.PictureBox();
            this.OuterPanel = new System.Windows.Forms.Panel();
            this.OuterPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // PicBox
            // 
            this.PicBox.Location = new System.Drawing.Point(0, 0);
            this.PicBox.Name = "PicBox";
            this.PicBox.TabIndex = 3;
            this.PicBox.TabStop = false;
            // 
            // OuterPanel
            // 
            this.OuterPanel.AutoScroll = true;
            this.OuterPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.OuterPanel.Controls.Add(this.PicBox);
            this.OuterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OuterPanel.Location = new System.Drawing.Point(0, 0);
            this.OuterPanel.Name = "OuterPanel";
            //this.OuterPanel.Size = new System.Drawing.Size(210, 190);
            this.OuterPanel.TabIndex = 4;
            // 
            // PictureBox
            // 
            this.Controls.Add(this.OuterPanel);
            this.Name = "PictureBox";
            //this.Size = new System.Drawing.Size(210, 190);
            this.OuterPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        public ZoomablePictureBox()
        {
            this.zoomMode = PictureBoxSizeMode.Zoom;
            InitializeComponent();
            InitCtrl(); // my special settings for the ctrl
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            PicBox.BackColor = Color.Transparent;
        }

        private Point _StartPoint;

        void PicBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _StartPoint = e.Location;
        }

        void PicBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point changePoint = new Point(e.Location.X - _StartPoint.X,
                    e.Location.Y - _StartPoint.Y);
                OuterPanel.AutoScrollPosition = new Point(-OuterPanel.AutoScrollPosition.X - changePoint.X,
                    -OuterPanel.AutoScrollPosition.Y - changePoint.Y);
            }
        }

        private Image _image;
        public Image Image
        {
            get { return _image; }
            set
            {
                if (null != value)
                {
                    try
                    {
                        PicBox.Image = value;
                        _image = value;
                    }
                    catch (OutOfMemoryException ex)
                    {
                        RedCross();
                    }
                }
                else
                {
                    RedCross();
                }
            }
        }

        /// <summary>
        /// Property to select the picture which is displayed in the picturebox. If the 
        /// file doesn´t exist or we receive an exception, the picturebox displays 
        /// a red cross.
        /// </summary>
        /// <value>Complete filename of the picture, including path information</value>
        /// <remarks>Supported fileformat: *.gif, *.tif, *.jpg, *.bmp</remarks>
        /// 

        [Browsable(false)]
        public string Picture
        {
            get { return m_sPicName; }
            set
            {
                if (null != value)
                {
                    if (System.IO.File.Exists(value))
                    {
                        try
                        {
                            PicBox.Image = Image.FromFile(value);
                            m_sPicName = value;
                        }
                        catch (OutOfMemoryException ex)
                        {
                            RedCross();
                        }
                    }
                    else
                    {
                        RedCross();
                    }
                }
            }
        }

        /// <summary>
        /// Set the frametype of the picturbox
        /// </summary>
        [Browsable(false)]
        public BorderStyle Border
        {
            get { return OuterPanel.BorderStyle; }
            set { OuterPanel.BorderStyle = value; }
        }

        /// <summary>
        /// Special settings for the picturebox ctrl
        /// </summary>
        private void InitCtrl()
        {
            PicBox.SizeMode = zoomMode;
            PicBox.Location = new Point(0, 0);
            OuterPanel.Dock = DockStyle.Fill;
            OuterPanel.Cursor = System.Windows.Forms.Cursors.NoMove2D;
            OuterPanel.AutoScroll = true;
            //OuterPanel.MouseEnter += new EventHandler(PicBox_MouseEnter); // It will steal focus without reason. It's very bad
            //PicBox.MouseEnter += new EventHandler(PicBox_MouseEnter); // It will steal focus without reason. It's very bad
            PicBox.DoubleClick += new EventHandler(PicBox_DoubleClick);
            PicBox.MouseDown += new MouseEventHandler(PicBox_MouseDown);
            PicBox.MouseMove += new MouseEventHandler(PicBox_MouseMove);
            OuterPanel.MouseWheel += new MouseEventHandler(PicBox_MouseWheel);
        }

        /// <summary>
        /// Create a simple red cross as a bitmap and display it in the picturebox
        /// </summary>
        private void RedCross()
        {
            //Bitmap bmp = new Bitmap(OuterPanel.Width, OuterPanel.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            ////Graphics gr;
            ////gr = Graphics.FromImage(bmp);
            ////Pen pencil = new Pen(Color.Red, 5);
            ////gr.DrawLine(pencil, 0, 0, OuterPanel.Width, OuterPanel.Height);
            ////gr.DrawLine(pencil, 0, OuterPanel.Height, OuterPanel.Width, 0);
            //PicBox.Image = bmp;
            ////gr.Dispose();
        }

        #region Zooming Methods

        /// <summary>
        /// Make the PictureBox dimensions larger to effect the Zoom.
        /// </summary>
        /// <remarks>Maximum 5 times bigger</remarks>
        private void ZoomIn()
        {
            if (PicBox.Width < (MINMAX * Image.Width))
            {
                int newWidth, newHeight;
                double zoomFactor = ZoomFactor();
                if (zoomFactor >= 1 || -(zoomFactor - 1) >= (ZoomDelta - 1))
                {
                    newWidth = Convert.ToInt32(PicBox.Width * ZoomDelta);
                    newHeight = Convert.ToInt32(PicBox.Height * ZoomDelta);
                }
                else
                {
                    newWidth = Image.Width;
                    newHeight = Image.Height;
                }
                PicBox.SizeMode = zoomMode;
                PicBox.Width = newWidth;
                PicBox.Height = newHeight;
                AfterZoomChange();
            }
        }

        /// <summary>
        /// Make the PictureBox dimensions smaller to effect the Zoom.
        /// </summary>
        /// <remarks>Minimum 5 times smaller</remarks>
        private void ZoomOut()
        {
            if (PicBox.Width > Math.Min(Image.Width, 30) && PicBox.Height > Math.Min(Image.Height, 30))
            {
                int newWidth, newHeight;
                double zoomFactor = ZoomFactor();
                if (zoomFactor <= 1 || (zoomFactor - 1) >= (ZoomDelta - 1))
                {
                    newWidth = Convert.ToInt32(PicBox.Width / ZoomDelta);
                    newHeight = Convert.ToInt32(PicBox.Height / ZoomDelta);
                }
                else
                {
                    newWidth = Image.Width;
                    newHeight = Image.Height;
                }
                PicBox.SizeMode = zoomMode;
                PicBox.Width = newWidth;
                PicBox.Height = newHeight;
                AfterZoomChange();
            }
        }

        private void AfterZoomChange()
        {
            PicBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            ZoomChanged(this, new EventArgs());
        }

        public void ZoomFitInside()
        {
            //PicBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            PicBox.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            if (OuterPanel.Width < Image.Width)
            {
                PicBox.Width = OuterPanel.Width;
                PicBox.Anchor = PicBox.Anchor | AnchorStyles.Right;
            }
            else
                PicBox.Width = Image.Width;
            if (OuterPanel.Height < Image.Height)
            {
                PicBox.Height = OuterPanel.Height;
                PicBox.Anchor = PicBox.Anchor | AnchorStyles.Bottom;
            }
            else
                PicBox.Height = Image.Height;
            ZoomChanged(this, new EventArgs());
        }

        public void ZoomOriginalSize()
        {
            PicBox.Width = Image.Width;
            PicBox.Height = Image.Height;
            AfterZoomChange();
        }

        public double ZoomFactor()
        {
            if (Image != null)
                return Math.Min((double) PicBox.Width / Image.Width, (double) PicBox.Height / Image.Height);
            else
                return 1;
        }
        #endregion

        #region Mouse events

        /// <summary>
        /// We use the mousewheel to zoom the picture in or out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PicBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                ZoomIn();
            }
            else
            {
                ZoomOut();
            }
        }

        private void PicBox_DoubleClick(object sender, EventArgs e)
        {
            DoubleClick(this, e);
        }

        /// <summary>
        /// Make sure that the PicBox have the focus, otherwise it doesn´t receive 
        /// mousewheel events !.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PicBox_MouseEnter(object sender, EventArgs e)
        {
            if (PicBox.Focused == false)
            {
                PicBox.Focus();
            }
        }

        #endregion

        #region Disposing

        /// <summary>
        /// Die verwendeten Ressourcen bereinigen.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        public void BeginInit()
        {
            //throw new NotImplementedException();
        }

        public void EndInit()
        {
            //throw new NotImplementedException();
        }
    }

}
