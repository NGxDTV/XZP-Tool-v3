using System;
using System.Collections.Generic;
using System.Text;

namespace XZPToolv3.XUI
{
    // XUI Specific Variable Types
    public struct XUIPOINT
    {
        public float x;
        public float y;
    }

    public struct XUIVECTOR
    {
        public float x;
        public float y;
        public float z;

        public override bool Equals(object obj)
        {
            XUIVECTOR vec = (XUIVECTOR)obj;
            if (x != vec.x) return false;
            if (y != vec.y) return false;
            if (z != vec.z) return false;
            return true;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Clear();
            builder.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0:f6},{1:f6},{2:f6}", x, y, z);
            return builder.ToString();
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        }
    }

    public struct XUIQUATERNION
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public override bool Equals(object obj)
        {
            XUIQUATERNION quat = (XUIQUATERNION)obj;
            if (x != quat.x) return false;
            if (y != quat.y) return false;
            if (z != quat.z) return false;
            if (w != quat.w) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Clear();
            builder.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0:f6},{1:f6},{2:f6},{3:f6}", x, y, z, w);
            return builder.ToString();
        }
    }

    public struct XUICOLOR
    {
        public uint argb;

        public override bool Equals(object obj)
        {
            XUICOLOR color = (XUICOLOR)obj;
            return argb == color.argb;
        }

        public override int GetHashCode()
        {
            return argb.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Clear();
            builder.AppendFormat("0x{0:x8}", argb);
            return builder.ToString();
        }
    }

    public struct XUIBEZIERPOINT
    {
        public XUIPOINT vecPoint;
        public XUIPOINT vecCtrl1;
        public XUIPOINT vecCtrl2;
    }

    public class XUICUSTOM
    {
        public uint Offset;
        public uint DataLen;
        public XUIPOINT BoundingBox;
        public uint NumPoints;
        public List<XUIBEZIERPOINT> Points;

        public XUICUSTOM()
        {
            DataLen = 0;
            BoundingBox.x = 0; BoundingBox.y = 0;
            NumPoints = 0;
            Points = new List<XUIBEZIERPOINT>();
            Offset = 0;
        }

        public override bool Equals(object obj)
        {
            XUICUSTOM custom = (XUICUSTOM)obj;
            if (DataLen != custom.DataLen) return false;
            if (BoundingBox.x != custom.BoundingBox.x) return false;
            if (BoundingBox.y != custom.BoundingBox.y) return false;
            if (NumPoints != custom.NumPoints) return false;
            if (Points.Count != custom.Points.Count) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return DataLen.GetHashCode() ^ NumPoints.GetHashCode();
        }

        public ByteArray ToBinary()
        {
            ByteArray byteArray = new ByteArray();
            byteArray.AddDWORD(DataLen);
            byteArray.AddFLOAT(BoundingBox.x);
            byteArray.AddFLOAT(BoundingBox.y);
            byteArray.AddDWORD(NumPoints);

            foreach (XUIBEZIERPOINT pt in Points)
            {
                byteArray.AddFLOAT(pt.vecPoint.x);
                byteArray.AddFLOAT(pt.vecPoint.y);
                byteArray.AddFLOAT(pt.vecCtrl1.x);
                byteArray.AddFLOAT(pt.vecCtrl1.y);
                byteArray.AddFLOAT(pt.vecCtrl2.x);
                byteArray.AddFLOAT(pt.vecCtrl2.y);
            }

            return byteArray;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Clear();
            builder.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0},", NumPoints);
            foreach (XUIBEZIERPOINT pt in Points)
            {
                builder.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, "{0:f6},{1:f6},{2:f6},{3:f6},{4:f6},{5:f6},0,",
                    pt.vecPoint.x, pt.vecPoint.y,
                    pt.vecCtrl1.x, pt.vecCtrl1.y,
                    pt.vecCtrl2.x, pt.vecCtrl2.y
                );
            }
            return builder.ToString();
        }
    }

    public struct XUINAMEDFRAMEDATA
    {
        public uint NameStringIndex;
        public string Name;
        public uint Time;
        public XUI_NAMEDFRAME_COMMAND Command;
        public uint TargetStringIndex;
        public string Target;

        public override bool Equals(object obj)
        {
            XUINAMEDFRAMEDATA data = (XUINAMEDFRAMEDATA)obj;
            if (NameStringIndex != data.NameStringIndex) return false;
            if (Name != data.Name) return false;
            if (Time != data.Time) return false;
            if (Command != data.Command) return false;
            if (TargetStringIndex != data.TargetStringIndex) return false;
            if (Target != data.Target) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return NameStringIndex.GetHashCode() ^ Time.GetHashCode();
        }
    }

    public struct XUIKEYFRAMEPROPDATA
    {
        public uint PropertyIndex;
    }

    public class XUIKEYFRAMEDATA
    {
        public uint Frame;
        public uint Flags;
        public XUI_INTERPOLATE InterpolateType;
        public uint Unknown1;
        public byte EaseIn;
        public byte EaseOut;
        public byte EaseScale;
        public uint KeyframePropIdx;
        public List<XUIPROPERTYDATA> PropList;
        public uint VectorIdx;
        public XUIVECTOR VectorRef;

        public override bool Equals(object obj)
        {
            XUIKEYFRAMEDATA data = (XUIKEYFRAMEDATA)obj;

            if (Frame != data.Frame) return false;
            if (Flags != data.Flags) return false;
            if (InterpolateType != data.InterpolateType) return false;
            if (Unknown1 != data.Unknown1) return false;
            if (EaseIn != data.EaseIn) return false;
            if (EaseOut != data.EaseOut) return false;
            if (EaseScale != data.EaseScale) return false;
            if (KeyframePropIdx != data.KeyframePropIdx) return false;
            if (VectorIdx != data.VectorIdx) return false;
            if (VectorRef.Equals(data.VectorRef) == false) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return Frame.GetHashCode() ^ Flags.GetHashCode();
        }

        public XUIKEYFRAMEDATA()
        {
            InterpolateType = XUI_INTERPOLATE.XUI_INTERPOLATE_LINEAR;
            Frame = 0;
            Flags = 0;
            Unknown1 = 0;
            EaseIn = 0;
            EaseOut = 0;
            EaseScale = 0;
            KeyframePropIdx = 0;
            PropList = new List<XUIPROPERTYDATA>();
            VectorIdx = 0;
            VectorRef = new XUIVECTOR();
        }
    }

    public class XUIOBJECTDATA
    {
        public uint ObjectId;
        public string ClassName;
        public uint Flags;
        public List<XUIPROPERTYDATA> PropertyArray;
        public List<XUIOBJECTDATA> ChildrenArray;
        public uint NumNamedFrames;
        public List<XUINAMEDFRAMEDATA> NamedFrameArray;
        public uint NumSubTimelines;
        public List<XUISUBTIMELINEDATA> SubTimelines;
        public string HeriarchyId;

        public object GetPropVal(string propId)
        {
            foreach (XUIPROPERTYDATA prop in PropertyArray)
            {
                if (prop.PropDef.PropName == propId)
                    return prop.PropValue;
            }
            return "";
        }

        public XUIOBJECTDATA()
        {
            ObjectId = 0;
            ClassName = "";
            Flags = 0;
            PropertyArray = new List<XUIPROPERTYDATA>();
            ChildrenArray = new List<XUIOBJECTDATA>();
            NumNamedFrames = 0;
            NamedFrameArray = new List<XUINAMEDFRAMEDATA>();
            NumSubTimelines = 0;
            SubTimelines = new List<XUISUBTIMELINEDATA>();
            HeriarchyId = "";
        }
    }

    public class XUIPROPERTYDATA
    {
        public uint Flags;
        public uint Index;
        public uint CompoundPropId;
        public object PropValue { get; set; }
        public XUIELEM_PROP_TYPE PropType;
        public XUIELEM_PROP_DEF PropDef { get; set; }

        public XUIPROPERTYDATA()
        {
            Flags = 0;
            Index = 0;
            CompoundPropId = 0;
            PropType = XUIELEM_PROP_TYPE.XUI_EPT_EMPTY;
        }
    }

    public struct XUIELEM_PROP_DEF
    {
        public uint Flags;
        public uint Index;
        public uint Offset;
        public uint Extra;
        public int Id;
        public string PropName;
        public XUIELEM_PROP_TYPE Type;
        public object DefaultVal;

        /// <summary>
        /// Get property definitions for object types (Fill, Stroke, Gradient)
        /// </summary>
        public List<XUIELEM_PROP_DEF> GetPropDefs()
        {
            if (PropName == "Fill")
                return XuiClass.Instance.GetFillPropArray();
            else if (PropName == "Stroke")
                return XuiClass.Instance.GetStrokePropArray();
            else if (PropName == "Gradient")
                return XuiClass.Instance.GetGradientPropArray();
            else
                return null;
        }
    }

    public struct XUI_CLASS
    {
        public string szClassName;
        public string szBaseClassName;
        public List<XUIELEM_PROP_DEF> PropDefs;
        public uint dwPropDefCount;
    }

    public class XUITIMELINEPROPPATH
    {
        public uint Flags;
        public uint Depth;
        public uint Index;
        public List<XUIELEM_PROP_DEF> PropDefArray;

        public XUITIMELINEPROPPATH()
        {
            Flags = 0;
            Depth = 0;
            Index = 0;
            PropDefArray = new List<XUIELEM_PROP_DEF>();
        }
    }

    public class XUISUBTIMELINEDATA
    {
        public uint ElementStringIdx;
        public string ElementId;
        public uint Flags;
        public uint NumPropPaths;
        public List<XUITIMELINEPROPPATH> PropPathArray;
        public List<uint> IndexArray;
        public List<XUIKEYFRAMEDATA> KeyframeDataArray;

        public XUISUBTIMELINEDATA()
        {
            ElementStringIdx = 0;
            ElementId = "";
            Flags = 0;
            NumPropPaths = 0;
            PropPathArray = new List<XUITIMELINEPROPPATH>();
            IndexArray = new List<uint>();
            KeyframeDataArray = new List<XUIKEYFRAMEDATA>();
        }
    }

    // XURv8 Structs
    public struct XUR8_HEADER
    {
        public uint Magic;
        public uint Version;
        public uint Flags;
        public ushort XuiVersion;
        public uint BinSize;
        public ushort NumSections;
    }

    public struct XUR8_HEADER_INFO
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

    public struct XUR8_SECTION
    {
        public uint Name;
        public uint Offset;
        public uint Size;

        public override string ToString()
        {
            return Name.ToString("X8");
        }
    }
}
