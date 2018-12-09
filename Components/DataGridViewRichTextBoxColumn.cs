using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.ComponentModel;

// https://www.codeproject.com/articles/31823/richtextbox-cell-in-a-datagridview

namespace ClipAngel
{
    public class DataGridViewRichTextBoxColumn : DataGridViewColumn
    {
        public DataGridViewRichTextBoxColumn()
            : base(new DataGridViewRichTextBoxCell())
        { 
        }

        public override DataGridViewCell CellTemplate
        {
            get
            {
                return base.CellTemplate;
            }
            set
            {
                if (!(value is DataGridViewRichTextBoxCell))
                    throw new InvalidCastException("CellTemplate must be a DataGridViewRichTextBoxCell");

                base.CellTemplate = value;  
            }
        }
    }

    public class DataGridViewRichTextBoxCell : DataGridViewImageCell
    {
        private static readonly RichTextBox _editingControl = new RichTextBox();

        //public override Type EditType
        //{
        //    get
        //    {
        //        return typeof(DataGridViewRichTextBoxEditingControl);
        //    }
        //}

        public override Type ValueType
        {
            get
            {
                return typeof(string);
            }
            set
            {
                base.ValueType = value;
            }
        }

        public override Type FormattedValueType
        {
            get
            {
                return typeof(string);
            }
        }

        private static bool SetRichTextBoxText(RichTextBox ctl, string text)
        {
            try
            {
                ctl.Rtf = text;
                return false;
            }
            catch (ArgumentException)
            {
                ctl.Text = text;
                return true;
            }
        }

        private Image GetRtfImage(int rowIndex, object value, bool selected, DataGridViewCellStyle cellStyle)
        {
            Size cellSize = GetSize(rowIndex);
            if (cellSize.Width < 1 || cellSize.Height < 1)
                return null;
            RichTextBox ctl = _editingControl;
            ctl.Size = GetSize(rowIndex);
            if (SetRichTextBoxText(ctl, Convert.ToString(value)))
                ctl.Font = cellStyle.Font;
            // Print the content of RichTextBox to an image.
            Size imgSize = new Size(cellSize.Width - 1, cellSize.Height - 1);
            Image rtfImg = null;
            ctl.DetectUrls = false; // todo customize
            ctl.WordWrap = false; // Printer does not respect this 
            ctl.Margin = new Padding(0);
            if (selected)
            {
                // Selected cell state
                ctl.BackColor = DataGridView.DefaultCellStyle.SelectionBackColor;
                ctl.ForeColor = DataGridView.DefaultCellStyle.SelectionForeColor;

                // tormozit: fix backgound color for cell
                //ctl.BackColor = cellStyle.SelectionBackColor;
                //ctl.ForeColor = cellStyle.SelectionForeColor;
            }
            else
            {
                // tormozit: fix backgound color for cell
                ctl.BackColor = cellStyle.BackColor;
                ctl.ForeColor = cellStyle.ForeColor;
            }
            //ctl.Font = cellStyle.Font; // It ignores font from in rtf text

            // Print image
            int extraWidth = 200; // To prevent last word cutting off, WordWrap is not respected by printer and is always ON
            rtfImg = RichTextBoxPrinter.Print(ctl, imgSize.Width + extraWidth, imgSize.Height);
                
            // Restore RichTextBox
            ctl.BackColor = DataGridView.DefaultCellStyle.BackColor;
            ctl.ForeColor = DataGridView.DefaultCellStyle.ForeColor;

            return rtfImg;
        }

        //public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        //{
        //    base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

        //    RichTextBox ctl = DataGridView.EditingControl as RichTextBox;

        //    if (ctl != null)
        //    {
        //        SetRichTextBoxText(ctl, Convert.ToString(initialFormattedValue));
        //    }
        //}

        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        {
            return value;
        }

        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, null, null, errorText, cellStyle, advancedBorderStyle, paintParts);
            Image img = GetRtfImage(rowIndex, value, base.Selected, cellStyle);
            int textWitdh = cellBounds.Right - cellBounds.Left;
            int horizontalSpaceTextImage = 3;
            int verticalSpace = (int) Math.Max(11 - cellStyle.Font.Size, 0);

