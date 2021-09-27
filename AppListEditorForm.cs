using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UIAutomationClient;

namespace ClipAngel
{
    public partial class AppListEditorForm : Form
    {
        public StringCollection AppList;
        private ImageList SmallImageList;

        public AppListEditorForm()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            AppList = new StringCollection();
            foreach (ListViewItem item in listViewChosenList.Items)
            {
                AppList.Add(item.SubItems[1].Text);
            }
        }

        private void AppListEditorForm_Load(object sender, EventArgs e)
        {
            SmallImageList = new ImageList();

            listViewChosenList.SmallImageList = SmallImageList;
            if (AppList != null)
            {
                foreach (string item in AppList)
                {
                    AddApplicationToList(listViewChosenList, item, false);
                }
                listViewChosenList.Sort();
            }
            PrepareColumns(listViewChosenList);

            listViewRunningList.SmallImageList = SmallImageList;
            RefreshRunningList();
        }

        private void RefreshRunningList()
        {
            listViewRunningList.Items.Clear();
            Process[] procs = Process.GetProcesses();
            foreach (var proc in procs)
            {
                string filename = Main.GetProcessMainModuleFullName(proc.Id);
                if (!String.IsNullOrEmpty(filename))
                    AddApplicationToList(listViewRunningList, filename, false);
            }
            listViewRunningList.Sort();
            PrepareColumns(listViewRunningList);
        }

        private void PrepareColumns(ListView list)
        {
            //list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //list.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            list.Columns[0].Width = -2;
            list.Columns[1].Width = -2;
        }

        private void AddApplicationToList(ListView List, string fullName, bool prepareColumns = true)
        {
            string ImageKey = AddApplicationIcon(fullName);
            if (List.Items.ContainsKey(ImageKey))
                return;
            ListViewItem item = List.Items.Add(ImageKey, ImageKey, ImageKey);
            item.SubItems.Add(new ListViewItem.ListViewSubItem() {Name = fullName, Text = fullName});
            if (prepareColumns)
                PrepareColumns(listViewChosenList);
        }

        private void buttonAddByFile_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "Application|*.exe";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                AddApplicationToList(listViewChosenList, ofd.FileName);
            }
        }

        string AddApplicationIcon(string fullFilename)
        {
            string shortFilename = Path.GetFileName(fullFilename);
            if (!SmallImageList.Images.ContainsKey(shortFilename))
            {
                Image image = Main.ApplicationIcon(fullFilename);
                if (image != null)
                    SmallImageList.Images.Add(shortFilename, image);
            }
            return shortFilename;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem listItem in listViewRunningList.SelectedItems)
            {
                AddApplicationToList(listViewChosenList, listItem.SubItems[1].Text, false);
            }
            PrepareColumns(listViewChosenList);
        }

        private void buttonDeleteSelected_Click(object sender, EventArgs e)
        {
            //for (int i = listViewSelectedList.SelectedItems.Count - 1; i >= 0; i--)
            //{
            //    ListViewItem listItem = listViewSelectedList.SelectedItems[i];
            foreach (ListViewItem listItem in listViewChosenList.SelectedItems)
            {
                listViewChosenList.Items.Remove(listItem);
            }
            PrepareColumns(listViewChosenList);
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            RefreshRunningList();
        }

        private void listViewRunningList_DoubleClick(object sender, EventArgs e)
        {
            AddApplicationToList(listViewChosenList, listViewRunningList.FocusedItem.SubItems[1].Text);
        }

        private void buttonSelectTargetApplication_Click(object sender, EventArgs e)
        {

        }

        private void buttonSelectClipboardOwner_Click(object sender, EventArgs e)
        {
            Main.ClipboardOwner clipboardOwner = ((Main)Owner).GetClipboardOwnerLockerInfo(false, false);
            ListViewItem[] items = listViewRunningList.Items.Find(clipboardOwner.appPath, true);
            if (items.Length > 0)
            {
                items[0].Selected = true;
                items[0].Focused = true;
                listViewRunningList.EnsureVisible(items[0].Index);
            }
        }

        private void listViewRunningList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
                CopySelectedValuesToClipboard(listViewRunningList);
        }
        private void CopySelectedValuesToClipboard(ListView list)
        {
            var builder = new StringBuilder();
            foreach (ListViewItem item in list.SelectedItems)
                builder.AppendLine(item.SubItems[1].Text);
            Clipboard.SetText(builder.ToString());
        }

        private void listViewChosenList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
                CopySelectedValuesToClipboard(listViewChosenList);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
