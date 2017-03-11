using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using GlobalizedPropertyGrid;
using Microsoft.Win32;

namespace ClipAngel
{
    public partial class SettingsForm : Form
    {
        private VisibleUserSettings set;
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender = null, EventArgs e = null)
        {
            set = new VisibleUserSettings(Owner as Main);
            //cultureManager1.UICulture = new CultureInfo((Owner as Main).Locale);
            var grid = propertyGrid1.Controls[2];
            grid.MouseClick += grid_MouseClick;
            grid.KeyDown += grid_KeyDown;
            propertyGrid1.SelectedObject = set;
            SetLabelColumnWidth(propertyGrid1, 300);
        }

        private void buttonOK_Click_1(object sender, EventArgs e)
        {
            set.Apply((Owner as Main).PortableMode);
            DialogResult = DialogResult.OK;
            Close();
        }


        void grid_MouseClick(object sender, MouseEventArgs e)
        {
            var grid = propertyGrid1.Controls[2];
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var invalidPoint = new Point(-2147483648, -2147483648);
            var FindPosition = grid.GetType().GetMethod("FindPosition", flags);
            var p = (Point)FindPosition.Invoke(grid, new object[] { e.X, e.Y });
            GridItem entry = null;
            if (p != invalidPoint && p.X == 2)
            {
                var GetGridEntryFromRow = grid.GetType()
                                              .GetMethod("GetGridEntryFromRow", flags);
                entry = (GridItem)GetGridEntryFromRow.Invoke(grid, new object[] { p.Y });
            }
            if (entry != null && entry.Value != null)
            {
                object parent;
                if (entry.Parent != null && entry.Parent.Value != null)
                    parent = entry.Parent.Value;
                else
                    parent = propertyGrid1.SelectedObject;
                if (entry.Value != null && entry.Value is bool)
                {
                    entry.PropertyDescriptor.SetValue(parent, !(bool)entry.Value);
                    propertyGrid1.Refresh();
                }
            }

        }

