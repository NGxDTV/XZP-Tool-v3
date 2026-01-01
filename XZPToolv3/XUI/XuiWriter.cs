using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace XZPToolv3.XUI
{
    class XuiWriter
    {
        XmlTextWriter xml;
        PROCESS_FLAGS processFlag;
        List<XUIELEM_PROP_DEF> ignoredProps;

        public void BuildXui(string savePath, XUIOBJECTDATA baseObject, PROCESS_FLAGS processFlags)
        {
            processFlag = new PROCESS_FLAGS();
            processFlag.useXam = processFlags.useXam;
            processFlag.xuiToolVersion = processFlags.xuiToolVersion;
            processFlag.useAnimations = processFlags.useAnimations;
            processFlag.extFile = processFlags.extFile;

            bool childHasExData = false;

            ignoredProps = new List<XUIELEM_PROP_DEF>();
            ignoredProps = XuiClass.Instance.GetIgnoredPropArray();
            if (ignoredProps == null)
                ignoredProps = new List<XUIELEM_PROP_DEF>();

            using (xml = new XmlTextWriter(savePath, null))
            {
                xml.WriteStartElement("XuiCanvas");
                xml.WriteAttributeString("version", "000c");
                xml.WriteRaw(Environment.NewLine);
                xml.WriteStartElement("Properties");
                xml.WriteRaw(Environment.NewLine);
                foreach (XUIPROPERTYDATA prop in baseObject.PropertyArray)
                {
                    xml.WriteStartElement(prop.PropDef.PropName);
                    xml.WriteValue(prop.PropValue.ToString());
                    xml.WriteEndElement();
                    xml.WriteRaw(Environment.NewLine);
                }
                xml.WriteEndElement();

                foreach (XUIOBJECTDATA child in baseObject.ChildrenArray)
                {
                    if (RecursiveWrite(child, ref baseObject))
                        childHasExData = true;
                }

                if (processFlag.useAnimations)
                    WriteAnimation(baseObject, childHasExData);

                xml.WriteEndElement();

                xml.Flush();
                xml.Close();
            }
        }

        public bool RecursiveWrite(XUIOBJECTDATA obj, ref XUIOBJECTDATA parent)
        {
            bool hasDataChunk = false;
            bool childHasExData = false;

            if (processFlag.useXam)
                xml.WriteStartElement(ConvertPropToXam(obj.ClassName));
            else
                xml.WriteStartElement(obj.ClassName);

            xml.WriteRaw(Environment.NewLine);
            xml.WriteStartElement("Properties");
            xml.WriteRaw(Environment.NewLine);

            List<XUIPROPERTYDATA> ignoredPropList = obj.PropertyArray.FindAll(x => Array.IndexOf<XUIELEM_PROP_DEF>(ignoredProps.ToArray(), x.PropDef) > -1);
            List<XUIPROPERTYDATA> nonIgnoredList = obj.PropertyArray.FindAll(x => Array.IndexOf<XUIELEM_PROP_DEF>(ignoredProps.ToArray(), x.PropDef) == -1);

            if (processFlag.xuiToolVersion && ignoredPropList.Count > 0)
            {
                hasDataChunk = true;
                Global.writeExtFile = true;
            }

            if (processFlag.xuiToolVersion && processFlag.extFile)
            {
                foreach (XUIPROPERTYDATA prop in ignoredPropList) { WriteProperty(prop); }
                WriteProperty(nonIgnoredList.Find(x => x.PropDef.PropName == "Id"));
            }
            else if (processFlag.xuiToolVersion && !processFlag.extFile)
            {
                foreach (XUIPROPERTYDATA prop in nonIgnoredList) { WriteProperty(prop); }
            }
            else if (!processFlag.xuiToolVersion)
            {
                foreach (XUIPROPERTYDATA prop in obj.PropertyArray) { WriteProperty(prop); }
            }

            xml.WriteEndElement();
            xml.WriteRaw(Environment.NewLine);

            if (obj.ChildrenArray.Count > 0)
            {
                foreach (XUIOBJECTDATA child in obj.ChildrenArray)
                {
                    if (RecursiveWrite(child, ref obj))
                        childHasExData = true;
                }
            }

            if (processFlag.useAnimations)
                WriteAnimation(obj, childHasExData);

            xml.WriteEndElement();
            xml.WriteRaw(Environment.NewLine);

            return hasDataChunk;
        }

        public void WriteProperty(XUIPROPERTYDATA prop)
        {
            xml.WriteStartElement(prop.PropDef.PropName);
            if (prop.Flags == 0x1) xml.WriteAttributeString("index", prop.Index.ToString());

            if (prop.PropType == XUIELEM_PROP_TYPE.XUI_EPT_OBJECT)
            {
                List<XUIPROPERTYDATA> child = (List<XUIPROPERTYDATA>)prop.PropValue;
                RecursivePropWrite(child);
            }
            else
            {
                if (prop.PropDef.PropName == "ImagePath" || prop.PropDef.PropName == "TextureFileName")
                    xml.WriteValue(FilterImagePaths(prop.PropValue.ToString()));
                else
                    xml.WriteValue(prop.PropValue.ToString());
            }

            xml.WriteEndElement();
            xml.WriteRaw(Environment.NewLine);
        }

        public void RecursivePropWrite(List<XUIPROPERTYDATA> subProps)
        {
            xml.WriteStartElement("Properties");
            xml.WriteRaw(Environment.NewLine);

            foreach (XUIPROPERTYDATA tmpProp in subProps)
            {
                xml.WriteStartElement(tmpProp.PropDef.PropName);
                if (tmpProp.Flags == 0x1) xml.WriteAttributeString("index", tmpProp.Index.ToString());

                if (tmpProp.PropType == XUIELEM_PROP_TYPE.XUI_EPT_OBJECT)
                {
                    List<XUIPROPERTYDATA> child = (List<XUIPROPERTYDATA>)tmpProp.PropValue;
                    RecursivePropWrite(child);
                }
                else
                {
                    xml.WriteValue(tmpProp.PropValue.ToString());
                }

                xml.WriteEndElement();
                xml.WriteRaw(Environment.NewLine);
            }
            xml.WriteEndElement();
            xml.WriteRaw(Environment.NewLine);
        }

        private void WriteAnimation(XUIOBJECTDATA obj, bool childHasExData)
        {
            if (obj.NumNamedFrames > 0 || obj.NumSubTimelines > 0)
            {
                xml.WriteStartElement("Timelines");
                xml.WriteRaw(Environment.NewLine);

                WriteNamedFrames(obj.NamedFrameArray);

                foreach (XUISUBTIMELINEDATA subtimeline in obj.SubTimelines)
                {
                    WriteSubTimeline(subtimeline, ref obj, childHasExData);
                }

                xml.WriteEndElement();
            }
        }

        private void WriteNamedFrames(List<XUINAMEDFRAMEDATA> namedFrameArray)
        {
            xml.WriteStartElement("NamedFrames");
            xml.WriteRaw(Environment.NewLine);
            foreach (XUINAMEDFRAMEDATA child in namedFrameArray)
            {
                xml.WriteStartElement("NamedFrame");
                xml.WriteRaw(Environment.NewLine);
                xml.WriteStartElement("Name");
                xml.WriteValue(child.Name);
                xml.WriteEndElement();
                xml.WriteRaw(Environment.NewLine);
                xml.WriteStartElement("Time");
                xml.WriteValue(child.Time);
                xml.WriteEndElement();
                xml.WriteRaw(Environment.NewLine);
                xml.WriteStartElement("Command");
                xml.WriteValue(XuiTypeConverter.NamedFrameCommandToString(child.Command));
                xml.WriteEndElement();
                xml.WriteRaw(Environment.NewLine);
                if ((int)child.Command >= 2)
                {
                    xml.WriteStartElement("CommandParams");
                    xml.WriteValue(child.Target);
                    xml.WriteEndElement();
                    xml.WriteRaw(Environment.NewLine);
                }
                xml.WriteEndElement();
                xml.WriteRaw(Environment.NewLine);
            }
            xml.WriteEndElement();
            xml.WriteRaw(Environment.NewLine);
        }

        private void WriteSubTimeline(XUISUBTIMELINEDATA subTimeLineData, ref XUIOBJECTDATA obj, bool childHasExData)
        {
            xml.WriteStartElement("Timeline");
            xml.WriteRaw(Environment.NewLine);

            xml.WriteStartElement("Id");
            xml.WriteValue(subTimeLineData.ElementId);
            xml.WriteEndElement();
            xml.WriteRaw(Environment.NewLine);

            List<int> index = new List<int>();
            List<XUITIMELINEPROPPATH> ignoredPropList = new List<XUITIMELINEPROPPATH>();
            List<XUITIMELINEPROPPATH> nonIgnoredList = new List<XUITIMELINEPROPPATH>();

            foreach (XUITIMELINEPROPPATH path in subTimeLineData.PropPathArray)
            {
                foreach (XUIELEM_PROP_DEF subData in path.PropDefArray)
                {
                    int idx = Array.IndexOf(ignoredProps.ToArray(), subData);
                    if (idx > -1)
                    {
                        ignoredPropList.Add(path);
                        index.Add(Array.IndexOf(subTimeLineData.PropPathArray.ToArray(), path));
                    }
                    else
                    {
                        if (Array.IndexOf(nonIgnoredList.ToArray(), path) == -1)
                            nonIgnoredList.Add(path);
                    }
                }
            }

            if (processFlag.xuiToolVersion && processFlag.extFile)
            {
                foreach (XUITIMELINEPROPPATH propPath in ignoredPropList)
                {
                    xml.WriteStartElement("TimelineProp");
                    xml.WriteAttributeString("index", propPath.Index.ToString());
                    string propName = "";

                    foreach (XUIELEM_PROP_DEF propDef in propPath.PropDefArray)
                    {
                        propName = propName + propDef.PropName + ".";
                    }
                    propName = propName.Substring(0, propName.Length - 1);
                    xml.WriteValue(propName);
                    xml.WriteEndElement();
                    xml.WriteRaw(Environment.NewLine);
                }
            }
            else if (processFlag.xuiToolVersion && !processFlag.extFile)
            {
                foreach (XUITIMELINEPROPPATH propPath in nonIgnoredList)
                {
                    xml.WriteStartElement("TimelineProp");
                    xml.WriteAttributeString("index", propPath.Index.ToString());
                    string propName = "";

                    foreach (XUIELEM_PROP_DEF propDef in propPath.PropDefArray)
                    {
                        propName = propName + propDef.PropName + ".";
                    }
                    propName = propName.Substring(0, propName.Length - 1);
                    xml.WriteValue(propName);
                    xml.WriteEndElement();
                    xml.WriteRaw(Environment.NewLine);
                }
            }
            else if (!processFlag.xuiToolVersion)
            {
                foreach (XUITIMELINEPROPPATH propPath in subTimeLineData.PropPathArray)
                {
                    xml.WriteStartElement("TimelineProp");
                    xml.WriteAttributeString("index", propPath.Index.ToString());
                    string propName = "";

                    foreach (XUIELEM_PROP_DEF propDef in propPath.PropDefArray)
                    {
                        propName = propName + propDef.PropName + ".";
                    }
                    propName = propName.Substring(0, propName.Length - 1);
                    xml.WriteValue(propName);
                    xml.WriteEndElement();
                    xml.WriteRaw(Environment.NewLine);
                }
            }

            WriteKeyframe(subTimeLineData.KeyframeDataArray, subTimeLineData.NumPropPaths, index);

            xml.WriteEndElement();
            xml.WriteRaw(Environment.NewLine);
        }

        private void WriteKeyframe(List<XUIKEYFRAMEDATA> keyframeDataArray, uint numPropPaths, List<int> index)
        {
            foreach (XUIKEYFRAMEDATA keyFrameData in keyframeDataArray)
            {
                xml.WriteStartElement("KeyFrame");
                xml.WriteRaw(Environment.NewLine);
                xml.WriteStartElement("Time");
                xml.WriteValue(keyFrameData.Frame.ToString());
                xml.WriteEndElement();
                xml.WriteRaw(Environment.NewLine);

                xml.WriteStartElement("Interpolation");
                byte interpType = (byte)keyFrameData.InterpolateType;
                xml.WriteValue(interpType.ToString());
                xml.WriteEndElement();
                xml.WriteRaw(Environment.NewLine);

                if (keyFrameData.Flags == 2)
                {
                    xml.WriteStartElement("EaseIn");
                    xml.WriteValue((int)(sbyte)keyFrameData.EaseIn);
                    xml.WriteEndElement();
                    xml.WriteRaw(Environment.NewLine);

                    xml.WriteStartElement("EaseOut");
                    xml.WriteValue((int)(sbyte)keyFrameData.EaseOut);
                    xml.WriteEndElement();
                    xml.WriteRaw(Environment.NewLine);

                    xml.WriteStartElement("EaseScale");
                    xml.WriteValue((int)(sbyte)keyFrameData.EaseScale);
                    xml.WriteEndElement();
                    xml.WriteRaw(Environment.NewLine);
                }

                uint propCount = numPropPaths;
                for (uint propIdx = 0; propIdx < propCount; propIdx++)
                {
                    if (processFlag.xuiToolVersion && processFlag.extFile)
                    {
                        if (index.Count > 0)
                        {
                            if (Array.IndexOf(index.ToArray(), (int)propIdx) > -1)
                            {
                                xml.WriteStartElement("Prop");
                                XUIPROPERTYDATA propData = keyFrameData.PropList[(int)propIdx];
                                xml.WriteValue(propData.PropValue.ToString());
                                xml.WriteEndElement();
                                xml.WriteRaw(Environment.NewLine);

                                Global.writeExtFile = true;
                            }
                        }
                    }
                    else if (index.Count > 0 && processFlag.xuiToolVersion && !processFlag.extFile)
                    {
                        if (Array.IndexOf(index.ToArray(), (int)propIdx) == -1)
                        {
                            xml.WriteStartElement("Prop");
                            XUIPROPERTYDATA propData = keyFrameData.PropList[(int)propIdx];
                            xml.WriteValue(propData.PropValue.ToString());
                            xml.WriteEndElement();
                            xml.WriteRaw(Environment.NewLine);
                        }
                    }
                    else
                    {
                        xml.WriteStartElement("Prop");
                        XUIPROPERTYDATA propData = keyFrameData.PropList[(int)propIdx];
                        xml.WriteValue(propData.PropValue.ToString());
                        xml.WriteEndElement();
                        xml.WriteRaw(Environment.NewLine);
                    }
                }

                xml.WriteEndElement();
                xml.WriteRaw(Environment.NewLine);
            }
        }

        private string ConvertPropToXam(string className)
        {
            if (className.Substring(0, 3) == "Xui" && (Array.IndexOf(Global.ignoreClasses, className) == -1))
                return ("Xam" + className.Substring(3, className.Length - 3));
            else
                return className;
        }

        private string FilterImagePaths(string path)
        {
            string[] pathType = path.Split(':');

            switch (pathType[0])
            {
                case "xam":
                    if (Array.IndexOf(Log.Instance.sharedresImages.ToArray(), path) == -1)
                        Log.Instance.xamImages.Add(path);
                    return path.Replace("://", "\\");
                case "sharedres":
                    if (Array.IndexOf(Log.Instance.sharedresImages.ToArray(), path) == -1)
                        Log.Instance.sharedresImages.Add(path);
                    return path.Replace("://", "\\");
                case "skin":
                    if (Array.IndexOf(Log.Instance.sharedresImages.ToArray(), path) == -1)
                        Log.Instance.skinImages.Add(path);
                    return path.Replace("://", "\\");
                case "file":
                    if (Array.IndexOf(Log.Instance.customImages.ToArray(), path) == -1)
                        Log.Instance.customImages.Add(path);
                    return path.Replace("file://Plugin:\\HudScene\\Images\\", "images\\");
                default:
                    if (Array.IndexOf(Log.Instance.hudImages.ToArray(), path) == -1)
                        Log.Instance.hudImages.Add(path);
                    return "hud\\" + path;
            }
        }
    }
}
