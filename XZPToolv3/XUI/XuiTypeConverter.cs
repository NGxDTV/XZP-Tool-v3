using System;
using System.Globalization;

namespace XZPToolv3.XUI
{
    public static class XuiTypeConverter
    {
        public static XUICOLOR ToXuiColor(this string s)
        {
            XUICOLOR color = new XUICOLOR();
            color.argb = Convert.ToUInt32(s, 16);
            return color;
        }

        public static XUIQUATERNION ToXuiQuaternion(this string s)
        {
            XUIQUATERNION quan = new XUIQUATERNION();

            string[] split = s.Split(',');
            quan.x = Single.Parse(split[0], CultureInfo.InvariantCulture);
            quan.y = Single.Parse(split[1], CultureInfo.InvariantCulture);
            quan.z = Single.Parse(split[2], CultureInfo.InvariantCulture);
            quan.w = Single.Parse(split[3], CultureInfo.InvariantCulture);
            return quan;
        }

        public static XUIVECTOR ToXuiVector(this string s)
        {
            XUIVECTOR vec = new XUIVECTOR();

            string[] split = s.Split(',');
            vec.x = Single.Parse(split[0], CultureInfo.InvariantCulture);
            vec.y = Single.Parse(split[1], CultureInfo.InvariantCulture);
            vec.z = Single.Parse(split[2], CultureInfo.InvariantCulture);
            return vec;
        }

        public static XUICUSTOM ToXuiCustom(this string s)
        {
            XUICUSTOM cust = new XUICUSTOM();
            XUIBEZIERPOINT points = new XUIBEZIERPOINT();
            XUIPOINT point = new XUIPOINT();

            string[] split = s.Split(',');
            cust.NumPoints = Convert.ToUInt32(split[0]);

            int rootCount = 1;
            for (int i = 0; i < Convert.ToInt32(split[0]); i++)
            {
                point.x = Single.Parse(split[rootCount], CultureInfo.InvariantCulture);
                rootCount++;
                point.y = Single.Parse(split[rootCount], CultureInfo.InvariantCulture);
                rootCount++;

                points.vecPoint = point;

                point.x = Single.Parse(split[rootCount], CultureInfo.InvariantCulture);
                rootCount++;
                point.y = Single.Parse(split[rootCount], CultureInfo.InvariantCulture);
                rootCount++;

                points.vecCtrl1 = point;

                point.x = Single.Parse(split[rootCount], CultureInfo.InvariantCulture);
                rootCount++;
                point.y = Single.Parse(split[rootCount], CultureInfo.InvariantCulture);
                rootCount += 2;

                points.vecCtrl2 = point;

                cust.Points.Add(points);
            }

            point.x = 0;
            point.y = 0;
            foreach (var pt in cust.Points)
            {
                if (pt.vecPoint.x > point.x) point.x = pt.vecPoint.x;
                if (pt.vecPoint.y > point.y) point.y = pt.vecPoint.y;
            }

            cust.BoundingBox = point;
            cust.DataLen = 8 + 4 + cust.NumPoints * 24;

            return cust;
        }

        public static object FormatPropType(string value, XUIELEM_PROP_TYPE propType)
        {
            switch (propType)
            {
                case XUIELEM_PROP_TYPE.XUI_EPT_BOOL:
                    return Convert.ToBoolean(value);
                case XUIELEM_PROP_TYPE.XUI_EPT_COLOR:
                    return value.ToXuiColor();
                case XUIELEM_PROP_TYPE.XUI_EPT_CUSTOM:
                    return value.ToXuiCustom();
                case XUIELEM_PROP_TYPE.XUI_EPT_FLOAT:
                    return float.Parse(value, CultureInfo.InvariantCulture);
                case XUIELEM_PROP_TYPE.XUI_EPT_INTEGER:
                    return Convert.ToInt32(value);
                case XUIELEM_PROP_TYPE.XUI_EPT_QUATERNION:
                    return value.ToXuiQuaternion();
                case XUIELEM_PROP_TYPE.XUI_EPT_STRING:
                    return value;
                case XUIELEM_PROP_TYPE.XUI_EPT_UNSIGNED:
                    return Convert.ToUInt32(value);
                case XUIELEM_PROP_TYPE.XUI_EPT_VECTOR:
                    return value.ToXuiVector();
            }
            return null;
        }

        public static string NamedFrameCommandToString(XUI_NAMEDFRAME_COMMAND cmd)
        {
            switch (cmd)
            {
                case XUI_NAMEDFRAME_COMMAND.XUI_CMD_PLAY:
                    return "play";
                case XUI_NAMEDFRAME_COMMAND.XUI_CMD_STOP:
                    return "stop";
                case XUI_NAMEDFRAME_COMMAND.XUI_CMD_GOTO:
                    return "goto";
                case XUI_NAMEDFRAME_COMMAND.XUI_CMD_GOTOANDPLAY:
                    return "gotoandplay";
                case XUI_NAMEDFRAME_COMMAND.XUI_CMD_GOTOANDSTOP:
                    return "gotoandstop";
                default:
                    return "unknown";
            }
        }

        public static XUI_NAMEDFRAME_COMMAND StringToNamedFrameCommand(string s)
        {
            switch (s.ToLower())
            {
                case "play":
                    return XUI_NAMEDFRAME_COMMAND.XUI_CMD_PLAY;
                case "stop":
                    return XUI_NAMEDFRAME_COMMAND.XUI_CMD_STOP;
                case "goto":
                    return XUI_NAMEDFRAME_COMMAND.XUI_CMD_GOTO;
                case "gotoandplay":
                    return XUI_NAMEDFRAME_COMMAND.XUI_CMD_GOTOANDPLAY;
                case "gotoandstop":
                    return XUI_NAMEDFRAME_COMMAND.XUI_CMD_GOTOANDSTOP;
                default:
                    return XUI_NAMEDFRAME_COMMAND.XUI_CMD_PLAY;
            }
        }
    }
}
