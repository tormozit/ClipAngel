using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Management;
using GlobalizedPropertyGrid;
using Microsoft.Win32;

namespace ClipAngel
{
    public partial class SettingsForm : Form
    {
        private VisibleUserSettings set;
        public SettingsForm(Main Owner)
        {
            InitializeComponent();
            this.Owner = Owner;
            set = new VisibleUserSettings(Owner);
            //cultureManager1.UICulture = new CultureInfo((Owner as Main).Locale);
            var grid = propertyGrid1.Controls[2];
            grid.MouseClick += grid_MouseClick;
            grid.ContextMenuStrip = contexMenuGrid;
            var label = propertyGrid1.Controls[0].Controls[0];
            label.ContextMenuStrip = contexMenuLabel;
            label = propertyGrid1.Controls[0].Controls[1];
            label.ContextMenuStrip = contexMenuLabel;
            propertyGrid1.SelectedObject = set;
            if (Properties.Settings.Default.SettingsWindowSize.Width > 0)
                this.Size = Properties.Settings.Default.SettingsWindowSize;
        }

        private void Settings_Load(object sender = null, EventArgs e = null)
        {
            (propertyGrid1.SelectedObject as VisibleUserSettings).Load((Owner as Main).PortableMode);
            propertyGrid1.SetLabelColumnWidth(300);
            RequeryRows();
        }

        private void RequeryRows()
        {
            List<string> filteredList = new List<string>();
            PropertyDescriptorCollection allproperties = TypeDescriptor.GetProperties(propertyGrid1.SelectedObject);
            foreach (PropertyDescriptor property in allproperties)
            {
                if (false
                    || textBoxFilter.Text == ""
                    || property.Description.IndexOf(textBoxFilter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0
                    || property.DisplayName.IndexOf(textBoxFilter.Text, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    filteredList.Add(property.Name);
                }
            }
            propertyGrid1.BrowsableProperties = filteredList.ToArray();
            propertyGrid1.Refresh();
            if (textBoxFilter.Text != "")
                buttonClearFilter.BackColor = Color.GreenYellow;
            else
                buttonClearFilter.BackColor = DefaultBackColor;
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

        private void buttonReset_Click(object sender, EventArgs e)
        {
            string question = Properties.Resources.QuestionResetSettings;
            if (DialogResult.Yes == MessageBox.Show(question, Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation))
            {
                //Properties.Settings.Default.Reset(); // Not working, it seems that reason is type System.Collections.Specialized.StringCollection
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

        private void comboBoxFilter_TextChanged(object sender, EventArgs e)
        {
            RequeryRows();
        }

        private void buttonClearFilter_Click(object sender, EventArgs e)
        {
            textBoxFilter.Text = "";
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText((((sender as ToolStripMenuItem).Owner as ContextMenuStrip).SourceControl as Label).Text);
        }

        private void gridContextMenuCopy_Click(object sender, EventArgs e)
        {
            if (propertyGrid1.SelectedGridItem != null)
            {
                Clipboard.SetText(propertyGrid1.SelectedGridItem.Label);
            }
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
                Properties.Settings.Default.SettingsWindowSize = Size;
            else
                Properties.Settings.Default.SettingsWindowSize = RestoreBounds.Size;
        }
    }

    class LanguageConverter : StringConverter
    {
        public static Dictionary<string, string> langList = new Dictionary<string, string>
        {
            { "Default", "Default"},
            { "English", "English (by Tormozit)"},
            { "Chinese", "Chinese (by Dwhite)"},
            { "French", "French (by François M.)"},
            { "German", "German (by Dieter Hummel)"},
            { "Greek", "Greek (by machine)"},
            { "Hindi", "Hindi (by machine)"},
            { "Italian", "Italian (by Bovirus)"},
            { "Polish", "Polish (by Galileusz)"},
            { "PortugueseBrazil", "Portuguese-Brazil (by Nelson Carvalho)"},
            { "Russian", "Russian (by Tormozit)"},
            { "Spain", "Spain (by machine)"}
        };

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<string> list = new List <string>();
            foreach (KeyValuePair<string, string> pair in langList)
            {
                list.Add(pair.Key);
            }
            return new StandardValuesCollection(list);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context,
            Type destType)
        {
            return destType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture,
            object value, Type destType)
        {
            if (destType == typeof(String))
            {
                if (langList.ContainsKey((string)value))
                    return langList[(string)value];
                else
                    return "Default";
            }
            return base.ConvertTo(context, culture, value, destType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
            Type srcType)
        {
            return srcType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture,
            object value)
        {
            foreach (KeyValuePair<string, string> pair in langList)
            {
                if ((string)pair.Value == (string)value)
                    return pair.Key;
            }
            return "Default";
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

    public class DatabaseFileNameEditor : FileNameEditor
    {
        protected override void InitializeDialog(System.Windows.Forms.OpenFileDialog openFileDialog)
        {
            base.InitializeDialog(openFileDialog);
            openFileDialog.CheckFileExists = false;
            openFileDialog.Filter = "ClipAngel database|*.db|All|*.*";
        }
    }

    public class SoundFileNameEditor : FileNameEditor
    {
        protected override void InitializeDialog(System.Windows.Forms.OpenFileDialog openFileDialog)
        {
            base.InitializeDialog(openFileDialog);
            openFileDialog.CheckFileExists = true;
            openFileDialog.Filter = "Sound file|*.wav|All|*.*";
        }
    }

    public class ApplicationPathEditor : UITypeEditor
    {
        private System.Windows.Forms.OpenFileDialog ofd;
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
                    ofd = new System.Windows.Forms.OpenFileDialog();
                    ofd.Multiselect = true;
                    ofd.Filter = "Application|*.exe";
                    ofd.FileName = value.ToString();
                    if (ofd.ShowDialog((((ObjectWrapper)context.Instance).SelectedObject as VisibleUserSettings).Owner.Owner) == DialogResult.OK)
                    {
                        return ofd.FileName;
                    }
                }
            }
            return base.EditValue(context, provider, value);
        }
    }

    public partial class AppListEditor : UITypeEditor
    {
        private AppListEditorForm ofd;
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
                    ofd = new AppListEditorForm();
                    ofd.AppList = (StringCollection) value;
                    if (ofd.ShowDialog((((ObjectWrapper)context.Instance).SelectedObject as VisibleUserSettings).Owner) == DialogResult.OK)
                    {
                        return ofd.AppList;
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
                    Form owner = (((ObjectWrapper) context.Instance).SelectedObject as VisibleUserSettings).Owner;
                    if (ofd.ShowDialog(owner) == DialogResult.OK)
                    {
                        return ofd.HotkeyTextbox.Text;
                    }
                }
            }
            return base.EditValue(context, provider, value);
        }
    }

    // Makes readonly value text of property
    public class HotkeyConverter : TypeConverter
    {
        public HotkeyConverter()
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(String))
            {
                if (value == null)
                    return "No";
                else
                    return value.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(String))
                return false;
            else
                return base.CanConvertFrom(context, sourceType);
        }
    }

