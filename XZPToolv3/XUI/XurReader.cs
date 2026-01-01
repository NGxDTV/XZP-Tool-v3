using System;
using System.Collections.Generic;
using System.IO;

namespace XZPToolv3.XUI
{
    /// <summary>
    /// Reader for XUR binary files. Ported from XUIWorkshop by TeamFSD.
    /// </summary>
    public class XurReader
    {
        // XUIBinaryTables holds all table data (strings, vectors, colors, etc.)
        private struct XUIBinaryTables
        {
            public List<string> StringTable;
            public List<XUIVECTOR> VectorTable;
            public List<XUIQUATERNION> QuaternionTable;
            public List<float> FloatTable;
            public List<XUICOLOR> ColorTable;
            public List<XUICUSTOM> CustomTable;

            public static XUIBinaryTables Initialize()
            {
                XUIBinaryTables bT = new XUIBinaryTables();
                bT.StringTable = new List<string>();
                bT.VectorTable = new List<XUIVECTOR>();
                bT.QuaternionTable = new List<XUIQUATERNION>();
                bT.FloatTable = new List<float>();
                bT.ColorTable = new List<XUICOLOR>();
                bT.CustomTable = new List<XUICUSTOM>();
                return bT;
            }
        }

        // Compound property data for object-type properties
        private struct XUICOMPOUNDPROPDATA
        {
            public uint Index;
            public List<XUIPROPERTYDATA> PropertyArray;
        }

        private struct XUIAnimationTables
        {
            public List<XUINAMEDFRAMEDATA> NamedFrameTable;
            public List<XUIKEYFRAMEDATA> KeyframeDataTable;
            public List<XUIKEYFRAMEPROPDATA> KeyframePropertyTable;

            public static XUIAnimationTables Initialize()
            {
                XUIAnimationTables aT = new XUIAnimationTables();
                aT.NamedFrameTable = new List<XUINAMEDFRAMEDATA>();
                aT.KeyframeDataTable = new List<XUIKEYFRAMEDATA>();
                aT.KeyframePropertyTable = new List<XUIKEYFRAMEPROPDATA>();
                return aT;
            }
        }

        // Load context for tracking objects and properties during reading
        private struct XUILoadContext
        {
            public uint ObjectCount;
            public uint MissingPropID;
            public List<XUIOBJECTDATA> ObjectArray;
            public List<List<XUIPROPERTYDATA>> ObjectPropArray;
            public List<XUICOMPOUNDPROPDATA> CompoundPropArray;

            public static XUILoadContext Initialize()
            {
                XUILoadContext lC = new XUILoadContext();
                lC.ObjectCount = 0;
                lC.MissingPropID = 0;
                lC.ObjectArray = new List<XUIOBJECTDATA>();
                lC.ObjectPropArray = new List<List<XUIPROPERTYDATA>>();
                lC.CompoundPropArray = new List<XUICOMPOUNDPROPDATA>();
                return lC;
            }
        }

        private XUIBinaryTables BinaryTables;
        private XUIAnimationTables AnimationTables;
        private XUILoadContext LoadContext;
        private BEBinaryReader reader;

        public XurReader(Stream stream)
        {
            reader = new BEBinaryReader(stream);
        }

        // Helper methods for reading packed data
        private uint ReadPackedUlong()
        {
            return reader.ReadPackedUlong();
        }

        private byte ReadByte()
        {
            return reader.ReadByte();
        }

        private uint ReadUInt32()
        {
            return reader.ReadUInt32();
        }

        private ushort ReadUInt16()
        {
            return reader.ReadUInt16();
        }

        private float ReadSingle()
        {
            return reader.ReadSingle();
        }

        private char ReadChar()
        {
            return reader.ReadChar();
        }

        /// <summary>
        /// Main entry point: Load all objects from the XUR binary file
        /// </summary>
        public XUIOBJECTDATA LoadObjectsFromBinary()
        {
            try
            {
                // Initialize Extension classes if not already loaded
                if (XuiClass.Instance.ExtensionClassList == null || XuiClass.Instance.ExtensionClassList.Count == 0)
                {
                    XuiClass.Instance.BuildClassFromDefaultExtensions();
                }

                // Reset stream to beginning
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                // Initialize structures
                BinaryTables = XUIBinaryTables.Initialize();
                AnimationTables = XUIAnimationTables.Initialize();
                LoadContext = XUILoadContext.Initialize();

                // Read header
                XUR8_HEADER header = ReadHeader();
                if (header.Magic != XurConstants.XUR8_MAGIC)
                    throw new Exception("Invalid XUR file: Magic number mismatch");

                // Read header info (not used but needed to advance stream)
                XUR8_HEADER_INFO headerInfo = ReadHeaderInfo();

                // Read section table
                Dictionary<XUR8_SECTION_NAME, XUR8_SECTION> sections = new Dictionary<XUR8_SECTION_NAME, XUR8_SECTION>();
                for (int i = 0; i < header.NumSections; i++)
                {
                    XUR8_SECTION section = ReadSectionInfo();
                    XUR8_SECTION_NAME sectionName = GetSectionNameFromMagic(section.Name);
                    if (sectionName != XUR8_SECTION_NAME.SECTION_COUNT)
                        sections[sectionName] = section;
                }

                // Load binary tables
                LoadBinaryTables(sections);

                // Load animation tables
                LoadAnimationTables(sections);

                // Load object data from DATA section
                if (!sections.ContainsKey(XUR8_SECTION_NAME.SECTION_DATA))
                    throw new Exception("XUR file missing DATA section");

                reader.BaseStream.Position = sections[XUR8_SECTION_NAME.SECTION_DATA].Offset;

                XUIOBJECTDATA rootObject = new XUIOBJECTDATA();
                XUIOBJECTDATA dummyParent = new XUIOBJECTDATA();
                LoadObjectFromBinary(ref rootObject, ref dummyParent);

                return rootObject;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load XUR file: {ex.Message}", ex);
            }
        }

