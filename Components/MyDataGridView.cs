using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipAngel
{
    public class MyDataGridView: DataGridView
    {
        private bool currentRowSelectedBeforeClick;
        // http://stackoverflow.com/questions/3172424/how-to-maintain-selected-rows-in-datagridview-when-mouse-is-held-down-on-a-cell
        protected override void OnCellMouseDown(DataGridViewCellMouseEventArgs e)
        {
            currentRowSelectedBeforeClick = Rows[e.RowIndex].Selected;
            IEnumerable<DataGridViewRow> sel = null;
            if (e.Button == MouseButtons.Left && Rows[e.RowIndex].Selected)
                sel = this.SelectedRows.OfType<DataGridViewRow>();
            base.OnCellMouseDown(e);
            if (sel != null)
            {
                foreach (var row in sel.Reverse())
                    row.Selected = true;
            }
        }

        protected override void OnCellMouseUp(DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && (Control.ModifierKeys & Keys.Shift) == 0 && (Control.ModifierKeys & Keys.Control) == 0)
            {
                foreach (DataGridViewRow selectedRow in SelectedRows)
                {
                    if (Rows[e.RowIndex] != selectedRow)
                        selectedRow.Selected = false;
                }
            }
            if (e.Button == MouseButtons.Left && currentRowSelectedBeforeClick && (ModifierKeys & Keys.Control) != 0 && SelectedRows.Count > 1)
            {
                Rows[e.RowIndex].Selected = false;
            }
        }

    }
}