    internal class ObjectWrapper : ICustomTypeDescriptor
    {
        /// <summary>Contain a reference to the selected objet that will linked to the parent PropertyGrid.</summary>
        private object m_SelectedObject = null;
        /// <summary>Contain a reference to the collection of properties to show in the parent PropertyGrid.</summary>
        /// <remarks>By default, m_PropertyDescriptors contain all the properties of the object. </remarks>
        List<PropertyDescriptor> m_PropertyDescriptors = new List<PropertyDescriptor>();

        /// <summary>Simple constructor.</summary>
        /// <param name="obj">A reference to the selected object that will linked to the parent PropertyGrid.</param>
        internal ObjectWrapper(object obj)
        {
            m_SelectedObject = obj;
        }

        /// <summary>Get or set a reference to the selected objet that will linked to the parent PropertyGrid.</summary>
        public object SelectedObject
        {
            get { return m_SelectedObject; }
            set { if (m_SelectedObject != value) m_SelectedObject = value; }
        }

        /// <summary>Get or set a reference to the collection of properties to show in the parent PropertyGrid.</summary>
        public List<PropertyDescriptor> PropertyDescriptors
        {
            get { return m_PropertyDescriptors; }
            set { m_PropertyDescriptors = value; }
        }

        #region ICustomTypeDescriptor Members
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return new PropertyDescriptorCollection(m_PropertyDescriptors.ToArray(), true);
        }