        private XUR8_HEADER ReadHeader()
        {
            XUR8_HEADER header;
            header.Magic = ReadUInt32();
            header.Version = ReadUInt32();
            header.Flags = ReadUInt32();
            header.XuiVersion = ReadUInt16();
            header.BinSize = ReadUInt32();
            header.NumSections = ReadUInt16();
            return header;
        }

        private XUR8_HEADER_INFO ReadHeaderInfo()
        {
            XUR8_HEADER_INFO headerInfo;
            headerInfo.ObjectCount = ReadPackedUlong();
            headerInfo.PropertyCount = ReadPackedUlong();
            headerInfo.PropertyArrayCount = ReadPackedUlong();
            headerInfo.CompoundObjectPropCount = ReadPackedUlong();
            headerInfo.CompoundObjectPropArrayCount = ReadPackedUlong();
            headerInfo.PropPathDepthCount = ReadPackedUlong();
            headerInfo.TimelinePropPathCount = ReadPackedUlong();
            headerInfo.SubTimelineCount = ReadPackedUlong();
            headerInfo.KeyframePropCount = ReadPackedUlong();
            headerInfo.KeyframeDataCount = ReadPackedUlong();
            headerInfo.NamedFrameCount = ReadPackedUlong();
            headerInfo.ObjectsWithChildrenCount = ReadPackedUlong();
            return headerInfo;
        }

        private XUR8_SECTION ReadSectionInfo()
        {
            XUR8_SECTION section;
            section.Name = ReadUInt32();
            section.Offset = ReadUInt32();
            section.Size = ReadUInt32();
            return section;
        }

        private XUR8_SECTION_NAME GetSectionNameFromMagic(uint magic)
        {
            switch (magic)
            {
                case 0x5354524E: return XUR8_SECTION_NAME.SECTION_STRN; // STRN
                case 0x56454354: return XUR8_SECTION_NAME.SECTION_VECT; // VECT
                case 0x51554154: return XUR8_SECTION_NAME.SECTION_QUAT; // QUAT
                case 0x464C4F54: return XUR8_SECTION_NAME.SECTION_FLOT; // FLOT
                case 0x434F4C52: return XUR8_SECTION_NAME.SECTION_COLR; // COLR
                case 0x43555354: return XUR8_SECTION_NAME.SECTION_CUST; // CUST
                case 0x44415441: return XUR8_SECTION_NAME.SECTION_DATA; // DATA
                case 0x4B455950: return XUR8_SECTION_NAME.SECTION_KEYP; // KEYP
                case 0x4B455944: return XUR8_SECTION_NAME.SECTION_KEYD; // KEYD
                case 0x4E414D45: return XUR8_SECTION_NAME.SECTION_NAME; // NAME
                case 0x44425547: return XUR8_SECTION_NAME.SECTION_DBUG; // DBUG
                default: return XUR8_SECTION_NAME.SECTION_COUNT;
            }
        }

        private void LoadBinaryTables(Dictionary<XUR8_SECTION_NAME, XUR8_SECTION> sections)
        {
            // Load String Table
            if (sections.ContainsKey(XUR8_SECTION_NAME.SECTION_STRN))
            {
                reader.BaseStream.Position = sections[XUR8_SECTION_NAME.SECTION_STRN].Offset;
                BinaryTables.StringTable = ReadStringTable(sections[XUR8_SECTION_NAME.SECTION_STRN].Size);
            }

            // Load Vector Table
            if (sections.ContainsKey(XUR8_SECTION_NAME.SECTION_VECT))
            {
                reader.BaseStream.Position = sections[XUR8_SECTION_NAME.SECTION_VECT].Offset;
                BinaryTables.VectorTable = ReadVectorTable(sections[XUR8_SECTION_NAME.SECTION_VECT].Size);
            }

            // Load Quaternion Table
            if (sections.ContainsKey(XUR8_SECTION_NAME.SECTION_QUAT))
            {
                reader.BaseStream.Position = sections[XUR8_SECTION_NAME.SECTION_QUAT].Offset;
                BinaryTables.QuaternionTable = ReadQuaternionTable(sections[XUR8_SECTION_NAME.SECTION_QUAT].Size);
            }

            // Load Float Table
            if (sections.ContainsKey(XUR8_SECTION_NAME.SECTION_FLOT))
            {
                reader.BaseStream.Position = sections[XUR8_SECTION_NAME.SECTION_FLOT].Offset;
                BinaryTables.FloatTable = ReadFloatTable(sections[XUR8_SECTION_NAME.SECTION_FLOT].Size);
            }

            // Load Color Table
            if (sections.ContainsKey(XUR8_SECTION_NAME.SECTION_COLR))
            {
                reader.BaseStream.Position = sections[XUR8_SECTION_NAME.SECTION_COLR].Offset;
                BinaryTables.ColorTable = ReadColorTable(sections[XUR8_SECTION_NAME.SECTION_COLR].Size);
            }

            // Load Custom Table
            if (sections.ContainsKey(XUR8_SECTION_NAME.SECTION_CUST))
            {
                reader.BaseStream.Position = sections[XUR8_SECTION_NAME.SECTION_CUST].Offset;
                BinaryTables.CustomTable = ReadCustomTable(sections[XUR8_SECTION_NAME.SECTION_CUST].Size);
            }
        }

