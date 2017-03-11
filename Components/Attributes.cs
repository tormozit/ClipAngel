using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Reflection;
using ClipAngel;

// https://www.codeproject.com/Articles/2138/Globalized-property-grid
// https://www.codeproject.com/Articles/4341/Globalized-Property-Grid-Revisited
namespace GlobalizedPropertyGrid
{
    // This attribute may be added to properties to control the resource table name and 
    // keys for accesging the property name and description. 
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class GlobalizedPropertyAttribute : Attribute
    {
        private String resourceName = "";
        private String resourceDescription = "";
        private String resourceTable = "";

        public GlobalizedPropertyAttribute(String name)
        {
            resourceName = name;
        }

        // The key for a property name into the resource table.
        // If empty the property name will be used by default.
        public String Name
        {
            get { return resourceName; }
            set { resourceName = value; }
        }

        // The key for a property description into the resource table.
        // If empty the property name appended by 'Description' will be used by default.
        public String Description
        {
            get { return resourceDescription; }
            set { resourceDescription = value; }
        }

        // The name fully qualified name of the resource table to use, f.ex.
        // "GlobalizedPropertyGrid.Person".
        public String Table
        {
            get { return resourceTable; }
            set { resourceTable = value; }
        }
    }

    // This attribute may be added to properties to control the resource table name and 
    // keys for accesging the property name and description. 
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class GlobalizedObjectAttribute : Attribute
    {
        private String resourceTable = "";

        public GlobalizedObjectAttribute(String name)
        {
            resourceTable = name;
        }

        // resource table name to use for the class. 
        // If this string is empty the name of the class will be used by default. 
        public String Table
        {
            get { return resourceTable; }
        }
    }

    // This attribute class extends the CategoryAttribute class from the .NET framework
    // to support localization.
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    class GlobalizedCategoryAttribute : CategoryAttribute
    {
        string table = "";

        public GlobalizedCategoryAttribute() : base() { }
        public GlobalizedCategoryAttribute(string name) : base(name) { }

        // The name fully qualified name of the resource table to use, f.ex.
        // "GlobalizedPropertyGrid.Person".
        public string TableName
        {
            set { table = value; }
            get { return table; }
        }

        // This method will be called by the component framework to get a localized
        // string. Note: Only called once on first access.
        protected override string GetLocalizedString(string value)
        {
            string baseStr = base.GetLocalizedString(value);

            // Now use table name and display name id to access the resources.  
            //ResourceManager rm = new ResourceManager(table, Assembly.GetExecutingAssembly());
            string dummy;
            ResourceManager rm = Main.getResourceManager(out dummy);
            // Get the string from the resources.
            try
            {
                return rm.GetString(value);
            }
            catch (Exception)
            {
                return value;
            }
        }
    }


}
