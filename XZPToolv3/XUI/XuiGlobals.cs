using System;
using System.Collections.Generic;

namespace XZPToolv3.XUI
{
    public struct PROCESS_FLAGS
    {
        public bool useXam;
        public bool xuiToolVersion;
        public bool useAnimations;
        public bool extFile;
    }

    public static class Global
    {
        public static string[] ignoreClasses = { "XuiVisual", "XuiText", "XuiImage", "XuiTextPresenter", "XuiImagePresenter", "XuiNineGrid", "XuiFigure" };
        public static bool writeExtFile { get; set; }

        public static void RemoveDesignTimeElements(XUIOBJECTDATA obj)
        {
            List<XUIOBJECTDATA> designTime = obj.ChildrenArray.FindAll(x =>
            {
                object val = x.GetPropVal("DesignTime");
                return val != null && val.ToString() == "True";
            });

            foreach (XUIOBJECTDATA child in designTime)
            {
                obj.ChildrenArray.Remove(child);
            }

            foreach (XUIOBJECTDATA child in obj.ChildrenArray)
            {
                RemoveDesignTimeElements(child);
            }
        }
    }
}
