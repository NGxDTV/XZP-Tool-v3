using System;
using System.Collections.Generic;
using System.Xml;
using System.Windows.Forms;

namespace XZPToolv3.XUI
{
    /// <summary>
    /// Class used to parse XUI Extension XML files.
    /// Ported from XUIWorkshop by TeamFSD.
    /// </summary>
    public class XmlExtensionParse
    {
        private string curTag;

        /// <summary>
        /// Read an Extension XML file and return list of XUI class definitions
        /// </summary>
        public List<XUI_CLASS> ReadExtension(string filePath)
        {
            XmlTextReader readXml = new XmlTextReader(filePath);
            List<XUI_CLASS> xmlClasses = new List<XUI_CLASS>();
            XUI_CLASS tmpClass = default(XUI_CLASS);
            XUIELEM_PROP_DEF tmpPropDef = default(XUIELEM_PROP_DEF);

            try
            {
                while (readXml.Read())
                {
                    switch (readXml.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (readXml.Name == "XUIClass")
                            {
                                tmpClass.szClassName = readXml.GetAttribute("Name");
                                tmpClass.szBaseClassName = readXml.GetAttribute("BaseClassName");

                                tmpClass.PropDefs = new List<XUIELEM_PROP_DEF>();
                                tmpClass.dwPropDefCount = 0;
                            }
                            else if (readXml.Name == "PropDef")
                            {
                                // Reset Values
                                tmpPropDef = default(XUIELEM_PROP_DEF);

                                // Assign values from xml
                                tmpPropDef.PropName = readXml.GetAttribute("Name");
                                tmpPropDef.Type = GetPropType(readXml.GetAttribute("Type"));
                                tmpPropDef.Id = Convert.ToInt32(readXml.GetAttribute("Id"));

                                if (readXml.GetAttribute("Flags") != null)
                                    tmpPropDef.Flags = GetFlagType(readXml.GetAttribute("Flags"));
                            }
                            else if (readXml.Name == "DefaultVal")
                                curTag = readXml.Name;
                            break;

                        case XmlNodeType.Text:
                            if (curTag == "DefaultVal")
                            {
                                tmpPropDef.DefaultVal = readXml.Value; // Will be read in as string
                            }
                            break;

                        case XmlNodeType.EndElement:
                            if (readXml.Name == "PropDef")
                            {
                                // Add Property
                                tmpClass.PropDefs.Add(tmpPropDef);
                                tmpClass.dwPropDefCount++;
                            }
                            if (readXml.Name == "XUIClass")
                            {
                                xmlClasses.Add(tmpClass);
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Extension File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Close xml reader
            readXml.Close();

            return xmlClasses;
        }

        /// <summary>
        /// Convert string type name to XUIELEM_PROP_TYPE enum
        /// </summary>
        private XUIELEM_PROP_TYPE GetPropType(string type)
        {
            if (type == "string")
                return XUIELEM_PROP_TYPE.XUI_EPT_STRING;
            else if (type == "unsigned")
                return XUIELEM_PROP_TYPE.XUI_EPT_UNSIGNED;
            else if (type == "bool")
                return XUIELEM_PROP_TYPE.XUI_EPT_BOOL;
            else if (type == "color")
                return XUIELEM_PROP_TYPE.XUI_EPT_COLOR;
            else if (type == "float")
                return XUIELEM_PROP_TYPE.XUI_EPT_FLOAT;
            else if (type == "vector")
                return XUIELEM_PROP_TYPE.XUI_EPT_VECTOR;
            else if (type == "integer")
                return XUIELEM_PROP_TYPE.XUI_EPT_INTEGER;
            else if (type == "quaternion")
                return XUIELEM_PROP_TYPE.XUI_EPT_QUATERNION;
            else if (type == "object")
                return XUIELEM_PROP_TYPE.XUI_EPT_OBJECT;
            else if (type == "custom")
                return XUIELEM_PROP_TYPE.XUI_EPT_CUSTOM;
            else
                return XUIELEM_PROP_TYPE.XUI_EPT_EMPTY;
        }

        /// <summary>
        /// Parse flags string to uint value
        /// </summary>
        private uint GetFlagType(string flags)
        {
            string[] flagArray = flags.Split('|');
            uint ret = 0;

            foreach (string flag in flagArray)
            {
                if (flag == "indexed")
                    ret += 0x1;
            }

            return ret;
        }
    }
}
