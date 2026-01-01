using System;
using System.Collections.Generic;
using System.IO;

namespace XZPToolv3.XUI
{
    /// <summary>
    /// Singleton class for managing XUI class definitions and hierarchies.
    /// Ported from XUIWorkshop by TeamFSD.
    /// </summary>
    public sealed class XuiClass
    {
        // Singleton instance
        private static readonly XuiClass instance = new XuiClass();

        static XuiClass() { }

        private XuiClass() { }

        public static XuiClass Instance
        {
            get { return instance; }
        }

        // List of all loaded XUI class definitions
        private List<XUI_CLASS> loc_extensionClassList;

        // Track missing classes for debugging
        public List<string> MissingClasses { get; private set; }

        /// <summary>
        /// Build the class list from Extension XML files
        /// </summary>
        public void BuildClass(List<string> xmlExtensionsList)
        {
            loc_extensionClassList = new List<XUI_CLASS>();
            MissingClasses = new List<string>();

            UpdateClassList(xmlExtensionsList);
        }

        /// <summary>
        /// Build the class list from the default Extensions directory
        /// </summary>
        public void BuildClassFromDefaultExtensions()
        {
            string extensionsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XUI", "Extensions");

            if (!Directory.Exists(extensionsDir))
            {
                throw new DirectoryNotFoundException($"Extensions directory not found: {extensionsDir}");
            }

            List<string> xmlFiles = new List<string>();
            xmlFiles.Add(Path.Combine(extensionsDir, "XuiElements.xml"));
            xmlFiles.Add(Path.Combine(extensionsDir, "XamElements.xml"));
            xmlFiles.Add(Path.Combine(extensionsDir, "CustomXuiElements.xml"));
            xmlFiles.Add(Path.Combine(extensionsDir, "ControlPackLegacyControl.xml"));

            // Only add files that exist
            List<string> existingFiles = new List<string>();
            foreach (string file in xmlFiles)
            {
                if (File.Exists(file))
                    existingFiles.Add(file);
            }

            if (existingFiles.Count == 0)
            {
                throw new FileNotFoundException("No Extension XML files found in " + extensionsDir);
            }

            BuildClass(existingFiles);
        }

        /// <summary>
        /// Find a class definition by name
        /// </summary>
        public XUI_CLASS FindClass(string className)
        {
            foreach (XUI_CLASS tmpClass in loc_extensionClassList)
            {
                if (tmpClass.szClassName == className)
                    return tmpClass;
            }

            // Return empty object if not found
            return default(XUI_CLASS);
        }

        /// <summary>
        /// Find a property definition by name in a given class hierarchy
        /// </summary>
        public XUIELEM_PROP_DEF FindProperty(string propertyName, string mostDerivedClass)
        {
            List<List<XUIELEM_PROP_DEF>> hierarchy = GetHierarchy(mostDerivedClass);

            if (hierarchy != null)
            {
                foreach (List<XUIELEM_PROP_DEF> baseList in hierarchy)
                {
                    foreach (XUIELEM_PROP_DEF tmpProp in baseList)
                    {
                        if (tmpProp.PropName == propertyName)
                            return tmpProp;
                    }
                }
            }

            return default(XUIELEM_PROP_DEF);
        }

        /// <summary>
        /// Find a figure property (Fill, Stroke, Gradient)
        /// </summary>
        public XUIELEM_PROP_DEF FindFigureProperty(string propertyName, string figureType)
        {
            List<XUIELEM_PROP_DEF> tmpClass;

            if (figureType == "Stroke")
                tmpClass = GetStrokePropArray();
            else if (figureType == "Fill")
                tmpClass = GetFillPropArray();
            else
                tmpClass = GetGradientPropArray();

            foreach (XUIELEM_PROP_DEF tmpProp in tmpClass)
            {
                if (tmpProp.PropName == propertyName)
                    return tmpProp;
            }

            return default(XUIELEM_PROP_DEF);
        }

        /// <summary>
        /// Update the class list from Extension XML files
        /// </summary>
        public void UpdateClassList(List<string> xmlExtensionsList)
        {
            // Empty List
            loc_extensionClassList.Clear();

            XmlExtensionParse parseXml = new XmlExtensionParse();
            List<XUI_CLASS> tempArray = new List<XUI_CLASS>();

            foreach (string path in xmlExtensionsList)
            {
                // Get Array of classes
                tempArray = parseXml.ReadExtension(path);

                // Add each class item to extension class array
                foreach (XUI_CLASS tempClass in tempArray)
                    loc_extensionClassList.Add(tempClass);
            }
        }

        /// <summary>
        /// Returns a list of property definition arrays based on the hierarchy of the given class.
        /// Index 0 is the root base class.
        /// </summary>
        public List<List<XUIELEM_PROP_DEF>> GetHierarchy(string mostDerivedClass)
        {
            List<List<XUIELEM_PROP_DEF>> hierarchy = new List<List<XUIELEM_PROP_DEF>>();
            XUI_CLASS temp = FindClass(mostDerivedClass);

            // Class missing - break out
            if (temp.szClassName == null)
            {
                // Track missing classes
                if (!MissingClasses.Contains(mostDerivedClass))
                    MissingClasses.Add(mostDerivedClass);

                return null;
            }

            hierarchy.Add(temp.PropDefs);

            // Max level I've seen is 3 but 5 is just to be safe
            for (int i = 0; i < 5; i++)
            {
                if (temp.szBaseClassName != "(null)")
                {
                    temp = FindClass(temp.szBaseClassName);

                    // Check if base class was found
                    if (temp.szClassName == null)
                        break;

                    hierarchy.Add(temp.PropDefs);
                }
                else
                {
                    break;
                }
            }

            // Reverse order so we start with Root class props
            hierarchy.Reverse();

            return hierarchy;
        }

        public List<XUIELEM_PROP_DEF> GetFillPropArray()
        {
            XUI_CLASS temp = FindClass("XuiFigureFill");
            return temp.PropDefs;
        }

        public List<XUIELEM_PROP_DEF> GetGradientPropArray()
        {
            XUI_CLASS temp = FindClass("XuiFigureFillGradient");
            return temp.PropDefs;
        }

        public List<XUIELEM_PROP_DEF> GetStrokePropArray()
        {
            XUI_CLASS temp = FindClass("XuiFigureStroke");
            return temp.PropDefs;
        }

        public List<XUIELEM_PROP_DEF> GetIgnoredPropArray()
        {
            XUI_CLASS temp = FindClass("IgnoredProps");
            return temp.PropDefs;
        }

        /// <summary>
        /// Accessor to the extension class list
        /// </summary>
        public List<XUI_CLASS> ExtensionClassList
        {
            get { return loc_extensionClassList; }
        }
    }
}