        private List<string> ReadStringTable(uint size)
        {
            List<string> stringTable = new List<string>();
            uint strLen = ReadUInt32();
            ushort strCount = ReadUInt16();

            // First entry is always empty string
            stringTable.Add("");

            string newString = "";
            char data = '\0';

            while (strCount > 0)
            {
                data = ReadChar();
                if (data != '\0')
                    newString += data.ToString();

                if (data == '\0')
                {
                    stringTable.Add(newString);
                    strCount--;
                    newString = "";
                }
            }

            return stringTable;
        }

        private void LoadAnimationTables(Dictionary<XUR8_SECTION_NAME, XUR8_SECTION> sections)
        {
            if (sections.ContainsKey(XUR8_SECTION_NAME.SECTION_KEYP))
            {
                reader.BaseStream.Position = sections[XUR8_SECTION_NAME.SECTION_KEYP].Offset;
                ReadKeyframePropertyTable(sections[XUR8_SECTION_NAME.SECTION_KEYP].Size);
            }

            if (sections.ContainsKey(XUR8_SECTION_NAME.SECTION_KEYD))
            {
                reader.BaseStream.Position = sections[XUR8_SECTION_NAME.SECTION_KEYD].Offset;
                ReadKeyframeDataTable(sections[XUR8_SECTION_NAME.SECTION_KEYD].Size);
            }

            if (sections.ContainsKey(XUR8_SECTION_NAME.SECTION_NAME))
            {
                reader.BaseStream.Position = sections[XUR8_SECTION_NAME.SECTION_NAME].Offset;
                ReadNamedFrameTable(sections[XUR8_SECTION_NAME.SECTION_NAME].Size);
            }
        }

        private List<XUIVECTOR> ReadVectorTable(uint size)
        {
            List<XUIVECTOR> vectorTable = new List<XUIVECTOR>();
            uint entries = size / 12;

            for (uint i = 0; i < entries; i++)
            {
                XUIVECTOR vector;
                vector.x = ReadSingle();
                vector.y = ReadSingle();
                vector.z = ReadSingle();
                vectorTable.Add(vector);
            }

            return vectorTable;
        }

        private List<XUIQUATERNION> ReadQuaternionTable(uint size)
        {
            List<XUIQUATERNION> quatTable = new List<XUIQUATERNION>();
            uint entries = size / 16;

            for (uint i = 0; i < entries; i++)
            {
                XUIQUATERNION quat;
                quat.x = ReadSingle();
                quat.y = ReadSingle();
                quat.z = ReadSingle();
                quat.w = ReadSingle();
                quatTable.Add(quat);
            }

            return quatTable;
        }

        private List<float> ReadFloatTable(uint size)
        {
            List<float> floatTable = new List<float>();
            uint entries = size / 4;

            for (uint i = 0; i < entries; i++)
            {
                floatTable.Add(ReadSingle());
            }

            return floatTable;
        }

        private List<XUICOLOR> ReadColorTable(uint size)
        {
            List<XUICOLOR> colorTable = new List<XUICOLOR>();
            uint entries = size / 4;

            for (uint i = 0; i < entries; i++)
            {
                XUICOLOR color;
                color.argb = ReadUInt32();
                colorTable.Add(color);
            }

            return colorTable;
        }

        private List<XUICUSTOM> ReadCustomTable(uint size)
        {
            List<XUICUSTOM> customTable = new List<XUICUSTOM>();
            long startPos = reader.BaseStream.Position;
            long endPos = startPos + size;

            while (reader.BaseStream.Position < endPos)
            {
                XUICUSTOM custom = new XUICUSTOM();
                custom.Offset = (uint)(reader.BaseStream.Position - startPos);
                custom.DataLen = ReadUInt32();
                custom.BoundingBox.x = ReadSingle();
                custom.BoundingBox.y = ReadSingle();
                custom.NumPoints = ReadUInt32();

                for (uint i = 0; i < custom.NumPoints; i++)
                {
                    XUIBEZIERPOINT pt;
                    pt.vecPoint.x = ReadSingle();
                    pt.vecPoint.y = ReadSingle();
                    pt.vecCtrl1.x = ReadSingle();
                    pt.vecCtrl1.y = ReadSingle();
                    pt.vecCtrl2.x = ReadSingle();
                    pt.vecCtrl2.y = ReadSingle();
                    custom.Points.Add(pt);
                }

                customTable.Add(custom);
            }

            return customTable;
        }

