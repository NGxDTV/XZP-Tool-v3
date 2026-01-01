using System;
using System.Collections.Generic;

namespace XZPToolv3.XUI
{
    /// <summary>
    /// Simplified XUI class definitions for basic XUR file support
    /// This is a minimal implementation without full XuiClass infrastructure
    /// </summary>
    public static class SimpleXuiClassDefinitions
    {
        private static Dictionary<string, List<string>> basicPropertyNames = new Dictionary<string, List<string>>();

        static SimpleXuiClassDefinitions()
        {
            // Initialize with common XUI element properties
            // These are simplified - real XUIWorkshop has much more detailed class hierarchies

            // Base XuiElement properties (most elements inherit these)
            var baseProps = new List<string> { "Id", "Width", "Height", "Position", "Opacity" };

            // XuiCanvas
            basicPropertyNames["XuiCanvas"] = new List<string>(baseProps);

            // XuiButton
            basicPropertyNames["XuiButton"] = new List<string>(baseProps) { "Text" };

            // XuiText
            basicPropertyNames["XuiText"] = new List<string>(baseProps) { "Text", "TextColor" };

            // XuiImage
            basicPropertyNames["XuiImage"] = new List<string>(baseProps) { "ImagePath" };

            // XuiScene
            basicPropertyNames["XuiScene"] = new List<string>(baseProps);

            // XuiControl
            basicPropertyNames["XuiControl"] = new List<string>(baseProps);

            // XuiGroup
            basicPropertyNames["XuiGroup"] = new List<string>(baseProps);

            // XuiList
            basicPropertyNames["XuiList"] = new List<string>(baseProps);

            // XuiLabel
            basicPropertyNames["XuiLabel"] = new List<string>(baseProps) { "Text", "TextColor" };
        }

        public static bool HasClass(string className)
        {
            return basicPropertyNames.ContainsKey(className);
        }

        public static List<string> GetPropertyNames(string className)
        {
            if (basicPropertyNames.ContainsKey(className))
            {
                return new List<string>(basicPropertyNames[className]);
            }

            // Return basic properties for unknown classes
            return new List<string> { "Id" };
        }

        public static XUIELEM_PROP_TYPE GuessPropertyType(string propName)
        {
            // Simple heuristics for property types
            switch (propName.ToLower())
            {
                case "id":
                case "text":
                case "imagepath":
                case "texturefilename":
                    return XUIELEM_PROP_TYPE.XUI_EPT_STRING;

                case "width":
                case "height":
                case "opacity":
                case "rotation":
                case "scale":
                    return XUIELEM_PROP_TYPE.XUI_EPT_FLOAT;

                case "position":
                case "size":
                    return XUIELEM_PROP_TYPE.XUI_EPT_VECTOR;

                case "textcolor":
                case "color":
                case "backgroundcolor":
                    return XUIELEM_PROP_TYPE.XUI_EPT_COLOR;

                case "visible":
                case "enabled":
                    return XUIELEM_PROP_TYPE.XUI_EPT_BOOL;

                default:
                    return XUIELEM_PROP_TYPE.XUI_EPT_STRING; // Default fallback
            }
        }

        public static XUIELEM_PROP_DEF CreatePropertyDef(string propName, int index)
        {
            XUIELEM_PROP_DEF propDef = new XUIELEM_PROP_DEF();
            propDef.PropName = propName;
            propDef.Type = GuessPropertyType(propName);
            propDef.Id = index;
            propDef.Flags = 0;
            propDef.Index = 0;
            propDef.Offset = 0;
            propDef.Extra = 0;
            propDef.DefaultVal = null;
            return propDef;
        }
    }
}
