using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipAngel
{
    public class CueComboBox : ComboBox
    {
        //http://www.aaronlerch.com/blog/2007/12/01/watermarked-edit-controls/
        #region PInvoke Helpers

        private static uint CB_SETCUEBANNER = 0x1703;
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(HandleRef hWnd, uint Msg, IntPtr wParam, String lParam);

        #endregion PInvoke Helpers

        #region CueText

        private string _cueText = String.Empty;

        /// <summary>
        /// Gets or sets the text the <see cref="ComboBox"/> will display as a cue to the user.
        /// </summary>
        [Description("The text value to be displayed as a cue to the user.")]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string CueText
        {
            get { return _cueText; }
            set
            {
                if (value == null)
                {
                    value = String.Empty;
                }

                if (!_cueText.Equals(value, StringComparison.CurrentCulture))
                {
                    _cueText = value;
                    UpdateCue();
                    OnCueTextChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when the <see cref="CueText"/> property value changes.
        /// </summary>
        public event EventHandler CueTextChanged;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        protected virtual void OnCueTextChanged(EventArgs e)
        {
            EventHandler handler = CueTextChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion CueText

        #region Overrides

        protected override void OnHandleCreated(EventArgs e)
        {
            UpdateCue();

            base.OnHandleCreated(e);
        }

        #endregion Overrides

        private void UpdateCue()
        {
            // If the handle isn't yet created, 
            // this will be called when it is created
            if (this.IsHandleCreated)
            {
                SendMessage(new HandleRef(this, this.Handle), CB_SETCUEBANNER, IntPtr.Zero, _cueText);
            }
        }
    }
}