            // TODO customize
            DataGridViewCell SampleCell = DataGridView.Rows[rowIndex].Cells["imageSample"];
            if (SampleCell.Value != null)
            {
                textWitdh -= SampleCell.Size.Width;
            }
            if (img != null)
            {
                Rectangle rect = new Rectangle(0, 0, textWitdh - horizontalSpaceTextImage, cellBounds.Bottom - 2);
                graphics.DrawImage(img, cellBounds.Left + 3, cellBounds.Top + verticalSpace, rect, GraphicsUnit.Pixel);
            }

            if (SampleCell.Value != null)
            {
                Image sample = (Image) SampleCell.Value;
                Pen borderPen = new Pen(Color.Green);
                graphics.DrawLine(borderPen, cellBounds.Left + textWitdh + horizontalSpaceTextImage - 2, cellBounds.Top + 1, cellBounds.Left + textWitdh + horizontalSpaceTextImage - 2, cellBounds.Bottom - 1);
                graphics.DrawImage(sample, cellBounds.Left + textWitdh + horizontalSpaceTextImage, cellBounds.Top + 1);
            }
        }

        #region Handlers of edit events, copyied from DataGridViewTextBoxCell

        private byte flagsState;

        protected override void OnEnter(int rowIndex, bool throughMouseClick)
        {
            base.OnEnter(rowIndex, throughMouseClick);

            if ((base.DataGridView != null) && throughMouseClick)
            {
                this.flagsState = (byte)(this.flagsState | 1);
            }
        }

        protected override void OnLeave(int rowIndex, bool throughMouseClick)
        {
            base.OnLeave(rowIndex, throughMouseClick);

            if (base.DataGridView != null)
            {
                this.flagsState = (byte)(this.flagsState & -2);
            }
        }

        //protected override void OnMouseClick(DataGridViewCellMouseEventArgs e)
        //{
        //    base.OnMouseClick(e);
        //    if (base.DataGridView != null)
        //    {
        //        Point currentCellAddress = base.DataGridView.CurrentCellAddress;

        //        if (((currentCellAddress.X == e.ColumnIndex) && (currentCellAddress.Y == e.RowIndex)) && (e.Button == MouseButtons.Left))
        //        {
        //            if ((this.flagsState & 1) != 0)
        //            {
        //                this.flagsState = (byte)(this.flagsState & -2);
        //            }
        //            else if (base.DataGridView.EditMode != DataGridViewEditMode.EditProgrammatically)
        //            {
        //                base.DataGridView.BeginEdit(false);
        //            }
        //        }
        //    }
        //}

        //public override bool KeyEntersEditMode(KeyEventArgs e)
        //{
        //    return (((((char.IsLetterOrDigit((char)((ushort)e.KeyCode)) && ((e.KeyCode < Keys.F1) || (e.KeyCode > Keys.F24))) || ((e.KeyCode >= Keys.NumPad0) && (e.KeyCode <= Keys.Divide))) || (((e.KeyCode >= Keys.OemSemicolon) && (e.KeyCode <= Keys.OemBackslash)) || ((e.KeyCode == Keys.Space) && !e.Shift))) && (!e.Alt && !e.Control)) || base.KeyEntersEditMode(e));
        //}

        #endregion
    }

    //public class  DataGridViewRichTextBoxEditingControl : RichTextBox, IDataGridViewEditingControl
    //{
    //    private DataGridView _dataGridView;
    //    private int _rowIndex;
    //    private bool _valueChanged;

    //    public DataGridViewRichTextBoxEditingControl()
    //    {
    //        this.BorderStyle = BorderStyle.None;
    //    }

    //    protected override void OnTextChanged(EventArgs e)
    //    {
    //        base.OnTextChanged(e);

    //        _valueChanged = true;
    //        EditingControlDataGridView.NotifyCurrentCellDirty(true);
    //    }

    //    protected override bool IsInputKey(Keys keyData)
    //    {
    //        Keys keys = keyData & Keys.KeyCode;
    //        if (keys == Keys.Return)
    //        {
    //            return this.Multiline;
    //        }

    //        return base.IsInputKey(keyData);
    //    }

    //    protected override void OnKeyDown(KeyEventArgs e)
    //    {
    //        base.OnKeyDown(e);

    //        if (e.Control)
    //        {
    //            switch (e.KeyCode)
    //            {
    //                // Control + B = Bold
    //                case Keys.B:
    //                    if (this.SelectionFont.Bold)
    //                    {
    //                        this.SelectionFont = new Font(this.Font.FontFamily, this.Font.Size, ~FontStyle.Bold & this.Font.Style);
    //                    }
    //                    else
    //                        this.SelectionFont = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Bold | this.Font.Style);
    //                    break;
    //                // Control + U = Underline
    //                case Keys.U:
    //                    if (this.SelectionFont.Underline)
    //                    {
    //                        this.SelectionFont = new Font(this.Font.FontFamily, this.Font.Size, ~FontStyle.Underline & this.Font.Style);
    //                    }
    //                    else
    //                        this.SelectionFont = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Underline | this.Font.Style);
    //                    break;
    //                // Control + I = Italic
    //                // Conflicts with the default shortcut
    //                //case Keys.I:
    //                //    if (this.SelectionFont.Italic)
    //                //    {
    //                //        this.SelectionFont = new Font(this.Font.FontFamily, this.Font.Size, ~FontStyle.Italic & this.Font.Style);
    //                //    }
    //                //    else
    //                //        this.SelectionFont = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Italic | this.Font.Style);
    //                //    break;
    //                default:
    //                    break;
    //            }
    //        }
    //    }

    //    #region IDataGridViewEditingControl Members

    //    public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
    //    {
    //        this.Font = dataGridViewCellStyle.Font;
    //    }

    //    public DataGridView EditingControlDataGridView
    //    {
    //        get
    //        {
    //            return _dataGridView;
    //        }
    //        set
    //        {
    //            _dataGridView = value;
    //        }
    //    }

    //    public object EditingControlFormattedValue
    //    {
    //        get
    //        {
    //            return this.Rtf;
    //        }
    //        set
    //        {
    //            if (value is string)
    //                this.Text = value as string;
    //        }
    //    }

    //    public int EditingControlRowIndex
    //    {
    //        get
    //        {
    //            return _rowIndex;
    //        }
    //        set
    //        {
    //            _rowIndex = value;
    //        }
    //    }

    //    public bool EditingControlValueChanged
    //    {
    //        get
    //        {
    //            return _valueChanged;
    //        }
    //        set
    //        {
    //            _valueChanged = value;
    //        }
    //    }
        
    //    public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
    //    {
    //        switch ((keyData & Keys.KeyCode))
    //        {
    //            case Keys.Return:
    //                if ((((keyData & (Keys.Alt | Keys.Control | Keys.Shift)) == Keys.Shift) && this.Multiline))
    //                {
    //                    return true;
    //                }
    //                break;
    //            case Keys.Left:
    //            case Keys.Right:
    //            case Keys.Up:
    //            case Keys.Down:
    //                return true;
    //        }

    //        return !dataGridViewWantsInputKey;
    //    }

    //    public Cursor EditingPanelCursor
    //    {
    //        get { return this.Cursor; }
    //    }

    //    public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
    //    {
    //        return this.Rtf;
    //    }

    //    public void PrepareEditingControlForEdit(bool selectAll)
    //    {
    //    }

    //    public bool RepositionEditingControlOnValueChange
    //    {
    //        get { return false; }
    //    }

    //    #endregion
    //}
}