        void grid_KeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("Toto");
            //    var grid = propertyGrid1.Controls[2];
            //    var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            //    var invalidPoint = new Point(-2147483648, -2147483648);
            //    var FindPosition = grid.GetType().GetMethod("FindPosition", flags);
            //    var p = (Point)FindPosition.Invoke(grid, new object[] { e.X, e.Y });
            //    GridItem entry = null;
            //    if (p != invalidPoint && p.X == 2)
            //    {
            //        var GetGridEntryFromRow = grid.GetType()
            //                                      .GetMethod("GetGridEntryFromRow", flags);
            //        entry = (GridItem)GetGridEntryFromRow.Invoke(grid, new object[] { p.Y });
            //    }
            //    if (entry != null && entry.Value != null)
            //    {
            //        object parent;
            //        if (entry.Parent != null && entry.Parent.Value != null)
            //            parent = entry.Parent.Value;
            //        else
            //            parent = propertyGrid1.SelectedObject;
            //        if (entry.Value != null && entry.Value is bool)
            //        {
            //            entry.PropertyDescriptor.SetValue(parent, !(bool)entry.Value);
            //            propertyGrid1.Refresh();
            //        }
            //    }
            //    if (false
            //        || e.KeyCode == Keys.ControlKey
            //        || e.KeyCode == Keys.ShiftKey
            //        || e.KeyCode == Keys.Menu)
            //    {
            //        e.Handled = true;
            //        return;
            //    }
            //    string HotkeyTitle = "";
            //    if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            //        HotkeyTitle = "No";
            //    else
            //    {
            //        if (e.Control)
            //            HotkeyTitle += Keys.Control.ToString() + " + ";
            //        if (e.Alt)
            //            HotkeyTitle += Keys.Alt.ToString() + " + ";
            //        if (e.Shift)
            //            HotkeyTitle += Keys.Shift.ToString() + " + ";
            //        HotkeyTitle += e.KeyCode.ToString();
            //    }
            //    (sender as TextBox).Text = HotkeyTitle;
            //    e.Handled = true;
        }

        public static void SetLabelColumnWidth(PropertyGrid grid, int width)
        {
            if (grid == null)
                return;

            FieldInfo fi = grid.GetType().GetField("gridView", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi == null)
                return;

            Control view = fi.GetValue(grid) as Control;
            if (view == null)
                return;

            MethodInfo mi = view.GetType().GetMethod("MoveSplitterTo", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null)
                return;
            mi.Invoke(view, new object[] { width });
        }

        private void HotkeyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (false
                || e.KeyCode == Keys.ControlKey
                || e.KeyCode == Keys.ShiftKey
                || e.KeyCode == Keys.Menu)
            {
                e.Handled = true;
                return;
            }
            string HotkeyTitle = "";
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                HotkeyTitle = "No";
            else
            {
                if (e.Control)
                    HotkeyTitle += Keys.Control.ToString() + " + ";
                if (e.Alt)
                    HotkeyTitle += Keys.Alt.ToString() + " + ";
                if (e.Shift)
                    HotkeyTitle += Keys.Shift.ToString() + " + ";
                HotkeyTitle += e.KeyCode.ToString();
            }
            (sender as TextBox).Text = HotkeyTitle;
            e.Handled = true;
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            string question = (Owner as Main).CurrentLangResourceManager.GetString("QuestionResetSettings");
            if (DialogResult.Yes == MessageBox.Show(question, Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation))
            {
                //Properties.Settings.Default.Reset(); // Not working, it seem reason is type System.Collections.Specialized.StringCollection
                foreach (SettingsProperty settingsKey in Properties.Settings.Default.Properties)
                {
                    var value = Properties.Settings.Default[settingsKey.Name];
                    object newValue;
                    try
                    {
                        newValue = Convert.ChangeType(settingsKey.DefaultValue, value.GetType());
                    }
                    catch
                    {
                        // All object types can not be deserialized. why?
                        continue;
                    }
                    if (newValue != null)
                        Properties.Settings.Default[settingsKey.Name] = newValue;
                }
                Settings_Load();
            }
        }
    }

    class LangConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(VisibleUserSettings.langList);
        }
    }
    public class MyBoolEditor : UITypeEditor
    {
        public override bool GetPaintValueSupported
            (System.ComponentModel.ITypeDescriptorContext context)
        { return true; }
        public override void PaintValue(PaintValueEventArgs e)
        {
            var rect = e.Bounds;
            rect.Inflate(1, 1);
            ControlPaint.DrawCheckBox(e.Graphics, rect, ButtonState.Flat |
                (((bool)e.Value) ? ButtonState.Checked : ButtonState.Normal));
        }
    }

    public class ApplicationPathEditor : UITypeEditor
    {
        private OpenFileDialog ofd;
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if ((context != null) && (provider != null))
            {
                IWindowsFormsEditorService editorService =
                (IWindowsFormsEditorService)
                provider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService != null)
                {
                    ofd = new OpenFileDialog();
                    ofd.Multiselect = true;
                    ofd.Filter = "Application|*.exe";
                    ofd.FileName = value.ToString();
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        return ofd.FileName;
                    }
                }
            }
            return base.EditValue(context, provider, value);
        }
    }

    public partial class HotkeyEditor : UITypeEditor
    {
        private HotkeyEditorForm ofd;
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if ((context != null) && (provider != null))
            {
                IWindowsFormsEditorService editorService =
                (IWindowsFormsEditorService)
                provider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService != null)
                {
                    ofd = new HotkeyEditorForm();
                    ofd.HotkeyTextbox.Text = value.ToString();
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        return ofd.HotkeyTextbox.Text;
                    }
                }
            }
            return base.EditValue(context, provider, value);
        }
    }
    class VisibleUserSettings : GlobalizedObject
    {
        public static List<string> langList = new List<string> { "Default", "English", "Russian" };
        public VisibleUserSettings(Main Owner)
        {
            HistoryDepthNumber = (int)Properties.Settings.Default.HistoryDepthNumber;
            DefaultFont = Properties.Settings.Default.Font;
            Language = Properties.Settings.Default.Language;
            Autostart = Properties.Settings.Default.Autostart;
            TextCompareApplication = Properties.Settings.Default.TextCompareApplication;
            MaxClipSizeKB = Properties.Settings.Default.MaxClipSizeKB;
            GlobalHotkeyShow = Properties.Settings.Default.HotkeyShow;
            GlobalHotkeyShowFavorites = Properties.Settings.Default.HotKeyShowFavorites;
            GlobalHotkeyIncrementalPaste = Properties.Settings.Default.HotkeyIncrementalPaste;
            WindowAutoPosition = Properties.Settings.Default.WindowAutoPosition;
            MoveCopiedClipToTop = Properties.Settings.Default.MoveCopiedClipToTop;
            ShowSizeColumn = Properties.Settings.Default.ShowVisualWeightColumn;
            ClipListSimpleDraw = Properties.Settings.Default.ClipListSimpleDraw;
            AutoCheckUpdate = Properties.Settings.Default.AutoCheckForUpdate;
            ClearFiltersOnClose = Properties.Settings.Default.ClearFiltersOnClose;
            ShowApplicationIconColumn = Properties.Settings.Default.ShowApplicationIconColumn;

            NumberOfClips = Owner.ClipsNumber.ToString();
            UserSettingsPath = Owner.UserSettingsPath;
            DatabaseSize = ((new FileInfo(Owner.DbFileName)).Length / (1024 * 1024)).ToString();
        }

        [GlobalizedCategory("Info")]
        [ReadOnly(true)]
        public string DatabaseSize { get; set; }

        [GlobalizedCategory("Info")]
        [ReadOnly(true)]
        public string UserSettingsPath { get; set; }

        [GlobalizedCategory("Info")]
        [ReadOnly(true)]
        public string NumberOfClips { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool ShowApplicationIconColumn { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool ClearFiltersOnClose { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool AutoCheckUpdate { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool ClipListSimpleDraw { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool ShowSizeColumn { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool MoveCopiedClipToTop { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool WindowAutoPosition { get; set; }

        [GlobalizedCategory("Hotkeys")]
        [EditorAttribute(typeof(HotkeyEditor), typeof(UITypeEditor))]
        public string GlobalHotkeyIncrementalPaste { get; set; }

        [GlobalizedCategory("Hotkeys")]
        [EditorAttribute(typeof(HotkeyEditor), typeof(UITypeEditor))]
        public string GlobalHotkeyShowFavorites { get; set; }

        [GlobalizedCategory("Hotkeys")]
        [EditorAttribute(typeof(HotkeyEditor), typeof(UITypeEditor))]
        public string GlobalHotkeyShow { get; set; }

        [GlobalizedCategory("Other")]
        public int MaxClipSizeKB { get; set; }

        [GlobalizedCategory("Other")]
        public int HistoryDepthNumber { get; set; }

        [GlobalizedCategory("Other")]
        public Font DefaultFont { get; set; }

        [GlobalizedCategory("Other")]
        [TypeConverter(typeof(LangConverter))]
        public string Language { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool Autostart { get; set; }

        [GlobalizedCategory("Other")]
        //[EditorAttribute(typeof(FileNameEditor), typeof(UITypeEditor))]
        [EditorAttribute(typeof(ApplicationPathEditor), typeof(UITypeEditor))]
        public string TextCompareApplication { get; set; }

        public void Apply(bool PortableMode = false)
        {
            Properties.Settings.Default.HistoryDepthNumber = HistoryDepthNumber;
            Properties.Settings.Default.Font = DefaultFont;
            Properties.Settings.Default.Language = Language;
            Properties.Settings.Default.TextCompareApplication = TextCompareApplication;
            Properties.Settings.Default.Font = DefaultFont;
            Properties.Settings.Default.ShowApplicationIconColumn = ShowApplicationIconColumn;
            Properties.Settings.Default.ClearFiltersOnClose = ClearFiltersOnClose;
            Properties.Settings.Default.AutoCheckForUpdate = AutoCheckUpdate;
            Properties.Settings.Default.ClipListSimpleDraw = ClipListSimpleDraw;
            Properties.Settings.Default.MoveCopiedClipToTop = MoveCopiedClipToTop;
            Properties.Settings.Default.ShowVisualWeightColumn = ShowSizeColumn;
            Properties.Settings.Default.WindowAutoPosition = WindowAutoPosition;
            Properties.Settings.Default.HotkeyShow = GlobalHotkeyShow;
            Properties.Settings.Default.HotKeyShowFavorites = GlobalHotkeyShowFavorites;
            Properties.Settings.Default.HotkeyIncrementalPaste = GlobalHotkeyIncrementalPaste;
            Properties.Settings.Default.Language = Language;

            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            string Keyname = Application.ProductName;
            try
            {
                if (Autostart)
                {
                    string CommandLine = Application.ExecutablePath + " /m";
                    if (PortableMode)
                        CommandLine += " /p";
                    reg.SetValue(Keyname, CommandLine);
                }
                else
                    reg.DeleteValue(Keyname);
                reg.Close();
                Properties.Settings.Default.Autostart = Autostart;
            }
            catch
            { }
        }
    }
}
