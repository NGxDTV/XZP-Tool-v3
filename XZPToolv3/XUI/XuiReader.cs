using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace XZPToolv3.XUI
{
    class XuiReader
    {
        private uint objectId;
        PROCESS_FLAGS processFlag;

        public XUIOBJECTDATA ReadXui(string filePath, PROCESS_FLAGS processFlags)
        {
            processFlag = new PROCESS_FLAGS();
            processFlag.useAnimations = processFlags.useAnimations;

            string xmlText;
            using (StreamReader streamReader = new StreamReader(filePath))
            {
                xmlText = streamReader.ReadToEnd();
            }

            objectId = 0;

            XUIOBJECTDATA xuiRead = new XUIOBJECTDATA();
            XElement xElement = XElement.Parse(xmlText);

            xuiRead.ClassName = ConvertPropToXui(xElement.Name.ToString());
            xuiRead.ObjectId = objectId;
            objectId++;

            foreach (XElement e in xElement.Elements())
            {
                if (e.Name.ToString() == "Properties")
                    ReadProperties(e, ref xuiRead);

                XUI_CLASS tmpClass = XuiClass.Instance.FindClass(ConvertPropToXui(e.Name.ToString()));
                if (tmpClass.szClassName != null)
                {
                    ReadClass(e, ref xuiRead);
                }

                GetClassFlag(ref xuiRead);

                if (processFlag.useAnimations && e.Name.ToString() == "Timelines")
                    ReadTimelines(e, ref xuiRead);
            }

            return xuiRead;
        }

        public void ReadClass(XElement e, ref XUIOBJECTDATA baseClass)
        {
            XUIOBJECTDATA childClass = new XUIOBJECTDATA();
            childClass.ClassName = ConvertPropToXui(e.Name.ToString());
            childClass.ObjectId = objectId;
            objectId++;

            foreach (XElement classItem in e.Elements())
            {
                if (classItem.Name.ToString() == "Properties")
                    ReadProperties(classItem, ref childClass);

                XUI_CLASS tmpClass = XuiClass.Instance.FindClass(ConvertPropToXui(classItem.Name.ToString()));
                if (tmpClass.szClassName != null)
                {
                    ReadClass(classItem, ref childClass);
                }

                GetClassFlag(ref childClass);

                if (processFlag.useAnimations && classItem.Name.ToString() == "Timelines")
                    ReadTimelines(classItem, ref childClass);
            }

            baseClass.ChildrenArray.Add(childClass);
        }

        public void ReadProperties(XElement e, ref XUIOBJECTDATA baseClass)
        {
            foreach (XElement prop in e.Elements())
            {
                XUIPROPERTYDATA tmpProp = new XUIPROPERTYDATA();
                XUIELEM_PROP_DEF tmpPropDef = XuiClass.Instance.FindProperty(prop.Name.ToString(), baseClass.ClassName);
                tmpProp.PropType = tmpPropDef.Type;
                tmpProp.PropDef = tmpPropDef;
                tmpProp.Flags = tmpPropDef.Flags;

                if (prop.Name.ToString() == "Stroke" || prop.Name.ToString() == "Fill")
                {
                    tmpProp.PropValue = ReadFigureProperties(prop);
                }
                else
                {
                    if (prop.Name.ToString() == "ImagePath" || prop.Name.ToString() == "TextureFileName")
                        tmpProp.PropValue = FilterImagePaths(prop.Value.ToString());
                    else
                        tmpProp.PropValue = XuiTypeConverter.FormatPropType(prop.Value, tmpPropDef.Type);
                }

                baseClass.PropertyArray.Add(tmpProp);
            }
        }

        public List<XUIPROPERTYDATA> ReadFigureProperties(XElement e)
        {
            List<XUIPROPERTYDATA> tempArray = new List<XUIPROPERTYDATA>();

            foreach (XElement prop in e.Elements())
            {
                foreach (XElement fig in prop.Elements())
                {
                    XUIPROPERTYDATA tmpProp = new XUIPROPERTYDATA();

                    if (fig.Name.ToString() == "Gradient")
                        tmpProp.PropValue = ReadFigureProperties(fig);
                    else
                        tmpProp.PropValue = fig.Value;

                    XUIELEM_PROP_DEF tmpPropDef = XuiClass.Instance.FindFigureProperty(fig.Name.ToString(), e.Name.ToString());
                    tmpProp.PropType = tmpPropDef.Type;
                    tmpProp.PropDef = tmpPropDef;
                    tmpProp.Flags = tmpPropDef.Flags;

                    if (fig.HasAttributes)
                        tmpProp.Index = Convert.ToUInt32(fig.Attribute("index").Value);

                    tempArray.Add(tmpProp);
                }
            }

            return tempArray;
        }

        public void ReadTimelines(XElement e, ref XUIOBJECTDATA baseClass)
        {
            foreach (XElement classItem in e.Elements())
            {
                if (classItem.Name.ToString() == "NamedFrames")
                    ReadNamedFrames(classItem, ref baseClass);

                if (classItem.Name.ToString() == "Timeline")
                    ReadTimeline(classItem, ref baseClass);
            }
        }

        public void ReadNamedFrames(XElement e, ref XUIOBJECTDATA baseClass)
        {
            foreach (XElement classItem in e.Elements())
            {
                if (classItem.Name.ToString() == "NamedFrame")
                    ReadNamedFrame(classItem, ref baseClass);
            }
        }

        public void ReadNamedFrame(XElement e, ref XUIOBJECTDATA baseClass)
        {
            XUINAMEDFRAMEDATA frame = new XUINAMEDFRAMEDATA();

            foreach (XElement classItem in e.Elements())
            {
                if (classItem.Name.ToString() == "Name")
                    frame.Name = classItem.Value;
                else if (classItem.Name.ToString() == "Time")
                    frame.Time = UInt32.Parse(classItem.Value);
                else if (classItem.Name.ToString() == "Command")
                    frame.Command = XuiTypeConverter.StringToNamedFrameCommand(classItem.Value.ToString());
                else if (classItem.Name.ToString() == "CommandParams")
                    frame.Target = classItem.Value.ToString();
            }

            baseClass.NamedFrameArray.Add(frame);
            baseClass.NumNamedFrames++;
        }

        public void ReadTimeline(XElement e, ref XUIOBJECTDATA baseClass)
        {
            XUISUBTIMELINEDATA timeline = new XUISUBTIMELINEDATA();
            XUITIMELINEPROPPATH propPath = new XUITIMELINEPROPPATH();

            foreach (XElement classItem in e.Elements())
            {
                if (classItem.Name.ToString() == "Id")
                    timeline.ElementId = classItem.Value.ToString();
                else if (classItem.Name.ToString() == "TimelineProp")
                {
                    propPath = new XUITIMELINEPROPPATH();

                    if (classItem.HasAttributes) propPath.Index = UInt32.Parse(classItem.Attribute("index").Value);
                    timeline.IndexArray.Add(propPath.Index);

                    string[] propSplit = classItem.Value.ToString().Split('.');
                    if (propSplit.Length > 1)
                    {
                        XUIELEM_PROP_DEF prop = new XUIELEM_PROP_DEF();

                        propPath.PropDefArray.Add(XuiClass.Instance.FindProperty(propSplit[0].ToString(), "XuiFigure"));

                        for (int i = 1; i < propSplit.Length; i++)
                        {
                            prop = new XUIELEM_PROP_DEF();

                            if (propSplit[i - 1] == "Fill")
                                prop = XuiClass.Instance.GetFillPropArray().Find(x => x.PropName == propSplit[i]);
                            else if (propSplit[i - 1] == "Stroke")
                                prop = XuiClass.Instance.GetStrokePropArray().Find(x => x.PropName == propSplit[i]);
                            else if (propSplit[i - 1] == "Gradient")
                                prop = XuiClass.Instance.GetGradientPropArray().Find(x => x.PropName == propSplit[i]);

                            propPath.PropDefArray.Add(prop);
                        }
                    }
                    else
                    {
                        XUIOBJECTDATA child = baseClass.ChildrenArray.Find(x => x.GetPropVal("Id").ToString() == timeline.ElementId);

                        if (child != null)
                            propPath.PropDefArray.Add(XuiClass.Instance.FindProperty(propSplit[0], child.ClassName));
                    }

                    propPath.Flags = 1;
                    propPath.Depth = (uint)propPath.PropDefArray.Count;
                    timeline.PropPathArray.Add(propPath);
                    timeline.NumPropPaths++;
                }
                else if (classItem.Name.ToString() == "KeyFrame")
                    ReadKeyFrame(classItem, ref baseClass, ref timeline);
            }

            baseClass.SubTimelines.Add(timeline);
            baseClass.NumSubTimelines++;
        }

        public void ReadKeyFrame(XElement e, ref XUIOBJECTDATA baseClass, ref XUISUBTIMELINEDATA timeline)
        {
            XUIKEYFRAMEDATA keyFrame = new XUIKEYFRAMEDATA();
            int counter = 0;

            foreach (XElement classItem in e.Elements())
            {
                XUIPROPERTYDATA propData = new XUIPROPERTYDATA();

                if (classItem.Name.ToString() == "Time")
                    keyFrame.Frame = UInt32.Parse(classItem.Value.ToString());
                else if (classItem.Name.ToString() == "Interpolation")
                    keyFrame.InterpolateType = (XUI_INTERPOLATE)int.Parse(classItem.Value);
                else if (classItem.Name.ToString() == "EaseIn")
                    keyFrame.EaseIn = (byte)SByte.Parse(classItem.Value.ToString());
                else if (classItem.Name.ToString() == "EaseOut")
                    keyFrame.EaseOut = (byte)SByte.Parse(classItem.Value.ToString());
                else if (classItem.Name.ToString() == "EaseScale")
                    keyFrame.EaseScale = (byte)SByte.Parse(classItem.Value.ToString());
                else if (classItem.Name.ToString() == "Prop")
                {
                    int index = timeline.PropPathArray[counter].PropDefArray.Count;
                    propData.PropDef = timeline.PropPathArray[counter].PropDefArray[index - 1];
                    propData.PropType = propData.PropDef.Type;
                    propData.PropValue = XuiTypeConverter.FormatPropType(classItem.Value.ToString(), propData.PropType);
                    propData.Flags = propData.PropDef.Flags;

                    propData.Index = timeline.IndexArray[counter];

                    counter++;
                    keyFrame.PropList.Add(propData);
                }
            }
            keyFrame.Flags = (uint)(int)keyFrame.InterpolateType;

            timeline.KeyframeDataArray.Add(keyFrame);
        }

        public string ConvertPropToXui(string className)
        {
            if (className.Substring(0, 3) == "Xam")
                return ("Xui" + className.Substring(3, className.Length - 3));
            else
                return className;
        }

        public void GetClassFlag(ref XUIOBJECTDATA baseClass)
        {
            uint flag = 0;

            if (baseClass.PropertyArray.Count > 0)
                flag += 0x1;
            if (baseClass.ChildrenArray.Count > 0)
                flag += 0x2;
            if (baseClass.NumSubTimelines > 0)
                flag += 0x4;

            baseClass.Flags = flag;
        }

        private string FilterImagePaths(string path)
        {
            string[] pathType = path.Split('\\');

            switch (pathType[0])
            {
                case "xam":
                    return path.Replace("\\", "://");
                case "sharedres":
                    return path.Replace("\\", "://");
                case "skin":
                    return path.Replace("\\", "://");
                case "hud":
                    return path.Remove(0, 4);
                case "images":
                    return path.Replace("images", "file://Plugin:\\HudScene\\Images");
            }
            return path;
        }
    }
}
