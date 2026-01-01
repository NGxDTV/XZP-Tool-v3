using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace XZPToolv3.XUI
{
    class XurWriter : BEBinaryWriter
    {
        public uint[] SectionNames = { 0x5354524E, 0x56454354, 0x51554154, 0x43555354, 0x464C4F54, 0x434F4C52, 0x4B455950, 0x4B455944, 0x4E414D45, 0x44415441 };
        public string[] SectionDispalyName = { "STRN", "VECT", "QUAT", "CUST", "FLOT", "COLR", "KEYP", "KEYD", "NAME", "DATA" };

        public XurWriter(Stream stream) : base(stream) { }

        private class XUIBinaryTables
        {
            public LookupTable<string> StringTable;
            public LookupTable<XUIVECTOR> VectorTable;
            public LookupTable<XUIQUATERNION> QuaternionTable;
            public LookupTable<float> FloatTable;
            public LookupTable<XUICOLOR> ColorTable;
            public LookupTable<XUICUSTOM> CustomTable;

            public XUIBinaryTables()
            {
                StringTable = new LookupTable<string>();
                VectorTable = new LookupTable<XUIVECTOR>();
                QuaternionTable = new LookupTable<XUIQUATERNION>();
                FloatTable = new LookupTable<float>();
                ColorTable = new LookupTable<XUICOLOR>();
                CustomTable = new LookupTable<XUICUSTOM>();
            }
        }
        private class XUIAnimationTables
        {
            public LookupTable<XUINAMEDFRAMEDATA> NamedframeTable;
            public LookupTable<XUIKEYFRAMEDATA> KeyframeDataTable;
            public LookupTable<XUIKEYFRAMEPROPDATA> KeyframePropTable;

            public XUIAnimationTables()
            {
                NamedframeTable = new LookupTable<XUINAMEDFRAMEDATA>();
                KeyframeDataTable = new LookupTable<XUIKEYFRAMEDATA>();
                KeyframePropTable = new LookupTable<XUIKEYFRAMEPROPDATA>();
            }
        }
        private struct XUILoadContext
        {
            public uint ObjectCount;
            public uint PropertyCount;
            public uint PropertyArrayCount;
            public uint CompoundObjectPropCount;
            public uint CompoundObjectPropArrayCount;
            public uint PropPathDepthCount;
            public uint TimelinePropPathCount;
            public uint SubTimelineCount;
            public uint KeyframePropCount;
            public uint KeyframeDataCount;
            public uint NamedFrameCount;
            public uint ObjectsWithChildrenCount;
        }

        XUIBinaryTables BinaryTables = new XUIBinaryTables();
        XUIAnimationTables AnimationTables = new XUIAnimationTables();
        XUILoadContext LoadContext = new XUILoadContext();

        public void WriteObjectsToBinary(XUIOBJECTDATA rootObject)
        {
            BinaryTables.StringTable.Add("");

            ByteArray[] tableData = new ByteArray[10];

            ByteArray objectByteArray = new ByteArray();
            WriteObjectToBinary(rootObject, ref objectByteArray);

            LoadContext.NamedFrameCount = (uint)AnimationTables.NamedframeTable.Count;
            LoadContext.KeyframeDataCount = (uint)AnimationTables.KeyframeDataTable.Count;
            LoadContext.KeyframePropCount = (uint)AnimationTables.KeyframePropTable.Count;

            if (objectByteArray.Count > 0)
                tableData[9] = objectByteArray;
            if (AnimationTables.NamedframeTable.Count > 0)
                tableData[8] = WriteNamedFramesTable(AnimationTables.NamedframeTable);
            if (AnimationTables.KeyframeDataTable.Count > 0)
                tableData[7] = WriteKeyframeDataTable(AnimationTables.KeyframeDataTable);
            if (AnimationTables.KeyframePropTable.Count > 0)
                tableData[6] = WriteKeyframePropTable(AnimationTables.KeyframePropTable);
            if (BinaryTables.ColorTable.Count > 0)
                tableData[5] = WriteColorTable(BinaryTables.ColorTable);
            if (BinaryTables.FloatTable.Count > 0)
                tableData[4] = WriteFloatTable(BinaryTables.FloatTable);
            if (BinaryTables.CustomTable.Count > 0)
                tableData[3] = WriteCustomTable(BinaryTables.CustomTable);
            if (BinaryTables.QuaternionTable.Count > 0)
                tableData[2] = WriteQuaternionTable(BinaryTables.QuaternionTable);
            if (BinaryTables.VectorTable.Count > 0)
                tableData[1] = WriteVectorTable(BinaryTables.VectorTable);
            if (BinaryTables.StringTable.Count > 0)
                tableData[0] = WriteStringTable(BinaryTables.StringTable);

            XUR8_HEADER_INFO headerInfo = new XUR8_HEADER_INFO();
            headerInfo.ObjectCount = LoadContext.ObjectCount;
            headerInfo.PropertyCount = LoadContext.PropertyCount;
            headerInfo.PropertyArrayCount = LoadContext.PropertyArrayCount;
            headerInfo.CompoundObjectPropCount = LoadContext.CompoundObjectPropCount;
            headerInfo.CompoundObjectPropArrayCount = LoadContext.CompoundObjectPropArrayCount;
            headerInfo.PropPathDepthCount = LoadContext.PropPathDepthCount;
            headerInfo.TimelinePropPathCount = LoadContext.TimelinePropPathCount;
            headerInfo.SubTimelineCount = LoadContext.SubTimelineCount;
            headerInfo.KeyframePropCount = LoadContext.KeyframePropCount;
            headerInfo.KeyframeDataCount = LoadContext.KeyframeDataCount;
            headerInfo.NamedFrameCount = LoadContext.NamedFrameCount;
            headerInfo.ObjectsWithChildrenCount = LoadContext.ObjectsWithChildrenCount;
            ByteArray headerInfoData = WriteHeaderInfo(headerInfo);

            XUR8_SECTION[] sectionData = new XUR8_SECTION[10];
            ushort numSections = 0;
            for (int sectionIdx = 0; sectionIdx < 10; sectionIdx++)
            {
                if (tableData[sectionIdx] != null)
                {
                    sectionData[sectionIdx].Name = SectionNames[sectionIdx];
                    sectionData[sectionIdx].Offset = 0;
                    sectionData[sectionIdx].Size = (uint)tableData[sectionIdx].Count;
                    numSections++;
                }
            }

            ByteArray sectionHeaderData = new ByteArray();
            uint fileSize = (uint)(0x14 + headerInfoData.Count + numSections * 0xC);
            for (int sectionIdx = 0; sectionIdx < 10; sectionIdx++)
            {
                if (tableData[sectionIdx] != null)
                {
                    sectionData[sectionIdx].Offset = fileSize;
                    fileSize += sectionData[sectionIdx].Size;
                    sectionHeaderData.AddByteARRAY(WriteSectionHeader(sectionData[sectionIdx]));
                }
            }

            XUR8_HEADER header = new XUR8_HEADER();
            header.Magic = 0x58554942;
            header.Version = 0x8;
            header.Flags = 0x0;
            header.XuiVersion = 0xE;
            header.BinSize = fileSize;
            header.NumSections = numSections;
            ByteArray headerData = WriteHeader(header);

            headerData.WriteToFile(BaseStream);
            headerInfoData.WriteToFile(BaseStream);
            sectionHeaderData.WriteToFile(BaseStream);
            foreach (ByteArray array in tableData)
                if (array != null) array.WriteToFile(BaseStream);

            Close();
        }

        public void WriteObjectToBinary(XUIOBJECTDATA objectData, ref ByteArray byteArray)
        {
            LoadContext.ObjectCount++;

            int classNameIndex = BinaryTables.StringTable.GetIndex(objectData.ClassName);
            Debug.Assert(classNameIndex > -1, "Error obtaining index");
            byteArray.AddPackedDWORD((uint)classNameIndex);

            byte objectFlags = 0x0;
            if (objectData.PropertyArray.Count > 0) objectFlags |= 0x1;
            if (objectData.ChildrenArray.Count > 0) objectFlags |= 0x2;
            if (objectData.SubTimelines.Count > 0) objectFlags |= 0x4;
            if (objectData.NamedFrameArray.Count > 0) objectFlags |= 0x4;
            byteArray.AddBYTE(objectFlags);

            if (objectData.PropertyArray.Count > 0)
            {
                uint propCount = 0;
                foreach (XUIPROPERTYDATA propData in objectData.PropertyArray)
                {
                    if ((propData.Flags & 0x100) == 0x100)
                        continue;
                    propCount++;
                    LoadContext.PropertyCount++;
                }

                byteArray.AddPackedDWORD((uint)propCount);
                LoadContext.PropertyArrayCount++;

                List<List<XUIELEM_PROP_DEF>> classList = XuiClass.Instance.GetHierarchy(objectData.ClassName);
                foreach (List<XUIELEM_PROP_DEF> propDefList in classList)
                    WriteObjectPropsToBinary(objectData.PropertyArray, propDefList, ref byteArray);
            }

            if (objectData.ChildrenArray.Count > 0)
            {
                byteArray.AddPackedDWORD((uint)objectData.ChildrenArray.Count);
                LoadContext.ObjectsWithChildrenCount++;

                foreach (XUIOBJECTDATA objectChild in objectData.ChildrenArray)
                    WriteObjectToBinary(objectChild, ref byteArray);
            }

            if (objectData.SubTimelines.Count > 0 || objectData.NamedFrameArray.Count > 0)
            {
                WriteTimelineDataToBinary(objectData, ref byteArray);
            }
        }

        private void WriteObjectPropsToBinary(List<XUIPROPERTYDATA> propList, List<XUIELEM_PROP_DEF> propDefList, ref ByteArray byteArray)
        {
            List<XUIPROPERTYDATA> subPropList = new List<XUIPROPERTYDATA>();
            uint nPropertyMask = 0;
            byte nFlagIdx = 0;
            foreach (XUIELEM_PROP_DEF propDef in propDefList)
            {
                uint nFlag = (uint)1 << nFlagIdx;

                foreach (XUIPROPERTYDATA propData in propList)
                {
                    if ((propData.Flags & 0x100) == 0x100) continue;

                    if (propData.PropDef.PropName == propDef.PropName)
                    {
                        subPropList.Add(propData);

                        if (propData.Index == 0)
                        {
                            nPropertyMask += nFlag;
                        }
                    }
                }

                nFlagIdx++;
            }

            byteArray.AddPackedDWORD(nPropertyMask);

            if (nPropertyMask > 0)
                WritePropertiesToBinary(subPropList, propDefList, ref byteArray);
        }

        private void WritePropertiesToBinary(List<XUIPROPERTYDATA> propDataList, List<XUIELEM_PROP_DEF> propDefList, ref ByteArray byteArray)
        {
            foreach (XUIPROPERTYDATA propData in propDataList)
            {
                if (propData.Index == 0 && (uint)(propData.Flags & 0x1) == 0x1)
                {
                    uint numIndices = 0;
                    foreach (XUIPROPERTYDATA idxProp in propDataList)
                    {
                        if (idxProp.PropDef.PropName == propData.PropDef.PropName)
                            numIndices++;
                    }

                    byteArray.AddPackedDWORD(numIndices);
                }

                if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_STRING)
                {
                    int index = BinaryTables.StringTable.GetIndex(propData.PropValue.ToString());
                    byteArray.AddPackedDWORD((uint)index);
                }
                else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_COLOR)
                {
                    int index = BinaryTables.ColorTable.GetIndex(propData.PropValue.ToString().ToXuiColor());
                    byteArray.AddPackedDWORD((uint)index);
                }
                else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_FLOAT)
                {
                    int index = BinaryTables.FloatTable.GetIndex(Single.Parse(propData.PropValue.ToString(), System.Globalization.CultureInfo.InvariantCulture));
                    byteArray.AddPackedDWORD((uint)index);
                }
                else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_INTEGER)
                {
                    byteArray.AddPackedDWORD((uint)Int32.Parse(propData.PropValue.ToString()));
                }
                else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_UNSIGNED)
                {
                    byteArray.AddPackedDWORD(UInt32.Parse(propData.PropValue.ToString()));
                }
                else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_BOOL)
                {
                    byteArray.AddBOOL(Convert.ToBoolean(propData.PropValue.ToString()));
                }
                else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_VECTOR)
                {
                    int index = BinaryTables.VectorTable.GetIndex(propData.PropValue.ToString().ToXuiVector());
                    byteArray.AddPackedDWORD((uint)index);
                }
                else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_QUATERNION)
                {
                    int index = BinaryTables.QuaternionTable.GetIndex(propData.PropValue.ToString().ToXuiQuaternion());
                    byteArray.AddPackedDWORD((uint)index);
                }
                else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_CUSTOM)
                {
                    int index = BinaryTables.CustomTable.GetIndex(propData.PropValue.ToString().ToXuiCustom());
                    uint offset = 0;
                    for (int x = 0; x < index; x++)
                    {
                        XUICUSTOM custom = (XUICUSTOM)BinaryTables.CustomTable[x];
                        ByteArray custArray = custom.ToBinary();
                        offset += (uint)custArray.Count;
                    }
                    byteArray.AddPackedDWORD((uint)offset);
                }
                else if (propData.PropType == XUIELEM_PROP_TYPE.XUI_EPT_OBJECT)
                {
                    byteArray.AddPackedDWORD((uint)LoadContext.CompoundObjectPropArrayCount);

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

                    List<XUIPROPERTYDATA> compoundProps = (List<XUIPROPERTYDATA>)propData.PropValue;

                    uint propCount = 0;
                    foreach (XUIPROPERTYDATA compoundPropData in compoundProps)
                    {
                        if ((compoundPropData.Flags & 0x100) == 0x100) continue;
                        propCount++;
                        LoadContext.CompoundObjectPropCount++;
                    }

                    byteArray.AddPackedDWORD((uint)propCount);
                    LoadContext.CompoundObjectPropArrayCount++;

                    WriteObjectPropsToBinary(compoundProps, subPropArray, ref byteArray);
                }
                else
                {
                    Debug.Assert(false, "Unknown Property Type");
                }
            }
        }

        private void WriteTimelineDataToBinary(XUIOBJECTDATA objectData, ref ByteArray byteArray)
        {
            uint numFrames = objectData.NumNamedFrames;
            byteArray.AddPackedDWORD(numFrames);

            if (numFrames > 0)
            {
                int index = AnimationTables.NamedframeTable.GetIndex(objectData.NamedFrameArray);
                Debug.Assert(index >= 0, "Invalid Named Frame Index");
                byteArray.AddPackedDWORD((uint)index);
            }

            uint numSubtimelines = objectData.NumSubTimelines;
            byteArray.AddPackedDWORD(numSubtimelines);

            LoadContext.SubTimelineCount += (uint)objectData.SubTimelines.Count;

            foreach (XUISUBTIMELINEDATA subData in objectData.SubTimelines)
            {
                int index = BinaryTables.StringTable.GetIndex(subData.ElementId);
                byteArray.AddPackedDWORD((uint)index);

                byteArray.AddPackedDWORD((uint)subData.NumPropPaths);

                foreach (XUITIMELINEPROPPATH propPath in subData.PropPathArray)
                {
                    WriteSubtimelineDataToBinary(subData.ElementId, propPath, objectData, ref byteArray);

                    LoadContext.TimelinePropPathCount++;
                }

                byteArray.AddPackedDWORD((uint)subData.KeyframeDataArray.Count);

                List<XUIKEYFRAMEDATA> kfArray = new List<XUIKEYFRAMEDATA>();
                foreach (XUIKEYFRAMEDATA kfData in subData.KeyframeDataArray)
                {
                    XUIKEYFRAMEDATA newKfData = new XUIKEYFRAMEDATA();
                    newKfData = kfData;

                    List<XUIKEYFRAMEPROPDATA> kpArray = new List<XUIKEYFRAMEPROPDATA>();
                    int propIdx = 0;
                    foreach (XUITIMELINEPROPPATH tlPath in subData.PropPathArray)
                    {
                        XUIELEM_PROP_DEF propDef = tlPath.PropDefArray[(int)tlPath.PropDefArray.Count - 1];

                        XUIKEYFRAMEPROPDATA propData = new XUIKEYFRAMEPROPDATA();

                        if (propDef.Type == XUIELEM_PROP_TYPE.XUI_EPT_BOOL)
                        {
                            bool val = Convert.ToBoolean(kfData.PropList[propIdx].PropValue);
                            propData.PropertyIndex = (uint)(val == true ? 1 : 0);
                        }
                        else if (propDef.Type == XUIELEM_PROP_TYPE.XUI_EPT_INTEGER)
                        {
                            int val = (int)kfData.PropList[propIdx].PropValue;
                            propData.PropertyIndex = (uint)val;
                        }
                        else if (propDef.Type == XUIELEM_PROP_TYPE.XUI_EPT_UNSIGNED)
                        {
                            uint val = (uint)kfData.PropList[propIdx].PropValue;
                            propData.PropertyIndex = (uint)val;
                        }
                        else if (propDef.Type == XUIELEM_PROP_TYPE.XUI_EPT_STRING)
                        {
                            string val = (string)kfData.PropList[propIdx].PropValue;
                            int idx = BinaryTables.StringTable.GetIndex(val);
                            propData.PropertyIndex = (uint)idx;
                        }
                        else if (propDef.Type == XUIELEM_PROP_TYPE.XUI_EPT_FLOAT)
                        {
                            float val = (float)kfData.PropList[propIdx].PropValue;
                            int idx = BinaryTables.FloatTable.GetIndex(val);
                            propData.PropertyIndex = (uint)idx;
                        }
                        else if (propDef.Type == XUIELEM_PROP_TYPE.XUI_EPT_VECTOR)
                        {
                            XUIVECTOR val = (XUIVECTOR)kfData.PropList[propIdx].PropValue;
                            int idx = BinaryTables.VectorTable.GetIndex(val);
                            propData.PropertyIndex = (uint)idx;
                        }
                        else if (propDef.Type == XUIELEM_PROP_TYPE.XUI_EPT_QUATERNION)
                        {
                            XUIQUATERNION val = (XUIQUATERNION)kfData.PropList[propIdx].PropValue;
                            int idx = BinaryTables.QuaternionTable.GetIndex(val);
                            propData.PropertyIndex = (uint)idx;
                        }
                        else if (propDef.Type == XUIELEM_PROP_TYPE.XUI_EPT_COLOR)
                        {
                            XUICOLOR val = (XUICOLOR)kfData.PropList[propIdx].PropValue;
                            int idx = BinaryTables.ColorTable.GetIndex(val);
                            propData.PropertyIndex = (uint)idx;
                        }

                        kpArray.Add(propData);
                        propIdx++;
                    }

                    int pIdx = AnimationTables.KeyframePropTable.GetIndex(kpArray);

                    newKfData.KeyframePropIdx = (uint)pIdx;
                    kfArray.Add(newKfData);
                }

                int kfIndex = AnimationTables.KeyframeDataTable.GetIndex(kfArray);
                byteArray.AddPackedDWORD((uint)kfIndex);
            }
        }

        private void WriteSubtimelineDataToBinary(string elementId, XUITIMELINEPROPPATH propPath, XUIOBJECTDATA objectData, ref ByteArray byteArray)
        {
            byte pathDepth = (byte)(propPath.Depth);
            byte loadIndex = (byte)((propPath.PropDefArray[(int)propPath.PropDefArray.Count - 1].Flags & 0x1) << 7);
            byte bitFlag = (byte)((loadIndex | pathDepth));

            byteArray.AddBYTE(bitFlag);

            LoadContext.PropPathDepthCount += pathDepth;

            string className = "";
            bool foundChild = false;
            foreach (XUIOBJECTDATA objChild in objectData.ChildrenArray)
            {
                string childId = (string)objChild.GetPropVal("Id");
                if (childId == "") continue;

                if (childId == elementId)
                {
                    className = objChild.ClassName;
                    foundChild = true;
                    break;
                }
            }

            Debug.Assert(foundChild == true, "Child containing subtimeline not found");

            XUI_CLASS pClass = XuiClass.Instance.FindClass(className);
            Debug.Assert(pClass.szClassName != null, "Class Properties could not be found");

            List<List<XUIELEM_PROP_DEF>> classList = XuiClass.Instance.GetHierarchy(className);
            classList.Reverse();

            bool foundProp = false;
            uint propDepth = 0;
            uint propIndex = 0;

            XUIELEM_PROP_DEF targetDef = propPath.PropDefArray[0];

            foreach (List<XUIELEM_PROP_DEF> propList in classList)
            {
                foreach (XUIELEM_PROP_DEF propDef in propList)
                {
                    if (propDef.PropName == targetDef.PropName)
                    {
                        foundProp = true;
                        break;
                    }

                    propIndex++;
                }
                if (foundProp == true) break;

                propIndex = 0;
                propDepth++;
            }

            byteArray.AddBYTE((byte)(propDepth));
            byteArray.AddBYTE((byte)(propIndex));

            if (pathDepth > 1)
            {
                for (uint cpd = 1; cpd < pathDepth; cpd++)
                {
                    Debug.Assert(targetDef.Type == XUIELEM_PROP_TYPE.XUI_EPT_OBJECT, "Invalid Prop Type");

                    List<XUIELEM_PROP_DEF> propList = targetDef.GetPropDefs();

                    XUIELEM_PROP_DEF midPropDef = propPath.PropDefArray[(int)cpd];
                    int indx = 0;
                    foreach (XUIELEM_PROP_DEF prop in propList)
                    {
                        if (prop.PropName == midPropDef.PropName)
                        {
                            break;
                        }
                        indx++;
                    }

                    byteArray.AddBYTE((byte)indx);

                    targetDef = midPropDef;
                }
            }

            if (loadIndex > 0) byteArray.AddPackedDWORD(propPath.Index);
        }

        private ByteArray WriteHeader(XUR8_HEADER header)
        {
            ByteArray byteArray = new ByteArray();

            byteArray.AddDWORD(header.Magic);
            byteArray.AddDWORD(header.Version);
            byteArray.AddDWORD(header.Flags);
            byteArray.AddWORD(header.XuiVersion);
            byteArray.AddDWORD(header.BinSize);
            byteArray.AddWORD(header.NumSections);

            return byteArray;
        }

        private ByteArray WriteHeaderInfo(XUR8_HEADER_INFO headerInfo)
        {
            ByteArray byteArray = new ByteArray();

            byteArray.AddPackedDWORD((uint)headerInfo.ObjectCount);
            byteArray.AddPackedDWORD((uint)headerInfo.PropertyCount);
            byteArray.AddPackedDWORD((uint)headerInfo.PropertyArrayCount);
            byteArray.AddPackedDWORD((uint)headerInfo.CompoundObjectPropCount);
            byteArray.AddPackedDWORD((uint)headerInfo.CompoundObjectPropArrayCount);
            byteArray.AddPackedDWORD((uint)headerInfo.PropPathDepthCount);
            byteArray.AddPackedDWORD((uint)headerInfo.TimelinePropPathCount);
            byteArray.AddPackedDWORD((uint)headerInfo.SubTimelineCount);
            byteArray.AddPackedDWORD((uint)headerInfo.KeyframePropCount);
            byteArray.AddPackedDWORD((uint)headerInfo.KeyframeDataCount);
            byteArray.AddPackedDWORD((uint)headerInfo.NamedFrameCount);
            byteArray.AddPackedDWORD((uint)headerInfo.ObjectsWithChildrenCount);

            return byteArray;
        }

        private ByteArray WriteSectionHeader(XUR8_SECTION sectionInfo)
        {
            ByteArray byteArray = new ByteArray();

            byteArray.AddDWORD(sectionInfo.Name);
            byteArray.AddDWORD(sectionInfo.Offset);
            byteArray.AddDWORD(sectionInfo.Size);

            return byteArray;
        }

        private ByteArray WriteCustomTable(LookupTable<XUICUSTOM> customTable)
        {
            ByteArray byteArray = new ByteArray();

            foreach (XUICUSTOM customEntry in customTable)
            {
                byteArray.AddByteARRAY(customEntry.ToBinary());
            }

            return byteArray;
        }
        private ByteArray WriteFloatTable(LookupTable<float> floatTable)
        {
            ByteArray byteArray = new ByteArray();

            foreach (float floatEntry in floatTable)
                byteArray.AddFLOAT(floatEntry);

            return byteArray;
        }
        private ByteArray WriteColorTable(LookupTable<XUICOLOR> colorTable)
        {
            ByteArray byteArray = new ByteArray();

            foreach (XUICOLOR colorEntry in colorTable)
                byteArray.AddDWORD(colorEntry.argb);

            return byteArray;
        }
        private ByteArray WriteVectorTable(LookupTable<XUIVECTOR> vectorTable)
        {
            ByteArray byteArray = new ByteArray();

            foreach (XUIVECTOR vectorEntry in vectorTable)
            {
                byteArray.AddFLOAT(vectorEntry.x);
                byteArray.AddFLOAT(vectorEntry.y);
                byteArray.AddFLOAT(vectorEntry.z);
            }

            return byteArray;
        }
        private ByteArray WriteQuaternionTable(LookupTable<XUIQUATERNION> quatTable)
        {
            ByteArray byteArray = new ByteArray();

            foreach (XUIQUATERNION quatEntry in quatTable)
            {
                byteArray.AddFLOAT(quatEntry.x);
                byteArray.AddFLOAT(quatEntry.y);
                byteArray.AddFLOAT(quatEntry.z);
                byteArray.AddFLOAT(quatEntry.w);
            }

            return byteArray;
        }
        private ByteArray WriteStringTable(LookupTable<string> stringTable)
        {
            ByteArray stringData = new ByteArray();

            ushort nCount = 0;
            foreach (string s in stringTable)
            {
                if (s == "") continue;
                stringData.AddSTRING(s);
                nCount++;
            }

            uint nSize = (uint)stringData.Count;

            ByteArray byteArray = new ByteArray();

            byteArray.AddDWORD(nSize);
            byteArray.AddWORD(nCount);
            byteArray.AddByteARRAY(stringData);

            return byteArray;
        }

        private ByteArray WriteNamedFramesTable(LookupTable<XUINAMEDFRAMEDATA> nfTable)
        {
            ByteArray byteArray = new ByteArray();

            foreach (XUINAMEDFRAMEDATA nfd in nfTable)
            {
                int nameStringIdx = BinaryTables.StringTable.GetIndex((string)nfd.Name);
                byteArray.AddPackedDWORD((uint)nameStringIdx);

                byteArray.AddPackedDWORD(nfd.Time);

                byteArray.AddBYTE((byte)nfd.Command);
                if (nfd.Command >= XUI_NAMEDFRAME_COMMAND.XUI_CMD_GOTO)
                {
                    int targetStringIdx = BinaryTables.StringTable.GetIndex((string)nfd.Target);
                    byteArray.AddPackedDWORD((uint)targetStringIdx);
                }
            }

            return byteArray;
        }
        private ByteArray WriteKeyframePropTable(LookupTable<XUIKEYFRAMEPROPDATA> kfpTable)
        {
            ByteArray byteArray = new ByteArray();
            foreach (XUIKEYFRAMEPROPDATA propData in kfpTable)
            {
                byteArray.AddPackedDWORD(propData.PropertyIndex);
            }
            return byteArray;
        }
        private ByteArray WriteKeyframeDataTable(LookupTable<XUIKEYFRAMEDATA> kfdTable)
        {
            ByteArray byteArray = new ByteArray();

            foreach (XUIKEYFRAMEDATA kfData in kfdTable)
            {
                byteArray.AddPackedDWORD(kfData.Frame);

                byteArray.AddBYTE((byte)kfData.InterpolateType);

                if (kfData.InterpolateType == XUI_INTERPOLATE.XUI_INTERPOLATE_EASE)
                {
                    byteArray.AddBYTE((byte)kfData.EaseIn);
                    byteArray.AddBYTE((byte)kfData.EaseOut);
                    byteArray.AddBYTE((byte)kfData.EaseScale);
                }

                byteArray.AddPackedDWORD((uint)kfData.KeyframePropIdx);
            }
            return byteArray;
        }
    }
}