        /// <summary>
        /// Recursively load an object and its children from the binary stream
        /// </summary>
        private void LoadObjectFromBinary(ref XUIOBJECTDATA baseObjectData, ref XUIOBJECTDATA parentData)
        {
            baseObjectData.ObjectId = LoadContext.ObjectCount;
            LoadContext.ObjectCount++;

            // Read class name string index and flags
            uint strId = ReadPackedUlong();
            uint flag = ReadByte();

            if (strId >= BinaryTables.StringTable.Count)
                return;

            string className = BinaryTables.StringTable[(int)strId];
            baseObjectData.ClassName = className;
            baseObjectData.Flags = flag;

            // Check if object has properties (flag bit 0)
            if ((baseObjectData.Flags & 0x1) == 0x1)
            {
                // Object has its own properties
                uint numPropArrays = ReadPackedUlong();
                LoadObjectPropsFromBinary(className, numPropArrays, ref baseObjectData.PropertyArray);
                LoadContext.ObjectPropArray.Add(baseObjectData.PropertyArray);
            }
            else if ((baseObjectData.Flags & 0x8) == 0x8)
            {
                // Object reuses properties from another object
                uint objectIdx = ReadPackedUlong();
                if (objectIdx < LoadContext.ObjectPropArray.Count)
                    baseObjectData.PropertyArray = LoadContext.ObjectPropArray[(int)objectIdx];
            }

            // If property has no Id, store generic Id using class name and counter
            object idValue = baseObjectData.GetPropVal("Id");
            if (idValue == null || idValue.ToString() == "")
            {
                if (LoadContext.MissingPropID == 0)
                    baseObjectData.PropertyArray.Add(CreatePropertyData("Id", baseObjectData.ClassName));
                else
                    baseObjectData.PropertyArray.Add(CreatePropertyData("Id", baseObjectData.ClassName + LoadContext.MissingPropID));

                LoadContext.MissingPropID++;
            }

            // Store hierarchy ID path
            if (baseObjectData.ObjectId == 0)
                baseObjectData.HeriarchyId = baseObjectData.ClassName;
            else
                baseObjectData.HeriarchyId = parentData.HeriarchyId + '#' + baseObjectData.GetPropVal("Id").ToString();

            // Check if object has children (flag bit 1)
            if ((baseObjectData.Flags & 0x2) == 0x2)
            {
                uint numChildren = ReadPackedUlong();
                for (uint childIdx = 0; childIdx < numChildren; childIdx++)
                {
                    XUIOBJECTDATA newChild = new XUIOBJECTDATA();
                    LoadObjectFromBinary(ref newChild, ref baseObjectData);
                    baseObjectData.ChildrenArray.Add(newChild);
                }
            }

            // Check if object has timelines (flag bit 2)
            if ((baseObjectData.Flags & 0x4) == 0x4)
            {
                LoadTimelineDataFromBinary(className, ref baseObjectData);
            }

            // Add this object to our array for later use
            LoadContext.ObjectArray.Add(baseObjectData);
        }

        private XUIPROPERTYDATA CreatePropertyData(string propName, object propValue)
        {
            XUIPROPERTYDATA prop = new XUIPROPERTYDATA();
            XUIELEM_PROP_DEF propDef = new XUIELEM_PROP_DEF();
            propDef.PropName = propName;
            propDef.Type = XUIELEM_PROP_TYPE.XUI_EPT_STRING;
            prop.PropDef = propDef;
            prop.PropValue = propValue;
            prop.PropType = XUIELEM_PROP_TYPE.XUI_EPT_STRING;
            return prop;
        }

        /// <summary>
        /// Load properties for an object using the class hierarchy
        /// </summary>
        private void LoadObjectPropsFromBinary(string className, uint numPropArrays, ref List<XUIPROPERTYDATA> propertyData)
        {
            // Get class hierarchy (base classes first, derived class last)
            List<List<XUIELEM_PROP_DEF>> classArray = XuiClass.Instance.GetHierarchy(className);

            if (classArray == null)
            {
                // Class not found - skip properties
                for (uint i = 0; i < numPropArrays; i++)
                {
                    uint propMask = ReadPackedUlong();
                    // Can't skip accurately without knowing property types
                }
                return;
            }

            // Load property data for each class in the hierarchy
            foreach (List<XUIELEM_PROP_DEF> propArray in classArray)
            {
                if (propArray != null)
                    LoadPropertyDataFromBinary(propArray, ref propertyData);
            }
        }