        /// <summary>GetAttributes.</summary>
        /// <returns>AttributeCollection</returns>
        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(m_SelectedObject, true);
        }
        /// <summary>Get Class Name.</summary>
        /// <returns>String</returns>
        public String GetClassName()
        {
            return TypeDescriptor.GetClassName(m_SelectedObject, true);
        }
        /// <summary>GetComponentName.</summary>
        /// <returns>String</returns>
        public String GetComponentName()
        {
            return TypeDescriptor.GetComponentName(m_SelectedObject, true);
        }

        /// <summary>GetConverter.</summary>
        /// <returns>TypeConverter</returns>
        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(m_SelectedObject, true);
        }

        /// <summary>GetDefaultEvent.</summary>
        /// <returns>EventDescriptor</returns>
        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(m_SelectedObject, true);
        }

        /// <summary>GetDefaultProperty.</summary>
        /// <returns>PropertyDescriptor</returns>
        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(m_SelectedObject, true);
        }

        /// <summary>GetEditor.</summary>
        /// <param name="editorBaseType">editorBaseType</param>
        /// <returns>object</returns>
        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(m_SelectedObject, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(m_SelectedObject, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return m_SelectedObject;
        }

        #endregion

    }

    // https://www.codeproject.com/Articles/13342/Filtering-properties-in-a-PropertyGrid
    /// <summary>
    /// This class overrides the standard PropertyGrid provided by Microsoft.
    /// It also allows to hide (or filter) the properties of the SelectedObject displayed by the PropertyGrid.
    /// </summary>
    public class FilteredPropertyGrid : PropertyGrid
    {
        /// <summary>Contain a reference to the collection of properties to show in the parent PropertyGrid.</summary>
        /// <remarks>By default, m_PropertyDescriptors contain all the properties of the object. </remarks>
        List<PropertyDescriptor> m_PropertyDescriptors = new List<PropertyDescriptor>();
        /// <summary>Contain a reference to the array of properties to display in the PropertyGrid.</summary>
        private AttributeCollection m_HiddenAttributes = null, m_BrowsableAttributes = null;
        /// <summary>Contain references to the arrays of properties or categories to hide.</summary>
        private string[] m_BrowsableProperties = null, m_HiddenProperties = null;
        /// <summary>Contain a reference to the wrapper that contains the object to be displayed into the PropertyGrid.</summary>
        private ObjectWrapper m_Wrapper = null;

        /// <summary>Public constructor.</summary>
        public FilteredPropertyGrid()
        {
            //InitializeComponent();
            base.SelectedObject = m_Wrapper;
        }

        public void SetLabelColumnWidth(int width)
        {
            FieldInfo fi = GetType().BaseType.GetField("gridView", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi == null)
                return;

            Control view = fi.GetValue(this) as Control;
            if (view == null)
                return;

            MethodInfo mi = view.GetType().GetMethod("MoveSplitterTo", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null)
                return;
            mi.Invoke(view, new object[] { width });
        }

        public new AttributeCollection BrowsableAttributes
        {
            get { return m_BrowsableAttributes; }
            set
            {
                if (m_BrowsableAttributes != value)
                {
                    m_HiddenAttributes = null;
                    m_BrowsableAttributes = value;
                    RefreshProperties();
                }
            }
        }

        /// <summary>Get or set the categories to hide.</summary>
        public AttributeCollection HiddenAttributes
        {
            get { return m_HiddenAttributes; }
            set
            {
                if (value != m_HiddenAttributes)
                {
                    m_HiddenAttributes = value;
                    m_BrowsableAttributes = null;
                    RefreshProperties();
                }
            }
        }
        /// <summary>Get or set the properties to show.</summary>
        /// <exception cref="ArgumentException">if one or several properties don't exist.</exception>
        public string[] BrowsableProperties
        {
            get { return m_BrowsableProperties; }
            set
            {
                if (value != m_BrowsableProperties)
                {
                    m_BrowsableProperties = value;
                    //m_HiddenProperties = null;
                    RefreshProperties();
                }
            }
        }

        /// <summary>Get or set the properties to hide.</summary>
        public string[] HiddenProperties
        {
            get { return m_HiddenProperties; }
            set
            {
                if (value != m_HiddenProperties)
                {
                    //m_BrowsableProperties = null;
                    m_HiddenProperties = value;
                    RefreshProperties();
                }
            }
        }

        /// <summary>Overwrite the PropertyGrid.SelectedObject property.</summary>
        /// <remarks>The object passed to the base PropertyGrid is the wrapper.</remarks>
        public new object SelectedObject
        {
            get { return m_Wrapper != null ? ((ObjectWrapper)base.SelectedObject).SelectedObject : null; }
            set
            {
                // Set the new object to the wrapper and create one if necessary.
                if (m_Wrapper == null)
                {
                    m_Wrapper = new ObjectWrapper(value);
                    RefreshProperties();
                }
                else if (m_Wrapper.SelectedObject != value)
                {
                    bool needrefresh = value.GetType() != m_Wrapper.SelectedObject.GetType();
                    m_Wrapper.SelectedObject = value;
                    if (needrefresh) RefreshProperties();
                }
                // Set the list of properties to the wrapper.
                m_Wrapper.PropertyDescriptors = m_PropertyDescriptors;
                // Link the wrapper to the parent PropertyGrid.
                base.SelectedObject = m_Wrapper;
            }
        }

        /// <summary>Called when the browsable properties have changed.</summary>
        private void OnBrowsablePropertiesChanged()
        {
            if (m_Wrapper == null) return;
        }

        /// <summary>Build the list of the properties to be displayed in the PropertyGrid, following the filters defined the Browsable and Hidden properties.</summary>
        private void RefreshProperties()
        {
            if (m_Wrapper == null) return;
            // Clear the list of properties to be displayed.
            m_PropertyDescriptors.Clear();
            // Check whether the list is filtered 
            if (m_BrowsableAttributes != null && m_BrowsableAttributes.Count > 0)
            {
                // Add to the list the attributes that need to be displayed.
                foreach (Attribute attribute in m_BrowsableAttributes) ShowAttribute(attribute);
            }
            else
            {
                // Fill the collection with all the properties.
                PropertyDescriptorCollection originalpropertydescriptors = TypeDescriptor.GetProperties(m_Wrapper.SelectedObject);
                foreach (PropertyDescriptor propertydescriptor in originalpropertydescriptors) m_PropertyDescriptors.Add(propertydescriptor);
                // Remove from the list the attributes that mustn't be displayed.
                if (m_HiddenAttributes != null) foreach (Attribute attribute in m_HiddenAttributes) HideAttribute(attribute);
            }
            // Get all the properties of the SelectedObject
            PropertyDescriptorCollection allproperties = TypeDescriptor.GetProperties(m_Wrapper.SelectedObject);
            // Hide if necessary, some properties
            if (m_HiddenProperties != null)
            {
                // Remove from the list the properties that mustn't be displayed.
                foreach (string propertyname in m_HiddenProperties)
                {
                    try
                    {
                        PropertyDescriptor property = allproperties[propertyname];
                        // Remove from the list the property
                        HideProperty(property);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException(ex.Message);
                    }
                }
            }
            // Display if necessary, some properties
            if (m_BrowsableProperties != null)
            {
                m_PropertyDescriptors.Clear(); // Added by tormozit
                foreach (string propertyname in m_BrowsableProperties)
                {
                    try
                    {
                        ShowProperty(allproperties[propertyname]);
                    }
                    catch
                    {
                        throw new ArgumentException("Property not found", propertyname);
                    }
                }
            }
        }
        /// <summary>Allows to hide a set of properties to the parent PropertyGrid.</summary>
        /// <param name="propertyname">A set of attributes that filter the original collection of properties.</param>
        /// <remarks>For better performance, include the BrowsableAttribute with true value.</remarks>
        private void HideAttribute(Attribute attribute)
        {
            PropertyDescriptorCollection filteredoriginalpropertydescriptors = TypeDescriptor.GetProperties(m_Wrapper.SelectedObject, new Attribute[] { attribute });
            if (filteredoriginalpropertydescriptors == null || filteredoriginalpropertydescriptors.Count == 0) throw new ArgumentException("Attribute not found", attribute.ToString());
            foreach (PropertyDescriptor propertydescriptor in filteredoriginalpropertydescriptors) HideProperty(propertydescriptor);
        }
        /// <summary>Add all the properties that match an attribute to the list of properties to be displayed in the PropertyGrid.</summary>
        /// <param name="property">The attribute to be added.</param>
        private void ShowAttribute(Attribute attribute)
        {
            PropertyDescriptorCollection filteredoriginalpropertydescriptors = TypeDescriptor.GetProperties(m_Wrapper.SelectedObject, new Attribute[] { attribute });
            if (filteredoriginalpropertydescriptors == null || filteredoriginalpropertydescriptors.Count == 0) throw new ArgumentException("Attribute not found", attribute.ToString());
            foreach (PropertyDescriptor propertydescriptor in filteredoriginalpropertydescriptors) ShowProperty(propertydescriptor);
        }
        /// <summary>Add a property to the list of properties to be displayed in the PropertyGrid.</summary>
        /// <param name="property">The property to be added.</param>
        private void ShowProperty(PropertyDescriptor property)
        {
            if (!m_PropertyDescriptors.Contains(property)) m_PropertyDescriptors.Add(property);
        }
        /// <summary>Allows to hide a property to the parent PropertyGrid.</summary>
        /// <param name="propertyname">The name of the property to be hidden.</param>
        private void HideProperty(PropertyDescriptor property)
        {
            if (m_PropertyDescriptors.Contains(property)) m_PropertyDescriptors.Remove(property);
        }

    }

    class VisibleUserSettings : GlobalizedObject
    {
        public Main Owner;

        public VisibleUserSettings(Main Owner)
        {
            this.Owner = Owner;
        }

        public void Load(bool PortableMode = false)
        {
            IgnoreUrlsClipCapture = Properties.Settings.Default.IgnoreUrlsClipCapture;
            GlobalHotkeyForcedCapture = Properties.Settings.Default.GlobalHotkeyForcedCapture;
            DontSendPaste = Properties.Settings.Default.DontSendPaste;
            DelimiterForTextJoin = Properties.Settings.Default.DelimiterForTextJoin;
            ConfirmationBeforeDelete = Properties.Settings.Default.ConfirmationBeforeDelete;
            AllowDownloadThumbnail = Properties.Settings.Default.AllowDownloadThumbnail;
            SearchAllFields = Properties.Settings.Default.SearchAllFields;
            CaptureImages = Properties.Settings.Default.CaptureImages;
            DeleteNonFavoriteClipsOnExit = Properties.Settings.Default.DeleteNonFavoriteClipsOnExit;
            FilterListBySearchString = Properties.Settings.Default.FilterListBySearchString;
            AutoSelectMatchedClip = Properties.Settings.Default.AutoSelectMatchedClip;
            Max1CCodeSizeToColorize = Properties.Settings.Default.Max1CCodeSizeToColorize;
            EncryptDatabaseForCurrentUser = Properties.Settings.Default.EncryptDatabaseForCurrentUser;
            ReplaceDuplicates = Properties.Settings.Default.ReplaceDuplicates;
            GlobalHotkeySwitchMonitoring = Properties.Settings.Default.GlobalHotkeySwitchMonitoring;
            ReadWindowTitles = Properties.Settings.Default.ReadWindowTitles;
            MaxCellsToCaptureFormattedText = Properties.Settings.Default.MaxCellsToCaptureFormattedText;
            MaxCellsToCaptureImage = Properties.Settings.Default.MaxCellsToCaptureImage;
            DatabaseFile = Properties.Settings.Default.DatabaseFile;
            PlaySoundOnClipCapture = Properties.Settings.Default.PlaySoundOnClipCapture;
            MonospacedFont = Properties.Settings.Default.MonospacedFont;
            WordWrap = Properties.Settings.Default.WordWrap;
            ShowNativeTextFormatting = Properties.Settings.Default.ShowNativeTextFormatting;
            SearchWordsIndependently = Properties.Settings.Default.SearchWordsIndependently;
            SearchWildcards = Properties.Settings.Default.SearchWildcards;
            AutoSelectMatch = Properties.Settings.Default.AutoSelectMatch;
            SearchCaseSensitive = Properties.Settings.Default.SearchCaseSensitive;
            SearchIgnoreBigTexts = Properties.Settings.Default.SearchIgnoreBigTexts;
            ClipTempFileFolder = Properties.Settings.Default.ClipTempFileFolder;
            IgnoreExclusiveFormatClipCapture = Properties.Settings.Default.IgnoreExclusiveFormatClipCapture;
            //RestoreCaretPositionOnFocusReturn = Properties.Settings.Default.RestoreCaretPositionOnFocusReturn;
            GlobalHotkeyPasteText = Properties.Settings.Default.GlobalHotkeyPasteText;
            GlobalHotkeySimulateInput = Properties.Settings.Default.GlobalHotkeySimulateInput;
            UseFormattingInDuplicateDetection = Properties.Settings.Default.UseFormattingInDuplicateDetection;
            IgnoreApplicationsClipCapture = Properties.Settings.Default.IgnoreApplicationsClipCapture;
            //CopyTextInAnyWindowOnCTRLF3 = Properties.Settings.Default.CopyTextInAnyWindowOnCTRLF3;
            FastWindowOpen = Properties.Settings.Default.FastWindowOpen;
            HistoryDepthDays = Properties.Settings.Default.HistoryDepthDays;
            HistoryDepthNumber = Properties.Settings.Default.HistoryDepthNumber;
            DefaultFont = Properties.Settings.Default.Font;
            Language = Properties.Settings.Default.Language;
            Autostart = Properties.Settings.Default.Autostart;
            TextCompareApplication = Properties.Settings.Default.TextCompareApplication;
            TextEditor = Properties.Settings.Default.TextEditor;
            RtfEditor = Properties.Settings.Default.RtfEditor;
            HtmlEditor = Properties.Settings.Default.HtmlEditor;
            ImageEditor = Properties.Settings.Default.ImageEditor;
            MaxClipSizeKB = Properties.Settings.Default.MaxClipSizeKB;
            GlobalHotkeyOpenLast = Properties.Settings.Default.GlobalHotkeyOpenLast;
            GlobalHotkeyOpenCurrent = Properties.Settings.Default.GlobalHotkeyOpenCurrent;
            GlobalHotkeyOpenFavorites = Properties.Settings.Default.GlobalHotkeyOpenFavorites;
            GlobalHotkeyIncrementalPaste = Properties.Settings.Default.GlobalHotkeyIncrementalPaste;
            GlobalHotkeyDecrementalPaste = Properties.Settings.Default.GlobalHotkeyDecrementalPaste;
            GlobalHotkeyCompareLastClips = Properties.Settings.Default.GlobalHotkeyCompareLastClips;
            WindowAutoPosition = Properties.Settings.Default.WindowAutoPosition;
            MoveCopiedClipToTop = Properties.Settings.Default.MoveCopiedClipToTop;
            AutoCheckUpdate = Properties.Settings.Default.AutoCheckForUpdate;
            ShowApplicationIconColumn = Properties.Settings.Default.ShowApplicationIconColumn;
            ShowSecondaryColumns = Properties.Settings.Default.ShowSecondaryColumns;

            NumberOfClips = Owner.ClipsCount().ToString();
            UserSettingsPath = Owner.UserSettingsPath;
            DatabaseSize = ((new FileInfo(Owner.DbFileName)).Length / (1024 * 1024)).ToString();

            //// Too much time ~100ms
            //string wmiQuery = string.Format("select CommandLine from Win32_Process where ProcessID={0}", Process.GetCurrentProcess().Id);
            //ManagementObjectSearcher searcher = new ManagementObjectSearcher(wmiQuery);
            //ManagementObjectCollection retObjectCollection = searcher.Get();
            //foreach (ManagementObject process in retObjectCollection)
            //    CurrentPath = process["CommandLine"].ToString();
            CurrentPath = "\"" + Application.ExecutablePath + "\"";
            if (PortableMode)
                CurrentPath += " /p";

            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            string Keyname = Application.ProductName;
            try
            {
                AutostartPath = reg.GetValue(Keyname).ToString();
            }
            catch
            {
            }
        }

        public void Apply(bool PortableMode = false)
        {
            Properties.Settings.Default.IgnoreUrlsClipCapture = IgnoreUrlsClipCapture;
            Properties.Settings.Default.GlobalHotkeyForcedCapture = GlobalHotkeyForcedCapture;
            Properties.Settings.Default.DontSendPaste = DontSendPaste;
            Properties.Settings.Default.DelimiterForTextJoin = DelimiterForTextJoin;
            Properties.Settings.Default.ConfirmationBeforeDelete = ConfirmationBeforeDelete;
            Properties.Settings.Default.AllowDownloadThumbnail = AllowDownloadThumbnail;
            Properties.Settings.Default.SearchAllFields = SearchAllFields;
            Properties.Settings.Default.CaptureImages = CaptureImages;
            Properties.Settings.Default.DeleteNonFavoriteClipsOnExit = DeleteNonFavoriteClipsOnExit;
            Properties.Settings.Default.Max1CCodeSizeToColorize = Max1CCodeSizeToColorize;
            Properties.Settings.Default.EncryptDatabaseForCurrentUser = EncryptDatabaseForCurrentUser;
            Properties.Settings.Default.ReplaceDuplicates = ReplaceDuplicates;
            Properties.Settings.Default.GlobalHotkeySwitchMonitoring = GlobalHotkeySwitchMonitoring;
            Properties.Settings.Default.ReadWindowTitles = ReadWindowTitles;
            Properties.Settings.Default.MaxCellsToCaptureImage = MaxCellsToCaptureImage;
            Properties.Settings.Default.MaxCellsToCaptureFormattedText = MaxCellsToCaptureFormattedText;
            Properties.Settings.Default.DatabaseFile = DatabaseFile;
            Properties.Settings.Default.PlaySoundOnClipCapture = PlaySoundOnClipCapture;
            Properties.Settings.Default.MonospacedFont = MonospacedFont;
            Properties.Settings.Default.WordWrap = WordWrap;
            Properties.Settings.Default.ShowNativeTextFormatting = ShowNativeTextFormatting;
            Properties.Settings.Default.SearchWordsIndependently = SearchWordsIndependently;
            Properties.Settings.Default.SearchWildcards = SearchWildcards;
            Properties.Settings.Default.AutoSelectMatch = AutoSelectMatch;
            Properties.Settings.Default.SearchCaseSensitive = SearchCaseSensitive;
            Properties.Settings.Default.SearchIgnoreBigTexts = SearchIgnoreBigTexts;
            Properties.Settings.Default.ClipTempFileFolder = ClipTempFileFolder;
            Properties.Settings.Default.IgnoreExclusiveFormatClipCapture = IgnoreExclusiveFormatClipCapture;
            //Properties.Settings.Default.RestoreCaretPositionOnFocusReturn = RestoreCaretPositionOnFocusReturn;
            Properties.Settings.Default.GlobalHotkeyPasteText = GlobalHotkeyPasteText;
            Properties.Settings.Default.GlobalHotkeySimulateInput = GlobalHotkeySimulateInput;
            Properties.Settings.Default.UseFormattingInDuplicateDetection = UseFormattingInDuplicateDetection;
            Properties.Settings.Default.IgnoreApplicationsClipCapture = IgnoreApplicationsClipCapture;
            //Properties.Settings.Default.CopyTextInAnyWindowOnCTRLF3 = CopyTextInAnyWindowOnCTRLF3;
            Properties.Settings.Default.FastWindowOpen = FastWindowOpen;
            Properties.Settings.Default.HistoryDepthDays = HistoryDepthDays;
            Properties.Settings.Default.HistoryDepthNumber = HistoryDepthNumber;
            Properties.Settings.Default.Font = DefaultFont;
            Properties.Settings.Default.Language = Language;
            Properties.Settings.Default.TextCompareApplication = TextCompareApplication;
            Properties.Settings.Default.TextEditor = TextEditor;
            Properties.Settings.Default.RtfEditor = RtfEditor;
            Properties.Settings.Default.HtmlEditor = HtmlEditor;
            Properties.Settings.Default.ImageEditor = ImageEditor;
            Properties.Settings.Default.MaxClipSizeKB = MaxClipSizeKB;
            Properties.Settings.Default.Font = DefaultFont;
            Properties.Settings.Default.ShowApplicationIconColumn = ShowApplicationIconColumn;
            Properties.Settings.Default.ShowSecondaryColumns = ShowSecondaryColumns;
            Properties.Settings.Default.AutoCheckForUpdate = AutoCheckUpdate;
            Properties.Settings.Default.MoveCopiedClipToTop = MoveCopiedClipToTop;
            Properties.Settings.Default.WindowAutoPosition = WindowAutoPosition;
            Properties.Settings.Default.GlobalHotkeyOpenLast = GlobalHotkeyOpenLast;
            Properties.Settings.Default.GlobalHotkeyOpenCurrent = GlobalHotkeyOpenCurrent;
            Properties.Settings.Default.GlobalHotkeyOpenFavorites = GlobalHotkeyOpenFavorites;
            Properties.Settings.Default.GlobalHotkeyIncrementalPaste = GlobalHotkeyIncrementalPaste;
            Properties.Settings.Default.GlobalHotkeyDecrementalPaste = GlobalHotkeyDecrementalPaste;
            Properties.Settings.Default.GlobalHotkeyCompareLastClips = GlobalHotkeyCompareLastClips;
            Properties.Settings.Default.Language = Language;

            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            string Keyname = Application.ProductName;
            try
            {
                object currentValue = reg.GetValue(Keyname);
                if (Autostart)
                {
                    string CommandLine = "\"" + Application.ExecutablePath + "\"" + " /m";
                    if (PortableMode)
                        CommandLine += " /p";
                    if (!CommandLine.Equals(currentValue))
                        reg.SetValue(Keyname, CommandLine);
                }
                else if (currentValue != null)
                    reg.DeleteValue(Keyname);
                reg.Close();
                Properties.Settings.Default.Autostart = Autostart;
            }
            catch
            { }
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

        [GlobalizedCategory("Info")]
        [ReadOnly(true)]
        public string AutostartPath { get; set; }

        [GlobalizedCategory("Info")]
        [ReadOnly(true)]
        public string CurrentPath { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool IgnoreExclusiveFormatClipCapture { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool ShowApplicationIconColumn { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool ShowSecondaryColumns { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool AutoCheckUpdate { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool FastWindowOpen { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool MoveCopiedClipToTop { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool WindowAutoPosition { get; set; }

        //[GlobalizedCategory("Other")]
        //[Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        //public bool RestoreCaretPositionOnFocusReturn { get; set; }

        [GlobalizedCategory("GlobalHotkeys")]
        [TypeConverterAttribute(typeof(HotkeyConverter))]
        [EditorAttribute(typeof(HotkeyEditor), typeof(UITypeEditor))]
        public string GlobalHotkeyOpenCurrent { get; set; }

        [GlobalizedCategory("GlobalHotkeys")]
        [TypeConverterAttribute(typeof(HotkeyConverter))]
        [EditorAttribute(typeof(HotkeyEditor), typeof(UITypeEditor))]
        public string GlobalHotkeyOpenLast { get; set; }

        [GlobalizedCategory("GlobalHotkeys")]
        [TypeConverterAttribute(typeof(HotkeyConverter))]
        [EditorAttribute(typeof(HotkeyEditor), typeof(UITypeEditor))]
        public string GlobalHotkeyOpenFavorites { get; set; }

        [GlobalizedCategory("GlobalHotkeys")]
        [TypeConverterAttribute(typeof(HotkeyConverter))]
        [EditorAttribute(typeof(HotkeyEditor), typeof(UITypeEditor))]
        public string GlobalHotkeyIncrementalPaste { get; set; }

        [GlobalizedCategory("GlobalHotkeys")]
        [TypeConverterAttribute(typeof(HotkeyConverter))]
        [EditorAttribute(typeof(HotkeyEditor), typeof(UITypeEditor))]
        public string GlobalHotkeyDecrementalPaste { get; set; }

        [GlobalizedCategory("GlobalHotkeys")]
        [TypeConverterAttribute(typeof(HotkeyConverter))]
        [EditorAttribute(typeof(HotkeyEditor), typeof(UITypeEditor))]
        public string GlobalHotkeyCompareLastClips { get; set; }

        [GlobalizedCategory("GlobalHotkeys")]
        [TypeConverterAttribute(typeof(HotkeyConverter))]
        [EditorAttribute(typeof(HotkeyEditor), typeof(UITypeEditor))]
        public string GlobalHotkeyPasteText { get; set; }

        [GlobalizedCategory("GlobalHotkeys")]
        [TypeConverterAttribute(typeof(HotkeyConverter))]
        [EditorAttribute(typeof(HotkeyEditor), typeof(UITypeEditor))]
        public string GlobalHotkeySimulateInput { get; set; }

        [GlobalizedCategory("GlobalHotkeys")]
        [TypeConverterAttribute(typeof(HotkeyConverter))]
        [EditorAttribute(typeof(HotkeyEditor), typeof(UITypeEditor))]
        public string GlobalHotkeySwitchMonitoring { get; set; }

        [GlobalizedCategory("GlobalHotkeys")]
        [TypeConverterAttribute(typeof(HotkeyConverter))]
        [EditorAttribute(typeof(HotkeyEditor), typeof(UITypeEditor))]
        public string GlobalHotkeyForcedCapture { get; set; }

       //[GlobalizedCategory("GlobalHotkeys")]
       // [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
       // public bool CopyTextInAnyWindowOnCTRLF3 { get; set; }

        [GlobalizedCategory("Other")]
        public int MaxClipSizeKB { get; set; }

        [GlobalizedCategory("Other")]
        public int HistoryDepthNumber { get; set; }

        [GlobalizedCategory("Other")]
        public int HistoryDepthDays { get; set; }

        [GlobalizedCategory("Other")]
        public int MaxCellsToCaptureFormattedText { get; set; }

        [GlobalizedCategory("Other")]
        public int MaxCellsToCaptureImage { get; set; }

        [GlobalizedCategory("Other")]
        public int Max1CCodeSizeToColorize { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool ReadWindowTitles { get; set; }

        [GlobalizedCategory("Other")]
        public Font DefaultFont { get; set; }

        [GlobalizedCategory("Other")]
        [TypeConverter(typeof(LanguageConverter))]
        public string Language { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool Autostart { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool UseFormattingInDuplicateDetection { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool ReplaceDuplicates { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool EncryptDatabaseForCurrentUser { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool DeleteNonFavoriteClipsOnExit { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool CaptureImages { get; set; }

        [GlobalizedCategory("Other")]
        [EditorAttribute(typeof(FolderNameEditor), typeof(UITypeEditor))]
        public string ClipTempFileFolder { get; set; }

        [GlobalizedCategory("Applications")]
        //[EditorAttribute(typeof(FileNameEditor), typeof(UITypeEditor))]
        [EditorAttribute(typeof(ApplicationPathEditor), typeof(UITypeEditor))]
        public string TextCompareApplication { get; set; }

        [GlobalizedCategory("Applications")]
        //[EditorAttribute(typeof(FileNameEditor), typeof(UITypeEditor))]
        [EditorAttribute(typeof(ApplicationPathEditor), typeof(UITypeEditor))]
        public string TextEditor { get; set; }

        [GlobalizedCategory("Applications")]
        //[EditorAttribute(typeof(FileNameEditor), typeof(UITypeEditor))]
        [EditorAttribute(typeof(ApplicationPathEditor), typeof(UITypeEditor))]
        public string HtmlEditor { get; set; }

        [GlobalizedCategory("Applications")]
        //[EditorAttribute(typeof(FileNameEditor), typeof(UITypeEditor))]
        [EditorAttribute(typeof(ApplicationPathEditor), typeof(UITypeEditor))]
        public string RtfEditor { get; set; }

        [GlobalizedCategory("Applications")]
        //[EditorAttribute(typeof(FileNameEditor), typeof(UITypeEditor))]
        [EditorAttribute(typeof(ApplicationPathEditor), typeof(UITypeEditor))]
        public string ImageEditor { get; set; }

        [GlobalizedCategory("Applications")]
        [Editor(typeof(AppListEditor), typeof(UITypeEditor))]
        public StringCollection IgnoreApplicationsClipCapture { get; set; }

        [GlobalizedCategory("Applications")]
        [Editor(typeof(UITypeEditor), typeof(UITypeEditor))]
        public string IgnoreUrlsClipCapture { get; set; }

        [GlobalizedCategory("Search")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool AutoSelectMatch { get; set; }

        [GlobalizedCategory("Search")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool SearchWildcards { get; set; }

        [GlobalizedCategory("Search")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool SearchWordsIndependently { get; set; }

        [GlobalizedCategory("Search")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool SearchCaseSensitive { get; set; }

        [GlobalizedCategory("Search")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool SearchIgnoreBigTexts { get; set; }

        [GlobalizedCategory("Search")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool FilterListBySearchString { get; set; }

        [GlobalizedCategory("Search")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool AutoSelectMatchedClip { get; set; }

        [GlobalizedCategory("Search")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool SearchAllFields { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool AllowDownloadThumbnail { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool ConfirmationBeforeDelete { get; set; }

        [GlobalizedCategory("Other")]
        public string DelimiterForTextJoin { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool DontSendPaste { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool ShowNativeTextFormatting { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool WordWrap { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(MyBoolEditor), typeof(UITypeEditor))]
        public bool MonospacedFont { get; set; }

        [GlobalizedCategory("Other")]
        [Editor(typeof(SoundFileNameEditor), typeof(UITypeEditor))]
        public string PlaySoundOnClipCapture { get; set; }

        [GlobalizedCategory("Other")]
        [EditorAttribute(typeof(DatabaseFileNameEditor), typeof(UITypeEditor))]
        public string DatabaseFile { get; set; }
    }
}