        /// <summary>
        /// Load property data for a single class (one property mask worth of properties)
        /// </summary>
        private void LoadPropertyDataFromBinary(List<XUIELEM_PROP_DEF> propArray, ref List<XUIPROPERTYDATA> propertyData)
        {
            uint propMask = ReadPackedUlong();
            int propIndex = 0;

            foreach (XUIELEM_PROP_DEF propEntry in propArray)
            {
                // Check if this property is included in the property mask
                int flag = 1 << propIndex;
                if ((propMask & flag) == (uint)flag)
                {
                    // Check if this property is indexed (can have multiple values)
                    uint indexCount = 1;
                    if ((propEntry.Flags & 0x1) == 0x1)
                    {
                        // Indexed property - read count
                        indexCount = ReadPackedUlong();
                    }

                    // Loop through each index and read the property value
                    for (uint curIdx = 0; curIdx < indexCount; curIdx++)
                    {
                        XUIPROPERTYDATA propData = new XUIPROPERTYDATA();
                        propData.Flags = propEntry.Flags;
                        propData.PropType = propEntry.Type;
                        propData.PropDef = propEntry;
                        propData.Index = curIdx;

                        // Read the property value based on its type
                        switch (propData.PropType)
                        {
                            case XUIELEM_PROP_TYPE.XUI_EPT_BOOL:
                                propData.PropValue = ReadByte() > 0;
                                propertyData.Add(propData);
                                break;

                            case XUIELEM_PROP_TYPE.XUI_EPT_INTEGER:
                                propData.PropValue = (int)ReadPackedUlong();
                                propertyData.Add(propData);
                                break;

                            case XUIELEM_PROP_TYPE.XUI_EPT_UNSIGNED:
                                propData.PropValue = ReadPackedUlong();
                                propertyData.Add(propData);
                                break;

                            case XUIELEM_PROP_TYPE.XUI_EPT_STRING:
                                uint stringIdx = ReadPackedUlong();
                                if (stringIdx < BinaryTables.StringTable.Count)
                                    propData.PropValue = BinaryTables.StringTable[(int)stringIdx];
                                else
                                    propData.PropValue = "";
                                propertyData.Add(propData);
                                break;

                            case XUIELEM_PROP_TYPE.XUI_EPT_FLOAT:
                                uint floatIdx = ReadPackedUlong();
                                if (floatIdx < BinaryTables.FloatTable.Count)
                                    propData.PropValue = BinaryTables.FloatTable[(int)floatIdx];
                                else
                                    propData.PropValue = 0.0f;
                                propertyData.Add(propData);
                                break;

                            case XUIELEM_PROP_TYPE.XUI_EPT_QUATERNION:
                                uint quatIdx = ReadPackedUlong();
                                if (quatIdx < BinaryTables.QuaternionTable.Count)
                                    propData.PropValue = BinaryTables.QuaternionTable[(int)quatIdx];
                                else
                                    propData.PropValue = new XUIQUATERNION();
                                propertyData.Add(propData);
                                break;

                            case XUIELEM_PROP_TYPE.XUI_EPT_VECTOR:
                                uint vecIdx = ReadPackedUlong();
                                if (vecIdx < BinaryTables.VectorTable.Count)
                                    propData.PropValue = BinaryTables.VectorTable[(int)vecIdx];
                                else
                                    propData.PropValue = new XUIVECTOR();
                                propertyData.Add(propData);
                                break;

                            case XUIELEM_PROP_TYPE.XUI_EPT_COLOR:
                                uint colIdx = ReadPackedUlong();
                                if (colIdx < BinaryTables.ColorTable.Count)
                                    propData.PropValue = BinaryTables.ColorTable[(int)colIdx];
                                else
                                    propData.PropValue = new XUICOLOR();
                                propertyData.Add(propData);
                                break;

                            case XUIELEM_PROP_TYPE.XUI_EPT_CUSTOM:
                                uint cusOffset = ReadPackedUlong();
                                foreach (XUICUSTOM cust in BinaryTables.CustomTable)
                                {
                                    if (cusOffset == cust.Offset)
                                    {
                                        propData.PropValue = cust;
                                        propertyData.Add(propData);
                                        break;
                                    }
                                }
                                break;

                            case XUIELEM_PROP_TYPE.XUI_EPT_OBJECT:
                                // Object properties are compound properties (Fill, Stroke, Gradient)
                                uint compoundPropId = ReadPackedUlong();

                                // Check if we've already loaded this compound property
                                bool foundMatch = false;
                                foreach (XUICOMPOUNDPROPDATA compoundPropData in LoadContext.CompoundPropArray)
                                {
                                    if (compoundPropData.Index == compoundPropId)
                                    {
                                        foundMatch = true;
                                        List<XUIPROPERTYDATA> compoundprop = new List<XUIPROPERTYDATA>();
                                        foreach (XUIPROPERTYDATA pData in compoundPropData.PropertyArray)
                                        {
                                            compoundprop.Add(pData);
                                        }
                                        propData.PropValue = compoundprop;
                                        propertyData.Add(propData);
                                        break;
                                    }
                                }

                                if (!foundMatch)
                                {
                                    // New compound property - need to load it
                                    List<XUIELEM_PROP_DEF> subPropArray = new List<XUIELEM_PROP_DEF>();
                                    switch (propData.PropDef.PropName)
                                    {
                                        case "Fill":
                                            subPropArray = XuiClass.Instance.GetFillPropArray();
                                            break;
                                        case "Gradient":
                                            subPropArray = XuiClass.Instance.GetGradientPropArray();
                                            break;
                                        case "Stroke":
                                            subPropArray = XuiClass.Instance.GetStrokePropArray();
                                            break;
                                    }

                                    uint numProps = ReadPackedUlong();
                                    List<XUIPROPERTYDATA> newcompoundprop = new List<XUIPROPERTYDATA>();
                                    LoadPropertyDataFromBinary(subPropArray, ref newcompoundprop);
                                    propData.PropValue = newcompoundprop;
                                    propertyData.Add(propData);

                                    // Add to compound prop array for reuse
                                    XUICOMPOUNDPROPDATA cpd = new XUICOMPOUNDPROPDATA();
                                    cpd.Index = compoundPropId;
                                    cpd.PropertyArray = newcompoundprop;
                                    LoadContext.CompoundPropArray.Add(cpd);
                                }
                                break;

                            default:
                                // Unknown type - skip
                                break;
                        }
                    }
                }
                propIndex++;
            }
        }

        /// <summary>
        /// Load timeline data for an object (named frames and subtimelines)
        /// Simplified version - just skip timeline data for now
        /// </summary>
        private void LoadTimelineDataFromBinary(string className, ref XUIOBJECTDATA objectData)
        {
            // Read number of named frames
            uint numNamedFrames = ReadPackedUlong();
            if (numNamedFrames > 0)
            {
                uint namedFrameIdx = ReadPackedUlong();
                objectData.NumNamedFrames = numNamedFrames;
                for (uint nfIdx = 0; nfIdx < numNamedFrames; nfIdx++)
                {
                    if ((int)(namedFrameIdx + nfIdx) < AnimationTables.NamedFrameTable.Count)
                        objectData.NamedFrameArray.Add(AnimationTables.NamedFrameTable[(int)(namedFrameIdx + nfIdx)]);
                }
            }

            // Read number of subtimelines
            uint numSubtimelines = ReadPackedUlong();
            objectData.NumSubTimelines = numSubtimelines;

            if (objectData.ChildrenArray.Count == 0)
            {
                for (uint i = 0; i < numSubtimelines; i++)
                    SkipSubTimelineData();
                return;
            }

            objectData.SubTimelines.Clear();
            if (objectData.NumSubTimelines == 0)
                return;

            for (uint subIdx = 0; subIdx < numSubtimelines; subIdx++)
            {
                XUISUBTIMELINEDATA timelineData = new XUISUBTIMELINEDATA();

                uint idStringIdx = ReadPackedUlong();
                timelineData.ElementStringIdx = idStringIdx;
                if ((int)idStringIdx < BinaryTables.StringTable.Count)
                    timelineData.ElementId = BinaryTables.StringTable[(int)idStringIdx];

                uint numPropPaths = ReadPackedUlong();
                timelineData.NumPropPaths = numPropPaths;
                for (uint ppIdx = 0; ppIdx < numPropPaths; ppIdx++)
                {
                    XUITIMELINEPROPPATH proppathData = new XUITIMELINEPROPPATH();

                    uint indexVal = 0;
                    uint ret = LoadSubTimelinePropertiesFromBinary(timelineData.ElementId, ref proppathData, ref objectData, ref indexVal);

                    timelineData.IndexArray.Add(indexVal);
                    proppathData.Index = indexVal;
                    timelineData.PropPathArray.Add(proppathData);
                    if (ret != 0)
                        continue;
                }

                uint numKeyframes = ReadPackedUlong();
                uint keyframeIdx = ReadPackedUlong();

                for (uint kfIdx = 0; kfIdx < numKeyframes; kfIdx++)
                {
                    if ((int)(keyframeIdx + kfIdx) >= AnimationTables.KeyframeDataTable.Count)
                        continue;

                    XUIKEYFRAMEDATA keyframeData = AnimationTables.KeyframeDataTable[(int)(keyframeIdx + kfIdx)];

                    for (uint propIdx = 0; propIdx < numPropPaths; propIdx++)
                    {
                        if ((int)(keyframeData.KeyframePropIdx + propIdx) >= AnimationTables.KeyframePropertyTable.Count)
                            continue;

                        XUIKEYFRAMEPROPDATA keyframeProp = AnimationTables.KeyframePropertyTable[(int)(keyframeData.KeyframePropIdx + propIdx)];
                        XUITIMELINEPROPPATH propPath = timelineData.PropPathArray[(int)propIdx];
                        if (propPath.PropDefArray.Count == 0)
                            continue;

                        XUIPROPERTYDATA propData = new XUIPROPERTYDATA();
                        propData.PropDef = propPath.PropDefArray[propPath.PropDefArray.Count - 1];
                        propData.PropType = propData.PropDef.Type;
                        propData.Flags = propData.PropDef.Flags;
                        propData.Index = propPath.Index;

                        if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_INTEGER || propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_UNSIGNED)
                        {
                            propData.PropValue = keyframeProp.PropertyIndex;
                        }
                        else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_BOOL)
                        {
                            uint val = keyframeProp.PropertyIndex;
                            propData.PropValue = val == 0 ? "false" : "true";
                        }
                        else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_STRING)
                        {
                            if ((int)keyframeProp.PropertyIndex < BinaryTables.StringTable.Count)
                                propData.PropValue = BinaryTables.StringTable[(int)keyframeProp.PropertyIndex];
                        }
                        else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_FLOAT)
                        {
                            if ((int)keyframeProp.PropertyIndex < BinaryTables.FloatTable.Count)
                                propData.PropValue = BinaryTables.FloatTable[(int)keyframeProp.PropertyIndex];
                        }
                        else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_COLOR)
                        {
                            if ((int)keyframeProp.PropertyIndex < BinaryTables.ColorTable.Count)
                                propData.PropValue = BinaryTables.ColorTable[(int)keyframeProp.PropertyIndex];
                        }
                        else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_VECTOR)
                        {
                            if ((int)keyframeProp.PropertyIndex < BinaryTables.VectorTable.Count)
                                propData.PropValue = BinaryTables.VectorTable[(int)keyframeProp.PropertyIndex];
                        }
                        else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_QUATERNION)
                        {
                            if ((int)keyframeProp.PropertyIndex < BinaryTables.QuaternionTable.Count)
                                propData.PropValue = BinaryTables.QuaternionTable[(int)keyframeProp.PropertyIndex];
                        }

                        keyframeData.PropList.Add(propData);
                    }

                    timelineData.KeyframeDataArray.Add(keyframeData);
                }

                objectData.SubTimelines.Add(timelineData);
            }
        }

        /// <summary>
        /// Skip timeline data (simplified - just consume bytes to keep stream in sync)
        /// </summary>
        private void SkipSubTimelineData()
        {
            try
            {
                ReadPackedUlong(); // Element string index
                uint numPropPaths = ReadPackedUlong();

                for (uint i = 0; i < numPropPaths; i++)
                {
                    byte bitFlag = ReadByte();
                    byte pathDepth = (byte)(bitFlag & 0x7F);
                    byte loadIndex = (byte)(bitFlag & 0x80);

                    ReadByte(); // prop depth
                    ReadByte(); // prop index

                    if (pathDepth > 1)
                    {
                        for (uint j = 1; j < pathDepth; j++)
                            ReadByte();
                    }

                    if (loadIndex > 0)
                        ReadPackedUlong();
                }

                ReadPackedUlong(); // num keyframes
                ReadPackedUlong(); // keyframe table index
            }
            catch
            {
                // If timeline skipping fails, just continue
            }
        }

        private uint LoadSubTimelinePropertiesFromBinary(string szId, ref XUITIMELINEPROPPATH proppathData, ref XUIOBJECTDATA objectData, ref uint indexVal)
        {
            byte bitPackedVar = ReadByte();
            proppathData.Flags |= 0x1;
            uint pathDepth = (uint)(bitPackedVar & 0x7F);
            uint upperBit = (uint)(bitPackedVar & 0x80);

            proppathData.Depth = pathDepth;

            List<XUIOBJECTDATA> childArray = objectData.ChildrenArray;
            int numChildren = childArray.Count;
            if (numChildren <= 0) return 0x8070000D;

            bool foundElement = false;
            string className = "";
            foreach (XUIOBJECTDATA childObj in childArray)
            {
                List<XUIPROPERTYDATA> propArray = childObj.PropertyArray;
                foreach (XUIPROPERTYDATA childProp in propArray)
                {
                    XUIELEM_PROP_DEF propDef = childProp.PropDef;
                    if (propDef.Type == XUIELEM_PROP_TYPE.XUI_EPT_STRING && propDef.PropName == "Id")
                    {
                        if (childProp.PropValue.ToString() == szId)
                        {
                            foundElement = true;
                            className = childObj.ClassName;
                            break;
                        }
                    }
                }
                if (foundElement == true) break;
            }

            if (foundElement == false) return 0x8070000E;

            XUI_CLASS pClass = XuiClass.Instance.FindClass(className);
            if (pClass.szClassName == null) return 0x80004005;

            XUIELEM_PROP_DEF propDeff = new XUIELEM_PROP_DEF();
            if (pathDepth != 0)
            {
                byte depth = ReadByte();
                byte index = ReadByte();

                List<List<XUIELEM_PROP_DEF>> classList = XuiClass.Instance.GetHierarchy(className);
                if (classList.Count == 0) return 0x80070057;
                classList.Reverse();

                List<XUIELEM_PROP_DEF> propList = classList[(int)depth];
                propDeff = propList[index];
                proppathData.PropDefArray.Add(propDeff);
            }

            if (pathDepth > 1)
            {
                for (uint cpd = 0; cpd < (pathDepth - 1); cpd++)
                {
                    List<XUIELEM_PROP_DEF> propList = propDeff.GetPropDefs();
                    uint propIdx = ReadByte();
                    propDeff = propList[(int)propIdx];
                    proppathData.PropDefArray.Add(propDeff);
                }
            }

            if (upperBit > 0)
            {
                uint idxVal = ReadPackedUlong();
                indexVal = idxVal;
            }

            return 0;
        }

        private void ReadNamedFrameTable(uint size)
        {
            AnimationTables.NamedFrameTable.Clear();

            uint bytesRemaining = size;
            while (bytesRemaining > 0)
            {
                long curPos = reader.BaseStream.Position;

                XUINAMEDFRAMEDATA namedFrame = new XUINAMEDFRAMEDATA();

                namedFrame.NameStringIndex = ReadPackedUlong();
                namedFrame.Name = BinaryTables.StringTable[(int)namedFrame.NameStringIndex];
                namedFrame.Time = ReadPackedUlong();
                namedFrame.Command = (XUI_NAMEDFRAME_COMMAND)ReadByte();
                if (namedFrame.Command >= XUI_NAMEDFRAME_COMMAND.XUI_CMD_GOTO)
                {
                    namedFrame.TargetStringIndex = ReadPackedUlong();
                    namedFrame.Target = BinaryTables.StringTable[(int)namedFrame.TargetStringIndex];
                }

                long bytesRead = reader.BaseStream.Position - curPos;
                bytesRemaining -= (uint)bytesRead;

                AnimationTables.NamedFrameTable.Add(namedFrame);
            }
        }

        private void ReadKeyframePropertyTable(uint size)
        {
            AnimationTables.KeyframePropertyTable.Clear();

            uint bytesRemaining = size;
            while (bytesRemaining > 0)
            {
                long curPos = reader.BaseStream.Position;

                XUIKEYFRAMEPROPDATA keyframeProp = new XUIKEYFRAMEPROPDATA();
                keyframeProp.PropertyIndex = ReadPackedUlong();

                long bytesRead = reader.BaseStream.Position - curPos;
                bytesRemaining -= (uint)bytesRead;

                AnimationTables.KeyframePropertyTable.Add(keyframeProp);
            }
        }

        private void ReadKeyframeDataTable(uint size)
        {
            AnimationTables.KeyframeDataTable.Clear();

            uint bytesRemaining = size;
            while (bytesRemaining > 0)
            {
                long curPos = reader.BaseStream.Position;

                XUIKEYFRAMEDATA keyframeData = new XUIKEYFRAMEDATA();

                keyframeData.Frame = ReadPackedUlong();
                byte flagData = ReadByte();
                keyframeData.Flags = (byte)(flagData & 0x3F);
                keyframeData.Unknown1 = (byte)(flagData >> 6);

                if (keyframeData.Flags == 0x0)
                {
                    keyframeData.EaseIn = 0x00;
                    keyframeData.EaseOut = 0x00;
                    keyframeData.EaseScale = 0x00;
                    keyframeData.InterpolateType = XUI_INTERPOLATE.XUI_INTERPOLATE_LINEAR;
                }
                else if (keyframeData.Flags == 0x1)
                {
                    keyframeData.EaseIn = 0x00;
                    keyframeData.EaseOut = 0x00;
                    keyframeData.EaseScale = 0x00;
                    keyframeData.InterpolateType = XUI_INTERPOLATE.XUI_INTERPOLATE_NONE;
                }
                else if (keyframeData.Flags == 0x2)
                {
                    keyframeData.EaseIn = ReadByte();
                    keyframeData.EaseOut = ReadByte();
                    keyframeData.EaseScale = ReadByte();
                    keyframeData.InterpolateType = XUI_INTERPOLATE.XUI_INTERPOLATE_EASE;
                }
                else if (keyframeData.Flags == 0x3)
                {
                    keyframeData.EaseIn = 0x00;
                    keyframeData.EaseOut = 0x00;
                    keyframeData.EaseScale = 0x00;
                    keyframeData.InterpolateType = XUI_INTERPOLATE.XUI_INTERPOLATE_LINEAR;
                }
                else if (keyframeData.Flags == 0xA)
                {
                    keyframeData.EaseIn = 0x00;
                    keyframeData.EaseOut = 0x00;
                    keyframeData.EaseScale = 0x00;
                    keyframeData.InterpolateType = XUI_INTERPOLATE.XUI_INTERPOLATE_LINEAR;
                    uint vectorIdx = ReadPackedUlong();
                    keyframeData.VectorIdx = vectorIdx;
                    if ((int)keyframeData.VectorIdx < BinaryTables.VectorTable.Count)
                        keyframeData.VectorRef = BinaryTables.VectorTable[(int)keyframeData.VectorIdx];
                }
                else
                {
                    if (!Log.Instance.flagErrors.Contains(keyframeData.Flags))
                        Log.Instance.flagErrors.Add(keyframeData.Flags);
                }

                uint propIndex = ReadPackedUlong();
                keyframeData.KeyframePropIdx = propIndex;

                long bytesRead = reader.BaseStream.Position - curPos;
                bytesRemaining -= (uint)bytesRead;

                AnimationTables.KeyframeDataTable.Add(keyframeData);
            }
        }

    }
}
